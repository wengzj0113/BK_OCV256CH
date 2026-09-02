$ErrorActionPreference = 'Stop'
$designer = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.Designer.vb')
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.vb')

if ($designer -notmatch 'gridSurface\s*=\s*New System\.Windows\.Forms\.Panel\(\)') { throw 'ChannelGridControl must use one framework Panel as its drawing surface.' }
if ($designer -notmatch 'Private gridSurface As System\.Windows\.Forms\.Panel') { throw 'The serialized drawing surface must be a framework Panel.' }
if ($source -match 'Private ReadOnly tileControls|BuildTiles\(\)') { throw 'ChannelGridControl must not create and retain 256 child tile controls.' }
if ($source -notmatch 'Private ReadOnly displayDataByChannel') { throw 'ChannelGridControl must retain channel display data without child controls.' }
if ($source -notmatch 'HandleGridPaint') { throw 'ChannelGridControl must paint all channel cells on its drawing surface.' }
Write-Output 'PASS: channel overview is virtualized into one framework drawing surface.'
