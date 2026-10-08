param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
$resolvedAssemblyPath = (Resolve-Path -LiteralPath $AssemblyPath).Path
$assembly = [System.Reflection.Assembly]::LoadFrom($resolvedAssemblyPath)
$dialogType = $assembly.GetType('software.StartupDeviceFailureDialog', $false)
if ($null -eq $dialogType) {
    throw 'software.StartupDeviceFailureDialog is not present in the application assembly.'
}

$dialog = [Activator]::CreateInstance($dialogType, @('Test message'))
if (-not $dialog.TopMost) {
    throw 'The startup device failure dialog must be topmost.'
}
$expectedTitle = [string]::Concat([char]0x8bbe, [char]0x5907, [char]0x8fde, [char]0x63a5, [char]0x68c0, [char]0x67e5)
if ($dialog.Text -ne $expectedTitle) {
    throw 'The startup device failure dialog title is incorrect.'
}

$watchdog = New-Object System.Windows.Forms.Timer
$watchdog.Interval = 8000
$watchdog.Add_Tick([System.EventHandler]{
    $watchdog.Stop()
    $dialog.Close()
})
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$watchdog.Start()
try {
    [void]$dialog.ShowDialog()
} finally {
    $watchdog.Stop()
    $watchdog.Dispose()
    $stopwatch.Stop()
    $dialog.Dispose()
}

if ($stopwatch.ElapsedMilliseconds -lt 4500 -or $stopwatch.ElapsedMilliseconds -gt 7000) {
    throw "The startup device failure dialog did not auto-close near five seconds (elapsed $($stopwatch.ElapsedMilliseconds) ms)."
}

Write-Output 'Startup device failure dialog verification passed.'
