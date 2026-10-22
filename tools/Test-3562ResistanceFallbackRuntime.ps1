param([string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\build-verify-numeric-diagnostics\XL_OCV_256CH.exe'))
$ErrorActionPreference = 'Stop'
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$module = $assembly.GetType('software.Module1', $true)
$flags = [Reflection.BindingFlags]'Static,Public,NonPublic'
$portField = $module.GetField('Com_3561', $flags)
$previousPort = $portField.GetValue($null)
$closedPort = New-Object System.IO.Ports.SerialPort
try {
    $portField.SetValue($null, $closedPort)
    $argsForRead = New-Object object[] 3
    $argsForRead[0] = 2
    $code = $module.GetMethod('Trans_ReadCachedData_3562', $flags).Invoke($null, $argsForRead)
    if ($code -ne 0 -or ($argsForRead[2] -join ',') -ne '10000,10000') { throw '3562 read exceptions must return a full resistance fallback batch.' }
} finally {
    $portField.SetValue($null, $previousPort)
    $closedPort.Dispose()
}

$snapshotType = $assembly.GetType('software.CalibrationSnapshot', $true)
$sourceType = $assembly.GetType('software.CalibrationValueSource', $true)
$values = [double[]]::new(256)
for ($i = 0; $i -lt 256; $i++) { $values[$i] = 2 }
$sources = [Array]::CreateInstance($sourceType, 256)
$snapshotArgs = New-Object object[] 5
$snapshotArgs[0] = $values
$snapshotArgs[1] = $sources
$snapshotArgs[2] = 'fallback test'
$snapshotArgs[3] = ''
$snapshotArgs[4] = $false
$snapshotConstructor = $snapshotType.GetConstructors([Reflection.BindingFlags]'Instance,Public,NonPublic')[0]
$snapshot = $snapshotConstructor.Invoke($snapshotArgs)
$planType = $assembly.GetType('software.TestChannelPlanService', $true)
$planService = [Activator]::CreateInstance($planType)
$planArgs = New-Object object[] 1
$planArgs[0] = [int[]]@(241)
$selection = $planType.GetMethod('BuildSparseSelection').Invoke($planService, $planArgs)
$mapperType = $assembly.GetType('software.TwoPassResultMapper', $true)
$mapper = [Activator]::CreateInstance($mapperType)
$parse = $assembly.GetType('software.InstrumentCacheParser', $true).GetMethod('Parse3562MemoryData')
$batType = $assembly.GetType('software.Module1+Bat_info', $true)
$voltage = [System.Collections.Generic.List[single]]::new()
$voltage.Add([single]3.5)
foreach ($fixture in @(@{Text="1,0.005,3.5`r`nEND";Expected=7;Raw=5;Compensation=2}, @{Text="1,1E+10,3.5`r`nEND";Expected=10000;Raw=10000;Compensation=0}, @{Text='';Expected=10000;Raw=10000;Compensation=0})) {
    $parsed = $parse.Invoke($null, @($fixture.Text, 1, $false))
    $data = [Array]::CreateInstance($batType, 256)
    $applyArgs = New-Object object[] 5
    $applyArgs[0] = $selection
    $applyArgs[1] = $parsed.Resistances
    $applyArgs[2] = $voltage
    $applyArgs[3] = $snapshot
    $applyArgs[4] = $data
    $null = $mapperType.GetMethod('Apply').Invoke($mapper, $applyArgs)
    $actual = $data.GetValue(0)
    if ($actual.Resistance -ne $fixture.Expected -or $actual.FinalResistanceMilliohm -ne $fixture.Expected -or $actual.RawResistanceMilliohm -ne $fixture.Raw -or $actual.CalibrationValueMilliohm -ne $fixture.Compensation) { throw "Resistance mismatch: expected=$($fixture.Expected) display=$($actual.Resistance) final=$($actual.FinalResistanceMilliohm) raw=$($actual.RawResistanceMilliohm) compensation=$($actual.CalibrationValueMilliohm)" }
    if ([Math]::Abs($actual.Voltage - 3.5) -gt 0.0001) { throw 'Resistance fallback must preserve the independent DMM voltage.' }
}
$settingsField = $module.GetField('Initsetting', $flags)
$previousSettings = $settingsField.GetValue($null)
try {
    $settings = $settingsField.GetValue($null)
    $settings.upVoltage = [single]5000
    $settings.downVoltage = [single]3000
    $settings.upResistance = [single]20000
    $settings.downResistance = [single]0
    $settingsField.SetValue($null, $settings)
    $ng = $module.GetMethod('IsNG', $flags).Invoke($null, @([single]3.5, [single]10000))
    if ($ng -ne 2) { throw 'Fallback resistance must be NG even if the configured upper limit is wider than the sentinel.' }
} finally { $settingsField.SetValue($null, $previousSettings) }
Write-Output 'PASS: missing resistance becomes 10000 in acquisition, display and storage, bypasses calibration and remains NG.'
