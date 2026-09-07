$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$frmTest = Get-Content -LiteralPath (Join-Path $root 'frmTest.vb') -Raw

$starTest = [regex]::Match($frmTest, '(?ms)^\s*Public Function StarTest\(.*?^\s*End Function')
if (-not $starTest.Success) {
    throw 'Could not isolate frmTest.StarTest.'
}
if ($starTest.Value -match 'CloseTray|TrayOpen|GetSignalTrayStatus|plcConnected|plcController') {
    throw 'The main test page must not control the PLC fixture lifecycle.'
}

Write-Host 'PASS: main test no longer has a silent PLC fixture-control branch.'
