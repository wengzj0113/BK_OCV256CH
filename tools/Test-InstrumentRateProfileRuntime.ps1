param(
    [string]$AssemblyPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'obj\instrument-rate-profile-anycpu\XL_OCV_256CH.exe')
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $AssemblyPath)) {
    throw "Build the application first; assembly not found: $AssemblyPath"
}

$assembly = [System.Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$profileType = $assembly.GetType('software.InstrumentRateProfile', $true)
$factory = $profileType.GetMethod('FromSettingName')
$rangeSelector = $profileType.GetMethod('SelectKeysightVoltageRange')

$expectedProfiles = @(
    @{ Name = '低速'; Speed = 0; Sample = 'SLOW'; Nplc = '1'; AutoZero = $true },
    @{ Name = '中速'; Speed = 1; Sample = 'MED'; Nplc = '1'; AutoZero = $true },
    @{ Name = '高速'; Speed = 2; Sample = 'FAST'; Nplc = '0.2'; AutoZero = $false },
    @{ Name = '超快'; Speed = 3; Sample = 'EXF'; Nplc = '0.02'; AutoZero = $false }
)

foreach ($expected in $expectedProfiles) {
    $actual = $factory.Invoke($null, @($expected.Name))
    if ($actual.ControllerSpeedValue -ne $expected.Speed -or
        $actual.HiokiSamplingCommand -ne $expected.Sample -or
        $actual.KeysightNplc -ne $expected.Nplc -or
        $actual.KeysightAutoZero -ne $expected.AutoZero) {
        throw "Unexpected rate profile mapping for $($expected.Name)."
    }
}

$emptyNameProfile = $factory.Invoke($null, @(''))
if ($emptyNameProfile.ControllerSpeedValue -ne 3 -or $emptyNameProfile.HiokiSamplingCommand -ne 'EXF') {
    throw 'An empty saved mode must use the high-speed default profile.'
}
$legacyHighSpeedProfile = $factory.Invoke($null, @('高速档'))
if ($legacyHighSpeedProfile.ControllerSpeedValue -ne 3 -or $legacyHighSpeedProfile.HiokiSamplingCommand -ne 'EXF') {
    throw 'The controller high-speed profile alias must map to the EX.FAST instrument profile.'
}

$rangeCases = @(
    @{ Up = [single]4200; Down = [single]2500; Expected = 10 },
    @{ Up = [single]9000; Down = [single]0; Expected = 100 },
    @{ Up = [single]100000; Down = [single]0; Expected = 1000 }
)
foreach ($testCase in $rangeCases) {
    $range = $rangeSelector.Invoke($null, @($testCase.Up, $testCase.Down))
    if ($range -ne $testCase.Expected) {
        throw "Expected a $($testCase.Expected) V range for configured voltage limits."
    }
}

$unknownRejected = $false
try {
    $factory.Invoke($null, @('未知档位')) | Out-Null
} catch [System.Reflection.TargetInvocationException] {
    $unknownRejected = $_.Exception.InnerException -is [System.ArgumentException]
} catch {
    $exception = $_.Exception
    while ($exception.InnerException) { $exception = $exception.InnerException }
    $unknownRejected = $exception -is [System.ArgumentException]
}
if (-not $unknownRejected) {
    throw 'Unknown non-empty rate mode names must be rejected.'
}

$overrangeRejected = $false
try {
    $rangeSelector.Invoke($null, @([single]900000, [single]0)) | Out-Null
} catch [System.Reflection.TargetInvocationException] {
    $overrangeRejected = $_.Exception.InnerException -is [System.ArgumentOutOfRangeException]
} catch {
    $exception = $_.Exception
    while ($exception.InnerException) { $exception = $exception.InnerException }
    $overrangeRejected = $exception -is [System.ArgumentOutOfRangeException]
}
if (-not $overrangeRejected) {
    throw 'Voltage limits that exceed the 34461A range must be rejected.'
}

Write-Output 'Instrument rate profile runtime verification passed.'
