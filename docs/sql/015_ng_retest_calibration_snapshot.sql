USE OCV_DATA;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'RetestRawResistanceMilliohm') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD RetestRawResistanceMilliohm decimal(18,6) NULL;
GO
IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'CalibrationValueMilliohm') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD CalibrationValueMilliohm decimal(18,6) NULL;
GO
IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'RetestFinalResistanceMilliohm') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD RetestFinalResistanceMilliohm decimal(18,6) NULL;
GO
IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'CalibrationSnapshotFingerprint') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD CalibrationSnapshotFingerprint char(64) NULL;
GO
IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'CalibrationSnapshotSource') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD CalibrationSnapshotSource nvarchar(200) NULL;
GO

UPDATE dbo.OCV_RETEST_DATA
SET RetestRawResistanceMilliohm = COALESCE(RetestRawResistanceMilliohm, CONVERT(decimal(18,6), RetestResistance)),
    CalibrationValueMilliohm = COALESCE(CalibrationValueMilliohm, CONVERT(decimal(18,6), 0)),
    RetestFinalResistanceMilliohm = COALESCE(RetestFinalResistanceMilliohm, CONVERT(decimal(18,6), RetestResistance))
WHERE RetestRawResistanceMilliohm IS NULL
   OR CalibrationValueMilliohm IS NULL
   OR RetestFinalResistanceMilliohm IS NULL;
GO

ALTER TABLE dbo.OCV_RETEST_DATA ALTER COLUMN RetestRawResistanceMilliohm decimal(18,6) NOT NULL;
ALTER TABLE dbo.OCV_RETEST_DATA ALTER COLUMN CalibrationValueMilliohm decimal(18,6) NOT NULL;
ALTER TABLE dbo.OCV_RETEST_DATA ALTER COLUMN RetestFinalResistanceMilliohm decimal(18,6) NOT NULL;
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
