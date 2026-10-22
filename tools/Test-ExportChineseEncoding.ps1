$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$manualTestSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'FrmManualTest.vb'), [System.Text.Encoding]::UTF8)
$calibrationSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'FrmChannelCalibration.vb'), [System.Text.Encoding]::UTF8)
$logExportSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'FrmLogQuery.vb'), [System.Text.Encoding]::UTF8)
$workbookSource = [System.IO.File]::ReadAllText((Join-Path $repoRoot 'ManualTestWorkbookWriter.vb'), [System.Text.Encoding]::UTF8)

if ($manualTestSource -notmatch 'File\.WriteAllText\(outputPath, csv\.ToString\(\), New System\.Text\.UTF8Encoding\(True\)\)') {
    throw 'Manual test CSV export must write UTF-8 with a BOM.'
}
if ($calibrationSource -notmatch 'File\.WriteAllText\(outputPath, csv\.ToString\(\), New UTF8Encoding\(True\)\)') {
    throw 'Calibration CSV export must write UTF-8 with a BOM.'
}
if ($logExportSource -notmatch 'File\.WriteAllText\(dialog\.FileName, BusinessLogService\.ToCsv\(currentEntries\), New UTF8Encoding\(True\)\)') {
    throw 'Log CSV export must write UTF-8 with a BOM.'
}
foreach ($header in @('采集时间', '内阻 mΩ', '统计轮数', '电压偏离度 mV')) {
    if ($workbookSource -notmatch [regex]::Escape($header)) {
        throw "Manual test workbook is missing expected Chinese header: $header"
    }
}

$encoding = New-Object System.Text.UTF8Encoding($true, $true)
$sample = '通道,分组,判定,备注' + "`r`n" + 'CH1,G2 本地CH1,通过,"维修,复测"' + "`r`n"
$tempPath = Join-Path ([System.IO.Path]::GetTempPath()) ('export-encoding-' + [guid]::NewGuid().ToString('N') + '.csv')
try {
    [System.IO.File]::WriteAllText($tempPath, $sample, $encoding)
    $bytes = [System.IO.File]::ReadAllBytes($tempPath)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
        throw 'UTF-8 BOM is missing from exported CSV sample.'
    }
    $roundTrip = [System.IO.File]::ReadAllText($tempPath, $encoding)
    if (-not $roundTrip.StartsWith($sample, [System.StringComparison]::Ordinal)) {
        throw 'Chinese CSV sample did not survive strict UTF-8 round trip.'
    }
}
finally {
    if ([System.IO.File]::Exists($tempPath)) { [System.IO.File]::Delete($tempPath) }
}

Write-Output 'Chinese export encoding verification passed.'
