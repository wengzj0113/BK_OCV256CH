$ErrorActionPreference = 'Stop'

$frmTestPath = Join-Path $PSScriptRoot '..\frmTest.vb'
$channelTilePath = Join-Path $PSScriptRoot '..\ChannelTileControl.vb'
$frmTest = Get-Content -LiteralPath $frmTestPath -Raw
$channelTile = Get-Content -LiteralPath $channelTilePath -Raw

if ($frmTest -notmatch 'Private ReadOnly testingChannelNumbers As New HashSet\(Of Integer\)\(\)') {
    throw 'frmTest does not keep an independent set of channels currently being tested.'
}
if ($frmTest -notmatch 'MarkSelectedChannelsTesting\(\)') {
    throw 'The normal test path does not mark selected channels as TESTING before SCM starts.'
}
if ($frmTest -notmatch 'End If\s*\r?\n\s*testingChannelNumbers\.Clear\(\)\s*\r?\n\s*RefreshChannelOverview\(\)\s*\r?\n\s*SaveOcvDataTestResults') {
    throw 'The test completion path does not refresh the final channel result after result assignment.'
}
$completionBlock = [regex]::Match($frmTest, "''刷新界面(?<body>[\s\S]*?)If testType = 1").Groups['body'].Value
if ($completionBlock -match 'RefreshChannelOverview\(\)') {
    throw 'The channel overview is still refreshed before the final OK/NG result assignment.'
}
if ($frmTest -notmatch 'testingChannelNumbers\.Contains\(channelNo\)') {
    throw 'Channel display state is not scoped to the selected channels under test.'
}
if ($channelTile -notmatch 'Case "TESTING"') {
    throw 'Channel tiles do not render the TESTING state.'
}

$emptyTrayBlock = [regex]::Match($frmTest, 'If strTrayNo = "" Then(?<body>[\s\S]*?)Else\s*\r?\n\s*Dim result2').Groups['body'].Value
if ($emptyTrayBlock -match 'frmInput\.ShowDialog|frmInput\.Activate') {
    throw 'An empty tray number still opens the tray input dialog.'
}
if ($emptyTrayBlock -notmatch 'MsgBox\(') {
    throw 'The empty tray number warning was removed.'
}
if ($frmTest -notmatch 'strTrayNo = lastTrayNo[\s\S]*?MessageBox\.Show[\s\S]*?MessageBoxButtons\.OKCancel') {
    throw 'The repeated tray number confirmation must remain.'
}
if ($frmTest -notmatch 'intOCV[\s\S]*?MessageBox\.Show[\s\S]*?MessageBoxButtons\.OKCancel') {
    throw 'The OCV confirmation must remain.'
}

Write-Output 'PASS: test-time channel state and final-result refresh wiring are present.'
