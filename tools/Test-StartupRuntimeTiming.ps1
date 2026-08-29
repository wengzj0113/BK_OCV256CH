$ErrorActionPreference = 'Stop'

$exePath = if ($env:STARTUP_EXE) { (Resolve-Path -LiteralPath $env:STARTUP_EXE).Path } else { throw 'STARTUP_EXE must point to an isolated x86 build.' }
$logPath = Join-Path $env:LOCALAPPDATA ('XK-OCV-256CH\logFile\Startup_' + (Get-Date -Format 'yyyyMMdd') + '.log')
$before = if (Test-Path -LiteralPath $logPath) { (Get-Content -LiteralPath $logPath).Count } else { 0 }
$workingDirectory = Split-Path -Parent $exePath
$process = Start-Process -FilePath $exePath -WorkingDirectory $workingDirectory -WindowStyle Hidden -PassThru
try {
    $deadline = [DateTime]::UtcNow.AddSeconds(35)
    $deferredCompleteSeen = $false
    do {
        Start-Sleep -Milliseconds 250
        if (Test-Path -LiteralPath $logPath) {
            $lines = @(Get-Content -LiteralPath $logPath)
            $newLines = if ($lines.Count -gt $before) { $lines[$before..($lines.Count - 1)] } else { @() }
            $deferredCompleteSeen = $newLines -match 'frmTest-deferred-complete'
        }
    } while (-not $deferredCompleteSeen -and [DateTime]::UtcNow -lt $deadline -and -not $process.HasExited)

    if (-not $deferredCompleteSeen) { throw 'frmTest-deferred-complete was not recorded within 35 seconds.' }
    $lines = @(Get-Content -LiteralPath $logPath)
    if ($lines.Count -gt $before) {
        $newLines = @($lines[$before..($lines.Count - 1)] | Where-Object { $_ -match '\[STARTUP\]' })
        $newLines | ForEach-Object { Write-Output $_ }
        $firstPaintIndex = [Array]::IndexOf($newLines, ($newLines | Where-Object { $_ -match 'frmTest-first-paint' } | Select-Object -First 1))
        $deviceStartIndex = [Array]::IndexOf($newLines, ($newLines | Where-Object { $_ -match 'device-check-start' } | Select-Object -First 1))
        if ($firstPaintIndex -lt 0 -or $deviceStartIndex -lt 0) { throw 'Startup timing must record both first paint and deferred device-check start.' }
        if ($deviceStartIndex -lt $firstPaintIndex) { throw 'Device checks started before the first page paint.' }
    }
    Write-Output 'PASS: real executable recorded first-paint and deferred device-start timing.'
}
finally {
    if (-not $process.HasExited) {
        $process.CloseMainWindow() | Out-Null
        if (-not $process.WaitForExit(5000)) { Stop-Process -Id $process.Id -Force }
    }
}
