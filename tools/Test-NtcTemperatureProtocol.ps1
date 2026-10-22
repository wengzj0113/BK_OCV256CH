param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
$assemblyFullPath = (Resolve-Path -LiteralPath $AssemblyPath).Path
$assembly = [Reflection.Assembly]::LoadFrom($assemblyFullPath)
$clientType = $assembly.GetType('software.NtcTemperatureModuleClient', $true)
$readingType = $assembly.GetType('software.NtcTemperatureReading', $true)
$formType = $assembly.GetType('software.frmTest', $true)
$decode = $clientType.GetMethod('DecodeRegister', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $decode) { throw 'NtcTemperatureModuleClient.DecodeRegister was not found.' }
$validateUnit = $clientType.GetMethod('ValidateTemperatureUnit', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $validateUnit) { throw 'NtcTemperatureModuleClient.ValidateTemperatureUnit was not found.' }

function Assert-Reading([UInt16]$raw, [bool]$valid, [decimal]$temperature, [string]$caseName) {
    $actual = $decode.Invoke($null, [object[]]@($raw))
    $actualValid = [bool]$readingType.GetProperty('IsValid').GetValue($actual, $null)
    $actualTemperature = [decimal]$readingType.GetProperty('TemperatureCelsius').GetValue($actual, $null)
    if ($actualValid -ne $valid) {
        throw "${caseName}: expected IsValid=$valid, got $actualValid."
    }
    if ($actualTemperature -ne $temperature) {
        throw "${caseName}: expected ${temperature}C, got ${actualTemperature}C."
    }
}

Assert-Reading 0x00FA $true 25.0 'positive temperature'
Assert-Reading 0xFFFF $true -0.1 'negative one tenth degree'
Assert-Reading 0xFFF6 $true -1.0 'negative one degree'
Assert-Reading 0x7FFF $false 0 'sensor fault marker'
Assert-Reading 0x8000 $true -3276.8 'signed Int16 lower boundary'

$formatReading = $formType.GetMethod('FormatTemperatureReading', [Reflection.BindingFlags]'NonPublic,Static')
if ($null -eq $formatReading) { throw 'frmTest.FormatTemperatureReading was not found.' }
$positiveReading = $decode.Invoke($null, [object[]]@([UInt16]0x00FA))
$faultReading = $decode.Invoke($null, [object[]]@([UInt16]0x7FFF))
if ([string]$formatReading.Invoke($null, [object[]]@($positiveReading)) -ne '25.0') {
    throw 'A valid 25.0°C reading must display as 25.0.'
}
if ([string]$formatReading.Invoke($null, [object[]]@($faultReading)) -ne '探头故障') {
    throw 'The LES30 sensor-fault marker must display as 探头故障.'
}

$testForm = [Activator]::CreateInstance($formType)
try {
    $updateReadings = $formType.GetMethod('UpdateTemperatureReadings', [Reflection.BindingFlags]'NonPublic,Instance')
    if ($null -eq $updateReadings) { throw 'frmTest.UpdateTemperatureReadings was not found.' }
    $channelOneField = $formType.GetField('_t1', [Reflection.BindingFlags]'NonPublic,Instance')
    $channelTwoField = $formType.GetField('_t2', [Reflection.BindingFlags]'NonPublic,Instance')
    if ($null -eq $channelOneField -or $null -eq $channelTwoField) { throw 'Main temperature controls t1/t2 were not found.' }
    $channelOne = $channelOneField.GetValue($testForm)
    $channelTwo = $channelTwoField.GetValue($testForm)
    $readings = [Array]::CreateInstance($readingType, 2)
    $readings.SetValue($positiveReading, 0)
    $readings.SetValue($decode.Invoke($null, [object[]]@([UInt16]0xFFF6)), 1)
    $readingArguments = [Array]::CreateInstance([object], 1)
    $readingArguments.SetValue($readings, 0)
    $updateReadings.Invoke($testForm, $readingArguments)
    if ($channelOne.Text -ne '25.0' -or $channelTwo.Text -ne '-1.0') {
        throw "Two-channel polling did not write both values to the main controls: t1=$($channelOne.Text), t2=$($channelTwo.Text)."
    }

    $readings.SetValue($faultReading, 0)
    $updateReadings.Invoke($testForm, $readingArguments)
    if ($channelOne.Text -ne '探头故障' -or $channelTwo.Text -ne '-1.0') {
        throw 'A sensor fault must update only its corresponding main-page temperature control.'
    }
}
finally {
    $testForm.Dispose()
}

