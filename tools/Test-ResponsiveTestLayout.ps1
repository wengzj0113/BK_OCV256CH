$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$uiAssembly = [Reflection.Assembly]::LoadFrom((Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'XL_OCV_UIControls.dll'))
$formType = $assembly.GetType('software.frmTest', $true)
$form = [Activator]::CreateInstance($formType)

function Find-Control([System.Windows.Forms.Control]$root, [string]$name) {
    foreach ($control in $root.Controls) {
        if ($control.Name -eq $name) { return $control }
        $nested = Find-Control $control $name
        if ($null -ne $nested) { return $nested }
    }
    return $null
}

try {
    $form.CreateControl()
    $root = Find-Control $form 'rootLayout'
    $panel = Find-Control $form 'Panel1'
    $content = Find-Control $form 'leftContentPanel'
    $right = Find-Control $form 'rightLayout'
    $stats = Find-Control $form 'statisticsControl'
    $grid = Find-Control $form 'channelGrid'
    if ($null -eq $root -or $null -eq $panel -or $null -eq $content -or $null -eq $right -or $null -eq $stats -or $null -eq $grid) {
        throw 'Serialized responsive root, sidebar or right-side controls are incomplete.'
    }
    if (-not $panel.AutoScroll) { throw 'Panel1 must remain vertically scrollable.' }
    if ($root.GetControlFromPosition(0, 0) -ne $panel -or $root.GetControlFromPosition(1, 0) -ne $right) { throw 'Root layout cells are not wired to the sidebar and right region.' }
    if ($right.GetControlFromPosition(0, 0) -ne $stats -or $right.GetControlFromPosition(0, 1) -ne $grid) { throw 'Right layout cells are not wired to statistics and channel grid.' }
    if ($stats.Dock -ne [System.Windows.Forms.DockStyle]::Fill -or $grid.Dock -ne [System.Windows.Forms.DockStyle]::Fill) { throw 'Right-side controls must fill their serialized cells.' }

    foreach ($size in @(@(1920, 1080), @(1366, 768), @(1024, 768), @(800, 600), @(600, 500))) {
        $form.ClientSize = New-Object System.Drawing.Size($size[0], $size[1])
        $form.PerformLayout()
        if ($root.Width -le 0 -or $root.Height -le 0 -or $right.Width -le 0 -or $right.Height -le 0) { throw "Root/right layout has no usable bounds at $($size[0])x$($size[1])." }
        if ($panel.Width -le 0 -or $content.Width -le 0 -or $grid.Width -le 0 -or $grid.Height -le 0) { throw "Sidebar/grid collapsed at $($size[0])x$($size[1])." }
        if ($content.Width -gt $panel.ClientSize.Width) { throw "Sidebar content requires horizontal scrolling at $($size[0])x$($size[1])." }
        $surface = Find-Control $grid 'gridSurface'
        if ($null -eq $surface -or $surface -isnot [System.Windows.Forms.Panel] -or $surface.ClientSize.Width -le 0 -or $surface.ClientSize.Height -le 0) { throw "Grid surface is invalid at $($size[0])x$($size[1])." }
    }
    Write-Output 'PASS: frmTest Designer layout adapts at 1920x1080, 1366x768, 1024x768, 800x600 and 600x500.'
}
finally {
    $form.Dispose()
}
