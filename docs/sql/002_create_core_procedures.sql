/*
  OCV_DATA core stored procedures
  Run after: 001_create_ocv_data_schema.sql
*/

USE OCV_DATA;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
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

CREATE OR ALTER PROCEDURE dbo.usp_UpsertCellOcvSummary
    @TestTaskId bigint,
    @StageTestTime datetime2(3),
    @Channels dbo.TestResultInputType READONLY,
    @TestResultId bigint = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @StageTestTime IS NULL
        THROW 51910, 'StageTestTime is required for cell OCV summary.', 1;

    DECLARE @StageCode nvarchar(20);
    SELECT @StageCode = os.StageCode
    FROM dbo.TestTask tt
    INNER JOIN dbo.OcvStage os ON os.OcvStageId = tt.OcvStageId
    WHERE tt.TestTaskId = @TestTaskId;
    IF @StageCode IS NULL OR @StageCode NOT IN (N'OCV0', N'OCV1', N'OCV2') RETURN;

    DECLARE @Source TABLE
    (
        BatteryCode nvarchar(100) NOT NULL PRIMARY KEY,
        VoltageVolt decimal(18,9) NULL,
        FinalResistanceMilliohm decimal(18,6) NULL,
        TrayCode nvarchar(100) NOT NULL,
        ChannelNo smallint NOT NULL
    );
    INSERT @Source(BatteryCode, VoltageVolt, FinalResistanceMilliohm, TrayCode, ChannelNo)
    SELECT LTRIM(RTRIM(tr.BatteryCode)), tr.VoltageVolt, tr.FinalResistanceMilliohm, tr.TrayCode, tr.ChannelNo
    FROM dbo.TestResult tr
    WHERE tr.TestTaskId = @TestTaskId
      AND ((@TestResultId IS NOT NULL AND tr.TestResultId = @TestResultId)
           OR (@TestResultId IS NULL AND EXISTS (SELECT 1 FROM @Channels c WHERE c.ChannelNo=tr.ChannelNo)))
      AND NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N'') IS NOT NULL;

    UPDATE summary
    SET Vol0 = CASE WHEN @StageCode = N'OCV0' THEN source.VoltageVolt ELSE summary.Vol0 END,
        Res0 = CASE WHEN @StageCode = N'OCV0' THEN source.FinalResistanceMilliohm ELSE summary.Res0 END,
        Time0 = CASE WHEN @StageCode = N'OCV0' THEN @StageTestTime ELSE summary.Time0 END,
        Vol1 = CASE WHEN @StageCode = N'OCV1' THEN source.VoltageVolt ELSE summary.Vol1 END,
        Res1 = CASE WHEN @StageCode = N'OCV1' THEN source.FinalResistanceMilliohm ELSE summary.Res1 END,
        Time1 = CASE WHEN @StageCode = N'OCV1' THEN @StageTestTime ELSE summary.Time1 END,
        Vol2 = CASE WHEN @StageCode = N'OCV2' THEN source.VoltageVolt ELSE summary.Vol2 END,
        Res2 = CASE WHEN @StageCode = N'OCV2' THEN source.FinalResistanceMilliohm ELSE summary.Res2 END,
        Time2 = CASE WHEN @StageCode = N'OCV2' THEN @StageTestTime ELSE summary.Time2 END,
        TrayCode = source.TrayCode,
        ChannelNo = source.ChannelNo,
        UpdatedAt = sysdatetime()
    FROM dbo.CellOcvSummary summary WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN @Source source ON source.BatteryCode = summary.BatteryCode;

    INSERT dbo.CellOcvSummary(BatteryCode, Vol0, Res0, Time0,
        Vol1, Res1, Time1, Vol2, Res2, Time2, TrayCode, ChannelNo)
    SELECT source.BatteryCode,
        CASE WHEN @StageCode=N'OCV0' THEN source.VoltageVolt END,
        CASE WHEN @StageCode=N'OCV0' THEN source.FinalResistanceMilliohm END,
        CASE WHEN @StageCode=N'OCV0' THEN @StageTestTime END,
        CASE WHEN @StageCode=N'OCV1' THEN source.VoltageVolt END,
        CASE WHEN @StageCode=N'OCV1' THEN source.FinalResistanceMilliohm END,
        CASE WHEN @StageCode=N'OCV1' THEN @StageTestTime END,
        CASE WHEN @StageCode=N'OCV2' THEN source.VoltageVolt END,
        CASE WHEN @StageCode=N'OCV2' THEN source.FinalResistanceMilliohm END,
        CASE WHEN @StageCode=N'OCV2' THEN @StageTestTime END,
        source.TrayCode, source.ChannelNo
    FROM @Source source
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.CellOcvSummary summary WITH (UPDLOCK, HOLDLOCK)
        WHERE summary.BatteryCode = source.BatteryCode
    );

    UPDATE summary
    SET K1 = CASE
            WHEN summary.Vol0 IS NULL OR summary.Vol1 IS NULL OR summary.Time0 IS NULL OR summary.Time1 IS NULL
              OR DATEDIFF_BIG(millisecond, summary.Time0, summary.Time1) <= 0 THEN NULL
            ELSE CONVERT(decimal(18,9), CONVERT(decimal(28,12), summary.Vol1 - summary.Vol0) * 3600000.0 /
                         DATEDIFF_BIG(millisecond, summary.Time0, summary.Time1)) END,
        K2 = CASE
            WHEN summary.Vol1 IS NULL OR summary.Vol2 IS NULL OR summary.Time1 IS NULL OR summary.Time2 IS NULL
              OR DATEDIFF_BIG(millisecond, summary.Time1, summary.Time2) <= 0 THEN NULL
            ELSE CONVERT(decimal(18,9), CONVERT(decimal(28,12), summary.Vol2 - summary.Vol1) * 3600000.0 /
                         DATEDIFF_BIG(millisecond, summary.Time1, summary.Time2)) END
    FROM dbo.CellOcvSummary summary
    INNER JOIN @Source source ON source.BatteryCode = summary.BatteryCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCellOcvSummary
    @BatteryCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET @BatteryCode = NULLIF(LTRIM(RTRIM(@BatteryCode)), N'');
    IF @BatteryCode IS NULL
        THROW 51911, 'BatteryCode is required.', 1;

    SELECT BatteryCode, Vol0, Vol1, Vol2, Res0, Res1, Res2, Time0, Time1, Time2,
           K1, K2, TrayCode, ChannelNo, CreatedAt, UpdatedAt, RowVersion
    FROM dbo.CellOcvSummary
    WHERE BatteryCode = @BatteryCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveTestResults
    @TestTaskId bigint,
    @StageTestTime datetime2(3),
    @Results dbo.TestResultInputType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        -- Serialize result identity changes across every task/mode, including NG overwrite.
        DECLARE @LockResult int;
        EXEC @LockResult = sys.sp_getapplock
            @Resource = N'OCV_DATA.TestResult.BarcodeIdentity',
            @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000;
        IF @LockResult < 0
            RAISERROR(N'Unable to acquire the test result barcode save lock.', 16, 1);

        DECLARE @TrayId bigint, @TrayCode nvarchar(100), @OcvStageId int, @TestSpecId int;
        SELECT @TrayId = TrayId, @TrayCode = TrayCode,
               @OcvStageId = OcvStageId, @TestSpecId = TestSpecId
        FROM dbo.TestTask WITH (UPDLOCK, HOLDLOCK) WHERE TestTaskId = @TestTaskId;
        IF @TrayId IS NULL
            RAISERROR(N'TestTaskId does not exist.', 16, 1);
        IF @StageTestTime IS NULL
            RAISERROR(N'StageTestTime is required.', 16, 1);
        IF EXISTS (SELECT 1 FROM @Results WHERE ChannelNo NOT BETWEEN 1 AND 256)
            RAISERROR(N'ChannelNo must be between 1 and 256.', 16, 1);
        IF EXISTS (SELECT 1 FROM @Results GROUP BY ChannelNo HAVING COUNT(*) > 1)
            RAISERROR(N'Duplicate ChannelNo exists in test result input.', 16, 1);

        DECLARE @Normalized TABLE
        (
            ChannelNo smallint NOT NULL, BatteryCode nvarchar(100) NULL,
            ExecutionOrder int NULL, VoltageVolt decimal(18,9) NULL,
            RawResistanceMilliohm decimal(18,6) NULL, CalibrationValueMilliohm decimal(18,6) NULL,
            FinalResistanceMilliohm decimal(18,6) NULL,
            IsNg bit NOT NULL, NgReason nvarchar(500) NULL, ResultSource nvarchar(30) NOT NULL
        );
        INSERT @Normalized
        SELECT r.ChannelNo,
               COALESCE(NULLIF(LTRIM(RTRIM(r.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(tc.BatteryCode)), N'')),
               r.ExecutionOrder, r.VoltageVolt, r.RawResistanceMilliohm,
               COALESCE(r.CalibrationValueMilliohm, cc.CalibrationValueMilliohm, CONVERT(decimal(18,6), 0)),
               COALESCE(r.FinalResistanceMilliohm,
                   r.RawResistanceMilliohm + COALESCE(r.CalibrationValueMilliohm, cc.CalibrationValueMilliohm, CONVERT(decimal(18,6), 0))),
               r.IsNg, r.NgReason, COALESCE(NULLIF(LTRIM(RTRIM(r.ResultSource)), N''), N'FormalTest')
        FROM @Results r
        LEFT JOIN dbo.TrayCell tc ON tc.TrayId = @TrayId AND tc.ChannelNo = r.ChannelNo AND tc.IsActive = 1
        LEFT JOIN dbo.ChannelCalibration cc ON cc.ChannelNo = r.ChannelNo AND cc.MeasurementType = N'Resistance' AND cc.IsActive = 1;
        IF EXISTS (SELECT 1 FROM @Normalized WHERE BatteryCode IS NULL)
            RAISERROR(N'BatteryCode is required for every result after tray-cell fallback.', 16, 1);
        DECLARE @DuplicateBatteryCode nvarchar(100);
        SELECT TOP (1) @DuplicateBatteryCode = BatteryCode
        FROM @Normalized GROUP BY BatteryCode HAVING COUNT(*) > 1 ORDER BY BatteryCode;
        IF @DuplicateBatteryCode IS NOT NULL
            RAISERROR(N'Duplicate normalized BatteryCode exists in test result input: %s.', 16, 1, @DuplicateBatteryCode);

        -- Occupants included in this batch may move channels together, including swaps.
        IF EXISTS
        (
            SELECT 1 FROM dbo.TestResult tr WITH (UPDLOCK, HOLDLOCK)
            LEFT JOIN dbo.TrayCell existingCell ON existingCell.TrayId = tr.TrayId AND existingCell.ChannelNo = tr.ChannelNo AND existingCell.IsActive = 1
            INNER JOIN @Normalized n ON n.ChannelNo = tr.ChannelNo
            WHERE tr.TestTaskId = @TestTaskId
              AND NOT EXISTS (SELECT 1 FROM @Normalized b WHERE b.BatteryCode = COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(existingCell.BatteryCode)), N'')))
        )
            RAISERROR(N'Target task/channel belongs to a barcode outside this save batch.', 16, 1);

        DECLARE @Existing TABLE (TestResultId bigint PRIMARY KEY, CanonicalId bigint NOT NULL, BatteryCode nvarchar(100) NOT NULL);
        ;WITH Ranked AS
        (
            SELECT tr.TestResultId, COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(existingCell.BatteryCode)), N'')) AS BatteryCode,
                ROW_NUMBER() OVER (PARTITION BY COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(existingCell.BatteryCode)), N''))
                    ORDER BY COALESCE(tr.UpdatedAt, tr.CreatedAt) DESC, tr.TestResultId DESC) AS rn
            FROM dbo.TestResult tr WITH (UPDLOCK, HOLDLOCK)
            LEFT JOIN dbo.TrayCell existingCell ON existingCell.TrayId = tr.TrayId AND existingCell.ChannelNo = tr.ChannelNo AND existingCell.IsActive = 1
            INNER JOIN @Normalized n ON n.BatteryCode = COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(existingCell.BatteryCode)), N''))
        )
        INSERT @Existing(TestResultId, CanonicalId, BatteryCode)
        SELECT r.TestResultId, c.TestResultId, r.BatteryCode FROM Ranked r
        INNER JOIN Ranked c ON c.BatteryCode = r.BatteryCode AND c.rn = 1;
        UPDATE audit SET SourceTestResultId = e.CanonicalId
        FROM dbo.OCV_RETEST_DATA audit INNER JOIN @Existing e ON e.TestResultId = audit.SourceTestResultId
        WHERE e.TestResultId <> e.CanonicalId;
        DELETE tr FROM dbo.TestResult tr INNER JOIN @Existing e ON e.TestResultId = tr.TestResultId
        WHERE e.TestResultId <> e.CanonicalId;

        DECLARE @VoltageLower decimal(18,9), @VoltageUpper decimal(18,9),
                @ResistanceLower decimal(18,6), @ResistanceUpper decimal(18,6);
        SELECT TOP (1) @VoltageLower = LowerLimit, @VoltageUpper = UpperLimit
        FROM dbo.TestSpecLimit WHERE TestSpecId = @TestSpecId AND MeasurementType = N'Voltage' ORDER BY TestSpecLimitId DESC;
        SELECT TOP (1) @ResistanceLower = LowerLimit, @ResistanceUpper = UpperLimit
        FROM dbo.TestSpecLimit WHERE TestSpecId = @TestSpecId AND MeasurementType = N'Resistance' ORDER BY TestSpecLimitId DESC;

        UPDATE tr SET TestTaskId = @TestTaskId, TrayId = @TrayId, TrayCode = @TrayCode, OcvStageId = @OcvStageId,
            ChannelNo = n.ChannelNo, BatteryCode = n.BatteryCode, ExecutionOrder = n.ExecutionOrder,
            VoltageVolt = n.VoltageVolt, RawResistanceMilliohm = n.RawResistanceMilliohm,
            CalibrationValueMilliohm = n.CalibrationValueMilliohm, FinalResistanceMilliohm = n.FinalResistanceMilliohm,
            VoltageLowerLimit = @VoltageLower, VoltageUpperLimit = @VoltageUpper,
            ResistanceLowerLimit = @ResistanceLower, ResistanceUpperLimit = @ResistanceUpper,
            IsNg = n.IsNg, NgReason = n.NgReason, ResultSource = n.ResultSource, UpdatedAt = SYSDATETIME()
        FROM dbo.TestResult tr
        INNER JOIN @Existing e ON e.TestResultId = tr.TestResultId AND e.TestResultId = e.CanonicalId
        INNER JOIN @Normalized n ON n.BatteryCode = e.BatteryCode;
        INSERT dbo.TestResult
        (TestTaskId, TrayId, TrayCode, OcvStageId, ChannelNo, BatteryCode, ExecutionOrder,
         VoltageVolt, RawResistanceMilliohm, CalibrationValueMilliohm, FinalResistanceMilliohm,
         VoltageLowerLimit, VoltageUpperLimit, ResistanceLowerLimit, ResistanceUpperLimit, IsNg, NgReason, ResultSource)
        SELECT @TestTaskId, @TrayId, @TrayCode, @OcvStageId, n.ChannelNo, n.BatteryCode, n.ExecutionOrder,
               n.VoltageVolt, n.RawResistanceMilliohm, n.CalibrationValueMilliohm, n.FinalResistanceMilliohm,
               @VoltageLower, @VoltageUpper, @ResistanceLower, @ResistanceUpper, n.IsNg, n.NgReason, n.ResultSource
        FROM @Normalized n WHERE NOT EXISTS (SELECT 1 FROM dbo.TestResult tr WHERE tr.BatteryCode = n.BatteryCode);
        EXEC dbo.usp_UpsertCellOcvSummary @TestTaskId=@TestTaskId, @StageTestTime=@StageTestTime, @Channels=@Results;
        UPDATE dbo.TestTask SET Status = CASE WHEN Status = N'Created' THEN N'Running' ELSE Status END,
            StartTime = COALESCE(StartTime, SYSDATETIME()), UpdatedAt = SYSDATETIME() WHERE TestTaskId = @TestTaskId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage nvarchar(4000), @ErrorSeverity int, @ErrorState int;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        RAISERROR(N'%s', @ErrorSeverity, @ErrorState, @ErrorMessage);
        RETURN;
    END CATCH;
    SELECT * FROM dbo.TestResult WHERE TestTaskId = @TestTaskId ORDER BY ChannelNo;
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
    @OcvStageCode nvarchar(20) = NULL,
    @DebugMode bit = 0
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
          AND ((@DebugMode = 1 AND tt.TestType = N'Debug')
               OR (@DebugMode = 0 AND tt.TestType <> N'Debug'))
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

