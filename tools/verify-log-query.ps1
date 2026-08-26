$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$requiredFiles = @('AppLog.vb', 'FrmLogQuery.vb', 'FrmLogQuery.Designer.vb')
$missingFiles = @($requiredFiles | Where-Object { -not (Test-Path (Join-Path $root $_)) })
if ($missingFiles.Count -gt 0) {
    Write-Host 'LOG_QUERY: FAIL'
    $missingFiles | ForEach-Object { Write-Host "Missing file: $_" }
    exit 1
}
$appLog = Get-Content -Raw -LiteralPath (Join-Path $root 'AppLog.vb')
$mdi = Get-Content -Raw -LiteralPath (Join-Path $root 'MDIForm.Designer.vb')
$mdiCode = Get-Content -Raw -LiteralPath (Join-Path $root 'MDIForm.vb')
$query = Get-Content -Raw -LiteralPath (Join-Path $root 'FrmLogQuery.vb')
$required = @(
    'LocalApplicationData',
    'LogDirectory',
    'GetLogFiles',
    'TlStripMenuLog',
    '日志查询',
    'FrmLogQuery'
)
$all = $appLog + "`n" + $mdi + "`n" + $mdiCode + "`n" + $query
$missing = @($required | Where-Object { $all -notmatch [regex]::Escape($_) })
if ($missing.Count -gt 0) {
    Write-Host 'LOG_QUERY: FAIL'
    $missing | ForEach-Object { Write-Host "Missing: $_" }
    exit 1
}
Write-Host 'LOG_QUERY: PASS'
