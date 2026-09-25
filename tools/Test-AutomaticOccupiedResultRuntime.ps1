$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$table = [System.Data.DataTable]::new()
[void]$table.Columns.Add('ChannelNo', [int])
[void]$table.Columns.Add('BatteryCode', [string])
[void]$table.Rows.Add(1, 'CELL-1')
[void]$table.Rows.Add(3, 'CELL-3')
$planType = $assembly.GetType('software.TrayTestPreparation', $true)
$plan = $planType.GetMethod('Prepare').Invoke($null, @($table, [int[]]@(1,2,3), $true))
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
try {
    $formType.GetField('activeTrayPlan', [Reflection.BindingFlags]'Instance,NonPublic').SetValue($form, $plan)
    $method = $formType.GetMethod('BuildAutomaticPassResult', [Reflection.BindingFlags]'Instance,NonPublic')
    $arguments = New-Object object[] 1
    $arguments[0] = [int[]]@(1,2,3)
    $result = $method.Invoke($form, $arguments)
    if ($result.Results.Count -ne 2) { throw "Expected two occupied results, got $($result.Results.Count)" }
    if (@($result.Results | ForEach-Object ChannelNumber) -join ',' -ne '1,3') { throw 'Automatic pass included an empty slot as a battery result.' }
} finally {
    $form.Dispose()
}
Write-Output 'PASS: empty measured slots are excluded from battery results'
