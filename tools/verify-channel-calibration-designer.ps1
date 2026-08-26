$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$codePath = Join-Path $root 'FrmChannelCalibration.vb'
$designerPath = Join-Path $root 'FrmChannelCalibration.Designer.vb'
$code = Get-Content -Raw -LiteralPath $codePath
$required = @(
    'Partial Public Class FrmChannelCalibration',
    'InitializeComponent()'
)
$missing = @($required | Where-Object { $code -notmatch [regex]::Escape($_) })
if (-not (Test-Path $designerPath)) { $missing += 'FrmChannelCalibration.Designer.vb' }
if ($missing.Count -gt 0) {
    Write-Host 'CHANNEL_CALIBRATION_DESIGNER: FAIL'
    $missing | ForEach-Object { Write-Host "Missing: $_" }
    exit 1
}
if ($code -match 'ConfigureDesignerButton' -or ((Test-Path $designerPath) -and ((Get-Content -Raw -LiteralPath $designerPath) -match 'ConfigureDesignerButton'))) {
    Write-Host 'CHANNEL_CALIBRATION_DESIGNER: FAIL'
    Write-Host 'Designer contains unsupported custom helper call: ConfigureDesignerButton'
    exit 1
}
Write-Host 'CHANNEL_CALIBRATION_DESIGNER: PASS'
