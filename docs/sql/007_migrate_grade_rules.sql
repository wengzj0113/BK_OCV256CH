/*
  OCV_DATA 迁移脚本:R2-15 分档规则系统
  目标:把 CWSQL 旧库里的 CW_tblCondition + CW_tblSelFld 数据
  一次性报迁到 OCV_DATA.GradeScheme / GradeRule / GradeSelectableField。

  运行顺序:
    1. 000_init_database.sql
    2. 001_create_ocv_data_schema.sql
    3. 002_create_core_procedures.sql
    4. 006_migrate_unsent_data_to_upload_outbox.sql
    5. 本脚本 007_migrate_grade_rules.sql

  旧表字段映射:
    CW_tblCondition.LvName      -> GradeScheme.SchemeName
    CW_tblCondition.CondName    -> GradeRule.GradeCode
    CW_tblCondition.FldName_E   -> GradeRule.FieldName (经过字段名映射)
    CW_tblCondition.FldCond ">=" + FldValue -> GradeRule.LowerLimit
    CW_tblCondition.FldCond "<"  + FldValue -> GradeRule.UpperLimit
    CW_tblCondition.Seq          -> GradeRule.SortOrder

    CW_tblSelFld.Fld_EN         -> GradeSelectableField.FieldName (经过字段名映射)
    CW_tblSelFld.Fld_CN          -> GradeSelectableField.DisplayName

  字段名映射规则 (旧 -> 新):
    CW_tblData_Info.Fld_VolA/B/C/D   -> VoltageVolt
    CW_tblData_Info.Fld_ResiA/B/C/D  -> FinalResistanceMilliohm
    CW_tblData_Info.Fld_TimeA/B/C/D  -> (跳过,不用于分档)
*/

USE OCV_DATA;
GO

