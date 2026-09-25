$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$type = $assembly.GetType('software.TrayTestPreparation', $true)
$table = [System.Data.DataTable]::new()
[void]$table.Columns.Add('ChannelNo', [int])
[void]$table.Columns.Add('BatteryCode', [string])
[void]$table.Rows.Add(1, 'CELL-1')
[void]$table.Rows.Add(2, [DBNull]::Value)
[void]$table.Rows.Add(3, 'CELL-3')
$selection = [int[]]@(1,3)
$auto = $type.GetMethod('Prepare').Invoke($null, @($table, $selection, $true))
if ($auto.MeasurementChannels.Count -ne 256) { throw 'Automatic mode must measure 256 channels.' }
if (@($auto.OccupiedChannels) -join ',' -ne '1,3') { throw 'Only populated channels are business results.' }
$manual = $type.GetMethod('Prepare').Invoke($null, @($table, [int[]]@(3), $false))
if (@($manual.MeasurementChannels) -join ',' -ne '3') { throw 'Manual mode must honor selection.' }
try {
    [void]$type.GetMethod('Prepare').Invoke($null, @($table, [int[]]@(2), $false))
    throw 'Manual mode accepted a selected channel without a battery code.'
} catch {
    if ($_.Exception.Message -notmatch 'CH2') { throw }
}
[void]$table.Rows.Add(4, 'CELL-3')
try {
    [void]$type.GetMethod('Prepare').Invoke($null, @($table, $selection, $true))
    throw 'Duplicate battery code was accepted.'
} catch {
    if ($_.Exception.Message -notmatch 'CELL-3') { throw }
}
Write-Output 'PASS: tray preparation automatic and manual channel behavior'
