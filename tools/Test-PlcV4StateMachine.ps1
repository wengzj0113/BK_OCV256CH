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

foreach ($name in @('Idle','ReadingTrayCode','WaitingManualTrayCode','WaitingR150Reset','ArmingR151','WaitingFixturePressed','PreparingTest','InitialTesting','AutoRetesting','Finalizing','WaitingCycleReset','Fault')) {
    Assert-True ($state.Contains($name)) "state $name is missing"
}
Assert-True ($coordinator.Contains('1000')) 'one-second polling interval is missing'
Assert-True ($coordinator.Contains('R150')) 'R150 transition is missing'
Assert-True ($coordinator.Contains('R151')) 'R151 transition is missing'
Assert-True ($coordinator.Contains('R160')) 'R160 transition is missing'
Assert-True ($coordinator.Contains('R161')) 'R161 transition is missing'
Assert-True ($coordinator.Contains('btnNeedNGRetest')) 'automatic retest setting is missing'
Assert-True ($coordinator.Contains('cmbRetestNum')) 'automatic retest count is missing'
Assert-True ($coordinator.Contains('txtTray')) 'tray-code UI callback is missing'
Assert-True ($coordinator.Contains('Interlocked')) 'non-reentrant guard is missing'
Assert-True ($form.Contains('Implements IAutomaticTestHost')) 'frmTest is not connected to the automatic host adapter'
Assert-True ($form.Contains('PlcAutomaticTestCoordinator')) 'frmTest does not own the PLC coordinator'
Assert-True ($form.Contains('btnNeedNGRetest')) 'NG retest checkbox is missing from frmTest'
Assert-True ($form.Contains('cmbRetestNum')) 'NG retest count control is missing from frmTest'
Assert-True ($form.Contains('StartPlcAutomaticTest')) 'automatic PLC startup hook is missing'
Assert-True ($coordinator.Contains('WriteChannelGrade')) 'automatic NG grade publishing is missing'
Assert-True ($form.Contains('Not chkAutoTest.Checked Then Return')) 'automatic coordinator must not own ports in manual mode'
Assert-True ($form.Contains('CreateOcvDataTestTask(1)')) 'final automatic OCV task publication is missing'
Assert-True ($form.Contains('If Not automaticMode Then') -and $form.Contains('SaveOcvDataTestResults(ocvTestTaskId, testType)')) 'automatic intermediate pass must not finalize OCV data'
Assert-True (-not $form.Contains("If autoStartFlag = True Then")) 'legacy autoStartFlag is still a formal trigger'

Write-Output 'PASS: PLC v4 state-machine structure'
