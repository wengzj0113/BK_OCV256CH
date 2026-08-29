$ErrorActionPreference = 'Stop'

$modulePath = Join-Path $PSScriptRoot '..\Module1.vb'
$source = Get-Content -Raw -LiteralPath $modulePath
$prepare = [regex]::Match($source, '(?s)Public Function PrepareCachedData_34461.*?End Function').Value
$read = [regex]::Match($source, '(?s)Public Function Trans_ReadCachedData_34461.*?End Function').Value
$check = [regex]::Match($source, '(?s)Public Function CheckCom_34461.*?End Function').Value
$connect = [regex]::Match($source, '(?s)Public Function ConnectFor34461.*?End Function').Value

if ([string]::IsNullOrWhiteSpace($prepare)) { throw 'PrepareCachedData_34461 was not found.' }
if ([string]::IsNullOrWhiteSpace($read)) { throw 'Trans_ReadCachedData_34461 was not found.' }
if ([string]::IsNullOrWhiteSpace($check)) { throw 'CheckCom_34461 was not found.' }
if ([string]::IsNullOrWhiteSpace($connect)) { throw 'ConnectFor34461 was not found.' }

$requiredPrepareCommands = @(
    'SAMP:COUN 1',
    'TRIG:COUN ',
    'TRIG:SOUR EXT',
    'VOLT:DC:NPLC 1',
    'INIT'
)

foreach ($command in $requiredPrepareCommands) {
    if ($prepare -notmatch [regex]::Escape($command)) {
        throw "Missing 34461 prepare command: $command"
    }
}

if ($prepare -match 'TRIG:SLOP POS' -or $prepare -match 'OUTP:TRIG:SLOP') {
    throw 'Prepare sequence must not force a positive trigger slope; the hardware trigger is low-active by default.'
}

if ($prepare -match 'FORM:DATA') {
    throw '34461A diagnostic rejected FORM:DATA; leave the instrument output format at its supported default.'
}

if ($prepare -notmatch 'Close34461Session' -or $prepare -notmatch 'ConnectFor34461') {
    throw 'Prepare sequence must recover a stale VISA session before retrying.'
}

if ($prepare -notmatch 'SYST:ERR\?') {
    throw 'Prepare sequence must verify the instrument error queue after initialization.'
}

if ($read -notmatch 'WriteString\("FETC\?"\)') {
    throw 'Read sequence must issue one FETC? query.'
}

if ($read -notmatch 'DATA:POINTS\?') {
    throw 'Read sequence must record the actual 34461 reading-memory point count before FETC?.'
}

if ($read -notmatch 'SYST:ERR\?') {
    throw 'Read failure path must query the 34461 system error queue.'
}

if ($check -notmatch 'Finally' -or $check -notmatch 'IO\.Close\(\)') {
    throw 'CheckCom_34461 must close its temporary VISA session.'
}

if ($connect -notmatch 'visaSession\.IO\.Close\(\)') {
    throw 'ConnectFor34461 must close an existing VISA session before reconnecting.'
}

$runner = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\FormalTestRunner.vb')
if ($runner -notmatch 'ResetCachedData_3562\(\)' -or $runner -notmatch 'Reset34461ForNewTest\(\)') {
    throw 'RunTwoPassMeasurement must reset both instruments before preparing a new test.'
}

$module = Get-Content -Raw -LiteralPath $modulePath
$read3562 = [regex]::Match($module, '(?s)Public Function Trans_ReadCachedData_3562.*?End Function').Value
$parser3562 = [regex]::Match($module, '(?s)Private Function Parse3562MemoryData.*?End Function').Value
if ($read3562 -notmatch 'Parse3562MemoryData' -or
    $parser3562 -notmatch 'fields\.Length\s*<\s*3' -or
    $parser3562 -notmatch 'resistanceOhm' -or
    $parser3562 -notmatch 'voltageValue' -or
    $parser3562 -notmatch 'Replace\(vbCr, vbLf\)') {
    throw '3562 memory data must be parsed as record number, resistance, and voltage.'
}
if ($read3562 -match 'values\(i \* 2\)' -or $read3562 -match 'values\(i \* 2 \+ 1\)') {
    throw '3562 memory parser must not pair flattened numeric values.'
}
if ($read3562 -match 'HasEnough3562CacheData\(recvStr, expectedCount\)' -or
    $module -match 'Return ExtractNumericValues\(text\)\.Count >= expectedCount \* 2') {
    throw '3562 cache reader must not stop after only two numeric fields per expected record.'
}
if ($module -notmatch 'ReadBufferSize\s*=\s*8192' -or
    $module -notmatch '3562 serial configured:') {
    throw '3562 serial diagnostics/buffer capacity are missing.'
}

Write-Output '34461 command sequence verification passed.'
