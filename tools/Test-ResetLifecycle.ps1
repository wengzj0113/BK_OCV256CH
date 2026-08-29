$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$client = Get-Content -LiteralPath (Join-Path $root 'SerialMiddleMachineClient.vb') -Raw
$runner = Get-Content -LiteralPath (Join-Path $root 'FormalTestRunner.vb') -Raw
$module = Get-Content -LiteralPath (Join-Path $root 'Module1.vb') -Raw
$mdi = Get-Content -LiteralPath (Join-Path $root 'MDIForm.vb') -Raw
$calibration = Get-Content -LiteralPath (Join-Path $root 'ChannelCalibrationRunner.vb') -Raw
$frmTest = Get-Content -LiteralPath (Join-Path $root 'frmTest.vb') -Raw
$resetService = Get-Content -LiteralPath (Join-Path $root 'TestEnvironmentResetService.vb') -Raw

if ($client -notmatch 'Public Sub ResetToSafeState\(') { throw 'Missing middle-machine safe reset.' }
if ($client -notmatch 'WriteRetryCount') { throw 'Missing middle-machine write retry policy.' }
if ($client -notmatch 'StopSettleDelayMs') { throw 'Missing STOP settle delay.' }
if ($runner -notmatch 'ResetTestEnvironment\(') { throw 'Formal test does not reset the test environment.' }
if ($module -notmatch 'Public Sub ResetCachedData_3562\(') { throw 'Missing 3562 reset routine.' }
if ($module -notmatch 'Public Sub ResetCachedData_34461\(') { throw 'Missing 34461 reset routine.' }
if ($module -notmatch 'visaSession\.IO\.Timeout = VisaInstrumentIoTimeoutMs') { throw '34461 VISA I/O timeout is not configured.' }
if ($module -notmatch 'VisaInstrumentIoTimeoutMs As Integer = 3000') { throw '34461 VISA I/O timeout is not 3000 ms.' }
if ($module -notmatch 'ResetCachedData_34461\([\s\S]*?visaSession\.IO\.Clear\([\s\S]*?visaSession\.WriteString\("ABOR", True\)') { throw '34461 reset does not clear VISA state before ABOR.' }
if ($mdi -notmatch 'Handles Me\.FormClosing') { throw 'Application exit reset is not attached to FormClosing.' }
if ($calibration -notmatch 'ResetEnvironment\(') { throw 'Calibration path does not reset the test environment.' }
if ($calibration -match 'GetMiddleMachineClient\(\)\.StopTest\(\)') { throw 'Manual calibration stop bypasses the full environment reset.' }
if ($calibration -notmatch 'Public Sub StopCurrentCalibration\(\)[\s\S]*?ResetEnvironment\(\)') { throw 'Manual calibration stop is not connected to the full environment reset.' }

if ($mdi -notmatch 'ApplicationShuttingDown\s*=\s*True') { throw 'Application shutdown flag is not set before exit cleanup.' }
if ($mdi -notmatch 'PrepareForShutdown\(\)') { throw 'frmTest is not prepared before application exit cleanup.' }
if ($mdi -notmatch 'ShutdownResetTimeoutMs\s+As Integer\s*=\s*3000') { throw 'Application exit reset budget is not 3000 ms.' }
if ($mdi -notmatch 'ResetAll\(middleMachine, True, ShutdownResetTimeoutMs\)') { throw 'Application exit reset does not use the bounded reset budget.' }
if ($mdi -notmatch 'If ApplicationShuttingDown Then Return') { throw 'MDI layout callbacks are not blocked during shutdown.' }
if ($frmTest -notmatch 'Public Sub PrepareForShutdown\(\)') { throw 'frmTest has no shutdown preparation method.' }
if ($frmTest -notmatch 'shutdownPrepared\s+OrElse\s+ApplicationShuttingDown') { throw 'frmTest channel refresh/layout is not blocked during shutdown.' }
if ($frmTest -notmatch 'timer3\.Stop\(\)' -or $frmTest -notmatch 'Timer1\.Stop\(\)' -or $frmTest -notmatch 'Timer2\.Stop\(\)' -or $frmTest -notmatch 'Timer4\.Stop\(\)') { throw 'frmTest timers are not stopped before exit cleanup.' }
if ($frmTest -notmatch 'channelGrid\.Visible\s*=\s*False' -or $frmTest -notmatch 'channelGrid\.SuspendLayout\(\)') { throw 'Channel grid is not hidden and suspended before exit cleanup.' }
if ($frmTest -notmatch 'Private Sub timer3_Elapsed[\s\S]*?shutdownPrepared\s+OrElse\s+ApplicationShuttingDown\s+Then Return') { throw 'Background channel/status polling is not blocked during shutdown.' }
if ($resetService -notmatch 'Optional totalTimeoutMs As Integer\s*=\s*30000') { throw 'Reset service has no explicit total timeout contract.' }
if ($resetService -notmatch 'ResetToSafeState\(remainingTimeoutMs\)') { throw 'Reset service does not pass the remaining timeout to the middle machine.' }
if ($module -notmatch 'VisaShutdownTimeoutMs\s+As Integer\s*=\s*1000') { throw 'VISA shutdown timeout is not bounded to 1000 ms.' }
if ($module -notmatch 'ResetCachedData_34461\([\s\S]*?IO\.Timeout\s*=\s*VisaShutdownTimeoutMs') { throw '34461 reset does not force the short shutdown timeout.' }
if ($module -notmatch 'Close34461Session\([\s\S]*?IO\.Timeout\s*=\s*VisaShutdownTimeoutMs') { throw '34461 close does not force the short shutdown timeout.' }
if ($client -notmatch 'Public Sub ResetToSafeState\(Optional totalTimeoutMs As Integer\s*=\s*3000\)') { throw 'Middle machine safe reset has no bounded timeout.' }
if ($client -notmatch 'WriteRegisterForDeadline') { throw 'Middle machine shutdown writes do not honor the deadline.' }

Write-Output 'PASS: reset lifecycle and middle-machine recovery hooks are present.'
