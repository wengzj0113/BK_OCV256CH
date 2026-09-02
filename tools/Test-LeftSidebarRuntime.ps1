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

function Assert-VisibleChildrenContained([System.Windows.Forms.Control]$root) {
    foreach ($control in $root.Controls) {
        if ($control.Visible -and ($control.Right -gt $root.ClientSize.Width -or $control.Bottom -gt $root.ClientSize.Height)) {
            throw "Visible control $($control.Name) escapes $($root.Name): right=$($control.Right), bottom=$($control.Bottom), client=$($root.ClientSize.Width)x$($root.ClientSize.Height)."
        }
        Assert-VisibleChildrenContained $control
    }
}

try {
    $configureStations = $formType.GetMethod('ConfigureStationControls', [Reflection.BindingFlags] 'Instance,NonPublic')
    if ($null -eq $configureStations) { throw 'ConfigureStationControls is missing.' }
    $configureStations.Invoke($form, @()) | Out-Null
    $form.CreateControl()

    $panel = Find-Control $form 'Panel1'
    $content = Find-Control $form 'leftContentPanel'
    if ($null -eq $panel -or $null -eq $content) { throw 'The serialized left sidebar containers are missing.' }
    if (-not $panel.AutoScroll) { throw 'The left sidebar must remain vertically scrollable.' }
    if ($panel.HorizontalScroll.Visible) { throw 'The left sidebar must not require horizontal scrolling.' }

    $station = $content.Controls['currentStationButton']
    if ($null -eq $station -or -not $station.Enabled) { throw 'The serialized current station button is missing or disabled.' }
    if ($station.Parent -ne $content -or $content.Controls.GetChildIndex($station) -ne 0) { throw 'Current station is not the first serialized sidebar item.' }
    if ($station.Height -lt 50) { throw "Current station button was not enlarged: height=$($station.Height)." }

    $names = @('grpTestMode', 'GroupBox1', 'grpChannelSelection', 'GroupBox3', 'GroupBox5', 'GroupBox8', 'GroupBox7', 'resetAlarmLayout')
    $boxes = @{}
    foreach ($name in $names) {
        $control = $content.Controls[$name]
        if ($null -eq $control) { throw "Serialized sidebar item is missing: $name." }
        if ($control.Width -lt 300 -or $control.Height -le 0) { throw "Sidebar item is collapsed: $name=$($control.Width)x$($control.Height)." }
        $boxes[$name] = $control
    }
    for ($i = 0; $i -lt $names.Count; $i++) {
        for ($j = $i + 1; $j -lt $names.Count; $j++) {
            $a = $boxes[$names[$i]]
            $b = $boxes[$names[$j]]
            if (($a.Left -lt $b.Right) -and ($a.Right -gt $b.Left) -and ($a.Top -lt $b.Bottom) -and ($a.Bottom -gt $b.Top)) {
                throw "Sidebar items overlap: $($names[$i]) and $($names[$j])."
            }
        }
    }

    $themeType = $uiAssembly.GetType('software.UiTheme', $true)
    $cardBorder = $themeType.GetField('CardBorder', [Reflection.BindingFlags] 'Public,Static').GetValue($null)
    foreach ($buttonName in @('btnTrayState', 'btnOpen', 'btnClose')) {
        $button = Find-Control $content $buttonName
        if ($null -eq $button -or $button.FlatStyle -ne [System.Windows.Forms.FlatStyle]::Flat -or $button.FlatAppearance.BorderColor.ToArgb() -ne $cardBorder.ToArgb()) {
            throw "Sidebar neutral button style is inconsistent: $buttonName."
        }
    }

    Assert-VisibleChildrenContained $content
    $form.ClientSize = New-Object System.Drawing.Size(800, 600)
    $form.PerformLayout()
    if (-not $panel.VerticalScroll.Visible) { throw 'The compact left sidebar must scroll vertically.' }
    if ($panel.HorizontalScroll.Visible) { throw 'The compact left sidebar must not scroll horizontally.' }
    Write-Output 'PASS: serialized left sidebar bounds, styles, station selection and compact scrolling are valid.'
}
finally {
    $form.Dispose()
}
