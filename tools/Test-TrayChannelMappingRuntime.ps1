$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath).Path)
$mappingType = $assembly.GetType('software.TrayChannelMapping', $false)
if ($null -eq $mappingType) { throw 'Tray channel mapping is missing.' }
$toDevice = $mappingType.GetMethod('ToDeviceChannel')
$toStandard = $mappingType.GetMethod('ToStandardChannel')

$expected = @{
    1 = 241
    16 = 1
    17 = 242
    241 = 256
    256 = 16
}
foreach ($standard in $expected.Keys) {
    $device = [int]$toDevice.Invoke($null, @([int]$standard))
    if ($device -ne $expected[$standard]) { throw "Standard $standard mapped to device $device, expected $($expected[$standard])." }
}

$seen = [Collections.Generic.HashSet[int]]::new()
for ($standard = 1; $standard -le 256; $standard++) {
    $device = [int]$toDevice.Invoke($null, @($standard))
    if ($device -lt 1 -or $device -gt 256 -or -not $seen.Add($device)) { throw "Invalid or duplicate device channel $device." }
    $roundTrip = [int]$toStandard.Invoke($null, @($device))
    if ($roundTrip -ne $standard) { throw "Device $device returned standard $roundTrip, expected $standard." }
}
if ($seen.Count -ne 256) { throw 'Mapping does not cover all device channels.' }

$selection = [software.TestChannelPlanService]::new().BuildSparseSelection([int[]]@(1, 241))
$mapperType = $assembly.GetType('software.TwoPassResultMapper', $true)
$mapper = [Activator]::CreateInstance($mapperType, $true)
$infoType = $assembly.GetType('software.Module1+Bat_info', $true)
$data = [Array]::CreateInstance($infoType, 256)
$firstCell = $data.GetValue(0)
$firstCell.BatCode = 'CELL-1'
$data.SetValue($firstCell, 0)
$sixteenthCell = $data.GetValue(15)
$sixteenthCell.BatCode = 'CELL-16'
$data.SetValue($sixteenthCell, 15)
$calibration = [double[]]::new(256)
$calibration[0] = 0.5
$calibration[240] = 2.0
$arguments = New-Object object[] 5
$arguments[0] = $selection
$arguments[1] = [single[]]@(5.0, 6.0)
$arguments[2] = [single[]]@(3.1, 3.6)
$arguments[3] = $calibration
$arguments[4] = $data
$mapperType.GetMethod('Apply').Invoke($mapper, $arguments) | Out-Null

$standardCell = $data.GetValue(0)
$sixteenthCell = $data.GetValue(15)
$deviceCell = $data.GetValue(240)
if ($standardCell.BatCode -ne 'CELL-1' -or
    [Math]::Abs($standardCell.Resistance - 7.0) -gt 0.0001 -or
    [Math]::Abs($standardCell.Voltage - 3.6) -gt 0.0001 -or
    $sixteenthCell.BatCode -ne 'CELL-16' -or
    [Math]::Abs($sixteenthCell.Resistance - 6.5) -gt 0.0001 -or
    [Math]::Abs($sixteenthCell.Voltage - 3.1) -gt 0.0001 -or
    $deviceCell.Resistance -ne 0) {
    throw 'G1/G2 device results or their device calibration did not land in the matching standard positions.'
}

$formType = $assembly.GetType('software.frmTest', $true)
$format = $formType.GetMethod('FormatLocalResultRows', [Reflection.BindingFlags]'Static,Public,NonPublic')
$rows = $format.Invoke($null, @($data, 1, [int[]]@(1, 16), [string[]]@('10', '11', '12', '13')))
if ($rows.Count -ne 2 -or $rows[0] -notmatch '^CELL-1,,1,7,3600,' -or
    $rows[1] -notmatch '^CELL-16,,16,6.5,3100,') {
    throw 'Mapped results were not serialized under their standard tray numbers.'
}

$form = [Activator]::CreateInstance($formType)
try {
    $buildDisplay = $formType.GetMethod('BuildChannelDisplayData', [Reflection.BindingFlags]'Instance,NonPublic')
    $position16 = $buildDisplay.Invoke($form, @(16))
    $position17 = $buildDisplay.Invoke($form, @(17))
    if ($position16.Row -ne 1 -or $position16.Column -ne 16 -or
        $position17.Row -ne 2 -or $position17.Column -ne 1) {
        throw 'Main page labels do not follow standard tray rows and columns.'
    }
} finally {
    $form.Dispose()
}

$manualType = $assembly.GetType('software.FrmManualTest', $true)
$manualForm = [Activator]::CreateInstance($manualType)
try {
    $manualType.GetMethod('SelectG1Channels', [Reflection.BindingFlags]'Instance,NonPublic').Invoke($manualForm, @($null, [EventArgs]::Empty)) | Out-Null
    $selected = $manualType.GetMethod('GetSelectedChannels', [Reflection.BindingFlags]'Instance,NonPublic').Invoke($manualForm, $null)
    if ($selected.Count -ne 128 -or -not $selected.Contains(16) -or $selected.Contains(1)) {
        throw 'Manual G1 selection did not select the standard positions connected to G1 device channels.'
    }
} finally {
    $manualForm.Dispose()
}

Write-Output 'PASS: 256 mappings, G1/G2 result placement, local save numbers, and standard UI selections.'
