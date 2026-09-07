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

function Invoke-Parse([string]$text, [int]$expectedCount) {
    return $parseMethod.Invoke($null, @($text, $expectedCount))
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

# An END marker with no records must remain a clear failure, not a false success.
$emptyResult = Invoke-Parse "END`r`n" 1
if ($emptyResult.Success) { throw 'Empty cache unexpectedly succeeded.' }
if ($emptyResult.ValidRecordCount -ne 0) { throw 'Empty cache reported valid records.' }

# END terminates the response; harmless trailing text must not become a fake
# invalid measurement record.
$terminatedResult = Invoke-Parse "1,0.01E+0,3.1E+0`r`nEND`r`ntrailing text`r`n" 1
if (-not $terminatedResult.Success) { throw 'Text after END incorrectly invalidated a complete cache.' }

$malformedResult = Invoke-Parse "1,not-a-number,3.1E+0`r`nEND`r`n" 1
if ($malformedResult.Success -or $malformedResult.InvalidRecordCount -ne 1) { throw 'Malformed cache record was not rejected and counted.' }

Write-Host 'PASS: 3562 cache parser accepts padded signed scientific values and enforces exact record counts.'
