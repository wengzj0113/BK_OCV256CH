/*
  OCV_DATA verification cases
  Run after:
  - 001_create_ocv_data_schema.sql
  - 002_create_core_procedures.sql

  This script uses a transaction and rolls back at the end.
*/

USE OCV_DATA;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @TrayCode nvarchar(100) = CONCAT(N'TEST-TRAY-', replace(CONVERT(nvarchar(36), NEWID()), N'-', N''));
    DECLARE @TrayId bigint;
    DECLARE @TaskCode nvarchar(100) = CONCAT(N'TEST-TASK-', replace(CONVERT(nvarchar(36), NEWID()), N'-', N''));
    DECLARE @TaskId bigint;
    DECLARE @UploadKey nvarchar(200) = CONCAT(N'UPLOAD-VERIFY-', replace(CONVERT(nvarchar(36), NEWID()), N'-', N''));

    PRINT N'Case 1: register tray';
    EXEC dbo.usp_RegisterTray
        @TrayCode = @TrayCode,
        @SourceType = N'Verification',
        @OperatorName = N'Verification',
        @TrayId = @TrayId OUTPUT;

    IF @TrayId IS NULL
        THROW 51001, 'Tray registration failed.', 1;

    PRINT N'Case 2: save partial tray cells';
    DECLARE @Cells dbo.TrayCellInputType;
    INSERT INTO @Cells(ChannelNo, BatteryCode, SourceType)
    VALUES
        (1, N'CELL-001', N'Verification'),
        (2, N'CELL-002', N'Verification'),
        (8, N'CELL-008', N'Verification'),
        (129, N'CELL-129', N'Verification'),
        (256, N'CELL-256', N'Verification');

    EXEC dbo.usp_SaveTrayCells
        @TrayCode = @TrayCode,
        @Cells = @Cells,
        @OperatorName = N'Verification',
        @WorkstationName = N'LOCAL';

    IF (SELECT COUNT(*) FROM dbo.TrayCell WHERE TrayId = @TrayId AND IsActive = 1) <> 5
        THROW 51002, 'TrayCell count is incorrect after partial save.', 1;

    PRINT N'Case 3: repeat save tray cells must stay idempotent';
    EXEC dbo.usp_SaveTrayCells
        @TrayCode = @TrayCode,
        @Cells = @Cells,
        @OperatorName = N'Verification',
        @WorkstationName = N'LOCAL';

    IF (SELECT COUNT(*) FROM dbo.TrayCell WHERE TrayId = @TrayId AND IsActive = 1) <> 5
        THROW 51003, 'TrayCell duplicate rows were created.', 1;

    PRINT N'Case 4: save full 256-channel tray cells';
    DELETE FROM @Cells;
    DECLARE @i int = 1;
    WHILE @i <= 256
    BEGIN
        INSERT INTO @Cells(ChannelNo, BatteryCode, SourceType)
        VALUES (@i, CONCAT(N'FULL-', RIGHT(CONCAT(N'000', @i), 3)), N'Verification');

        SET @i += 1;
    END

    EXEC dbo.usp_SaveTrayCells
        @TrayCode = @TrayCode,
        @Cells = @Cells,
        @OperatorName = N'Verification',
        @WorkstationName = N'LOCAL';

    IF (SELECT COUNT(*) FROM dbo.TrayCell WHERE TrayId = @TrayId AND IsActive = 1) <> 256
        THROW 51004, 'Full tray save did not produce 256 active cells.', 1;

    PRINT N'Case 5: create test task with idempotency key';
    DECLARE @TaskResult table
    (
        TestTaskId bigint,
        TaskCode nvarchar(100),
        IdempotencyKey nvarchar(200),
        TrayId bigint,
        TrayCode nvarchar(100),
        OcvStageId int,
        TestSpecId int NULL,
        TestType nvarchar(30),
        TestRound int,
        IsRetest bit,
        SelectedChannelsJson nvarchar(max) NULL,
        FirstChannelNo smallint NULL,
        LastChannelNo smallint NULL,
        ChannelCount int,
        EquipmentCode nvarchar(50) NULL,
        WorkstationName nvarchar(100) NULL,
        OperatorName nvarchar(100) NULL,
        Status nvarchar(30),
        StartTime datetime2(0) NULL,
        EndTime datetime2(0) NULL,
        ErrorMessage nvarchar(max) NULL,
        CreatedAt datetime2(0),
        UpdatedAt datetime2(0) NULL,
        RowVersion binary(8)
    );

    INSERT INTO @TaskResult
    EXEC dbo.usp_CreateTestTask
        @TaskCode = @TaskCode,
        @IdempotencyKey = N'VERIFY-IDEMPOTENCY-001',
        @TrayCode = @TrayCode,
        @OcvStageCode = N'OCV0',
        @TestType = N'Formal',
        @SelectedChannelsJson = N'[1,2,8,129,256]',
        @ChannelCount = 5,
        @EquipmentCode = N'XL_OCV_256CH',
        @WorkstationName = N'LOCAL',
        @OperatorName = N'Verification',
        @IsRetest = 0;

    SELECT TOP (1) @TaskId = TestTaskId FROM @TaskResult;

    IF @TaskId IS NULL
        THROW 51005, 'Test task creation failed.', 1;

    DELETE FROM @TaskResult;
    DECLARE @RepeatTaskCode nvarchar(100) = CONCAT(@TaskCode, N'-REPEAT');
    INSERT INTO @TaskResult
    EXEC dbo.usp_CreateTestTask
        @TaskCode = @RepeatTaskCode,
        @IdempotencyKey = N'VERIFY-IDEMPOTENCY-001',
        @TrayCode = @TrayCode,
        @OcvStageCode = N'OCV0',
        @TestType = N'Formal',
        @SelectedChannelsJson = N'[1,2,8,129,256]',
        @ChannelCount = 5,
        @EquipmentCode = N'XL_OCV_256CH',
        @WorkstationName = N'LOCAL',
        @OperatorName = N'Verification',
        @IsRetest = 0;

    IF (SELECT TOP (1) TestTaskId FROM @TaskResult) <> @TaskId
        THROW 51006, 'Test task idempotency failed.', 1;

    PRINT N'Case 6: save calibration and test results';
    EXEC dbo.usp_SaveChannelCalibration
        @ChannelNo = 1,
        @StandardValue = 1.000000,
        @MeasuredValue = 0.990000,
        @CalibrationValueMilliohm = 0.010000,
        @OperatorName = N'Verification',
        @Remark = N'Verification calibration';

    DECLARE @Results dbo.TestResultInputType;
    INSERT INTO @Results
    (
        ChannelNo, BatteryCode, ExecutionOrder,
        VoltageVolt, RawResistanceMilliohm,
        CalibrationValueMilliohm, FinalResistanceMilliohm,
        IsNg, NgReason, ResultSource
    )
    VALUES
        (1, NULL, 1, 3.701000000, 1.000000, NULL, NULL, 0, NULL, N'FormalTest'),
        (2, NULL, 2, 3.702000000, 1.100000, 0.000000, 1.100000, 0, NULL, N'FormalTest'),
        (8, NULL, 3, 3.703000000, 1.200000, 0.000000, 1.200000, 0, NULL, N'FormalTest'),
        (129, NULL, 4, 3.704000000, 1.300000, 0.000000, 1.300000, 0, NULL, N'FormalTest'),
        (256, NULL, 5, 3.705000000, 1.400000, 0.000000, 1.400000, 0, NULL, N'FormalTest');

    EXEC dbo.usp_SaveTestResults @TestTaskId = @TaskId, @StageTestTime = sysdatetime(), @Results = @Results;

    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE TestTaskId = @TaskId) <> 5
        THROW 51007, 'TestResult count is incorrect.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.TestResult
        WHERE TestTaskId = @TaskId
          AND ChannelNo = 1
          AND FinalResistanceMilliohm = 1.010000
    )
        THROW 51008, 'Calibration value was not applied to final resistance.', 1;

    PRINT N'Case 7: repeat save test results must stay idempotent';
    EXEC dbo.usp_SaveTestResults @TestTaskId = @TaskId, @StageTestTime = sysdatetime(), @Results = @Results;

    IF (SELECT COUNT(*) FROM dbo.TestResult WHERE TestTaskId = @TaskId) <> 5
        THROW 51009, 'Duplicate test results were created.', 1;

    PRINT N'Case 8: complete task and query latest stage results';
    EXEC dbo.usp_CompleteTestTask @TestTaskId = @TaskId, @Status = N'Completed';

    IF NOT EXISTS (SELECT 1 FROM dbo.TestTask WHERE TestTaskId = @TaskId AND Status = N'Completed')
        THROW 51010, 'Test task was not completed.', 1;

    EXEC dbo.usp_GetTrayLatestStageResults @TrayCode = @TrayCode, @OcvStageCode = N'OCV0';

    PRINT N'Case 9: upload outbox idempotency and lock';
    EXEC dbo.usp_EnqueueUpload
        @IdempotencyKey = @UploadKey,
        @UploadType = N'TestResult',
        @BusinessKey = @TrayCode,
        @TestTaskId = @TaskId,
        @PayloadJson = N'{"ok":true}';

    EXEC dbo.usp_EnqueueUpload
        @IdempotencyKey = @UploadKey,
        @UploadType = N'TestResult',
        @BusinessKey = @TrayCode,
        @TestTaskId = @TaskId,
        @PayloadJson = N'{"ok":true}';

    IF (SELECT COUNT(*) FROM dbo.UploadOutbox WHERE IdempotencyKey = @UploadKey) <> 1
        THROW 51011, 'UploadOutbox idempotency failed.', 1;

    EXEC dbo.usp_GetPendingUploads @BatchSize = 10, @LockedBy = N'Verification', @LockSeconds = 120;

    IF NOT EXISTS (SELECT 1 FROM dbo.UploadOutbox WHERE IdempotencyKey = @UploadKey AND Status = N'Uploading')
        THROW 51012, 'UploadOutbox lock failed.', 1;

    PRINT N'Case 10: health check';
    EXEC dbo.usp_CheckDatabaseHealth;

    PRINT N'All verification cases passed. Rolling back verification data.';
    ROLLBACK TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    DECLARE @Message nvarchar(4000) = ERROR_MESSAGE();
    DECLARE @Severity int = ERROR_SEVERITY();
    DECLARE @State int = ERROR_STATE();

    RAISERROR(@Message, @Severity, @State);
END CATCH;
GO
