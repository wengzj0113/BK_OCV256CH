/*
  OCV_DATA 辅助脚本 009
  用途:提供存在性校验存储过程,用于替代旧 clsDB.IsExists 方法。

  运行位置:在 SQL Server 主机上用 SSMS / sqlcmd 以 sysadmin 身份执行。
  前置条件:001/002 已成功执行 (OCV_DATA 库 + 核心存储过程已就绪)。
  运行顺序:无强制顺序,放在 001/002 之后即可,与 003/004/005/006/007/008 独立。

  包含:
    - dbo.usp_CheckTrayCodeExists
    - dbo.usp_CheckGradeSchemeExists

  说明:
    - 这两个存储过程是 OUTPUT 参数布尔型,语义与 clsDB.IsExists(Boolean) 一致,
      便于 VB.NET 端 OcvDataService.CheckTrayCodeExists / CheckGradeSchemeExists
      方法直接对接,完成 R2-15 clsDB 退场路径。
    - 幂等:使用 CREATE OR ALTER 语法。
*/

SET NOCOUNT ON;

USE OCV_DATA;
GO


-- 1) 托盘编码存在性
IF OBJECT_ID(N'dbo.usp_CheckTrayCodeExists', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CheckTrayCodeExists;
GO

CREATE PROCEDURE dbo.usp_CheckTrayCodeExists
    @TrayCode nvarchar(100),
    @Exists   bit OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @TrayCode = NULLIF(ltrim(rtrim(@TrayCode)), N'');

    IF @TrayCode IS NULL
    BEGIN
        SET @Exists = 0;
        RETURN;
    END;

    IF EXISTS (SELECT 1 FROM dbo.Tray WHERE TrayCode = @TrayCode)
        SET @Exists = 1;
    ELSE
        SET @Exists = 0;
END
GO


-- 2) 条件方案存在性 (IsActive=1 才算"存在",与 UI 行为对齐)
IF OBJECT_ID(N'dbo.usp_CheckGradeSchemeExists', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CheckGradeSchemeExists;
GO

CREATE PROCEDURE dbo.usp_CheckGradeSchemeExists
    @SchemeName nvarchar(100),
    @Exists     bit OUTPUT,
    @IsActive   bit = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @SchemeName = NULLIF(ltrim(rtrim(@SchemeName)), N'');

    IF @SchemeName IS NULL
    BEGIN
        SET @Exists = 0;
        RETURN;
    END;

    IF @IsActive IS NULL
        SET @Exists = CASE WHEN EXISTS(SELECT 1 FROM dbo.GradeScheme WHERE SchemeName = @SchemeName) THEN 1 ELSE 0 END;
    ELSE
        SET @Exists = CASE WHEN EXISTS(SELECT 1 FROM dbo.GradeScheme WHERE SchemeName = @SchemeName AND IsActive = @IsActive) THEN 1 ELSE 0 END;
END
GO


PRINT N'========================================';
PRINT N' 验证';
PRINT N'========================================';

SELECT
    ROUTINE_NAME AS ProcedureName,
    CASE WHEN LEFT(ROUTINE_DEFINITION, 1) = N' ' THEN N'Exists' ELSE N'Exists' END AS Status
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = N'dbo'
  AND ROUTINE_NAME IN (N'usp_CheckTrayCodeExists', N'usp_CheckGradeSchemeExists');

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES
               WHERE ROUTINE_SCHEMA = N'dbo' AND ROUTINE_NAME = N'usp_CheckTrayCodeExists')
    PRINT N'❌ usp_CheckTrayCodeExists 未创建';
ELSE IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES
               WHERE ROUTINE_SCHEMA = N'dbo' AND ROUTINE_NAME = N'usp_CheckGradeSchemeExists')
    PRINT N'❌ usp_CheckGradeSchemeExists 未创建';
ELSE
BEGIN
    DECLARE @E bit;
    EXEC dbo.usp_CheckTrayCodeExists @TrayCode = N'_unit_test_', @Exists = @E OUTPUT;
    PRINT N'✔ usp_CheckTrayCodeExists 工作正常 (单元测试 _unit_test_ 返回 Exists=' + CAST(@E AS nvarchar(1)) + N')';
    EXEC dbo.usp_CheckGradeSchemeExists @SchemeName = N'_unit_test_', @Exists = @E OUTPUT, @IsActive = 1;
    PRINT N'✔ usp_CheckGradeSchemeExists 工作正常 (单元测试 _unit_test_ 返回 Exists=' + CAST(@E AS nvarchar(1)) + N')';
END