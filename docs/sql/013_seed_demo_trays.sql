/*
  Seed two sample trays with 256 battery barcodes each.
  Prerequisites: OCV_DATA schema and dbo.usp_SaveTrayCells from 001/002.
  Re-running this script updates the active channel mappings for TP001/TP002.
*/
USE OCV_DATA;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Tray001 dbo.TrayCellInputType;
DECLARE @Tray002 dbo.TrayCellInputType;

;WITH Channels AS
(
    SELECT CONVERT(int, 1) AS ChannelNo
    UNION ALL
    SELECT ChannelNo + 1
    FROM Channels
    WHERE ChannelNo < 256
)
INSERT INTO @Tray001 (ChannelNo, BatteryCode, SourceType, RowNo, ColNo)
SELECT
    ChannelNo,
    N'BAT01' + RIGHT(N'000' + CONVERT(nvarchar(3), ChannelNo), 3),
    N'ManualImport',
    CONVERT(tinyint, ((ChannelNo - 1) % 16) + 1),
    CONVERT(tinyint, ((ChannelNo - 1) / 16) + 1)
FROM Channels
OPTION (MAXRECURSION 256);

;WITH Channels AS
(
    SELECT CONVERT(int, 1) AS ChannelNo
    UNION ALL
    SELECT ChannelNo + 1
    FROM Channels
    WHERE ChannelNo < 256
)
INSERT INTO @Tray002 (ChannelNo, BatteryCode, SourceType, RowNo, ColNo)
SELECT
    ChannelNo,
    N'BAT02' + RIGHT(N'000' + CONVERT(nvarchar(3), ChannelNo), 3),
    N'ManualImport',
    CONVERT(tinyint, ((ChannelNo - 1) % 16) + 1),
    CONVERT(tinyint, ((ChannelNo - 1) / 16) + 1)
FROM Channels
OPTION (MAXRECURSION 256);

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC dbo.usp_SaveTrayCells @TrayCode = N'TP001', @Cells = @Tray001;
    EXEC dbo.usp_SaveTrayCells @TrayCode = N'TP002', @Cells = @Tray002;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- Verify the tray-to-battery mapping. Expected: 256 rows for each tray.
SELECT
    t.TrayCode,
    tc.ChannelNo,
    tc.BatteryCode
FROM dbo.Tray AS t
INNER JOIN dbo.TrayCell AS tc
    ON tc.TrayId = t.TrayId
   AND tc.IsActive = 1
WHERE t.TrayCode IN (N'TP001', N'TP002')
ORDER BY t.TrayCode, tc.ChannelNo;
GO
