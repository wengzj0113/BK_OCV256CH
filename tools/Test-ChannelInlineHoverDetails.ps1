$ErrorActionPreference = 'Stop'

$tileSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelTileControl.vb')
$gridSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ChannelGridControl.vb')

if ($tileSource -match '\.Show\s*\(') {
    throw 'ChannelTileControl must not show a screen-level tooltip.'
}
if ($gridSource -notmatch 'hoverDetailsLabel') {
    throw 'ChannelGridControl must own an inline hover details label.'
}
if ($gridSource -notmatch 'HandleGridMouseMove' -or $gridSource -notmatch 'BuildHoverDetailsText') {
    throw 'ChannelGridControl must render channel hover data in the title row.'
}

Write-Host 'PASS: channel hover details are rendered inline in the grid title row.'
