$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms

$assemblyPath = if ($env:FRMTEST_EXE) {
    $env:FRMTEST_EXE
} else {
    Join-Path $PSScriptRoot '..\bin\AnyCPU\XL_OCV_256CH.exe'
}

if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Assembly not found: $assemblyPath"
}

$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $assemblyPath))
$parserType = $assembly.GetType('software.InstrumentCacheParser', $false)
if ($null -eq $parserType) {
    throw 'InstrumentCacheParser is not available in the application assembly.'
}

$parseMethod = $parserType.GetMethod('Parse3562MemoryData', [Reflection.BindingFlags] 'Public,Static')
if ($null -eq $parseMethod) {
    throw 'Parse3562MemoryData public static method is not available.'
}

function Invoke-Parse([string]$text, [int]$expectedCount, [bool]$resistanceOnly = $false) {
    return $parseMethod.Invoke($null, @($text, $expectedCount, $resistanceOnly))
}

# Regression for the captured main-page failure: the 3562 pads the number after
# the sign, e.g. "-   0.04E+0".  It must remain a valid record.
$paddedSignText = "15,-   0.04E+0, 3.17881E+0`r`n16, 0.005E+0,-   1.2E+0`r`nEND`r`n"
$paddedResult = Invoke-Parse $paddedSignText 2
if (-not $paddedResult.Success) { throw 'Padded signed scientific notation was not parsed.' }
if ($paddedResult.ValidRecordCount -ne 2) { throw "Padded sample parsed $($paddedResult.ValidRecordCount) records, expected 2." }
if ([Math]::Abs([double]$paddedResult.Resistances[0] - (-40.0)) -gt 0.0001) { throw 'Padded resistance value was not converted from ohms to milliohms.' }
if ([Math]::Abs([double]$paddedResult.Voltages[1] - (-1.2)) -gt 0.0001) { throw 'Padded voltage value was not parsed correctly.' }

# Keep arbitrary/discrete channel runs strict: a complete 256-record cache is
# accepted even when signed scientific values are mixed into the response.
$records = New-Object System.Collections.Generic.List[string]
for ($i = 1; $i -le 256; $i++) {
    $resistance = if (($i % 2) -eq 0) { '-   0.04E+0' } else { '0.005E+0' }
    $voltage = if (($i % 3) -eq 0) { '-   1.2E+0' } else { '3.17881E+0' }
    [void]$records.Add("$i,$resistance,$voltage")
}
$fullResult = Invoke-Parse (($records -join "`r`n") + "`r`nEND`r`n") 256
if (-not $fullResult.Success) { throw "256-record mixed-format cache was rejected: valid=$($fullResult.ValidRecordCount), invalid=$($fullResult.InvalidRecordCount)." }
if ($fullResult.ValidRecordCount -ne 256) { throw "256-record cache parsed $($fullResult.ValidRecordCount) records." }

# Missing resistance data is represented by 10000 mOhm for every expected slot.
$emptyResult = Invoke-Parse "END`r`n" 1
if (-not $emptyResult.Success -or $emptyResult.Resistances[0] -ne 10000) { throw 'Empty resistance cache must use the 10000 mOhm fallback.' }
if ($emptyResult.ValidRecordCount -ne 0) { throw 'Empty cache reported valid records.' }
if ($emptyResult.FallbackRecordCount -ne 1) { throw 'Empty cache must report its substituted resistance count.' }

# END terminates the response; harmless trailing text must not become a fake
# invalid measurement record.
$terminatedResult = Invoke-Parse "1,0.01E+0,3.1E+0`r`nEND`r`ntrailing text`r`n" 1
if (-not $terminatedResult.Success) { throw 'Text after END incorrectly invalidated a complete cache.' }

$malformedResult = Invoke-Parse "1,not-a-number,3.1E+0`r`nEND`r`n" 1
if (-not $malformedResult.Success -or $malformedResult.InvalidRecordCount -ne 1 -or $malformedResult.Resistances[0] -ne 10000) { throw 'Malformed resistance must be substituted and counted.' }

foreach ($reading in @('1E+10', '-1E+10', 'NaN', 'Infinity', '-Infinity')) {
    $result = Invoke-Parse "1,$reading,3.5`r`nEND`r`n" 1
    if (-not $result.Success -or $result.Resistances[0] -ne 10000 -or $result.FallbackRecordCount -ne 1) { throw "Invalid resistance $reading must become 10000 mOhm." }
}
$mixed = Invoke-Parse "1,0.005,3.5`r`n2,bad,3.5`r`n3,0.007,3.5`r`nEND`r`n" 3
if (($mixed.Resistances -join ',') -ne '5,10000,7') { throw 'A malformed record must retain its slot without shifting subsequent channels.' }
foreach ($text in @('', "1,0.005,3.5`r`n3,0.007,3.5`r`nEND", "1,0.005,3.5`r`nEND", "bad-index,0.005,3.5`r`n2,0.007,3.5`r`nEND")) {
    $result = Invoke-Parse $text 2
    if (-not $result.Success -or ($result.Resistances -join ',') -ne '10000,10000') { throw 'Missing or ambiguous record positions must substitute the entire resistance batch.' }
}
$extra = Invoke-Parse "1,0.005,3.5`r`n2,0.007,3.5`r`n3,0.009,3.5`r`nEND" 2
if (($extra.Resistances -join ',') -ne '10000,10000') { throw 'Extra records must not be silently mapped to the wrong channels.' }
$resOnly = Invoke-Parse "1,0.005`r`n2,1E+10`r`nEND" 2 $true
if (-not $resOnly.Success -or ($resOnly.Resistances -join ',') -ne '5,10000') { throw 'RES-only acquisition must use the same fallback policy.' }
$invalidPlan = Invoke-Parse '' 0
if ($invalidPlan.Success -or $invalidPlan.Resistances.Count -ne 0) { throw 'An invalid acquisition plan must remain a failure.' }

Write-Host 'PASS: 3562 resistance fallback is 10000 mOhm; normal values and channel alignment are preserved.'
