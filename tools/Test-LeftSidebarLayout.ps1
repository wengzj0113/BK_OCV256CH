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
Write-Output "Left sidebar static checks passed."
