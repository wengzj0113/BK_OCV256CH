/* Run only against a disposable verification database after 015/016.
   All fixtures run inside transactions and are rolled back, including negative cases.
   Compatible with SQL Server 2008 R2+. No application connection settings are used. */
USE OCV_DATA;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Prefix nvarchar(70) = N'VERIFY-BARCODE-' + REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N'');
DECLARE @Tray nvarchar(100) = @Prefix + N'-TRAY', @OtherTray nvarchar(100) = @Prefix + N'-OTHER';
DECLARE @A nvarchar(100) = @Prefix + N'-A', @B nvarchar(100) = @Prefix + N'-B', @Untouched nvarchar(100) = @Prefix + N'-U';
DECLARE @TrayId bigint, @OtherTrayId bigint, @Task1 bigint, @Task2 bigint, @Task3 bigint, @Stage1 int, @Stage2 int, @Spec int;
DECLARE @OldId bigint, @CanonicalId bigint, @Created datetime2(0), @Operation uniqueidentifier = NEWID(), @Now datetime2 = SYSDATETIME();
DECLARE @Results dbo.TestResultInputType;
SELECT @Stage1 = OcvStageId FROM dbo.OcvStage WHERE StageCode = N'OCV0';
SELECT @Stage2 = OcvStageId FROM dbo.OcvStage WHERE StageCode = N'OCV1';
IF @Stage1 IS NULL OR @Stage2 IS NULL RAISERROR(N'Baseline OCV stages are required.', 16, 1);
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT dbo.Tray(TrayCode, SourceType) VALUES(@Tray, N'Verification');
    SET @TrayId = SCOPE_IDENTITY();
    INSERT dbo.Tray(TrayCode, SourceType) VALUES(@OtherTray, N'Verification');
    SET @OtherTrayId = SCOPE_IDENTITY();
    INSERT dbo.TestSpec(SpecName, OcvStageId) VALUES(@Prefix, @Stage2);
    SET @Spec = SCOPE_IDENTITY();
    INSERT dbo.TestSpecLimit(TestSpecId, MeasurementType, LowerLimit, UpperLimit, UnitName)
    VALUES(@Spec, N'Voltage', 3, 4.5, N'V'), (@Spec, N'Resistance', 0.1, 9, N'mOhm');
    INSERT dbo.TestTask(TaskCode, TrayId, TrayCode, OcvStageId, TestType, ChannelCount)
    VALUES(@Prefix + N'-1', @TrayId, @Tray, @Stage1, N'Formal', 2);
    SET @Task1 = SCOPE_IDENTITY();
    INSERT dbo.TestTask(TaskCode, TrayId, TrayCode, OcvStageId, TestSpecId, TestType, ChannelCount)
    VALUES(@Prefix + N'-2', @OtherTrayId, @OtherTray, @Stage2, @Spec, N'Debug', 2);
    SET @Task2 = SCOPE_IDENTITY();
    INSERT dbo.TestTask(TaskCode, TrayId, TrayCode, OcvStageId, TestType, ChannelCount)
    VALUES(@Prefix + N'-3', @TrayId, @Tray, @Stage1, N'Manual', 2);
    SET @Task3 = SCOPE_IDENTITY();
    INSERT dbo.TrayCell(TrayId, ChannelNo, RowNo, ColNo, BatteryCode, SourceType)
    VALUES(@OtherTrayId, 1, 1, 1, N'  ' + @A + N'  ', N'Verification');

    -- Both touched and untouched legacy duplicates, including padded legacy barcodes.
    INSERT dbo.TestResult(TestTaskId, TrayId, TrayCode, OcvStageId, ChannelNo, BatteryCode, IsNg, CreatedAt)
    VALUES(@Task1, @TrayId, @Tray, @Stage1, 1, N' '+@A+N' ', 1, '20010101');
    SET @OldId = SCOPE_IDENTITY();
    INSERT dbo.TestResult(TestTaskId, TrayId, TrayCode, OcvStageId, ChannelNo, BatteryCode, IsNg, CreatedAt, UpdatedAt)
    VALUES(@Task1, @TrayId, @Tray, @Stage1, 2, @A, 1, '20000101', '20020101');
    SET @CanonicalId = SCOPE_IDENTITY();
    SELECT @Created = CreatedAt FROM dbo.TestResult WHERE TestResultId = @CanonicalId;
    INSERT dbo.TestResult(TestTaskId, TrayId, TrayCode, OcvStageId, ChannelNo, BatteryCode)
    VALUES(@Task1, @TrayId, @Tray, @Stage1, 3, @Untouched), (@Task1, @TrayId, @Tray, @Stage1, 4, @Untouched);
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@A, @TrayCode=@Tray, @ChannelNo=1, @TrayPosition=N'1-1',
        @OriginalVoltage=3, @OriginalResistance=1, @OriginalResult=N'NG', @RetestVoltage=3.5,
        @RetestResistance=1.2, @RetestRawResistanceMilliohm=1, @CalibrationValueMilliohm=0.2,
        @RetestFinalResistanceMilliohm=1.2, @RetestResult=N'OK', @RetestTime=@Now,
        @OverwriteOriginal=0, @OperatorName=N'Verification', @MeterPort=N'VERIFY',
        @SourceTestResultId=@OldId, @RetestOperationId=@Operation;
    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE LTRIM(RTRIM(BatteryCode))=@A) <> 2
        RAISERROR(N'Audit-only NG retest changed original duplicates.',16,1);

    INSERT @Results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm, CalibrationValueMilliohm, IsNg, ResultSource)
    VALUES(1, N' ', 2, 3.7, 1.1, 0.2, 1, N'ManualTest'), (2, @B, 1, 3.8, 1.2, 0.3, 0, N'ManualTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task2, @Results=@Results;
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task2, @Results=@Results;
    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE LTRIM(RTRIM(BatteryCode))=@A) <> 1
        RAISERROR(N'Touched legacy duplicates were not collapsed.',16,1);
    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE BatteryCode=@Untouched) <> 2
        RAISERROR(N'Untouched legacy duplicates changed.',16,1);
    IF NOT EXISTS (SELECT 1 FROM dbo.TestResult WHERE TestResultId=@CanonicalId AND CreatedAt=@Created
        AND TestTaskId=@Task2 AND TrayId=@OtherTrayId AND TrayCode=@OtherTray AND OcvStageId=@Stage2
        AND ChannelNo=1 AND BatteryCode=@A AND ExecutionOrder=2 AND VoltageVolt=3.7
        AND RawResistanceMilliohm=1.1 AND CalibrationValueMilliohm=0.2 AND FinalResistanceMilliohm=1.3
        AND VoltageLowerLimit=3 AND VoltageUpperLimit=4.5 AND ResistanceLowerLimit=0.1 AND ResistanceUpperLimit=9
        AND IsNg=1 AND ResultSource=N'ManualTest')
        RAISERROR(N'Canonical ID/time/context/spec/numeric values were not preserved or updated correctly.',16,1);
    IF NOT EXISTS(SELECT 1 FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId=@Operation AND SourceTestResultId=@CanonicalId)
        RAISERROR(N'Historical retest reference was not redirected.',16,1);

    -- A delayed audit request still carries the deleted duplicate ID from its original query.
    DECLARE @DelayedOperation uniqueidentifier=NEWID();
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@A,@TrayCode=@Tray,@ChannelNo=1,@TrayPosition=N'1-1',
        @OriginalVoltage=2.9,@OriginalResistance=0.8,@OriginalResult=N'NG',@RetestVoltage=3.1,
        @RetestResistance=1.7,@RetestRawResistanceMilliohm=1.4,@CalibrationValueMilliohm=0.3,
        @RetestFinalResistanceMilliohm=1.7,@RetestResult=N'OK',@RetestTime=@Now,
        @OverwriteOriginal=0,@OperatorName=N'Verification',@MeterPort=N'VERIFY',
        @SourceTestResultId=@OldId,@RetestOperationId=@DelayedOperation;
    IF NOT EXISTS(SELECT 1 FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId=@DelayedOperation
        AND SourceTestResultId=@CanonicalId AND OriginalVoltage=CONVERT(real,2.9) AND OriginalResistance=CONVERT(real,0.8)
        AND RetestVoltage=CONVERT(real,3.1) AND RetestRawResistanceMilliohm=1.4 AND CalibrationValueMilliohm=0.3
        AND RetestFinalResistanceMilliohm=1.7 AND TrayCode=@Tray AND ChannelNo=1)
        RAISERROR(N'Delayed audit failed source remapping or changed its original snapshots.',16,1);
    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE BatteryCode=@A)<>1
        OR NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@CanonicalId AND VoltageVolt=3.7 AND IsNg=1)
        RAISERROR(N'Delayed audit changed the current result.',16,1);
    -- A/B exchange channels in one batch; the old unique task/channel index prevented this.
    DELETE FROM @Results;
    INSERT @Results(ChannelNo, BatteryCode, VoltageVolt, RawResistanceMilliohm, CalibrationValueMilliohm, IsNg)
    VALUES(2,@A,3.9,2,0.1,1),(1,@B,3.6,3,0.2,0);
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task2, @Results=@Results;
    IF NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@CanonicalId AND ChannelNo=2)
        RAISERROR(N'Barcode channel swap failed.',16,1);
    -- Move the same barcodes again across stage, tray, Debug -> Manual; clear obsolete spec limits.
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task3, @Results=@Results;
    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE BatteryCode IN(@A,@B)) <> 2
        RAISERROR(N'Cross-task/stage/mode save created extra rows.',16,1);
    IF NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@CanonicalId AND TestTaskId=@Task3
        AND TrayId=@TrayId AND OcvStageId=@Stage1 AND VoltageLowerLimit IS NULL AND ResistanceUpperLimit IS NULL)
        RAISERROR(N'Cross-context save retained obsolete context/spec limits.',16,1);

    -- Introduce one older legacy duplicate; overwrite must collapse it and remap audit references.
    INSERT dbo.TestResult(TestTaskId, TrayId, TrayCode, OcvStageId, ChannelNo, BatteryCode, IsNg, CreatedAt)
    VALUES(@Task1,@TrayId,@Tray,@Stage1,5,@A,1,'19990101');
    SET @OldId=SCOPE_IDENTITY();
    UPDATE dbo.OCV_RETEST_DATA SET SourceTestResultId=@OldId WHERE RetestOperationId=@Operation;
    DECLARE @OverwriteOperation uniqueidentifier=NEWID(), @Fingerprint char(64)=REPLICATE('a',64);
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@A, @TrayCode=@Tray, @ChannelNo=2, @TrayPosition=N'1-2',
        @OriginalVoltage=3.9, @OriginalResistance=2.1, @OriginalResult=N'NG', @RetestVoltage=4,
        @RetestResistance=4.5, @RetestRawResistanceMilliohm=4, @CalibrationValueMilliohm=0.5,
        @RetestFinalResistanceMilliohm=4.5, @RetestResult=N'OK', @RetestTime=@Now,
        @OverwriteOriginal=1, @OperatorName=N'Verification', @MeterPort=N'VERIFY',
        @SourceTestResultId=@CanonicalId, @RetestOperationId=@OverwriteOperation,
        @CalibrationSnapshotFingerprint=@Fingerprint, @CalibrationSnapshotSource=N'Verification snapshot';
    -- Idempotent retry must succeed even though the source is now OK.
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@A, @TrayCode=@Tray, @ChannelNo=2, @TrayPosition=N'1-2',
        @OriginalVoltage=3.9, @OriginalResistance=2.1, @OriginalResult=N'NG', @RetestVoltage=4,
        @RetestResistance=4.5, @RetestRawResistanceMilliohm=4, @CalibrationValueMilliohm=0.5,
        @RetestFinalResistanceMilliohm=4.5, @RetestResult=N'OK', @RetestTime=@Now,
        @OverwriteOriginal=1, @OperatorName=N'Verification', @MeterPort=N'VERIFY',
        @SourceTestResultId=@CanonicalId, @RetestOperationId=@OverwriteOperation;
    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE BatteryCode=@A) <> 1
        OR NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@CanonicalId AND CreatedAt=@Created
            AND RawResistanceMilliohm=4 AND CalibrationValueMilliohm=0.5 AND FinalResistanceMilliohm=4.5 AND IsNg=0)
        RAISERROR(N'NG overwrite failed canonical collapse/calibration preservation.',16,1);
    IF EXISTS(SELECT 1 FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId IN(@Operation,@OverwriteOperation) AND SourceTestResultId<>@CanonicalId)
        RAISERROR(N'NG overwrite did not remap source references.',16,1);
    IF (SELECT COUNT(*) FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId=@OverwriteOperation
        AND CalibrationSnapshotFingerprint=@Fingerprint AND CalibrationSnapshotSource=N'Verification snapshot')<>1
        RAISERROR(N'NG snapshot or operation idempotency failed.',16,1);
    -- Historical rows with a NULL result barcode remain valid NG sources via TrayCell fallback.
    DECLARE @FallbackCode nvarchar(100)=@Prefix+N'-F', @FallbackId bigint, @FallbackOperation uniqueidentifier=NEWID();
    INSERT dbo.TrayCell(TrayId,ChannelNo,RowNo,ColNo,BatteryCode,SourceType)
    VALUES(@TrayId,8,8,1,N' '+@FallbackCode+N' ',N'Verification');
    INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,IsNg,VoltageVolt,FinalResistanceMilliohm)
    VALUES(@Task1,@TrayId,@Tray,@Stage1,8,NULL,1,3,1);
    SET @FallbackId=SCOPE_IDENTITY();
    DECLARE @FallbackDuplicateId bigint,@FallbackAuditOperation uniqueidentifier=NEWID();
    INSERT dbo.TrayCell(TrayId,ChannelNo,RowNo,ColNo,BatteryCode,SourceType)
    VALUES(@TrayId,9,9,1,@FallbackCode,N'Verification');
    INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,IsNg,CreatedAt)
    VALUES(@Task1,@TrayId,@Tray,@Stage1,9,NULL,1,'19990101');
    SET @FallbackDuplicateId=SCOPE_IDENTITY();
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@FallbackCode,@TrayCode=@Tray,@ChannelNo=9,@TrayPosition=N'9-1',
        @OriginalVoltage=3,@OriginalResistance=1,@OriginalResult=N'NG',@RetestVoltage=3,
        @RetestResistance=1,@RetestRawResistanceMilliohm=1,@CalibrationValueMilliohm=0,
        @RetestFinalResistanceMilliohm=1,@RetestResult=N'OK',@RetestTime=@Now,
        @OverwriteOriginal=0,@OperatorName=N'Verification',@MeterPort=N'VERIFY',
        @SourceTestResultId=@FallbackDuplicateId,@RetestOperationId=@FallbackAuditOperation;
    IF NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@FallbackDuplicateId)
        RAISERROR(N'Audit-only fallback retest collapsed original duplicates.',16,1);
    DECLARE @NgSource TABLE(TestResultId bigint,BatteryCode nvarchar(100),TrayCode nvarchar(100),ChannelNo smallint,
        OriginalVoltage decimal(18,9),OriginalResistance decimal(18,6),OriginalResult nvarchar(10),OriginalTestTime datetime2(0),TrayPosition nvarchar(20));
    INSERT @NgSource EXEC dbo.usp_GetNgRetestSourceByBatteryCode @BatteryCode=@FallbackCode;
    IF NOT EXISTS(SELECT 1 FROM @NgSource WHERE TestResultId=@FallbackId AND BatteryCode=@FallbackCode)
        RAISERROR(N'Legacy NULL-barcode source was not returned by TrayCell fallback.',16,1);
    UPDATE dbo.TestResult SET BatteryCode=N'   ' WHERE TestResultId=@FallbackId;
    DELETE FROM @NgSource;
    INSERT @NgSource EXEC dbo.usp_GetNgRetestSourceByBatteryCode @BatteryCode=@FallbackCode;
    IF NOT EXISTS(SELECT 1 FROM @NgSource WHERE TestResultId=@FallbackId AND BatteryCode=@FallbackCode)
        RAISERROR(N'Legacy blank-barcode source did not return normalized TrayCell fallback.',16,1);
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@FallbackCode,@TrayCode=@Tray,@ChannelNo=8,@TrayPosition=N'8-1',
        @OriginalVoltage=3,@OriginalResistance=1,@OriginalResult=N'NG',@RetestVoltage=3.25,
        @RetestResistance=1.5,@RetestRawResistanceMilliohm=1.2,@CalibrationValueMilliohm=0.3,
        @RetestFinalResistanceMilliohm=1.5,@RetestResult=N'OK',@RetestTime=@Now,
        @OverwriteOriginal=1,@OperatorName=N'Verification',@MeterPort=N'VERIFY',
        @SourceTestResultId=@FallbackId,@RetestOperationId=@FallbackOperation;
    IF NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@FallbackId AND BatteryCode=@FallbackCode
        AND IsNg=0 AND VoltageVolt=3.25 AND RawResistanceMilliohm=1.2 AND CalibrationValueMilliohm=0.3 AND FinalResistanceMilliohm=1.5)
        RAISERROR(N'Legacy NULL-barcode source could not be overwritten with normalized TrayCell identity.',16,1);
    IF NOT EXISTS(SELECT 1 FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId=@FallbackOperation AND SourceTestResultId=@FallbackId)
        RAISERROR(N'Legacy fallback overwrite audit reference is invalid.',16,1);
    IF EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@FallbackDuplicateId)
        OR NOT EXISTS(SELECT 1 FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId=@FallbackAuditOperation AND SourceTestResultId=@FallbackId)
        RAISERROR(N'Legacy fallback overwrite did not collapse duplicate or remap audit reference.',16,1);
    -- Ordinary saves must also reuse effective legacy identities provided by active TrayCell mappings.
    DECLARE @NormalCode nvarchar(100)=@Prefix+N'-NORMAL',@NormalOldId bigint,@NormalCanonicalId bigint,
        @NormalCreated datetime2(0),@NormalAudit uniqueidentifier=NEWID();
    INSERT dbo.TrayCell(TrayId,ChannelNo,RowNo,ColNo,BatteryCode,SourceType)
    VALUES(@TrayId,10,10,1,@NormalCode,N'Verification'),(@TrayId,11,11,1,N' '+@NormalCode+N' ',N'Verification');
    INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,IsNg,CreatedAt)
    VALUES(@Task1,@TrayId,@Tray,@Stage1,10,NULL,1,'20000101');
    SET @NormalOldId=SCOPE_IDENTITY();
    INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,IsNg,CreatedAt,UpdatedAt)
    VALUES(@Task1,@TrayId,@Tray,@Stage1,11,N'  ',1,'19990101','20010101');
    SET @NormalCanonicalId=SCOPE_IDENTITY();
    SELECT @NormalCreated=CreatedAt FROM dbo.TestResult WHERE TestResultId=@NormalCanonicalId;
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@NormalCode,@TrayCode=@Tray,@ChannelNo=10,@TrayPosition=N'10-1',
        @OriginalVoltage=3,@OriginalResistance=1,@OriginalResult=N'NG',@RetestVoltage=3,
        @RetestResistance=1,@RetestRawResistanceMilliohm=1,@CalibrationValueMilliohm=0,
        @RetestFinalResistanceMilliohm=1,@RetestResult=N'OK',@RetestTime=@Now,
        @OverwriteOriginal=0,@OperatorName=N'Verification',@MeterPort=N'VERIFY',
        @SourceTestResultId=@NormalOldId,@RetestOperationId=@NormalAudit;
    DELETE FROM @Results;
    INSERT @Results(ChannelNo,BatteryCode,VoltageVolt,RawResistanceMilliohm,CalibrationValueMilliohm,IsNg,ResultSource)
    VALUES(3,N' '+@NormalCode+N' ',3.75,2,0.25,1,N'ManualTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task3,@Results=@Results;
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task3,@Results=@Results;
    IF NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@NormalCanonicalId AND CreatedAt=@NormalCreated
        AND BatteryCode=@NormalCode AND TestTaskId=@Task3 AND ChannelNo=3 AND VoltageVolt=3.75
        AND RawResistanceMilliohm=2 AND CalibrationValueMilliohm=0.25 AND FinalResistanceMilliohm=2.25)
        OR EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@NormalOldId)
        RAISERROR(N'Ordinary save failed to preserve/normalize the canonical legacy fallback row.',16,1);
    IF (SELECT COUNT(*) FROM dbo.TestResult tr
        LEFT JOIN dbo.TrayCell tc ON tc.TrayId=tr.TrayId AND tc.ChannelNo=tr.ChannelNo AND tc.IsActive=1
        WHERE COALESCE(NULLIF(LTRIM(RTRIM(tr.BatteryCode)),N''),NULLIF(LTRIM(RTRIM(tc.BatteryCode)),N''))=@NormalCode)<>1
        RAISERROR(N'Ordinary save left duplicate effective fallback barcode results.',16,1);
    IF NOT EXISTS(SELECT 1 FROM dbo.OCV_RETEST_DATA WHERE RetestOperationId=@NormalAudit AND SourceTestResultId=@NormalCanonicalId)
        RAISERROR(N'Ordinary fallback save failed to redirect historical audit source.',16,1);
    DELETE FROM @NgSource;
    INSERT @NgSource EXEC dbo.usp_GetNgRetestSourceByBatteryCode @BatteryCode=@NormalCode;
    IF (SELECT COUNT(*) FROM @NgSource)<>1
        OR NOT EXISTS(SELECT 1 FROM @NgSource WHERE TestResultId=@NormalCanonicalId AND BatteryCode=@NormalCode)
        RAISERROR(N'Ordinary fallback save returned a duplicate or incorrect NG source.',16,1);

    -- Re-save a legacy fallback occupant in its existing task/channel to exercise the conflict guard.
    UPDATE dbo.TestResult SET BatteryCode=NULL WHERE TestResultId=@NormalCanonicalId;
    INSERT dbo.TrayCell(TrayId,ChannelNo,RowNo,ColNo,BatteryCode,SourceType)
    VALUES(@TrayId,3,3,1,@NormalCode,N'Verification');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@Task3,@Results=@Results;
    IF NOT EXISTS(SELECT 1 FROM dbo.TestResult WHERE TestResultId=@NormalCanonicalId AND BatteryCode=@NormalCode)
        RAISERROR(N'Ordinary fallback occupant was rejected or replaced instead of updated.',16,1);
    ROLLBACK TRANSACTION;
    PRINT N'PASS: repeat save, fallback, newest canonical, touched/untouched duplicates, source remap, swaps, cross-mode/stage/tray, NG overwrite, delayed audits, legacy NULL/blank-barcode NG and ordinary fallback identity, and snapshots.';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    DECLARE @Failure nvarchar(4000)=ERROR_MESSAGE();
    RAISERROR(N'%s',16,1,@Failure);
    RETURN;
