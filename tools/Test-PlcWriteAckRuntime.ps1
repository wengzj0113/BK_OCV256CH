$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$protocol = $assembly.GetType('software.PlcAutomationProtocol', $true)
$method = $protocol.GetMethod('IsWriteAcknowledged')
if ($method.Invoke($null, @('%01!WC01**', 'WC')) -eq $true) { throw 'PLC error response was accepted as write ACK.' }
if ($method.Invoke($null, @('garbage', 'WC')) -eq $true) { throw 'Malformed response was accepted as write ACK.' }
if ($method.Invoke($null, @('%01$WC**', 'WC')) -ne $true) { throw 'PLC success response was rejected.' }
if ($method.Invoke($null, @('%01$WD**', 'WD')) -ne $true) { throw 'PLC DT write acknowledgement was rejected.' }
if ($method.Invoke($null, @('%01$WD**', 'WC')) -eq $true) { throw 'PLC DT acknowledgement was accepted for coil write.' }
if ($method.Invoke($null, @('%01$WC**', 'WD')) -eq $true) { throw 'PLC coil acknowledgement was accepted for DT write.' }
if ($method.Invoke($null, @('%01$WC14', 'WC')) -ne $true) { throw 'PLC valid BCC acknowledgement was rejected.' }
if ($method.Invoke($null, @('%01$WC15', 'WC')) -eq $true) { throw 'PLC invalid BCC acknowledgement was accepted.' }
$read = $protocol.GetMethod('ParseSignal')
if ($read.Invoke($null, @('%01$CS1**')) -ne $true) { throw 'Valid PLC signal was rejected.' }
if ($read.Invoke($null, @('%01$RC120')) -ne $true) { throw 'Valid signal BCC was rejected.' }
try {
    [void]$read.Invoke($null, @('garbag1**'))
    throw 'Malformed PLC signal frame was accepted.'
} catch {
    if ($_.Exception.Message -eq 'Malformed PLC signal frame was accepted.') { throw }
}
try {
    [void]$read.Invoke($null, @('%01$RC121'))
    throw 'Invalid signal BCC was accepted.'
} catch {
    if ($_.Exception.Message -eq 'Invalid signal BCC was accepted.') { throw }
}
Write-Output 'PASS: PLC write acknowledgement validation'
