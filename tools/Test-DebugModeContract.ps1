$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$code = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')
$designer = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.Designer.vb')

if ($designer -notmatch 'Me\.chk_Test\.Name\s*=\s*"chk_Test"') {
    throw 'The existing chk_Test control must remain the debug mode switch.'
}
if ($code -notmatch 'Private Function CreateDebugTrayPlan\(' -or
    $code -notmatch 'DebugBarcodePolicy\.CreateCodes') {
    throw 'Debug tray plan must use the shared generated barcode policy.'
}

$prepare = [regex]::Match($code, '(?ms)Private Function PrepareTrayForTest\(.*?^\s*End Function').Value
if ($prepare -notmatch 'chk_Test\.Checked' -or
    $prepare -notmatch 'CreateDebugTrayPlan' -or
    $prepare -notmatch 'GetTrayCellsByTrayCode') {
    throw 'Tray preparation must branch between generated debug codes and the existing real-code lookup.'
}

$modeHandler = [regex]::Match($code, '(?ms)Private Sub DebugTestModeCheckedChanged\(.*?^\s*End Sub').Value
if ($modeHandler -notmatch 'Handles\s+chk_Test\.CheckedChanged' -or
    $modeHandler -notmatch 'activeTrayPlan\s*=\s*Nothing' -or
    $modeHandler -notmatch 'BatCode\s*=\s*String\.Empty') {
    throw 'Changing debug mode must invalidate the active plan and all cached cell codes.'
}
if ($modeHandler -notmatch 'WaitingTrayPreparationRecovery' -or
    $modeHandler -notmatch 'RequestTrayPreparationRetry') {
    throw 'Enabling debug mode during tray-preparation recovery must retry the retained tray.'
}

$visuals = [regex]::Match($code, '(?ms)Private Sub UpdateTestModeVisuals\(\).*?^\s*End Sub').Value
if ($visuals -notmatch 'chk_Test\.Enabled' -or
    $visuals -notmatch 'automaticTestPassInProgress' -or
    $visuals -notmatch 'plcAutomaticCoordinator' -or
    $visuals -notmatch 'WaitingTrayPreparationRecovery') {
    throw 'Debug mode must be locked during a manual pass and a complete PLC tray cycle, but enabled during preparation recovery.'
}

$automaticStatus = [regex]::Match($code, '(?ms)Public Sub ReportAutomaticStatus\(.*?^\s*End Sub').Value
if ($automaticStatus -notmatch 'UpdateTestModeVisuals') {
    throw 'PLC state changes must refresh the debug mode lock state.'
}

$uploadGuards = [regex]::Matches($code, 'Not\s+activeTrayDebugMode\s+AndAlso\s+FrmMesTask\.chkResultDataUploadAssembly\.Checked')
if ($uploadGuards.Count -ne 2) {
    throw 'Both manual and PLC automatic production MES upload paths must be disabled for debug runs.'
}

Write-Output 'Debug mode form contract: PASS'
