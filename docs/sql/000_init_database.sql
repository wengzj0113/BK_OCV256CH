/*
  OCV_DATA 初始化脚本:R2-15 部署前置
  目标:创建 OCV_DATA 数据库,设置兼容级别、恢复模式、初始配置;
        在执行 001 之前作为最优先步骤运行。

  运行权限:需要 sysadmin 或 CREATE DATABASE 权限
  SQL Server 兼容:2012+
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TargetCompatLevel smallint = 150;  -- SQL Server 2019 兼容级别
DECLARE @RecoveryModel nvarchar(60) = N'SIMPLE';  -- 测试环境用 SIMPLE,生产建议 FULL + 定期日志备份

-- 1. 预检:不能重复执行(除非明确要 Drop 重建)
IF DB_ID(N'OCV_DATA') IS NOT NULL
BEGIN
    PRINT N'[000] OCV_DATA 数据库已存在,跳过 CREATE DATABASE。';
    PRINT N'[000] 如需重建,请先 DROP DATABASE [OCV_DATA] (注意会丢失所有数据)。';
END
ELSE
BEGIN
    PRINT N'[000] 创建 OCV_DATA 数据库 (CompatLevel=' + CONVERT(nvarchar(10), @TargetCompatLevel) + ') ...';
    DECLARE @Sql nvarchar(max) =
        N'CREATE DATABASE OCV_DATA' +
        N' COLLATE Chinese_PRC_CI_AS' +
        N' CONTAINMENT = NONE;';
    EXEC sp_executesql @Sql;
    PRINT N'[000] OCV_DATA 数据库创建成功。';
END
GO

-- 2. 设置兼容级别和恢复模式
IF DB_ID(N'OCV_DATA') IS NOT NULL
BEGIN
    DECLARE @CurrentCompat tinyint = CONVERT(tinyint, DATABASEPROPERTYEX(N'OCV_DATA', N'COLLATION'));
    DECLARE @CurrentRecovery nvarchar(60);
    SET @CurrentRecovery = CONVERT(nvarchar(60), DATABASEPROPERTYEX(N'OCV_DATA', N'RECOVERY'));

    IF @CurrentCompat <> 150
    BEGIN
        EXEC sp_executesql N'ALTER DATABASE OCV_DATA SET COMPATIBILITY_LEVEL = 150;';
        PRINT N'[000] 兼容级别已设置为 150 (SQL Server 2019)。';
    END

    IF @CurrentRecovery <> N'SIMPLE'
    BEGIN
        ALTER DATABASE OCV_DATA SET RECOVERY SIMPLE;
        PRINT N'[000] 恢复模式已设置为 SIMPLE。';
    END
END
GO

-- 3. 启用必要的数据库配置(查询存储/变更跟踪等)
IF DB_ID(N'OCV_DATA') IS NOT NULL
BEGIN
    ALTER DATABASE OCV_DATA SET QUERY_STORE = ON;
    ALTER DATABASE OCV_DATA SET QUERY_STORE (
        OPERATION_MODE = READ_WRITE,
        CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
        DATA_FLUSH_INTERVAL_SECONDS = 900
    );
    PRINT N'[000] Query Store 已启用。';
END
GO

-- 4. 预检结果报告
IF DB_ID(N'OCV_DATA') IS NOT NULL
BEGIN
    SELECT
        N'OCV_DATA' AS DatabaseName,
        DATABASEPROPERTYEX(N'OCV_DATA', N'COLLATION') AS Collation,
        DATABASEPROPERTYEX(N'OCV_DATA', N'RECOVERY') AS RecoveryModel,
        DATABASEPROPERTYEX(N'OCV_DATA', N'COMPATIBILITY_LEVEL') AS CompatLevel,
        DATABASEPROPERTYEX(N'OCV_DATA', N'IS_AUTO_CREATE_STATS_ON') AS AutoCreateStats,
        DATABASEPROPERTYEX(N'OCV_DATA', N'IS_AUTO_UPDATE_STATS_ON') AS AutoUpdateStats
    WHERE DB_ID(N'OCV_DATA') IS NOT NULL;
    PRINT N'[000] OCV_DATA 初始化完成,可以运行 001_create_ocv_data_schema.sql。';
END
ELSE
BEGIN
    PRINT N'[000][FATAL] OCV_DATA 数据库未创建,后续脚本无法继续。';
    THROW 50000, N'OCV_DATA database creation failed.', 1;
END
GO