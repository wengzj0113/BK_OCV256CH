$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\MDIForm.vb')
$method = [regex]::Match($source, 'Private Sub OnLineTest_Click\(.*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($method)) { throw 'OnLineTest_Click method body not found.' }

$layoutIndex = $method.IndexOf('ApplyMdiClientLayout()')
$showIndex = $method.IndexOf('obj_frmTest.Show()')
$maximizeIndex = $method.IndexOf('obj_frmTest.WindowState = FormWindowState.Maximized')
if ($layoutIndex -lt 0) { throw 'Initial test-page creation must apply the MDI client layout first.' }
if ($maximizeIndex -lt 0) { throw 'Initial test page maximize call is missing.' }
if ($showIndex -lt 0) { throw 'Initial test page show call is missing.' }
if ($showIndex -gt $maximizeIndex) { throw 'Initial test page must be maximized immediately after it is shown.' }
if ($layoutIndex -gt $showIndex) { throw 'MDI client layout must be applied before the child is shown.' }

Write-Output 'PASS: initial MDI child sizing applies the client layout before the first maximize.'
