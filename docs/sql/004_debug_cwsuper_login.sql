/*
  OCV_DATA 诊断脚本 004
  用途:排查上位机启动时 OCV_DATA 健康检查报 "用户 'cwsuper' 登录失败"。
  触发条件:XL_OCV_256CH.exe 启动后弹出
           "OCV_DATA database health check failed: 用户 'cwsuper' 登录失败"。

  运行位置:在 SQL Server 主机上用 SSMS / sqlcmd 以 sysadmin 身份执行。
  运行顺序:无前置依赖,可与 001/002/003 独立使用。

  兼容 SQL Server 2012+(STRING_AGG 在 2017+ 有低版本回退,见 [3]/[5])。

  脚本内不做任何变更,仅做只读诊断 + 给出排查建议。
*/

SET NOCOUNT ON;

DECLARE @LoginName sysname = N'cwsuper';
DECLARE @CurrentLoginMode int;
DECLARE @MajorVersion int = CAST(SERVERPROPERTY('ProductMajorVersion') AS int);

PRINT N'========================================';
PRINT N' OCV_DATA 登录诊断  cwsuper';
PRINT N' 服务器:' + CAST(SERVERPROPERTY('ServerName') AS nvarchar(256));
PRINT N' 诊断时间:' + CONVERT(nvarchar(30), GETDATE(), 120);
PRINT N'========================================';


-- 1. 服务器身份验证模式 (Mixed / Windows Only)
PRINT N'';
PRINT N'[1] 服务器身份验证模式';
PRINT N'----------------------------------';

-- LoginMode:1 = Windows Only, 2 = Mixed
BEGIN TRY
    EXEC master.dbo.xp_instance_regread
        N'HKEY_LOCAL_MACHINE',
        N'SOFTWARE\Microsoft\MSSQLServer\MSSQLServer',
        N'LoginMode',
        @CurrentLoginMode OUTPUT;

    IF @CurrentLoginMode IS NULL
        PRINT N'  当前:LoginMode=<NULL> (注册表读不到,可能是命名实例或容器)';
    ELSE IF @CurrentLoginMode = 1
        PRINT N'  当前:Windows 身份验证模式 (LoginMode=1)';
    ELSE IF @CurrentLoginMode = 2
        PRINT N'  当前:混合身份验证模式 (LoginMode=2)';
    ELSE
        PRINT N'  当前:LoginMode=' + CAST(@CurrentLoginMode AS nvarchar(16));

    IF @CurrentLoginMode = 1
        PRINT N'  结论:cwsuper (SQL 登录) 必无法登录,需改为混合模式。';
    ELSE IF @CurrentLoginMode = 2
        PRINT N'  结论:模式支持 SQL 登录,问题在登录或密码。';
    ELSE
        PRINT N'  结论:无法读出 LoginMode,请手工在 SSMS → 服务器属性 → 安全性确认。';
END TRY
BEGIN CATCH
    PRINT N'  xp_instance_regread 失败 (可能在容器或受限环境):' + ERROR_MESSAGE();
END CATCH;


-- 2. 登录是否存在 / 是否被禁用
PRINT N'';
PRINT N'[2] 登录状态';
PRINT N'----------------------------------';

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginName)
BEGIN
    PRINT N'  ❌ sys.server_principals 中找不到 ' + @LoginName;
    PRINT N'     → 必须先创建 SQL 登录:';
    PRINT N'         CREATE LOGIN ' + @LoginName + ' WITH PASSWORD = ''<Initsetting.strpwd>'';';
