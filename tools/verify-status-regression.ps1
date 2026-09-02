$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$mdi = Get-Content -Raw -LiteralPath (Join-Path $root 'MDIForm.vb')
$mdiDesigner = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $root 'MDIForm.Designer.vb')
$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$bottom = Get-Content -Raw -LiteralPath (Join-Path $root 'BottomStatusControl.vb')
$designer = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.Designer.vb')
$frmTest = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')
$calibrationService = Get-Content -Raw -LiteralPath (Join-Path $root 'CalibrationService.vb')

if ($bottom -notmatch 'FormatStatus' -or $bottom -notmatch '控制模块') { throw 'Bottom status label is not 控制模块.' }
if ($mdi -notmatch 'databaseConnected' -or $mdi -match 'If\(blnOnLine') { throw 'Database status does not use databaseConnected.' }
if ($module -notmatch 'HttpWebRequest|HttpClient') { throw 'MES check does not use HTTP.' }
if ($module -match 'login\.ShowDialog\(\).*Application\.Run' -or $module -notmatch 'Application\.Run\(mainWindow\)') { throw 'Application startup login flow is invalid.' }
$loginTextIndex = $mdiDesigner.IndexOf('loginMenu.Text', [StringComparison]::Ordinal)
$loginTypeIndex = $mdi.IndexOf('New FrmLoginNew', [StringComparison]::Ordinal)
if ($loginTextIndex -lt 0 -or $loginTypeIndex -lt 0) { throw "MDIForm login menu is missing: textIndex=$loginTextIndex typeIndex=$loginTypeIndex." }
if ($mdiDesigner -notmatch 'Me\.Text = "OCV.*V1\.0"' -or $mdiDesigner -match 'XK\s+OCV|V2\.3\.0') { throw 'Application title/version regression detected.' }
if ($mdi -match 'navigationBrand|navigationVersion|Items\.Insert\(0, navigationBrand\)' -or $mdiDesigner -match 'Controls\.Add\(Me\.txtTesting\)') { throw 'Legacy title/status controls remain mounted incorrectly.' }
foreach ($label in @('Label9', 'Label8', 'Label3', 'Label4', 'Label1', 'Label14', 'Label5')) {
    if ($designer -notmatch ([regex]::Escape($label) + '\.TextAlign = .*Middle')) { throw "Alignment missing for $label." }
}
if ($designer -notmatch 'Me\.rootLayout\.ColumnStyles\.Add\(New System\.Windows\.Forms\.ColumnStyle\(System\.Windows\.Forms\.SizeType\.Absolute, 350\.0!\)\)') { throw 'Left layout column is not fixed at 350px.' }
if ($frmTest -match 'Const filePath As String = "CalibrateArr\.csv"' -or $frmTest -notmatch 'SpecialFolder\.LocalApplicationData' -or $frmTest -notmatch '保存校准数组失败') { throw 'frmTest calibration file is not protected from install-directory write failure.' }
if ($calibrationService -notmatch 'SpecialFolder\.LocalApplicationData') { throw 'CalibrationService still writes calibration data under the installation directory.' }
Write-Output 'STATUS_REGRESSION: PASS'
