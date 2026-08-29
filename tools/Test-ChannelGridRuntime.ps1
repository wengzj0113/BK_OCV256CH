$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$gridType = $assembly.GetType('software.ChannelGridControl', $true)
foreach ($size in @(@(876, 774), @(524, 555))) {
    $grid = [Activator]::CreateInstance($gridType)
    $grid.Size = New-Object System.Drawing.Size($size[0], $size[1])
    $grid.CreateControl()
    $grid.PerformLayout()

    $gridSurface = $null
    $pending = New-Object System.Collections.Queue
    foreach ($child in $grid.Controls) { $pending.Enqueue($child) }
    while ($pending.Count -gt 0 -and $null -eq $gridSurface) {
        $candidate = $pending.Dequeue()
        if ($candidate.GetType().Name -eq 'ChannelGridSurface') { $gridSurface = $candidate; break }
        foreach ($nested in $candidate.Controls) { $pending.Enqueue($nested) }
    }
    if ($null -eq $gridSurface) { throw "Virtual channel surface was not found at $($size[0])x$($size[1])." }
    if ($gridSurface.Dock -ne [System.Windows.Forms.DockStyle]::Fill) { throw 'Virtual channel surface must fill its host cell.' }
    if ($gridSurface.ClientSize.Width -le 0 -or $gridSurface.ClientSize.Height -le 0) { throw 'Virtual channel surface has no usable size.' }
    $outer = $grid.Controls | Where-Object { $_ -is [System.Windows.Forms.TableLayoutPanel] } | Select-Object -First 1
    $headerHeight = $outer.GetRowHeights()[0] + $outer.GetRowHeights()[1]
    if ($headerHeight -gt [Math]::Max(80, [int]($grid.ClientSize.Height * 0.20))) {
        throw "Matrix headers consume too much vertical space at $($size[0])x$($size[1]): height=$headerHeight."
    }
    Write-Output "PASS: $($size[0])x$($size[1]); virtual surface=$($gridSurface.ClientSize.Width)x$($gridSurface.ClientSize.Height)."
}
