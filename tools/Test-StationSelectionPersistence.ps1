$ErrorActionPreference = 'Stop'

$formPath = Join-Path $PSScriptRoot '..\frmInit.vb'
$form = Get-Content -LiteralPath $formPath -Raw
$selectionBlock = [regex]::Match($form, "'工作站选择[\s\S]*?Return True").Value

if ([string]::IsNullOrWhiteSpace($selectionBlock)) {
    throw 'Could not locate the workstation selection save block.'
}

if ($selectionBlock -notmatch 'If\s+radioOCV0Set\.Checked\s+Then[\s\S]*?stationSelect\s*=\s*0[\s\S]*?ElseIf\s+radioOCV1Set\.Checked\s+Then[\s\S]*?stationSelect\s*=\s*1[\s\S]*?Else[\s\S]*?stationSelect\s*=\s*2') {
    throw 'Workstation selection must use one mutually exclusive 0/1/2 branch.'
}

if ($selectionBlock -match 'End If\s*\r?\n\s*If\s+radioOCV1Set\.Checked') {
    throw 'OCV0 selection can be overwritten by a second independent If block.'
}

$testFormPath = Join-Path $PSScriptRoot '..\frmTest.vb'
$testForm = Get-Content -LiteralPath $testFormPath -Raw
if ($testForm -notmatch 'Case 1[\s\S]*?currentStationButton\.Text\s*=\s*"OCV1"[\s\S]*?Case 2[\s\S]*?currentStationButton\.Text\s*=\s*"OCV2"[\s\S]*?Case Else[\s\S]*?currentStationButton\.Text\s*=\s*"OCV0"') {
    throw 'Startup workstation mapping is not 0=OCV0, 1=OCV1, 2=OCV2.'
}

Write-Output 'PASS: workstation selection persistence and startup mapping are protected.'
