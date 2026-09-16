$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$mdi = Get-Content -Raw -LiteralPath (Join-Path $root 'MDIForm.vb')

function Assert-Contains([string]$source, [string]$pattern, [string]$message) {
    if ($source -notmatch $pattern) { throw $message }
}

function Assert-NotContains([string]$source, [string]$pattern, [string]$message) {
    if ($source -match $pattern) { throw $message }
}

$mainMethod = [regex]::Match($module, 'Public Sub main\(\).*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($mainMethod)) { throw 'Module1.main method body not found.' }

Assert-Contains $mainMethod 'ReadInitFile\(\)' '启动入口必须读取初始化配置。'
Assert-Contains $mainMethod 'mainWindow\.operTypeNum\s*=\s*0' '启动入口必须以未登录权限创建主窗体。'
Assert-NotContains $mainMethod 'operatorName\s*=\s*["'']Admin["'']' '启动入口不能伪造 Admin 登录身份。'
Assert-NotContains $mainMethod '默认使用 Admin 身份' '启动注释不能再声明默认 Admin 身份。'

Assert-Contains $mdi 'Public operTypeNum As Integer\s*=\s*0' 'MDIForm 默认权限必须是未登录。'
Assert-Contains $mdi 'If\(String\.IsNullOrWhiteSpace\(operatorName\),\s*["'']未登录["'']' '空用户名必须显示未登录。'
Assert-NotContains $mdi 'If\(String\.IsNullOrWhiteSpace\(operatorName\),\s*["'']Admin["'']' '空用户名不能回退显示 Admin。'
Assert-Contains $mdi 'operatorName\s*=\s*Initsetting\.curOperator' '登录成功后仍必须显示实际登录用户。'

Write-Output 'Current-user startup contract: PASS'
