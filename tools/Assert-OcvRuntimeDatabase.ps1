param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$projectFile = Join-Path $Root 'MP-60A-RV-64Mem.vbproj'
$excludedPath = '\\(bin|obj|\.vs|packages|docs)\\'
$excludedName = '副本'
$legacyPatterns = @(
    '(?i)DataSource\s*=\s*["'']CWSQL["'']',
    '(?i)Database\s*=\s*["'']?CWSQL',
    '(?i)InitialCatalog\s*=\s*["'']?CWSQL',
    '(?i)CW_tbl(?:Data_Info|Condition|SelFld)',
    '(?i)\b(?:New\s+)?clsDB\b',
    '(?i)\b(?:New\s+)?SqlServerHelper\b',
    '(?i)\bmyConnection\b'
)

$projectText = Get-Content -LiteralPath $projectFile -Raw
$projectFiles = [regex]::Matches($projectText, '<Compile Include="([^"]+)"') |
    ForEach-Object { Join-Path $Root $_.Groups[1].Value } |
    Where-Object { Test-Path -LiteralPath $_ }
$files = $projectFiles | ForEach-Object { Get-Item -LiteralPath $_ } |
    Where-Object { $_.FullName -notmatch $excludedPath -and $_.Name -notmatch $excludedName }

$violations = foreach ($file in $files) {
    $lineNumber = 0
    foreach ($line in (Get-Content -LiteralPath $file.FullName)) {
        $lineNumber++
        if ($line -match '^\s*''') { continue }
        foreach ($pattern in $legacyPatterns) {
            if ($line -match $pattern) {
                [pscustomobject]@{ Path = $file.FullName; LineNumber = $lineNumber; Line = $line.Trim() }
                break
            }
        }
    }
}

$service = Join-Path $Root 'OcvDataService.vb'
if (-not (Test-Path -LiteralPath $service)) {
    throw "OcvDataService.vb not found: $service"
}

$serviceText = Get-Content -LiteralPath $service -Raw
if ($serviceText -notmatch '(?i)DatabaseName\s+As\s+String\s*=\s*["'']OCV_DATA["'']') {
    throw 'OcvDataService.DatabaseName is not fixed to OCV_DATA.'
}

if ($violations) {
    $violations | Format-Table -AutoSize | Out-String | Write-Host
    throw "Found $($violations.Count) legacy CWSQL runtime reference(s)."
}

Write-Host 'PASS: runtime database references are OCV_DATA-only.'