END CATCH;

-- Each rejection has its own rolled-back fixture because procedure errors roll back the enclosing transaction.
DECLARE @Case int=1, @Rejected bit, @Expected nvarchar(200), @Message nvarchar(4000);
WHILE @Case<=10
BEGIN
    BEGIN TRANSACTION;
    INSERT dbo.Tray(TrayCode,SourceType) VALUES(@Tray,N'Verification');
    SET @TrayId=SCOPE_IDENTITY();
    INSERT dbo.TestTask(TaskCode,TrayId,TrayCode,OcvStageId,TestType,ChannelCount)
    VALUES(@Prefix,@TrayId,@Tray,@Stage1,N'Manual',2);
    SET @Task1=SCOPE_IDENTITY();
    DELETE FROM @Results;
    SET @Rejected=0;
    IF @Case=1
    BEGIN
        INSERT @Results(ChannelNo,BatteryCode,IsNg) VALUES(1,@A,0),(2,N' '+@A+N' ',0);
        SET @Expected=N'Duplicate normalized BatteryCode';
    END
    IF @Case=2
    BEGIN
        INSERT @Results(ChannelNo,BatteryCode,IsNg) VALUES(1,@A,0),(1,@B,0);
        SET @Expected=N'Duplicate ChannelNo';
    END
    IF @Case=3
    BEGIN
        INSERT @Results(ChannelNo,BatteryCode,IsNg) VALUES(1,N' ',0);
        SET @Expected=N'BatteryCode is required';
    END
    IF @Case=4 OR @Case=10
    BEGIN
        INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode)
        VALUES(@Task1,@TrayId,@Tray,@Stage1,1,CASE WHEN @Case=10 THEN NULL ELSE @B END);
        IF @Case=10 INSERT dbo.TrayCell(TrayId,ChannelNo,RowNo,ColNo,BatteryCode,SourceType)
            VALUES(@TrayId,1,1,1,@B,N'Verification');
        INSERT @Results(ChannelNo,BatteryCode,IsNg) VALUES(1,@A,0);
        SET @Expected=N'Target task/channel belongs';
    END
    IF @Case=5
    BEGIN
        INSERT @Results(ChannelNo,BatteryCode,IsNg) VALUES(0,@A,0);
        SET @Expected=N'ChannelNo must be';
    END
    IF @Case=6
    BEGIN
        INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,IsNg,CreatedAt)
        VALUES(@Task1,@TrayId,@Tray,@Stage1,1,@A,1,'20000101');
        SET @OldId=SCOPE_IDENTITY();
        INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,IsNg,CreatedAt)
        VALUES(@Task1,@TrayId,@Tray,@Stage1,2,@A,1,'20010101');
        SET @Expected=N'no longer the current barcode result';
    END
    IF @Case=7
    BEGIN
        -- Fail after audit remapping and duplicate deletion, proving they roll back together.
        IF OBJECT_ID(N'dbo.tr_VerifyBarcodeAtomicRollback',N'TR') IS NOT NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR(N'Unexpected verification trigger already exists.',16,1);
            RETURN;
        END
        INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode,CreatedAt)
        VALUES(@Task1,@TrayId,@Tray,@Stage1,1,@A,'20000101'),(@Task1,@TrayId,@Tray,@Stage1,2,@A,'20010101');
        SELECT @OldId=MIN(TestResultId) FROM dbo.TestResult WHERE BatteryCode=@A;
        EXEC dbo.usp_SaveNgRetest @BatteryCode=@A,@TrayCode=@Tray,@ChannelNo=1,@TrayPosition=N'1-1',
            @OriginalVoltage=3,@OriginalResistance=1,@OriginalResult=N'NG',@RetestVoltage=3,
            @RetestResistance=1,@RetestRawResistanceMilliohm=1,@CalibrationValueMilliohm=0,
            @RetestFinalResistanceMilliohm=1,@RetestResult=N'OK',@RetestTime=@Now,
            @OverwriteOriginal=0,@OperatorName=N'Verification',@MeterPort=N'VERIFY',@SourceTestResultId=@OldId;
        EXEC(N'CREATE TRIGGER dbo.tr_VerifyBarcodeAtomicRollback ON dbo.TestResult AFTER UPDATE AS
            BEGIN RAISERROR(N''Injected verification failure after duplicate collapse.'',16,1); END');
        INSERT @Results(ChannelNo,BatteryCode,IsNg) VALUES(1,@A,0);
        SET @Expected=N'Injected verification failure after duplicate collapse';
    END
    IF @Case=8
    BEGIN
        SET @OldId=-1;
        SET @Expected=N'No valid result exists for the NG retest audit barcode';
    END
    IF @Case=9
    BEGIN
        INSERT dbo.TestResult(TestTaskId,TrayId,TrayCode,OcvStageId,ChannelNo,BatteryCode)
        VALUES(@Task1,@TrayId,@Tray,@Stage1,1,@B);
        SET @OldId=SCOPE_IDENTITY();
        SET @Expected=N'audit source result belongs to another barcode';
    END
    BEGIN TRY
        IF @Case<>6 AND @Case<>8 AND @Case<>9 EXEC dbo.usp_SaveTestResults @TestTaskId=@Task1,@Results=@Results;
        ELSE IF @Case=6 EXEC dbo.usp_SaveNgRetest @BatteryCode=@A,@TrayCode=@Tray,@ChannelNo=1,@TrayPosition=N'1-1',
            @OriginalVoltage=3,@OriginalResistance=1,@OriginalResult=N'NG',@RetestVoltage=3,
            @RetestResistance=1,@RetestRawResistanceMilliohm=1,@CalibrationValueMilliohm=0,
            @RetestFinalResistanceMilliohm=1,@RetestResult=N'OK',@RetestTime=@Now,
            @OverwriteOriginal=1,@OperatorName=N'Verification',@MeterPort=N'VERIFY',@SourceTestResultId=@OldId;
        ELSE EXEC dbo.usp_SaveNgRetest @BatteryCode=@A,@TrayCode=@Tray,@ChannelNo=1,@TrayPosition=N'1-1',
            @OriginalVoltage=3,@OriginalResistance=1,@OriginalResult=N'NG',@RetestVoltage=3,
            @RetestResistance=1,@RetestRawResistanceMilliohm=1,@CalibrationValueMilliohm=0,
            @RetestFinalResistanceMilliohm=1,@RetestResult=N'OK',@RetestTime=@Now,
            @OverwriteOriginal=0,@OperatorName=N'Verification',@MeterPort=N'VERIFY',@SourceTestResultId=@OldId;
    END TRY
    BEGIN CATCH
        SET @Message=ERROR_MESSAGE();
        IF CHARINDEX(@Expected,@Message)>0 AND (@Case<>1 OR CHARINDEX(@A,@Message)>0) SET @Rejected=1;
    END CATCH;
    IF @@TRANCOUNT<>0
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR(N'Rejected save did not roll back its complete transaction, or invalid input was accepted.',16,1);
        RETURN;
    END
    IF @Rejected<>1
    BEGIN
        RAISERROR(N'Unexpected rejection result: %s',16,1,@Message);
        RETURN;
    END
    IF OBJECT_ID(N'dbo.tr_VerifyBarcodeAtomicRollback',N'TR') IS NOT NULL
    BEGIN
        RAISERROR(N'Failed transaction left its verification trigger behind.',16,1);
        RETURN;
    END
    IF EXISTS(SELECT 1 FROM dbo.Tray WHERE TrayCode=@Tray)
    BEGIN
        RAISERROR(N'Rejection left fixture writes behind.',16,1);
        RETURN;
    END
    SET @Case=@Case+1;
END;
PRINT N'PASS: normalized duplicate, duplicate channel, blank, unrelated occupant, invalid channel and stale NG source reject atomically; invalid audit sources reject; late mutation failure rolls back all changes.';
IF EXISTS(SELECT 1 FROM dbo.Tray WHERE TrayCode IN(@Tray,@OtherTray))
    RAISERROR(N'Verification left fixture data behind.',16,1);
ELSE PRINT N'PASS: all verification data rolled back.';
GO