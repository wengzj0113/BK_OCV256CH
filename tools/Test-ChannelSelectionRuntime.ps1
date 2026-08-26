$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$gridType = $assembly.GetType('software.ChannelGridControl', $true)
$dataType = $assembly.GetType('software.ChannelDisplayData', $true)
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
$grid.SelectionMode = [System.Enum]::Parse($assembly.GetType('software.ChannelSelectionMode'), 'SingleChannel')
$fields = $gridType.GetField('tileControls', [Reflection.BindingFlags] 'Instance,NonPublic')
$tiles = $fields.GetValue($grid)
$tile = $tiles[1]
$handler = $tile.GetType().GetMethod('HandleSelected', [Reflection.BindingFlags] 'Instance,NonPublic')
$handler.Invoke($tile, @($tile, [EventArgs]::Empty)) | Out-Null
if ($grid.SelectedChannelCount -ne 1 -or $grid.SelectedChannelNumbers[0] -ne 1) { throw 'Single channel selection did not select channel 1.' }
$grid.ClearSelection()
$labelClick = $tile.Controls[0].GetType().GetMethod('OnClick', [Reflection.BindingFlags] 'Instance,NonPublic')
$labelClick.Invoke($tile.Controls[0], @([EventArgs]::Empty)) | Out-Null
if ($grid.SelectedChannelCount -ne 1 -or $grid.SelectedChannelNumbers[0] -ne 1) { throw 'Clicking the inside of the channel tile did not select channel 1.' }
$grid.ClearSelection()
$mouseUp = $tile.GetType().GetMethod('HandleTileMouseUp', [Reflection.BindingFlags] 'Instance,NonPublic')
$mouseArgs = [System.Windows.Forms.MouseEventArgs]::new([System.Windows.Forms.MouseButtons]::Left, 1, 10, 10, 0)
$mouseUp.Invoke($tile, @($tile.Controls[0], $mouseArgs)) | Out-Null
if ($grid.SelectedChannelCount -ne 1 -or $grid.SelectedChannelNumbers[0] -ne 1) { throw 'Mouse release inside the channel tile did not select channel 1.' }
$tile.Size = New-Object System.Drawing.Size(60, 40)
$tile.CreateControl()
$tile.PerformLayout()
$label = $tile.Controls[0]
if (-not $label.IsSelected) { throw 'Selected tile did not propagate selection to the visible label.' }

Write-Host 'PASS: runtime select-all, clear, and single-channel selection behavior verified.'
