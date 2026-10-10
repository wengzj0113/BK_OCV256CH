Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading

Public Interface IAutomaticTestHost
    Function IsAutomaticModeEnabled() As Boolean
    Function IsDebugModeEnabled() As Boolean
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
    Sub ReportAutomaticEvent(ByVal message As String)
End Interface

Public Interface IManualTestHost
    Function IsManualModeEnabled() As Boolean
    Sub EnsureManualTestReady()
    Sub PrepareManualTray(ByVal trayCode As String, ByVal channels As IReadOnlyList(Of Integer))
    Sub StartManualInitialPass(ByVal trayCode As String,
                               ByVal channels As IReadOnlyList(Of Integer),
                               ByVal canStart As Func(Of Boolean),
                               ByVal completed As Action(Of TestPassResult),
                               ByVal failed As Action(Of Exception))
End Interface

Public Class PlcAutomaticTestCoordinator
    Implements IDisposable

    Private ReadOnly plc As IPlcAutomationClient
    Private ReadOnly scanner As ITrayScannerClient
    Private ReadOnly host As IAutomaticTestHost
    Private ReadOnly stateLock As New Object()
    Private ReadOnly lastReportedRegisterValues As New Dictionary(Of String, Boolean)(StringComparer.Ordinal)
    Private ReadOnly cycle As New AutomaticTestCycle()
    Private manualPolling As Boolean
    Private manualCycleRequestInProgress As Boolean
    'OCV0 has one completed-result handoff in flight while the next tray may be scanned.
    Private pendingNgSelection As PendingNgSelection
    'Accept a high input once. PLC low re-arms the next request; the first
    'observed value may already be high, so no prior low sample is required.
    Private r150RequestConsumed As Boolean
    Private r151RequestConsumed As Boolean
    Private r152RequestConsumed As Boolean
    Private trayRequestPending As Boolean
    Private testRequestPending As Boolean
    Private sortingRequestPending As Boolean
    Private sortingRequestTestStateAtCapture As PlcR152TestState = PlcR152TestState.NoTestStarted
    Private sortingRequestRequiresNgSortingAtCapture As Boolean
    Private trayPreparationInProgress As Boolean
    Private manualTrayCodePending As Boolean
    Private manualTrayCodeGeneration As Integer
    Private nextTrayScanRetryAtUtc As DateTime = DateTime.MinValue
    Private trayConfirmationPending As Boolean
    Private r160WriteInProgress As Boolean
    Private r161WriteInProgress As Boolean
    Private testPrerequisiteWaitingReported As Boolean
    Private testTrayConfirmationWaitingReported As Boolean
    Private sortingRequestWaitingReported As Boolean
    Private trayRequestBlockedReason As String = String.Empty
    Private trayPreparationRecoveryCode As String = String.Empty
    Private trayPreparationRetryPending As Boolean
    Private pollTimer As Timer
    Private pollBusy As Integer
    Private stopped As Boolean
    Private requestGeneration As Integer
    Private pauseAtBoundaryState As Integer
    Private boundaryPauseCallback As Action
    Private currentState As PlcAutomaticTestState = PlcAutomaticTestState.Idle

    Public Sub New(ByVal plcClient As IPlcAutomationClient,
                   ByVal scannerClient As ITrayScannerClient,
                   ByVal testHost As IAutomaticTestHost)
        If plcClient Is Nothing Then Throw New ArgumentNullException(NameOf(plcClient))
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

    Public ReadOnly Property IsCycleOccupied As Boolean
        Get
            SyncLock stateLock
                Return IsCycleOccupiedLocked()
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property IsPausedAtCycleBoundary As Boolean
        Get
            Return Volatile.Read(pauseAtBoundaryState) = 2
        End Get
    End Property

    Public Sub RequestPauseAtCycleBoundary(ByVal pausedCallback As Action)
        If pausedCallback Is Nothing Then Throw New ArgumentNullException(NameOf(pausedCallback))
        SyncLock stateLock
            If stopped Then Throw New ObjectDisposedException(NameOf(PlcAutomaticTestCoordinator))
            boundaryPauseCallback = pausedCallback
            Interlocked.Exchange(pauseAtBoundaryState, 1)
        End SyncLock
        TryCompleteBoundaryPause()
    End Sub

    Public Sub ResumeAfterPause()
        SyncLock stateLock
            If stopped Then Return
            boundaryPauseCallback = Nothing
            Interlocked.Exchange(pauseAtBoundaryState, 0)
        End SyncLock
    End Sub

    Public Sub Start()
        SyncLock stateLock
            If stopped Then Throw New ObjectDisposedException(NameOf(PlcAutomaticTestCoordinator))
            If pollTimer IsNot Nothing Then Return
            If scanner Is Nothing Then Throw New InvalidOperationException("自动测试需要扫码器客户端。")
            manualPolling = False
            plc.Open()
            scanner.Open()
            pollTimer = New Timer(AddressOf Poll, Nothing, 1000, 1000)
        End SyncLock
        host.ReportAutomaticStatus("PLC自动流程已启动")
        ReportAutomaticEventSafely("自动流程启动")
    End Sub

    Public Sub StartManual()
        SyncLock stateLock
            If stopped Then Throw New ObjectDisposedException(NameOf(PlcAutomaticTestCoordinator))
            If pollTimer IsNot Nothing Then Return
            manualPolling = True
            plc.Open()
            pollTimer = New Timer(AddressOf Poll, Nothing, 1000, 1000)
        End SyncLock
        host.ReportAutomaticStatus("手动测试 PLC 周期已就绪，等待操作员启动")
        ReportAutomaticEventSafely("手动测试流程已启动，等待主页面启动测试")
    End Sub

    Public Sub [Stop]()
        SyncLock stateLock
            stopped = True
            manualPolling = False
            manualCycleRequestInProgress = False
            boundaryPauseCallback = Nothing
            Interlocked.Exchange(pauseAtBoundaryState, 0)
            If pollTimer IsNot Nothing Then
                pollTimer.Dispose()
                pollTimer = Nothing
            End If
            currentState = PlcAutomaticTestState.Fault
        End SyncLock
        Try
            If scanner IsNot Nothing Then scanner.Close()
        Finally
            plc.Close()
        End Try
    End Sub

    Public Function RequestManualCycle(ByVal trayCode As String,
                                       ByVal selectedChannels As IEnumerable(Of Integer)) As Boolean
        Dim manualHost As IManualTestHost = TryCast(host, IManualTestHost)
        If manualHost Is Nothing Then Throw New InvalidOperationException("当前测试宿主不支持手动 PLC 测试周期。")
        If Not manualPolling OrElse Not manualHost.IsManualModeEnabled() Then
            Throw New InvalidOperationException("请先切换到主测试页手动模式。")
        End If

        Dim normalizedTrayCode As String = If(trayCode, String.Empty).Trim()
        If Not host.ValidateTrayCode(normalizedTrayCode) Then
            Throw New InvalidOperationException("托盘码为空或长度不符合设置。")
        End If

        Dim requestedChannels As List(Of Integer) = If(selectedChannels, Enumerable.Empty(Of Integer)()).ToList()
        If requestedChannels.Count = 0 Then Throw New InvalidOperationException("请先选择至少一个待测试通道。")
        If requestedChannels.Any(Function(channel) channel < 1 OrElse channel > 256) Then
            Throw New InvalidOperationException("所选通道超出有效范围 1–256。")
        End If
        If requestedChannels.Distinct().Count() <> requestedChannels.Count Then
            Throw New InvalidOperationException("所选通道包含重复通道。")
        End If
        Dim channelSnapshot As IReadOnlyList(Of Integer) = Array.AsReadOnly(requestedChannels.OrderBy(Function(channel) channel).ToArray())

        Dim manualGeneration As Integer
        SyncLock stateLock
            If stopped Then Throw New ObjectDisposedException(NameOf(PlcAutomaticTestCoordinator))
            If manualCycleRequestInProgress OrElse IsCycleOccupiedLocked() OrElse currentState <> PlcAutomaticTestState.Idle Then Return False
            manualCycleRequestInProgress = True
            manualGeneration = requestGeneration
        End SyncLock

        Try
            manualHost.EnsureManualTestReady()
            host.SetTrayCode(normalizedTrayCode)
            manualHost.PrepareManualTray(normalizedTrayCode, channelSnapshot)

            SyncLock stateLock
                If stopped OrElse requestGeneration <> manualGeneration Then
                    manualCycleRequestInProgress = False
                    Return False
                End If
                cycle.TrayCode = normalizedTrayCode
                cycle.IsManualCycle = True
                cycle.MeasurementChannels = channelSnapshot
                trayConfirmationPending = True
                trayPreparationRecoveryCode = String.Empty
                trayPreparationRetryPending = False
                manualCycleRequestInProgress = False
            End SyncLock
            MoveTo(PlcAutomaticTestState.PreparingTest, "手动托盘准备完成，等待 R160 确认")
            ReportAutomaticEventSafely("手动测试启动请求已接受，托盘码和通道集合已冻结")
            Return True
        Catch
            SyncLock stateLock
                manualCycleRequestInProgress = False
                If String.IsNullOrWhiteSpace(cycle.TrayCode) AndAlso Not stopped Then currentState = PlcAutomaticTestState.Idle
            End SyncLock
            Throw
        End Try
    End Function

    Private Function IsCycleOccupiedLocked() As Boolean
        Return manualCycleRequestInProgress OrElse
               trayRequestPending OrElse testRequestPending OrElse sortingRequestPending OrElse
               trayPreparationInProgress OrElse trayConfirmationPending OrElse r160WriteInProgress OrElse
               r161WriteInProgress OrElse Not String.IsNullOrWhiteSpace(cycle.TrayCode) OrElse
               cycle.TestStarted OrElse cycle.FinalResultPublished OrElse pendingNgSelection IsNot Nothing
    End Function

    Private Sub Poll(ByVal state As Object)
        If Interlocked.Exchange(pollBusy, 1) = 1 Then Return
        Try
            If stopped Then Return
            Dim automaticModeEnabled As Boolean = host.IsAutomaticModeEnabled()
            Dim manualHost As IManualTestHost = TryCast(host, IManualTestHost)
            Dim manualModeEnabled As Boolean = manualPolling AndAlso manualHost IsNot Nothing AndAlso manualHost.IsManualModeEnabled()
            If Not automaticModeEnabled AndAlso Not manualModeEnabled Then Return
            If Volatile.Read(pauseAtBoundaryState) = 2 Then Return
            If Volatile.Read(pauseAtBoundaryState) = 1 AndAlso IsAtSafeCycleBoundary() Then Return
            Dim pollGeneration As Integer
            SyncLock stateLock
                pollGeneration = requestGeneration
            End SyncLock
            Dim r150ReadSucceeded As Boolean
            Dim r151ReadSucceeded As Boolean
            Dim r152ReadSucceeded As Boolean
            Dim r150High As Boolean
            If automaticModeEnabled Then r150High = ReadRegisterSignal("R150", AddressOf plc.ReadR150, r150ReadSucceeded)
            Dim r151High As Boolean
            If automaticModeEnabled Then r151High = ReadRegisterSignal("R151", AddressOf plc.ReadR151, r151ReadSucceeded)
            Dim r152High As Boolean = ReadRegisterSignal("R152", AddressOf plc.ReadR152, r152ReadSucceeded)
            Dim r150Accepted As Boolean
            Dim r151Accepted As Boolean
            Dim r152Accepted As Boolean
            Dim trayPreparationRecoveryReleased As Boolean

            Dim pollRequestsAccepted As Boolean
            SyncLock stateLock
                pollRequestsAccepted = (pollGeneration = requestGeneration)
                If pollRequestsAccepted Then
                    Dim canAcceptR151 As Boolean = automaticModeEnabled
                    If r150ReadSucceeded AndAlso Not r150High Then
                        r150RequestConsumed = False
                        If currentState = PlcAutomaticTestState.WaitingTrayPreparationRecovery Then
                            trayPreparationRecoveryCode = String.Empty
                            trayPreparationRetryPending = False
                            trayRequestPending = False
                            trayPreparationRecoveryReleased = True
                            currentState = PlcAutomaticTestState.Idle
                        End If
                    End If
                    If r151ReadSucceeded AndAlso Not r151High Then r151RequestConsumed = False
                    If r152ReadSucceeded AndAlso Not r152High Then r152RequestConsumed = False

                    If r150ReadSucceeded AndAlso r150High AndAlso Not r150RequestConsumed Then
                        r150RequestConsumed = True
                        trayRequestPending = True
                        r150Accepted = True
                    End If
                    If r151ReadSucceeded AndAlso r151High AndAlso Not r151RequestConsumed Then
                        r151RequestConsumed = True
                        If canAcceptR151 Then
                            testRequestPending = True
                            r151Accepted = True
                        End If
                    End If
                    If r152ReadSucceeded AndAlso r152High AndAlso Not r152RequestConsumed Then
                        r152RequestConsumed = True
                        sortingRequestPending = True
                        sortingRequestTestStateAtCapture = CaptureR152TestStateLocked()
                        sortingRequestRequiresNgSortingAtCapture = pendingNgSelection IsNot Nothing OrElse cycle.RequiresNgSorting
                        sortingRequestWaitingReported = False
                        r152Accepted = True
                    End If
                End If
            End SyncLock

            If pollRequestsAccepted Then
                If r150Accepted Then ReportAutomaticEventSafely("检测到R150=1，托盘扫码请求已记录")
                If r151Accepted Then ReportAutomaticEventSafely("检测到R151=1，测试请求已记录")
                If r152Accepted Then ReportAutomaticEventSafely("检测到R152=1，NG分选请求已记录")
            End If
            If trayPreparationRecoveryReleased Then
                ReportAutomaticEventSafely("检测到R150=0，已释放准备失败的托盘；等待新的R150请求")
                host.ReportAutomaticStatus("托盘准备请求已复位，等待新托盘")
            End If

            ProcessPendingRequests(pollGeneration)
        Catch ex As Exception
            EnterFault(ex)
        Finally
            Volatile.Write(pollBusy, 0)
            TryCompleteBoundaryPause()
        End Try
    End Sub

    Private Function CaptureR152TestStateLocked() As PlcR152TestState
        If pendingNgSelection IsNot Nothing Then Return PlcR152TestState.TestCompleted
        If Not cycle.TestStarted Then Return PlcR152TestState.NoTestStarted
        If currentState = PlcAutomaticTestState.Fault Then Return PlcR152TestState.TestFaulted
        If cycle.FinalResultPublished AndAlso cycle.R161Written Then Return PlcR152TestState.TestCompleted
        Return PlcR152TestState.TestInProgress
    End Function

    Private Function IsAtSafeCycleBoundary() As Boolean
        Dim current As PlcAutomaticTestState = Me.State
        SyncLock stateLock
            Return (current = PlcAutomaticTestState.Idle OrElse current = PlcAutomaticTestState.Fault) AndAlso
                   Not trayRequestPending AndAlso Not testRequestPending AndAlso Not sortingRequestPending AndAlso
                   Not trayPreparationInProgress AndAlso Not manualTrayCodePending AndAlso
                   String.IsNullOrWhiteSpace(cycle.TrayCode)
        End SyncLock
    End Function

    Private Sub ProcessPendingRequests(ByVal pollGeneration As Integer)
        Dim retryTestCompletion As Boolean
        Dim acknowledgeEarlySortingRequest As Boolean
        SyncLock stateLock
            If sortingRequestPending AndAlso
               sortingRequestTestStateAtCapture = PlcR152TestState.TestInProgress AndAlso
               currentState = PlcAutomaticTestState.Fault Then
                sortingRequestTestStateAtCapture = PlcR152TestState.TestFaulted
                sortingRequestWaitingReported = False
            End If
            retryTestCompletion = cycle.FinalResultPublished AndAlso Not cycle.R161Written
            acknowledgeEarlySortingRequest = sortingRequestPending AndAlso
                                             sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted AndAlso
                                             pendingNgSelection Is Nothing
        End SyncLock

        If acknowledgeEarlySortingRequest Then SendEarlySortingCompletionSignal()
        'Release the completed tray before consuming a queued R150/R151 request.
        If retryTestCompletion Then SendTestCompletionSignal()

        Dim retryTrayPreparation As Boolean
        Dim retryTrayCode As String = String.Empty
        SyncLock stateLock
            If currentState = PlcAutomaticTestState.WaitingTrayPreparationRecovery AndAlso
               trayPreparationRetryPending AndAlso Not trayPreparationInProgress AndAlso
               Not String.IsNullOrWhiteSpace(trayPreparationRecoveryCode) Then
                retryTrayPreparation = True
                retryTrayCode = trayPreparationRecoveryCode
                trayPreparationRetryPending = False
                trayPreparationInProgress = True
            End If
        End SyncLock
        If retryTrayPreparation Then
            Try
                PrepareTrayAndConfirm(retryTrayCode, pollGeneration)
            Finally
                SyncLock stateLock
                    trayPreparationInProgress = False
                End SyncLock
            End Try
        End If

        Dim canPrepareTray As Boolean
        Dim trayRequestWaitMessage As String = String.Empty
        SyncLock stateLock
            canPrepareTray = trayRequestPending AndAlso Not trayPreparationInProgress AndAlso
                             Not manualTrayCodePending AndAlso String.IsNullOrWhiteSpace(cycle.TrayCode) AndAlso
                             DateTime.UtcNow >= nextTrayScanRetryAtUtc
            If canPrepareTray Then
                trayRequestPending = False
                trayPreparationInProgress = True
                nextTrayScanRetryAtUtc = DateTime.MinValue
                trayRequestBlockedReason = String.Empty
            ElseIf trayRequestPending Then
                Dim blockedReason As String
                If trayPreparationInProgress Then
                    blockedReason = "扫码流程正在处理中"
                ElseIf manualTrayCodePending Then
                    blockedReason = "等待手工输入托盘码"
                ElseIf Not String.IsNullOrWhiteSpace(cycle.TrayCode) Then
                    blockedReason = "当前托盘状态尚未释放，托盘码=" & cycle.TrayCode
                ElseIf DateTime.UtcNow < nextTrayScanRetryAtUtc Then
                    blockedReason = "等待扫码重试时间"
                Else
                    blockedReason = "扫码条件尚未满足"
                End If
                If Not String.Equals(trayRequestBlockedReason, blockedReason, StringComparison.Ordinal) Then
                    trayRequestBlockedReason = blockedReason
                    trayRequestWaitMessage = "R150请求已保留，暂未触发扫码：" & blockedReason
                End If
            End If
        End SyncLock
        If trayRequestWaitMessage.Length > 0 Then ReportAutomaticEventSafely(trayRequestWaitMessage)

        If canPrepareTray Then
            Try
                BeginTrayCodeRead(pollGeneration)
            Catch ex As Exception
                EnterFault(ex)
            Finally
                SyncLock stateLock
                    trayPreparationInProgress = False
                End SyncLock
            End Try
        End If

        Dim confirmTray As Boolean
        SyncLock stateLock
            confirmTray = trayConfirmationPending AndAlso Not cycle.TrayConfirmed
        End SyncLock
        If confirmTray Then ConfirmTrayCodeWithPlc()

        Dim startTest As Boolean
        Dim waitForTrayConfirmation As Boolean
        Dim manualCycle As Boolean
        SyncLock stateLock
            startTest = testRequestPending AndAlso Not cycle.TestStarted AndAlso cycle.TrayConfirmed
            manualCycle = cycle.IsManualCycle
            waitForTrayConfirmation = testRequestPending AndAlso Not cycle.TrayConfirmed AndAlso
                                      Not testTrayConfirmationWaitingReported
            If waitForTrayConfirmation Then testTrayConfirmationWaitingReported = True
        End SyncLock
        If waitForTrayConfirmation Then
            ReportAutomaticEventSafely("R151测试请求已暂存，等待托盘码写入R160并确认")
        End If
        If startTest Then
            Dim prerequisitesReady As Boolean = False
            Dim prerequisiteFailure As String = String.Empty
            Try
                If manualCycle Then
                    Dim manualHost As IManualTestHost = TryCast(host, IManualTestHost)
                    If manualHost Is Nothing Then Throw New InvalidOperationException("手动测试宿主不可用。")
                    manualHost.EnsureManualTestReady()
                    prerequisitesReady = True
                Else
                    prerequisitesReady = host.CanStartAutomaticTest()
                End If
            Catch ex As Exception
                prerequisiteFailure = ex.Message
                Debug.WriteLine("PLC automatic test prerequisite check failed: " & ex.Message)
            End Try

            If prerequisitesReady Then
                SyncLock stateLock
                    If testRequestPending AndAlso Not cycle.TestStarted AndAlso cycle.TrayConfirmed Then
                        testRequestPending = False
                        testPrerequisiteWaitingReported = False
                        testTrayConfirmationWaitingReported = False
                        cycle.TestStarted = True
                    Else
                        startTest = False
                    End If
                End SyncLock
                If startTest Then
                    Try
                        BeginInitialTest()
                    Catch ex As Exception
                        EnterFault(ex)
                    End Try
                End If
            ElseIf Not testPrerequisiteWaitingReported Then
                testPrerequisiteWaitingReported = True
                Dim waitingMessage As String = If(manualCycle,
                                                  "手动启动请求已暂存，等待测试前置条件满足",
                                                  "R151测试请求已暂存，等待测试前置条件满足")
                If manualCycle AndAlso prerequisiteFailure.Length > 0 Then waitingMessage &= "：" & prerequisiteFailure
                ReportAutomaticEventSafely(waitingMessage)
                If manualCycle AndAlso prerequisiteFailure.Length > 0 Then host.ReportAutomaticStatus(waitingMessage)
            End If
        End If

        Dim startSorting As Boolean
        Dim reportSortingWait As Boolean
        Dim sortingWaitMessage As String = String.Empty
        SyncLock stateLock
            startSorting = sortingRequestPending AndAlso
                           sortingRequestTestStateAtCapture <> PlcR152TestState.NoTestStarted AndAlso
                           sortingRequestTestStateAtCapture <> PlcR152TestState.TestFaulted AndAlso
                           ((pendingNgSelection IsNot Nothing) OrElse
                            (cycle.TestStarted AndAlso cycle.FinalResultPublished AndAlso cycle.R161Written) OrElse
                            (sortingRequestTestStateAtCapture = PlcR152TestState.TestCompleted AndAlso
                             Not sortingRequestRequiresNgSortingAtCapture))
            If sortingRequestPending AndAlso Not startSorting AndAlso Not sortingRequestWaitingReported Then
                Select Case sortingRequestTestStateAtCapture
                    Case PlcR152TestState.TestFaulted
                        reportSortingWait = True
                        sortingRequestWaitingReported = True
                        sortingWaitMessage = "R152在测试异常期间到达；结果无效，等待人工停止或恢复"
                    Case PlcR152TestState.TestInProgress
                        reportSortingWait = True
                        sortingRequestWaitingReported = True
                        sortingWaitMessage = If(cycle.FinalResultPublished,
                                                "R152分选请求已暂存，等待R161测试完成确认",
                                                "R152分选请求已暂存，等待当前测试结果")
                End Select
            End If
            If startSorting Then sortingRequestWaitingReported = False
        End SyncLock
        If reportSortingWait Then ReportAutomaticEventSafely(sortingWaitMessage)
        If startSorting Then
            Dim selection As PendingNgSelection
            Dim noSortingCycleReady As Boolean
            Dim noSortingRequestReady As Boolean
            SyncLock stateLock
                selection = pendingNgSelection
                noSortingCycleReady = selection Is Nothing AndAlso cycle.TestStarted AndAlso
                                      cycle.FinalResultPublished AndAlso cycle.R161Written AndAlso
                                      Not cycle.RequiresNgSorting
                noSortingRequestReady = selection Is Nothing AndAlso
                                        sortingRequestTestStateAtCapture = PlcR152TestState.TestCompleted AndAlso
                                        Not sortingRequestRequiresNgSortingAtCapture
            End SyncLock
            If selection IsNot Nothing Then
                PublishNgGrades(selection)
            ElseIf noSortingCycleReady OrElse noSortingRequestReady Then
                SendNoSortingCompletionSignal()
            End If
        End If
    End Sub

    Private Function IsRequestGenerationCurrent(ByVal expectedGeneration As Integer) As Boolean
        SyncLock stateLock
            Return Not stopped AndAlso requestGeneration = expectedGeneration
        End SyncLock
    End Function

    Public Sub RequestStopCurrentCycle()
        Dim cancelledTrayRequest As Boolean
        SyncLock stateLock
            If stopped Then Return
            requestGeneration += 1
            manualCycleRequestInProgress = False
            cancelledTrayRequest = trayRequestPending OrElse trayPreparationInProgress OrElse manualTrayCodePending
            trayRequestPending = False
            testRequestPending = False
            sortingRequestPending = False
            sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted
            sortingRequestRequiresNgSortingAtCapture = False
            trayPreparationInProgress = False
            trayPreparationRecoveryCode = String.Empty
            trayPreparationRetryPending = False
            manualTrayCodePending = False
            nextTrayScanRetryAtUtc = DateTime.MinValue
            trayConfirmationPending = False
            testPrerequisiteWaitingReported = False
            testTrayConfirmationWaitingReported = False
            sortingRequestWaitingReported = False
            trayRequestBlockedReason = String.Empty
            cycle.Reset()
            currentState = PlcAutomaticTestState.Idle
        End SyncLock
        If cancelledTrayRequest Then
            ReportAutomaticEventSafely("停止操作已取消当前R150扫码请求；如PLC仍保持R150=1，需先复位为0再重新置1")
        End If
        ReportAutomaticEventSafely("自动流程已复位，等待新托盘")
        host.ReportAutomaticStatus("测试已停止，等待托盘进入")
    End Sub

    Private Sub TryCompleteBoundaryPause()
        If Volatile.Read(pauseAtBoundaryState) <> 1 OrElse
           Volatile.Read(pollBusy) <> 0 OrElse
           Not IsAtSafeCycleBoundary() Then Return
        If Interlocked.CompareExchange(pauseAtBoundaryState, 2, 1) <> 1 Then Return

        Dim callback As Action = Nothing
        SyncLock stateLock
            callback = boundaryPauseCallback
            boundaryPauseCallback = Nothing
        End SyncLock
        If callback IsNot Nothing Then
            Try
                callback()
            Catch ex As Exception
                Debug.WriteLine("PLC boundary-pause callback failed: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub BeginTrayCodeRead(ByVal expectedGeneration As Integer)
        If Not IsRequestGenerationCurrent(expectedGeneration) Then
            ReportAutomaticEventSafely("R150扫码请求在启动前被流程停止/复位取消")
            Return
        End If
        MoveTo(PlcAutomaticTestState.ReadingTrayCode, "收到R150，触发扫码并读取托盘码")
        ReportAutomaticEventSafely("托盘到位，开始扫码")
        If Not IsRequestGenerationCurrent(expectedGeneration) Then
            ReportAutomaticEventSafely("流程已停止/复位，未触发本次扫码")
            Return
        End If
        Dim trayCode As String
        Try
            scanner.TriggerScan()
            trayCode = scanner.ReadTrayCode(1500)
        Catch ex As Exception
            If Not IsRequestGenerationCurrent(expectedGeneration) Then
                ReportAutomaticEventSafely("扫码期间流程已停止/复位，本次扫码失败结果已丢弃")
                Return
            End If
            ReportAutomaticEventSafely("托盘扫码失败：" & ex.Message & "，请手工输入托盘码或取消后自动重试")
            PromptManualTrayCode("扫码失败：" & ex.Message, expectedGeneration)
            Return
        End Try
        If Not IsRequestGenerationCurrent(expectedGeneration) Then
            ReportAutomaticEventSafely("扫码期间流程已停止/复位，本次扫码结果已丢弃")
            Return
        End If
        If Not host.ValidateTrayCode(trayCode) Then
            If Not IsRequestGenerationCurrent(expectedGeneration) Then Return
            ReportAutomaticEventSafely("未读取到有效托盘码，请手工输入托盘码或取消后自动重试")
            PromptManualTrayCode("未识别到托盘码或托盘码长度不符合设置。", expectedGeneration)
            Return
        End If
        If Not IsRequestGenerationCurrent(expectedGeneration) Then Return
        ReportAutomaticEventSafely("扫码成功，托盘码：" & trayCode.Trim())
        AcceptTrayCode(trayCode, expectedGeneration)
    End Sub

    Private Sub AcceptManualTrayCode(ByVal trayCode As String)
        If stopped OrElse Not manualTrayCodePending Then Return
        Dim manualGeneration As Integer
        SyncLock stateLock
            manualGeneration = manualTrayCodeGeneration
        End SyncLock
        If Not host.ValidateTrayCode(trayCode) Then
            PromptManualTrayCode("手工输入的托盘码为空或长度不符合设置，请重新输入。", manualGeneration)
            Return
        End If
        SyncLock stateLock
            If stopped OrElse requestGeneration <> manualGeneration OrElse Not manualTrayCodePending Then Return
            manualTrayCodePending = False
            trayRequestBlockedReason = String.Empty
            nextTrayScanRetryAtUtc = DateTime.MinValue
        End SyncLock
        Try
            ReportAutomaticEventSafely("手工输入托盘码已确认")
            AcceptTrayCode(trayCode, manualGeneration)
        Catch ex As Exception
            EnterFault(ex)
        End Try
    End Sub

    Private Sub PromptManualTrayCode(ByVal reason As String, ByVal expectedGeneration As Integer)
        SyncLock stateLock
            If stopped OrElse requestGeneration <> expectedGeneration Then Return
            manualTrayCodePending = True
            manualTrayCodeGeneration = expectedGeneration
        End SyncLock
        MoveTo(PlcAutomaticTestState.WaitingManualTrayCode, reason & "请手工输入托盘码。")
        host.RequestManualTrayCode(AddressOf AcceptManualTrayCode, AddressOf CancelManualTrayCode)
    End Sub

    Private Sub CancelManualTrayCode()
        If stopped Then Return
        SyncLock stateLock
            manualTrayCodePending = False
            trayRequestBlockedReason = String.Empty
            If String.IsNullOrWhiteSpace(cycle.TrayCode) Then
                trayRequestPending = True
                nextTrayScanRetryAtUtc = DateTime.UtcNow.AddSeconds(2)
            End If
        End SyncLock
        ReportAutomaticEventSafely("手工输入已取消，约2秒后自动重新扫码")
        MoveTo(PlcAutomaticTestState.WaitingTrayScanRetry, "手工输入已取消，约2秒后自动重新扫码")
    End Sub

    Private Sub AcceptTrayCode(ByVal trayCode As String, ByVal expectedGeneration As Integer)
        If Not IsRequestGenerationCurrent(expectedGeneration) Then
            ReportAutomaticEventSafely("托盘码确认前流程已停止/复位，已丢弃本次托盘码")
            Return
        End If
        PrepareTrayAndConfirm(trayCode, expectedGeneration)
    End Sub

    Private Sub PrepareTrayAndConfirm(ByVal trayCode As String, ByVal expectedGeneration As Integer)
        If Not IsRequestGenerationCurrent(expectedGeneration) Then Return
        Dim normalizedTrayCode As String = If(trayCode, String.Empty).Trim()
        host.SetTrayCode(normalizedTrayCode)
        Try
            host.PrepareAutomaticTray(normalizedTrayCode)
        Catch ex As Exception
            Dim preparationRequestCancelled As Boolean
            SyncLock stateLock
                If stopped OrElse requestGeneration <> expectedGeneration Then
                    preparationRequestCancelled = True
                Else
                    trayPreparationRecoveryCode = normalizedTrayCode
                    trayPreparationRetryPending = False
                    currentState = PlcAutomaticTestState.WaitingTrayPreparationRecovery
                End If
            End SyncLock
            If preparationRequestCancelled Then
                ReportAutomaticEventSafely("托盘准备期间流程已停止/复位，未向PLC确认本托盘")
                Return
            End If
            ReportAutomaticEventSafely("托盘准备失败，托盘码已保留，R160未写入；请开启调试模式后重试：" & ex.Message)
            host.ReportAutomaticStatus("托盘准备失败，已保留当前托盘码；开启调试模式后自动重试")
            Return
        End Try
        Dim requestCancelled As Boolean
        SyncLock stateLock
            If stopped OrElse requestGeneration <> expectedGeneration Then
                requestCancelled = True
            Else
                cycle.TrayCode = normalizedTrayCode
                trayConfirmationPending = True
                trayPreparationRecoveryCode = String.Empty
                trayPreparationRetryPending = False
            End If
        End SyncLock
        If requestCancelled Then
            ReportAutomaticEventSafely("托盘准备期间流程已停止/复位，未向PLC确认本托盘")
            Return
        End If
        ConfirmTrayCodeWithPlc()
    End Sub

    Public Sub RequestTrayPreparationRetry()
        If Not host.IsDebugModeEnabled() Then Return
        SyncLock stateLock
            If stopped OrElse currentState <> PlcAutomaticTestState.WaitingTrayPreparationRecovery OrElse
               trayPreparationInProgress OrElse trayPreparationRetryPending OrElse
               String.IsNullOrWhiteSpace(trayPreparationRecoveryCode) Then Return
            trayPreparationRetryPending = True
        End SyncLock
        ReportAutomaticEventSafely("调试模式已开启，准备重试当前托盘")
    End Sub

    Private Sub ConfirmTrayCodeWithPlc()
        Dim manualCycle As Boolean
        SyncLock stateLock
            If Not trayConfirmationPending OrElse cycle.TrayConfirmed OrElse r160WriteInProgress Then Return
            r160WriteInProgress = True
            manualCycle = cycle.IsManualCycle
        End SyncLock
        Try
            ReportAutomaticEventSafely("上位机准备写入PLC寄存器 R160=1")
            plc.WriteR160()
        Catch ex As Exception
            ReportAutomaticEventSafely("上位机写入PLC寄存器 R160=1失败：" & ex.Message)
            Dim retryState As PlcAutomaticTestState = If(manualCycle, PlcAutomaticTestState.PreparingTest, PlcAutomaticTestState.WaitingR150Reset)
            Dim retryMessage As String = If(manualCycle,
                                           "手动托盘码尚未得到R160确认，将重试：" & ex.Message,
                                           "PLC寄存器 R160写入未确认，将重试：" & ex.Message)
            MoveTo(retryState, retryMessage)
            Return
        Finally
            SyncLock stateLock
                r160WriteInProgress = False
            End SyncLock
        End Try
        SyncLock stateLock
            trayConfirmationPending = False
            cycle.TrayConfirmed = True
            trayRequestPending = False
            manualTrayCodePending = False
            nextTrayScanRetryAtUtc = DateTime.MinValue
            If manualCycle Then
                testRequestPending = True
                testPrerequisiteWaitingReported = False
                testTrayConfirmationWaitingReported = False
            End If
        End SyncLock
        If manualCycle Then
            ReportAutomaticEventSafely("手动托盘码已确认，PLC寄存器 R160=1（已确认），开始手选通道测试")
            MoveTo(PlcAutomaticTestState.PreparingTest, "R160=1已确认，开始手选通道测试")
        Else
            ReportAutomaticEventSafely("托盘码已确认，PLC寄存器 R160=1（已确认）")
            MoveTo(PlcAutomaticTestState.WaitingFixturePressed, "托盘准备完成，等待独立的R151测试请求")
        End If
    End Sub

    Private Sub BeginInitialTest()
        Dim isManualCycle As Boolean
        Dim measurementChannels As IReadOnlyList(Of Integer)
        Dim trayCodeSnapshot As String
        Dim initialGeneration As Integer
        SyncLock stateLock
            If stopped OrElse Not cycle.TestStarted OrElse Not cycle.TrayConfirmed OrElse String.IsNullOrWhiteSpace(cycle.TrayCode) Then Return
            isManualCycle = cycle.IsManualCycle
            measurementChannels = cycle.MeasurementChannels
            trayCodeSnapshot = cycle.TrayCode
            initialGeneration = requestGeneration
        End SyncLock

        Dim needNgRetest As Boolean = host.GetNeedNgRetest()
        Dim requiresNgSorting As Boolean = host.RequiresNgSorting()
        Dim maxRetestRounds As Integer = Math.Max(0, host.GetRetestRoundCount())
        SyncLock stateLock
            If Not IsCurrentTestCycleLocked(initialGeneration, trayCodeSnapshot, isManualCycle, measurementChannels) Then Return
            cycle.NeedNgRetest = needNgRetest
            cycle.RequiresNgSorting = requiresNgSorting
            cycle.MaxRetestRounds = maxRetestRounds
            cycle.CurrentRetestRound = 0
            cycle.CurrentNgChannels.Clear()
            cycle.NgGradesWrittenChannels.Clear()
            cycle.FinalResultPublished = False
            cycle.NgGradesWritten = False
            cycle.R161Written = False
        End SyncLock

        If isManualCycle Then
            If Not TryMoveToCurrentManualCycle(initialGeneration, trayCodeSnapshot, measurementChannels,
                                               PlcAutomaticTestState.PreparingTest, "手动启动已确认，开始测试准备") Then Return
        Else
            MoveTo(PlcAutomaticTestState.PreparingTest, "R151已到位，开始测试准备")
        End If
        Dim initialPassDescription As String = If(isManualCycle, "手选通道初次测试", "256通道初次测试")
        ReportAutomaticEventSafely(If(isManualCycle,
                                      "R160=1已确认，开始手选通道初次测试",
                                      "收到R151=1，开始256通道初次测试"))
        If isManualCycle Then
            If Not TryMoveToCurrentManualCycle(initialGeneration, trayCodeSnapshot, measurementChannels,
                                               PlcAutomaticTestState.InitialTesting, "开始" & initialPassDescription) Then Return
        Else
            MoveTo(PlcAutomaticTestState.InitialTesting, "开始" & initialPassDescription)
        End If
        If isManualCycle Then
            Dim manualHost As IManualTestHost = TryCast(host, IManualTestHost)
            If manualHost Is Nothing Then Throw New InvalidOperationException("手动测试宿主不可用。")
            Dim canStart As Func(Of Boolean) = Function() IsCurrentTestCycle(initialGeneration, trayCodeSnapshot, True, measurementChannels)
            Dim completed As Action(Of TestPassResult) =
                Sub(result)
                    If IsCurrentTestCycle(initialGeneration, trayCodeSnapshot, True, measurementChannels) Then InitialPassCompleted(result)
                End Sub
            Dim failed As Action(Of Exception) =
                Sub(ex)
                    If IsCurrentTestCycle(initialGeneration, trayCodeSnapshot, True, measurementChannels) Then TestPassFailed(ex)
                End Sub
            manualHost.StartManualInitialPass(trayCodeSnapshot, measurementChannels, canStart, completed, failed)
        Else
            host.StartInitialPass(AddressOf InitialPassCompleted, AddressOf TestPassFailed)
        End If
    End Sub

    Private Function TryMoveToCurrentManualCycle(ByVal expectedGeneration As Integer,
                                                 ByVal expectedTrayCode As String,
                                                 ByVal expectedChannels As IReadOnlyList(Of Integer),
                                                 ByVal nextState As PlcAutomaticTestState,
                                                 ByVal message As String) As Boolean
        SyncLock stateLock
            If Not IsCurrentTestCycleLocked(expectedGeneration, expectedTrayCode, True, expectedChannels) Then Return False
            currentState = nextState
        End SyncLock
        host.ReportAutomaticStatus(message)
        Return True
    End Function

    Private Function IsCurrentTestCycle(ByVal expectedGeneration As Integer,
                                        ByVal expectedTrayCode As String,
                                        ByVal expectedManualCycle As Boolean,
                                        ByVal expectedChannels As IReadOnlyList(Of Integer)) As Boolean
        SyncLock stateLock
            Return IsCurrentTestCycleLocked(expectedGeneration, expectedTrayCode, expectedManualCycle, expectedChannels)
        End SyncLock
    End Function

    Private Function IsCurrentTestCycleLocked(ByVal expectedGeneration As Integer,
                                              ByVal expectedTrayCode As String,
                                              ByVal expectedManualCycle As Boolean,
                                              ByVal expectedChannels As IReadOnlyList(Of Integer)) As Boolean
        Return Not stopped AndAlso requestGeneration = expectedGeneration AndAlso
               cycle.TestStarted AndAlso cycle.TrayConfirmed AndAlso
               cycle.IsManualCycle = expectedManualCycle AndAlso
               String.Equals(cycle.TrayCode, expectedTrayCode, StringComparison.Ordinal) AndAlso
               expectedChannels IsNot Nothing AndAlso cycle.MeasurementChannels.SequenceEqual(expectedChannels)
    End Function

    Private Sub InitialPassCompleted(ByVal result As TestPassResult)
        If result Is Nothing OrElse Not result.Completed Then
            TestPassFailed(New InvalidOperationException("初次测试未完成。"))
            Return
        End If
        UpdateNgSet(result)
        ReportAutomaticEventSafely(String.Format("初次测试完成，NG通道 {0} 个", cycle.CurrentNgChannels.Count))
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
        ReportAutomaticEventSafely(String.Format("第{0}轮NG复测完成，剩余NG通道 {1} 个", cycle.CurrentRetestRound, cycle.CurrentNgChannels.Count))
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
                ReportAutomaticEventSafely("最终测试结果保存完成")
            End If
            SendTestCompletionSignal()
        Catch ex As Exception
            EnterFault(ex)
        End Try
    End Sub

    Private Sub PublishNgGrades(ByVal selection As PendingNgSelection)
        Try
            Dim ngChannels As List(Of Integer)
            SyncLock stateLock
                If Not Object.ReferenceEquals(pendingNgSelection, selection) Then Return
                ngChannels = selection.Channels.OrderBy(Function(item) item).ToList()
            End SyncLock

            If Not selection.GradesWritten Then
                'R152=1后发送最终NG；每个已确认的固定DT地址只在失败重试时跳过。
                If ngChannels.Count > 0 Then
                    ReportAutomaticEventSafely(String.Format("上位机开始写入PLC NG档位，共{0}个点位", ngChannels.Count))
                End If
                For Each channel As Integer In ngChannels
                    Dim alreadyWritten As Boolean
                    SyncLock stateLock
                        alreadyWritten = selection.WrittenChannels.Contains(channel)
                    End SyncLock
                    If alreadyWritten Then Continue For

                    Dim dtAddress As Integer = 1000 + channel
                    Try
                        ReportAutomaticEventSafely(String.Format("上位机准备写入PLC寄存器 DT{0}=2", dtAddress))
                        plc.WriteNgSelection(channel)
                    Catch ex As Exception
                        ReportAutomaticEventSafely(String.Format("上位机写入PLC寄存器 DT{0}=2失败：{1}", dtAddress, ex.Message))
                        Throw
                    End Try
                    SyncLock stateLock
                        selection.WrittenChannels.Add(channel)
                    End SyncLock
                    ReportAutomaticEventSafely(String.Format("NG档位已写入PLC：DT{0}=2", dtAddress))
                Next
                SyncLock stateLock
                    selection.GradesWritten = True
                End SyncLock
            End If
            SendSortingCompletionSignal(selection)
        Catch ex As Exception
                MoveTo(PlcAutomaticTestState.WaitingR152, "NG档位写入失败，等待R152重试：" & ex.Message)
        End Try
    End Sub

    Private Sub SendTestCompletionSignal()
        Dim blockedByPendingSelection As Boolean
        SyncLock stateLock
            If cycle.R161Written OrElse r161WriteInProgress Then Return
            If cycle.RequiresNgSorting AndAlso pendingNgSelection IsNot Nothing Then
                blockedByPendingSelection = True
            Else
                r161WriteInProgress = True
            End If
        End SyncLock
        If blockedByPendingSelection Then
            MoveTo(PlcAutomaticTestState.WaitingR152, "上一托盘NG结果尚未收到R162，暂缓当前托盘R161确认")
            Return
        End If
        Try
            ReportAutomaticEventSafely("上位机准备写入PLC寄存器 R161=1")
            plc.WriteR161()
        Catch ex As Exception
            ReportAutomaticEventSafely("上位机写入PLC寄存器 R161=1失败：" & ex.Message)
            MoveTo(PlcAutomaticTestState.WaitingR161Ack, "PLC寄存器 R161写入未确认，将重试：" & ex.Message)
            Return
        Finally
            SyncLock stateLock
                r161WriteInProgress = False
            End SyncLock
        End Try
        SyncLock stateLock
            cycle.R161Written = True
            If sortingRequestPending AndAlso Not cycle.RequiresNgSorting AndAlso
               sortingRequestTestStateAtCapture = PlcR152TestState.TestInProgress Then
                sortingRequestTestStateAtCapture = PlcR152TestState.TestCompleted
            End If
            If cycle.RequiresNgSorting Then
                pendingNgSelection = New PendingNgSelection With {
                    .TrayCode = cycle.TrayCode,
                    .Channels = New HashSet(Of Integer)(cycle.CurrentNgChannels)
                }
            End If
            cycle.Reset()
        End SyncLock
        ReportAutomaticEventSafely("测试结果已保存，PLC寄存器 R161=1（已确认）")
        ReportAutomaticEventSafely("测试结果已保存，释放当前托盘上下文，允许开始下一托盘扫码")
        MoveTo(PlcAutomaticTestState.Idle, "测试完成，等待下一托盘R150；上一份NG结果保留至R152")
    End Sub

    Private Sub SendSortingCompletionSignal(ByVal selection As PendingNgSelection)
        If Not selection.GradesWritten Then
            MoveTo(PlcAutomaticTestState.WaitingR152, "等待NG档位全部写入后发送R162")
            Return
        End If
        Try
            ReportAutomaticEventSafely("上位机准备写入PLC寄存器 R162=1")
            plc.WriteR162()
        Catch ex As Exception
            ReportAutomaticEventSafely("上位机写入PLC寄存器 R162=1失败：" & ex.Message)
            MoveTo(PlcAutomaticTestState.WaitingR162Ack, "PLC寄存器 R162写入未确认，将重试：" & ex.Message)
            Return
        End Try
        Dim currentTrayInProgress As Boolean
        SyncLock stateLock
            If Not Object.ReferenceEquals(pendingNgSelection, selection) Then Return
            sortingRequestPending = False
            sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted
            sortingRequestRequiresNgSortingAtCapture = False
            sortingRequestWaitingReported = False
            pendingNgSelection = Nothing
            currentTrayInProgress = Not String.IsNullOrWhiteSpace(cycle.TrayCode)
        End SyncLock
        If selection.Channels.Count > 0 Then
            ReportAutomaticEventSafely(String.Format("托盘{0}的NG档位发送完成（{1}个点位），PLC寄存器 R162=1（已确认）", selection.TrayCode, selection.Channels.Count))
        Else
            ReportAutomaticEventSafely(String.Format("托盘{0}没有NG档位，PLC寄存器 R162=1（已确认）", selection.TrayCode))
        End If
        If Not currentTrayInProgress Then
            MoveTo(PlcAutomaticTestState.Idle, "上一托盘分选握手完成，等待后续PLC请求")
        End If
    End Sub

    Private Sub SendNoSortingCompletionSignal()
        Try
            ReportAutomaticEventSafely("当前机型无需NG分选，直接准备写入PLC寄存器 R162=1")
            plc.WriteR162()
        Catch ex As Exception
            ReportAutomaticEventSafely("上位机写入PLC寄存器 R162=1失败：" & ex.Message)
            MoveTo(PlcAutomaticTestState.WaitingR162Ack, "PLC寄存器 R162写入未确认，将重试：" & ex.Message)
            Return
        End Try
        SyncLock stateLock
            sortingRequestPending = False
            sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted
            sortingRequestRequiresNgSortingAtCapture = False
            sortingRequestWaitingReported = False
        End SyncLock
        ReportAutomaticEventSafely("当前机型无分选要求，PLC寄存器 R162=1（已确认）")
    End Sub

    Private Sub SendEarlySortingCompletionSignal()
        SyncLock stateLock
            If Not sortingRequestPending OrElse
               sortingRequestTestStateAtCapture <> PlcR152TestState.NoTestStarted Then Return
        End SyncLock

        Try
            ReportAutomaticEventSafely("测试尚未开始，跳过R152分选请求并准备直接写入PLC寄存器 R162=1")
            plc.WriteR162()
        Catch ex As Exception
            ReportAutomaticEventSafely("上位机写入PLC寄存器 R162=1失败：" & ex.Message)
            MoveTo(PlcAutomaticTestState.WaitingR162Ack, "R162确认失败，将重试：" & ex.Message)
            Return
        End Try

        Dim waitForR151 As Boolean
        SyncLock stateLock
            If sortingRequestPending AndAlso
               sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted Then
                sortingRequestPending = False
                sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted
                sortingRequestRequiresNgSortingAtCapture = False
                sortingRequestWaitingReported = False
            End If
            waitForR151 = cycle.TrayConfirmed AndAlso Not cycle.TestStarted AndAlso Not cycle.IsManualCycle
        End SyncLock

        ReportAutomaticEventSafely("测试未启动，R152分选请求已跳过，PLC寄存器 R162=1（已确认）")
        If waitForR151 Then
            MoveTo(PlcAutomaticTestState.WaitingFixturePressed, "R152请求已确认，继续等待R151测试请求")
        End If
    End Sub

    Private Sub TestPassFailed(ByVal exception As Exception)
        If exception Is Nothing Then exception = New InvalidOperationException("测试过程失败。")
        If TypeOf exception Is OperationCanceledException Then
            SyncLock stateLock
                requestGeneration += 1
                trayRequestPending = False
                testRequestPending = False
                sortingRequestPending = False
                sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted
                sortingRequestRequiresNgSortingAtCapture = False
                trayPreparationInProgress = False
                manualTrayCodePending = False
                trayRequestBlockedReason = String.Empty
                nextTrayScanRetryAtUtc = DateTime.MinValue
                trayConfirmationPending = False
                testPrerequisiteWaitingReported = False
                testTrayConfirmationWaitingReported = False
                cycle.Reset()
            End SyncLock
            ReportAutomaticEventSafely("自动测试已手动停止，本托盘流程已复位，等待新托盘")
            MoveTo(PlcAutomaticTestState.Idle, "测试已停止，等待托盘进入")
            Return
        End If
        EnterFault(exception)
    End Sub

    Private Sub ResetCycle()
        SyncLock stateLock
            cycle.Reset()
        End SyncLock
        ReportAutomaticEventSafely("设备已复位，等待下一托盘")
        MoveTo(PlcAutomaticTestState.Idle, "上一托盘流程已复位，等待R150")
    End Sub

    Private Sub EnterFault(ByVal exception As Exception)
        If exception Is Nothing Then exception = New InvalidOperationException("PLC自动流程异常。")
        ReportAutomaticEventSafely("自动流程异常：" & exception.Message)
        MoveTo(PlcAutomaticTestState.Fault, "PLC自动流程异常：" & exception.Message)
    End Sub

    Private Sub MoveTo(ByVal nextState As PlcAutomaticTestState, ByVal message As String)
        SyncLock stateLock
            If stopped Then Return
            currentState = nextState
        End SyncLock
        host.ReportAutomaticStatus(message)
    End Sub

    Private Function ReadRegisterSignal(ByVal registerName As String,
                                        ByVal readSignal As Func(Of Boolean),
                                        ByRef readSucceeded As Boolean) As Boolean
        readSucceeded = False
        Dim signalValue As Boolean
        Try
            signalValue = readSignal()
        Catch ex As Exception
            ReportAutomaticEventSafely("PLC寄存器 " & registerName & " 读取失败：" & ex.Message)
            Return False
        End Try
        readSucceeded = True

        Dim hasPreviousValue As Boolean
        Dim previousValue As Boolean
        SyncLock stateLock
            hasPreviousValue = lastReportedRegisterValues.TryGetValue(registerName, previousValue)
            lastReportedRegisterValues(registerName) = signalValue
        End SyncLock

        If host.IsDebugModeEnabled() OrElse Not hasPreviousValue OrElse previousValue <> signalValue Then
            ReportAutomaticEventSafely(String.Format("PLC寄存器 {0}={1}", registerName, If(signalValue, "1", "0")))
        End If
        Return signalValue
    End Function

    Private Sub ReportAutomaticEventSafely(ByVal message As String)
        Try
            host.ReportAutomaticEvent(message)
        Catch ex As Exception
            Debug.WriteLine("PLC automatic event reporting failed: " & ex.Message)
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
    End Sub
End Class
