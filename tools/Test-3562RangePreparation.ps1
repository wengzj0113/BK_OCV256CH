$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$methodMatch = [regex]::Match(
    $module,
    'Public Function PrepareCachedData_3562\(.*?\n    End Function',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)

if (-not $methodMatch.Success) {
    throw 'PrepareCachedData_3562 method was not found.'
}

$method = $methodMatch.Value
$autoOffIndex = $method.IndexOf(':AUT OFF', [System.StringComparison]::OrdinalIgnoreCase)
$rangeIndex = $method.IndexOf(':RES:RANG 300E-3', [System.StringComparison]::OrdinalIgnoreCase)
$clearIndex = $method.IndexOf('Com_3561.Write("MEMory:CLEA"', [System.StringComparison]::OrdinalIgnoreCase)

if ($autoOffIndex -lt 0) { throw 'PrepareCachedData_3562 must disable auto-ranging before each test.' }
if ($rangeIndex -lt 0) { throw 'PrepareCachedData_3562 must select the 300 mOhm range before each test.' }
if ($clearIndex -lt 0) { throw 'PrepareCachedData_3562 must clear the instrument cache.' }
if ($autoOffIndex -ge $rangeIndex -or $rangeIndex -ge $clearIndex) {
    throw 'Range preparation order must be :AUT OFF, :RES:RANG 300E-3, then MEMory:CLEA.'
}
if ($method -notmatch 'InstrumentRateProfile\.FromSettingName' -or $method -notmatch 'SAMPle:RATE') {
    throw 'PrepareCachedData_3562 must apply the selected sample rate on every test.'
}

Write-Host 'PASS: 3562 test preparation applies rate, disables auto-ranging and selects 300 mOhm before clearing memory.'
