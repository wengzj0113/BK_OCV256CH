$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$mdi = Get-Content -Raw -LiteralPath (Join-Path $root 'MDIForm.vb')
$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$frmTest = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')
$responsive = Get-Content -Raw -LiteralPath (Join-Path $root 'ResponsiveTestLayout.vb')
$grid = Get-Content -Raw -LiteralPath (Join-Path $root 'ChannelGridControl.vb')
$dataService = Get-Content -Raw -LiteralPath (Join-Path $root 'OcvDataService.vb')
$deviceService = Get-Content -Raw -LiteralPath (Join-Path $root 'StartupDeviceService.vb')

function Assert-Contains([string]$source, [string]$pattern, [string]$message) {
    if ($source -notmatch $pattern) { throw $message }
}

function Assert-NotContains([string]$source, [string]$pattern, [string]$message) {
    if ($source -match $pattern) { throw $message }
}

Assert-Contains $mdi 'Private Const ResizeDebounceMs\s+As Integer\s*=\s*75' 'MDI resize debounce must be 75 ms.'
Assert-Contains $mdi 'ResizeDebounceTimer_Tick' 'MDI must process resize through a debounce timer.'
Assert-NotContains $mdi 'child\.WindowState\s*=\s*FormWindowState\.Normal[\s\S]*?child\.WindowState\s*=\s*FormWindowState\.Maximized' 'Resize path must not force Normal -> Maximized.'
Assert-NotContains $mdi 'SyncMaximizedMdiChildren' 'The old maximize-toggle synchronization method must be removed.'

Assert-NotContains $frmTest 'Private Sub FitToMdiClient' 'frmTest must not keep the forced ClientSize synchronization method.'
Assert-NotContains $frmTest 'Handles Me\.Resize[\s\S]*?FitToMdiClient' 'frmTest Resize must not force ClientSize through FitToMdiClient.'
Assert-Contains $frmTest 'If Pic_Bat IsNot Nothing AndAlso Pic_Bat\.Visible Then[\s\S]*?FreshBat\(' 'Legacy bitmap refresh must be skipped while Pic_Bat is hidden.'
Assert-Contains $frmTest 'ThreadPool\.QueueUserWorkItem\([\s\S]*?GetCN\(\)' 'Grade-field loading must happen in the background.'
Assert-NotContains $frmTest '判断数据库的连接状态\s*\r?\n\s*Call GetCN\(\)' 'frmTest Load must not synchronously call GetCN.'

$checkResultsMethod = [regex]::Match($module, 'Private Sub ApplyCheckResultsToUi\(.*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($checkResultsMethod)) { throw 'ApplyCheckResultsToUi method body not found.' }
Assert-NotContains $checkResultsMethod 'ConnectFor34461\s*\(' 'Device result UI callback must not reconnect the 34461 on the UI thread.'
Assert-NotContains $checkResultsMethod 'ConnectFor3561\s*\(' 'Device result UI callback must not reconnect the 3561 on the UI thread.'
Assert-NotContains $checkResultsMethod 'MessageBox\.Show|MsgBox\s*\(' 'Startup device result callback must not show a modal dialog.'

Assert-Contains $module 'StartupDeviceService\.StartAsync' 'Startup device checks must be owned by a background device service/task.'
Assert-Contains $deviceService 'Task\.Run' 'Startup device checks must be queued away from the UI thread.'
Assert-Contains $deviceService 'DeviceStartupCheck' 'The background service must expose the startup device check operation.'

$loadMethod = [regex]::Match($frmTest, 'Private Sub frmTest_Load\(.*?\n    End Sub', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
if ([string]::IsNullOrWhiteSpace($loadMethod)) { throw 'frmTest_Load method body not found.' }
Assert-NotContains $loadMethod 'timer3\.Start\(\)' 'timer3 must not start before the first page paint.'
Assert-NotContains $loadMethod 'Timer4\.Start\(\)' 'Timer4 must not start before the first page paint.'
Assert-Contains $frmTest 'StartDeferredStartupWork' 'frmTest must schedule timers and non-critical startup work after first paint.'

Assert-Contains $frmTest 'ApplyFrmTestEssentialLayout' 'frmTest must have a separate essential first-paint layout phase.'
Assert-Contains $frmTest 'ApplyFrmTestDeferredVisualStyle' 'frmTest must have a deferred visual-style phase.'
Assert-NotContains $frmTest 'ScheduleDeferredStructuredLayout\(\)' 'Startup must not schedule repeated sidebar layout batches after first paint.'
Assert-NotContains $frmTest 'deferredStructuredLayoutTimer' 'Startup must not keep a timer that repaints sidebar containers one by one.'
Assert-Contains $frmTest 'ApplyFrmTestDeferredVisualStyle\(\)' 'Post-first-paint work must remain limited to visual styling and background services.'
Assert-Contains $frmTest 'StartupTiming' 'Startup phases must use the shared timing helper.'
Assert-Contains $mdi 'StartupTiming' 'MDI startup must record timing phases.'
Assert-Contains $module 'StartupTiming' 'Device startup checks must record timing phases.'
Assert-NotContains $mdi 'mdiClient\.SetBounds\(' 'MDI client layout must not cache a manual client rectangle.'
Assert-Contains $mdi 'mdiClient\.Dock\s*=\s*DockStyle\.Fill' 'MDI client must naturally fill the space left by the menu and status bar.'

Assert-Contains $responsive 'host\.ResumeLayout\(False\)' 'Responsive startup layout must resume without an immediate recursive layout.'
Assert-NotContains $responsive 'host\.ResumeLayout\(True\)' 'Responsive startup layout must not trigger an extra automatic layout pass.'

Assert-Contains $grid 'Public Sub BeginBatchUpdate\(\)' 'Channel grid must expose batch update start.'
Assert-Contains $grid 'Public Sub EndBatchUpdate\(\)' 'Channel grid must expose batch update end.'
Assert-Contains $grid 'gridSurface\.Visible\s*=\s*False' 'Batch update must suppress intermediate grid paints.'
Assert-Contains $grid 'gridSurface\.Visible\s*=\s*batchGridWasVisible' 'Batch update must restore grid visibility before the final repaint.'
Assert-Contains $grid 'Public Sub UpdateAll[\s\S]*?BeginBatchUpdate\(\)[\s\S]*?Finally[\s\S]*?EndBatchUpdate\(\)' 'UpdateAll must update all tiles inside one batch.'
Assert-Contains $grid 'EndBatchUpdate\(\)[\s\S]*?gridSurface\.Invalidate\(\)' 'Batch update must invalidate the grid once at the end.'

$selectableFieldMethod = [regex]::Match($dataService, 'Public Function GetGradeSelectableFields\(\).*?\n    End Function', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
Assert-Contains $selectableFieldMethod 'command\.CommandTimeout\s*=\s*3' 'Grade selectable-field query timeout must be 3 seconds.'

Write-Output 'PASS: startup, resize debounce, async initialization, and channel-grid batching hooks are present.'
