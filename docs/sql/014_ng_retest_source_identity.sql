/*
  NG retest source identity and idempotent save migration.
  Compatible with SQL Server 2008 R2 and newer.
  Run against an existing OCV_DATA database before deploying the matching app.
*/
USE OCV_DATA;
GO

IF OBJECT_ID(N'dbo.OCV_RETEST_DATA', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OCV_RETEST_DATA
    (
        Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_OCV_RETEST_DATA PRIMARY KEY,
        BatteryCode nvarchar(100) NOT NULL,
        TrayCode nvarchar(100) NULL,
        ChannelNo smallint NULL,
        TrayPosition nvarchar(20) NULL,
        SourceTestResultId bigint NULL,
        OriginalVoltage real NOT NULL,
        OriginalResistance real NOT NULL,
        OriginalResult nvarchar(10) NOT NULL,
        RetestVoltage real NOT NULL,
        RetestResistance real NOT NULL,
        RetestResult nvarchar(10) NOT NULL,
        RetestTime datetime2(7) NOT NULL,
        OverwriteOriginal bit NOT NULL,
        RetestOperationId uniqueidentifier NULL,
        OperatorName nvarchar(100) NULL,
        MeterPort nvarchar(20) NULL
    );
END
GO

IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'SourceTestResultId') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD SourceTestResultId bigint NULL;
GO

IF COL_LENGTH(N'dbo.OCV_RETEST_DATA', N'RetestOperationId') IS NULL
    ALTER TABLE dbo.OCV_RETEST_DATA ADD RetestOperationId uniqueidentifier NULL;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_OCV_RETEST_DATA_BatteryCode_Time'
      AND object_id = OBJECT_ID(N'dbo.OCV_RETEST_DATA')
)
    CREATE INDEX IX_OCV_RETEST_DATA_BatteryCode_Time
        ON dbo.OCV_RETEST_DATA(BatteryCode, RetestTime DESC);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_OCV_RETEST_DATA_RetestOperationId'
      AND object_id = OBJECT_ID(N'dbo.OCV_RETEST_DATA')
)
    CREATE UNIQUE INDEX UX_OCV_RETEST_DATA_RetestOperationId
        ON dbo.OCV_RETEST_DATA(RetestOperationId)
        WHERE RetestOperationId IS NOT NULL;
GO

IF OBJECT_ID(N'dbo.usp_GetNgRetestSourceByBatteryCode', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.usp_GetNgRetestSourceByBatteryCode AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE dbo.usp_GetNgRetestSourceByBatteryCode
    @BatteryCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        tr.TestResultId,
        COALESCE(tr.BatteryCode, tc.BatteryCode) AS BatteryCode,
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
    WHERE COALESCE(tr.BatteryCode, tc.BatteryCode) = @BatteryCode
      AND tr.IsNg = 1
      AND tt.TestType <> N'Debug'
    ORDER BY tr.CreatedAt DESC, tr.TestResultId DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_SaveNgRetest', N'P') IS NULL
    EXEC(N'CREATE PROCEDURE dbo.usp_SaveNgRetest AS SET NOCOUNT ON;');
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
    @RetestResult nvarchar(10),
    @RetestTime datetime2,
    @OverwriteOriginal bit,
    @OperatorName nvarchar(100),
    @MeterPort nvarchar(20),
    @SourceTestResultId bigint = NULL,
    @RetestOperationId uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

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

        IF @OverwriteOriginal = 1
        BEGIN
            IF @SourceTestResultId IS NULL
                RAISERROR(N'SourceTestResultId is required when overwriting the original result.', 16, 1);

            UPDATE tr
            SET VoltageVolt = @RetestVoltage,
                RawResistanceMilliohm = @RetestResistance,
                FinalResistanceMilliohm = @RetestResistance,
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
              AND COALESCE(tr.BatteryCode, tc.BatteryCode) = @BatteryCode
              AND t.TrayCode = @TrayCode
              AND tr.ChannelNo = @ChannelNo;

            IF @@ROWCOUNT <> 1
                RAISERROR(N'The queried NG source result changed or no longer matches the barcode, tray, and channel.', 16, 1);
        END

        INSERT dbo.OCV_RETEST_DATA
        (
            BatteryCode, TrayCode, ChannelNo, TrayPosition, SourceTestResultId,
            OriginalVoltage, OriginalResistance, OriginalResult,
            RetestVoltage, RetestResistance, RetestResult, RetestTime,
            OverwriteOriginal, RetestOperationId, OperatorName, MeterPort
        )
        VALUES
        (
            @BatteryCode, @TrayCode, @ChannelNo, @TrayPosition, @SourceTestResultId,
            @OriginalVoltage, @OriginalResistance, @OriginalResult,
            @RetestVoltage, @RetestResistance, @RetestResult, @RetestTime,
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