CREATE OR ALTER PROCEDURE dbo.usp_GetNgRetestSourceByBatteryCode
    @BatteryCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET @BatteryCode = NULLIF(LTRIM(RTRIM(@BatteryCode)), N'');

    SELECT TOP (1)
        tr.TestResultId,
        COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(tc.BatteryCode)), N'')) AS BatteryCode,
        t.TrayCode,
        tr.ChannelNo,
        tr.VoltageVolt AS OriginalVoltage,
        tr.FinalResistanceMilliohm AS OriginalResistance,
        CASE WHEN tr.IsNg = 1 THEN N'NG' ELSE N'OK' END AS OriginalResult,
        tr.CreatedAt AS OriginalTestTime,
        CONVERT(nvarchar(10), COALESCE(tc.RowNo, ((tr.ChannelNo - 1) / 16) + 1)) + N'-' +
            CONVERT(nvarchar(10), COALESCE(tc.ColNo, ((tr.ChannelNo - 1) % 16) + 1)) AS TrayPosition
    FROM dbo.TestResult tr
    INNER JOIN dbo.TestTask tt ON tt.TestTaskId = tr.TestTaskId
    INNER JOIN dbo.Tray t ON t.TrayId = tr.TrayId
    LEFT JOIN dbo.TrayCell tc
        ON tc.TrayId = tr.TrayId
       AND tc.ChannelNo = tr.ChannelNo
       AND tc.IsActive = 1
    WHERE COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(tc.BatteryCode)), N'')) = @BatteryCode
      AND tr.IsNg = 1
      AND tt.TestType <> N'Debug'
    ORDER BY COALESCE(tr.UpdatedAt, tr.CreatedAt) DESC, tr.TestResultId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SaveNgRetest
    @BatteryCode nvarchar(100),
    @TrayCode nvarchar(100),
    @ChannelNo smallint,
    @TrayPosition nvarchar(20),
    @OriginalVoltage real,
    @OriginalResistance real,
    @OriginalResult nvarchar(10),
    @RetestVoltage real,
    @RetestResistance real,
    @RetestRawResistanceMilliohm decimal(18,6),
    @CalibrationValueMilliohm decimal(18,6),
    @RetestFinalResistanceMilliohm decimal(18,6),
    @RetestResult nvarchar(10),
    @RetestTime datetime2,
    @OverwriteOriginal bit,
    @OperatorName nvarchar(100),
    @MeterPort nvarchar(20),
    @SourceTestResultId bigint = NULL,
    @RetestOperationId uniqueidentifier = NULL,
    @CalibrationSnapshotFingerprint char(64) = NULL,
    @CalibrationSnapshotSource nvarchar(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;
        -- Audit insertion also serializes with saves so source IDs cannot become dangling.
        DECLARE @LockResult int;
        EXEC @LockResult = sys.sp_getapplock
            @Resource = N'OCV_DATA.TestResult.BarcodeIdentity',
            @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 15000;
        IF @LockResult < 0
            RAISERROR(N'Unable to acquire the test result barcode save lock.', 16, 1);

        IF @RetestOperationId IS NOT NULL AND EXISTS
        (
            SELECT 1
            FROM dbo.OCV_RETEST_DATA WITH (UPDLOCK, HOLDLOCK)
            WHERE RetestOperationId = @RetestOperationId
        )
        BEGIN
            COMMIT TRANSACTION;
            RETURN;
        END

        SET @BatteryCode = NULLIF(LTRIM(RTRIM(@BatteryCode)), N'');
        IF @BatteryCode IS NULL
            RAISERROR(N'BatteryCode is required for NG retest.', 16, 1);
        DECLARE @BarcodeRows TABLE (TestResultId bigint PRIMARY KEY, CurrentAt datetime2(0) NOT NULL);
        INSERT @BarcodeRows(TestResultId, CurrentAt)
        SELECT tr.TestResultId, COALESCE(tr.UpdatedAt, tr.CreatedAt)
        FROM dbo.TestResult tr WITH (UPDLOCK, HOLDLOCK)
        LEFT JOIN dbo.TrayCell tc ON tc.TrayId = tr.TrayId AND tc.ChannelNo = tr.ChannelNo AND tc.IsActive = 1
        WHERE COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(tc.BatteryCode)), N'')) = @BatteryCode;
        DECLARE @CanonicalId bigint;
        SELECT TOP (1) @CanonicalId = TestResultId FROM @BarcodeRows ORDER BY CurrentAt DESC, TestResultId DESC;

        IF @OverwriteOriginal = 1
        BEGIN
            IF @SourceTestResultId IS NULL
                RAISERROR(N'SourceTestResultId is required when overwriting the original result.', 16, 1);
            IF @CanonicalId IS NULL OR @CanonicalId <> @SourceTestResultId
                RAISERROR(N'The queried NG source result is no longer the current barcode result.', 16, 1);

            UPDATE tr
            SET VoltageVolt = @RetestVoltage,
                RawResistanceMilliohm = @RetestRawResistanceMilliohm,
                CalibrationValueMilliohm = @CalibrationValueMilliohm,
                FinalResistanceMilliohm = @RetestFinalResistanceMilliohm,
                IsNg = CASE WHEN @RetestResult = N'NG' THEN 1 ELSE 0 END,
                UpdatedAt = sysdatetime(),
                ResultSource = N'NgRetest'
            FROM dbo.TestResult tr
            INNER JOIN dbo.Tray t ON t.TrayId = tr.TrayId
            LEFT JOIN dbo.TrayCell tc
                ON tc.TrayId = tr.TrayId
               AND tc.ChannelNo = tr.ChannelNo
               AND tc.IsActive = 1
            WHERE tr.TestResultId = @SourceTestResultId
              AND tr.IsNg = 1
              AND COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)), N''), NULLIF(LTRIM(RTRIM(tc.BatteryCode)), N'')) = @BatteryCode
              AND t.TrayCode = @TrayCode
              AND tr.ChannelNo = @ChannelNo;

            IF @@ROWCOUNT <> 1
                RAISERROR(N'The queried NG source result changed or no longer matches the barcode, tray, and channel.', 16, 1);

            UPDATE audit SET SourceTestResultId = @CanonicalId
            FROM dbo.OCV_RETEST_DATA audit
            INNER JOIN @BarcodeRows duplicate ON duplicate.TestResultId = audit.SourceTestResultId
            WHERE duplicate.TestResultId <> @CanonicalId;
            DELETE tr FROM dbo.TestResult tr INNER JOIN @BarcodeRows duplicate ON duplicate.TestResultId = tr.TestResultId
            WHERE duplicate.TestResultId <> @CanonicalId;
            UPDATE dbo.TestResult SET BatteryCode = @BatteryCode WHERE TestResultId = @CanonicalId;
            DECLARE @SummaryChannels dbo.TestResultInputType;
            DECLARE @SummaryTaskId bigint;
            SELECT @SummaryTaskId = TestTaskId FROM dbo.TestResult WHERE TestResultId = @SourceTestResultId;
            EXEC dbo.usp_UpsertCellOcvSummary
                @TestTaskId=@SummaryTaskId,
                @StageTestTime=@RetestTime,
                @Channels=@SummaryChannels,
                @TestResultId=@SourceTestResultId;
        END
        ELSE
        BEGIN
            -- Keep a valid audit source; a delayed deleted duplicate resolves to the current row.
            -- Measurement/context snapshot parameters remain the original caller values.
            IF @SourceTestResultId IS NOT NULL AND EXISTS
                (SELECT 1 FROM dbo.TestResult WHERE TestResultId = @SourceTestResultId)
                AND NOT EXISTS (SELECT 1 FROM @BarcodeRows WHERE TestResultId = @SourceTestResultId)
                RAISERROR(N'The audit source result belongs to another barcode.', 16, 1);
            IF @SourceTestResultId IS NULL OR NOT EXISTS
                (SELECT 1 FROM @BarcodeRows WHERE TestResultId = @SourceTestResultId)
                SET @SourceTestResultId = @CanonicalId;
            IF @SourceTestResultId IS NULL
                RAISERROR(N'No valid result exists for the NG retest audit barcode.', 16, 1);
        END

        INSERT dbo.OCV_RETEST_DATA
        (
            BatteryCode, TrayCode, ChannelNo, TrayPosition, SourceTestResultId,
            OriginalVoltage, OriginalResistance, OriginalResult,
            RetestVoltage, RetestResistance, RetestRawResistanceMilliohm,
            CalibrationValueMilliohm, RetestFinalResistanceMilliohm,
            CalibrationSnapshotFingerprint, CalibrationSnapshotSource,
            RetestResult, RetestTime,
            OverwriteOriginal, RetestOperationId, OperatorName, MeterPort
        )
        VALUES
        (
            @BatteryCode, @TrayCode, @ChannelNo, @TrayPosition, @SourceTestResultId,
            @OriginalVoltage, @OriginalResistance, @OriginalResult,
            @RetestVoltage, @RetestResistance, @RetestRawResistanceMilliohm,
            @CalibrationValueMilliohm, @RetestFinalResistanceMilliohm,
            @CalibrationSnapshotFingerprint, @CalibrationSnapshotSource,
            @RetestResult, @RetestTime,
            @OverwriteOriginal, @RetestOperationId, @OperatorName, @MeterPort
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage nvarchar(4000);
        DECLARE @ErrorSeverity int;
        DECLARE @ErrorState int;
        SELECT @ErrorMessage = ERROR_MESSAGE(),
               @ErrorSeverity = ERROR_SEVERITY(),
               @ErrorState = ERROR_STATE();
        RAISERROR(N'%s', @ErrorSeverity, @ErrorState, @ErrorMessage);
    END CATCH
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
