$ErrorActionPreference = 'Stop'

$designer = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.Designer.vb')
$statisticsDesigner = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\StatisticsControl.Designer.vb')

if ($designer -notmatch 'outerLayout\.ColumnStyles\.Add\(New System\.Windows\.Forms\.ColumnStyle\(System\.Windows\.Forms\.SizeType\.Absolute, 44\.0!\)\)') { throw 'Row-number gutter must remain a fixed 44px.' }
if ($designer -notmatch 'rowHeader\.ColumnStyles\.Add\(New System\.Windows\.Forms\.ColumnStyle\(System\.Windows\.Forms\.SizeType\.Percent, 100\.0!\)\)') { throw 'Row header must declare an explicit column style.' }
if ($designer -notmatch 'gridSurface\.Dock\s*=\s*System\.Windows\.Forms\.DockStyle\.Fill') { throw 'Drawing surface must fill the matrix cell.' }
if ($designer -match 'columnHeader\.ColumnStyles\.Add\(New System\.Windows\.Forms\.ColumnStyle\(System\.Windows\.Forms\.SizeType\.Absolute') { throw 'Channel columns must not use absolute pixels.' }
if ($designer -match 'rowHeader\.RowStyles\.Add\(New System\.Windows\.Forms\.RowStyle\(System\.Windows\.Forms\.SizeType\.Absolute') { throw 'Channel rows must not use absolute pixels.' }
if ($statisticsDesigner -match 'System\.Windows\.Forms\.SizeType\.Absolute') { throw 'Statistics layout must not use an absolute pixel row.' }
Write-Output 'PASS: channel grid and statistics Designer layouts use scalable rows/columns and preserve the fixed row gutter.'
