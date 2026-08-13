/*
  OCV_DATA core stored procedures
  Run after: 001_create_ocv_data_schema.sql
*/

USE OCV_DATA;
GO

CREATE OR ALTER PROCEDURE dbo.usp_RegisterTray
    @TrayCode nvarchar(100),
    @SourceType nvarchar(30) = N'Manual',
    @OperatorName nvarchar(100) = NULL,
    @TrayId bigint OUTPUT,
    @ReturnRow bit = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @TrayCode = NULLIF(ltrim(rtrim(@TrayCode)), N'');
    IF @TrayCode IS NULL
        THROW 50001, 'TrayCode is required.', 1;

    BEGIN TRAN;

    SELECT @TrayId = TrayId
    FROM dbo.Tray WITH (UPDLOCK, HOLDLOCK)
    WHERE TrayCode = @TrayCode;

    IF @TrayId IS NULL
    BEGIN
        INSERT INTO dbo.Tray(TrayCode, SourceType, TrayStatus)
        VALUES (@TrayCode, ISNULL(@SourceType, N'Manual'), N'Created');

        SET @TrayId = CONVERT(bigint, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.Tray
        SET SourceType = COALESCE(@SourceType, SourceType),
            UpdatedAt = sysdatetime()
        WHERE TrayId = @TrayId;
    END

    INSERT INTO dbo.OperatorLog(OperatorName, ActionName, Detail)
    VALUES (@OperatorName, N'RegisterTray', CONCAT(N'TrayCode=', @TrayCode));

    COMMIT TRAN;

    IF @ReturnRow = 1
    BEGIN
        SELECT *
        FROM dbo.Tray
        WHERE TrayId = @TrayId;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveTrayCells
    @TrayCode nvarchar(100),
    @Cells dbo.TrayCellInputType READONLY,
    @OperatorName nvarchar(100) = NULL,
    @WorkstationName nvarchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @TrayId bigint;

    IF EXISTS (SELECT 1 FROM @Cells WHERE ChannelNo NOT BETWEEN 1 AND 256)
        THROW 50002, 'ChannelNo must be between 1 and 256.', 1;

    IF EXISTS (SELECT 1 FROM @Cells GROUP BY ChannelNo HAVING COUNT(*) > 1)
        THROW 50003, 'Duplicate ChannelNo exists in tray cell input.', 1;

    EXEC dbo.usp_RegisterTray
        @TrayCode = @TrayCode,
        @SourceType = N'TrayCellSave',
        @OperatorName = @OperatorName,
        @TrayId = @TrayId OUTPUT,
        @ReturnRow = 0;

    BEGIN TRAN;

    ;WITH Normalized AS
    (
        SELECT
            ChannelNo,
            COALESCE(RowNo, CONVERT(tinyint, ((ChannelNo - 1) % 16) + 1)) AS RowNo,
            COALESCE(ColNo, CONVERT(tinyint, ((ChannelNo - 1) / 16) + 1)) AS ColNo,
            NULLIF(ltrim(rtrim(BatteryCode)), N'') AS BatteryCode,
            COALESCE(NULLIF(ltrim(rtrim(SourceType)), N''), N'ManualImport') AS SourceType
        FROM @Cells
    )
    MERGE dbo.TrayCell AS target
    USING Normalized AS source
        ON target.TrayId = @TrayId
        AND target.ChannelNo = source.ChannelNo
        AND target.IsActive = 1
    WHEN MATCHED THEN
        UPDATE SET
            RowNo = source.RowNo,
            ColNo = source.ColNo,
            BatteryCode = source.BatteryCode,
            SourceType = source.SourceType,
            UpdatedAt = sysdatetime()
    WHEN NOT MATCHED THEN
        INSERT (TrayId, ChannelNo, RowNo, ColNo, BatteryCode, SourceType, IsActive)
        VALUES (@TrayId, source.ChannelNo, source.RowNo, source.ColNo, source.BatteryCode, source.SourceType, 1);

    INSERT INTO dbo.AuditLog(EntityName, EntityKey, ActionName, AfterJson, OperatorName, WorkstationName)
    VALUES
    (
        N'TrayCell',
        @TrayCode,
        N'Upsert',
        (SELECT ChannelNo, BatteryCode, SourceType, RowNo, ColNo FROM @Cells FOR JSON PATH),
        @OperatorName,
        @WorkstationName
    );

    COMMIT TRAN;

    EXEC dbo.usp_GetTrayCellsByTrayCode @TrayCode = @TrayCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetTrayCellsByTrayCode
    @TrayCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.TrayId,
        t.TrayCode,
        tc.ChannelNo,
        tc.RowNo,
        tc.ColNo,
        tc.BatteryCode,
        tc.SourceType,
        tc.UpdatedAt,
        tc.CreatedAt
    FROM dbo.Tray t
    INNER JOIN dbo.TrayCell tc ON tc.TrayId = t.TrayId AND tc.IsActive = 1
    WHERE t.TrayCode = @TrayCode
    ORDER BY tc.ChannelNo;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateTestTask
    @TaskCode nvarchar(100) = NULL,
    @IdempotencyKey nvarchar(200) = NULL,
    @TrayCode nvarchar(100),
    @OcvStageCode nvarchar(20),
    @TestType nvarchar(30),
    @SelectedChannelsJson nvarchar(max) = NULL,
    @FirstChannelNo smallint = NULL,
    @LastChannelNo smallint = NULL,
    @ChannelCount int = NULL,
    @EquipmentCode nvarchar(50) = NULL,
    @WorkstationName nvarchar(100) = NULL,
    @OperatorName nvarchar(100) = NULL,
    @IsRetest bit = 0,
    @TestSpecId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ExistingTaskId bigint;
    DECLARE @TrayId bigint;
    DECLARE @OcvStageId int;
    DECLARE @TestRound int;
    DECLARE @Now datetime2(0) = sysdatetime();

    SET @TrayCode = NULLIF(ltrim(rtrim(@TrayCode)), N'');
    SET @OcvStageCode = NULLIF(ltrim(rtrim(@OcvStageCode)), N'');
    SET @TestType = COALESCE(NULLIF(ltrim(rtrim(@TestType)), N''), N'Formal');
    SET @TaskCode = COALESCE(NULLIF(ltrim(rtrim(@TaskCode)), N''), CONCAT(N'TASK-', replace(CONVERT(nvarchar(36), NEWID()), N'-', N'')));

    IF @TrayCode IS NULL
        THROW 50004, 'TrayCode is required.', 1;

    IF @OcvStageCode IS NULL
        THROW 50005, 'OcvStageCode is required.', 1;

    IF @SelectedChannelsJson IS NOT NULL AND isjson(@SelectedChannelsJson) <> 1
        THROW 50006, 'SelectedChannelsJson is not valid JSON.', 1;

    IF @FirstChannelNo IS NOT NULL AND (@FirstChannelNo NOT BETWEEN 1 AND 256)
        THROW 50007, 'FirstChannelNo must be between 1 and 256.', 1;

    IF @LastChannelNo IS NOT NULL AND (@LastChannelNo NOT BETWEEN 1 AND 256)
        THROW 50008, 'LastChannelNo must be between 1 and 256.', 1;

    IF @FirstChannelNo IS NOT NULL AND @LastChannelNo IS NOT NULL AND @FirstChannelNo > @LastChannelNo
        THROW 50009, 'FirstChannelNo cannot be greater than LastChannelNo.', 1;

    SELECT @ExistingTaskId = TestTaskId
    FROM dbo.TestTask
    WHERE @IdempotencyKey IS NOT NULL AND IdempotencyKey = @IdempotencyKey;

    IF @ExistingTaskId IS NOT NULL
    BEGIN
        SELECT * FROM dbo.TestTask WHERE TestTaskId = @ExistingTaskId;
        RETURN;
    END

    SELECT @OcvStageId = OcvStageId
    FROM dbo.OcvStage
    WHERE StageCode = @OcvStageCode AND IsActive = 1;

    IF @OcvStageId IS NULL
        THROW 50010, 'OcvStageCode does not exist or is inactive.', 1;

    EXEC dbo.usp_RegisterTray
        @TrayCode = @TrayCode,
        @SourceType = N'TestTask',
        @OperatorName = @OperatorName,
        @TrayId = @TrayId OUTPUT,
        @ReturnRow = 0;

    SET @ChannelCount = COALESCE(
        @ChannelCount,
        CASE
            WHEN @FirstChannelNo IS NOT NULL AND @LastChannelNo IS NOT NULL THEN @LastChannelNo - @FirstChannelNo + 1
            ELSE 0
        END
    );

    IF @ChannelCount NOT BETWEEN 0 AND 256
        THROW 50011, 'ChannelCount must be between 0 and 256.', 1;

    BEGIN TRAN;

    SELECT @TestRound = ISNULL(MAX(TestRound), 0) + 1
    FROM dbo.TestTask WITH (UPDLOCK, HOLDLOCK)
    WHERE TrayId = @TrayId
      AND OcvStageId = @OcvStageId
      AND TestType = @TestType
      AND IsRetest = @IsRetest;

    INSERT INTO dbo.TestTask
    (
        TaskCode, IdempotencyKey, TrayId, TrayCode, OcvStageId, TestSpecId,
        TestType, TestRound, IsRetest, SelectedChannelsJson,
        FirstChannelNo, LastChannelNo, ChannelCount,
        EquipmentCode, WorkstationName, OperatorName, Status, CreatedAt
    )
    VALUES
    (
        @TaskCode, @IdempotencyKey, @TrayId, @TrayCode, @OcvStageId, @TestSpecId,
        @TestType, @TestRound, @IsRetest, @SelectedChannelsJson,
        @FirstChannelNo, @LastChannelNo, @ChannelCount,
        @EquipmentCode, @WorkstationName, @OperatorName, N'Created', @Now
    );

    SET @ExistingTaskId = CONVERT(bigint, SCOPE_IDENTITY());

    COMMIT TRAN;

    SELECT * FROM dbo.TestTask WHERE TestTaskId = @ExistingTaskId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveTestResults
    @TestTaskId bigint,
    @Results dbo.TestResultInputType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @TrayId bigint;
    DECLARE @TrayCode nvarchar(100);
    DECLARE @OcvStageId int;

    IF EXISTS (SELECT 1 FROM @Results WHERE ChannelNo NOT BETWEEN 1 AND 256)
        THROW 50012, 'ChannelNo must be between 1 and 256.', 1;

    IF EXISTS (SELECT 1 FROM @Results GROUP BY ChannelNo HAVING COUNT(*) > 1)
        THROW 50013, 'Duplicate ChannelNo exists in test result input.', 1;

    SELECT
        @TrayId = TrayId,
        @TrayCode = TrayCode,
        @OcvStageId = OcvStageId
    FROM dbo.TestTask
    WHERE TestTaskId = @TestTaskId;

    IF @TrayId IS NULL
        THROW 50014, 'TestTaskId does not exist.', 1;

    BEGIN TRAN;

    UPDATE dbo.TestTask
    SET Status = CASE WHEN Status = N'Created' THEN N'Running' ELSE Status END,
        StartTime = CASE WHEN StartTime IS NULL THEN sysdatetime() ELSE StartTime END,
        UpdatedAt = sysdatetime()
    WHERE TestTaskId = @TestTaskId;

    ;WITH Normalized AS
    (
        SELECT
            r.ChannelNo,
            COALESCE(NULLIF(ltrim(rtrim(r.BatteryCode)), N''), tc.BatteryCode) AS BatteryCode,
            r.ExecutionOrder,
            r.VoltageVolt,
            r.RawResistanceMilliohm,
            COALESCE(r.CalibrationValueMilliohm, cc.CalibrationValueMilliohm, CONVERT(decimal(18,6), 0)) AS CalibrationValueMilliohm,
            COALESCE
            (
                r.FinalResistanceMilliohm,
                CASE
                    WHEN r.RawResistanceMilliohm IS NULL THEN NULL
                    ELSE r.RawResistanceMilliohm + COALESCE(r.CalibrationValueMilliohm, cc.CalibrationValueMilliohm, CONVERT(decimal(18,6), 0))
                END
            ) AS FinalResistanceMilliohm,
            r.IsNg,
            r.NgReason,
            COALESCE(NULLIF(ltrim(rtrim(r.ResultSource)), N''), N'FormalTest') AS ResultSource
        FROM @Results r
        LEFT JOIN dbo.TrayCell tc
            ON tc.TrayId = @TrayId
           AND tc.ChannelNo = r.ChannelNo
           AND tc.IsActive = 1
        LEFT JOIN dbo.ChannelCalibration cc
            ON cc.ChannelNo = r.ChannelNo
           AND cc.MeasurementType = N'Resistance'
           AND cc.IsActive = 1
    )
    MERGE dbo.TestResult AS target
    USING Normalized AS source
        ON target.TestTaskId = @TestTaskId
       AND target.ChannelNo = source.ChannelNo
    WHEN MATCHED THEN
        UPDATE SET
            BatteryCode = source.BatteryCode,
            ExecutionOrder = source.ExecutionOrder,
            VoltageVolt = source.VoltageVolt,
            RawResistanceMilliohm = source.RawResistanceMilliohm,
            CalibrationValueMilliohm = source.CalibrationValueMilliohm,
            FinalResistanceMilliohm = source.FinalResistanceMilliohm,
            IsNg = source.IsNg,
            NgReason = source.NgReason,
            ResultSource = source.ResultSource,
            UpdatedAt = sysdatetime()
    WHEN NOT MATCHED THEN
        INSERT
        (
            TestTaskId, TrayId, TrayCode, OcvStageId,
            ChannelNo, BatteryCode, ExecutionOrder,
            VoltageVolt, RawResistanceMilliohm, CalibrationValueMilliohm, FinalResistanceMilliohm,
            IsNg, NgReason, ResultSource
        )
        VALUES
        (
            @TestTaskId, @TrayId, @TrayCode, @OcvStageId,
            source.ChannelNo, source.BatteryCode, source.ExecutionOrder,
            source.VoltageVolt, source.RawResistanceMilliohm, source.CalibrationValueMilliohm, source.FinalResistanceMilliohm,
            source.IsNg, source.NgReason, source.ResultSource
        );

    COMMIT TRAN;

    SELECT *
    FROM dbo.TestResult
    WHERE TestTaskId = @TestTaskId
    ORDER BY ChannelNo;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CompleteTestTask
    @TestTaskId bigint,
    @Status nvarchar(30) = N'Completed',
    @ErrorMessage nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Status NOT IN (N'Completed', N'Failed', N'Canceled')
        THROW 50015, 'Status must be Completed, Failed, or Canceled.', 1;

    UPDATE dbo.TestTask
    SET Status = @Status,
        EndTime = sysdatetime(),
        ErrorMessage = @ErrorMessage,
        UpdatedAt = sysdatetime()
    WHERE TestTaskId = @TestTaskId;

    IF @@ROWCOUNT = 0
        THROW 50016, 'TestTaskId does not exist.', 1;

    SELECT * FROM dbo.TestTask WHERE TestTaskId = @TestTaskId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetTrayLatestStageResults
    @TrayCode nvarchar(100),
    @OcvStageCode nvarchar(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RankedTask AS
    (
        SELECT
            tt.TestTaskId,
            ROW_NUMBER() OVER
            (
                PARTITION BY tt.TrayId, tt.OcvStageId
                ORDER BY tt.TestRound DESC, tt.EndTime DESC, tt.TestTaskId DESC
            ) AS rn
        FROM dbo.TestTask tt
        INNER JOIN dbo.Tray t ON t.TrayId = tt.TrayId
        INNER JOIN dbo.OcvStage os ON os.OcvStageId = tt.OcvStageId
        WHERE t.TrayCode = @TrayCode
          AND tt.Status = N'Completed'
          AND (@OcvStageCode IS NULL OR os.StageCode = @OcvStageCode)
    )
    SELECT
        t.TrayCode,
        os.StageCode AS OcvStage,
        tt.TestRound,
        tt.TestType,
        tt.Status AS TaskStatus,
        tr.ChannelNo,
        tc.RowNo,
        tc.ColNo,
        COALESCE(tr.BatteryCode, tc.BatteryCode) AS BatteryCode,
        tr.VoltageVolt,
        tr.RawResistanceMilliohm,
        tr.CalibrationValueMilliohm,
        tr.FinalResistanceMilliohm,
        tr.IsNg,
        tr.NgReason,
        tt.StartTime,
        tt.EndTime,
        tr.CreatedAt AS ResultCreatedAt
    FROM RankedTask rt
    INNER JOIN dbo.TestTask tt ON tt.TestTaskId = rt.TestTaskId
    INNER JOIN dbo.TestResult tr ON tr.TestTaskId = tt.TestTaskId
    INNER JOIN dbo.Tray t ON t.TrayId = tt.TrayId
    INNER JOIN dbo.OcvStage os ON os.OcvStageId = tt.OcvStageId
    LEFT JOIN dbo.TrayCell tc ON tc.TrayId = tt.TrayId AND tc.ChannelNo = tr.ChannelNo AND tc.IsActive = 1
    WHERE rt.rn = 1
    ORDER BY os.StageCode, tr.ChannelNo;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EnqueueUpload
    @IdempotencyKey nvarchar(200),
    @UploadType nvarchar(50),
    @BusinessKey nvarchar(200),
    @TestTaskId bigint = NULL,
    @PayloadJson nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(ltrim(rtrim(@IdempotencyKey)), N'') IS NULL
        THROW 50017, 'IdempotencyKey is required.', 1;

    IF isjson(@PayloadJson) <> 1
        THROW 50018, 'PayloadJson is not valid JSON.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.UploadOutbox WHERE IdempotencyKey = @IdempotencyKey)
    BEGIN
        INSERT INTO dbo.UploadOutbox(IdempotencyKey, UploadType, BusinessKey, TestTaskId, PayloadJson)
        VALUES (@IdempotencyKey, @UploadType, @BusinessKey, @TestTaskId, @PayloadJson);
    END

    SELECT *
    FROM dbo.UploadOutbox
    WHERE IdempotencyKey = @IdempotencyKey;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPendingUploads
    @BatchSize int = 20,
    @LockedBy nvarchar(100),
    @LockSeconds int = 120
AS
BEGIN
    SET NOCOUNT ON;

    IF @BatchSize IS NULL OR @BatchSize < 1 OR @BatchSize > 200
        SET @BatchSize = 20;

    ;WITH Pending AS
    (
        SELECT TOP (@BatchSize) *
        FROM dbo.UploadOutbox WITH (READPAST, UPDLOCK, ROWLOCK)
        WHERE Status IN (N'Pending', N'Failed', N'Uploading')
          AND NextRetryAt <= sysdatetime()
          AND
          (
              Status <> N'Uploading'
              OR LockedAt IS NULL
              OR dateadd(second, @LockSeconds, LockedAt) < sysdatetime()
          )
        ORDER BY NextRetryAt ASC, CreatedAt ASC
    )
    UPDATE Pending
    SET Status = N'Uploading',
        LockedBy = @LockedBy,
        LockedAt = sysdatetime(),
        UpdatedAt = sysdatetime()
    OUTPUT inserted.*;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_MarkUploadSent
    @UploadOutboxId bigint
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.UploadOutbox
    SET Status = N'Sent',
        SentAt = sysdatetime(),
        LockedBy = NULL,
        LockedAt = NULL,
        LastError = NULL,
        UpdatedAt = sysdatetime()
    WHERE UploadOutboxId = @UploadOutboxId;

    IF @@ROWCOUNT = 0
        THROW 50019, 'UploadOutboxId does not exist.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_MarkUploadFailed
    @UploadOutboxId bigint,
    @ErrorMessage nvarchar(max),
    @RetryDelaySeconds int = 60
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.UploadOutbox
    SET Status = N'Failed',
        RetryCount = RetryCount + 1,
        NextRetryAt = dateadd(second, ISNULL(@RetryDelaySeconds, 60), sysdatetime()),
        LockedBy = NULL,
        LockedAt = NULL,
        LastError = @ErrorMessage,
        UpdatedAt = sysdatetime()
    WHERE UploadOutboxId = @UploadOutboxId;

    IF @@ROWCOUNT = 0
        THROW 50020, 'UploadOutboxId does not exist.', 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveMesUploadLog
    @UploadOutboxId bigint = NULL,
    @TestTaskId bigint = NULL,
    @UploadType nvarchar(50),
    @BusinessKey nvarchar(200) = NULL,
    @RequestJson nvarchar(max) = NULL,
    @ResponseJson nvarchar(max) = NULL,
    @IsSuccess bit,
    @ErrorMessage nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.MesUploadLog
    (
        UploadOutboxId, TestTaskId, UploadType, BusinessKey,
        RequestJson, ResponseJson, IsSuccess, ErrorMessage
    )
    VALUES
    (
        @UploadOutboxId, @TestTaskId, @UploadType, @BusinessKey,
        @RequestJson, @ResponseJson, @IsSuccess, @ErrorMessage
    );

    SELECT * FROM dbo.MesUploadLog WHERE MesUploadLogId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetActiveCalibrationValues
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ChannelNo, MeasurementType, CalibrationValueMilliohm, StandardValue, MeasuredValue, EffectiveFrom
    FROM dbo.ChannelCalibration
    WHERE IsActive = 1
    ORDER BY ChannelNo;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveChannelCalibration
    @ChannelNo smallint,
    @StandardValue decimal(18,6) = NULL,
    @MeasuredValue decimal(18,6) = NULL,
    @CalibrationValueMilliohm decimal(18,6),
    @OperatorName nvarchar(100) = NULL,
    @Remark nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @ChannelNo NOT BETWEEN 1 AND 256
        THROW 50021, 'ChannelNo must be between 1 and 256.', 1;

    BEGIN TRAN;

    UPDATE dbo.ChannelCalibration
    SET IsActive = 0,
        EffectiveTo = sysdatetime()
    WHERE ChannelNo = @ChannelNo
      AND MeasurementType = N'Resistance'
      AND IsActive = 1;

    INSERT INTO dbo.ChannelCalibration
    (
        ChannelNo, MeasurementType, StandardValue, MeasuredValue,
        CalibrationValueMilliohm, IsActive, OperatorName, Remark
    )
    VALUES
    (
        @ChannelNo, N'Resistance', @StandardValue, @MeasuredValue,
        @CalibrationValueMilliohm, 1, @OperatorName, @Remark
    );

    INSERT INTO dbo.AuditLog(EntityName, EntityKey, ActionName, AfterJson, OperatorName)
    VALUES
    (
        N'ChannelCalibration',
        CONVERT(nvarchar(20), @ChannelNo),
        N'Update',
        (
            SELECT @ChannelNo AS ChannelNo,
                   @StandardValue AS StandardValue,
                   @MeasuredValue AS MeasuredValue,
                   @CalibrationValueMilliohm AS CalibrationValueMilliohm
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        ),
        @OperatorName
    );

    COMMIT TRAN;

    SELECT *
    FROM dbo.ChannelCalibration
    WHERE ChannelNo = @ChannelNo AND MeasurementType = N'Resistance' AND IsActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveDeviceCommLog
    @TestTaskId bigint = NULL,
    @DeviceName nvarchar(50),
    @Direction nvarchar(10),
    @CommandName nvarchar(100) = NULL,
    @FrameHex nvarchar(max) = NULL,
    @IsSuccess bit = 1,
    @ErrorMessage nvarchar(max) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.DeviceCommLog
    (
        TestTaskId, DeviceName, Direction, CommandName,
        FrameHex, IsSuccess, ErrorMessage
    )
    VALUES
    (
        @TestTaskId, @DeviceName, @Direction, @CommandName,
        @FrameHex, @IsSuccess, @ErrorMessage
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveAlarmEvent
    @AlarmCode nvarchar(50) = NULL,
    @AlarmMessage nvarchar(500),
    @Severity nvarchar(20) = N'Error',
    @OperatorName nvarchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AlarmEvent(AlarmCode, AlarmMessage, Severity, OperatorName)
    VALUES (@AlarmCode, @AlarmMessage, @Severity, @OperatorName);

    SELECT * FROM dbo.AlarmEvent WHERE AlarmEventId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveAuditLog
    @EntityName nvarchar(100),
    @EntityKey nvarchar(200),
    @ActionName nvarchar(50),
    @BeforeJson nvarchar(max) = NULL,
    @AfterJson nvarchar(max) = NULL,
    @OperatorName nvarchar(100) = NULL,
    @WorkstationName nvarchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AuditLog
    (
        EntityName, EntityKey, ActionName,
        BeforeJson, AfterJson, OperatorName, WorkstationName
    )
    VALUES
    (
        @EntityName, @EntityKey, @ActionName,
        @BeforeJson, @AfterJson, @OperatorName, @WorkstationName
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEntityAuditTrail
    @EntityName nvarchar(100),
    @EntityKey nvarchar(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM dbo.AuditLog
    WHERE EntityName = @EntityName
      AND EntityKey = @EntityKey
    ORDER BY CreatedAt DESC, AuditId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSchemaVersion
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1) *
    FROM dbo.SchemaVersion
    ORDER BY SchemaVersionId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CheckDatabaseHealth
AS
BEGIN
    SET NOCOUNT ON;

    SELECT N'Database' AS CheckName,
           CONVERT(bit, CASE WHEN DB_NAME() = N'OCV_DATA' THEN 1 ELSE 0 END) AS IsOk,
           DB_NAME() AS Detail
    UNION ALL
    SELECT N'SchemaVersion',
           CONVERT(bit, CASE WHEN EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo IN (N'001', N'002')) THEN 1 ELSE 0 END),
           COALESCE((SELECT TOP (1) VersionNo FROM dbo.SchemaVersion ORDER BY SchemaVersionId DESC), N'Missing')
    UNION ALL
    SELECT N'OcvStage',
           CONVERT(bit, CASE WHEN EXISTS (SELECT 1 FROM dbo.OcvStage WHERE IsActive = 1) THEN 1 ELSE 0 END),
           CONCAT(N'ActiveCount=', (SELECT COUNT(*) FROM dbo.OcvStage WHERE IsActive = 1))
    UNION ALL
    SELECT N'PendingUploads',
           CONVERT(bit, 1),
           CONCAT(N'PendingOrFailed=', (SELECT COUNT(*) FROM dbo.UploadOutbox WHERE Status IN (N'Pending', N'Failed')))
    UNION ALL
    SELECT N'UnfinishedTasks',
           CONVERT(bit, CASE WHEN (SELECT COUNT(*) FROM dbo.TestTask WHERE Status IN (N'Created', N'Running')) < 100 THEN 1 ELSE 0 END),
           CONCAT(N'CreatedOrRunning=', (SELECT COUNT(*) FROM dbo.TestTask WHERE Status IN (N'Created', N'Running')));
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo = N'002')
BEGIN
    INSERT INTO dbo.SchemaVersion(VersionNo, ScriptName, Remark)
    VALUES (N'002', N'002_create_core_procedures.sql', N'Core stored procedures.');
END
GO
