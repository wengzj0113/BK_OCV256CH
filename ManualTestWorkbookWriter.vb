Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports Excel = Microsoft.Office.Interop.Excel

Friend NotInheritable Class ManualTestWorkbookRow
    Public Property ChannelNumber As Integer
    Public Property BatteryCode As String
    Public Property GroupText As String
    Public Property VoltageMillivolts As Double
    Public Property ResistanceMilliohms As Double
    Public Property ResultText As String
End Class

Friend NotInheritable Class ManualTestWorkbookWriter
    Implements IDisposable

    Private Const SummarySheetName As String = "汇总"
    Private Const SummaryMaximumRows As Integer = 257

    Private ReadOnly outputPath As String
    Private ReadOnly batchDate As DateTime
    Private ReadOnly excelApplication As Excel.Application
    Private ReadOnly workbook As Excel.Workbook
    Private ReadOnly summaryWorksheet As Excel.Worksheet
    Private ReadOnly completedRows As New List(Of ManualTestWorkbookRow)()
    Private completedRoundCount As Integer
    Private hasSavedFile As Boolean
    Private isDisposed As Boolean

    Public Sub New(outputPath As String, batchDate As DateTime)
        Me.outputPath = outputPath
        Me.batchDate = batchDate

        Dim outputDirectory As String = Path.GetDirectoryName(outputPath)
        If Not String.IsNullOrWhiteSpace(outputDirectory) Then
            Directory.CreateDirectory(outputDirectory)
        End If

        Try
            excelApplication = New Excel.Application()
            excelApplication.Visible = False
            excelApplication.DisplayAlerts = False
            workbook = excelApplication.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet)

            Dim worksheets As Excel.Sheets = Nothing
            Try
                worksheets = workbook.Worksheets
                summaryWorksheet = CType(worksheets(1), Excel.Worksheet)
                summaryWorksheet.Name = SummarySheetName
            Finally
                ReleaseComObject(worksheets)
            End Try
        Catch
            Dispose()
            Throw
        End Try
    End Sub

    Public Sub AddRound(roundNumber As Integer,
                        capturedAt As DateTime,
                        rows As IList(Of ManualTestWorkbookRow))
        If isDisposed Then Throw New ObjectDisposedException(NameOf(ManualTestWorkbookWriter))
        If rows Is Nothing OrElse rows.Count = 0 Then
            Throw New ArgumentException("每轮至少需要一条通道结果。", NameOf(rows))
        End If

        Dim candidateRows As List(Of ManualTestWorkbookRow) = completedRows.Concat(rows).ToList()
        Dim worksheets As Excel.Sheets = Nothing
        Dim lastWorksheet As Excel.Worksheet = Nothing
        Dim roundWorksheet As Excel.Worksheet = Nothing

        Try
            worksheets = workbook.Worksheets
            lastWorksheet = CType(worksheets(worksheets.Count), Excel.Worksheet)
            roundWorksheet = CType(worksheets.Add(After:=lastWorksheet), Excel.Worksheet)
            roundWorksheet.Name = batchDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) &
                                  "第" & roundNumber.ToString(CultureInfo.InvariantCulture) & "次"

            WriteRoundWorksheet(roundWorksheet, capturedAt, rows)
            WriteSummary(candidateRows, completedRoundCount + 1)
            SaveWorkbook()

            completedRows.AddRange(rows)
            completedRoundCount += 1
        Finally
            ReleaseComObject(roundWorksheet)
            ' The first round's last worksheet is the summary retained for later rounds.
            If Not Object.ReferenceEquals(lastWorksheet, summaryWorksheet) Then
                ReleaseComObject(lastWorksheet)
            End If
            ReleaseComObject(worksheets)
        End Try
    End Sub

    Private Shared Sub WriteRoundWorksheet(worksheet As Excel.Worksheet,
                                           capturedAt As DateTime,
                                           rows As IList(Of ManualTestWorkbookRow))
        Dim values(rows.Count, 6) As Object
        values(0, 0) = "通道"
        values(0, 1) = "分组"
        values(0, 2) = "采集时间"
        values(0, 3) = "电压 mV"
        values(0, 4) = "内阻 mΩ"
        values(0, 5) = "判定"
        values(0, 6) = "电芯条码"

        For index As Integer = 0 To rows.Count - 1
            Dim row As ManualTestWorkbookRow = rows(index)
            values(index + 1, 0) = "CH" & row.ChannelNumber.ToString(CultureInfo.InvariantCulture)
            values(index + 1, 1) = row.GroupText
            values(index + 1, 2) = capturedAt
            values(index + 1, 3) = row.VoltageMillivolts
            values(index + 1, 4) = row.ResistanceMilliohms
            values(index + 1, 5) = row.ResultText
            values(index + 1, 6) = row.BatteryCode
        Next

        Dim allCells As Excel.Range = Nothing
        Dim topLeftCell As Excel.Range = Nothing
        Dim headerRange As Excel.Range = Nothing
        Dim numericRange As Excel.Range = Nothing
        Dim timestampRange As Excel.Range = Nothing
        Dim barcodeRange As Excel.Range = Nothing
        Dim columnRange As Excel.Range = Nothing
        Try
            topLeftCell = CType(worksheet.Range("A1"), Excel.Range)
            allCells = CType(topLeftCell.Resize(rows.Count + 1, 7), Excel.Range)
            Dim lastRow As String = (rows.Count + 1).ToString(CultureInfo.InvariantCulture)
            barcodeRange = CType(worksheet.Range("G2", "G" & lastRow), Excel.Range)
            barcodeRange.NumberFormat = "@"
            allCells.Value2 = values

            headerRange = CType(worksheet.Range("A1", "G1"), Excel.Range)
            headerRange.Font.Bold = True

            numericRange = CType(worksheet.Range("D2", "E" & lastRow), Excel.Range)
            numericRange.NumberFormat = "0.0000"
            timestampRange = CType(worksheet.Range("C2", "C" & lastRow), Excel.Range)
            timestampRange.NumberFormat = "yyyy-mm-dd hh:mm:ss.000"

            columnRange = CType(allCells.Columns, Excel.Range)
            columnRange.AutoFit()
        Finally
            ReleaseComObject(columnRange)
            ReleaseComObject(barcodeRange)
            ReleaseComObject(timestampRange)
            ReleaseComObject(numericRange)
            ReleaseComObject(headerRange)
            ReleaseComObject(allCells)
            ReleaseComObject(topLeftCell)
        End Try
    End Sub

    Private Sub WriteSummary(rows As IList(Of ManualTestWorkbookRow), roundCount As Integer)
        Dim groupedRows = rows.GroupBy(Function(row) row.ChannelNumber).OrderBy(Function(group) group.Key).ToList()
        Dim values(groupedRows.Count, 9) As Object

        values(0, 0) = "通道"
        values(0, 1) = "分组"
        values(0, 2) = "统计轮数"
        values(0, 3) = "电压最小值 mV"
        values(0, 4) = "电压最大值 mV"
        values(0, 5) = "电压偏离度 mV"
        values(0, 6) = "内阻最小值 mΩ"
        values(0, 7) = "内阻最大值 mΩ"
        values(0, 8) = "内阻偏离度 mΩ"
        values(0, 9) = "电芯条码"

        For index As Integer = 0 To groupedRows.Count - 1
            Dim channelRows As List(Of ManualTestWorkbookRow) = groupedRows(index).ToList()
            Dim voltageMinimum As Double = channelRows.Min(Function(row) row.VoltageMillivolts)
            Dim voltageMaximum As Double = channelRows.Max(Function(row) row.VoltageMillivolts)
            Dim resistanceMinimum As Double = channelRows.Min(Function(row) row.ResistanceMilliohms)
            Dim resistanceMaximum As Double = channelRows.Max(Function(row) row.ResistanceMilliohms)

            values(index + 1, 0) = "CH" & groupedRows(index).Key.ToString(CultureInfo.InvariantCulture)
            values(index + 1, 1) = channelRows(0).GroupText
            values(index + 1, 2) = roundCount
            values(index + 1, 3) = voltageMinimum
            values(index + 1, 4) = voltageMaximum
            values(index + 1, 5) = voltageMaximum - voltageMinimum
            values(index + 1, 6) = resistanceMinimum
            values(index + 1, 7) = resistanceMaximum
            values(index + 1, 8) = resistanceMaximum - resistanceMinimum
            values(index + 1, 9) = channelRows(0).BatteryCode
        Next

        Dim clearRange As Excel.Range = Nothing
        Dim allCells As Excel.Range = Nothing
        Dim topLeftCell As Excel.Range = Nothing
        Dim headerRange As Excel.Range = Nothing
        Dim numericRange As Excel.Range = Nothing
        Dim barcodeRange As Excel.Range = Nothing
        Dim columnRange As Excel.Range = Nothing
        Try
            clearRange = CType(summaryWorksheet.Range("A1", "J" & SummaryMaximumRows.ToString(CultureInfo.InvariantCulture)), Excel.Range)
            clearRange.ClearContents()

            topLeftCell = CType(summaryWorksheet.Range("A1"), Excel.Range)
            allCells = CType(topLeftCell.Resize(groupedRows.Count + 1, 10), Excel.Range)
            If groupedRows.Count > 0 Then
                Dim lastRow As String = (groupedRows.Count + 1).ToString(CultureInfo.InvariantCulture)
                barcodeRange = CType(summaryWorksheet.Range("J2", "J" & lastRow), Excel.Range)
                barcodeRange.NumberFormat = "@"
            End If
            allCells.Value2 = values
            headerRange = CType(summaryWorksheet.Range("A1", "J1"), Excel.Range)
            headerRange.Font.Bold = True

            If groupedRows.Count > 0 Then
                Dim lastRow As String = (groupedRows.Count + 1).ToString(CultureInfo.InvariantCulture)
                numericRange = CType(summaryWorksheet.Range("D2", "I" & lastRow), Excel.Range)
                numericRange.NumberFormat = "0.0000"
            End If
            columnRange = CType(allCells.Columns, Excel.Range)
            columnRange.AutoFit()
        Finally
            ReleaseComObject(columnRange)
            ReleaseComObject(barcodeRange)
            ReleaseComObject(numericRange)
            ReleaseComObject(headerRange)
            ReleaseComObject(allCells)
            ReleaseComObject(topLeftCell)
            ReleaseComObject(clearRange)
        End Try
    End Sub

    Private Sub SaveWorkbook()
        If hasSavedFile Then
            workbook.Save()
        Else
            workbook.SaveAs(outputPath, Excel.XlFileFormat.xlOpenXMLWorkbook)
            hasSavedFile = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If isDisposed Then Return
        isDisposed = True

        Dim disposeException As Exception = Nothing
        ReleaseComObject(summaryWorksheet)

        If workbook IsNot Nothing Then
            Try
                workbook.Close(SaveChanges:=False)
            Catch ex As Exception
                disposeException = ex
            Finally
                ReleaseComObject(workbook)
            End Try
        End If

        If excelApplication IsNot Nothing Then
            Try
                excelApplication.Quit()
            Catch ex As Exception
                If disposeException Is Nothing Then disposeException = ex
            Finally
                ReleaseComObject(excelApplication)
            End Try
        End If

        If disposeException IsNot Nothing Then Throw disposeException
    End Sub

    Private Shared Sub ReleaseComObject(value As Object)
        If value Is Nothing OrElse Not Marshal.IsComObject(value) Then Return
        Try
            Marshal.FinalReleaseComObject(value)
        Catch
            ' Continue releasing the remaining Excel objects during cleanup.
        End Try
    End Sub
End Class
