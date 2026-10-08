$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$main = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')
$service = Get-Content -Raw -LiteralPath (Join-Path $root 'OcvDataService.vb')
$queryForm = Get-Content -Raw -LiteralPath (Join-Path $root 'FrmInquery.vb')
$queryDesigner = Get-Content -Raw -LiteralPath (Join-Path $root 'FrmInquery.Designer.vb')
$modernSql = Get-Content -Raw -LiteralPath (Join-Path $root 'docs/sql/002_create_core_procedures.sql')
$legacySql = Get-Content -Raw -LiteralPath (Join-Path $root 'OCV_DATA_SQLServer2008R2.sql')

$createTask = [regex]::Match($main, '(?ms)Private Function CreateOcvDataTestTask\(.*?^\s*End Function').Value
if ($createTask -notmatch "activeTrayDebugMode" -or $createTask -notmatch '"Debug"') {
    throw 'OCV_DATA debug tasks must be created with TestType=Debug.'
}
$saveResults = [regex]::Match($main, '(?ms)Private Sub SaveOcvDataTestResults\(.*?^\s*End Sub').Value
if ($saveResults -notmatch 'activeTrayDebugMode' -or $saveResults -notmatch 'ResultSource.*Debug') {
    throw 'OCV_DATA debug results must be stored with ResultSource=Debug.'
}

if ($service -notmatch 'debugMode' -or $queryForm -notmatch 'chkDebugResults' -or
    $queryDesigner -notmatch 'chkDebugResults') {
    throw 'Tray result and date-list queries must expose a default-off debug result selector.'
}

$latestProc = [regex]::Match($modernSql, '(?ms)CREATE OR ALTER PROCEDURE dbo\.usp_GetTrayLatestStageResults.*?^GO\s*$').Value
if ($latestProc -notmatch '@DebugMode' -or
    $latestProc -notmatch "TestType\s*=\s*N'Debug'" -or
    $latestProc -notmatch "TestType\s*<>\s*N'Debug'") {
    throw 'Latest tray results must rank only the selected debug or production task type.'
}
$ngProc = [regex]::Match($modernSql, '(?ms)CREATE OR ALTER PROCEDURE dbo\.usp_GetNgRetestSourceByBatteryCode.*?^GO\s*$').Value
if ($ngProc -notmatch "TestType\s*<>\s*N'Debug'") {
    throw 'Production NG retest source lookup must exclude debug tasks.'
}
if ($legacySql -notmatch "TestType\s*<>\s*N'Debug'" -or
    -not (Test-Path -LiteralPath (Join-Path $root 'docs/sql/012_debug_mode_result_queries_sqlserver2008r2.sql'))) {
    throw 'SQL Server 2008 R2 install and additive migration scripts must include debug query isolation.'
}

Write-Output 'Debug mode SQL/query contract: PASS'
