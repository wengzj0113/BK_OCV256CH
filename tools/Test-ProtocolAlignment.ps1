$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$client = Get-Content -LiteralPath (Join-Path $root 'SerialMiddleMachineClient.vb') -Raw

if ($client -notmatch 'Public Sub ResetToDefaults\(') {
    throw 'Missing protocol default restore entry point.'
}
if ($client -notmatch 'ResetToDefaults[\s\S]{0,500}RegStartStop[\s\S]{0,100}&H6') {
    throw 'Protocol default restore does not write 0x0030=0x0006.'
}
if ($client -notmatch 'Private Shared Function IsSynchronizedPlan\(') {
    throw 'Missing synchronized-plan classifier.'
}
if ($client -notmatch 'IsSynchronizedPlan\(plan\)') {
    throw 'Plan description/configuration does not use synchronized-plan classification.'
}

$configure = [regex]::Match($client, 'Public Sub ConfigurePlan\([\s\S]*?\r?\n    End Sub').Value
if ($configure -match 'WriteRegister\(RegTwoPass') {
    throw 'ConfigurePlan still overrides the protocol default two-pass setting.'
}
if ($configure -match 'WriteRegister\(RegMeasureItems') {
    throw 'ConfigurePlan still overrides the protocol default measurement-item setting.'
}
if ($configure -notmatch 'WriteRegister\(RegGroupEnable, GetGroupEnableValue') {
    throw 'ConfigurePlan does not explicitly set group enable.'
}
if ($configure -notmatch 'WriteRegister\(RegSpeed, speedValue\)') {
    throw 'ConfigurePlan does not write the explicit speed value.'
}
if ($client -notmatch 'Public Sub StartTest\([\s\S]{0,120}WriteRegister\(RegStartStop, &H1\)') {
    throw 'StartTest does not use the protocol start register.'
}

Write-Output 'PASS: middle-machine configuration is aligned with the reference protocol structure.'
