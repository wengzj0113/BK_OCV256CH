param(
    [string]$SourcePath = (Join-Path $PSScriptRoot '..\frmTest.vb'),
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath $SourcePath -Raw

$constantMatch = [regex]::Match(
    $source,
    'Private\s+Const\s+SmokeAlarmIsActiveLow\s+As\s+Boolean\s*=\s*(True|False)',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)
if (-not $constantMatch.Success) {
    throw 'SmokeAlarmIsActiveLow constant was not found in frmTest.vb.'
}

$functionMatch = [regex]::Match(
    $source,
    'Private\s+Shared\s+Function\s+IsSmokeAlarmActive\(ByVal\s+inputHigh\s+As\s+Boolean\)\s+As\s+Boolean(?<body>[\s\S]*?)End Function',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
)
if (-not $functionMatch.Success -or
    $functionMatch.Groups['body'].Value -notmatch 'Return\s+If\s*\(\s*SmokeAlarmIsActiveLow\s*,\s*Not\s+inputHigh\s*,\s*inputHigh\s*\)') {
    throw 'IsSmokeAlarmActive no longer uses the expected polarity mapping; update this regression check.'
}

$isActiveLow = [bool]::Parse($constantMatch.Groups[1].Value)
$normalLevelAlarm = if ($isActiveLow) { $true } else { $false }
$alarmLevelAlarm = if ($isActiveLow) { $false } else { $true }

if ($normalLevelAlarm) {
    throw 'X1F=0 is the normal state but IsSmokeAlarmActive(0) reports an alarm.'
}
if (-not $alarmLevelAlarm) {
    throw 'X1F=1 is the alarm state but IsSmokeAlarmActive(1) does not report an alarm.'
}

if (-not [string]::IsNullOrWhiteSpace($AssemblyPath)) {
    $assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
    $formType = $assembly.GetType('software.frmTest', $true)
    $polarityMethod = $formType.GetMethod(
        'IsSmokeAlarmActive',
        [Reflection.BindingFlags]'Static,NonPublic'
    )
    if ($null -eq $polarityMethod) {
        throw 'Compiled frmTest.IsSmokeAlarmActive method was not found.'
    }

    $compiledNormalLevelAlarm = $polarityMethod.Invoke($null, [object[]]@($false))
    $compiledAlarmLevelAlarm = $polarityMethod.Invoke($null, [object[]]@($true))
    if ($compiledNormalLevelAlarm -ne $false -or $compiledAlarmLevelAlarm -ne $true) {
        throw 'Compiled application does not map X1F=0 to normal and X1F=1 to alarm.'
    }
}

Write-Output 'PASS: X1F=0 is normal and X1F=1 is alarm.'
