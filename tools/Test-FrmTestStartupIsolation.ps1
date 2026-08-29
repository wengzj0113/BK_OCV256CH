$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\frmTest.vb')
$load = [regex]::Match($source, 'Private Sub frmTest_Load\(.*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($load)) { throw 'frmTest_Load method body not found.' }
if ($load -match 'frmMDI\.txtTesting\.Text\s*=') {
    throw 'frmTest_Load must not write to the MDI parent during initial child layout.'
}
Write-Output 'PASS: frmTest_Load does not trigger parent-form layout through txtTesting.'
