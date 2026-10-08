$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'

$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
try {
    $debugCheck = $form.Controls.Find('chk_Test', $true) | Select-Object -First 1
    if ($null -eq $debugCheck) { throw 'The main form does not expose the existing chk_Test checkbox.' }
    if ($debugCheck.Checked) { throw 'Debug mode must default to unchecked.' }
    $form.SelectedChannelNumbers = [System.Collections.Generic.List[int]]::new([int[]]@(1, 16))
    $debugCheck.Checked = $true
    $createPlan = $formType.GetMethod('CreateDebugTrayPlan', $flags)
    $plan = $createPlan.Invoke($form, @($false))
    $dateText = [DateTime]::Now.ToString('yyyyMMdd', [Globalization.CultureInfo]::InvariantCulture)
    if (@($plan.MeasurementChannels) -join ',' -ne '1,16' -or
        @($plan.OccupiedChannels) -join ',' -ne '1,16' -or
        $plan.BatteryCodes[1] -ne "TEST-$dateText-1" -or
        $plan.BatteryCodes[16] -ne "TEST-$dateText-16") {
        throw 'Manual debug plan did not assign generated codes to the selected channels.'
    }

    $module = $assembly.GetType('software.Module1', $true)
    $receiveData = $module.GetField('RecieveData').GetValue($null)
    $firstCell = $receiveData.GetValue(0)
    if ($firstCell.BatCode -ne "TEST-$dateText-1") { throw 'Generated debug code was not copied to the channel result.' }

    $debugCheck.Checked = $false
    if ($formType.GetField('activeTrayPlan', $flags).GetValue($form) -ne $null) {
        throw 'Changing the debug mode must invalidate the cached tray plan.'
    }
    $firstCell = $receiveData.GetValue(0)
    if (-not [string]::IsNullOrEmpty($firstCell.BatCode)) { throw 'Changing the debug mode must clear generated cell codes.' }
} finally {
    $form.Dispose()
}

$queryType = $assembly.GetType('software.frmInquery', $true)
$queryForm = [Activator]::CreateInstance($queryType)
try {
    $queryCheck = $queryForm.Controls.Find('chkDebugResults', $true) | Select-Object -First 1
    if ($null -eq $queryCheck) { throw 'The query form does not expose its debug result selector.' }
    $expectedDebugLabel = [string]([char]0x8C03) + [char]0x8BD5 + [char]0x7ED3 + [char]0x679C
    if ($queryCheck.Checked -or $queryCheck.Text -ne $expectedDebugLabel) {
        throw "The result query must default to production data and label its debug selector (Checked=$($queryCheck.Checked), Text=$($queryCheck.Text))."
    }
} finally {
    $queryForm.Dispose()
}

Write-Output 'Debug mode runtime: PASS'
