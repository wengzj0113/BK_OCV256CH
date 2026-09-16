$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$assemblyPath = if ($env:GET_BARCODE_EXE) {
    $env:GET_BARCODE_EXE
} else {
    Join-Path $root 'bin\build-verify-get-barcode2\XL_OCV_256CH.exe'
}
if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "找不到主程序构建产物: $assemblyPath"
}

$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
try {
    $layoutField = $formType.GetField('controlBoardLayout', [Reflection.BindingFlags] 'Instance,NonPublic')
    $layout = $layoutField.GetValue($form)
    $txtTray = $layout.Controls | Where-Object { $_.Name -eq 'txtTray' } | Select-Object -First 1
    $button = $layout.Controls | Where-Object { $_.Name -eq 'btnGetBarcode' } | Select-Object -First 1
    if ($null -eq $txtTray -or $null -eq $button) { throw '托盘码输入框或获取条码按钮未创建。' }
    if ($layout.ColumnCount -ne 3) { throw "controlBoardLayout 列数错误: $($layout.ColumnCount)" }
    $txtPosition = $layout.GetCellPosition($txtTray)
    $buttonPosition = $layout.GetCellPosition($button)
    if ($txtPosition.Column -ne 1 -or $buttonPosition.Column -ne 2 -or $txtPosition.Row -ne 0 -or $buttonPosition.Row -ne 0) {
        throw "控件位置错误: txtTray=($($txtPosition.Column),$($txtPosition.Row)), btnGetBarcode=($($buttonPosition.Column),$($buttonPosition.Row))"
    }
    if ($button.Text -ne '获取条码') { throw "按钮文本错误: $($button.Text)" }
    Write-Output 'frmTest get barcode runtime layout: PASS'
}
finally {
    $form.Dispose()
}
