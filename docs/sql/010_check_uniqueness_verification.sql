/*
  OCV_DATA 辅助脚本 010
  用途:对 009 创建的 usp_CheckTrayCodeExists / usp_CheckGradeSchemeExists
        做事务回滚式验证(改真实数据前先模拟,验证通过后回滚)。

  运行位置:在 SQL Server 主机上用 SSMS / sqlcmd 以 sysadmin 身份执行。
  前置条件:009_check_uniqueness.sql 已成功执行。
  运行顺序:与 009 紧接,放在 008 之后或独立运行皆可。

  说明:
    - 用 XACT_ABORT + 事务回滚,不在 OCV_DATA 库留下任何痕迹。
    - 走 6 个用例,覆盖:
        Tray:NULL / 不存在 / 存在 (临时建一行,验证完回滚)
        GradeScheme:NULL / 不存在 / 存在 (临时建一行)
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

USE OCV_DATA;
GO


DECLARE @Passed int = 0, @Failed int = 0;
DECLARE @E bit;
DECLARE @TmpTrayCode nvarchar(100) = N'_test_tray_' + CONVERT(nvarchar(20), GETDATE(), 112) + N'_' + CONVERT(nvarchar(20), ABS(CHECKSUM(NEWID())));
DECLARE @TmpSchemeName nvarchar(100) = N'_test_scheme_' + CONVERT(nvarchar(20), GETDATE(), 112) + N'_' + CONVERT(nvarchar(20), ABS(CHECKSUM(NEWID())));

PRINT N'========================================';
PRINT N' 010 校验:CheckTrayCodeExists / CheckGradeSchemeExists';
PRINT N' 临时托盘码: ' + @TmpTrayCode;
PRINT N' 临时方案名: ' + @TmpSchemeName;
PRINT N'========================================';


BEGIN TRAN;

--------------------------------------------------------------
-- 用例 1: NULL 输入,CheckTrayCodeExists 应返回 0
--------------------------------------------------------------
EXEC dbo.usp_CheckTrayCodeExists @TrayCode = NULL, @Exists = @E OUTPUT;
IF @E = 0 BEGIN SET @Passed = @Passed + 1; PRINT N'  ✔ 用例 1 (Tray=NULL) Exists=0'; END
ELSE      BEGIN SET @Failed = @Failed + 1; PRINT N'  ❌ 用例 1 (Tray=NULL) Exists=' + CAST(@E AS nvarchar(1)) + N' (期望 0)'; END;

--------------------------------------------------------------
-- 用例 2: 不存在的 Tray,CheckTrayCodeExists 应返回 0
--------------------------------------------------------------
EXEC dbo.usp_CheckTrayCodeExists @TrayCode = N'_definitely_not_exists_zzz_', @Exists = @E OUTPUT;
IF @E = 0 BEGIN SET @Passed = @Passed + 1; PRINT N'  ✔ 用例 2 (Tray=不存在) Exists=0'; END
ELSE      BEGIN SET @Failed = @Failed + 1; PRINT N'  ❌ 用例 2 (Tray=不存在) Exists=' + CAST(@E AS nvarchar(1)) + N' (期望 0)'; END;

--------------------------------------------------------------
-- 用例 3: 插入临时 Tray 后再查,CheckTrayCodeExists 应返回 1
--------------------------------------------------------------
INSERT INTO dbo.Tray(TrayCode, SourceType, TrayStatus) VALUES (@TmpTrayCode, N'Test', N'Created');
EXEC dbo.usp_CheckTrayCodeExists @TrayCode = @TmpTrayCode, @Exists = @E OUTPUT;
IF @E = 1 BEGIN SET @Passed = @Passed + 1; PRINT N'  ✔ 用例 3 (Tray=新建) Exists=1'; END
ELSE      BEGIN SET @Failed = @Failed + 1; PRINT N'  ❌ 用例 3 (Tray=新建) Exists=' + CAST(@E AS nvarchar(1)) + N' (期望 1)'; END;

ROLLBACK TRAN;  -- 用例 3 之后回滚,后续 GradeScheme 用例独立新事务


BEGIN TRAN;

--------------------------------------------------------------
-- 用例 4: NULL 输入,CheckGradeSchemeExists 应返回 0
--------------------------------------------------------------
EXEC dbo.usp_CheckGradeSchemeExists @SchemeName = NULL, @Exists = @E OUTPUT, @IsActive = 1;
IF @E = 0 BEGIN SET @Passed = @Passed + 1; PRINT N'  ✔ 用例 4 (Scheme=NULL, IsActive=1) Exists=0'; END
ELSE      BEGIN SET @Failed = @Failed + 1; PRINT N'  ❌ 用例 4 (Scheme=NULL) Exists=' + CAST(@E AS nvarchar(1)) + N' (期望 0)'; END;

--------------------------------------------------------------
-- 用例 5: 不存在的 Scheme,CheckGradeSchemeExists 应返回 0
--------------------------------------------------------------
EXEC dbo.usp_CheckGradeSchemeExists @SchemeName = N'_definitely_not_exists_zzz_', @Exists = @E OUTPUT, @IsActive = 1;
IF @E = 0 BEGIN SET @Passed = @Passed + 1; PRINT N'  ✔ 用例 5 (Scheme=不存在, IsActive=1) Exists=0'; END
ELSE      BEGIN SET @Failed = @Failed + 1; PRINT N'  ❌ 用例 5 (Scheme=不存在) Exists=' + CAST(@E AS nvarchar(1)) + N' (期望 0)'; END;

--------------------------------------------------------------
-- 用例 6: 插入临时 GradeScheme 后再查,IsActive=1 应返回 1,IsActive=0 应返回 0
--------------------------------------------------------------
INSERT INTO dbo.GradeScheme(SchemeName, IsActive) VALUES (@TmpSchemeName, 1);
EXEC dbo.usp_CheckGradeSchemeExists @SchemeName = @TmpSchemeName, @Exists = @E OUTPUT, @IsActive = 1;
DECLARE @ActiveCase bit = CASE WHEN @E = 1 THEN 1 ELSE 0 END;
UPDATE dbo.GradeScheme SET IsActive = 0 WHERE SchemeName = @TmpSchemeName;
EXEC dbo.usp_CheckGradeSchemeExists @SchemeName = @TmpSchemeName, @Exists = @E OUTPUT, @IsActive = 1;
DECLARE @InactiveCase bit = CASE WHEN @E = 0 THEN 1 ELSE 0 END;

IF @ActiveCase = 1 AND @InactiveCase = 1
BEGIN
    SET @Passed = @Passed + 1;
    PRINT N'  ✔ 用例 6 (Scheme=新建, IsActive 双态) 活跃=1 / 禁用=0';
END
ELSE
BEGIN
    SET @Failed = @Failed + 1;
    PRINT N'  ❌ 用例 6 (Scheme=新建) 活跃=' + CAST(@ActiveCase AS nvarchar(1)) + N' 禁用=' + CAST(@InactiveCase AS nvarchar(1));
END;

ROLLBACK TRAN;


PRINT N'========================================';
PRINT N' 010 校验结果:Passed=' + CAST(@Passed AS nvarchar(10)) + N', Failed=' + CAST(@Failed AS nvarchar(10));
PRINT N'========================================';

IF @Failed > 0
BEGIN
    PRINT N'❌ 至少一个用例失败';
    -- 用 RAISERROR 抛错,SSMS 会标红
    RAISERROR(N'010 校验失败', 16, 1);
END
ELSE
    PRINT N'✔ 全部 6 个用例通过';