$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$type = $assembly.GetType('software.DebugBarcodePolicy', $false)
if ($null -eq $type) { throw 'DebugBarcodePolicy is missing from the application assembly.' }
$createCodes = $type.GetMethod('CreateCodes', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $createCodes) { throw 'DebugBarcodePolicy.CreateCodes is missing.' }
$codes = $createCodes.Invoke($null, @([int[]]@(1, 16, 256, 16), [DateTime]::new(2026, 10, 7)))
if ($codes.Count -ne 3 -or
    $codes[1] -ne 'TEST-20261007-1' -or
    $codes[16] -ne 'TEST-20261007-16' -or
    $codes[256] -ne 'TEST-20261007-256') {
    throw 'Debug codes do not match the required date and channel format.'
}

foreach ($invalid in @(0, 257)) {
    $caught = $false
    try {
        [void]$createCodes.Invoke($null, @([int[]]@($invalid), [DateTime]::new(2026, 10, 7)))
    } catch {
        $caught = $true
        $failure = $_.Exception
        while ($null -ne $failure.InnerException) { $failure = $failure.InnerException }
        if ($failure -isnot [ArgumentOutOfRangeException]) {
            throw "Out-of-range channel $invalid failed for the wrong reason: $($failure.GetType().FullName)"
        }
    }
    if (-not $caught) { throw "Out-of-range channel $invalid was accepted." }
}

Write-Output 'Debug barcode policy runtime: PASS'
