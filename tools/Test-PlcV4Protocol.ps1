Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
}

function Read-Source([string]$name) {
    $path = Join-Path $root $name
    Assert-True (Test-Path -LiteralPath $path) "missing source file $name"
    return Get-Content -Raw -LiteralPath $path
}

$protocol = Read-Source 'PlcAutomationProtocol.vb'
$client = Read-Source 'PlcAutomationClient.vb'
$scanner = Read-Source 'TrayScannerClient.vb'
$initForm = Read-Source 'frmInit.vb'
$module = Read-Source 'Module1.vb'

Assert-True ($protocol.Contains('%01#CSR0150**')) 'R150 read frame is missing'
Assert-True ($protocol.Contains('%01#RCSR0151**')) 'R151 read frame is missing'
Assert-True ($protocol.Contains('%01#RCSR0170**')) 'R170 read frame is missing'
Assert-True ($protocol.Contains('%01#WCSR01601**')) 'R160 write frame is missing'
Assert-True ($protocol.Contains('%01#WCSR01611**')) 'R161 write frame is missing'
Assert-True ($protocol.Contains('1001 + column * 16 + row')) 'DT address must transpose tray row-major channel numbers to PLC column-major addresses'
Assert-True ($protocol.Contains('channelNumber < 1') -and $protocol.Contains('channelNumber > 256')) 'DT channel bounds are missing'
Assert-True ($client.Contains('Parity.Odd')) 'PLC odd parity is missing'
Assert-True ($client.Contains('BaudRate')) 'PLC baud-rate configuration is missing'
Assert-True ($client.Contains('DefaultPlcBaudRate')) 'PLC default baud rate is missing'
Assert-True ($client.Contains('SyncLock')) 'PLC request serialization is missing'
Assert-True ($scanner.Contains('Parity.None')) 'scanner no-parity configuration is missing'
Assert-True ($scanner.Contains('DefaultScannerBaudRate')) 'scanner default baud rate is missing'
Assert-True ($scanner.Contains('ReadTimeout')) 'scanner read timeout is missing'
Assert-True ($scanner.Contains('不能在本次读取开始时丢弃输入')) 'scanner must preserve data already received before R150'
Assert-True ($initForm.Contains('.Items.Add("38400")')) 'frmInit PLC rate list is missing 38400'
Assert-True ($initForm.Contains('If CInt(cmb_PlcRate.Items.Item(intLoop)) = If(Initsetting.PLCComRate > 0, Initsetting.PLCComRate, 38400)')) 'frmInit PLC default rate is not 38400'
Assert-True ($initForm.Contains('cmb_ScanerRate.Items.Item(intLoop) = Initsetting.ScanerRate')) 'frmInit scanner rate restore uses the wrong setting'
Assert-True ($module.Contains('DefaultPlcBaudRate')) 'shared PLC default constant is missing'
Assert-True ($module.Contains('DefaultScannerBaudRate')) 'shared scanner default constant is missing'

Write-Output 'PASS: PLC/scanner protocol structure'