IF OBJECT_ID(N'dbo.usp_MigrateLegacyGradeRules', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_MigrateLegacyGradeRules;
GO

CREATE PROCEDURE dbo.usp_MigrateLegacyGradeRules
    @SourceDatabase sysname = N'CWSQL',
    @DryRun bit = 1,
    @MigratedSchemes int OUTPUT,
    @MigratedRules int OUTPUT,
    @MigratedFields int OUTPUT,
    @SkippedRows int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @MigratedSchemes = 0;
    SET @MigratedRules = 0;
    SET @MigratedFields = 0;
    SET @SkippedRows = 0;

    -- 1. 迁移 CW_tblSelFld -> GradeSelectableField
    DECLARE @SelFldSql nvarchar(max) = N'
        SELECT Fld_CN, Fld_EN
        FROM ' + QUOTENAME(@SourceDatabase) + N'.dbo.CW_tblSelFld';

    DECLARE @SelFld TABLE (Fld_CN nvarchar(100), Fld_EN nvarchar(200));

    BEGIN TRY
        INSERT INTO @SelFld EXEC sp_executesql @SelFldSql;
    END TRY
    BEGIN CATCH
        PRINT N'[Migrate] 跳过 CW_tblSelFld 表读取: ' + ERROR_MESSAGE();
    END CATCH

    DECLARE curFld CURSOR LOCAL FAST_FORWARD FOR
        SELECT Fld_CN, Fld_EN FROM @SelFld;

    DECLARE @FldCN nvarchar(100), @FldEN nvarchar(200);
    DECLARE @NewFieldName nvarchar(50);

    OPEN curFld;
    FETCH NEXT FROM curFld INTO @FldCN, @FldEN;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- 字段名映射:旧全限定列名 -> 新字段名
        SET @NewFieldName = CASE
            WHEN @FldEN LIKE N'%Fld_Vol%' THEN N'VoltageVolt'
            WHEN @FldEN LIKE N'%Fld_Resi%' THEN N'FinalResistanceMilliohm'
            ELSE NULL
        END;

        IF @NewFieldName IS NULL
        BEGIN
            -- 时间字段等不用于分档,跳过
            SET @SkippedRows = @SkippedRows + 1;
            FETCH NEXT FROM curFld INTO @FldCN, @FldEN;
            CONTINUE;
        END

        IF @DryRun = 0
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.GradeSelectableField WHERE FieldName = @NewFieldName)
            BEGIN
                INSERT INTO dbo.GradeSelectableField(FieldName, DisplayName, UnitName, IsActive)
                VALUES (@NewFieldName, @FldCN,
                    CASE WHEN @NewFieldName = N'VoltageVolt' THEN N'V'
                         WHEN @NewFieldName = N'FinalResistanceMilliohm' THEN N'mOhm'
                         ELSE NULL END, 1);
            END
        END

        SET @MigratedFields = @MigratedFields + 1;
        FETCH NEXT FROM curFld INTO @FldCN, @FldEN;
    END;

    CLOSE curFld;
    DEALLOCATE curFld;

    -- 2. 迁移 CW_tblCondition -> GradeScheme + GradeRule
    DECLARE @CondSql nvarchar(max) = N'
        SELECT LvName, CondName, FldName, FldCond, FldValue, FldName_E
        FROM ' + QUOTENAME(@SourceDatabase) + N'.dbo.CW_tblCondition';

    DECLARE @Cond TABLE
    (
        LvName nvarchar(100),
        CondName nvarchar(100),
        FldName nvarchar(100),
        FldCond nvarchar(20),
        FldValue nvarchar(200),
        FldName_E nvarchar(200)
    );

    BEGIN TRY
        INSERT INTO @Cond EXEC sp_executesql @CondSql;
    END TRY
    BEGIN CATCH
        PRINT N'[Migrate] 跳过 CW_tblCondition 表读取: ' + ERROR_MESSAGE();
    END CATCH

    -- 先创建所有 GradeScheme (每个 LvName 一个)
    DECLARE @DistinctSchemes TABLE (LvName nvarchar(100));
    INSERT INTO @DistinctSchemes
    SELECT DISTINCT LvName FROM @Cond WHERE LvName IS NOT NULL;

    DECLARE curSch CURSOR LOCAL FAST_FORWARD FOR
        SELECT LvName FROM @DistinctSchemes;

    DECLARE @LvName nvarchar(100);
    DECLARE @GradeSchemeId int;

    OPEN curSch;
    FETCH NEXT FROM curSch INTO @LvName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @DryRun = 0
        BEGIN
            -- 创建或获取 GradeScheme
            SELECT @GradeSchemeId = GradeSchemeId
            FROM dbo.GradeScheme
            WHERE SchemeName = @LvName AND IsActive = 1;

            IF @GradeSchemeId IS NULL
            BEGIN
                INSERT INTO dbo.GradeScheme(SchemeName, IsActive)
                VALUES (@LvName, 1);

                SET @GradeSchemeId = CONVERT(int, SCOPE_IDENTITY());
            END
        END
        ELSE
        BEGIN
            SET @GradeSchemeId = -1;
        END

        SET @MigratedSchemes = @MigratedSchemes + 1;

        -- 迁移该方案下的所有规则
        DECLARE curRule CURSOR LOCAL FAST_FORWARD FOR
            SELECT CondName, FldName, FldCond, FldValue, FldName_E
            FROM @Cond
            WHERE LvName = @LvName
            ORDER BY CondName, FldName_E, FldCond;

        DECLARE @CondName nvarchar(100), @FldName2 nvarchar(100);
        DECLARE @FldCond2 nvarchar(20), @FldValue2 nvarchar(200), @FldName_E2 nvarchar(200);
        DECLARE @RuleFieldName nvarchar(50);
        DECLARE @LowerLimit decimal(18,9), @UpperLimit decimal(18,9);
        DECLARE @SortOrder int;
        DECLARE @LastCondName nvarchar(100) = '';
        DECLARE @LastFldName_E nvarchar(200) = '';

        SET @SortOrder = 0;

        OPEN curRule;
        FETCH NEXT FROM curRule INTO @CondName, @FldName2, @FldCond2, @FldValue2, @FldName_E2;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- 字段名映射
            SET @RuleFieldName = CASE
                WHEN @FldName_E2 LIKE N'%Fld_Vol%' THEN N'VoltageVolt'
                WHEN @FldName_E2 LIKE N'%Fld_Resi%' THEN N'FinalResistanceMilliohm'
                ELSE NULL
            END;

            IF @RuleFieldName IS NULL
            BEGIN
                SET @SkippedRows = @SkippedRows + 1;
                FETCH NEXT FROM curRule INTO @CondName, @FldName2, @FldCond2, @FldValue2, @FldName_E2;
                CONTINUE;
            END

            -- 解析上下限:">=" 的行作为 LowerLimit,"<" 的行作为 UpperLimit
            SET @LowerLimit = NULL;
            SET @UpperLimit = NULL;

            IF @FldCond2 = N'>=' AND TRY_CONVERT(decimal(18,9), @FldValue2) IS NOT NULL
                SET @LowerLimit = CONVERT(decimal(18,9), @FldValue2);
            ELSE IF @FldCond2 = N'<' AND TRY_CONVERT(decimal(18,9), @FldValue2) IS NOT NULL
                SET @UpperLimit = CONVERT(decimal(18,9), @FldValue2);
            ELSE
            BEGIN
                SET @SkippedRows = @SkippedRows + 1;
                FETCH NEXT FROM curRule INTO @CondName, @FldName2, @FldCond2, @FldValue2, @FldName_E2;
                CONTINUE;
            END

            -- 排序:同一 GradeCode + FieldName 的成对规则用相同 SortOrder
            IF @CondName <> @LastCondName OR @FldName_E2 <> @LastFldName_E
                SET @SortOrder = @SortOrder + 1;

            SET @LastCondName = @CondName;
            SET @LastFldName_E = @FldName_E2;

            IF @DryRun = 0
            BEGIN
                -- 检查是否已存在相同的 (GradeSchemeId, GradeCode, FieldName, LowerLimit, UpperLimit)
                IF NOT EXISTS (
                    SELECT 1 FROM dbo.GradeRule
                    WHERE GradeSchemeId = @GradeSchemeId
                      AND GradeCode = @CondName
                      AND FieldName = @RuleFieldName
                      AND ISNULL(LowerLimit, -999999) = ISNULL(@LowerLimit, -999999)
                      AND ISNULL(UpperLimit, -999999) = ISNULL(@UpperLimit, -999999)
                )
                BEGIN
                    INSERT INTO dbo.GradeRule(GradeSchemeId, GradeCode, FieldName, LowerLimit, UpperLimit, SortOrder)
                    VALUES (@GradeSchemeId, @CondName, @RuleFieldName, @LowerLimit, @UpperLimit, @SortOrder);
                END
            END

            SET @MigratedRules = @MigratedRules + 1;
            FETCH NEXT FROM curRule INTO @CondName, @FldName2, @FldCond2, @FldValue2, @FldName_E2;
        END;

        CLOSE curRule;
        DEALLOCATE curRule;

        FETCH NEXT FROM curSch INTO @LvName;
    END;

    CLOSE curSch;
    DEALLOCATE curSch;

    PRINT N'[Migrate] ' + CASE WHEN @DryRun = 1 THEN N'(DryRun) ' ELSE N'' END +
          N'已报迁 Schemes=' + CONVERT(nvarchar(20), @MigratedSchemes) +
          N', Rules=' + CONVERT(nvarchar(20), @MigratedRules) +
          N', Fields=' + CONVERT(nvarchar(20), @MigratedFields) +
          N', Skipped=' + CONVERT(nvarchar(20), @SkippedRows);
