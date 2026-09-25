$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$designer = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.Designer.vb')
$code = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')

if ($designer -notmatch 'Me\.btnGetBarcode\s*=\s*New\s+System\.Windows\.Forms\.Button\(\)') {
    throw 'frmTest Designer 未声明 btnGetBarcode。'
}
if ($designer -notmatch 'controlBoardLayout\.ColumnCount\s*=\s*3') {
    throw 'controlBoardLayout 必须保持三列。'
}
if ($designer -notmatch 'controlBoardLayout\.Controls\.Add\(Me\.txtTray,\s*1,\s*0\)') {
    throw 'txtTray 必须放在 controlBoardLayout 第二列。'
}
if ($designer -notmatch 'controlBoardLayout\.Controls\.Add\(Me\.btnGetBarcode,\s*2,\s*0\)') {
    throw 'btnGetBarcode 必须放在 controlBoardLayout 第三列。'
}
if ($designer -match 'SetColumnSpan\(Me\.txtTray') {
    throw 'txtTray 不应再跨列。'
}
if ($designer -notmatch 'Me\.btnGetBarcode\.Text\s*=\s*"获取条码"') {
    throw 'btnGetBarcode 文本必须为“获取条码”。'
}
if ($code -notmatch 'Handles\s+btnGetBarcode\.Click') {
    throw 'btnGetBarcode 未绑定点击事件。'
}
if ($code -notmatch 'PrepareTrayForTest\(trayCode, chkAutoTest.Checked\)' -or
    $code -notmatch 'GetTrayCellsByTrayCode\(normalizedCode\)') {
    throw '获取条码按钮未执行共用的托盘电芯码查询。'
}
if ($code -notmatch 'RecieveData\(item\.Key\s*-\s*1\)\.BatCode\s*=') {
    throw 'MES 查询结果未按物理通道写入 RecieveData。'
}
if ($code -notmatch 'RefreshChannelOverview\(\)') {
    throw '获取条码成功后未刷新主页面。'
}

Write-Output 'frmTest get barcode contract: PASS'
