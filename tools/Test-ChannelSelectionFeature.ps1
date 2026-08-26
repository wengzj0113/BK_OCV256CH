$ErrorActionPreference = 'Stop'

$tileSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelTileControl.vb')
$gridSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.vb')
$formSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb')
$designerSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.Designer.vb')
$themeSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\UiTheme.vb')

if ($gridSource -notmatch 'Enum\s+ChannelSelectionMode') { throw 'Selection mode enum is missing.' }
if ($gridSource -notmatch 'SelectAll\s*\(') { throw 'SelectAll API is missing.' }
if ($gridSource -notmatch 'ClearSelection\s*\(') { throw 'ClearSelection API is missing.' }
if ($gridSource -notmatch 'SelectedChannelsChanged') { throw 'SelectedChannelsChanged event is missing.' }
if ($gridSource -notmatch 'MouseMove') { throw 'Rectangle selection mouse tracking is missing.' }
if ($tileSource -notmatch 'IsSelected') { throw 'Tile selected state is missing.' }
if ($formSource -notmatch 'lblBarNum') { throw 'Left-side pending channel count value is missing.' }
if ($gridSource -notmatch 'Barcode') { throw 'Hover details must include the battery barcode field.' }
if ($gridSource -notmatch 'ForeColor = UiTheme.TextPrimary') { throw 'Hover details must use the high-contrast primary text color.' }
if ($themeSource -notmatch 'TextPrimary As Color') { throw 'Primary text theme color is missing.' }
if ($designerSource -notmatch 'btnChannelSelectAll') { throw 'Selection buttons must be Designer controls.' }
if ($designerSource -notmatch 'grpChannelSelection') { throw 'Selection button group must be Designer-visible.' }
if ($tileSource -notmatch 'Overrides Sub OnPaint') { throw 'Selected border must be painted by the tile control itself.' }
if ($designerSource -notmatch 'Label3.Visible = False' -or $designerSource -notmatch 'Label4.Visible = False') { throw 'First/last battery position labels must be hidden.' }
if ($designerSource -notmatch 'txtFirstNum.Visible = False' -or $designerSource -notmatch 'txtLastNum.Visible = False') { throw 'First/last battery position fields must be hidden.' }

Write-Host 'PASS: channel selection and hover requirements are present.'
