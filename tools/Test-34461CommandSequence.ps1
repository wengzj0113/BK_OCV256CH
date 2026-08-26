$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot '..\Module1.vb'
$source = Get-Content -Raw -LiteralPath $modulePath
$prepare = [regex]::Match($source, '(?s)Public Function PrepareCachedData_34461.*?End Function').Value
$read = [regex]::Match($source, '(?s)Public Function Trans_ReadCachedData_34461.*?End Function').Value

if ([string]::IsNullOrWhiteSpace($prepare)) { throw 'PrepareCachedData_34461 was not found.' }
if ([string]::IsNullOrWhiteSpace($read)) { throw 'Trans_ReadCachedData_34461 was not found.' }

$requiredPrepareCommands = @(
    'SAMP:COUN 1',
    'TRIG:COUN ',
    'TRIG:SOUR EXT',
    'TRIG:SLOP POS',
    'INIT'
)

foreach ($command in $requiredPrepareCommands) {
    if ($prepare -notmatch [regex]::Escape($command)) {
        throw "Missing 34461 prepare command: $command"
    }
}

if ($prepare -match 'OUTP:TRIG:SLOP') {
    throw 'Prepare sequence must not configure VM complete output slope.'
}

if ($read -match 'DATA:POINTS\?') {
    throw 'Read sequence must not poll DATA:POINTS? before the final fetch.'
}

if ($read -notmatch 'WriteString\("FETC\?"\)') {
    throw 'Read sequence must issue one FETC? query.'
}

if ($read -notmatch 'SYST:ERR\?') {
    throw 'Read failure path must query the 34461 system error queue.'
}

Write-Output '34461 command sequence verification passed.'
