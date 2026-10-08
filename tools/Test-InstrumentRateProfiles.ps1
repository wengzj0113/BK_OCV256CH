$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$profilePath = Join-Path $root 'InstrumentRateProfile.vb'
$projectPath = Join-Path $root 'MP-60A-RV-64Mem.vbproj'
$profileTest = if (Test-Path -LiteralPath $profilePath) { Get-Content -Raw -LiteralPath $profilePath } else { '' }
$project = Get-Content -Raw -LiteralPath $projectPath

if ([string]::IsNullOrWhiteSpace($profileTest)) {
    throw 'InstrumentRateProfile.vb is missing.'
}

$mappingExpectations = @(
    @{ Name = '低速'; Pattern = 'Case "低速", "观察档"[\s\S]*?New InstrumentRateProfile\("低速", CUShort\(0\), "SLOW", "1", True\)' },
    @{ Name = '中速'; Pattern = 'Case "中速", "稳定档"[\s\S]*?New InstrumentRateProfile\("中速", CUShort\(1\), "MED", "1", True\)' },
    @{ Name = '高速'; Pattern = 'Case "高速", "快速档"[\s\S]*?New InstrumentRateProfile\("高速", CUShort\(2\), "FAST", "0\.2", False\)' },
    @{ Name = '超快'; Pattern = 'Case "", "超快", "高速档"[\s\S]*?New InstrumentRateProfile\("超快", CUShort\(3\), "EXF", "0\.02", False\)' }
)
foreach ($expectation in $mappingExpectations) {
    if ($profileTest -notmatch $expectation.Pattern) {
        throw "Rate profile mapping is incorrect for $($expectation.Name)."
    }
}

if ($profileTest -notmatch 'Case Else[\s\S]*Throw New ArgumentException') {
    throw 'Unknown non-empty rate profile names must fail explicitly.'
}
if ($project -notmatch '<Compile Include="InstrumentRateProfile.vb"') {
    throw 'InstrumentRateProfile.vb is not included in the application project.'
}

$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$init = [regex]::Match($module, '(?s)Public Sub InitRes.*?End Sub').Value
$prepare3562 = [regex]::Match($module, '(?s)Public Function PrepareCachedData_3562.*?End Function').Value
$prepare34461 = [regex]::Match($module, '(?s)Public Function PrepareCachedData_34461.*?End Function').Value
foreach ($entry in @(@('InitRes', $init), @('PrepareCachedData_3562', $prepare3562))) {
    if ($entry[1] -notmatch 'InstrumentRateProfile\.FromSettingName' -or $entry[1] -notmatch 'SAMPle:RATE') {
        throw "$($entry[0]) must apply the selected 3562 sampling profile."
    }
}

foreach ($command in @('VOLT:DC:NPLC ', 'VOLT:DC:ZERO:AUTO ', 'VOLT:DC:RANG:AUTO OFF', 'VOLT:DC:RANG ', 'TRIG:SLOP NEG', 'TRIG:DEL 0')) {
    if ($prepare34461 -notmatch [regex]::Escape($command)) {
        throw "34461A preparation is missing profile-aware command: $command"
    }
}
if ($prepare34461 -notmatch 'InstrumentRateProfile\.FromSettingName') {
    throw '34461A preparation must use the selected rate profile.'
}

$runner = Get-Content -Raw -LiteralPath (Join-Path $root 'FormalTestRunner.vb')
if ($runner -notmatch 'InstrumentRateProfile\.FromSettingName' -or
    $runner -notmatch 'RunPlan\([\s\S]*?rateProfile\.ControllerSpeedValue') {
    throw 'The selected controller speed value must be passed to RunPlan.'
}

$form = Get-Content -Raw -LiteralPath (Join-Path $root 'frmInit.vb')
if ($form -notmatch 'Initsetting\.ComRateMode[\s\S]*?ComRate\.SelectedItem') {
    throw 'The initialization form must restore the saved rate mode and choose a default when absent.'
}

Write-Output 'Instrument rate profile verification passed.'
