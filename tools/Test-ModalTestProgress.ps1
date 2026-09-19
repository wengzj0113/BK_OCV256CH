$ErrorActionPreference = 'Stop'

$dialogPath = Join-Path $PSScriptRoot '..\frmTestProgress.vb'
$designerPath = Join-Path $PSScriptRoot '..\frmTestProgress.Designer.vb'
$formProjectPath = Join-Path $PSScriptRoot '..\MP-60A-RV-64Mem.vbproj'
$frmTestPath = Join-Path $PSScriptRoot '..\frmTest.vb'
$designerText = if (Test-Path $designerPath) { Get-Content -Raw -LiteralPath $designerPath } else { '' }
$dialogText = if (Test-Path $dialogPath) { Get-Content -Raw -LiteralPath $dialogPath } else { '' }
$projectText = Get-Content -Raw -LiteralPath $formProjectPath
$frmTestText = Get-Content -Raw -LiteralPath $frmTestPath

if (-not (Test-Path $dialogPath)) { throw 'frmTestProgress.vb is missing.' }
if (-not (Test-Path $designerPath)) { throw 'frmTestProgress.Designer.vb is missing.' }
if ($projectText -notmatch 'Compile Include="frmTestProgress\.vb"') { throw 'frmTestProgress.vb is not included in the project.' }
if ($projectText -notmatch 'Compile Include="frmTestProgress\.Designer\.vb"') { throw 'frmTestProgress.Designer.vb is not included in the project.' }

foreach ($required in @('BeginProgress', 'ApplySnapshot', 'MarkCompleted', 'MarkFailed')) {
    if ($dialogText -notmatch "Public Sub $required") { throw "Progress dialog method $required is missing." }
}
foreach ($required in @('TopMost = True', 'FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog', 'ControlBox = False')) {
    if ($designerText -notmatch [regex]::Escape($required)) { throw "Progress dialog must configure $required." }
}
foreach ($required in @('lblProgressStage', 'progressTest', 'lblProgressCount', 'lblProgressElapsed', 'lblProgressEta', 'lblProgressDetail', 'btnClose')) {
    $namePattern = 'Name = "' + [regex]::Escape($required) + '"'
    if ($designerText -notmatch $namePattern) { throw "Progress dialog control $required is missing from Designer." }
}
if ($frmTestText -notmatch 'Private testProgressDialog As frmTestProgress') { throw 'frmTest must own the progress dialog.' }
if ($frmTestText -notmatch 'testProgressDialog\.Show\(Me\)') { throw 'frmTest must show the progress dialog with frmTest as owner.' }
if ($frmTestText -match 'grpTestProgress|lblTestProgressStage|progressTest\.Value') { throw 'The old left-sidebar progress card must be removed from frmTest.' }

Write-Output 'Modal test progress contract: PASS'
