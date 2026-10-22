/* SQL Server 2008 R2+ additive upgrade. Run after 014 and 015.
   Historical duplicates are retained until that barcode is saved. */
USE OCV_DATA;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TestResult') AND name = N'UX_TestResult_Task_Channel')
    DROP INDEX UX_TestResult_Task_Channel ON dbo.TestResult;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TestResult') AND name = N'IX_TestResult_Task_Channel')
    CREATE INDEX IX_TestResult_Task_Channel ON dbo.TestResult(TestTaskId, ChannelNo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TestResult') AND name = N'IX_TestResult_BatteryCode')
    CREATE INDEX IX_TestResult_BatteryCode ON dbo.TestResult(BatteryCode);
GO
ALTER PROCEDURE dbo.usp_SaveTestResults
    @TestTaskId bigint,
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
ALTER PROCEDURE dbo.usp_SaveNgRetest
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

ALTER PROCEDURE dbo.usp_GetNgRetestSourceByBatteryCode
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
