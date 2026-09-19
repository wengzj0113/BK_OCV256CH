$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$sourcePath = Join-Path $root 'TestProgressState.vb'
if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw 'TestProgressState.vb is missing.'
}

$source = Get-Content -LiteralPath $sourcePath -Raw

foreach ($stage in @('Idle', 'Preparing', 'Testing', 'CollectingResults', 'Saving', 'Completed', 'Failed')) {
    if ($source -notmatch "\b$stage\b") {
        throw "Test progress stage '$stage' is missing."
    }
}

foreach ($member in @('CompletedCount', 'TotalCount', 'Elapsed', 'EstimatedRemaining', 'CurrentChannel', 'Message')) {
    if ($source -notmatch "\b$member\b") {
        throw "Test progress member '$member' is missing."
    }
}

foreach ($method in @('Reset', 'Begin', 'SetStage', 'Advance', 'Complete', 'Fail', 'CreateSnapshot')) {
    if ($source -notmatch "\b$method\b") {
        throw "Test progress method '$method' is missing."
    }
}

if ($source -notmatch 'EstimatedRemaining[\s\S]*Nothing') {
    throw 'ETA must remain unknown until a progress sample exists.'
}

Write-Output 'Test progress state contract: PASS'
