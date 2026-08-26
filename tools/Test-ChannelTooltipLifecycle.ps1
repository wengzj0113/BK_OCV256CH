param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $Root 'ChannelTileControl.vb') -Raw

if ($source -notmatch 'AddHandler\s+Me\.MouseLeave\s*,\s*AddressOf\s+HandleMouseLeave') {
    throw 'Missing parent MouseLeave handler for inline hover cleanup.'
}

if ($source -notmatch 'AddHandler\s+channelLabel\.MouseLeave\s*,\s*AddressOf\s+HandleMouseLeave') {
    throw 'Missing label MouseLeave handler for inline hover cleanup.'
}

if ($source -notmatch 'Private Sub HandleMouseLeave') {
    throw 'Missing HandleMouseLeave implementation.'
}

if ($source -notmatch 'RaiseEvent\s+ChannelHoverEnded') {
    throw 'MouseLeave does not clear inline hover details.'
}

if ($source -notmatch 'If Not hasDisplayData Then Return') {
    throw 'MouseEnter can still publish hover data before channel data is bound.'
}

Write-Host 'PASS: inline channel hover lifecycle is guarded and cleaned up on leave.'
