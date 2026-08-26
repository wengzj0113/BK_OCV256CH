$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$formal = Get-Content -LiteralPath (Join-Path $root 'FormalTestRunner.vb') -Raw
$client = Get-Content -LiteralPath (Join-Path $root 'SerialMiddleMachineClient.vb') -Raw
$calibration = Get-Content -LiteralPath (Join-Path $root 'ChannelCalibrationRunner.vb') -Raw

if ($formal -notmatch 'Optional timeoutMs As Integer = 300000') {
    throw 'Formal test timeout default is not 300000 ms.'
}

if ($calibration -notmatch 'MiddleMachineTimeoutMs As Integer = 300000') {
    throw 'Calibration timeout is not 300000 ms.'
}

foreach ($method in @('RunPlan', 'RunSingleRange', 'RunChannelList', 'RunTwoRanges')) {
    if ($client -notmatch ('(?s)' + $method + '.*?Optional timeoutMs As Integer = DefaultTestTimeoutMs')) {
        throw ($method + ' does not use the 300-second test timeout default.')
    }
}

$planCalls = [regex]::Matches($client, 'ConfigurePlan\(plan, CUShort\(3\)\)')
if ($planCalls.Count -ne 4) {
    throw "Expected 4 plan configuration calls, found $($planCalls.Count)."
}

foreach ($call in $planCalls) {
    if ($call.Value -ne 'ConfigurePlan(plan, CUShort(3))') {
        throw "Plan configuration is not using high-speed mode: $($call.Value)"
    }
}

Write-Output 'PASS: formal timeout is 300000 ms and all plan paths use fastest speed.'