END
ELSE
BEGIN
    SELECT
        sp.name                                AS LoginName,
        sp.type_desc                           AS LoginType,
        sp.is_disabled                         AS IsDisabled,
        sp.default_database_name               AS DefaultDatabase,
        sp.default_language_name               AS DefaultLanguage,
        (SELECT COUNT(*) FROM sys.server_permissions p
         WHERE p.grantee_principal_id = sp.principal_id
           AND p.permission_name = N'CONNECT SQL') AS HasConnectSql,
        sl.password_hash                       AS PasswordHash,
        LOGINPROPERTY(@LoginName, 'PasswordLastSetTime') AS PasswordLastSet,
        LOGINPROPERTY(@LoginName, 'IsLocked')  AS IsLocked,
        LOGINPROPERTY(@LoginName, 'IsExpired') AS IsExpired,
        LOGINPROPERTY(@LoginName, 'BadPasswordCount') AS BadPasswordCount,
        LOGINPROPERTY(@LoginName, 'BadPasswordTime') AS BadPasswordTime
    FROM sys.server_principals sp
    LEFT JOIN sys.sql_logins sl ON sl.principal_id = sp.principal_id
    WHERE sp.name = @LoginName;

    IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginName AND is_disabled = 1)
        PRINT N'  ❌ 登录被禁用,需执行:ALTER LOGIN ' + @LoginName + ' ENABLE;';

    IF LOGINPROPERTY(@LoginName, 'IsLocked') = 1
    BEGIN
        PRINT N'  ⚠ 登录被锁定 (失败次数过多),需执行:';
        PRINT N'         ALTER LOGIN ' + @LoginName + ' WITH PASSWORD = ''<new-pwd>'' UNLOCK;';
    END

    IF LOGINPROPERTY(@LoginName, 'BadPasswordCount') > 0
    BEGIN
        PRINT N'  ⚠ BadPasswordCount=' + CAST(LOGINPROPERTY(@LoginName, 'BadPasswordCount') AS nvarchar(10))
            + N' (' + ISNULL(CONVERT(nvarchar(30), LOGINPROPERTY(@LoginName, 'BadPasswordTime'), 120), N'?') + N')';
    END
END


-- 3. OCV_DATA 库的 USER 映射
PRINT N'';
PRINT N'[3] OCV_DATA 中的 USER 映射';
PRINT N'----------------------------------';

IF DB_ID('OCV_DATA') IS NULL
BEGIN
    PRINT N'  ❌ OCV_DATA 数据库不存在。请先执行 001_create_ocv_data_schema.sql。';
END
ELSE
BEGIN
    USE OCV_DATA;

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @LoginName)
    BEGIN
        PRINT N'  ❌ OCV_DATA 中没有 USER ' + @LoginName + N',需要:';
        PRINT N'         USE OCV_DATA; CREATE USER ' + @LoginName + ' FOR LOGIN ' + @LoginName + N';';
    END
    ELSE
    BEGIN
        SELECT
            dp.name         AS UserName,
            dp.type_desc    AS UserType,
            dp.authentication_type_desc AS AuthType,
            (
                SELECT
                    CASE WHEN @MajorVersion >= 14
                         THEN STRING_AGG(rm.name, N', ')
                         ELSE (SELECT STUFF(
                                  (SELECT N', ' + r2.name
                                   FROM sys.database_role_members drm2
                                   INNER JOIN sys.database_principals r2 ON r2.principal_id = drm2.role_principal_id
                                   WHERE drm2.member_principal_id = dp.principal_id
                                   FOR XML PATH(N''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''))
                    END
                FROM sys.database_role_members drm
                INNER JOIN sys.database_principals rm ON rm.principal_id = drm.role_principal_id
                WHERE drm.member_principal_id = dp.principal_id
            ) AS RoleNames,
            'CONNECT' AS HasConnectRight         -- 默认就有 CONNECT 权限
        FROM sys.database_principals dp
        WHERE dp.name = @LoginName;

        -- 检查是否在 db_owner 角色(R2-15 约定 cwsuper 需拥有建表/建过程权限)
        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_role_members drm
            INNER JOIN sys.database_principals rm ON rm.principal_id = drm.role_principal_id
            INNER JOIN sys.database_principals mp ON mp.principal_id = drm.member_principal_id
            WHERE rm.name = N'db_owner' AND mp.name = @LoginName
        )
        BEGIN
            PRINT N'  ⚠ cwsuper 不在 db_owner 角色中,后续执行 001/002 脚本可能失败。';
        END
    END

    -- 关键存储过程是否就绪
    PRINT N'';
    PRINT N'[4] 关键存储过程是否存在';
    PRINT N'----------------------------------';
    SELECT
        ROUTINE_NAME AS ProcedureName,
        CASE WHEN LEFT(ROUTINE_DEFINITION, 1) = N' ' THEN 'Exists' ELSE 'Exists' END AS Status
    FROM INFORMATION_SCHEMA.ROUTINES
    WHERE ROUTINE_SCHEMA = N'dbo'
      AND ROUTINE_NAME IN (
        N'usp_CheckDatabaseHealth',
        N'usp_RegisterTray',
        N'usp_SaveTrayCells',
        N'usp_GetTrayCellsByTrayCode',
        N'usp_CreateTestTask',
        N'usp_SaveTestResults',
        N'usp_CompleteTestTask',
        N'usp_GetTrayLatestStageResults',
        N'usp_EnqueueUpload',
        N'usp_GetPendingUploads',
        N'usp_MarkUploadSent',
        N'usp_MarkUploadFailed',
        N'usp_SaveMesUploadLog',
        N'usp_GetActiveCalibrationValues',
        N'usp_GetGradeSchemes',
        N'usp_GetGradeRules',
        N'usp_GetGradeCodes',
        N'usp_GetGradeSelectableFields',
        N'usp_SaveChannelCalibration',
        N'usp_SaveDeviceCommLog'
      );

    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES
                   WHERE ROUTINE_SCHEMA = N'dbo' AND ROUTINE_NAME = N'usp_CheckDatabaseHealth')
    BEGIN
        PRINT N'  ❌ usp_CheckDatabaseHealth 缺失,需先执行 002_create_core_procedures.sql';
    END
