param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$policyType = $assembly.GetType('software.InitSettingsApplyPolicy', $true)
$kindType = $assembly.GetType('software.InitSettingsChangeKind', $true)
$canApply = $policyType.GetMethod('CanApplyNow', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $canApply) { throw 'InitSettingsApplyPolicy.CanApplyNow was not found.' }

function Assert-ApplyGate([bool]$testRunning,
                          [bool]$automaticCycleBusy,
                          [bool]$deviceCheckRunning,
                          [bool]$engineeringWindowOpen,
                          [bool]$expected) {
    $actual = [bool]$canApply.Invoke($null, [object[]]@(
        $testRunning,
        $automaticCycleBusy,
        $deviceCheckRunning,
        $engineeringWindowOpen))
    if ($actual -ne $expected) {
        throw "Apply gate expected $expected for running=$testRunning, cycleBusy=$automaticCycleBusy, deviceCheck=$deviceCheckRunning, window=$engineeringWindowOpen; got $actual."
    }
}

Assert-ApplyGate $false $false $false $false $true
Assert-ApplyGate $true $false $false $false $false
Assert-ApplyGate $false $true $false $false $false
Assert-ApplyGate $false $false $true $false $false
Assert-ApplyGate $false $false $false $true $false
$requiresBoundary = $policyType.GetMethod('RequiresSafeBoundary', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $requiresBoundary) { throw 'InitSettingsApplyPolicy.RequiresSafeBoundary was not found.' }
$displayOnly = [Enum]::Parse($kindType, 'Display')
$testRules = [Enum]::Parse($kindType, 'TestRules')
if ([bool]$requiresBoundary.Invoke($null, [object[]]@($displayOnly))) {
    throw 'Display-only configuration must not wait for a test boundary.'
}
if (-not [bool]$requiresBoundary.Invoke($null, [object[]]@($testRules))) {
    throw 'Test rules must wait for a safe test boundary.'
}

$testFormType = $assembly.GetType('software.frmTest', $true)
$testApply = $testFormType.GetMethod('ApplyConfiguredInitialSettings', [Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($null -eq $testApply) { throw 'frmTest.ApplyConfiguredInitialSettings was not found.' }
if ($null -eq $testFormType.GetProperty('CanApplyInitialSettings', [Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    throw 'frmTest.CanApplyInitialSettings was not found.'
}
$mdiType = $assembly.GetType('software.MDIForm', $true)
if ($null -eq $mdiType.GetMethod('CanApplyInitialSettings', [Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    throw 'MDIForm.CanApplyInitialSettings was not found.'
}
if ($null -eq $mdiType.GetMethod('ApplyInitialSettings', [Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    throw 'MDIForm.ApplyInitialSettings was not found.'
}
if ($null -eq $mdiType.GetMethod('SaveAndApplyInitialSettings', [Reflection.BindingFlags]'Public,NonPublic,Instance')) {
    throw 'MDIForm.SaveAndApplyInitialSettings was not found.'
}
$startupType = $assembly.GetType('software.StartupDeviceService', $true)
if ($null -eq $startupType.GetProperty('IsRunning', [Reflection.BindingFlags]'Public,Static')) {
    throw 'StartupDeviceService.IsRunning was not found.'
}
if ($null -eq $assembly.GetType('software.StartupDeviceCheckOptions', $false)) {
    throw 'StartupDeviceCheckOptions was not found.'
}
$resultType = $assembly.GetType('software.StartupDeviceCheckResult', $true)
foreach ($propertyName in @('Retest3562', 'Retest3562Checked')) {
    if ($null -eq $resultType.GetProperty($propertyName, [Reflection.BindingFlags]'Public,Instance')) {
        throw "StartupDeviceCheckResult.$propertyName was not found."
    }
}
$optionsType = $assembly.GetType('software.StartupDeviceCheckOptions', $true)
if ($null -eq $optionsType.GetProperty('ReportRetest3562Failures', [Reflection.BindingFlags]'Public,Instance')) {
    throw 'StartupDeviceCheckOptions.ReportRetest3562Failures was not found.'
}
$failedRetest = [Activator]::CreateInstance($resultType)
$resultType.GetProperty('Retest3562Checked').SetValue($failedRetest, $true)
$resultType.GetProperty('Retest3562').SetValue($failedRetest, [short]-1)
if (-not [bool]$resultType.GetProperty('HasFailures').GetValue($failedRetest)) {
    throw 'A failed checked retest meter must be reported as a device failure.'
}
$coordinatorType = $assembly.GetType('software.PlcAutomaticTestCoordinator', $true)
if ($null -eq $coordinatorType.GetMethod('RequestPauseAtCycleBoundary', [Reflection.BindingFlags]'Public,Instance') -or
    $null -eq $coordinatorType.GetMethod('ResumeAfterPause', [Reflection.BindingFlags]'Public,Instance')) {
    throw 'PLC cycle-boundary pause/resume was not found.'
}

$frmInitSource = Get-Content -LiteralPath (Join-Path $PSScriptRoot '..\frmInit.vb') -Raw
if ($frmInitSource.Contains('现在退出吗？')) {
    throw 'Engineer process settings still request an application restart after saving.'
}
if (-not $frmInitSource.Contains('SaveAndApplyInitialSettings(updatedSettings')) {
    throw 'Engineer process settings do not use the shared save-and-apply coordinator.'
}
$frmTestSource = Get-Content -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb') -Raw
if (-not $frmTestSource.Contains('PLC或扫码器客户端未能按新设置重新启动')) {
    throw 'A failed automatic PLC/scanner restart is not surfaced to the settings coordinator.'
}

Write-Output 'Init settings hot-apply safety gate: PASS'
