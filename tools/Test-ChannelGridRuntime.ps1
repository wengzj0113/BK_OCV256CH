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

    $gridLayout = $null
    foreach ($child in $grid.Controls) {
        if ($child -is [System.Windows.Forms.TableLayoutPanel]) {
            foreach ($nested in $child.Controls) {
                if ($nested -is [System.Windows.Forms.TableLayoutPanel] -and $nested.RowCount -eq 16 -and $nested.ColumnCount -eq 16) {
                    $gridLayout = $nested
                    break
                }
            }
        }
    }
    if ($null -eq $gridLayout) { throw "16x16 runtime grid was not found at $($size[0])x$($size[1])." }

    $lastTile = $gridLayout.GetControlFromPosition(15, 15)
    if ($null -eq $lastTile) { throw "Last matrix tile was not created at $($size[0])x$($size[1])." }
    $lastBottom = $lastTile.Bottom + $gridLayout.Top
    if ($lastBottom -gt $grid.ClientSize.Height) {
        throw "Last matrix row is clipped at $($size[0])x$($size[1]): bottom=$lastBottom, viewport=$($grid.ClientSize.Height)."
    }
    $firstTile = $gridLayout.GetControlFromPosition(0, 0)
    $firstTileLeft = $firstTile.Left + $gridLayout.Left
    $maxFirstTileLeft = [Math]::Max(60, [int]($grid.ClientSize.Width * 0.10))
    if ($firstTileLeft -gt $maxFirstTileLeft) {
        throw "First matrix tile is too far from the upper-left corner at $($size[0])x$($size[1]): left=$firstTileLeft, limit=$maxFirstTileLeft."
    }
    $outer = $grid.Controls | Where-Object { $_ -is [System.Windows.Forms.TableLayoutPanel] } | Select-Object -First 1
    $headerHeight = $outer.GetRowHeights()[0] + $outer.GetRowHeights()[1]
    if ($headerHeight -gt [Math]::Max(80, [int]($grid.ClientSize.Height * 0.20))) {
        throw "Matrix headers consume too much vertical space at $($size[0])x$($size[1]): height=$headerHeight."
    }
    Write-Output "PASS: $($size[0])x$($size[1]); last row bottom=$lastBottom, viewport=$($grid.ClientSize.Height)."
}
