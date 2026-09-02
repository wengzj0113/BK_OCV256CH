$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$designer = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.Designer.vb')
$form = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')

foreach ($name in @('chkAutoTest', 'chkManualTest', 'OCV0', 'OCV1', 'OCV2', 'currentStationButton', 'leftContentPanel')) {
    if (($designer + $form) -notmatch [regex]::Escape($name)) { throw "Missing UI token: $name" }
}
foreach ($name in @('btnChannelSelectAll', 'btnChannelClear', 'btnChannelRectangle', 'btnChannelSelectSingle')) {
    if ($designer -notmatch [regex]::Escape($name)) { throw "Missing channel button: $name" }
}
if ($designer -notmatch 'Panel1\.AutoScroll\s*=\s*True') { throw 'Serialized sidebar scrolling is missing.' }
if ($designer -notmatch 'leftContentPanel\.Dock\s*=\s*System\.Windows\.Forms\.DockStyle\.Top') { throw 'Serialized sidebar content must dock to the top of the scrollable panel.' }
if ($designer -match 'Controls\.Clear\s*\(') { throw 'Designer must not clear and rebuild the sidebar control tree.' }
if ($form -notmatch 'GroupBox2\.Visible\s*=\s*False') { throw 'Legacy station container must remain hidden at runtime.' }
foreach ($name in @('Rbtn1', 'Rbtn2', 'Rbtn3')) {
    if ($form -notmatch ([regex]::Escape($name) + '\.Visible\s*=\s*False')) { throw "Station control must be hidden before selecting the configured station: $name" }
}
Write-Output 'PASS: serialized left sidebar names, hierarchy, scrolling and station visibility wiring are present.'
