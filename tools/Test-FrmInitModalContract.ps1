$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Assert-Contains([string]$path, [string]$pattern, [string]$message) {
    $fullPath = Join-Path $root $path
    $text = Get-Content -LiteralPath $fullPath -Raw
    if ($text -notmatch [regex]::Escape($pattern)) { throw $message }
}

Assert-Contains 'MDIForm.vb' 'initForm.ShowDialog(Me)' '设置窗体未以 MDIForm 为 Owner 模态显示'
Assert-Contains 'frmInit.Designer.vb' 'Me.TopMost = True' 'frmInit 未设置为始终置顶'
Write-Output 'frmInit modal/topmost contract: PASS'
