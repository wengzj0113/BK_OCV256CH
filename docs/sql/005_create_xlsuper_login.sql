/*
  OCV_DATA 部署脚本 005
  用途:创建 SQL 登录 xlsuper/123,映射到 OCV_DATA 库并授予 db_owner
        (R2-15 约定 cwsuper 或同等权限账号需要 db_owner 以便后续 001/002 脚本建表/建过程)。

  运行位置:在 SQL Server 主机上用 SSMS / sqlcmd 以 sysadmin 身份执行。
  前置条件:
    - 001/002/003 已成功执行 (OCV_DATA 库已存在,核心存储过程已就绪);
    - 服务器已启用"混合身份验证"(
        SSMS → 服务器属性 → 安全性 → SQL Server 和 Windows 身份验证模式,
        然后重启 SQL Server 服务)。

  后续动作:
    在上位机登录窗 (frmInit) 把 Initsetting.strUserID 改为 xlsuper,strpwd 改为 123,
    或直接将 004 诊断脚本中 @LoginName 改为 'xlsuper' 后重跑一次验证。

  ⚠ 安全提示:密码 '123' 仅供本地测试环境使用,生产环境务必改强密码。
*/

SET NOCOUNT ON;

DECLARE @LoginName sysname = N'xlsuper';
DECLARE @Password   nvarchar(256) = N'123';
DECLARE @Database   sysname = N'OCV_DATA';

PRINT N'========================================';
PRINT N' 创建 OCV_DATA 登录: ' + @LoginName;
PRINT N' 目标数据库: ' + @Database;
PRINT N' 服务器: ' + CAST(SERVERPROPERTY('ServerName') AS nvarchar(256));
PRINT N'========================================';


-- 0. 体检
IF DB_ID(@Database) IS NULL
BEGIN
    PRINT N'❌ ' + @Database + N' 数据库不存在,请先执行 001_create_ocv_data_schema.sql';
    RETURN;
END;

IF SERVERPROPERTY('IsIntegratedSecurityOnly') = 1
BEGIN
    PRINT N'❌ SQL Server 当前为 Windows Only 身份验证,执行后 cwsuper/xlsuper 类 SQL 登录仍无法登录。';
    PRINT N'   请先在 SSMS → 服务器属性 → 安全性 改为 混合身份验证,并重启 SQL Server 服务。';
    RETURN;
END;


