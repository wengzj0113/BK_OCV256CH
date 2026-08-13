/*
  OCV_DATA schema initialization
  Target: SQL Server
  Notes:
  - Run this script first.
  - The script is designed to be re-runnable for missing objects.
  - Existing tables are not dropped or rebuilt.
  - 建议先运行 000_init_database.sql 设置兼容级别/恢复模式
*/

IF DB_ID(N'OCV_DATA') IS NULL
BEGIN
    -- 兜底建库:正常应先运行 000_init_database.sql
    CREATE DATABASE OCV_DATA COLLATE Chinese_PRC_CI_AS;
    PRINT N'[001] OCV_DATA 数据库兜底创建 (建议先运行 000_init_database.sql)';
END
GO

USE OCV_DATA;
GO

IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersion
    (
        SchemaVersionId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaVersion PRIMARY KEY,
        VersionNo nvarchar(50) NOT NULL,
        ScriptName nvarchar(200) NOT NULL,
        AppliedAt datetime2(0) NOT NULL CONSTRAINT DF_SchemaVersion_AppliedAt DEFAULT (sysdatetime()),
        Remark nvarchar(500) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.OcvStage', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OcvStage
    (
        OcvStageId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OcvStage PRIMARY KEY,
        StageCode nvarchar(20) NOT NULL,
        LegacyStageNo int NULL,
        LegacyColumnSuffix nvarchar(10) NULL,
        DisplayName nvarchar(50) NOT NULL,
        MesStageCode nvarchar(50) NULL,
        SortOrder int NOT NULL CONSTRAINT DF_OcvStage_SortOrder DEFAULT (0),
        IsActive bit NOT NULL CONSTRAINT DF_OcvStage_IsActive DEFAULT (1),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_OcvStage_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT UQ_OcvStage_StageCode UNIQUE (StageCode),
        CONSTRAINT CK_OcvStage_StageCode_NotBlank CHECK (ltrim(rtrim(StageCode)) <> N'')
    );
END
GO

IF OBJECT_ID(N'dbo.Equipment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Equipment
    (
        EquipmentId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Equipment PRIMARY KEY,
        EquipmentCode nvarchar(50) NOT NULL,
        EquipmentName nvarchar(100) NULL,
        StationCode nvarchar(50) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Equipment_IsActive DEFAULT (1),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Equipment_CreatedAt DEFAULT (sysdatetime()),
        UpdatedAt datetime2(0) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT UQ_Equipment_EquipmentCode UNIQUE (EquipmentCode)
    );
END
GO

IF OBJECT_ID(N'dbo.SystemSetting', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemSetting
    (
        SettingKey nvarchar(100) NOT NULL CONSTRAINT PK_SystemSetting PRIMARY KEY,
        SettingValue nvarchar(max) NULL,
        SettingGroup nvarchar(50) NULL,
        Description nvarchar(500) NULL,
        UpdatedAt datetime2(0) NOT NULL CONSTRAINT DF_SystemSetting_UpdatedAt DEFAULT (sysdatetime()),
        UpdatedBy nvarchar(100) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Tray', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tray
    (
        TrayId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tray PRIMARY KEY,
        TrayCode nvarchar(100) NOT NULL,
        SourceType nvarchar(30) NOT NULL CONSTRAINT DF_Tray_SourceType DEFAULT (N'Manual'),
        TrayStatus nvarchar(30) NOT NULL CONSTRAINT DF_Tray_TrayStatus DEFAULT (N'Created'),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_Tray_CreatedAt DEFAULT (sysdatetime()),
        UpdatedAt datetime2(0) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT UQ_Tray_TrayCode UNIQUE (TrayCode),
        CONSTRAINT CK_Tray_TrayCode_NotBlank CHECK (ltrim(rtrim(TrayCode)) <> N'')
    );
END
GO

IF OBJECT_ID(N'dbo.TrayCell', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrayCell
    (
        TrayCellId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_TrayCell PRIMARY KEY,
        TrayId bigint NOT NULL,
        ChannelNo smallint NOT NULL,
        RowNo tinyint NOT NULL,
        ColNo tinyint NOT NULL,
        BatteryCode nvarchar(100) NULL,
        SourceType nvarchar(30) NOT NULL CONSTRAINT DF_TrayCell_SourceType DEFAULT (N'ManualImport'),
        IsActive bit NOT NULL CONSTRAINT DF_TrayCell_IsActive DEFAULT (1),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TrayCell_CreatedAt DEFAULT (sysdatetime()),
        UpdatedAt datetime2(0) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT FK_TrayCell_Tray FOREIGN KEY (TrayId) REFERENCES dbo.Tray(TrayId),
        CONSTRAINT CK_TrayCell_ChannelNo CHECK (ChannelNo BETWEEN 1 AND 256),
        CONSTRAINT CK_TrayCell_RowNo CHECK (RowNo BETWEEN 1 AND 16),
        CONSTRAINT CK_TrayCell_ColNo CHECK (ColNo BETWEEN 1 AND 16),
        CONSTRAINT CK_TrayCell_BatteryCode_NotBlank CHECK (BatteryCode IS NULL OR ltrim(rtrim(BatteryCode)) <> N'')
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TrayCell_Active_Channel' AND object_id = OBJECT_ID(N'dbo.TrayCell'))
BEGIN
    CREATE UNIQUE INDEX UX_TrayCell_Active_Channel ON dbo.TrayCell(TrayId, ChannelNo) WHERE IsActive = 1;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TrayCell_BatteryCode' AND object_id = OBJECT_ID(N'dbo.TrayCell'))
BEGIN
    CREATE INDEX IX_TrayCell_BatteryCode ON dbo.TrayCell(BatteryCode) WHERE BatteryCode IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.TestSpec', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TestSpec
    (
        TestSpecId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestSpec PRIMARY KEY,
        SpecName nvarchar(100) NOT NULL,
        OcvStageId int NULL,
        EffectiveFrom datetime2(0) NOT NULL CONSTRAINT DF_TestSpec_EffectiveFrom DEFAULT (sysdatetime()),
        EffectiveTo datetime2(0) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_TestSpec_IsActive DEFAULT (1),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TestSpec_CreatedAt DEFAULT (sysdatetime()),
        CreatedBy nvarchar(100) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT FK_TestSpec_OcvStage FOREIGN KEY (OcvStageId) REFERENCES dbo.OcvStage(OcvStageId),
        CONSTRAINT UQ_TestSpec_SpecName UNIQUE (SpecName)
    );
END
GO

IF OBJECT_ID(N'dbo.TestSpecLimit', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TestSpecLimit
    (
        TestSpecLimitId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestSpecLimit PRIMARY KEY,
        TestSpecId int NOT NULL,
        MeasurementType nvarchar(20) NOT NULL,
        LowerLimit decimal(18,9) NULL,
        UpperLimit decimal(18,9) NULL,
        UnitName nvarchar(20) NOT NULL,
        NgCode nvarchar(50) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TestSpecLimit_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT FK_TestSpecLimit_TestSpec FOREIGN KEY (TestSpecId) REFERENCES dbo.TestSpec(TestSpecId),
        CONSTRAINT CK_TestSpecLimit_MeasurementType CHECK (MeasurementType IN (N'Voltage', N'Resistance')),
        CONSTRAINT CK_TestSpecLimit_Range CHECK (LowerLimit IS NULL OR UpperLimit IS NULL OR LowerLimit <= UpperLimit)
    );
END
GO

IF OBJECT_ID(N'dbo.TestTask', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TestTask
    (
        TestTaskId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestTask PRIMARY KEY,
        TaskCode nvarchar(100) NOT NULL,
        IdempotencyKey nvarchar(200) NULL,
        TrayId bigint NOT NULL,
        TrayCode nvarchar(100) NOT NULL,
        OcvStageId int NOT NULL,
        TestSpecId int NULL,
        TestType nvarchar(30) NOT NULL,
        TestRound int NOT NULL CONSTRAINT DF_TestTask_TestRound DEFAULT (1),
        IsRetest bit NOT NULL CONSTRAINT DF_TestTask_IsRetest DEFAULT (0),
        SelectedChannelsJson nvarchar(max) NULL,
        FirstChannelNo smallint NULL,
        LastChannelNo smallint NULL,
        ChannelCount int NOT NULL,
        EquipmentCode nvarchar(50) NULL,
        WorkstationName nvarchar(100) NULL,
        OperatorName nvarchar(100) NULL,
        Status nvarchar(30) NOT NULL CONSTRAINT DF_TestTask_Status DEFAULT (N'Created'),
        StartTime datetime2(0) NULL,
        EndTime datetime2(0) NULL,
        ErrorMessage nvarchar(max) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TestTask_CreatedAt DEFAULT (sysdatetime()),
        UpdatedAt datetime2(0) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT FK_TestTask_Tray FOREIGN KEY (TrayId) REFERENCES dbo.Tray(TrayId),
        CONSTRAINT FK_TestTask_OcvStage FOREIGN KEY (OcvStageId) REFERENCES dbo.OcvStage(OcvStageId),
        CONSTRAINT FK_TestTask_TestSpec FOREIGN KEY (TestSpecId) REFERENCES dbo.TestSpec(TestSpecId),
        CONSTRAINT UQ_TestTask_TaskCode UNIQUE (TaskCode),
        CONSTRAINT CK_TestTask_ChannelCount CHECK (ChannelCount BETWEEN 0 AND 256),
        CONSTRAINT CK_TestTask_ChannelRange CHECK
        (
            (FirstChannelNo IS NULL AND LastChannelNo IS NULL)
            OR
            (FirstChannelNo BETWEEN 1 AND 256 AND LastChannelNo BETWEEN 1 AND 256 AND FirstChannelNo <= LastChannelNo)
        ),
        CONSTRAINT CK_TestTask_SelectedChannelsJson CHECK (SelectedChannelsJson IS NULL OR isjson(SelectedChannelsJson) = 1),
        CONSTRAINT CK_TestTask_Status CHECK (Status IN (N'Created', N'Running', N'Completed', N'Failed', N'Canceled'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TestTask_IdempotencyKey' AND object_id = OBJECT_ID(N'dbo.TestTask'))
BEGIN
    CREATE UNIQUE INDEX UX_TestTask_IdempotencyKey ON dbo.TestTask(IdempotencyKey) WHERE IdempotencyKey IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestTask_TrayStage' AND object_id = OBJECT_ID(N'dbo.TestTask'))
BEGIN
    CREATE INDEX IX_TestTask_TrayStage ON dbo.TestTask(TrayId, OcvStageId, TestRound DESC, CreatedAt DESC);
END
GO

IF OBJECT_ID(N'dbo.TestResult', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TestResult
    (
        TestResultId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestResult PRIMARY KEY,
        TestTaskId bigint NOT NULL,
        TrayId bigint NOT NULL,
        TrayCode nvarchar(100) NOT NULL,
        OcvStageId int NOT NULL,
        ChannelNo smallint NOT NULL,
        BatteryCode nvarchar(100) NULL,
        ExecutionOrder int NULL,
        VoltageVolt decimal(18,9) NULL,
        RawResistanceMilliohm decimal(18,6) NULL,
        CalibrationValueMilliohm decimal(18,6) NULL,
        FinalResistanceMilliohm decimal(18,6) NULL,
        VoltageLowerLimit decimal(18,9) NULL,
        VoltageUpperLimit decimal(18,9) NULL,
        ResistanceLowerLimit decimal(18,6) NULL,
        ResistanceUpperLimit decimal(18,6) NULL,
        IsNg bit NOT NULL CONSTRAINT DF_TestResult_IsNg DEFAULT (0),
        NgReason nvarchar(500) NULL,
        ResultSource nvarchar(30) NOT NULL CONSTRAINT DF_TestResult_ResultSource DEFAULT (N'FormalTest'),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TestResult_CreatedAt DEFAULT (sysdatetime()),
        UpdatedAt datetime2(0) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT FK_TestResult_TestTask FOREIGN KEY (TestTaskId) REFERENCES dbo.TestTask(TestTaskId),
        CONSTRAINT FK_TestResult_Tray FOREIGN KEY (TrayId) REFERENCES dbo.Tray(TrayId),
        CONSTRAINT FK_TestResult_OcvStage FOREIGN KEY (OcvStageId) REFERENCES dbo.OcvStage(OcvStageId),
        CONSTRAINT CK_TestResult_ChannelNo CHECK (ChannelNo BETWEEN 1 AND 256)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TestResult_Task_Channel' AND object_id = OBJECT_ID(N'dbo.TestResult'))
BEGIN
    CREATE UNIQUE INDEX UX_TestResult_Task_Channel ON dbo.TestResult(TestTaskId, ChannelNo);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TestResult_TrayStageChannel' AND object_id = OBJECT_ID(N'dbo.TestResult'))
BEGIN
    CREATE INDEX IX_TestResult_TrayStageChannel ON dbo.TestResult(TrayId, OcvStageId, ChannelNo, CreatedAt DESC);
END
GO

IF OBJECT_ID(N'dbo.RetestTask', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RetestTask
    (
        RetestTaskId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_RetestTask PRIMARY KEY,
        ParentTestTaskId bigint NOT NULL,
        RetestTestTaskId bigint NOT NULL,
        Reason nvarchar(500) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_RetestTask_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT FK_RetestTask_Parent FOREIGN KEY (ParentTestTaskId) REFERENCES dbo.TestTask(TestTaskId),
        CONSTRAINT FK_RetestTask_Retest FOREIGN KEY (RetestTestTaskId) REFERENCES dbo.TestTask(TestTaskId)
    );
END
GO

IF OBJECT_ID(N'dbo.ChannelCalibration', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChannelCalibration
    (
        ChannelCalibrationId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChannelCalibration PRIMARY KEY,
        ChannelNo smallint NOT NULL,
        MeasurementType nvarchar(20) NOT NULL CONSTRAINT DF_ChannelCalibration_MeasurementType DEFAULT (N'Resistance'),
        StandardValue decimal(18,6) NULL,
        MeasuredValue decimal(18,6) NULL,
        CalibrationValueMilliohm decimal(18,6) NOT NULL,
        EffectiveFrom datetime2(0) NOT NULL CONSTRAINT DF_ChannelCalibration_EffectiveFrom DEFAULT (sysdatetime()),
        EffectiveTo datetime2(0) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_ChannelCalibration_IsActive DEFAULT (1),
        OperatorName nvarchar(100) NULL,
        Remark nvarchar(500) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ChannelCalibration_CreatedAt DEFAULT (sysdatetime()),
        RowVersion rowversion NOT NULL,
        CONSTRAINT CK_ChannelCalibration_ChannelNo CHECK (ChannelNo BETWEEN 1 AND 256),
        CONSTRAINT CK_ChannelCalibration_MeasurementType CHECK (MeasurementType IN (N'Resistance'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ChannelCalibration_Active' AND object_id = OBJECT_ID(N'dbo.ChannelCalibration'))
BEGIN
    CREATE UNIQUE INDEX UX_ChannelCalibration_Active ON dbo.ChannelCalibration(ChannelNo, MeasurementType) WHERE IsActive = 1;
END
GO

IF OBJECT_ID(N'dbo.GradeScheme', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GradeScheme
    (
        GradeSchemeId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GradeScheme PRIMARY KEY,
        SchemeName nvarchar(100) NOT NULL,
        OcvStageId int NULL,
        IsActive bit NOT NULL CONSTRAINT DF_GradeScheme_IsActive DEFAULT (1),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_GradeScheme_CreatedAt DEFAULT (sysdatetime()),
        CreatedBy nvarchar(100) NULL,
        UpdatedAt datetime2(0) NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT FK_GradeScheme_OcvStage FOREIGN KEY (OcvStageId) REFERENCES dbo.OcvStage(OcvStageId),
        CONSTRAINT UQ_GradeScheme_SchemeName UNIQUE (SchemeName)
    );
END
GO

IF OBJECT_ID(N'dbo.GradeRule', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GradeRule
    (
        GradeRuleId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_GradeRule PRIMARY KEY,
        GradeSchemeId int NOT NULL,
        GradeCode nvarchar(50) NOT NULL,
        FieldName nvarchar(50) NOT NULL,
        LowerLimit decimal(18,9) NULL,
        UpperLimit decimal(18,9) NULL,
        SortOrder int NOT NULL CONSTRAINT DF_GradeRule_SortOrder DEFAULT (0),
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_GradeRule_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT FK_GradeRule_GradeScheme FOREIGN KEY (GradeSchemeId) REFERENCES dbo.GradeScheme(GradeSchemeId),
        CONSTRAINT CK_GradeRule_Range CHECK (LowerLimit IS NULL OR UpperLimit IS NULL OR LowerLimit <= UpperLimit)
    );
END
GO

IF OBJECT_ID(N'dbo.GradeSelectableField', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GradeSelectableField
    (
        FieldName nvarchar(50) NOT NULL CONSTRAINT PK_GradeSelectableField PRIMARY KEY,
        DisplayName nvarchar(100) NOT NULL,
        UnitName nvarchar(20) NULL,
        IsActive bit NOT NULL CONSTRAINT DF_GradeSelectableField_IsActive DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.UploadOutbox', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UploadOutbox
    (
        UploadOutboxId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_UploadOutbox PRIMARY KEY,
        IdempotencyKey nvarchar(200) NOT NULL,
        UploadType nvarchar(50) NOT NULL,
        BusinessKey nvarchar(200) NOT NULL,
        TestTaskId bigint NULL,
        PayloadJson nvarchar(max) NOT NULL,
        Status nvarchar(30) NOT NULL CONSTRAINT DF_UploadOutbox_Status DEFAULT (N'Pending'),
        RetryCount int NOT NULL CONSTRAINT DF_UploadOutbox_RetryCount DEFAULT (0),
        NextRetryAt datetime2(0) NOT NULL CONSTRAINT DF_UploadOutbox_NextRetryAt DEFAULT (sysdatetime()),
        LockedBy nvarchar(100) NULL,
        LockedAt datetime2(0) NULL,
        LastError nvarchar(max) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_UploadOutbox_CreatedAt DEFAULT (sysdatetime()),
        UpdatedAt datetime2(0) NULL,
        SentAt datetime2(0) NULL,
        CONSTRAINT FK_UploadOutbox_TestTask FOREIGN KEY (TestTaskId) REFERENCES dbo.TestTask(TestTaskId),
        CONSTRAINT UQ_UploadOutbox_IdempotencyKey UNIQUE (IdempotencyKey),
        CONSTRAINT CK_UploadOutbox_PayloadJson CHECK (isjson(PayloadJson) = 1),
        CONSTRAINT CK_UploadOutbox_Status CHECK (Status IN (N'Pending', N'Uploading', N'Sent', N'Failed', N'Canceled'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UploadOutbox_Pending' AND object_id = OBJECT_ID(N'dbo.UploadOutbox'))
BEGIN
    CREATE INDEX IX_UploadOutbox_Pending ON dbo.UploadOutbox(Status, NextRetryAt, CreatedAt) INCLUDE (RetryCount, LockedBy, LockedAt);
END
GO

IF OBJECT_ID(N'dbo.MesUploadLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MesUploadLog
    (
        MesUploadLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_MesUploadLog PRIMARY KEY,
        UploadOutboxId bigint NULL,
        TestTaskId bigint NULL,
        UploadType nvarchar(50) NOT NULL,
        BusinessKey nvarchar(200) NULL,
        RequestJson nvarchar(max) NULL,
        ResponseJson nvarchar(max) NULL,
        IsSuccess bit NOT NULL,
        ErrorMessage nvarchar(max) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_MesUploadLog_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT FK_MesUploadLog_UploadOutbox FOREIGN KEY (UploadOutboxId) REFERENCES dbo.UploadOutbox(UploadOutboxId),
        CONSTRAINT FK_MesUploadLog_TestTask FOREIGN KEY (TestTaskId) REFERENCES dbo.TestTask(TestTaskId)
    );
END
GO

IF OBJECT_ID(N'dbo.DeviceCommLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DeviceCommLog
    (
        DeviceCommLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_DeviceCommLog PRIMARY KEY,
        TestTaskId bigint NULL,
        DeviceName nvarchar(50) NOT NULL,
        Direction nvarchar(10) NOT NULL,
        CommandName nvarchar(100) NULL,
        FrameHex nvarchar(max) NULL,
        IsSuccess bit NOT NULL CONSTRAINT DF_DeviceCommLog_IsSuccess DEFAULT (1),
        ErrorMessage nvarchar(max) NULL,
        CreatedAt datetime2(3) NOT NULL CONSTRAINT DF_DeviceCommLog_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT FK_DeviceCommLog_TestTask FOREIGN KEY (TestTaskId) REFERENCES dbo.TestTask(TestTaskId),
        CONSTRAINT CK_DeviceCommLog_Direction CHECK (Direction IN (N'TX', N'RX', N'INFO', N'ERROR'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DeviceCommLog_TaskTime' AND object_id = OBJECT_ID(N'dbo.DeviceCommLog'))
BEGIN
    CREATE INDEX IX_DeviceCommLog_TaskTime ON dbo.DeviceCommLog(TestTaskId, CreatedAt);
END
GO

IF OBJECT_ID(N'dbo.AlarmEvent', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AlarmEvent
    (
        AlarmEventId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AlarmEvent PRIMARY KEY,
        AlarmCode nvarchar(50) NULL,
        AlarmMessage nvarchar(500) NOT NULL,
        Severity nvarchar(20) NOT NULL CONSTRAINT DF_AlarmEvent_Severity DEFAULT (N'Error'),
        IsCleared bit NOT NULL CONSTRAINT DF_AlarmEvent_IsCleared DEFAULT (0),
        RaisedAt datetime2(0) NOT NULL CONSTRAINT DF_AlarmEvent_RaisedAt DEFAULT (sysdatetime()),
        ClearedAt datetime2(0) NULL,
        OperatorName nvarchar(100) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.EnergySnapshot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EnergySnapshot
    (
        EnergySnapshotId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_EnergySnapshot PRIMARY KEY,
        EquipmentCode nvarchar(50) NULL,
        SnapshotJson nvarchar(max) NOT NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_EnergySnapshot_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT CK_EnergySnapshot_Json CHECK (isjson(SnapshotJson) = 1)
    );
END
GO

IF OBJECT_ID(N'dbo.OperatorLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OperatorLog
    (
        OperatorLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_OperatorLog PRIMARY KEY,
        OperatorName nvarchar(100) NULL,
        ActionName nvarchar(100) NOT NULL,
        Detail nvarchar(max) NULL,
        WorkstationName nvarchar(100) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_OperatorLog_CreatedAt DEFAULT (sysdatetime())
    );
END
GO

IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
        EntityName nvarchar(100) NOT NULL,
        EntityKey nvarchar(200) NOT NULL,
        ActionName nvarchar(50) NOT NULL,
        BeforeJson nvarchar(max) NULL,
        AfterJson nvarchar(max) NULL,
        OperatorName nvarchar(100) NULL,
        WorkstationName nvarchar(100) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_AuditLog_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT CK_AuditLog_BeforeJson CHECK (BeforeJson IS NULL OR isjson(BeforeJson) = 1),
        CONSTRAINT CK_AuditLog_AfterJson CHECK (AfterJson IS NULL OR isjson(AfterJson) = 1)
    );
END
GO

IF OBJECT_ID(N'dbo.FileArtifact', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FileArtifact
    (
        FileArtifactId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_FileArtifact PRIMARY KEY,
        ArtifactType nvarchar(50) NOT NULL,
        TestTaskId bigint NULL,
        TrayCode nvarchar(100) NULL,
        FilePath nvarchar(1000) NOT NULL,
        FileHash nvarchar(128) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_FileArtifact_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT FK_FileArtifact_TestTask FOREIGN KEY (TestTaskId) REFERENCES dbo.TestTask(TestTaskId)
    );
END
GO

IF OBJECT_ID(N'dbo.ExternalSyncLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExternalSyncLog
    (
        ExternalSyncLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExternalSyncLog PRIMARY KEY,
        SourceName nvarchar(100) NOT NULL,
        BusinessKey nvarchar(200) NULL,
        RequestJson nvarchar(max) NULL,
        ResponseJson nvarchar(max) NULL,
        IsSuccess bit NOT NULL,
        ErrorMessage nvarchar(max) NULL,
        CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_ExternalSyncLog_CreatedAt DEFAULT (sysdatetime()),
        CONSTRAINT CK_ExternalSyncLog_RequestJson CHECK (RequestJson IS NULL OR isjson(RequestJson) = 1),
        CONSTRAINT CK_ExternalSyncLog_ResponseJson CHECK (ResponseJson IS NULL OR isjson(ResponseJson) = 1)
    );
END
GO

IF TYPE_ID(N'dbo.TrayCellInputType') IS NULL
BEGIN
    EXEC(N'CREATE TYPE dbo.TrayCellInputType AS TABLE
    (
        ChannelNo smallint NOT NULL,
        BatteryCode nvarchar(100) NULL,
        SourceType nvarchar(30) NULL,
        RowNo tinyint NULL,
        ColNo tinyint NULL
    );');
END
GO

IF TYPE_ID(N'dbo.TestResultInputType') IS NULL
BEGIN
    EXEC(N'CREATE TYPE dbo.TestResultInputType AS TABLE
    (
        ChannelNo smallint NOT NULL,
        BatteryCode nvarchar(100) NULL,
        ExecutionOrder int NULL,
        VoltageVolt decimal(18,9) NULL,
        RawResistanceMilliohm decimal(18,6) NULL,
        CalibrationValueMilliohm decimal(18,6) NULL,
        FinalResistanceMilliohm decimal(18,6) NULL,
        IsNg bit NOT NULL,
        NgReason nvarchar(500) NULL,
        ResultSource nvarchar(30) NULL
    );');
END
GO

MERGE dbo.OcvStage AS target
USING
(
    VALUES
        (N'OCV0', 1, N'A', N'OCV0', N'OCV0', 10, 1),
        (N'OCV1', 2, N'B', N'OCV1', N'OCV1', 20, 1),
        (N'OCV2', 3, N'C', N'OCV2', N'OCV2', 30, 1)
) AS source(StageCode, LegacyStageNo, LegacyColumnSuffix, DisplayName, MesStageCode, SortOrder, IsActive)
ON target.StageCode = source.StageCode
WHEN MATCHED THEN
    UPDATE SET
        LegacyStageNo = source.LegacyStageNo,
        LegacyColumnSuffix = source.LegacyColumnSuffix,
        DisplayName = source.DisplayName,
        MesStageCode = source.MesStageCode,
        SortOrder = source.SortOrder,
        IsActive = source.IsActive
WHEN NOT MATCHED THEN
    INSERT (StageCode, LegacyStageNo, LegacyColumnSuffix, DisplayName, MesStageCode, SortOrder, IsActive)
    VALUES (source.StageCode, source.LegacyStageNo, source.LegacyColumnSuffix, source.DisplayName, source.MesStageCode, source.SortOrder, source.IsActive);
GO

MERGE dbo.GradeSelectableField AS target
USING
(
    VALUES
        (N'VoltageVolt', N'Voltage', N'V', 1),
        (N'FinalResistanceMilliohm', N'Resistance', N'mOhm', 1)
) AS source(FieldName, DisplayName, UnitName, IsActive)
ON target.FieldName = source.FieldName
WHEN MATCHED THEN
    UPDATE SET DisplayName = source.DisplayName, UnitName = source.UnitName, IsActive = source.IsActive
WHEN NOT MATCHED THEN
    INSERT (FieldName, DisplayName, UnitName, IsActive)
    VALUES (source.FieldName, source.DisplayName, source.UnitName, source.IsActive);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE VersionNo = N'001')
BEGIN
    INSERT INTO dbo.SchemaVersion(VersionNo, ScriptName, Remark)
    VALUES (N'001', N'001_create_ocv_data_schema.sql', N'Initial OCV_DATA schema.');
END
GO

CREATE OR ALTER VIEW dbo.vw_CellTestSummary
AS
SELECT
    t.TrayCode,
    os.StageCode AS OcvStage,
    tt.TestRound,
    tt.TestType,
    tt.Status AS TaskStatus,
    tr.ChannelNo,
    tc.RowNo,
    tc.ColNo,
    COALESCE(tr.BatteryCode, tc.BatteryCode) AS BatteryCode,
    tr.VoltageVolt,
    tr.RawResistanceMilliohm,
    tr.CalibrationValueMilliohm,
    tr.FinalResistanceMilliohm,
    tr.IsNg,
    tr.NgReason,
    tt.StartTime,
    tt.EndTime,
    tr.CreatedAt AS ResultCreatedAt
FROM dbo.TestResult tr
INNER JOIN dbo.TestTask tt ON tt.TestTaskId = tr.TestTaskId
INNER JOIN dbo.Tray t ON t.TrayId = tr.TrayId
INNER JOIN dbo.OcvStage os ON os.OcvStageId = tr.OcvStageId
LEFT JOIN dbo.TrayCell tc ON tc.TrayId = tr.TrayId AND tc.ChannelNo = tr.ChannelNo AND tc.IsActive = 1;
GO
