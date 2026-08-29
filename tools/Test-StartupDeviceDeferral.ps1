$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$frmTest = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')

function Assert-Contains([string]$source, [string]$pattern, [string]$message) {
    if ($source -notmatch $pattern) { throw $message }
}

function Assert-NotContains([string]$source, [string]$pattern, [string]$message) {
    if ($source -match $pattern) { throw $message }
}

$mainMethod = [regex]::Match($module, 'Public Sub main\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($mainMethod)) { throw 'Module1.main method body not found.' }
Assert-NotContains $mainMethod 'MainwindowsShowAsync\s*\(' 'Device startup checks must not start before Application.Run and first paint.'
Assert-Contains $mainMethod 'frmMDI\s*=\s*mainWindow' 'The global MDI reference must be set before the post-paint startup phase.'

$deferredMethod = [regex]::Match($frmTest, 'Private Sub StartDeferredStartupWork\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($deferredMethod)) { throw 'StartDeferredStartupWork method body not found.' }
Assert-Contains $deferredMethod 'MainwindowsShowAsync\s*\(' 'Device startup checks must start from the post-first-paint startup phase.'
Assert-Contains $deferredMethod 'timer3\.Start\(\)' 'timer3 must remain in the post-first-paint phase.'
Assert-Contains $deferredMethod 'Timer4\.Start\(\)' 'Timer4 must remain in the post-first-paint phase.'

Write-Output 'PASS: device checks and polling are deferred until after the first page paint.'
