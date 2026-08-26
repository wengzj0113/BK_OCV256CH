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

if ($body -notmatch 'mdiClient\.Dock\s*=\s*DockStyle\.None') {
    throw 'ApplyMdiClientLayout must take explicit ownership of the MDI client bounds.'
}

if ($body -notmatch 'mdiClient\.SetBounds\(target\.X') {
    throw 'ApplyMdiClientLayout must set the MDI client bounds after the menu and status controls are laid out.'
}

if ($body -notmatch 'MenuStrip\.Bottom' -or $body -notmatch 'bottomStatusControl\.Top') {
    throw 'MDI client bounds must be derived from the actual menu bottom and status bar top.'
}

Write-Output 'PASS: MdiClient is explicitly bounded between menu bottom and status top.'
