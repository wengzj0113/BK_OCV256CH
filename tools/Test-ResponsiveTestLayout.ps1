$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { (Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe') }
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$layoutType = $assembly.GetType('software.ResponsiveTestLayout', $false)
if ($null -eq $layoutType) { throw 'ResponsiveTestLayout type is missing.' }
$layoutSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\ResponsiveTestLayout.vb')
if ($layoutSource -match 'leftPanel\.AutoScrollMinSize\s*=\s*New Size\(0,\s*1280\)') {
    throw 'Legacy fixed left scroll height must be removed.'
}
if ($layoutSource -notmatch 'StretchEventLogToViewport') { throw 'Flexible event-log height path is missing.' }
if ($layoutSource -notmatch 'CalculateLeftContentHeight') { throw 'Dynamic left content height calculation is missing.' }

$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)
if ($form.MinimumSize.Width -ne 0 -or $form.MinimumSize.Height -ne 0) { throw "frmTest designer still imposes a fixed minimum window size: $($form.MinimumSize)." }
$form.MinimumSize = New-Object System.Drawing.Size(0, 0)
$form.MinimumSize = New-Object System.Drawing.Size(0, 0)
$leftPanel = $form.Controls['Panel1']
if ($null -eq $leftPanel) { throw 'Panel1 control is missing.' }
$applyEssentialLayout = $formType.GetMethod('ApplyFrmTestEssentialLayout', [Reflection.BindingFlags] 'Instance,NonPublic')
if ($null -eq $applyEssentialLayout) { throw 'frmTest essential layout entry point is missing.' }
$applyEssentialLayout.Invoke($form, @()) | Out-Null
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
if ($leftContent.Width -gt $leftPanel.DisplayRectangle.Width) {
    throw "Left control content exceeds its viewport width: display=$($leftPanel.DisplayRectangle.Width), content=$($leftContent.Width)."
}

$rightRegion = $root.GetControlFromPosition(1, 0)
$leftRegion = $root.GetControlFromPosition(0, 0)
$stats = $rightRegion.GetControlFromPosition(0, 0)
if ($leftRegion -ne $leftPanel) { throw 'Root left cell does not contain Panel1.' }
if ($stats -eq $null) { throw 'Right region does not contain the statistics control.' }
if ($leftPanel.Dock -ne [System.Windows.Forms.DockStyle]::Fill) {
    throw "Left control region must fill its root cell; actual dock=$($leftPanel.Dock)."
}
if ($stats.Dock -ne [System.Windows.Forms.DockStyle]::Fill) {
    throw "Statistics region must fill its right cell; actual dock=$($stats.Dock)."
}
if ($grid.Dock -ne [System.Windows.Forms.DockStyle]::Fill) {
    throw "Channel grid must fill its right cell; actual dock=$($grid.Dock)."
}
if ($stats.Left -ne 0 -or $grid.Left -ne 0) {
    throw "Right content must start at the right region origin; stats=$($stats.Left), grid=$($grid.Left)."
}
foreach ($size in @(@(1920, 1080), @(1920, 991), @(1366, 768), @(1024, 768), @(800, 600), @(600, 500))) {
    $form.Size = New-Object System.Drawing.Size($size[0], $size[1])
    $form.PerformLayout()
    if ($rightRegion.Width -le 0 -or $rightRegion.Height -le 0) { throw "Right region did not adapt at $($size[0])x$($size[1])." }

    $gridSurface = $null
    $pending = New-Object System.Collections.Queue
    foreach ($child in $grid.Controls) { $pending.Enqueue($child) }
    while ($pending.Count -gt 0 -and $null -eq $gridSurface) {
        $candidate = $pending.Dequeue()
        if ($candidate.GetType().Name -eq 'ChannelGridSurface') {
            $gridSurface = $candidate
            break
        }
        foreach ($nested in $candidate.Controls) { $pending.Enqueue($nested) }
    }
    if ($null -eq $gridSurface) { throw "Virtual channel surface was not found at $($size[0])x$($size[1])." }
    if ($gridSurface.Dock -ne [System.Windows.Forms.DockStyle]::Fill) {
        throw "Virtual channel surface must fill the grid at $($size[0])x$($size[1])."
    }
    if ($gridSurface.ClientSize.Width -le 0 -or $gridSurface.ClientSize.Height -le 0) {
        throw "Virtual channel surface has no usable bounds at $($size[0])x$($size[1])."
    }
}

Write-Output "PASS: responsive layouts at 1920x1080, 1920x991, 1366x768, 1024x768, 800x600 and 600x500; left-scroll=$($leftPanel.AutoScroll)."
