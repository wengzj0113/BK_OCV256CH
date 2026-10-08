Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class FrmLogQuery
    Private currentEntries As New List(Of BusinessLogEntry)()
    Private allEntries As New List(Of BusinessLogEntry)()
    Private selectedCategory As String = "全部"
    Private pageIndex As Integer
    Private pageSize As Integer = 50
    Private quickAbnormal As Boolean
    Private isUpdating As Boolean

    Private Shared ReadOnly muted As Color = Color.FromArgb(102, 117, 138)

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub FrmLogQuery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WindowState = FormWindowState.Maximized
        btnExport.Enabled = AuthorizationService.CanAccess(AppPermission.ExportLogs)
        dateFrom.Value = Date.Today.AddDays(-7)
        dateTo.Value = Date.Today
        levelFilter.SelectedIndex = 0
        userFilter.SelectedIndex = 0
        pageSizeFilter.SelectedIndex = 0
        UpdateStorageStatus()
        UpdateCategoryStyles()
        RefreshResults()
    End Sub

    Private Sub FrmLogQuery_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        SetSplitDistance()
    End Sub

    Private Sub FrmLogQuery_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If IsHandleCreated Then SetSplitDistance()
    End Sub

    Private Sub SetSplitDistance()
        If split.ClientSize.Width < 750 Then Return
        Dim desired As Integer = CInt(split.ClientSize.Width * 0.74F)
        split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(desired, split.ClientSize.Width - split.Panel2MinSize - split.SplitterWidth))
    End Sub

    Private Sub CategoryButton_Click(sender As Object, e As EventArgs) Handles btnAll.Click, btnAlarms.Click, btnOperations.Click, btnCalibration.Click
        Dim button As Button = TryCast(sender, Button)
        If button Is Nothing Then Return
        Select Case button.Name
            Case "btnAll" : selectedCategory = "全部"
            Case "btnAlarms" : selectedCategory = "报警"
            Case "btnOperations" : selectedCategory = "操作"
            Case "btnCalibration" : selectedCategory = "校准"
        End Select
        quickAbnormal = False
        levelFilter.SelectedIndex = 0
        UpdateCategoryStyles()
        RefreshResults()
    End Sub

    Private Sub UpdateCategoryStyles()
        Dim buttons As Button() = {btnAll, btnAlarms, btnOperations, btnCalibration}
        Dim categories As String() = {"全部", "报警", "操作", "校准"}
        For index As Integer = 0 To buttons.Length - 1
            Dim active As Boolean = String.Equals(categories(index), selectedCategory, StringComparison.OrdinalIgnoreCase)
            buttons(index).BackColor = If(active, Color.FromArgb(234, 243, 255), Color.White)
            buttons(index).ForeColor = If(active, Color.FromArgb(20, 93, 176), Color.FromArgb(81, 97, 119))
            buttons(index).Font = New Font(Font.FontFamily, 9.5!, If(active, FontStyle.Bold, FontStyle.Regular))
        Next
    End Sub

    Private Sub btnToday_Click(sender As Object, e As EventArgs) Handles btnToday.Click
        dateFrom.Value = Date.Today
        dateTo.Value = Date.Today
        quickAbnormal = False
        RefreshResults()
    End Sub

    Private Sub btnAbnormal_Click(sender As Object, e As EventArgs) Handles btnAbnormal.Click
        quickAbnormal = True
        selectedCategory = "全部"
        UpdateCategoryStyles()
        RefreshResults()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        RefreshResults()
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        quickAbnormal = False
        RefreshResults()
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        selectedCategory = "全部"
        quickAbnormal = False
        dateFrom.Value = Date.Today.AddDays(-7)
        dateTo.Value = Date.Today
        levelFilter.SelectedIndex = 0
        userFilter.SelectedIndex = 0
        keywordFilter.Clear()
        UpdateCategoryStyles()
        RefreshResults()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        ExportResults()
    End Sub

    Private Sub btnPrev_Click(sender As Object, e As EventArgs) Handles btnPrev.Click
        If pageIndex <= 0 Then Return
        pageIndex -= 1
        DisplayPage()
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        If (pageIndex + 1) * pageSize >= currentEntries.Count Then Return
        pageIndex += 1
        DisplayPage()
    End Sub

    Private Sub pageSizeFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles pageSizeFilter.SelectedIndexChanged
        If isUpdating OrElse pageSizeFilter.SelectedIndex < 0 Then Return
        pageSize = CInt(Val(CStr(pageSizeFilter.SelectedItem)))
        pageIndex = 0
        DisplayPage()
    End Sub

    Private Sub RefreshResults()
        If dateFrom.Value.Date > dateTo.Value.Date Then
            MessageBox.Show(Me, "开始日期不能晚于结束日期。", "查询条件", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim fromTime As DateTime = dateFrom.Value.Date
        Dim toTime As DateTime = dateTo.Value.Date.AddDays(1).AddTicks(-1)
        Dim selectedUser As String = If(userFilter.SelectedIndex <= 0, String.Empty, CStr(userFilter.SelectedItem))
        Dim selectedLevel As String = If(quickAbnormal, "异常", CStr(If(levelFilter.SelectedItem, "全部状态")))

        isUpdating = True
        allEntries = BusinessLogService.Query(fromTime, toTime)
        UpdateStorageStatus()
        userFilter.BeginUpdate()
        userFilter.Items.Clear()
        userFilter.Items.Add("全部人员")
        For Each userName As String In allEntries.Select(Function(entry) entry.UserName).Where(Function(name) Not String.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(Function(name) name)
            userFilter.Items.Add(userName)
        Next
        Dim userIndex As Integer = 0
        If selectedUser <> String.Empty Then
            For index As Integer = 1 To userFilter.Items.Count - 1
                If String.Equals(CStr(userFilter.Items(index)), selectedUser, StringComparison.OrdinalIgnoreCase) Then
                    userIndex = index
                    Exit For
                End If
            Next
        End If
        userFilter.SelectedIndex = userIndex
        userFilter.EndUpdate()
        isUpdating = False

        lblAllCount.Text = allEntries.Count.ToString("N0", CultureInfo.CurrentCulture)
        lblAlarmCount.Text = allEntries.Where(Function(entry) entry.Category = "报警").Count().ToString("N0", CultureInfo.CurrentCulture)
        lblOperationCount.Text = allEntries.Where(Function(entry) entry.Category = "操作").Count().ToString("N0", CultureInfo.CurrentCulture)
        lblCalibrationCount.Text = allEntries.Where(Function(entry) entry.Category = "校准").Count().ToString("N0", CultureInfo.CurrentCulture)

        Dim userForQuery As String = If(userFilter.SelectedIndex <= 0, String.Empty, CStr(userFilter.SelectedItem))
        Dim queryText As String = keywordFilter.Text.Trim()
        currentEntries = BusinessLogService.FilterEntries(allEntries, selectedCategory, selectedLevel, userForQuery, queryText)
        pageIndex = 0
        UpdateStats()
        DisplayPage()
    End Sub

    Private Sub UpdateStats()
        resultStatValue.Text = currentEntries.Count.ToString("N0", CultureInfo.CurrentCulture)
        alarmStatValue.Text = allEntries.Where(Function(entry) entry.Category = "报警").Count().ToString("N0", CultureInfo.CurrentCulture)
        operationStatValue.Text = allEntries.Where(Function(entry) entry.Category = "操作").Count().ToString("N0", CultureInfo.CurrentCulture)
        calibrationStatValue.Text = allEntries.Where(Function(entry) entry.Category = "校准").Count().ToString("N0", CultureInfo.CurrentCulture)
    End Sub

    Private Sub UpdateStorageStatus()
        Dim message As String = "日志目录" & Environment.NewLine & BusinessLogService.LogDirectory & Environment.NewLine & Environment.NewLine & "旧版 AlarmLog.txt 会自动纳入报警查询。"
        If Not String.IsNullOrWhiteSpace(BusinessLogService.LastIssueMessage) Then
            message = "日志读写异常" & Environment.NewLine & BusinessLogService.LastIssueMessage & Environment.NewLine & Environment.NewLine & message
            lblStorageInfo.ForeColor = Color.FromArgb(183, 46, 57)
        Else
            lblStorageInfo.ForeColor = Color.FromArgb(102, 117, 138)
        End If
        lblStorageInfo.Text = message
    End Sub

    Private Sub DisplayPage()
        grid.Rows.Clear()
        Dim start As Integer = pageIndex * pageSize
        Dim finish As Integer = Math.Min(start + pageSize, currentEntries.Count)
        For index As Integer = start To finish - 1
            Dim entry As BusinessLogEntry = currentEntries(index)
            Dim rowIndex As Integer = grid.Rows.Add(entry.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture), entry.Category, entry.Level, entry.UserName, entry.Target, entry.Summary)
            Dim row As DataGridViewRow = grid.Rows(rowIndex)
            row.Tag = entry
            row.Cells("Category").Style.ForeColor = If(entry.Category = "报警", Color.FromArgb(183, 46, 57), If(entry.Category = "校准", Color.FromArgb(8, 123, 81), Color.FromArgb(23, 101, 189)))
            row.Cells("Level").Style.ForeColor = If(entry.Level = "失败" OrElse entry.Level = "报警", Color.FromArgb(197, 59, 66), If(entry.Level = "已复位", Color.FromArgb(171, 107, 0), Color.FromArgb(22, 132, 91)))
        Next
        Dim pageCount As Integer = Math.Max(1, CInt(Math.Ceiling(currentEntries.Count / CDbl(pageSize))))
        pageLabel.Text = If(currentEntries.Count = 0,
                            "没有符合条件的日志记录",
                            String.Format(CultureInfo.CurrentCulture, "共 {0:N0} 条　·　第 {1}/{2} 页", currentEntries.Count, pageIndex + 1, pageCount))
        btnPrev.Enabled = pageIndex > 0
        btnNext.Enabled = (pageIndex + 1) * pageSize < currentEntries.Count
        If grid.Rows.Count > 0 Then
            grid.Rows(0).Selected = True
            ShowDetails(TryCast(grid.Rows(0).Tag, BusinessLogEntry))
        Else
            ShowDetails(Nothing)
        End If
    End Sub

    Private Sub grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
        If grid.SelectedRows.Count = 0 Then Return
        ShowDetails(TryCast(grid.SelectedRows(0).Tag, BusinessLogEntry))
    End Sub

    Private Sub ShowDetails(entry As BusinessLogEntry)
        If entry Is Nothing Then
            detailStatus.Text = ""
            detailTime.Text = ""
            detailCategory.Text = ""
            detailUser.Text = ""
            detailTarget.Text = ""
            detailAction.Text = ""
            detailSource.Text = ""
            detailText.Text = "选择一条日志查看完整内容。"
            Return
        End If
        detailStatus.Text = entry.Level & If(entry.IsLegacy, "　·　历史报警", "")
        detailStatus.ForeColor = If(entry.Level = "报警" OrElse entry.Level = "失败", Color.FromArgb(197, 59, 66), Color.FromArgb(22, 132, 91))
        detailTime.Text = entry.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.CurrentCulture)
        detailCategory.Text = entry.Category
        detailUser.Text = entry.UserName
        detailTarget.Text = entry.Target
        detailAction.Text = entry.Action
        detailSource.Text = entry.Source
        detailText.Text = entry.Summary & If(String.IsNullOrWhiteSpace(entry.Details), "", Environment.NewLine & Environment.NewLine & entry.Details)
    End Sub

    Private Sub ExportResults()
        If Not AuthorizationService.RequirePermission(AppPermission.ExportLogs, Me) Then Return
        If currentEntries.Count = 0 Then
            MessageBox.Show(Me, "当前筛选结果为空。", "导出日志", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Using dialog As New SaveFileDialog()
            dialog.Filter = "CSV 文件 (*.csv)|*.csv"
            dialog.FileName = "日志记录_" & DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) & ".csv"
            dialog.Title = "导出当前筛选结果"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                File.WriteAllText(dialog.FileName, BusinessLogService.ToCsv(currentEntries), New UTF8Encoding(True))
                MessageBox.Show(Me, "已导出 " & currentEntries.Count.ToString("N0", CultureInfo.CurrentCulture) & " 条日志。", "导出完成", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show(Me, "导出失败：" & ex.Message, "导出日志", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub
End Class