-- 1. 创建服务器登录 (已存在则跳过)
IF SUSER_ID(@LoginName) IS NULL
BEGIN
    PRINT N'[1] 创建服务器登录 ' + @LoginName;
    DECLARE @Sql nvarchar(max) =
        N'CREATE LOGIN ' + QUOTENAME(@LoginName) +
        N' WITH PASSWORD = ' + QUOTENAME(@Password, N'''') +
        N', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF, DEFAULT_DATABASE = ' + QUOTENAME(@Database) + N';';
    EXEC sp_executesql @Sql;
    PRINT N'  ✔ 已创建';
END
ELSE
BEGIN
    PRINT N'[1] 服务器登录 ' + @LoginName + N' 已存在,校验密码是否符合预期';
    -- 同步密码 (CHECK_POLICY=OFF 时直接 ALTER 即可,无需 UNLOCK)
    DECLARE @SqlAlter nvarchar(max) =
        N'ALTER LOGIN ' + QUOTENAME(@LoginName) +
        N' WITH PASSWORD = ' + QUOTENAME(@Password, N'''') + N', CHECK_POLICY = OFF;';
    EXEC sp_executesql @SqlAlter;
    PRINT N'  ✔ 密码已同步为本次指定值';
END;

-- 解锁 + 启用
IF LOGINPROPERTY(@LoginName, 'IsLocked') = 1
BEGIN
    PRINT N'[2] 登录被锁定,解锁';
    DECLARE @SqlUnlock nvarchar(max) =
        N'ALTER LOGIN ' + QUOTENAME(@LoginName) + N' WITH PASSWORD = ' + QUOTENAME(@Password, N'''') + N' UNLOCK;';
    EXEC sp_executesql @SqlUnlock;
END;

IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @LoginName AND is_disabled = 1)
BEGIN
    PRINT N'[2] 登录被禁用,启用';
    DECLARE @SqlEnable nvarchar(max) = N'ALTER LOGIN ' + QUOTENAME(@LoginName) + N' ENABLE;';
    EXEC sp_executesql @SqlEnable;
END;


-- 3. 在 OCV_DATA 库创建 USER (已存在则复用)
USE OCV_DATA;
IF USER_ID(@LoginName) IS NULL
BEGIN
    PRINT N'[3] 创建 OCV_DATA 数据库用户 ' + @LoginName;
    DECLARE @SqlUser nvarchar(max) =
        N'CREATE USER ' + QUOTENAME(@LoginName) + N' FOR LOGIN ' + QUOTENAME(@LoginName) + N';';
    EXEC sp_executesql @SqlUser;
    PRINT N'  ✔ 已创建';
END
ELSE
BEGIN
    PRINT N'[3] OCV_DATA 用户 ' + @LoginName + N' 已存在,跳过';
END;


-- 4. 授予 db_owner 角色 (R2-15 必需)
IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    INNER JOIN sys.database_principals rm ON rm.principal_id = drm.role_principal_id
    INNER JOIN sys.database_principals mp ON mp.principal_id = drm.member_principal_id
    WHERE rm.name = N'db_owner' AND mp.name = @LoginName
)
BEGIN
    PRINT N'[4] 授予 db_owner 角色';
    DECLARE @SqlGrant nvarchar(max) =
        N'ALTER ROLE ' + QUOTENAME(N'db_owner') + N' ADD MEMBER ' + QUOTENAME(@LoginName) + N';';
    EXEC sp_executesql @SqlGrant;
    PRINT N'  ✔ 已授予';
END
ELSE
BEGIN
    PRINT N'[4] 已是 db_owner,跳过';
END;


-- 5. 验证
PRINT N'';
PRINT N'========================================';
PRINT N' 验证结果';
PRINT N'========================================';

SELECT
    sp.name AS LoginName,
    sp.type_desc AS LoginType,
    sp.is_disabled AS IsDisabled,
    LOGINPROPERTY(@LoginName, 'IsLocked') AS IsLocked,
    LOGINPROPERTY(@LoginName, 'BadPasswordCount') AS BadPasswordCount,
    su.name AS DatabaseUser,
    (
        SELECT STUFF(
            (SELECT N', ' + r.name
             FROM sys.database_role_members drm
             INNER JOIN sys.database_principals r ON r.principal_id = drm.role_principal_id
             WHERE drm.member_principal_id = mp.principal_id
             FOR XML PATH(N''), TYPE).value('.', 'nvarchar(max)'),
            1, 2, '')
    ) AS RoleNames
FROM sys.server_principals sp
LEFT JOIN sys.database_principals mp ON mp.name = sp.name
LEFT JOIN sys.sysusers su ON su.name = sp.name
WHERE sp.name = @LoginName;

-- 健康检查存储过程能否被调用
PRINT N'';
PRINT N'[5] 跑一次健康检查 (usp_CheckDatabaseHealth)';
BEGIN TRY
    DECLARE @HasHealth bit = 0;
    IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.usp_CheckDatabaseHealth') AND type = N'P')
    BEGIN
        SET @HasHealth = 1;
        EXEC dbo.usp_CheckDatabaseHealth;
    END
    ELSE
    BEGIN
        PRINT N'  ❌ dbo.usp_CheckDatabaseHealth 不存在,需先执行 002_create_core_procedures.sql';
    END;
END TRY
BEGIN CATCH
    PRINT N'  ❌ usp_CheckDatabaseHealth 执行失败: ' + ERROR_MESSAGE();
END CATCH;

PRINT N'';
PRINT N'完成。下一步:在 frmInit 登录窗把 strUserID=xlsuper, strpwd=123 重启 XL_OCV_256CH.exe。';
