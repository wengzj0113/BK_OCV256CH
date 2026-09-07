$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$frmTest = Get-Content -LiteralPath (Join-Path $root 'frmTest.vb') -Raw

if ($frmTest -notmatch 'Dim runChannels As (?:New )?List\(Of Integer\)') {
    throw 'StarTest must snapshot the selected physical channels before starting the test.'
}
if ($frmTest -notmatch 'currentTestChannels\s*=\s*runChannels\.ToList\(\)') {
    throw 'The UI result path must use the same channel snapshot as the hardware test.'
}
if ($frmTest -notmatch 'RunSparseMeasurement\(runChannels\)') {
    throw 'The main test page must pass the channel snapshot to sparse measurement.'
}
if ($frmTest -notmatch 'firstCellNumToTest\s*=\s*runChannels\.Min\(\)' -or
    $frmTest -notmatch 'lastCellNumToTest\s*=\s*runChannels\.Max\(\)' -or
    $frmTest -notmatch 'totalCellNumToTest\s*=\s*runChannels\.Count') {
    throw 'Legacy display range values must be derived from the selected channel snapshot.'
}
if ($frmTest -notmatch 'isPoint\s*=\s*selectedPhysicalChannels\.Contains\(') {
    throw 'Legacy battery rendering must honor the selected channel snapshot.'
}

Write-Host 'PASS: main test uses one stable channel snapshot for execution and display.'
