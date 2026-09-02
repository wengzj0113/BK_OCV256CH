param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $Root 'ChannelGridControl.vb') -Raw

if ($source -notmatch 'AddHandler\s+gridSurface\.MouseMove\s*,\s*AddressOf\s+HandleGridMouseMove') {
    throw 'Missing grid MouseMove handler for inline hover details.'
}

if ($source -notmatch 'AddHandler\s+gridSurface\.MouseLeave\s*,\s*AddressOf\s+HandleGridMouseLeave') {
    throw 'Missing grid MouseLeave handler for inline hover cleanup.'
}

if ($source -notmatch 'Private Sub HandleGridMouseLeave') {
    throw 'Missing HandleGridMouseLeave implementation.'
}

if ($source -notmatch 'HandleGridMouseLeave[\s\S]*?hoverDetailsLabel\.Text\s*=\s*String\.Empty') {
    throw 'MouseLeave does not clear inline hover details.'
}

if ($source -notmatch 'TryGetValue\(e\.Location|TryGetValue\(channelNo') {
    throw 'MouseMove can still publish hover data without a bound channel lookup.'
}

Write-Host 'PASS: inline channel hover lifecycle is guarded and cleaned up on leave.'
