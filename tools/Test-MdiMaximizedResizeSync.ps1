$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')

if ($source -notmatch 'Private Sub MDIForm_Resize[\s\S]*?Handles Me\.Resize') {
    throw 'MDIForm must react to host resize events.'
}
if ($source -notmatch 'Private Const ResizeDebounceMs\s+As Integer\s*=\s*75') {
    throw 'MDI resize must use the 75 ms debounce interval.'
}
if ($source -notmatch 'ResizeDebounceTimer_Tick') {
    throw 'MDI resize must be processed by the debounce timer.'
}
if ($source -match 'child\.WindowState\s*=\s*FormWindowState\.Normal[\s\S]*?child\.WindowState\s*=\s*FormWindowState\.Maximized') {
    throw 'Resize must not force maximized MDI children through Normal -> Maximized.'
}

Write-Host 'PASS: MDI resize is debounced without forcing a maximize toggle.'
