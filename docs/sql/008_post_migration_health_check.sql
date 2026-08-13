/*
  OCV_DATA 迁移后健康检查脚本:R2-15
  目标:在迁移完成后,验证数据完整性、约束、视图、存储过程是否齐全,
        并报告关键表的行数,供运维快速确认迁移成功。

  运行顺序:
    1. 000_init_database.sql
    2. 001_create_ocv_data_schema.sql
    3. 002_create_core_procedures.sql
    4. 006_migrate_unsent_data_to_upload_outbox.sql
    5. 007_migrate_grade_rules.sql
    6. 本脚本 005_post_migration_health_check.sql
*/

USE OCV_DATA;
GO

SET NOCOUNT ON;

PRINT N'=== OCV_DATA 迁移后健康检查 ===';
PRINT N'执行时间: ' + CONVERT(nvarchar(30), sysdatetime(), 120);
PRINT N'';

-- 1. SchemaVersion 检查
PRINT N'[1] SchemaVersion 表:';
SELECT SchemaVersionId, VersionNo, ScriptName, AppliedAt
FROM dbo.SchemaVersion
ORDER BY SchemaVersionId;

DECLARE @ExpectedVersions TABLE (VersionNo nvarchar(10));
INSERT INTO @ExpectedVersions VALUES (N'001'), (N'002'), (N'006'), (N'007');

DECLARE @MissingCount int = (
    SELECT COUNT(*) FROM @ExpectedVersions ev
    WHERE NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion sv WHERE sv.VersionNo = ev.VersionNo)
);

IF @MissingCount > 0
BEGIN
    PRINT N'[1][WARN] 缺少 ' + CONVERT(nvarchar(10), @MissingCount) + N' 个 SchemaVersion 记录。';
END
ELSE
BEGIN
    PRINT N'[1][OK] SchemaVersion 记录齐全 (001/002/006/007)。';
END
PRINT N'';

-- 2. 必备存储过程检查
PRINT N'[2] 必备存储过程:';
DECLARE @RequiredProcs TABLE (ProcName nvarchar(100));
INSERT INTO @RequiredProcs VALUES
    (N'dbo.usp_RegisterTray'),
    (N'dbo.usp_SaveTrayCells'),
    (N'dbo.usp_GetTrayCellsByTrayCode'),
    (N'dbo.usp_CreateTestTask'),
    (N'dbo.usp_SaveTestResults'),
    (N'dbo.usp_CompleteTestTask'),
    (N'dbo.usp_GetTrayLatestStageResults'),
    (N'dbo.usp_GetActiveCalibrationValues'),
    (N'dbo.usp_SaveChannelCalibration'),
    (N'dbo.usp_SaveDeviceCommLog'),
    (N'dbo.usp_SaveAlarmEvent'),
    (N'dbo.usp_SaveAuditLog'),
    (N'dbo.usp_GetEntityAuditTrail'),
    (N'dbo.usp_GetSchemaVersion'),
    (N'dbo.usp_CheckDatabaseHealth'),
    (N'dbo.usp_EnqueueUpload'),
    (N'dbo.usp_GetPendingUploads'),
    (N'dbo.usp_MarkUploadSent'),
    (N'dbo.usp_MarkUploadFailed'),
    (N'dbo.usp_SaveMesUploadLog');

SELECT
    rp.ProcName,
    CASE WHEN OBJECT_ID(rp.ProcName, N'P') IS NOT NULL THEN N'OK' ELSE N'MISSING' END AS Status
FROM @RequiredProcs rp
ORDER BY rp.ProcName;
PRINT N'';

-- 3. 关键表的行数统计
PRINT N'[3] 关键表行数:';
SELECT
    N'Tray' AS TableName, COUNT(*) AS RowCount FROM dbo.Tray
