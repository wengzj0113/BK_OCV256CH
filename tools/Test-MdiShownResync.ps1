param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $Root 'MDIForm.vb') -Raw

if ($source -notmatch 'Private Sub NormalizeInitialMdiChildLayout\(\)') {
    throw 'MDIForm must have a post-Shown child layout synchronization step.'
}

$shown = [regex]::Match($source, 'Private Sub MDIForm_Shown\(.*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $shown.Success -or $shown.Value -notmatch 'NormalizeInitialMdiChildLayout') {
    throw 'MDIForm_Shown must schedule a child layout synchronization after frmTest is shown.'
}

$method = [regex]::Match($source, 'Private Sub NormalizeInitialMdiChildLayout\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $method.Success -or $method.Value -notmatch 'ApplyMdiClientLayout' -or $method.Value -notmatch 'WindowState\s*=\s*FormWindowState\.Maximized') {
    throw 'Post-Shown synchronization must relayout the MDI client and reapply maximized child bounds.'
}

Write-Host 'PASS: MDI child layout is resynchronized after the initial Shown cycle.'