END;


-- 5. 应用实际拉起 SQL 连接时的网络 / 端口测试
PRINT N'';
PRINT N'[5] 端口与端点 (供应用侧排查)';
PRINT N'----------------------------------';

SELECT
    e.name         AS EndpointName,
    e.protocol_desc,
    e.state_desc,
    ISNULL(tcp.port, 0) AS Port,
    ISNULL(allowed.AllowedLogins, N'<unrestricted>') AS AllowedLogins
FROM sys.endpoints e
LEFT JOIN sys.tcp_endpoints tcp ON tcp.endpoint_id = e.endpoint_id
OUTER APPLY (
    SELECT
        CASE WHEN @MajorVersion >= 14
             THEN STRING_AGG(l.name, N', ')
             ELSE (SELECT STUFF(
                      (SELECT N', ' + l2.name
                       FROM sys.server_permissions p2
                       INNER JOIN sys.server_principals l2 ON l2.principal_id = p2.grantee_principal_id
                       WHERE p2.major_id = e.endpoint_id AND p2.permission_name = N'CONNECT'
                       FOR XML PATH(N''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''))
        END AS AllowedLogins
    FROM sys.server_permissions p
    INNER JOIN sys.server_principals l ON l.principal_id = p.grantee_principal_id
    WHERE p.major_id = e.endpoint_id AND p.permission_name = N'CONNECT'
) allowed
WHERE e.type = N'TSQL';

PRINT N'';
PRINT N'  默认 TSQL 端口:1433 (TCP),如 Initsetting.strDatabase 配的是实例名 SQLEXPRESS:';
PRINT N'  应用必须用 SqlConnectionStringBuilder 拼成 server=<host>\SQLEXPRESS,';
PRINT N'  或显式在连接串中加 port=...。';


-- 6. 建议修复清单
PRINT N'';
PRINT N'========================================';
PRINT N' 排查建议 (按出现概率从高到低)';
PRINT N'========================================';
PRINT N' A. 混合模式未启用 → 服务端 SSMS → 服务器属性 → 安全性 → 选 SQL Server 和 Windows 身份验证,重启 SQL Server;';
PRINT N' B. 登录不存在 → 检查旧库 CWSQL 中是否原本就是用 cwsuper;若仅迁库未建登录,需 CREATE LOGIN ...';
PRINT N' C. 登录被禁用 → ALTER LOGIN cwsuper ENABLE;';
PRINT N' D. 密码不一致 → 在 SSMS 用 ALTER LOGIN 同步一次密码,或上位机登录窗 (frmInit) 重输;';
PRINT N' E. 登录被锁定 → ALTER LOGIN cwsuper WITH PASSWORD = ''<pwd>'' UNLOCK;';
PRINT N' F. 应用连接到错误端口 / 实例 → 核对 Initsetting.strDatabase 是 Host 还是 Host\InstanceName;';
PRINT N' G. 数据库镜像 / AG 读副本只读 → 应连主端点。';
