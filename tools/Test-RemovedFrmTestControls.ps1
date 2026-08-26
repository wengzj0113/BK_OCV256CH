param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$frmTest = Get-Content -LiteralPath (Join-Path $Root 'frmTest.vb') -Raw
$mdiForm = Get-Content -LiteralPath (Join-Path $Root 'MDIForm.vb') -Raw

foreach ($name in @('lblPath', 'probeProgress', 'txtInfo')) {
    if ($frmTest -match "\b$name\b" -or $mdiForm -match "\bfrmTest\.$name\b") {
        throw "Removed frmTest control '$name' is still referenced by runtime code."
    }
}

Write-Host 'PASS: runtime code no longer depends on manually removed frmTest controls.'
