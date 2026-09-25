$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb')
$status = [regex]::Match($source, '(?ms)^\s*Public Function GetSignalTrayStatus\(\).*?^\s*End Function').Value
if ($status -notmatch 'plcAutomationClient.ReadR151\(\)') { throw 'Manual serial preflight must read R151 through the ASCII PLC client.' }
$mode = [regex]::Match($source, '(?ms)^\s*Private Sub TestModeCheckedChanged\(.*?^\s*End Sub').Value
if ($mode -notmatch 'StartManualPlcConnection') { throw 'Manual mode must own the ASCII PLC connection.' }
Write-Output 'PASS: manual PLC serial preflight ownership'
