$ErrorActionPreference = 'Stop'

$solutionPath = Join-Path $PSScriptRoot '..\Battery Testing.sln'
$solutionText = Get-Content -LiteralPath $solutionPath -Raw
$projectMatches = [regex]::Matches($solutionText, 'Project\("[^"\r\n]+"\)\s*=\s*"[^"]+",\s*"([^"]+\.vbproj)"')

if ($projectMatches.Count -eq 0) {
    throw 'No VB project reference was found in the solution.'
}

foreach ($match in $projectMatches) {
    $projectPath = Join-Path (Split-Path -Parent $solutionPath) $match.Groups[1].Value
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        throw "Solution references missing project: $projectPath"
    }
}

Write-Output "PASS: $($projectMatches.Count) solution project reference(s) exist."
