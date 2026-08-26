$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')

if ($source -notmatch 'MenuStrip\.Renderer\s*=\s*New\s+HeaderMenuRenderer') {
    throw 'MenuStrip must use the header-aware renderer.'
}
if ($source -notmatch 'Class HeaderMenuRenderer') {
    throw 'HeaderMenuRenderer is missing.'
}
if ($source -notmatch 'e\.TextColor\s*=\s*Color\.White') {
    throw 'Top-level menu text must remain white when selected.'
}

Write-Host 'PASS: top-level menu text remains visible in selected state.'
