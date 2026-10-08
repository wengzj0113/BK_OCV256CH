$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
function Read-Source([string]$name) {
    return Get-Content -LiteralPath (Join-Path $root $name) -Raw
}
function Assert-Contains([string]$text, [string]$pattern, [string]$message) {
    if ($text -notmatch $pattern) { throw $message }
}

$client = Read-Source 'SerialMiddleMachineClient.vb'
$runner = Read-Source 'FormalTestRunner.vb'
$form = Read-Source 'frmTest.vb'
$coordinator = Read-Source 'PlcAutomaticTestCoordinator.vb'

Assert-Contains $client 'WaitForCompletion\(timeoutMs As Integer,[\s\S]{0,180}cancellationRequested As Func\(Of Boolean\)' 'Middle-machine completion wait has no cancellation callback.'
Assert-Contains $client 'Private Sub ThrowIfCancellationRequested[\s\S]{0,360}StopAndClearLists\(\)[\s\S]{0,260}OperationCanceledException' 'Cancellation does not send STOP and clear the middle-machine lists.'
Assert-Contains $client 'Application\.DoEvents\(\)[\s\S]{0,100}ThrowIfCancellationRequested\(cancellationRequested\)' 'Completion wait does not process button events before checking cancellation.'
Assert-Contains $runner 'middleMachine\.RunPlan\([\s\S]{0,220}cancellationRequested' 'FormalTestRunner does not pass cancellation to the middle machine.'
Assert-Contains $form 'btnStop\.Enabled\s*=\s*isRuning[\s\S]{0,80}blstop' 'Stop button is not enabled only for an active, not-yet-cancelled run.'
Assert-Contains $form 'Private Sub btnStop_Click[\s\S]{0,260}blstop\s*=\s*True' 'Stop button does not request cancellation.'
Assert-Contains $form 'formalRunner\.RunSparseMeasurement\(deviceChannels[\s\S]{0,180}Function\(\) blstop' 'Manual and automatic measurement do not share the stop signal.'
Assert-Contains $form 'If TypeOf ex Is OperationCanceledException OrElse blstop[\s\S]{0,500}CompleteOcvDataTestTaskSafely\(ocvTestTaskId, "Canceled"[\s\S]{0,300}ClearIncompleteTestResults\(runChannels\)[\s\S]{0,300}GoTo exit_sub' 'Cancellation does not mark the task Canceled and exit before result persistence.'
Assert-Contains $form 'Private Sub ClearIncompleteTestResults[\s\S]{0,500}activeTrayPlan = Nothing[\s\S]{0,220}txtTray\.Clear\(\)' 'Manual and automatic cancellation do not clear the in-memory tray/test state.'
Assert-Contains $coordinator 'TypeOf exception Is OperationCanceledException[\s\S]{0,500}sortingRequestPending\s*=\s*False[\s\S]{0,260}cycle\.Reset\(\)' 'Automatic cancellation does not clear the active tray and its pending R152 request.'
Assert-Contains $coordinator 'trayRequestPending\s*=\s*False[\s\S]{0,200}testRequestPending\s*=\s*False[\s\S]{0,200}sortingRequestPending\s*=\s*False' 'Automatic cancellation does not clear all pending PLC requests.'
Assert-Contains $coordinator 'requestGeneration\s*\+=\s*1[\s\S]{0,600}cycle\.Reset\(\)' 'An in-flight PLC poll can restore pre-stop requests after cancellation.'
Assert-Contains (Read-Source 'TestProgressState.vb') 'Public Sub Cancel\(message As String\)[\s\S]{0,100}TestProgressStage\.Canceled' 'Progress UI has no distinct canceled state.'

Write-Output 'PASS: stop-test cancellation contract is present.'
