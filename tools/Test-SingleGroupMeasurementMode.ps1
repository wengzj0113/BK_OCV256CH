$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$client = Get-Content -Raw -LiteralPath (Join-Path $root 'SerialMiddleMachineClient.vb')

$configureMatch = [regex]::Match(
    $client,
    'Public Sub ConfigurePlan\(.*?\n    End Sub',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $configureMatch.Success) {
    throw 'ConfigurePlan was not found.'
}

$configurePlan = $configureMatch.Value
if ($configurePlan -notmatch 'RegTwoPass') {
    throw 'ConfigurePlan must explicitly configure the two-pass register.'
}
if ($configurePlan -notmatch 'hasG1\s+AndAlso\s+hasG2') {
    throw 'ConfigurePlan must distinguish single-group and dual-group measurement modes.'
}
if ($configurePlan -notmatch 'WriteRegister\(RegTwoPass') {
    throw 'ConfigurePlan must write the selected two-pass mode before START.'
}

$verifyMatch = [regex]::Match(
    $client,
    'Private Sub VerifyPlan\(.*?\n    End Sub',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $verifyMatch.Success) {
    throw 'VerifyPlan was not found.'
}
if ($verifyMatch.Value -notmatch 'RegTwoPass') {
    throw 'VerifyPlan must verify the two-pass register.'
}

Write-Host 'PASS: single-group plans explicitly use single-pass mode and dual-group plans retain two-pass mode.'
