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
$cycleRecovery = Read-Source 'tools/Test-PlcCycleRecoveryRuntime.ps1'
$scanner = Read-Source 'TrayScannerClient.vb'
$initForm = Read-Source 'frmInit.vb'
$initDesigner = Read-Source 'frmInit.Designer.vb'
$module = Read-Source 'Module1.vb'

Assert-True ($protocol.Contains('%01#RCSR0150**')) 'R150 read frame is missing'
Assert-True ($protocol.Contains('%01#RCSR0151**')) 'R151 read frame is missing'
Assert-True ($protocol.Contains('%01#RCSR0152**')) 'R152 read frame is missing'
Assert-True ($protocol.Contains('%01#WCSR01601**')) 'R160 write frame is missing'
Assert-True ($protocol.Contains('%01#WCSR01611**')) 'R161 write frame is missing'
Assert-True ($protocol.Contains('%01#WCSR01621**')) 'R162 write frame is missing'
Assert-True ($protocol.Contains('.ToString("D5", CultureInfo.InvariantCulture)')) 'DT addresses must be padded to the required five decimal digits'
Assert-True ($protocol.Contains('WithCr("%01#WDD" & dtAddress & dtAddress & "0200**")')) 'DT frame must contain five-digit start/end addresses and the NG grade word'
Assert-True ($cycleRecovery.Contains('frame.Substring(7, 5)')) 'cycle-recovery verification must parse the five-digit DT address after the WDD prefix'
Assert-True ($protocol.Contains('1000 + ngChannelNumber')) 'DT address must follow the sequential NG selection address order'
Assert-True ($protocol.Contains('ngChannelNumber < 1') -and $protocol.Contains('ngChannelNumber > 256')) 'NG channel bounds are missing'
Assert-True ($protocol.Contains('BuildNgSelectionFrame')) 'NG-only grade-2 frame builder is missing'
Assert-True (-not $protocol.Contains('BuildDtGradeFrame')) 'generic DT grade builder can send an unapproved OK/clear grade'
Assert-True ($client.Contains('Function ReadR152()')) 'R152 read operation is missing from the PLC client interface'
Assert-True ($client.Contains('Sub WriteR162()')) 'R162 write operation is missing from the PLC client interface'
Assert-True ([regex]::IsMatch($client, 'Sub\s+WriteNgSelection\(ByVal\s+ngChannelNumber\s+As\s+Integer,\s+Optional\s+ByVal\s+responseReceived\s+As\s+System\.Action\(Of\s+String\)')) 'NG-only grade-2 write operation with response logging is missing from the PLC client interface'
Assert-True ($client.Contains('Parity.Odd')) 'PLC odd parity is missing'
Assert-True ($client.Contains('BaudRate')) 'PLC baud-rate configuration is missing'
Assert-True ($client.Contains('DefaultPlcBaudRate')) 'PLC default baud rate is missing'
$r160 = [regex]::Match($client, '(?ms)^\s*Public Sub WriteR160\(\).*?^\s*End Sub').Value
$sendOnly = [regex]::Match($client, '(?ms)^\s*Private Sub SendWithoutResponse\(.*?^\s*End Sub').Value
Assert-True ($r160.Contains('SendWithoutResponse(') -and -not $r160.Contains('WriteAndRequireAck(')) 'R160 has no PLC write ACK and must only send its frame'
Assert-True ($sendOnly.Contains('SyncLock syncRoot') -and $sendOnly.Contains('serialPortPLC.Write(frame)')) 'send-only R160 must serialize access and write the frame'
Assert-True ($sendOnly -notmatch 'SendAndReceive|WriteAndRequireAck|ReadExisting|ReadLine|BytesToRead|Thread.Sleep|DiscardOutBuffer') 'send-only R160 must not wait for a response or discard queued output'
Assert-True ($client.Contains('SyncLock')) 'PLC request serialization is missing'
Assert-True ($scanner.Contains('Parity.None')) 'scanner no-parity configuration is missing'
Assert-True ($scanner.Contains('DefaultScannerBaudRate')) 'scanner default baud rate is missing'
Assert-True ($scanner.Contains('ReadTimeout')) 'scanner read timeout is missing'
Assert-True ($scanner.Contains('不能在本次读取开始时丢弃输入')) 'scanner must preserve data already received before R150'
Assert-True ($initDesigner.Contains('{"9600", "19200", "38400", "115200"}')) 'frmInit PLC rate list is missing 38400'
Assert-True ($initForm.Contains('If CInt(cmb_PlcRate.Items.Item(intLoop)) = If(Initsetting.PLCComRate > 0, Initsetting.PLCComRate, 38400)')) 'frmInit PLC default rate is not 38400'
Assert-True ($initForm.Contains('cmb_ScanerRate.Items.Item(intLoop) = Initsetting.ScanerRate')) 'frmInit scanner rate restore uses the wrong setting'
Assert-True ($module.Contains('DefaultPlcBaudRate')) 'shared PLC default constant is missing'
Assert-True ($module.Contains('DefaultScannerBaudRate')) 'shared scanner default constant is missing'

Write-Output 'PASS: PLC/scanner protocol structure'
