$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$table = [System.Data.DataTable]::new()
[void]$table.Columns.Add('ChannelNo', [int])
[void]$table.Columns.Add('BatteryCode', [string])
[void]$table.Rows.Add(1, 'CELL-1')
[void]$table.Rows.Add(3, 'CELL-3')
[void]$table.Rows.Add(16, 'CELL-16')
[void]$table.Rows.Add(241, 'CELL-241')
[void]$table.Rows.Add(256, 'CELL-256')
$planType = $assembly.GetType('software.TrayTestPreparation', $true)
$plan = $planType.GetMethod('Prepare').Invoke($null, @($table, [int[]]@(1,2,3,16,241,256), $true))
$data = $assembly.GetType('software.Module1', $true).GetField('RecieveData').GetValue($null)
foreach ($channel in @(1,3,16,241,256)) {
    $cell = $data.GetValue($channel - 1)
    $cell.Voltage = [single]($channel / 1000.0)
    $cell.Resistance = [single]$channel
    $data.SetValue($cell, $channel - 1)
}
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
try {
    $formType.GetField('activeTrayPlan', [Reflection.BindingFlags]'Instance,NonPublic').SetValue($form, $plan)
    $method = $formType.GetMethod('BuildAutomaticPassResult', [Reflection.BindingFlags]'Instance,NonPublic')
    $arguments = New-Object object[] 1
    $arguments[0] = [int[]]@(1,2,3,16,241,256)
    $result = $method.Invoke($form, $arguments)
    if ($result.Results.Count -ne 5) { throw "Expected five occupied results, got $($result.Results.Count)" }
    if (@($result.Results | ForEach-Object ChannelNumber) -join ',' -ne '1,3,16,241,256') {
        throw 'Automatic pass did not preserve standard tray numbers or included an empty slot.'
    }
    foreach ($item in $result.Results) {
        if ($item.Resistance -ne $item.ChannelNumber -or
            [Math]::Abs($item.Voltage - $item.ChannelNumber / 1000.0) -gt 0.00001) {
            throw "Automatic result $($item.ChannelNumber) did not read its standard-numbered result slot."
        }
    }
} finally {
    $form.Dispose()
}
Write-Output 'PASS: automatic results use standard tray numbers and exclude empty slots'
