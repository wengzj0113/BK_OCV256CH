Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
}

function Read-Source([string]$name) {
    $path = Join-Path $root $name
    Assert-True (Test-Path -LiteralPath $path) "missing source file $name"
    return Get-Content -Raw -LiteralPath $path
}

$state = Read-Source 'PlcAutomaticTestState.vb'
$coordinator = Read-Source 'PlcAutomaticTestCoordinator.vb'
$form = Read-Source 'frmTest.vb'
$poll = $coordinator.Substring($coordinator.IndexOf('Private Sub Poll'), $coordinator.IndexOf('Private Function IsAtSafeCycleBoundary') - $coordinator.IndexOf('Private Sub Poll'))

foreach ($name in @('Idle','ReadingTrayCode','WaitingManualTrayCode','WaitingTrayScanRetry','WaitingR150Reset','ArmingR151','WaitingFixturePressed','PreparingTest','InitialTesting','AutoRetesting','Finalizing','WaitingR152','WaitingR161Ack','WaitingR162Ack','WaitingCycleReset','Fault')) {
    Assert-True ($state.Contains($name)) "state $name is missing"
}
Assert-True ($coordinator.Contains('1000')) 'one-second polling interval is missing'
Assert-True ($coordinator.Contains('R150')) 'R150 transition is missing'
Assert-True ($coordinator.Contains('R151')) 'R151 transition is missing'
Assert-True ($coordinator.Contains('ReportAutomaticEventSafely("检测到R150=1，托盘扫码请求已记录")')) 'R150 trigger must be logged independently'
Assert-True ($coordinator.Contains('ReportAutomaticEventSafely("检测到R151=1，测试请求已记录")')) 'R151 trigger must be logged independently'
Assert-True ($coordinator.Contains('ReportAutomaticEventSafely("检测到R152=1，NG分选请求已记录")')) 'R152 trigger must be logged independently'
Assert-True ($coordinator.Contains('R160=1') -and $coordinator.Contains('R151=1')) 'manual and automatic initial-test triggers must be visible in the event log'
Assert-True ($coordinator.Contains('R160')) 'R160 transition is missing'
Assert-True ($coordinator.Contains('R161')) 'R161 transition is missing'
Assert-True ($coordinator.Contains('ReadR152')) 'R152 NG-slot gate is missing'
Assert-True ($coordinator.Contains('WriteR162')) 'R162 NG-handoff completion is missing'
Assert-True ($state.Contains('Public Enum PlcR152TestState')) 'R152 arrival-time test-state model is missing'
foreach ($name in @('NoTestStarted','TestInProgress','TestCompleted','TestFaulted')) {
    Assert-True ($state.Contains($name)) "R152 state $name is missing"
}
Assert-True ($poll.Contains('ReadRegisterSignal("R150", AddressOf plc.ReadR150, r150ReadSucceeded)')) 'R150 must be polled independently in automatic mode'
Assert-True ($poll.Contains('If pollR151 Then r151High = ReadRegisterSignal("R151", AddressOf plc.ReadR151, r151ReadSucceeded)')) 'R151 must gate startup in both manual and automatic modes'
Assert-True ($poll.Contains('ReadRegisterSignal("R152", AddressOf plc.ReadR152, r152ReadSucceeded)')) 'R152 must be polled independently on every coordinator tick'
Assert-True (-not $poll.Contains('Select Case Me.State')) 'input-register polling must not be gated by the current workflow state'
Assert-True ($coordinator.Contains('testRequestPending AndAlso Not cycle.TestStarted AndAlso cycle.TrayConfirmed')) 'an early R151 trigger must be held until its tray is prepared and R160 sent'
Assert-True ($coordinator.Contains('sortingRequestTestStateAtCapture = CaptureR152TestStateLocked()')) 'R152 must retain the test state captured atomically at its rising edge'
Assert-True ($coordinator.Contains('CaptureR152TestStateLocked()')) 'R152 state must be captured atomically with its input edge'
Assert-True ($coordinator.Contains('sortingRequestTestStateAtCapture = PlcR152TestState.NoTestStarted')) 'R152 capture state must clear after acknowledgement or stop'
Assert-True ($coordinator.Contains('sortingRequestPending AndAlso') -and $coordinator.Contains('PlcR152TestState.TestFaulted')) 'faulted test results must not be acknowledged as no NG'
$processPending = $coordinator.Substring($coordinator.IndexOf('Private Sub ProcessPendingRequests'), $coordinator.IndexOf('Private Function IsRequestGenerationCurrent') - $coordinator.IndexOf('Private Sub ProcessPendingRequests'))
Assert-True ($processPending.IndexOf('If sortingArrivedBeforeTest Then PublishNgGrades()') -lt $processPending.IndexOf('Dim startTest As Boolean')) 'pre-test R152 must be acknowledged before a newly captured R151 can start testing'
Assert-True ($coordinator.Contains('selection.WrittenChannels.Add(channel)')) 'confirmed NG grade writes must be tracked for retry'
Assert-True ($coordinator.Contains('RequestManualTrayCode(AddressOf AcceptManualTrayCode')) 'invalid scanned tray code does not trigger manual entry'
Assert-True (-not $coordinator.Contains('ReadR170')) 'obsolete R170 gate remains in automatic flow'
Assert-True (-not $coordinator.Contains('WriteChannelGrade')) 'generic-grade PLC writes remain available to the automatic flow'
Assert-True ($coordinator.Contains('btnNeedNGRetest')) 'automatic retest setting is missing'
Assert-True ($coordinator.Contains('cmbRetestNum')) 'automatic retest count is missing'
Assert-True ($coordinator.Contains('txtTray')) 'tray-code UI callback is missing'
Assert-True ($coordinator.Contains('Interlocked')) 'non-reentrant guard is missing'
Assert-True ($form.Contains('Implements IAutomaticTestHost')) 'frmTest is not connected to the automatic host adapter'
$manualTrayDialog = $form.Substring($form.IndexOf('Public Sub RequestManualTrayCode'), $form.IndexOf('Public Sub StartInitialPass') - $form.IndexOf('Public Sub RequestManualTrayCode'))
Assert-True ($manualTrayDialog.Contains('ExecuteAutomaticOnUi(Sub()')) 'manual tray-code dialog must be dispatched to the UI thread'
Assert-True ($manualTrayDialog.Contains('dialog.ShowDialog(Me)')) 'manual tray-code dialog must be owned by the main form and shown modally'
Assert-True (-not $manualTrayDialog.Contains('Interaction.InputBox')) 'manual tray-code dialog must not use an unowned input box that can be hidden behind the main window'
Assert-True ($form.Contains('Implements IAutomaticTestHost.IsDebugModeEnabled')) 'frmTest does not provide debug-mode state to the PLC event logger'
Assert-True ($form.Contains('PlcAutomaticTestCoordinator')) 'frmTest does not own the PLC coordinator'
Assert-True ($form.Contains('btnNeedNGRetest')) 'NG retest checkbox is missing from frmTest'
Assert-True ($form.Contains('cmbRetestNum')) 'NG retest count control is missing from frmTest'
Assert-True ($form.Contains('StartPlcAutomaticTest')) 'automatic PLC startup hook is missing'
Assert-True ($coordinator.Contains('WriteNgSelection')) 'automatic NG grade publishing is missing'
Assert-True (-not $coordinator.Contains('WriteR150(') -and -not $coordinator.Contains('WriteR151(') -and -not $coordinator.Contains('WriteR152(')) 'PLC-owned input registers must never be written by the application'
$registerReader = $coordinator.Substring($coordinator.IndexOf('Private Function ReadRegisterSignal'), $coordinator.IndexOf('Private Sub ReportAutomaticEventSafely') - $coordinator.IndexOf('Private Function ReadRegisterSignal'))
Assert-True ($coordinator.Contains('Function IsDebugModeEnabled() As Boolean')) 'automatic host must expose the current debug-mode state'
Assert-True ($registerReader.Contains('host.IsDebugModeEnabled() OrElse Not hasPreviousValue OrElse previousValue <> signalValue')) 'register polling events must be logged every read only in debug mode, and on changes in normal mode'
Assert-True ($form.Contains('Not chkAutoTest.Checked Then Return')) 'automatic coordinator must not own ports in manual mode'
Assert-True ($form.Contains('CreateOcvDataTestTask(1, cycle.TrayCode)')) 'final unified-cycle OCV task publication must use the captured tray code'
Assert-True ($form.Contains('If Not automaticMode Then') -and $form.Contains('SaveOcvDataTestResults(ocvTestTaskId, testType)')) 'automatic intermediate pass must not finalize OCV data'
Assert-True (-not $form.Contains("If autoStartFlag = True Then")) 'legacy autoStartFlag is still a formal trigger'

Write-Output 'PASS: PLC v4 state-machine structure'
