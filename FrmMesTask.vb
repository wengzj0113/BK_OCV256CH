Imports System.Threading.Tasks
Imports System.Threading.Thread
Public Class FrmMesTask
    ' 其他代码
    Private WithEvents timer1 As New Timer()
    Private heartbeat As Heartbeat
    Private alarmUpload As AlarmUpload
    Private stateAndStopReasonUpload As StateAndStopReasonUpload
    Private wipInStationCheck As WIPInStationCheck
    Private resultDataUploadAssembly As ResultDataUploadAssembly
    Private Sub FrmMesTask_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        heartbeat = New Heartbeat(listBoxOutput)
        alarmUpload = New AlarmUpload(
            listBoxOutput,
            Function() frmTest.CurrentAlarmValue,
            Function() frmTest.CurrentAlarmCodeValue,
            Function() frmTest.CurrentAlarmStartTimeValue)
        stateAndStopReasonUpload = New StateAndStopReasonUpload(listBoxOutput)
        wipInStationCheck = New WIPInStationCheck(listBoxOutput)
        resultDataUploadAssembly = New ResultDataUploadAssembly(listBoxOutput)

        If chkHeartbeat.Checked Then
            heartbeat.StartTimer()
        End If
        If chkAlarmUpload.Checked Then
            alarmUpload.StartTimer()
        End If
        If chkStateAndStopReason.Checked Then
            stateAndStopReasonUpload.StartTimer()
        End If
        'If chkWIPInStationCheck.Checked Then
        '    wipInStationCheck.StartTimer()
        'End If
        'If chkResultDataUploadAssembly.Checked Then
        '    resultDataUploadAssembly.StartTimer()
        'End If
        btnStart.Enabled = False
    End Sub
    ' 其他代码
    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()
        ' 在此添加任何初始化代码。
        timer1.Interval = 1000 ' 默认间隔，可以根据需要更改
        AddHandler timer1.Tick, AddressOf timer1_Tick
    End Sub
    Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick

    End Sub
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        '用锁串行 Stop+Start,避免 Sleep(100) 期间异步回调与新 Timer 并发 (R2-11)
        If chkHeartbeat.Checked Then
            RestartTimerSafe(AddressOf heartbeat.StopTimer, AddressOf heartbeat.StartTimer)
        End If
        If chkAlarmUpload.Checked Then
            RestartTimerSafe(AddressOf alarmUpload.StopTimer, AddressOf alarmUpload.StartTimer)
        End If
        If chkStateAndStopReason.Checked Then
            RestartTimerSafe(AddressOf stateAndStopReasonUpload.StopTimer, AddressOf stateAndStopReasonUpload.StartTimer)
        End If
        If chkResultDataUploadAssembly.Checked Then
            RestartTimerSafe(AddressOf resultDataUploadAssembly.StopTimer, AddressOf resultDataUploadAssembly.StartTimer)
        End If
    End Sub

    '在锁内 Stop+Start,确保旧 Timer 释放后再启新 Timer (R2-11)
    Private Shared ReadOnly restartLock As New Object()
    Private Sub RestartTimerSafe(stopAction As Action, startAction As Action)
        SyncLock restartLock
            stopAction.Invoke()
            startAction.Invoke()
        End SyncLock
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        timer1.Stop()
        heartbeat.StopTimer()
        alarmUpload.StopTimer()
        stateAndStopReasonUpload.StopTimer()
        'wipInStationCheck.StopTimer()
        resultDataUploadAssembly.StopTimer()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Hide()
    End Sub

    Private Sub FrmMesTask_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        heartbeat.StopTimer()
        alarmUpload.StopTimer()
        stateAndStopReasonUpload.StopTimer()
        wipInStationCheck.StopTimer()
        resultDataUploadAssembly.StopTimer()
    End Sub

    Private Sub txtProcessNum_TextChanged(sender As Object, e As EventArgs) Handles txtProcessNum.TextChanged
        processNumSet = txtProcessNum.Text
    End Sub

    Private Sub txtOperatorName_TextChanged(sender As Object, e As EventArgs) Handles txtOperatorName.TextChanged
        operatorName = txtOperatorName.Text
    End Sub
End Class
