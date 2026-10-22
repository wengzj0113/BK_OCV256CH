$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$form = Get-Content -LiteralPath (Join-Path $root 'frmTest.vb') -Raw
$reportMatch = [regex]::Match($form, '(?ms)^\s*Public Sub ReportAutomaticEvent\(.*?^\s*End Sub')
if (-not $reportMatch.Success) { throw 'Could not isolate frmTest.ReportAutomaticEvent.' }

if ($reportMatch.Value -notmatch 'automaticEventLogWriter\.TryEnqueue') {
    throw 'Automatic event file logging must be queued to the background writer.'
}
if ($reportMatch.Value -match 'AppendAutomaticEventLog|File\.AppendAllText') {
    throw 'ReportAutomaticEvent must not write the event log file on the UI thread.'
}

$writerPath = Join-Path $root 'AutomaticEventLogWriter.vb'
if (-not (Test-Path -LiteralPath $writerPath)) { throw 'AutomaticEventLogWriter.vb is missing.' }
$writer = Get-Content -LiteralPath $writerPath -Raw
if ($writer -notmatch 'BlockingCollection') { throw 'The event writer must use a background queue.' }
if ($writer -notmatch 'GetConsumingEnumerable') { throw 'The event writer must drain queued entries in order.' }

Write-Output 'PASS: automatic event file logging is isolated from the UI thread.'