END
GO

-- 演练模式调用示例:
-- DECLARE @s int, @r int, @f int, @k int;
-- EXEC dbo.usp_MigrateLegacyGradeRules @SourceDatabase = N'CWSQL', @DryRun = 1,
--     @MigratedSchemes = @s OUTPUT, @MigratedRules = @r OUTPUT,
--     @MigratedFields = @f OUTPUT, @SkippedRows = @k OUTPUT;
-- SELECT @s AS Schemes, @r AS Rules, @f AS Fields, @k AS Skipped;

-- 正式执行(请确认 DryRun 结果后再设 DryRun = 0):
-- DECLARE @s int, @r int, @f int, @k int;
-- EXEC dbo.usp_MigrateLegacyGradeRules @SourceDatabase = N'CWSQL', @DryRun = 0,
--     @MigratedSchemes = @s OUTPUT, @MigratedRules = @r OUTPUT,
--     @MigratedFields = @f OUTPUT, @SkippedRows = @k OUTPUT;
-- SELECT @s AS Schemes, @r AS Rules, @f AS Fields, @k AS Skipped;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo = N'004')
BEGIN
    INSERT INTO dbo.SchemaVersion(VersionNo, ScriptName, Remark)
    VALUES (N'004', N'004_migrate_grade_rules.sql', N'Migrate legacy CW_tblCondition/CW_tblSelFld to GradeScheme/GradeRule/GradeSelectableField.');
END
GO