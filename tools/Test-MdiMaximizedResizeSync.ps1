$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')

if ($source -notmatch 'Private Sub MDIForm_Resize[\s\S]*?Handles Me\.Resize') {
    throw 'MDIForm must react to host resize events.'
}
if ($source -notmatch 'SyncMaximizedMdiChildren') {
    throw 'MDIForm must resynchronize maximized MDI children after host resize.'
}
if ($source -notmatch 'child\.WindowState\s*=\s*FormWindowState\.Normal[\s\S]*?child\.WindowState\s*=\s*FormWindowState\.Maximized') {
    throw 'Maximized MDI children must be refreshed after the host client area changes.'
}

Write-Host 'PASS: maximized MDI children are synchronized after host resize.'
