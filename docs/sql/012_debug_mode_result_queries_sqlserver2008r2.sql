/*
    OCV_DATA debug result query update for SQL Server 2008 R2.
    Apply this additive procedure migration to an existing OCV_DATA database.
    It changes no tables or result rows.
*/
USE [OCV_DATA]
GO

IF OBJECT_ID(N'dbo.usp_GetTrayLatestStageResults', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetTrayLatestStageResults
GO

CREATE PROCEDURE dbo.usp_GetTrayLatestStageResults
    @TrayCode nvarchar(100),
    @OcvStageCode nvarchar(20) = NULL,
    @DebugMode bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH RankedTask AS
    (
        SELECT
            tt.TestTaskId,
            ROW_NUMBER() OVER
            (
                PARTITION BY tt.TrayId, tt.OcvStageId
                ORDER BY tt.TestRound DESC, tt.EndTime DESC, tt.TestTaskId DESC
            ) AS rn
        FROM dbo.TestTask tt
        INNER JOIN dbo.Tray t ON t.TrayId = tt.TrayId
        INNER JOIN dbo.OcvStage os ON os.OcvStageId = tt.OcvStageId
        WHERE t.TrayCode = @TrayCode
          AND tt.Status = N'Completed'
          AND (@OcvStageCode IS NULL OR os.StageCode = @OcvStageCode)
          AND ((@DebugMode = 1 AND tt.TestType = N'Debug')
               OR (@DebugMode = 0 AND tt.TestType <> N'Debug'))
    )
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
    FROM RankedTask rt
    INNER JOIN dbo.TestTask tt ON tt.TestTaskId = rt.TestTaskId
    INNER JOIN dbo.TestResult tr ON tr.TestTaskId = tt.TestTaskId
    INNER JOIN dbo.Tray t ON t.TrayId = tt.TrayId
    INNER JOIN dbo.OcvStage os ON os.OcvStageId = tt.OcvStageId
    LEFT JOIN dbo.TrayCell tc ON tc.TrayId = tt.TrayId AND tc.ChannelNo = tr.ChannelNo AND tc.IsActive = 1
    WHERE rt.rn = 1
    ORDER BY os.StageCode, tr.ChannelNo;
END
GO

IF OBJECT_ID(N'dbo.usp_GetNgRetestSourceByBatteryCode', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetNgRetestSourceByBatteryCode
GO

CREATE PROCEDURE dbo.usp_GetNgRetestSourceByBatteryCode
    @BatteryCode nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        BatteryCode, TrayCode, ChannelNo,
        VoltageVolt AS OriginalVoltage,
        FinalResistanceMilliohm AS OriginalResistance,
        CASE WHEN IsNg = 1 THEN N'NG' ELSE N'OK' END AS OriginalResult,
        ResultCreatedAt AS OriginalTestTime,
        (CONVERT(nvarchar(10), COALESCE(RowNo, ((ChannelNo - 1) / 16) + 1)) + N'-' +
         CONVERT(nvarchar(10), COALESCE(ColNo, ((ChannelNo - 1) % 16) + 1))) AS TrayPosition
    FROM dbo.vw_CellTestSummary
    WHERE BatteryCode = @BatteryCode
      AND IsNg = 1
      AND TestType <> N'Debug'
    ORDER BY ResultCreatedAt DESC;
END
GO