$validateUnit.Invoke($null, [object[]]@([UInt16]0))
foreach ($unitValue in @([UInt16]1, [UInt16]2)) {
    try {
        $validateUnit.Invoke($null, [object[]]@($unitValue)) | Out-Null
        throw "temperature unit $unitValue must be rejected."
    }
    catch {
        if ($_.Exception.GetBaseException().GetType().Name -ne 'NtcTemperatureUnitException') { throw }
    }
}

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$clientSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'NtcTemperatureModuleClient.vb'), [System.Text.Encoding]::UTF8)
$formSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'frmTest.vb'), [System.Text.Encoding]::UTF8)
$designerSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'frmTest.Designer.vb'), [System.Text.Encoding]::UTF8)
$initSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'frmInit.vb'), [System.Text.Encoding]::UTF8)
$loadBlock = [Regex]::Match($formSource, '(?s)Private Sub frmTest_Load\(.*?End Sub').Value
$deferredStartupBlock = [Regex]::Match($formSource, '(?s)Private Sub StartDeferredStartupWork\(\).*?End Sub').Value
$temperatureStartBlock = [Regex]::Match($formSource, '(?s)Private Sub StartTemperaturePolling\(\).*?End Sub').Value
$updateBlock = [Regex]::Match($formSource, '(?s)Private Sub UpdateTemperatureReadings\(.*?End Sub').Value
$communicationErrorBlock = [Regex]::Match($formSource, '(?s)Private Sub UpdateTemperatureCommunicationError\(\).*?End Sub').Value
$unitErrorBlock = [Regex]::Match($formSource, '(?s)Private Sub UpdateTemperatureUnitError\(\).*?End Sub').Value
if ([string]::IsNullOrWhiteSpace($updateBlock) -or
    [string]::IsNullOrWhiteSpace($communicationErrorBlock) -or
    [string]::IsNullOrWhiteSpace($unitErrorBlock) -or
    [string]::IsNullOrWhiteSpace($loadBlock) -or
    [string]::IsNullOrWhiteSpace($deferredStartupBlock)) {
    throw 'Temperature UI update/error methods were not found.'
}

if ($clientSource -notmatch 'ReadInputRegisters\(SlaveAddress\s*,\s*FirstTemperatureRegister\s*,\s*TemperatureRegisterCount\)') {
    throw 'Temperature client must read the first input registers with Modbus function 04.'
}
if ($clientSource -notmatch 'FirstTemperatureRegister\s+As UShort\s*=\s*0' -or
    $clientSource -notmatch 'TemperatureRegisterCount\s+As UShort\s*=\s*2') {
    throw 'Temperature client must read exactly registers 0 and 1.'
}
if ($clientSource -notmatch 'DefaultTemperatureBaudRate\s*,\s*Parity\.None\s*,\s*8\s*,\s*StopBits\.One') {
    throw 'Temperature client must retain station installation serial defaults 9600/8N1.'
}
if ($clientSource -notmatch 'ReadHoldingRegisters\(SlaveAddress\s*,\s*&H85US\s*,\s*1US\)' -or
    $clientSource -notmatch 'ValidateTemperatureUnit\(unitRegisters\(0\)\)') {
    throw 'Temperature client must verify Celsius using holding register 0x0085 (function 03).'
}
if ($updateBlock -notmatch 'readings\.Length\s*<>\s*2') {
    throw 'Main temperature update must require exactly two readings.'
}
if ($updateBlock -match 'readings\.Length\s*<>\s*4' -or $updateBlock -match 'Updatet[34]Text') {
    throw 'Main temperature update must not require four readings.'
}
if ($communicationErrorBlock -notmatch 'Updatet1Text.*通信故障' -or
    $communicationErrorBlock -notmatch 'Updatet2Text.*通信故障' -or
    $communicationErrorBlock -match 'Updatet[34]Text') {
    throw 'Communication failures must affect only the two active temperature channels.'
}
if ($unitErrorBlock -notmatch '请设摄氏度') {
    throw 'A Fahrenheit module must be identified to the operator as a unit-setting problem.'
}
if ($formSource -notmatch 'Catch ex As NtcTemperatureUnitException') {
    throw 'The polling loop must preserve the link and show a unit error separately from communication failure.'
}
if ($formSource -notmatch 'temperatureTimer\.Interval\s*=\s*5000') {
    throw 'Temperature polling interval must remain 5 seconds.'
}
if ($loadBlock -notmatch 'StartTemperaturePolling\(\)' -or
    $temperatureStartBlock -notmatch 'temperatureTimer\.Start\(\)' -or
    $deferredStartupBlock -match 'temperatureTimer\.Start\(\)') {
    throw 'Temperature polling must start from frmTest_Load, independently of the first Paint/deferred-start path.'
}
if ($formSource -notmatch 't1\.Text\s*,\s*t2\.Text\s*,\s*String\.Empty\s*,\s*String\.Empty') {
    throw 'Local result CSV must serialize active temperatures and two blank inactive columns.'
}
if ($formSource -notmatch 'TA1,TA2,TA3,TA4' -or
    $designerSource -notmatch 'Me\.t3\.Visible\s*=\s*False' -or
    $designerSource -notmatch 'Me\.t4\.Visible\s*=\s*False') {
    throw 'The four-temperature CSV shape must remain and inactive temperature display slots must stay hidden.'
}
if ($initSource -notmatch 'TemperatureConnectTestClick' -or $initSource -notmatch 'New NtcTemperatureModuleClient\(cmb_TemPort\.SelectedIndex\)') {
    throw 'Temperature connection test must use the configured temperature COM port and shared client.'
}
if ($initSource -notmatch 'NtcTemperatureUnitException' -or
    $initSource -notmatch 'Modbus通信正常，但') {
    throw 'Connection test must distinguish a responding module/unit setting and report sensor faults.'
}

Write-Output 'LES30 NTC temperature protocol checks: PASS'
