Imports System.IO
Imports System.Linq

Partial Public Class FrmLogQuery
    Private ReadOnly filePathByDisplay As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub FrmLogQuery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WindowState = FormWindowState.Maximized
        RefreshLogFiles()
    End Sub

    Private Sub FrmLogQuery_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        UpdateSplitLayout()
    End Sub

    Private Sub FrmLogQuery_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If WindowState <> FormWindowState.Minimized Then UpdateSplitLayout()
    End Sub

    Private Sub UpdateSplitLayout()
        If split Is Nothing OrElse split.ClientSize.Width <= 0 Then Return

        ' Designer 初始化阶段控件宽度尚未确定，不能提前设置较大的最小面板宽度。
        ' 窗体达到可用宽度后再应用最小宽度约束，避免 SplitterDistance 越界。
        If split.ClientSize.Width < 700 Then
            split.Panel1MinSize = 0
            split.Panel2MinSize = 0
            split.SplitterDistance = Math.Max(1, CInt(split.ClientSize.Width * 0.25F))
            Return
        End If

        split.Panel1MinSize = 180
        split.Panel2MinSize = 480

        Dim desiredDistance As Integer = CInt(split.ClientSize.Width * 0.25F)
        Dim minimumDistance As Integer = split.Panel1MinSize
        Dim maximumDistance As Integer = split.ClientSize.Width - split.Panel2MinSize - split.SplitterWidth
        split.SplitterDistance = Math.Max(minimumDistance, Math.Min(desiredDistance, maximumDistance))
    End Sub

    Private Sub RefreshLogFiles()
        Dim filter As String = txtFilter.Text.Trim()
        filePathByDisplay.Clear()
        lstFiles.Items.Clear()

        For Each path As String In AppLog.GetLogFiles()
            Dim displayName As String = System.IO.Path.GetFileName(path)
            If filter <> "" AndAlso displayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 Then Continue For
            Dim display As String = displayName & "    [" & File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm:ss") & "]"
            filePathByDisplay(display) = path
            lstFiles.Items.Add(display)
        Next

        lblDirectory.Text = "日志目录：" & AppLog.LogDirectory
        If lstFiles.Items.Count > 0 Then lstFiles.SelectedIndex = 0
        If lstFiles.Items.Count = 0 Then txtContent.Text = "未找到日志文件。"
    End Sub

    Private Sub lstFiles_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstFiles.SelectedIndexChanged
        Dim display As String = TryCast(lstFiles.SelectedItem, String)
        If String.IsNullOrWhiteSpace(display) OrElse Not filePathByDisplay.ContainsKey(display) Then
            txtContent.Clear()
            Return
        End If
        txtContent.Text = AppLog.ReadLogFile(filePathByDisplay(display))
        txtContent.SelectionStart = txtContent.TextLength
        txtContent.ScrollToCaret()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        RefreshLogFiles()
    End Sub

    Private Sub txtFilter_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFilter.KeyDown
        If e.KeyCode = Keys.Enter Then
            RefreshLogFiles()
            e.Handled = True
        End If
    End Sub
End Class
