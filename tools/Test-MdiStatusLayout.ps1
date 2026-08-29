$ErrorActionPreference = 'Stop'

$sourcePath = Join-Path $PSScriptRoot '..\MDIForm.vb'
$source = Get-Content -Raw -LiteralPath $sourcePath

if ($source -notmatch 'ApplyMdiClientLayout') {
    throw 'MDIForm must reserve the bottom status control area from the MDI client.'
}

$method = [regex]::Match($source, 'Private Sub ApplyMdiClientLayout\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $method.Success) {
    throw 'ApplyMdiClientLayout method body not found.'
}
$body = $method.Value

if ($body -notmatch 'mdiClient\.Dock\s*=\s*DockStyle\.Fill') {
    throw 'MDI client must fill the naturally calculated area left by the menu and status bar.'
}

if ($body -match 'mdiClient\.Dock\s*=\s*DockStyle\.None|mdiClient\.SetBounds') {
    throw 'MDI client must not use cached manual bounds.'
}

Write-Output 'PASS: MdiClient uses Dock=Fill and leaves menu/status space to WinForms.'
