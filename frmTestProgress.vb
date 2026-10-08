Imports System.Drawing

Public Partial Class frmTestProgress
    Private closeRequested As Boolean
    Private lastMessage As String = String.Empty

    Public Sub BeginProgress(snapshot As TestProgressSnapshot)
        If InvokeRequired Then
            BeginInvoke(New Action(Of TestProgressSnapshot)(AddressOf BeginProgress), snapshot)
            Return
        End If

        closeRequested = False
        btnClose.Enabled = False
        txtProgressLog.Clear()
        lastMessage = String.Empty
        ApplySnapshot(snapshot)
    End Sub

    Public Sub ApplySnapshot(snapshot As TestProgressSnapshot)
        If snapshot Is Nothing Then Return
        If IsDisposed OrElse Disposing Then Return

        If InvokeRequired Then
            BeginInvoke(New Action(Of TestProgressSnapshot)(AddressOf ApplySnapshot), snapshot)
            Return
        End If

        Dim stageText As String = GetStageText(snapshot.Stage)
        lblProgressStage.Text = stageText
        lblProgressCount.Text = String.Format("通道：{0}/{1}", snapshot.CompletedCount, snapshot.TotalCount)
        lblProgressElapsed.Text = "用时 " & FormatTime(snapshot.Elapsed)

        If snapshot.EstimatedRemaining.HasValue Then
            lblProgressEta.Text = "剩余 " & FormatTime(snapshot.EstimatedRemaining.Value)
        ElseIf snapshot.Stage = TestProgressStage.Idle OrElse
               snapshot.Stage = TestProgressStage.Completed OrElse
               snapshot.Stage = TestProgressStage.Failed OrElse
               snapshot.Stage = TestProgressStage.Canceled Then
            lblProgressEta.Text = "剩余 --"
        Else
            lblProgressEta.Text = "剩余计算中"
        End If

        Dim message As String = If(String.IsNullOrWhiteSpace(snapshot.Message), stageText, snapshot.Message)
        lblProgressDetail.Text = message
        If Not String.Equals(lastMessage, message, StringComparison.Ordinal) Then
            If txtProgressLog.TextLength > 0 Then txtProgressLog.AppendText(Environment.NewLine)
            txtProgressLog.AppendText(DateTime.Now.ToString("HH:mm:ss") & "  " & message)
            lastMessage = message
        End If

        If snapshot.Stage = TestProgressStage.Idle OrElse
           snapshot.Stage = TestProgressStage.Completed OrElse
           snapshot.Stage = TestProgressStage.Failed Then
            progressTest.Style = ProgressBarStyle.Continuous
        Else
            progressTest.Style = ProgressBarStyle.Continuous
        End If
        progressTest.Value = Math.Max(0, Math.Min(100, snapshot.ProgressPercent))
        lblProgressStage.ForeColor = GetStageColor(snapshot.Stage)
        btnClose.Enabled = snapshot.Stage = TestProgressStage.Completed OrElse
                           snapshot.Stage = TestProgressStage.Failed OrElse
                           snapshot.Stage = TestProgressStage.Canceled
        Refresh()
    End Sub

    Public Sub MarkCompleted(message As String)
        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf MarkCompleted), message)
            Return
        End If
        lblProgressDetail.Text = message
        btnClose.Enabled = True
        BringToFront()
    End Sub

    Public Sub MarkFailed(message As String)
        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf MarkFailed), message)
            Return
        End If
        lblProgressDetail.Text = message
        btnClose.Enabled = True
        BringToFront()
    End Sub

    Public Sub CloseForOwner()
        closeRequested = True
        Close()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        closeRequested = True
        Hide()
    End Sub

    Private Sub frmTestProgress_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Not closeRequested AndAlso Not btnClose.Enabled AndAlso e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
        End If
    End Sub

    Private Shared Function GetStageText(stage As TestProgressStage) As String
        Select Case stage
            Case TestProgressStage.Preparing
                Return "准备测试"
            Case TestProgressStage.CreatingTask
                Return "创建测试任务"
            Case TestProgressStage.Testing
                Return "正在测试"
            Case TestProgressStage.CollectingResults
                Return "读取测试结果"
            Case TestProgressStage.Saving
                Return "保存测试结果"
            Case TestProgressStage.Uploading
                Return "提交后台上传"
            Case TestProgressStage.Completed
                Return "测试完成"
            Case TestProgressStage.Failed
                Return "测试失败"
            Case TestProgressStage.Canceled
                Return "测试已停止"
            Case Else
                Return "等待测试"
        End Select
    End Function

    Private Shared Function GetStageColor(stage As TestProgressStage) As Color
        Select Case stage
            Case TestProgressStage.Completed
                Return Color.FromArgb(24, 169, 104)
            Case TestProgressStage.Failed
                Return Color.FromArgb(239, 61, 67)
            Case TestProgressStage.Canceled
                Return Color.FromArgb(106, 114, 128)
            Case TestProgressStage.Saving, TestProgressStage.Uploading
                Return Color.FromArgb(242, 126, 49)
            Case TestProgressStage.Idle
                Return Color.FromArgb(90, 104, 120)
            Case Else
                Return Color.FromArgb(23, 105, 232)
        End Select
    End Function

    Private Shared Function FormatTime(value As TimeSpan) As String
        If value.TotalHours >= 1 Then
            Return value.ToString("hh\:mm\:ss")
        End If
        Return value.ToString("mm\:ss")
    End Function
End Class
