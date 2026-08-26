$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$layoutType = $assembly.GetType('software.ResponsiveTestLayout', $false)
if ($null -eq $layoutType) { throw 'ResponsiveTestLayout type is missing.' }

$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
if ($form.MinimumSize.Width -ne 0 -or $form.MinimumSize.Height -ne 0) { throw "frmTest designer still imposes a fixed minimum window size: $($form.MinimumSize)." }
$form.MinimumSize = New-Object System.Drawing.Size(0, 0)
$form.MinimumSize = New-Object System.Drawing.Size(0, 0)
$leftPanel = $form.Controls['Panel1']
if ($null -eq $leftPanel) { throw 'Panel1 control is missing.' }
$applyStyle = $formType.GetMethod('ApplyFrmTestVisualStyle', [Reflection.BindingFlags] 'Instance,NonPublic')
if ($null -eq $applyStyle) { throw 'frmTest visual style entry point is missing.' }
$applyStyle.Invoke($form, @()) | Out-Null
$form.CreateControl()
$gridField = $formType.GetField('_channelGrid', [Reflection.BindingFlags] 'Instance,NonPublic,Public')
if ($null -eq $gridField) { throw 'channelGrid field is missing.' }
$grid = $gridField.GetValue($form)
$form.Size = New-Object System.Drawing.Size(1280, 902)
$form.PerformLayout()

$root = $null
foreach ($child in $form.Controls) {
    if ($child -is [System.Windows.Forms.TableLayoutPanel] -and $child.ColumnCount -eq 2 -and $child.RowCount -eq 1) {
        $root = $child
        break
    }
}
if ($null -eq $root) { throw 'Responsive root layout was not created.' }
if ($root.GetControlFromPosition(0, 0) -eq $null -or $root.GetControlFromPosition(1, 0) -eq $null) {
    throw 'Responsive root layout does not contain both left and right regions.'
}
if ($root.Width -le 0 -or $root.Height -le 0) { throw 'Responsive root layout has no usable bounds.' }
if ($form.MinimumSize.Width -ne 0 -or $form.MinimumSize.Height -ne 0) { throw 'frmTest must not impose a fixed minimum window size.' }
if ($form.MinimumSize.Width -ne 0 -or $form.MinimumSize.Height -ne 0) { throw 'frmTest must not impose a fixed minimum window size.' }

if (-not $leftPanel.AutoScroll) { throw 'Left control region must remain scrollable.' }
$leftContent = $leftPanel.Controls['LeftControlContent']
if ($null -eq $leftContent) { throw 'Left control content container is missing.' }
if ($leftPanel.DisplayRectangle.Width -lt $leftContent.Width -or -not $leftPanel.HorizontalScroll.Visible) {
    throw "Left control region does not expose its full content width: display=$($leftPanel.DisplayRectangle.Width), content=$($leftContent.Width), horizontal-scroll=$($leftPanel.HorizontalScroll.Visible)."
}

$rightRegion = $root.GetControlFromPosition(1, 0)
foreach ($size in @(@(1920, 1080), @(1920, 991), @(1366, 768), @(1024, 768), @(800, 600), @(600, 500))) {
    $form.Size = New-Object System.Drawing.Size($size[0], $size[1])
    $form.PerformLayout()
    if ($rightRegion.Width -le 0 -or $rightRegion.Height -le 0) { throw "Right region did not adapt at $($size[0])x$($size[1])." }

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
    if ($null -eq $gridLayout) { throw "Matrix layout was not found at $($size[0])x$($size[1])." }
    $lastTile = $gridLayout.GetControlFromPosition(15, 15)
    $lastBottom = $lastTile.Bottom + $gridLayout.Top
    if ($lastBottom -gt $grid.ClientSize.Height) {
        throw "Matrix is clipped at $($size[0])x$($size[1]): bottom=$lastBottom, viewport=$($grid.ClientSize.Height)."
    }
}

Write-Output "PASS: responsive layouts at 1920x1080, 1920x991, 1366x768, 1024x768, 800x600 and 600x500; left-scroll=$($leftPanel.AutoScroll)."
