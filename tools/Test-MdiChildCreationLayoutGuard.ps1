$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')
if ($source -notmatch 'Private mdiChildCreationInProgress\s+As Boolean') {
    throw 'MDI must mark test-child creation while the child is being laid out.'
}
$method = [regex]::Match($source, 'Private Sub ApplyMdiClientLayout\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ($method -notmatch 'mdiChildCreationInProgress') {
    throw 'MDI client layout must be suppressed while frmTest is being created.'
}
Write-Output 'PASS: MDI layout is guarded during initial test-child creation.'
