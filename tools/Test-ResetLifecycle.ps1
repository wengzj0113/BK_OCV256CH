$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$client = Get-Content -LiteralPath (Join-Path $root 'SerialMiddleMachineClient.vb') -Raw
$runner = Get-Content -LiteralPath (Join-Path $root 'FormalTestRunner.vb') -Raw
$module = Get-Content -LiteralPath (Join-Path $root 'Module1.vb') -Raw
$mdi = Get-Content -LiteralPath (Join-Path $root 'MDIForm.vb') -Raw
$calibration = Get-Content -LiteralPath (Join-Path $root 'ChannelCalibrationRunner.vb') -Raw

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

Write-Output 'PASS: reset lifecycle and middle-machine recovery hooks are present.'
