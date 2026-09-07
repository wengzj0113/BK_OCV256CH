$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$module = Get-Content (Join-Path $root 'Module1.vb') -Raw
$mdi = Get-Content (Join-Path $root 'MDIForm.vb') -Raw
$client = Get-Content (Join-Path $root 'SerialMiddleMachineClient.vb') -Raw
$startup = Get-Content (Join-Path $root 'StartupDeviceService.vb') -Raw

function Assert-Contains([string]$text, [string]$pattern, [string]$message) {
    if ($text -notmatch $pattern) { throw "FAIL: $message" }
}

Assert-Contains $module 'Public controlBoardConnected As Boolean = False' 'missing explicit control-board connection state'
Assert-Contains $module 'controlBoardConnected\s*=\s*\(reflag_SCM = 1\)' 'startup result does not set explicit control-board state'
Assert-Contains $module 'ApplyCheckResultsToUi[\s\S]*?UpdateDeviceCheckStatus\(' 'startup result does not refresh bottom status'
Assert-Contains $mdi 'StartExternalDeviceChecksAsync[\s\S]*?MainwindowsShowAsync\(Me\)[\s\S]*?StartDatabaseHealthCheckAsync\(\)' 'MDI shown path does not start startup checks'
Assert-Contains $mdi 'dmm34461Connected\s*=\s*\(dmmStatus = 0\)' '34461 success-code mapping is incorrect'
Assert-Contains $mdi 'resistance3561Connected\s*=\s*\(resistanceStatus = 1\)' '3561 success-code mapping is incorrect'
Assert-Contains $mdi 'mesConnected\s*=\s*\(mesStatus = 1\)' 'MES success-code mapping is incorrect'
Assert-Contains $module 'New SerialMiddleMachineClient[\s\S]*?CheckConnection' 'SCM check does not perform protocol handshake'

$updateBlock = [regex]::Match($mdi, 'Private Sub UpdateBottomStatus\(\)[\s\S]*?\r?\n    End Sub').Value
Assert-Contains $updateBlock 'controlBoardConnected' 'bottom status is not driven by control-board state'
if ($updateBlock -match 'mSock') { throw 'FAIL: bottom status still uses obsolete TCP mSock state' }
Assert-Contains $client 'Public Function CheckConnection' 'serial client has no non-destructive connection check'
Assert-Contains $startup 'FailureReasons' 'startup result does not retain per-device failure reasons'
Assert-Contains $mdi 'ShowStartupDeviceFailurePrompt' 'MDI form has no startup failure prompt entry point'
Assert-Contains $mdi 'MessageBox\.Show' 'startup failure prompt does not show a message box'
Assert-Contains $module 'ShowStartupDeviceFailurePrompt\(result\)' 'startup completion does not display the failure summary'
Assert-Contains $startup 'CheckCom_SCM\(True\)' 'SCM startup check does not pass through its detailed failure reason'
Assert-Contains $module 'rethrowOnFailure\s+Then Throw New System\.IO\.IOException' 'SCM check cannot pass its detailed failure reason to startup'
Assert-Contains $mdi 'IpAddOf34461' 'startup failure summary omits the DMM address'
Assert-Contains $mdi 'MesApiAddress' 'startup failure summary omits the MES address'
Assert-Contains $mdi 'FormatPlcDisplayName' 'startup failure summary omits the PLC communication address'

Write-Output 'CONTROL_BOARD_STATUS: PASS'
