$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$client = Get-Content -Raw -LiteralPath (Join-Path $root 'SerialMiddleMachineClient.vb')
$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'FormalTestRunner.vb')
$manual = Get-Content -Raw -LiteralPath (Join-Path $root 'FrmManualTest.vb')
$required = @(
    'TraceLogger As Action(Of String)',
    'TraceMessage(',
    'New FormalTestRunner(AddressOf WriteManualLog)',
    'TX WRITE',
    'RX WRITE ECHO',
    'TX READ',
    'RX READ',
    'RX EVENT'
)
$all = $client + "`n" + $runner + "`n" + $manual
$missing = @($required | Where-Object { $all -notmatch [regex]::Escape($_) })
if ($missing.Count -gt 0) {
    Write-Host 'MANUAL_TEST_PROTOCOL_LOG: FAIL'
    $missing | ForEach-Object { Write-Host "Missing: $_" }
    exit 1
}
Write-Host 'MANUAL_TEST_PROTOCOL_LOG: PASS'
