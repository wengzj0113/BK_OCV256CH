$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
$initializer = $formType.GetMethod('InitializeChannelSelectionControls', [Reflection.BindingFlags] 'Instance,NonPublic')
$panelField = $formType.GetField('_Panel1', [Reflection.BindingFlags] 'Instance,NonPublic')
$labelField = $formType.GetField('_Label5', [Reflection.BindingFlags] 'Instance,NonPublic')
$panel = $panelField.GetValue($form)
$label = $labelField.GetValue($form)
$contentPanel = $panel.Controls | Where-Object { $_.Name -eq 'leftContentPanel' }
if ($null -eq $contentPanel) { throw 'Serialized Designer layout did not contain the left content panel.' }
$selectionGroup = $contentPanel.Controls | Where-Object { $_.Name -eq 'grpChannelSelection' }
if ($null -eq $selectionGroup) { throw 'Serialized Designer layout did not contain the channel selection group.' }
$selectionLayout = $selectionGroup.Controls | Where-Object { $_.Name -eq 'channelSelectionLayout' }
if ($null -eq $selectionLayout) { throw 'Serialized Designer layout did not contain the channel selection layout.' }
if ($selectionLayout.Controls.Count -ne 4) { throw "Selection layout has $($selectionLayout.Controls.Count) controls, expected 4." }
$beforeCount = $contentPanel.Controls.Count
$initializer.Invoke($form, $null) | Out-Null
if ($contentPanel.Controls.Count -ne $beforeCount) { throw 'InitializeChannelSelectionControls rebuilt the serialized left panel.' }
$expectedLabel = -join ([char]0x5f85, [char]0x6d4b, [char]0x8bd5, [char]0x901a, [char]0x9053, [char]0x6570)
if ($label.Text -ne $expectedLabel) { throw 'The left count label was not renamed.' }
$valueField = $formType.GetField('_lblBarNum', [Reflection.BindingFlags] 'Instance,NonPublic')
$value = $valueField.GetValue($form)
if ($label.Top -ne $value.Top) { throw "Count label/value are not horizontally aligned: $($label.Top) vs $($value.Top)." }
$label3 = $formType.GetField('_Label3', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form)
$label4 = $formType.GetField('_Label4', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form)
if ($label3.Visible -or $label4.Visible) { throw 'First/last battery position labels are still visible.' }
$form.Dispose()
Write-Host 'PASS: frmTest creates four selection controls and exposes the pending-channel count label.'
