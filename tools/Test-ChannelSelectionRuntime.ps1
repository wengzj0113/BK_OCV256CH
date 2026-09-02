$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$uiAssemblyPath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'XL_OCV_UIControls.dll'
$uiAssembly = [Reflection.Assembly]::LoadFrom($uiAssemblyPath)
$gridType = $uiAssembly.GetType('software.ChannelGridControl', $true)
$dataType = $uiAssembly.GetType('software.ChannelDisplayData', $true)
$grid = [Activator]::CreateInstance($gridType)
$grid.CreateControl()

$grid.SelectAll()
if ($grid.SelectedChannelCount -ne 256) { throw "SelectAll returned $($grid.SelectedChannelCount), expected 256." }
$grid.ClearSelection()
if ($grid.SelectedChannelCount -ne 0) { throw "ClearSelection returned $($grid.SelectedChannelCount), expected 0." }

$data = [Activator]::CreateInstance($dataType)
$data.ChannelNo = 1
$data.Row = 1
$data.Column = 1
$data.StatusText = 'IDLE'
$grid.UpdateChannel(1, $data)
$grid.SelectionMode = [System.Enum]::Parse($uiAssembly.GetType('software.ChannelSelectionMode'), 'SingleChannel')
$surfaceField = $gridType.GetField('gridSurface', [Reflection.BindingFlags] 'Instance,NonPublic')
$surface = $surfaceField.GetValue($grid)
$grid.Size = New-Object System.Drawing.Size(640, 480)
$surface.CreateControl()
$grid.PerformLayout()
$mouseDown = $gridType.GetMethod('HandleGridMouseDown', [Reflection.BindingFlags] 'Instance,NonPublic')
$mouseDown.Invoke($grid, @($surface, [System.Windows.Forms.MouseEventArgs]::new([System.Windows.Forms.MouseButtons]::Left, 1, 10, 10, 0))) | Out-Null
if ($grid.SelectedChannelCount -ne 1 -or $grid.SelectedChannelNumbers[0] -ne 1) { throw 'Single channel selection did not select channel 1.' }
$grid.ClearSelection()
$mouseDown.Invoke($grid, @($surface, [System.Windows.Forms.MouseEventArgs]::new([System.Windows.Forms.MouseButtons]::Left, 1, 10, 10, 0))) | Out-Null
if ($grid.SelectedChannelCount -ne 1 -or $grid.SelectedChannelNumbers[0] -ne 1) { throw 'Clicking the inside of the channel tile did not select channel 1.' }
$grid.ClearSelection()
$selectionMode = [System.Enum]::Parse($uiAssembly.GetType('software.ChannelSelectionMode'), 'Rectangle')
$grid.SelectionMode = $selectionMode
$mouseDownSurface = $gridType.GetMethod('HandleGridMouseDown', [Reflection.BindingFlags] 'Instance,NonPublic')
$mouseUpSurface = $gridType.GetMethod('HandleGridMouseUp', [Reflection.BindingFlags] 'Instance,NonPublic')
$mouseDownSurface.Invoke($grid, @($surface, [System.Windows.Forms.MouseEventArgs]::new([System.Windows.Forms.MouseButtons]::Left, 1, 1, 1, 0))) | Out-Null
$mouseUpSurface.Invoke($grid, @($surface, [System.Windows.Forms.MouseEventArgs]::new([System.Windows.Forms.MouseButtons]::Left, 1, 20, 20, 0))) | Out-Null
if ($grid.SelectedChannelCount -ne 1 -or $grid.SelectedChannelNumbers[0] -ne 1) { throw 'Mouse release inside the channel tile did not select channel 1.' }

Write-Host 'PASS: runtime select-all, clear, single-channel, and rectangle selection behavior verified.'
