$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$source = Get-Content -LiteralPath (Join-Path $root 'frmTest.vb') -Raw
$match = [regex]::Match($source, '(?ms)^\s*Public Function StarTest\(.*?^\s*End Function')
if (-not $match.Success) {
    throw 'Could not isolate frmTest.StarTest.'
}

$forbidden = @('plcConnected', 'plcController', 'GetSignalTrayStatus', 'CloseTray', 'TrayOpen')
foreach ($name in $forbidden) {
    if ($match.Value -match [regex]::Escape($name)) {
        throw "StarTest must not control PLC lifecycle: found '$name'."
    }
}

Write-Host 'PASS: StarTest leaves PLC preparation, completion, and fixture actions to the PLC workflow.'
