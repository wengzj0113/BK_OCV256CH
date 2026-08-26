param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $Root 'ChannelTileControl.vb') -Raw

if ($source -match 'ToolTip|tooltipWidth|PointToScreen') {
    throw 'Screen-level tooltip positioning should no longer be used for channel hover details.'
}

Write-Host 'PASS: channel hover details no longer depend on screen-level tooltip positioning.'
