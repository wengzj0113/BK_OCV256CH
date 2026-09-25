Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading

Public Interface IAutomaticTestHost
    Function IsAutomaticModeEnabled() As Boolean
    Function CanStartAutomaticTest() As Boolean
    Function ValidateTrayCode(ByVal trayCode As String) As Boolean
    Function GetNeedNgRetest() As Boolean ' btnNeedNGRetest.Checked
    Function GetRetestRoundCount() As Integer ' cmbRetestNum.Value
    Function RequiresNgSorting() As Boolean
    Sub SetTrayCode(ByVal trayCode As String) ' UI adapter assigns txtTray.Text
    Sub PrepareAutomaticTray(ByVal trayCode As String)
    Sub RequestManualTrayCode(ByVal accepted As Action(Of String), ByVal cancelled As Action)
    Sub StartInitialPass(ByVal completed As Action(Of TestPassResult), ByVal failed As Action(Of Exception))
    Sub StartRetestPass(ByVal channels As IReadOnlyList(Of Integer), ByVal completed As Action(Of TestPassResult), ByVal failed As Action(Of Exception))
    Function PublishFinalCycle(ByVal cycle As AutomaticTestCycle) As Boolean
    Sub ReportAutomaticStatus(ByVal message As String)
End Interface

Public Class PlcAutomaticTestCoordinator
    Implements IDisposable

    Private ReadOnly plc As IPlcAutomationClient
    Private ReadOnly scanner As ITrayScannerClient
    Private ReadOnly host As IAutomaticTestHost
    Private ReadOnly stateLock As New Object()
    Private ReadOnly cycle As New AutomaticTestCycle()
    Private pollTimer As Timer
    Private pollBusy As Integer
    Private stopped As Boolean
    Private currentState As PlcAutomaticTestState = PlcAutomaticTestState.Idle
    Private ngSlotNeedsBusyEdge As Boolean
    Private ngSlotSawBusy As Boolean

    Public Sub New(ByVal plcClient As IPlcAutomationClient,
                   ByVal scannerClient As ITrayScannerClient,
                   ByVal testHost As IAutomaticTestHost)
        If plcClient Is Nothing Then Throw New ArgumentNullException(NameOf(plcClient))
        If scannerClient Is Nothing Then Throw New ArgumentNullException(NameOf(scannerClient))
        If testHost Is Nothing Then Throw New ArgumentNullException(NameOf(testHost))
        plc = plcClient
        scanner = scannerClient
        host = testHost
    End Sub

    Public ReadOnly Property State As PlcAutomaticTestState
        Get
            SyncLock stateLock
                Return currentState
            End SyncLock
        End Get
    End Property

    Public Sub Start()
        SyncLock stateLock
            If stopped Then Throw New ObjectDisposedException(NameOf(PlcAutomaticTestCoordinator))
            If pollTimer IsNot Nothing Then Return
            plc.Open()
            scanner.Open()
            pollTimer = New Timer(AddressOf Poll, Nothing, 1000, 1000)
        End SyncLock
        host.ReportAutomaticStatus("PLC自动流程已启动")
    End Sub

    Public Sub [Stop]()
        SyncLock stateLock
            stopped = True
            If pollTimer IsNot Nothing Then
                pollTimer.Dispose()
                pollTimer = Nothing
            End If
            currentState = PlcAutomaticTestState.Fault
        End SyncLock
        Try
            scanner.Close()
        Finally
            plc.Close()
        End Try
    End Sub

    Private Sub Poll(ByVal state As Object)
        If Interlocked.Exchange(pollBusy, 1) = 1 Then Return
        Try
            If stopped OrElse Not host.IsAutomaticModeEnabled() Then Return
            ObserveNgSlotBusy()

            Select Case Me.State
                Case PlcAutomaticTestState.Idle
                    If plc.ReadR150() Then BeginTrayCodeRead()
                Case PlcAutomaticTestState.WaitingR150Reset
                    If Not plc.ReadR150() Then MoveTo(PlcAutomaticTestState.ArmingR151, "R150已复位，准备等待R151")
                Case PlcAutomaticTestState.ArmingR151
                    If Not plc.ReadR151() Then MoveTo(PlcAutomaticTestState.WaitingFixturePressed, "等待针床压合到位")
                Case PlcAutomaticTestState.WaitingFixturePressed
                    If plc.ReadR151() Then BeginInitialTest()
                Case PlcAutomaticTestState.WaitingCycleReset
                    If Not plc.ReadR151() Then ResetCycle()
                Case PlcAutomaticTestState.WaitingNgSlotReady
                    PublishNgGradesWhenReady()
                Case PlcAutomaticTestState.WaitingR161Ack
                    SendCompletionSignal()
            End Select
        Catch ex As Exception
            EnterFault(ex)
        Finally
            Volatile.Write(pollBusy, 0)
        End Try
    End Sub

    Private Sub BeginTrayCodeRead()
        MoveTo(PlcAutomaticTestState.ReadingTrayCode, "收到R150，读取托盘码")
        Dim trayCode As String = scanner.ReadTrayCode(1500)
        If Not host.ValidateTrayCode(trayCode) Then
            EnterFault(New InvalidOperationException("自动扫码未取得有效托盘码，请检查扫码器或切换手动模式。"))
            Return
        End If
        AcceptTrayCode(trayCode)
    End Sub

    Private Sub AcceptManualTrayCode(ByVal trayCode As String)
        If stopped Then Return
        If Not host.ValidateTrayCode(trayCode) Then
            MoveTo(PlcAutomaticTestState.WaitingManualTrayCode, "手工托盘码无效，继续等待输入")
            Return
        End If
        AcceptTrayCode(trayCode)
    End Sub

    Private Sub CancelManualTrayCode()
        If stopped Then Return
        MoveTo(PlcAutomaticTestState.WaitingManualTrayCode, "等待有效托盘码")
    End Sub

    Private Sub AcceptTrayCode(ByVal trayCode As String)
        host.SetTrayCode(trayCode)
        host.PrepareAutomaticTray(trayCode)
        SyncLock stateLock
            cycle.TrayCode = trayCode.Trim()
        End SyncLock
        plc.WriteR160()
        MoveTo(PlcAutomaticTestState.WaitingR150Reset, "托盘码已确认，已发送R160")
    End Sub

    Private Sub BeginInitialTest()
        If Not host.CanStartAutomaticTest() Then
            EnterFault(New InvalidOperationException("自动测试前置条件未满足。"))
            Return
        End If

        SyncLock stateLock
            cycle.NeedNgRetest = host.GetNeedNgRetest()
            cycle.RequiresNgSorting = host.RequiresNgSorting()
            cycle.MaxRetestRounds = Math.Max(0, host.GetRetestRoundCount())
            cycle.CurrentRetestRound = 0
            cycle.CurrentNgChannels.Clear()
            cycle.WrittenNgChannels.Clear()
            cycle.FinalResultPublished = False
            cycle.NgGradesWritten = False
            cycle.R161Written = False
        End SyncLock

        MoveTo(PlcAutomaticTestState.PreparingTest, "R151已到位，开始测试准备")
        MoveTo(PlcAutomaticTestState.InitialTesting, "开始256通道初次测试")
        host.StartInitialPass(AddressOf InitialPassCompleted, AddressOf TestPassFailed)
    End Sub

    Private Sub InitialPassCompleted(ByVal result As TestPassResult)
        If result Is Nothing OrElse Not result.Completed Then
            TestPassFailed(New InvalidOperationException("初次测试未完成。"))
            Return
        End If
        UpdateNgSet(result)
        If ShouldRetest() Then
            StartNextRetest()
        Else
            FinalizeCycle()
        End If
    End Sub

    Private Function ShouldRetest() As Boolean
        SyncLock stateLock
            Return cycle.NeedNgRetest AndAlso cycle.MaxRetestRounds > 0 AndAlso cycle.CurrentNgChannels.Count > 0
        End SyncLock
    End Function

    Private Sub StartNextRetest()
        Dim channels As List(Of Integer)
        SyncLock stateLock
            If cycle.CurrentRetestRound >= cycle.MaxRetestRounds Then
                FinalizeCycle()
                Return
            End If
            cycle.CurrentRetestRound += 1
            channels = cycle.CurrentNgChannels.OrderBy(Function(channel) channel).ToList()
        End SyncLock

        MoveTo(PlcAutomaticTestState.AutoRetesting, String.Format("开始第{0}轮NG复测，共{1}个通道", cycle.CurrentRetestRound, channels.Count))
        host.StartRetestPass(channels, AddressOf RetestPassCompleted, AddressOf TestPassFailed)
    End Sub

    Private Sub RetestPassCompleted(ByVal result As TestPassResult)
        If result Is Nothing OrElse Not result.Completed Then
            TestPassFailed(New InvalidOperationException("NG复测未完成。"))
            Return
        End If
        UpdateNgSet(result)
        If ShouldRetest() Then
            StartNextRetest()
        Else
            FinalizeCycle()
        End If
    End Sub

    Private Sub UpdateNgSet(ByVal result As TestPassResult)
        SyncLock stateLock
            For Each item As ChannelTestResult In result.Results
                If item.IsNg Then
                    cycle.CurrentNgChannels.Add(item.ChannelNumber)
                    cycle.WrittenNgChannels.Add(item.ChannelNumber)
                Else
                    cycle.CurrentNgChannels.Remove(item.ChannelNumber)
                End If
            Next
        End SyncLock
    End Sub

    Private Sub FinalizeCycle()
        MoveTo(PlcAutomaticTestState.Finalizing, "测试和复测完成，保存最终结果")
        Try
            If Not cycle.FinalResultPublished Then
                If Not host.PublishFinalCycle(cycle) Then Throw New InvalidOperationException("最终结果发布失败。")
                SyncLock stateLock
                    cycle.FinalResultPublished = True
                End SyncLock
            End If
            If cycle.RequiresNgSorting Then
                PublishNgGradesWhenReady()
            Else
                SendCompletionSignal()
            End If
        Catch ex As Exception
            EnterFault(ex)
        End Try
    End Sub

    Private Sub ObserveNgSlotBusy()
        If Not ngSlotNeedsBusyEdge OrElse ngSlotSawBusy Then Return
        Try
            If Not plc.ReadR170() Then ngSlotSawBusy = True
        Catch ex As Exception
            '仅延迟下一次交接；正式写入前仍会重新读取 R170。
        End Try
    End Sub

    Private Sub PublishNgGradesWhenReady()
        Try
            Dim slotReady As Boolean = plc.ReadR170()
            If ngSlotNeedsBusyEdge AndAlso Not slotReady Then ngSlotSawBusy = True
            If Not slotReady OrElse (ngSlotNeedsBusyEdge AndAlso Not ngSlotSawBusy) Then
                MoveTo(PlcAutomaticTestState.WaitingNgSlotReady, "最终测试结果已保存，等待上一托盘NG挑选完成及R170重新置1")
                Return
            End If
            If Not cycle.NgGradesWritten Then
                '上一托盘挑选完成后，先清除全部旧标记；任何写入失败都从清零重新尝试。
                For channel As Integer = 1 To 256
                    plc.WriteChannelGrade(channel, 0)
                Next
                For Each channel As Integer In cycle.CurrentNgChannels.OrderBy(Function(item) item)
                    plc.WriteChannelGrade(channel, 2)
                Next
                SyncLock stateLock
                    cycle.NgGradesWritten = True
                End SyncLock
                ngSlotNeedsBusyEdge = True
                ngSlotSawBusy = False
            End If
            SendCompletionSignal()
        Catch ex As Exception
            MoveTo(PlcAutomaticTestState.WaitingNgSlotReady, "等待PLC结果区可写或NG写入确认：" & ex.Message)
        End Try
    End Sub

    Private Sub SendCompletionSignal()
        Try
            plc.WriteR161()
            SyncLock stateLock
                cycle.R161Written = True
            End SyncLock
            MoveTo(PlcAutomaticTestState.WaitingCycleReset, "R161已确认，等待R151复位")
        Catch ex As Exception
            MoveTo(PlcAutomaticTestState.WaitingR161Ack, "R161未确认，将重试：" & ex.Message)
        End Try
    End Sub

    Private Sub TestPassFailed(ByVal exception As Exception)
        If exception Is Nothing Then exception = New InvalidOperationException("测试过程失败。")
        EnterFault(exception)
    End Sub

    Private Sub ResetCycle()
        SyncLock stateLock
            cycle.Reset()
        End SyncLock
        MoveTo(PlcAutomaticTestState.Idle, "上一托盘流程已复位，等待R150")
    End Sub

    Private Sub EnterFault(ByVal exception As Exception)
        If exception Is Nothing Then exception = New InvalidOperationException("PLC自动流程异常。")
        MoveTo(PlcAutomaticTestState.Fault, "PLC自动流程异常：" & exception.Message)
    End Sub

    Private Sub MoveTo(ByVal nextState As PlcAutomaticTestState, ByVal message As String)
        SyncLock stateLock
            If stopped Then Return
            currentState = nextState
        End SyncLock
        host.ReportAutomaticStatus(message)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
    End Sub
End Class
