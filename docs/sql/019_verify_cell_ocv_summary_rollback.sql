/*
  Cell OCV summary verification for a disposable OCV_DATA database.
  Run after 018_cell_ocv_summary.sql, or after the modern 001/002 scripts.
  All fixture writes are rolled back.
*/
USE OCV_DATA;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.CellOcvSummary', N'U') IS NULL
BEGIN
    RAISERROR(N'dbo.CellOcvSummary is missing.', 16, 1);
    RETURN;
END
IF OBJECT_ID(N'dbo.usp_GetCellOcvSummary', N'P') IS NULL
BEGIN
    RAISERROR(N'dbo.usp_GetCellOcvSummary is missing.', 16, 1);
    RETURN;
END

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @suffix nvarchar(32) = REPLACE(CONVERT(nvarchar(36), NEWID()), N'-', N'');
    DECLARE @tray nvarchar(100) = N'CELL-OCV-VERIFY-' + @suffix;
    DECLARE @cell nvarchar(100) = N'CELL-OCV-CELL-' + @suffix;
    DECLARE @ngCell nvarchar(100) = N'CELL-OCV-NG-' + @suffix;
    DECLARE @auditCell nvarchar(100) = N'CELL-OCV-AUDIT-' + @suffix;
    DECLARE @taskCode nvarchar(100);
    DECLARE @taskId bigint;
    DECLARE @results dbo.TestResultInputType;

    SET @taskCode = @tray + N'-OCV0';
    EXEC dbo.usp_CreateTestTask @TaskCode=@taskCode, @TrayCode=@tray, @OcvStageCode=N'OCV0',
        @TestType=N'Formal', @ChannelCount=1, @OperatorName=N'Verification';
    SELECT @taskId=TestTaskId FROM dbo.TestTask WHERE TaskCode=@taskCode;
    INSERT @results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm, IsNg, ResultSource)
    VALUES (1, @cell, 1, 3.500000000, 1.000000, 0, 1.000000, 0, N'FormalTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@taskId, @StageTestTime='2026-10-21T08:00:00.000', @Results=@results;
    DELETE @results;

    SET @taskCode = @tray + N'-OCV1';
    EXEC dbo.usp_CreateTestTask @TaskCode=@taskCode, @TrayCode=@tray, @OcvStageCode=N'OCV1',
        @TestType=N'Formal', @ChannelCount=1, @OperatorName=N'Verification';
    SELECT @taskId=TestTaskId FROM dbo.TestTask WHERE TaskCode=@taskCode;
    INSERT @results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm, IsNg, ResultSource)
    VALUES (1, @cell, 1, 3.510000000, 1.100000, 0, 1.100000, 0, N'FormalTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@taskId, @StageTestTime='2026-10-21T10:00:00.000', @Results=@results;
    DELETE @results;

    SET @taskCode = @tray + N'-OCV2';
    EXEC dbo.usp_CreateTestTask @TaskCode=@taskCode, @TrayCode=@tray, @OcvStageCode=N'OCV2',
        @TestType=N'Formal', @ChannelCount=1, @OperatorName=N'Verification';
    SELECT @taskId=TestTaskId FROM dbo.TestTask WHERE TaskCode=@taskCode;
    INSERT @results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm, IsNg, ResultSource)
    VALUES (1, @cell, 1, 3.530000000, 1.200000, 0, 1.200000, 0, N'FormalTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@taskId, @StageTestTime='2026-10-21T14:00:00.000', @Results=@results;
    DELETE @results;

    IF NOT EXISTS
    (
        SELECT 1 FROM dbo.CellOcvSummary
        WHERE BatteryCode=@cell AND Vol0=3.500000000 AND Vol1=3.510000000 AND Vol2=3.530000000
          AND Res0=1.000000 AND Res1=1.100000 AND Res2=1.200000
          AND Time0='2026-10-21T08:00:00.000' AND Time1='2026-10-21T10:00:00.000'
          AND Time2='2026-10-21T14:00:00.000' AND K1=0.005000000 AND K2=0.005000000
    )
        RAISERROR(N'OCV0/1/2 summary values or K1/K2 calculation are incorrect.', 16, 1);

    DECLARE @summary TABLE
    (
        BatteryCode nvarchar(100), Vol0 decimal(18,9), Vol1 decimal(18,9), Vol2 decimal(18,9),
        Res0 decimal(18,6), Res1 decimal(18,6), Res2 decimal(18,6),
        Time0 datetime2(3), Time1 datetime2(3), Time2 datetime2(3),
        K1 decimal(18,9), K2 decimal(18,9), TrayCode nvarchar(100), ChannelNo smallint,
        TaskId0 bigint bigint bigint, CreatedAt datetime2(0), UpdatedAt datetime2(3), RowVersion binary(8)
    );
    INSERT @summary EXEC dbo.usp_GetCellOcvSummary @BatteryCode=@cell;
    IF (SELECT COUNT(*) FROM @summary) <> 1
        RAISERROR(N'Summary lookup did not return exactly one row.', 16, 1);

    -- Replacing OCV0 must refresh K1 without discarding OCV1/2 or leaving K2 stale.
    SET @taskCode = @tray + N'-OCV0-REPEAT';
    EXEC dbo.usp_CreateTestTask @TaskCode=@taskCode, @TrayCode=@tray, @OcvStageCode=N'OCV0',
        @TestType=N'Formal', @ChannelCount=1, @OperatorName=N'Verification';
    SELECT @taskId=TestTaskId FROM dbo.TestTask WHERE TaskCode=@taskCode;
    INSERT @results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm, IsNg, ResultSource)
    VALUES (1, @cell, 1, 3.490000000, 0.900000, 0, 0.900000, 0, N'FormalTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@taskId, @StageTestTime='2026-10-21T06:00:00.000', @Results=@results;
    IF NOT EXISTS (SELECT 1 FROM dbo.CellOcvSummary WHERE BatteryCode=@cell AND Vol0=3.490000000 AND K1=0.005000000 AND K2=0.005000000)
        RAISERROR(N'Replacing OCV0 did not recompute both adjacent K values.', 16, 1);

    -- Non-positive elapsed time must produce NULL rather than an invalid rate.
    SET @taskCode = @tray + N'-OCV1-REPEAT';
    EXEC dbo.usp_CreateTestTask @TaskCode=@taskCode, @TrayCode=@tray, @OcvStageCode=N'OCV1',
        @TestType=N'Formal', @ChannelCount=1, @OperatorName=N'Verification';
    SELECT @taskId=TestTaskId FROM dbo.TestTask WHERE TaskCode=@taskCode;
    DELETE @results;
    INSERT @results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm, IsNg, ResultSource)
    VALUES (1, @cell, 1, 3.515000000, 1.150000, 0, 1.150000, 0, N'FormalTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@taskId, @StageTestTime='2026-10-21T05:00:00.000', @Results=@results;
    IF NOT EXISTS (SELECT 1 FROM dbo.CellOcvSummary WHERE BatteryCode=@cell AND K1 IS NULL AND K2=0.001666667)
        RAISERROR(N'A non-positive interval was not represented as NULL K1.', 16, 1);

    -- Prepare NG source rows through the normal save path before exercising retest.
    SET @taskCode = @tray + N'-OCV0-NG-FIXTURE';
    EXEC dbo.usp_CreateTestTask @TaskCode=@taskCode, @TrayCode=@tray, @OcvStageCode=N'OCV0',
        @TestType=N'Formal', @ChannelCount=3, @OperatorName=N'Verification';
    SELECT @taskId=TestTaskId FROM dbo.TestTask WHERE TaskCode=@taskCode;
    DELETE @results;
    INSERT @results(ChannelNo, BatteryCode, ExecutionOrder, VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm, IsNg, ResultSource)
    VALUES (2, @ngCell, 2, 3.400000000, 1.500000, 0, 1.500000, 1, N'FormalTest'),
           (3, @auditCell, 3, 3.410000000, 1.600000, 0, 1.600000, 1, N'FormalTest');
    EXEC dbo.usp_SaveTestResults @TestTaskId=@taskId, @StageTestTime='2026-10-21T08:00:00.000', @Results=@results;

    -- An NG retest that overwrites the current result must update the summary.
    DECLARE @ngResultId bigint;
    SELECT @ngResultId=TestResultId FROM dbo.TestResult WHERE BatteryCode=@ngCell;
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@ngCell, @TrayCode=@tray, @ChannelNo=2,
        @TrayPosition=N'1-2', @OriginalVoltage=3.400, @OriginalResistance=1.500,
        @OriginalResult=N'NG', @RetestVoltage=3.425, @RetestResistance=1.250,
        @RetestRawResistanceMilliohm=1.250, @CalibrationValueMilliohm=0,
        @RetestFinalResistanceMilliohm=1.250, @RetestResult=N'OK',
        @RetestTime='2026-10-21T09:30:00.000', @OverwriteOriginal=1,
        @OperatorName=N'Verification', @MeterPort=N'TEST', @SourceTestResultId=@ngResultId,
        @RetestOperationId='11111111-1111-1111-1111-111111111111';
    IF NOT EXISTS (SELECT 1 FROM dbo.CellOcvSummary WHERE BatteryCode=@ngCell AND Vol0=3.425000000 AND Res0=1.250000 AND Time0='2026-10-21T09:30:00.000')
        RAISERROR(N'Overwriting an NG result did not update its OCV summary.', 16, 1);

    -- Audit-only retest must preserve the measured result and summary.
    DECLARE @auditResultId bigint;
    SELECT @auditResultId=TestResultId FROM dbo.TestResult WHERE BatteryCode=@auditCell;
    EXEC dbo.usp_SaveNgRetest @BatteryCode=@auditCell, @TrayCode=@tray, @ChannelNo=3,
        @TrayPosition=N'1-3', @OriginalVoltage=3.410, @OriginalResistance=1.600,
        @OriginalResult=N'NG', @RetestVoltage=3.430, @RetestResistance=1.300,
        @RetestRawResistanceMilliohm=1.300, @CalibrationValueMilliohm=0,
        @RetestFinalResistanceMilliohm=1.300, @RetestResult=N'OK',
        @RetestTime='2026-10-21T09:45:00.000', @OverwriteOriginal=0,
        @OperatorName=N'Verification', @MeterPort=N'TEST', @SourceTestResultId=@auditResultId,
        @RetestOperationId='22222222-2222-2222-2222-222222222222';
    IF NOT EXISTS (SELECT 1 FROM dbo.CellOcvSummary WHERE BatteryCode=@auditCell AND Vol0=3.410000000 AND Res0=1.600000 AND Time0='2026-10-21T08:00:00.000')
        RAISERROR(N'Audit-only NG retest changed the OCV summary.', 16, 1);
    IF NOT EXISTS (SELECT 1 FROM dbo.TestResult WHERE TestResultId=@auditResultId AND VoltageVolt=3.410 AND IsNg=1)
        RAISERROR(N'Audit-only NG retest changed the current TestResult row.', 16, 1);

    ROLLBACK TRANSACTION;
    PRINT N'PASS: staged OCV summary, K1/K2, replacement recomputation, non-positive interval, lookup, and NG retest paths.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    DECLARE @ErrorMessage nvarchar(4000), @ErrorSeverity int, @ErrorState int;
    SELECT @ErrorMessage=ERROR_MESSAGE(), @ErrorSeverity=ERROR_SEVERITY(), @ErrorState=ERROR_STATE();
    RAISERROR(N'%s', @ErrorSeverity, @ErrorState, @ErrorMessage);
END CATCH;
GO
