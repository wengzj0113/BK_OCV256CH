$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$utf8 = New-Object System.Text.UTF8Encoding($false)
$module = [IO.File]::ReadAllText((Join-Path $root 'Module1.vb'), $utf8)
$mdi = [IO.File]::ReadAllText((Join-Path $root 'MDIForm.vb'), $utf8)

function Assert-Contains([string]$source, [string]$pattern, [string]$message) {
    if ($source -notmatch $pattern) { throw $message }
}

function Assert-NotContains([string]$source, [string]$pattern, [string]$message) {
    if ($source -match $pattern) { throw $message }
}

$mainMethod = [regex]::Match($module, 'Public Sub main\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($mainMethod)) { throw 'Module1.main method body not found.' }

Assert-Contains $mainMethod 'ReadInitFile\(\)' 'Startup must read init settings.'
Assert-Contains $mainMethod 'mainWindow\.operTypeNum\s*=\s*0' 'Startup must use unauthenticated permissions.'
Assert-NotContains $mainMethod 'operatorName\s*=\s*.*Admin' 'Startup must not fake an Admin login.'
Assert-NotContains $mainMethod '默认使用 Admin 身份' 'Startup must not document a default Admin identity.'

Assert-Contains $mdi 'Public operTypeNum As Integer\s*=\s*0' 'MDIForm default permissions must be unauthenticated.'
Assert-Contains $mdi 'AuthorizationService\.CurrentUserName' 'Status bar must use the centralized current-user session.'
Assert-Contains $mdi 'String\.IsNullOrWhiteSpace\(AuthorizationService\.CurrentUserName\),\s*"' 'Empty username must display an explicit unauthenticated label.'
Assert-NotContains $mdi 'String\.IsNullOrWhiteSpace\(AuthorizationService\.CurrentUserName\),\s*"Admin"' 'Empty username must not fall back to Admin.'

Write-Output 'Current-user startup contract: PASS'
