param(
    [string]$Source = (Join-Path (Split-Path -Parent $PSScriptRoot) 'frmTest.vb')
)

$ErrorActionPreference = 'Stop'
$text = Get-Content -LiteralPath $Source -Raw

if ($text -match 'Convert\.ToString\(RecieveData\([^\)]*\)\.Result\)\.Trim\(\)') {
    throw 'Unsafe Result string normalization found: Convert.ToString(...Result).Trim()'
}

if ($text -notmatch 'If\(RecieveData\([^\)]*\)\.Result,\s*String\.Empty\)\.Trim\(\)') {
    throw 'Safe Result string normalization is missing.'
}

Write-Host 'PASS: RecieveData.Result normalization is null-safe.'
