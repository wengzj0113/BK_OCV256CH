$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$frmTest = Get-Content -LiteralPath (Join-Path $root 'frmTest.vb') -Raw
$designer = Get-Content -LiteralPath (Join-Path $root 'frmTestProgress.Designer.vb') -Raw

foreach ($name in @('progressTest', 'lblProgressStage', 'lblProgressCount', 'lblProgressElapsed', 'lblProgressEta', 'lblProgressDetail', 'txtProgressLog', 'btnClose')) {
    if ($designer -notmatch [regex]::Escape($name)) {
        throw "Designer progress control '$name' is missing."
    }
}

foreach ($method in @('BeginTestProgress', 'PublishTestProgress', 'CompleteTestProgress', 'FailTestProgress', 'ApplyTestProgressSnapshot')) {
    if ($frmTest -notmatch "\b$method\b") {
        throw "frmTest progress method '$method' is missing."
    }
}

foreach ($stage in @('CreatingTask', 'Preparing', 'Testing', 'CollectingResults', 'Saving', 'Uploading')) {
    if ($frmTest -notmatch "TestProgressStage\.$stage") {
        throw "frmTest does not publish stage '$stage'."
    }
}

if ($frmTest -notmatch 'TimerTestProgress_Tick') {
    throw 'The progress refresh timer is not wired.'
}
if ($frmTest -notmatch 'testProgressDialog\.Show\(Me\)') {
    throw 'The progress dialog is not shown with frmTest as owner.'
}
if ($frmTest -match 'grpTestProgress|lblTestProgress|progressTest\.Value') {
    throw 'The old left-sidebar progress card is still referenced by frmTest.'
}

Write-Output 'Test progress feedback contract: PASS'
