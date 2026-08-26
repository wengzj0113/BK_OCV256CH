param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $Root 'MDIForm.vb') -Raw
$method = [regex]::Match($source, 'Private Sub ApplyMdiClientLayout\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $method.Success) { throw 'ApplyMdiClientLayout method body not found.' }
$body = $method.Value

if ($body -notmatch 'Dim top As Integer\s*=\s*Math\.Max\(0,\s*MenuStrip\.Bottom\)') {
    throw 'Initial MDI layout does not explicitly start below the menu bar.'
}
if ($body -notmatch 'Dim bottom As Integer\s*=\s*Math\.Min\(ClientSize\.Height,\s*bottomStatusControl\.Top\)') {
    throw 'Initial MDI layout does not explicitly end above the bottom status bar.'
}

Write-Host 'PASS: initial MDI layout is bounded between the menu and bottom status bar.'
