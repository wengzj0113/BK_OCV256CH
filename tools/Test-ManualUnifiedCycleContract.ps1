Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$form = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')
$coordinator = Get-Content -Raw -LiteralPath (Join-Path $root 'PlcAutomaticTestCoordinator.vb')

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
}

$start = [regex]::Match($form, '(?ms)^\s*Private Sub CmdStart_Click\(.*?^\s*End Sub')
Assert-True $start.Success 'could not locate the main-page Start Test handler'
Assert-True ($start.Value -match 'RequestManualCycle\(') 'manual Start Test must submit a request to the shared PLC cycle coordinator'
foreach ($forbidden in @('StarTest\(', 'GetSignalTrayStatus\(', 'PrepareTrayForTest\(', 'WriteR160\(')) {
    Assert-True ($start.Value -notmatch $forbidden) "manual Start Test contains a direct test/PLC path: $forbidden"
}

Assert-True ($form.Contains('Implements IManualTestHost')) 'frmTest must provide the manual coordinator host adapter'
Assert-True ($form.Contains('plcAutomaticCoordinator.StartManual()')) 'manual mode must start PLC cycle polling without starting the scanner'
Assert-True ($form.Contains('StartManualInitialPass')) 'manual cycle must have a coordinator callback for the initial pass'
Assert-True ($form.Contains('IsCycleOccupied')) 'the coordinator cycle lock must drive main-page controls'
Assert-True ($coordinator.Contains('If automaticModeEnabled Then r150High = ReadRegisterSignal')) 'manual PLC polling must skip R150 reads'
Assert-True ($coordinator.Contains('manualHost.StartManualInitialPass(trayCodeSnapshot, measurementChannels, canStart')) 'manual initial measurement must consume the frozen tray and channel snapshots and recheck cycle authorization'

$publish = [regex]::Match($form, '(?ms)^\s*Public Function PublishFinalCycle\(.*?^\s*End Function')
Assert-True ($publish.Success -and $publish.Value -match 'cycle\.MeasurementChannels\.ToList\(\)') 'manual final publication must use the cycle channel snapshot'
Assert-True ($publish.Value -match 'CreateOcvDataTestTask\(1, cycle\.TrayCode\)' -and
             $publish.Value -match 'SaveTestDataLocalFile\(RecieveData, cycle\.TrayCode\)') 'manual final persistence must use the cycle tray-code snapshot'
Assert-True ($publish.Value -match 'If Not cycle\.IsManualCycle Then lastPublishedAutomaticTrayCode') 'manual publication must not affect the automatic duplicate-tray guard'

$prepare = [regex]::Match($form, '(?ms)^\s*Private Function PrepareTrayForTest\(.*?^\s*End Function')
Assert-True ($prepare.Success -and $prepare.Value -match 'selectedChannels') 'tray preparation must accept the startup channel snapshot'
Assert-True ($prepare.Value -match 'TrayTestPreparation\.Prepare\(trayCells,.*channelsForPreparation') 'normal tray mapping must use the manual snapshot'

$modeChange = [regex]::Match($form, '(?ms)^\s*Private Sub TestModeCheckedChanged\(.*?^\s*End Sub')
Assert-True ($modeChange.Success -and $modeChange.Value -match 'StopPlcTestCoordinator') 'mode switching must release the current coordinator before recreating it'
Assert-True ($form.Contains('StarTest(1, True, completed, failed, channels, True, trayCodeValue)')) 'coordinated manual measurement must avoid the direct-save/NG-dialog path and use the frozen tray code'
Assert-True ($form.Contains('If canStart Is Nothing OrElse Not canStart() Then Return')) 'a queued manual UI callback must revalidate its PLC cycle before starting measurement'

Write-Output 'PASS: main-page manual entry uses the shared PLC test-cycle coordinator'
