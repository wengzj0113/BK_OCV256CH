$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb')
$initial = [regex]::Match($source, '(?ms)^\s*Public Sub StartInitialPass\(.*?^\s*End Sub').Value
$retest = [regex]::Match($source, '(?ms)^\s*Public Sub StartRetestPass\(.*?^\s*End Sub').Value
if ($initial -match 'SelectedChannelNumbers\s*=') { throw 'Automatic initial pass overwrites the operator channel selection.' }
if ($retest -match 'SelectedChannelNumbers\s*=') { throw 'Automatic retest overwrites the operator channel selection.' }
Write-Output 'PASS: automatic passes preserve manual channel selection'
