$ErrorActionPreference = 'Stop'

$exePath = if ($env:STARTUP_EXE) { (Resolve-Path -LiteralPath $env:STARTUP_EXE).Path } else { throw 'STARTUP_EXE must point to an isolated x86 build.' }
$logPath = Join-Path $env:LOCALAPPDATA ('XK-OCV-256CH\logFile\Startup_' + (Get-Date -Format 'yyyyMMdd') + '.log')
$before = if (Test-Path -LiteralPath $logPath) { (Get-Content -LiteralPath $logPath).Count } else { 0 }
$workingDirectory = Split-Path -Parent $exePath
$process = Start-Process -FilePath $exePath -WorkingDirectory $workingDirectory -WindowStyle Hidden -PassThru
try {
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    $deferredCompleteSeen = $false
    do {
        Start-Sleep -Milliseconds 250
        if (Test-Path -LiteralPath $logPath) {
            $lines = @(Get-Content -LiteralPath $logPath)
            $newLines = if ($lines.Count -gt $before) { @($lines[$before..($lines.Count - 1)]) } else { @() }
            $deferredCompleteSeen = @($newLines | Where-Object { $_ -match 'frmTest-deferred-complete' }).Count -gt 0
        }
    } while (-not $deferredCompleteSeen -and [DateTime]::UtcNow -lt $deadline -and -not $process.HasExited)

    if (-not $deferredCompleteSeen) { throw 'The deferred startup completion marker was not recorded within 30 seconds.' }
    Start-Sleep -Milliseconds 1500

    $lines = if (Test-Path -LiteralPath $logPath) { @(Get-Content -LiteralPath $logPath) } else { @() }
    $newLines = if ($lines.Count -gt $before) { @($lines[$before..($lines.Count - 1)] | Where-Object { $_ -match '\[STARTUP\]' }) } else { @() }
    $newLines | ForEach-Object { Write-Output $_ }

    $stepCount = @($newLines | Where-Object { $_ -match 'frmTest-structured-layout-step-start' }).Count
    if ($stepCount -gt 0) {
        throw "Startup still performs $stepCount post-first-paint sidebar layout batches; this is the reported one-container-at-a-time refresh." 
    }
    Write-Output 'PASS: startup has no post-first-paint sidebar layout batches.'
}
finally {
    if (-not $process.HasExited) {
        $process.CloseMainWindow() | Out-Null
        if (-not $process.WaitForExit(5000)) { Stop-Process -Id $process.Id -Force }
    }
}
