$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$designer = Get-Content (Join-Path $root "frmTest.Designer.vb") -Raw
$form = Get-Content (Join-Path $root "frmTest.vb") -Raw
$layout = Get-Content (Join-Path $root "ResponsiveTestLayout.vb") -Raw

foreach ($name in @("chkAutoTest", "chkManualTest", "OCV0", "OCV1", "OCV2")) {
    if (($designer + $form) -notmatch [regex]::Escape($name)) { throw "Missing UI token: $name" }
}
foreach ($name in @("btnChannelSelectAll", "btnChannelClear", "btnChannelRectangle", "btnChannelSelectSingle")) {
    if ($designer -notmatch [regex]::Escape($name)) { throw "Missing channel button: $name" }
}
if ($form -notmatch "AutoScrollPosition\s*=\s*New Point\(0, 0\)") { throw "Missing startup scroll reset" }
if ($layout -notmatch "TableLayoutPanel|FlowLayoutPanel") { throw "Missing managed left layout container" }
if ($layout -notmatch "TakeCurrentStationControl") { throw "Missing current-station extraction path" }
if ($layout -match 'AddIfPresent\(existingChildren, orderedChildren, "GroupBox2"\)') { throw "Legacy station container must not be added to the left content" }
if ($form -notmatch "GroupBox2\.Visible\s*=\s*False") { throw "Legacy station container must be hidden" }
foreach ($name in @("Rbtn1", "Rbtn2", "Rbtn3")) {
    if ($form -notmatch ([regex]::Escape($name) + "\.Visible\s*=\s*False")) { throw "Station control must be hidden before selecting the configured station: $name" }
}
Write-Output "Left sidebar static checks passed."
