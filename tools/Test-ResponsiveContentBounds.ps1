$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ResponsiveTestLayout.vb')
if ($source -notmatch '\.AutoSize\s*=\s*False') {
    throw 'Scrollable left content must not advertise AutoSize to the outer TableLayoutPanel.'
}
if ($source -notmatch 'leftContentPanel\.Height\s*=') {
    throw 'Scrollable left content must calculate an internal content height.'
}
Write-Output 'PASS: left scrolling content has an internal height and cannot expand the outer layout.'
