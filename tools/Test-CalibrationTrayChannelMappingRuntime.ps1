$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath).Path)

$runnerType = $assembly.GetType('software.ChannelCalibrationRunner', $true)
$serviceType = $assembly.GetType('software.CalibrationService', $true)
$mappingType = $assembly.GetType('software.TrayChannelMapping', $true)
$runner = [Activator]::CreateInstance($runnerType, @([Activator]::CreateInstance($serviceType, @('calibration-mapping-test.csv'))))
$buildPlan = $runnerType.GetMethod('BuildChannelPlan', [Reflection.BindingFlags]'Instance,NonPublic')
$standardIndexes0 = [Collections.Generic.List[int]]::new()
foreach ($standardChannel in @(2, 16, 17, 241)) { $standardIndexes0.Add($standardChannel - 1) }
$buildPlanArguments = New-Object object[] 1
$buildPlanArguments[0] = $standardIndexes0
$plan = $buildPlan.Invoke($runner, $buildPlanArguments)
$actualDeviceOrder = @($plan.ExecutionOrder)
$expectedDeviceOrder = @(1, 225, 242, 256)
if (($actualDeviceOrder -join ',') -ne ($expectedDeviceOrder -join ',')) {
    throw "校准计划没有按托盘标准通道映射到设备通道。实际=$($actualDeviceOrder -join ',')；预期=$($expectedDeviceOrder -join ',')。"
}

$reindex = $serviceType.GetMethod('MapStandardChannelValuesToDeviceChannels', [Reflection.BindingFlags]'Static,Public')
if ($null -eq $reindex) { throw 'CalibrationService.MapStandardChannelValuesToDeviceChannels is missing.' }
$standardValues = [double[]]::new(256)
$standardValues[1] = 2.0
$standardValues[15] = 16.0
$standardValues[16] = 17.0
$standardValues[240] = 241.0
$reindexArguments = New-Object object[] 1
$reindexArguments[0] = $standardValues
$deviceValues = [double[]]$reindex.Invoke($null, $reindexArguments)
if ($deviceValues[0] -ne 16.0 -or
    $deviceValues[224] -ne 2.0 -or
    $deviceValues[241] -ne 17.0 -or
    $deviceValues[255] -ne 241.0) {
    throw 'Calibration values were not persisted under their mapped device-channel indexes.'
}

$snapshotType = $assembly.GetType('software.CalibrationSnapshot', $true)
$sourceType = $assembly.GetType('software.CalibrationValueSource', $true)
$sources = [Array]::CreateInstance($sourceType, 256)
$snapshotArgs = [object[]]@($deviceValues, $sources, 'mapping regression', '', $false)
$constructor = $snapshotType.GetConstructors([Reflection.BindingFlags]'Instance,Public,NonPublic')[0]
$snapshot = $constructor.Invoke($snapshotArgs)
$getByStandard = $snapshotType.GetMethod('GetCompensationByStandardChannel', [Reflection.BindingFlags]'Instance,Public')
if ($null -eq $getByStandard) { throw 'CalibrationSnapshot.GetCompensationByStandardChannel is missing.' }
if ([double]$getByStandard.Invoke($snapshot, @([int]1)) -ne 0.0 -or
    [double]$getByStandard.Invoke($snapshot, @([int]2)) -ne 2.0 -or
    [double]$getByStandard.Invoke($snapshot, @([int]16)) -ne 16.0 -or
    [double]$getByStandard.Invoke($snapshot, @([int]17)) -ne 17.0 -or
    [double]$getByStandard.Invoke($snapshot, @([int]241)) -ne 241.0) {
    throw 'The calibration page lookup did not retrieve compensation by mapped device channel.'
}

$formType = $assembly.GetType('software.FrmChannelCalibration', $true)
$form = [Activator]::CreateInstance($formType)
try {
    $gridProperty = $formType.GetProperty('grid', [Reflection.BindingFlags]'Instance,Public,NonPublic')
    $grid = $gridProperty.GetValue($form)
    if ($grid.Columns['ChannelNo'].HeaderText -ne '托盘标准通道号' -or
        $grid.Columns['DeviceChannel'].HeaderText -ne '设备通道号') {
        throw 'The calibration grid does not clearly distinguish tray-standard and device channel numbers.'
    }

    for ($standardChannel = 1; $standardChannel -le 256; $standardChannel++) {
        $rowValues = [object[]]::new(9)
        $rowValues[0] = $false
        $rowValues[1] = $standardChannel
        $rowValues[2] = [int]$mappingType.GetMethod('ToDeviceChannel').Invoke($null, @([int]$standardChannel))
        $rowValues[3] = [double]$standardChannel
        for ($column = 4; $column -lt 9; $column++) { $rowValues[$column] = '' }
        $grid.Rows.Add($rowValues) | Out-Null
    }
    $readGridValues = $formType.GetMethod('ReadGridValues', [Reflection.BindingFlags]'Instance,NonPublic')
    $formDeviceValues = [double[]]$readGridValues.Invoke($form, $null)
    if ($formDeviceValues[0] -ne 16.0 -or
        $formDeviceValues[15] -ne 256.0 -or
        $formDeviceValues[224] -ne 2.0 -or
        $formDeviceValues[240] -ne 1.0 -or
        $formDeviceValues[255] -ne 241.0) {
        throw 'The calibration page did not save each displayed tray-standard value in its mapped device-channel slot.'
    }

    $planSummary = $formType.GetMethod('BuildPlanSummary', [Reflection.BindingFlags]'Instance,NonPublic')
    $summaryIndexes0 = [Collections.Generic.List[int]]::new()
    foreach ($standardChannel in @(2, 16, 17, 241)) { $summaryIndexes0.Add($standardChannel - 1) }
    $summaryArguments = New-Object object[] 1
    $summaryArguments[0] = $summaryIndexes0
    $summary = [string]$planSummary.Invoke($form, $summaryArguments)
    if (-not $summary.Contains('CH2→CH225') -or
        -not $summary.Contains('CH16→CH1') -or
        -not $summary.Contains('CH17→CH242') -or
        -not $summary.Contains('CH241→CH256')) {
        throw 'The calibration confirmation does not show the selected standard-to-device channel pairs.'
    }
} finally {
    $form.Dispose()
}

Write-Output 'PASS: calibration selection maps tray-standard channels to device channels, saves device-indexed values, and displays the mapping.'
