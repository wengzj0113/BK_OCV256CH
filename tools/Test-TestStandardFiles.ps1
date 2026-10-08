$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$utf8 = New-Object System.Text.UTF8Encoding($false, $true)
$designer = [System.IO.File]::ReadAllText((Join-Path $root 'frmInit.Designer.vb'), $utf8)
$form = [System.IO.File]::ReadAllText((Join-Path $root 'frmInit.vb'), $utf8)
$project = Get-Content -Raw -LiteralPath (Join-Path $root 'MP-60A-RV-64Mem.vbproj')

foreach ($controlName in @('btnImportTestStandard', 'btnSaveTestStandard')) {
    if ($designer -notmatch "Friend WithEvents $controlName As (System\.Windows\.Forms\.)?Button") {
        throw "$controlName must be declared as a WinForms Designer button."
    }
    if ($designer -notmatch "GroupBox6\.Controls\.Add\(Me\.$controlName\)") {
        throw "$controlName must be placed in the existing parameter range group."
    }
    if ($form -notmatch "Handles $controlName\.Click") {
        throw "$controlName must have a click handler in frmInit.vb."
    }
}

if ($designer -notmatch ('Me\.btnImportTestStandard\.Text = "\u5BFC\u5165\u6807\u51C6"') -or
    $designer -notmatch ('Me\.btnSaveTestStandard\.Text = "\u4FDD\u5B58\u6807\u51C6"')) {
    throw 'The Designer buttons must display the agreed Chinese labels.'
}
if ($project -notmatch '<Compile Include="TestStandardProfile\.vb"') {
    throw 'TestStandardProfile.vb must be included in the application project.'
}
if ($form -match 'btnImportTestStandard\s*=\s*New\s+Button\(' -or
    $form -match 'btnSaveTestStandard\s*=\s*New\s+Button\(') {
    throw 'The test standard buttons must remain authored in frmInit.Designer.vb.'
}

$importHandler = [regex]::Match($form, '(?s)Private Sub btnImportTestStandard_Click.*?End Sub').Value
$saveHandler = [regex]::Match($form, '(?s)Private Sub btnSaveTestStandard_Click.*?End Sub').Value
if (-not $importHandler -or -not $saveHandler) {
    throw 'Both standard-file click handlers must be implemented.'
}
if ($importHandler -notmatch 'testStandardFileStore\.Read\(' -or
    $importHandler -notmatch 'txtRes1\.Text' -or $importHandler -notmatch 'txtRes2\.Text' -or
    $importHandler -notmatch 'txtVol1\.Text' -or $importHandler -notmatch 'txtVol2\.Text') {
    throw 'Import must read a profile and populate all four range controls.'
}
if ($saveHandler -notmatch 'testStandardFileStore\.Save\(' -or
    $saveHandler -notmatch 'TryCreateTestStandardProfile\(profile\)') {
    throw 'Save must validate the current controls and write through the profile store.'
}
$profileBuilder = [regex]::Match($form, '(?s)Private Function TryCreateTestStandardProfile.*?End Function').Value
if (-not $profileBuilder -or
    $profileBuilder -notmatch 'txtRes1\.Text' -or $profileBuilder -notmatch 'txtRes2\.Text' -or
    $profileBuilder -notmatch 'txtVol1\.Text' -or $profileBuilder -notmatch 'txtVol2\.Text') {
    throw 'The saved profile must be built from all four range controls.'
}
foreach ($handler in @($importHandler, $saveHandler)) {
    if ($handler -match 'SaveInitFile\(|Initsetting\s*=') {
        throw 'Import and save actions must not bypass the existing Apply/Cancel boundary.'
    }
}

$okHandler = [regex]::Match($form, '(?s)Private Sub CmdOK_Click.*?End Sub').Value
$cancelHandler = [regex]::Match($form, '(?s)Private Sub Button1_Click.*?End Sub').Value
if ($okHandler -notmatch 'TextToStruct\(\)' -or $okHandler -notmatch 'SaveInitFile\(\)' -or
    $cancelHandler -notmatch 'Me\.Close\(\)') {
    throw 'The existing OK and Cancel commit paths must remain intact.'
}

Write-Output 'Test standard file UI verification passed.'
