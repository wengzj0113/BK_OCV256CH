$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$protocol = $assembly.GetType('software.PlcAutomationProtocol', $true)
$build = $protocol.GetMethod('BuildNgSelectionFrame')
if ($build.GetParameters()[0].Name -ne 'ngChannelNumber') {
    throw 'PLC DT frame input must be explicitly named as an NG selection sequence number.'
}
$plcClient = $assembly.GetType('software.IPlcAutomationClient', $true)
if ($plcClient.GetMethod('WriteNgSelection').GetParameters()[0].Name -ne 'ngChannelNumber') {
    throw 'PLC grade write input must be explicitly named as an NG selection sequence number.'
}
$mapping = $assembly.GetType('software.TrayChannelMapping', $true)
$toDevice = $mapping.GetMethod('ToDeviceChannel')
$toStandard = $mapping.GetMethod('ToStandardChannel')
$seen = @{}
for ($channel = 1; $channel -le 256; $channel++) {
    $row = [int][Math]::Floor(($channel - 1) / 16)
    $column = ($channel - 1) % 16
    $expectedAddress = 1000 + $channel
    $expectedDevice = 16 * (15 - $column) + $row + 1
    $device = [int]$toDevice.Invoke($null, @($channel))
    $standard = [int]$toStandard.Invoke($null, @($expectedDevice))
    if ($device -ne $expectedDevice -or $standard -ne $channel) {
        throw "Standard $channel at row $row, column $column maps to device $device; expected device $expectedDevice and reverse standard $channel."
    }
    $frame = [string]$build.Invoke($null, @($channel))
    if ($frame -notmatch '^%01#WDD(\d{5})(\d{5})0200\*\*\r$') { throw "Malformed DT frame for CH$channel`: $frame" }
    $startAddress = [int]$Matches[1]
    $endAddress = [int]$Matches[2]
    if ($startAddress -ne $expectedAddress -or $endAddress -ne $expectedAddress) {
        throw "CH$channel mapped to DT$startAddress-$endAddress; expected DT$expectedAddress"
    }
    if ($seen.ContainsKey($startAddress)) { throw "DT$startAddress is mapped twice." }
    $seen[$startAddress] = $channel
}
if ($seen.Count -ne 256 -or $seen[1001] -ne 1 -or $seen[1002] -ne 2 -or $seen[1256] -ne 256) {
    throw 'Sequential NG-address mapping or 256-address coverage is incorrect.'
}
foreach ($invalid in @(0, 257)) {
    try {
        [void]$build.Invoke($null, @([int]$invalid))
        throw "Invalid standard tray number $invalid was accepted."
    } catch {
        $failure = $_.Exception
        while ($null -ne $failure.InnerException) { $failure = $failure.InnerException }
        if ($failure -isnot [ArgumentOutOfRangeException] -or
            $failure.ParamName -ne 'ngChannelNumber') {
            throw
        }
    }
}
Write-Output 'PASS: tray/device mapping remains intact and NG logical numbers map sequentially to all 256 PLC DT positions'
