$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')

if ($source -notmatch 'Private mdiLayoutOrderApplied\s+As Boolean') {
    throw 'MDI layout must remember that z-order has already been normalized.'
}
$method = [regex]::Match($source, 'Private Sub ApplyMdiClientLayout\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($method)) { throw 'ApplyMdiClientLayout method body not found.' }
if ($method -notmatch 'If Not mdiLayoutOrderApplied Then[\s\S]*?BringToFront\(\)[\s\S]*?SendToBack\(\)[\s\S]*?mdiLayoutOrderApplied\s*=\s*True') {
    throw 'MDI z-order normalization must run once instead of on every Layout event.'
}
if ($method -notmatch 'If MenuStrip\.Dock\s*<>\s*DockStyle\.Top Then[\s\S]*?MenuStrip\.Dock\s*=\s*DockStyle\.Top') {
    throw 'MenuStrip.Dock must only be assigned when it actually changes.'
}
if ($method -notmatch 'If bottomStatusControl\.Dock\s*<>\s*DockStyle\.Bottom Then[\s\S]*?bottomStatusControl\.Dock\s*=\s*DockStyle\.Bottom') {
    throw 'BottomStatusControl.Dock must only be assigned when it actually changes.'
}
if ($method -notmatch 'If mdiClient\.Dock\s*<>\s*DockStyle\.Fill Then[\s\S]*?mdiClient\.Dock\s*=\s*DockStyle\.Fill') {
    throw 'MdiClient.Dock must only be assigned when it actually changes.'
}

Write-Output 'PASS: MDI layout z-order normalization is one-shot and cannot loop on Layout.'
