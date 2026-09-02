$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$mdi = Get-Content -Raw -LiteralPath (Join-Path $root 'MDIForm.vb')
$module = Get-Content -Raw -LiteralPath (Join-Path $root 'Module1.vb')
$frmTest = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.vb')
$designer = Get-Content -Raw -LiteralPath (Join-Path $root 'frmTest.Designer.vb')
$grid = Get-Content -Raw -LiteralPath (Join-Path $root 'ChannelGridControl.vb')
$dataService = Get-Content -Raw -LiteralPath (Join-Path $root 'OcvDataService.vb')
$deviceService = Get-Content -Raw -LiteralPath (Join-Path $root 'StartupDeviceService.vb')

function Assert-Contains([string]$source, [string]$pattern, [string]$message) { if ($source -notmatch $pattern) { throw $message } }
function Assert-NotContains([string]$source, [string]$pattern, [string]$message) { if ($source -match $pattern) { throw $message } }

Assert-Contains $mdi 'Private Const ResizeDebounceMs\s+As Integer\s*=\s*75' 'MDI resize debounce must be 75 ms.'
Assert-Contains $mdi 'ResizeDebounceTimer_Tick' 'MDI must process resize through a debounce timer.'
Assert-NotContains $mdi 'child\.WindowState\s*=\s*FormWindowState\.Normal[\s\S]*?child\.WindowState\s*=\s*FormWindowState\.Maximized' 'Resize path must not force Normal -> Maximized.'
Assert-NotContains $mdi 'SyncMaximizedMdiChildren' 'The old maximize-toggle synchronization method must be removed.'
Assert-NotContains $frmTest 'Private Sub FitToMdiClient' 'frmTest must not keep forced client-size synchronization.'
Assert-Contains $frmTest 'StartDeferredStartupWork' 'frmTest must defer non-critical startup work until after first paint.'
Assert-Contains $frmTest 'ThreadPool\.QueueUserWorkItem[\s\S]*?GetCN\(\)' 'Grade-field loading must happen in the background.'
Assert-NotContains $frmTest '判断数据库的连接状态\s*\r?\n\s*Call GetCN\(\)' 'frmTest Load must not synchronously call GetCN.'
Assert-Contains $module 'StartupDeviceService\.StartAsync' 'Startup device checks must use the background service.'
Assert-Contains $deviceService 'Task\.Run' 'Startup device checks must be queued away from the UI thread.'
Assert-Contains $deviceService 'DeviceStartupCheck' 'Startup device check operation is missing.'
Assert-Contains $mdi 'mdiClient\.Dock\s*=\s*DockStyle\.Fill' 'MDI client must naturally fill the area below menu/status.'
Assert-NotContains $mdi 'mdiClient\.SetBounds\(' 'MDI client must not cache manual bounds.'
Assert-Contains $designer 'Me\.Controls\.Add\(Me\.rootLayout\)' 'frmTest must mount its serialized root layout once.'
Assert-NotContains $designer 'BuildDesignerLayout|BuildDesignerTestModeLayout' 'Designer must not reconstruct layout through helper calls.'
Assert-Contains $grid 'Public Sub BeginBatchUpdate\(\)' 'Channel grid must expose batch update start.'
Assert-Contains $grid 'Public Sub EndBatchUpdate\(\)' 'Channel grid must expose batch update end.'
Assert-Contains $grid 'gridSurface\.Visible\s*=\s*False' 'Batch update must suppress intermediate grid paints.'
Assert-Contains $grid 'gridSurface\.Visible\s*=\s*batchGridWasVisible' 'Batch update must restore grid visibility.'
Assert-Contains $grid 'Public Sub UpdateAll[\s\S]*?BeginBatchUpdate\(\)[\s\S]*?Finally[\s\S]*?EndBatchUpdate\(\)' 'UpdateAll must update inside one batch.'
Assert-Contains $dataService 'command\.CommandTimeout\s*=\s*3' 'Grade selectable-field query timeout must be 3 seconds.'
Write-Output 'PASS: startup deferral, resize debounce, serialized root layout and channel-grid batching hooks are present.'
