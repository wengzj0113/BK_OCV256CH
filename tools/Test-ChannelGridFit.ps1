$ErrorActionPreference = 'Stop'

$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelTileControl.vb')

if ($source -notmatch 'Margin\s*=\s*New Padding\(2,\s*2,\s*2,\s*2\)') {
    throw 'Channel tiles must use a compact 2px margin so all 16 rows can fit in the right-side viewport.'
}

if ($source -notmatch 'MinimumSize\s*=\s*New Size\(0,\s*0\)') {
    throw 'Channel tiles must not impose a fixed minimum height that clips the final matrix rows.'
}

$tileSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelTileControl.vb')
if ($tileSource -notmatch '\.AutoSize\s*=\s*False') {
    throw 'Channel labels must not contribute a wrapped preferred height to the matrix rows.'
}

$gridSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.vb')
if ($gridSource -match 'gridLayout\.(ColumnStyles|RowStyles)\.Add\(New (ColumnStyle|RowStyle)\(SizeType\.Absolute') {
    throw 'Channel tile rows/columns must not use absolute pixels (only the 44px row-number gutter may).'
}

if ($gridSource -notmatch 'outerLayout\.ColumnStyles\.Add\(New ColumnStyle\(SizeType\.Absolute, 44\.0F\)\)') {
    throw 'Row-number gutter must be a fixed 44px so row numbers stay readable and are not covered by the matrix.'
}

if ($gridSource -notmatch 'rowHeader\.ColumnStyles\.Add\(New ColumnStyle\(SizeType\.Percent, 100\.0F\)\)') {
    throw 'Row header must declare an explicit column style, or row labels overflow the gutter and get clipped.'
}

$statisticsSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\StatisticsControl.vb')
if ($statisticsSource -match 'SizeType\.Absolute') {
    throw 'Statistics layout must not use an absolute pixel row.'
}

Write-Output 'PASS: channel tile spacing permits the full 16x16 matrix to fit.'