UNION ALL SELECT N'TrayCell', COUNT(*) FROM dbo.TrayCell
UNION ALL SELECT N'TestTask', COUNT(*) FROM dbo.TestTask
UNION ALL SELECT N'TestResult', COUNT(*) FROM dbo.TestResult
UNION ALL SELECT N'RetestTask', COUNT(*) FROM dbo.RetestTask
UNION ALL SELECT N'ChannelCalibration', COUNT(*) FROM dbo.ChannelCalibration
UNION ALL SELECT N'GradeScheme', COUNT(*) FROM dbo.GradeScheme
UNION ALL SELECT N'GradeRule', COUNT(*) FROM dbo.GradeRule
UNION ALL SELECT N'GradeSelectableField', COUNT(*) FROM dbo.GradeSelectableField
UNION ALL SELECT N'UploadOutbox (Total)', COUNT(*) FROM dbo.UploadOutbox
UNION ALL SELECT N'UploadOutbox (Pending)', COUNT(*) FROM dbo.UploadOutbox WHERE Status IN (N'Pending', N'Failed')
UNION ALL SELECT N'UploadOutbox (Uploading)', COUNT(*) FROM dbo.UploadOutbox WHERE Status = N'Uploading'
UNION ALL SELECT N'UploadOutbox (Sent)', COUNT(*) FROM dbo.UploadOutbox WHERE Status = N'Sent'
UNION ALL SELECT N'MesUploadLog', COUNT(*) FROM dbo.MesUploadLog
UNION ALL SELECT N'AuditLog', COUNT(*) FROM dbo.AuditLog
UNION ALL SELECT N'OperatorLog', COUNT(*) FROM dbo.OperatorLog
UNION ALL SELECT N'AlarmEvent', COUNT(*) FROM dbo.AlarmEvent
UNION ALL SELECT N'EnergySnapshot', COUNT(*) FROM dbo.EnergySnapshot
ORDER BY TableName;
PRINT N'';

-- 4. OcvStage 种子数据
PRINT N'[4] OcvStage 种子数据:';
SELECT StageCode, LegacyStageNo, LegacyColumnSuffix, DisplayName, IsActive
FROM dbo.OcvStage
ORDER BY SortOrder;
PRINT N'';

-- 5. 迁移标记数据(由 usp_MigrateLegacyGradeRules 写入的 IdempotencyKey)
PRINT N'[5] 迁移标识 (Migration Source):';
SELECT
    UploadType,
    SourceSystem,
    COUNT(*) AS MigratedRows
FROM dbo.UploadOutbox
WHERE IdempotencyKey LIKE N'Migrated:%'
GROUP BY UploadType, SourceSystem
ORDER BY UploadType, SourceSystem;
PRINT N'';

-- 6. 索引检查
PRINT N'[6] 关键索引:';
SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE OBJECT_NAME(i.object_id) IN
    (N'UploadOutbox', N'TestResult', N'TestTask', N'GradeRule', N'GradeScheme')
    AND i.is_hypothetical = 0
    AND i.is_primary_key = 0
ORDER BY TableName, IndexName;
PRINT N'';

-- 7. 视图检查
PRINT N'[7] 关键视图:';
SELECT
    v.name AS ViewName,
    'OK' AS Status
FROM sys.views v
WHERE v.schema_id = SCHEMA_ID(N'dbo')
ORDER BY v.name;
PRINT N'';

-- 8. 综合健康检查(直接调用存储过程)
PRINT N'[8] usp_CheckDatabaseHealth 综合检查:';
DECLARE @HealthResult TABLE (
    CheckName nvarchar(100),
    IsHealthy bit,
    Detail nvarchar(500)
);
INSERT INTO @HealthResult
EXEC dbo.usp_CheckDatabaseHealth;

SELECT * FROM @HealthResult;
PRINT N'';

PRINT N'=== OCV_DATA 健康检查完成 ===';
PRINT N'如果 [8] 中所有 CheckName 的 IsHealthy = 1,迁移成功。';
PRINT N'否则请根据 Detail 列检查并修复。';
GO