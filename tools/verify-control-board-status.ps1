$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$module = Get-Content (Join-Path $root 'Module1.vb') -Raw
$mdi = Get-Content (Join-Path $root 'MDIForm.vb') -Raw
$client = Get-Content (Join-Path $root 'SerialMiddleMachineClient.vb') -Raw

function Assert-Contains([string]$text, [string]$pattern, [string]$message) {
    if ($text -notmatch $pattern) { throw "FAIL: $message" }
}

Assert-Contains $module 'Public controlBoardConnected As Boolean = False' 'missing explicit control-board connection state'
Assert-Contains $module 'controlBoardConnected\s*=\s*\(reflag_SCM = 1\)' 'startup result does not set explicit control-board state'
Assert-Contains $module 'ApplyCheckResultsToUi[\s\S]*?UpdateBottomStatusForDeviceCheck\(\)' 'startup result does not refresh bottom status'
Assert-Contains $module 'New SerialMiddleMachineClient[\s\S]*?CheckConnection' 'SCM check does not perform protocol handshake'

$updateBlock = [regex]::Match($mdi, 'Private Sub UpdateBottomStatus\(\)[\s\S]*?\r?\n    End Sub').Value
Assert-Contains $updateBlock 'controlBoardConnected' 'bottom status is not driven by control-board state'
if ($updateBlock -match 'mSock') { throw 'FAIL: bottom status still uses obsolete TCP mSock state' }
Assert-Contains $client 'Public Function CheckConnection' 'serial client has no non-destructive connection check'

Write-Output 'CONTROL_BOARD_STATUS: PASS'
