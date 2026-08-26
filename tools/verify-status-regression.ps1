$ErrorActionPreference = 'Stop'

$mdi = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')
$mdiDesigner = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.Designer.vb')
$module = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\Module1.vb')
$bottom = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\BottomStatusControl.vb')
$designer = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.Designer.vb')
$responsive = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ResponsiveTestLayout.vb')
$frmTest = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb')
$calibrationService = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\CalibrationService.vb')

if (($bottom -notmatch 'FormatStatus') -or ($bottom -notmatch '控制模块')) {
    throw 'Bottom status label is not 控制模块.'
}
if ($mdi -notmatch "databaseConnected") {
    throw 'Database status does not use databaseConnected.'
}
if ($mdi -match 'If\(blnOnLine') {
    throw 'Database status still uses blnOnLine.'
}
if ($module -notmatch "HttpWebRequest|HttpClient") {
    throw 'MES check does not use HTTP.'
}
if ($module -match 'login\.ShowDialog\(\).*Application\.Run') {
    throw 'Application startup still blocks on the login dialog.'
}
if ($module -notmatch 'Application\.Run\(mainWindow\)') {
    throw 'Application startup does not run MDIForm directly.'
}
if ($mdi -notmatch '登录') {
    throw 'MDIForm has no login menu entry.'
}
if ($mdi -notmatch 'New FrmLoginNew') {
    throw 'MDIForm login menu does not open FrmLoginNew.'
}
if ($mdiDesigner -notmatch 'Me\.Text = "OCV.*V1\.0"') {
    throw 'Application header title/version is not updated.'
}
if ($mdi -match 'XK\s+OCV' -or $mdi -match 'V2\.3\.0' -or $mdiDesigner -match 'XK\s+OCV' -or $mdiDesigner -match 'V2\.3\.0') {
    throw 'Legacy application title or version is still present.'
}
if ($mdiDesigner -notmatch 'Me\.Text = "OCV.*V1\.0"') {
    throw 'Window title bar text is not configured.'
}
if ($mdi -match 'navigationBrand|navigationVersion|Items\.Insert\(0, navigationBrand\)') {
    throw 'Application title is still being rendered in the menu bar.'
}
if ($mdiDesigner -match 'Controls\.Add\(Me\.txtTesting\)') {
    throw 'Legacy txtTesting label is still mounted on MDIForm.'
}
if ($designer -notmatch 'Label9\.TextAlign = .*MiddleCenter' -or $designer -notmatch 'Label8\.TextAlign = .*MiddleCenter') {
    throw 'Min/Max headers are not centered over their input columns.'
}
if ($designer -notmatch 'Label3\.TextAlign = .*MiddleRight' -or
    $designer -notmatch 'Label4\.TextAlign = .*MiddleRight' -or
    $designer -notmatch 'Label1\.TextAlign = .*MiddleRight' -or
    $designer -notmatch 'Label14\.TextAlign = .*MiddleRight') {
    throw 'Left-panel labels are not using consistent vertical alignment.'
}
if ($designer -notmatch 'Label5\.TextAlign = .*MiddleRight') {
    throw 'Database server label is not aligned with its input.'
}
if ($responsive -notmatch 'ColumnStyle\(SizeType\.Absolute, 330\.0F\)') {
    throw 'Left layout column is not fixed near 330px.'
}
if ($responsive -notmatch 'ResizeLeftContent\(leftPanel\)') {
    throw 'Left content does not resize with the sidebar.'
}
if ($frmTest -match 'Const filePath As String = "CalibrateArr\.csv"' -or
    $frmTest -notmatch 'SpecialFolder\.LocalApplicationData' -or
    $frmTest -notmatch '保存校准数组失败') {
    throw 'frmTest calibration file is not protected from WindowsApps write failure.'
}
if ($calibrationService -notmatch 'SpecialFolder\.LocalApplicationData') {
    throw 'CalibrationService still writes calibration data under the installation directory.'
}

Write-Output 'STATUS_REGRESSION: PASS'
