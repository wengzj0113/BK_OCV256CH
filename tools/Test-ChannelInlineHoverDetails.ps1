$ErrorActionPreference = 'Stop'

$tileSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelTileControl.vb')
$gridSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.vb')

if ($tileSource -notmatch 'Event\s+ChannelHovered') {
    throw 'ChannelTileControl must publish a hover event.'
}
if ($tileSource -match '\.Show\s*\(') {
    throw 'ChannelTileControl must not show a screen-level tooltip.'
}
if ($gridSource -notmatch 'hoverDetailsLabel') {
    throw 'ChannelGridControl must own an inline hover details label.'
}
if ($gridSource -notmatch 'HandleChannelHovered') {
    throw 'ChannelGridControl must render tile hover data in the title row.'
}

Write-Host 'PASS: channel hover details are rendered inline in the grid title row.'
