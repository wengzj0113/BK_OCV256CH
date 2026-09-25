$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$protocol = $assembly.GetType('software.PlcAutomationProtocol', $true)
$build = $protocol.GetMethod('BuildDtGradeFrame')
$seen = @{}
for ($channel = 1; $channel -le 256; $channel++) {
    $row = [int][Math]::Floor(($channel - 1) / 16)
    $column = ($channel - 1) % 16
    $expectedAddress = 1001 + 16 * $column + $row
    $frame = [string]$build.Invoke($null, @($channel, 2))
    if ($frame -notmatch '^%01#WD(\d{4})0(\d{4})0200\*\*\r$') { throw "Malformed DT frame for CH$channel`: $frame" }
    $startAddress = [int]$Matches[1]
    $endAddress = [int]$Matches[2]
    if ($startAddress -ne $expectedAddress -or $endAddress -ne $expectedAddress) {
        throw "CH$channel mapped to DT$startAddress-$endAddress; expected DT$expectedAddress"
    }
    if ($seen.ContainsKey($startAddress)) { throw "DT$startAddress is mapped twice." }
    $seen[$startAddress] = $channel
}
if ($seen.Count -ne 256 -or $seen[1001] -ne 1 -or $seen[1241] -ne 16 -or
    $seen[1002] -ne 17 -or $seen[1016] -ne 241 -or $seen[1256] -ne 256) {
    throw 'Tray corner mapping or 256-address coverage is incorrect.'
}
Write-Output 'PASS: all 256 physical channels map one-to-one to PLC tray DT positions'
