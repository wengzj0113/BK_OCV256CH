/*
  OCV_DATA 迁移脚本:R2-15
  目标:把 CWSQL 旧库里的 UnsentData + PendingUploads 历史未上传数据
  一次性报迁到 OCV_DATA.UploadOutbox。

  运行顺序:
    1. 000_init_database.sql
    2. 001_create_ocv_data_schema.sql
    3. 002_create_core_procedures.sql
    4. 本脚本 006_migrate_unsent_data_to_upload_outbox.sql

  说明:
    - 旧表 CWSQL.UnsentData 已加 Is_Sent / InterfaceName / TargetUrl 字段 (见 SqlServerHelper.EnsureUnsentDataSchema)。
    - InterfaceName 取值来自旧版 6 种类: ResultDataUploadAssembly / AlarmUpload /
      WIPInStationCheck / Heartbeat / StateAndStopReasonUpload / EnergyUpload。
    - 报迁完成后,旧表数据保留 30 天观察期再人工 DROP。
    - UploadOutbox 的 Status 用 'Pending',NextRetryAt 立即到期以便后台立即拾取。
*/

USE OCV_DATA;
GO

IF OBJECT_ID(N'dbo.usp_MigrateLegacyUnsentDataToUploadOutbox', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_MigrateLegacyUnsentDataToUploadOutbox;
GO

CREATE PROCEDURE dbo.usp_MigrateLegacyUnsentDataToUploadOutbox
    @SourceDatabase sysname = N'CWSQL',
    @DryRun bit = 1,
    @MigratedRows int OUTPUT,
    @SkippedRows int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @MigratedRows = 0;
    SET @SkippedRows = 0;

    DECLARE @UnsentSql nvarchar(max) = N'
        SELECT Id, Data, Upload_Time, InterfaceName, TargetUrl
        FROM ' + QUOTENAME(@SourceDatabase) + N'.dbo.UnsentData
        WHERE Is_Sent = 0';

    DECLARE @PendingSql nvarchar(max) = N'
        SELECT Id, JsonData, InterfaceName
        FROM ' + QUOTENAME(@SourceDatabase) + N'.dbo.PendingUploads';

    DECLARE @Unsent TABLE
    (
        LegacyId int NOT NULL,
        Data nvarchar(max) NOT NULL,
        UploadTime datetime NOT NULL,
        InterfaceName nvarchar(100) NULL,
        TargetUrl nvarchar(500) NULL
    );

    DECLARE @Pending TABLE
    (
        LegacyId int NOT NULL,
        Data nvarchar(max) NULL,
        InterfaceName nvarchar(100) NULL
    );

    BEGIN TRY
        INSERT INTO @Unsent EXEC sp_executesql @UnsentSql;
    END TRY
    BEGIN CATCH
        PRINT N'[Migrate] 跳过 UnsentData 表读取: ' + ERROR_MESSAGE();
    END CATCH

    BEGIN TRY
        INSERT INTO @Pending EXEC sp_executesql @PendingSql;
    END TRY
    BEGIN CATCH
        PRINT N'[Migrate] 跳过 PendingUploads 表读取: ' + ERROR_MESSAGE();
    END CATCH

    -- UnsentData 报迁
    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT LegacyId, Data, InterfaceName, TargetUrl FROM @Unsent;

    DECLARE @LegacyId int, @Data nvarchar(max), @InterfaceName nvarchar(100), @TargetUrl nvarchar(500);

    OPEN cur;
    FETCH NEXT FROM cur INTO @LegacyId, @Data, @InterfaceName, @TargetUrl;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @Data IS NULL OR LEN(@Data) = 0 OR ISJSON(@Data) = 0
        BEGIN
            SET @SkippedRows = @SkippedRows + 1;
            FETCH NEXT FROM cur INTO @LegacyId, @Data, @InterfaceName, @TargetUrl;
            CONTINUE;
        END

        DECLARE @UploadType nvarchar(50) = COALESCE(NULLIF(@InterfaceName, N''), N'LegacyUnsentData');
        DECLARE @BusinessKey nvarchar(200) = COALESCE(@TargetUrl, N'LegacyUnsent:' + CONVERT(nvarchar(20), @LegacyId));
        DECLARE @IdempotencyKey nvarchar(200) = N'Migrated:' + @UploadType + N':' + CONVERT(nvarchar(20), @LegacyId);

        IF @DryRun = 0
        BEGIN
            EXEC dbo.usp_EnqueueUpload
                @IdempotencyKey = @IdempotencyKey,
                @UploadType = @UploadType,
                @BusinessKey = @BusinessKey,
                @TestTaskId = NULL,
                @PayloadJson = @Data;
        END

        SET @MigratedRows = @MigratedRows + 1;
        FETCH NEXT FROM cur INTO @LegacyId, @Data, @InterfaceName, @TargetUrl;
    END;

    CLOSE cur;
    DEALLOCATE cur;

    -- PendingUploads 报迁 (无 UploadType 字段,统一归为 PendingUploadLegacy)
    DECLARE curP CURSOR LOCAL FAST_FORWARD FOR
        SELECT LegacyId, Data, InterfaceName FROM @Pending;

    DECLARE @PData nvarchar(max), @PInterface nvarchar(100);

    OPEN curP;
    FETCH NEXT FROM curP INTO @LegacyId, @PData, @PInterface;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @PData IS NULL OR LEN(@PData) = 0 OR ISJSON(@PData) = 0
        BEGIN
            SET @SkippedRows = @SkippedRows + 1;
            FETCH NEXT FROM curP INTO @LegacyId, @PData, @PInterface;
            CONTINUE;
        END

        DECLARE @PUploadType nvarchar(50) = N'PendingUploadLegacy';
        DECLARE @PIdempotencyKey nvarchar(200) = N'Migrated:PendingUpload:' + CONVERT(nvarchar(20), @LegacyId);

        IF @DryRun = 0
        BEGIN
            EXEC dbo.usp_EnqueueUpload
                @IdempotencyKey = @PIdempotencyKey,
                @UploadType = @PUploadType,
                @BusinessKey = N'LegacyPending:' + CONVERT(nvarchar(20), @LegacyId),
                @TestTaskId = NULL,
                @PayloadJson = @PData;
        END

        SET @MigratedRows = @MigratedRows + 1;
        FETCH NEXT FROM curP INTO @LegacyId, @PData, @PInterface;
    END;

    CLOSE curP;
    DEALLOCATE curP;

    PRINT N'[Migrate] ' + CASE WHEN @DryRun = 1 THEN N'(DryRun) ' ELSE N'' END +
          N'已报迁 ' + CONVERT(nvarchar(20), @MigratedRows) +
          N' 条,跳过 ' + CONVERT(nvarchar(20), @SkippedRows) + N' 条。';
END
GO

-- 演练模式调用示例:
-- DECLARE @m int, @s int;
-- EXEC dbo.usp_MigrateLegacyUnsentDataToUploadOutbox @SourceDatabase = N'CWSQL', @DryRun = 1, @MigratedRows = @m OUTPUT, @SkippedRows = @s OUTPUT;
-- SELECT @m AS MigratedRows, @s AS SkippedRows;

-- 正式执行(请确认 DryRun 结果后再设 DryRun = 0):
-- DECLARE @m int, @s int;
-- EXEC dbo.usp_MigrateLegacyUnsentDataToUploadOutbox @SourceDatabase = N'CWSQL', @DryRun = 0, @MigratedRows = @m OUTPUT, @SkippedRows = @s OUTPUT;
-- SELECT @m AS MigratedRows, @s AS SkippedRows;

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo = N'003')
BEGIN
    INSERT INTO dbo.SchemaVersion(VersionNo, ScriptName, Remark)
    VALUES (N'003', N'003_migrate_unsent_data_to_upload_outbox.sql', N'Migrate legacy CWSQL.UnsentData / PendingUploads to UploadOutbox.');
END
GO