/*
  OCV_DATA baseline seed data for SQL Server 2008 R2.

  Use this after the OCV_DATA_SQLServer2008R2.sql schema/procedure script has
  completed successfully. It restores required baseline rows and extends the
  health check to verify the grade-field seed data. Safe to rerun.
*/
USE [OCV_DATA];
GO

ALTER PROCEDURE dbo.usp_CheckDatabaseHealth
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
           (N'ActiveCount=' + CONVERT(nvarchar(20), (SELECT COUNT(*) FROM dbo.OcvStage WHERE IsActive = 1)))
    UNION ALL
    SELECT N'GradeSelectableField',
           CONVERT(bit, CASE WHEN (SELECT COUNT(*) FROM dbo.GradeSelectableField WHERE IsActive = 1 AND FieldName IN (N'VoltageVolt', N'FinalResistanceMilliohm')) = 2 THEN 1 ELSE 0 END),
           (N'ActiveCount=' + CONVERT(nvarchar(20), (SELECT COUNT(*) FROM dbo.GradeSelectableField WHERE IsActive = 1 AND FieldName IN (N'VoltageVolt', N'FinalResistanceMilliohm'))))
    UNION ALL
    SELECT N'PendingUploads',
           CONVERT(bit, 1),
           (N'PendingOrFailed=' + CONVERT(nvarchar(20), (SELECT COUNT(*) FROM dbo.UploadOutbox WHERE Status IN (N'Pending', N'Failed'))))
    UNION ALL
    SELECT N'UnfinishedTasks',
           CONVERT(bit, CASE WHEN (SELECT COUNT(*) FROM dbo.TestTask WHERE Status IN (N'Created', N'Running')) < 100 THEN 1 ELSE 0 END),
           (N'CreatedOrRunning=' + CONVERT(nvarchar(20), (SELECT COUNT(*) FROM dbo.TestTask WHERE Status IN (N'Created', N'Running'))));
END;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'OCV_DATA'
BEGIN
    RAISERROR(N'This script must run in the OCV_DATA database.', 16, 1);
    RETURN;
END;

IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NULL
   OR OBJECT_ID(N'dbo.OcvStage', N'U') IS NULL
   OR OBJECT_ID(N'dbo.GradeSelectableField', N'U') IS NULL
BEGIN
    RAISERROR(N'Required OCV_DATA tables are missing. Run the SQL Server 2008 R2 schema script first.', 16, 1);
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.OcvStage WHERE StageCode = N'OCV0')
        UPDATE dbo.OcvStage
        SET LegacyStageNo = 1, LegacyColumnSuffix = N'A', DisplayName = N'OCV0',
            MesStageCode = N'OCV0', SortOrder = 10, IsActive = 1
        WHERE StageCode = N'OCV0';
    ELSE
        INSERT INTO dbo.OcvStage
            (StageCode, LegacyStageNo, LegacyColumnSuffix, DisplayName, MesStageCode, SortOrder, IsActive, CreatedAt)
        VALUES
            (N'OCV0', 1, N'A', N'OCV0', N'OCV0', 10, 1, GETDATE());

    IF EXISTS (SELECT 1 FROM dbo.OcvStage WHERE StageCode = N'OCV1')
        UPDATE dbo.OcvStage
        SET LegacyStageNo = 2, LegacyColumnSuffix = N'B', DisplayName = N'OCV1',
            MesStageCode = N'OCV1', SortOrder = 20, IsActive = 1
        WHERE StageCode = N'OCV1';
    ELSE
        INSERT INTO dbo.OcvStage
            (StageCode, LegacyStageNo, LegacyColumnSuffix, DisplayName, MesStageCode, SortOrder, IsActive, CreatedAt)
        VALUES
            (N'OCV1', 2, N'B', N'OCV1', N'OCV1', 20, 1, GETDATE());

    IF EXISTS (SELECT 1 FROM dbo.OcvStage WHERE StageCode = N'OCV2')
        UPDATE dbo.OcvStage
        SET LegacyStageNo = 3, LegacyColumnSuffix = N'C', DisplayName = N'OCV2',
            MesStageCode = N'OCV2', SortOrder = 30, IsActive = 1
        WHERE StageCode = N'OCV2';
    ELSE
        INSERT INTO dbo.OcvStage
            (StageCode, LegacyStageNo, LegacyColumnSuffix, DisplayName, MesStageCode, SortOrder, IsActive, CreatedAt)
        VALUES
            (N'OCV2', 3, N'C', N'OCV2', N'OCV2', 30, 1, GETDATE());

    IF EXISTS (SELECT 1 FROM dbo.GradeSelectableField WHERE FieldName = N'VoltageVolt')
        UPDATE dbo.GradeSelectableField
        SET DisplayName = N'Voltage', UnitName = N'V', IsActive = 1
        WHERE FieldName = N'VoltageVolt';
    ELSE
        INSERT INTO dbo.GradeSelectableField (FieldName, DisplayName, UnitName, IsActive)
        VALUES (N'VoltageVolt', N'Voltage', N'V', 1);

    IF EXISTS (SELECT 1 FROM dbo.GradeSelectableField WHERE FieldName = N'FinalResistanceMilliohm')
        UPDATE dbo.GradeSelectableField
        SET DisplayName = N'Resistance', UnitName = N'mOhm', IsActive = 1
        WHERE FieldName = N'FinalResistanceMilliohm';
    ELSE
        INSERT INTO dbo.GradeSelectableField (FieldName, DisplayName, UnitName, IsActive)
        VALUES (N'FinalResistanceMilliohm', N'Resistance', N'mOhm', 1);

    IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo = N'001')
        INSERT INTO dbo.SchemaVersion (VersionNo, ScriptName, AppliedAt, Remark)
        VALUES (N'001', N'OCV_DATA_SQLServer2008R2.sql', GETDATE(), N'Baseline schema installed.');

    IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo = N'002')
        INSERT INTO dbo.SchemaVersion (VersionNo, ScriptName, AppliedAt, Remark)
        VALUES (N'002', N'OCV_DATA_SQLServer2008R2.sql', GETDATE(), N'Core procedures installed.');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage nvarchar(4000);
    SET @ErrorMessage = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO

EXEC dbo.usp_CheckDatabaseHealth;
GO
