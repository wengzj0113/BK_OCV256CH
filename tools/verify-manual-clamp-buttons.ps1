$ErrorActionPreference = 'Stop'
$path = Join-Path (Split-Path -Parent $PSScriptRoot) 'FrmManualTest.vb'
$text = Get-Content -Raw -LiteralPath $path
$required = @(
    '夹具压合',
    '夹具松开',
    'CloseTray()',
    'OpenTray()',
    'ReadTrayStatus()'
)
$missing = @($required | Where-Object { $text -notmatch [regex]::Escape($_) })
if ($missing.Count -gt 0) {
    Write-Host 'MANUAL_CLAMP_BUTTONS: FAIL'
    $missing | ForEach-Object { Write-Host "Missing: $_" }
    exit 1
}
Write-Host 'MANUAL_CLAMP_BUTTONS: PASS'
