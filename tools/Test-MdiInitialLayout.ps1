param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $Root 'MDIForm.vb') -Raw
$method = [regex]::Match($source, 'Private Sub ApplyMdiClientLayout\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $method.Success) { throw 'ApplyMdiClientLayout method body not found.' }
$body = $method.Value

if ($body -notmatch 'mdiClient\.Dock\s*=\s*DockStyle\.Fill') {
    throw 'Initial MDI layout must let MdiClient fill the naturally calculated client area.'
}
if ($body -match 'mdiClient\.Dock\s*=\s*DockStyle\.None|mdiClient\.SetBounds') {
    throw 'Initial MDI layout must not cache manual MdiClient bounds.'
}

Write-Host 'PASS: initial MDI layout uses Dock=Fill between the menu and bottom status bar.'
