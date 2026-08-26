$ErrorActionPreference = 'Stop'

$module = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\Module1.vb')
$frmTest = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb')
$mdi = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')
$designer = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.Designer.vb')

# Remove the disabled legacy IO implementation before checking active source.
$activeModule = [regex]::Replace($module, '(?s)#If False Then.*?#End If', '')

if ($activeModule -match 'Com_IOCard\.(Open|Close|Write|Read|ReadByte|BytesToRead)') {
    throw 'Active Module1.vb still accesses Com_IOCard.'
}
if ($activeModule -match 'With\s+frmMDI\.Com_IOCard') {
    throw 'Active Module1.vb still configures Com_IOCard.'
}
if ($activeModule -notmatch 'IO 板动作由控制板执行') {
    throw 'IO bypass compatibility entry points are missing.'
}
if ($module -notmatch '#If False Then' -or $module -notmatch '#End If') {
    throw 'Legacy IO serial implementation is not disabled.'
}
$startupBlock = [regex]::Match($module, '(?s)Public Sub MainwindowsShowAsync.*?End Sub').Value
if ($startupBlock -match 'CheckCom_IOCard\(\)') {
    throw 'Startup device check still invokes CheckCom_IOCard.'
}
if ($mdi -match 'Com_IOCard\.Open|Com_IOCard\.Write|Com_IOCard\.Read') {
    throw 'MDIForm directly accesses Com_IOCard.'
}
if ($designer -notmatch 'Com_IOCard = New System\.IO\.Ports\.SerialPort') {
    throw 'Designer compatibility field for Com_IOCard is missing.'
}
if ($frmTest -notmatch 'IOCardSetAcirOrOcv') {
    throw 'Test flow no longer contains the compatibility call expected to be bypassed.'
}

Write-Output 'CONTROL_BOARD_ONLY: PASS'
