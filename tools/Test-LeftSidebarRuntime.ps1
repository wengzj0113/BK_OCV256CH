$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path $assemblyPath))
$formType = $assembly.GetType('software.frmTest', $true)
$moduleType = $assembly.GetType('software.Module1', $true)
$initField = $moduleType.GetField('Initsetting', [Reflection.BindingFlags] 'Static,Public,NonPublic')
$stationField = $initField.FieldType.GetField('stationSelect', [Reflection.BindingFlags] 'Instance,Public,NonPublic')
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

function Get-BottomRelativeTo([System.Windows.Forms.Control]$control, [System.Windows.Forms.Control]$ancestor) {
    $bottom = $control.Bottom
    $parent = $control.Parent
    while ($null -ne $parent -and $parent -ne $ancestor) {
        $bottom += $parent.Top
        $parent = $parent.Parent
    }
    if ($parent -ne $ancestor) { throw "$($control.Name) is not contained by $($ancestor.Name)." }
    return $bottom
}

function Set-ConfiguredStation([int]$stationIndex) {
    $init = $initField.GetValue($null)
    $stationField.SetValue($init, $stationIndex)
    $initField.SetValue($null, $init)
}

function Assert-StationVariant([int]$stationIndex) {
    Set-ConfiguredStation $stationIndex
    $variant = [Activator]::CreateInstance($formType)
    try {
        $configure = $formType.GetMethod('ConfigureStationControls', [Reflection.BindingFlags] 'Instance,NonPublic')
        $apply = $formType.GetMethod('ApplyFrmTestVisualStyle', [Reflection.BindingFlags] 'Instance,NonPublic')
        $configure.Invoke($variant, @()) | Out-Null
        $apply.Invoke($variant, @()) | Out-Null
        $variant.CreateControl()
        $variant.ClientSize = New-Object System.Drawing.Size(800, 600)
        $variant.PerformLayout()

        $panel = Find-Control $variant 'Panel1'
        $layout = $formType.GetField('responsiveLayout', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($variant)
        $layout.RefreshLeftContent($panel)
        $variant.PerformLayout()
        $stations = @(
            $formType.GetField('_Rbtn1', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($variant),
            $formType.GetField('_Rbtn2', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($variant),
            $formType.GetField('_Rbtn3', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($variant)
        )
        $selected = @($stations | Where-Object { $_.Checked })
        if ($selected.Count -ne 1 -or $selected[0].Text -ne ('OCV' + $stationIndex)) {
            throw "stationSelect=$stationIndex did not expose the configured station: selected=$($selected.Text)."
        }
        if ($selected[0].Parent -ne $panel.Controls['LeftControlContent'] -or $panel.HorizontalScroll.Visible) {
            throw "stationSelect=$stationIndex produced an invalid sidebar placement or horizontal scroll."
        }
    }
    finally {
        $variant.Dispose()
    }
}

try {
    $applyStyle = $formType.GetMethod('ApplyFrmTestVisualStyle', [Reflection.BindingFlags] 'Instance,NonPublic')
    $configureStations = $formType.GetMethod('ConfigureStationControls', [Reflection.BindingFlags] 'Instance,NonPublic')
    $resetScroll = $formType.GetMethod('ResetLeftPanelScroll', [Reflection.BindingFlags] 'Instance,NonPublic')
    $configureStations.Invoke($form, @()) | Out-Null
    $applyStyle.Invoke($form, @()) | Out-Null
    $form.CreateControl()
    $form.ClientSize = New-Object System.Drawing.Size(1280, 920)
    $form.PerformLayout()

    $panel = Find-Control $form 'Panel1'
    $content = $panel.Controls['LeftControlContent']
    $layoutField = $formType.GetField('responsiveLayout', [Reflection.BindingFlags] 'Instance,NonPublic')
    $layoutField.GetValue($form).RefreshLeftContent($panel)
    $form.PerformLayout()
    if ($null -eq $content) { throw 'LeftControlContent is missing.' }
    if ($form.AutoScaleMode -eq [System.Windows.Forms.AutoScaleMode]::None) {
        throw 'The structured sidebar must remain compatible with normal WinForms font/DPI scaling.'
    }
    foreach ($layoutName in @(
        'ControlBoardLayout',
        'ChannelSelectionLayout',
        'RangeSettingsLayout',
        'AlarmTemperatureLayout',
        'AlarmStatusLayout',
        'ProbeUsageLayout'
    )) {
        $managedLayout = Find-Control $form $layoutName
        if ($null -eq $managedLayout -or ($managedLayout -isnot [System.Windows.Forms.TableLayoutPanel] -and $managedLayout -isnot [System.Windows.Forms.FlowLayoutPanel])) {
            throw "Structured sidebar layout container is missing: $layoutName."
        }
    }

    $auto = $formType.GetField('chkAutoTest', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form)
    $manual = $formType.GetField('chkManualTest', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form)
    if ($null -eq $auto.Parent -or $null -eq $manual.Parent -or -not $auto.Enabled -or -not $manual.Enabled) { throw 'Automatic/manual test controls are not available.' }
    if (-not $auto.Checked -or $manual.Checked) { throw 'Automatic mode must be the default mode.' }
    if ($auto.Appearance -ne [System.Windows.Forms.Appearance]::Button -or $manual.Appearance -ne [System.Windows.Forms.Appearance]::Button) { throw "Automatic/manual controls must use the compact toggle-button appearance: auto=$($auto.Appearance), manual=$($manual.Appearance)." }

    $manual.Checked = $true
    if ($auto.Checked -or -not $manual.Checked) { throw 'Automatic/manual mode controls are not mutually exclusive.' }
    $manual.Checked = $false
    if (-not $auto.Checked) { throw 'Clearing both modes must restore automatic mode.' }

    $stations = @(
        $formType.GetField('_Rbtn1', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form),
        $formType.GetField('_Rbtn2', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form),
        $formType.GetField('_Rbtn3', [Reflection.BindingFlags] 'Instance,NonPublic').GetValue($form)
    )
    if (($stations | Where-Object { $_.Text -in @('OCV0','OCV1','OCV2') }).Count -ne 3) { throw 'OCV0/OCV1/OCV2 are not all present.' }
    if (($stations | Where-Object { $null -eq $_.Parent }).Count -ne 0) { throw 'One or more OCV station controls are not attached.' }
    $selectedStations = @($stations | Where-Object { $_.Checked })
    if ($selectedStations.Count -ne 1) { throw "Expected exactly one selected station button, actual=$($selectedStations.Count)." }
    $station = $selectedStations[0]
    if ($station.Parent -ne $content) { throw 'Current station is not a direct child of LeftControlContent.' }
    if ($content.Controls.GetChildIndex($station) -ne 0) { throw 'Current station is not the first left-sidebar item.' }
    if ($station.Height -lt 50) { throw "Current station button was not enlarged: height=$($station.Height)." }
    if ($null -ne $content.Controls['GroupBox2']) { throw 'Legacy station container must not remain in LeftControlContent.' }

    $channelGroup = $content.Controls['grpChannelSelection']
    $channelButtons = @('btnChannelSelectAll','btnChannelClear','btnChannelRectangle','btnChannelSelectSingle') | ForEach-Object { Find-Control $channelGroup $_ }
    if (($channelButtons | Where-Object { $null -eq $_ }).Count -ne 0) { throw 'A channel selection button is missing.' }
    if ((($channelButtons | ForEach-Object { $_.Top } | Select-Object -Unique).Count) -ne 1) { throw 'Channel selection buttons are not on one row.' }
    for ($i = 0; $i -lt $channelButtons.Count - 1; $i++) {
        if ($channelButtons[$i].Right -gt $channelButtons[$i + 1].Left) { throw 'Channel selection buttons overlap.' }
    }
    $rangeGroup = $content.Controls['GroupBox3']
    $minVoltage = Find-Control $rangeGroup 'txtMinV'
    $maxVoltage = Find-Control $rangeGroup 'txtMaxV'
    $ngTestButton = Find-Control $content.Controls['GroupBox1'] 'btnNGTest'
    $singleChannelButton = $channelButtons[3]
    $minHeader = Find-Control $rangeGroup 'Label9'
    $maxHeader = Find-Control $rangeGroup 'Label8'
    $minResistance = Find-Control $rangeGroup 'txtMinR'
    $maxResistance = Find-Control $rangeGroup 'txtMaxR'
    if ($minHeader.TextAlign -ne [System.Drawing.ContentAlignment]::MiddleCenter -or $maxHeader.TextAlign -ne [System.Drawing.ContentAlignment]::MiddleCenter) { throw 'Min/Max headers must be centered above their input columns.' }
    if ([Math]::Abs($maxVoltage.Width - $maxResistance.Width) -gt 2 -or [Math]::Abs($minResistance.Width - $minVoltage.Width) -gt 2 -or $maxVoltage.Top -ne $minVoltage.Top -or $maxResistance.Top -ne $minResistance.Top) { throw "Voltage and resistance input columns are not aligned: minV=$($minVoltage.Left),$($minVoltage.Top),$($minVoltage.Width); maxV=$($maxVoltage.Left),$($maxVoltage.Top),$($maxVoltage.Width); minR=$($minResistance.Left),$($minResistance.Top),$($minResistance.Width); maxR=$($maxResistance.Left),$($maxResistance.Top),$($maxResistance.Width)." }
    if ([Math]::Abs(($minHeader.Left + $minHeader.Width / 2) - ($minVoltage.Left + $minVoltage.Width / 2)) -gt 2 -or [Math]::Abs(($maxHeader.Left + $maxHeader.Width / 2) - ($maxVoltage.Left + $maxVoltage.Width / 2)) -gt 2) { throw 'Min/Max headers are not centered over their input boxes.' }
    if ($maxVoltage.Right -gt $rangeGroup.ClientSize.Width) {
        throw "Maximum-voltage input is clipped by its group: right=$($maxVoltage.Right), client=$($rangeGroup.ClientSize.Width)."
    }
    if ($ngTestButton.Right -gt $content.Controls['GroupBox1'].ClientSize.Width) {
        throw "NG test button is clipped by its group: right=$($ngTestButton.Right), client=$($content.Controls['GroupBox1'].ClientSize.Width)."
    }
    if ($singleChannelButton.Right -gt $channelGroup.ClientSize.Width) {
        throw "Single-channel button is clipped by its group: right=$($singleChannelButton.Right), client=$($channelGroup.ClientSize.Width)."
    }
    $statusLabel = Find-Control $content.Controls['GroupBox5'] 'LabelRun'
    $alarmStatusLabel = Find-Control $content.Controls['GroupBox5'] 'LabelAlarm'
    $statusFlow = $statusLabel.Parent
    if ($statusLabel.Right -gt $statusFlow.ClientSize.Width) { throw 'Device running status is clipped by its status cell.' }

    foreach ($statusState in @(
        @{ Name = 'running'; RunVisible = $true; AlarmVisible = $false; Active = $statusLabel },
        @{ Name = 'emergency'; RunVisible = $false; AlarmVisible = $true; Active = $alarmStatusLabel }
    )) {
        $statusLabel.Visible = $statusState.RunVisible
        $alarmStatusLabel.Visible = $statusState.AlarmVisible
        $form.PerformLayout()

        $activeStatus = $statusState.Active
        if ($activeStatus.Right -gt $activeStatus.Parent.ClientSize.Width) {
            throw "Alarm status state '$($statusState.Name)' is clipped: right=$($activeStatus.Right), client=$($activeStatus.Parent.ClientSize.Width)."
        }
    }

    $statusLabel.Visible = $true
    $alarmStatusLabel.Visible = $false
    $form.PerformLayout()

    $alarmGroup = $content.Controls['GroupBox5']
    $temperaturePanel = Find-Control $alarmGroup 'Panel2'
    $temperatureLayout = Find-Control $alarmGroup 'AlarmTemperatureLayout'
    $statusPanel = Find-Control $alarmGroup 'Panel3'
    if (($temperaturePanel.ClientSize.Height - $temperatureLayout.Bottom) -gt 8) { throw "Alarm temperature panel has excessive blank space: panel=$($temperaturePanel.ClientSize.Height), layoutBottom=$($temperatureLayout.Bottom)." }
    if (($statusPanel.Top - $temperaturePanel.Bottom) -gt 8) { throw "Alarm temperature/status gap is too large: $($statusPanel.Top - $temperaturePanel.Bottom)." }

    foreach ($rangeLabel in @((Find-Control $rangeGroup 'Label7'), (Find-Control $rangeGroup 'Label12'))) {
        if ($rangeLabel.PreferredSize.Width -gt $rangeLabel.ClientSize.Width) { throw "Range label wraps because its column is too narrow: $($rangeLabel.Name)." }
    }

    $probeGroup = $content.Controls['GroupBox8']
    $remainingUsage = Find-Control $probeGroup 'txtRemain'
    $usageUnit = Find-Control $probeGroup 'Label29'
    if (($usageUnit.Left - $remainingUsage.Right) -gt 8) { throw "Probe usage unit is detached from its input: gap=$($usageUnit.Left - $remainingUsage.Right)." }

    $eventLogGroup = $content.Controls['GroupBox7']
    if ($eventLogGroup.Height -lt 82) { throw "Event log group is too short: $($eventLogGroup.Height)." }
    if ($eventLogGroup.Bottom -gt $content.Height) { throw 'Event log group is outside the left content bounds.' }
    $resetButton = Find-Control $content 'Button2'
    $resetBottom = Get-BottomRelativeTo $resetButton $panel
    if ($resetBottom -gt $panel.ClientSize.Height) { throw "Alarm reset button is below the normal production viewport: bottom=$resetBottom, client=$($panel.ClientSize.Height)." }
    if ($panel.HorizontalScroll.Visible) { throw 'The left sidebar must not require horizontal scrolling.' }

    $uiThemeType = $assembly.GetType('software.UiTheme', $true)
    $textPrimary = $uiThemeType.GetField('TextPrimary', [Reflection.BindingFlags] 'Public,Static').GetValue($null)
    $cardBorder = $uiThemeType.GetField('CardBorder', [Reflection.BindingFlags] 'Public,Static').GetValue($null)
    foreach ($neutralButtonName in @('btnTrayState','btnOpen','btnClose')) {
        $neutralButton = Find-Control $content $neutralButtonName
        if ($neutralButton.FlatStyle -ne [System.Windows.Forms.FlatStyle]::Flat -or $neutralButton.FlatAppearance.BorderColor.ToArgb() -ne $cardBorder.ToArgb()) { throw "Sidebar neutral button style is inconsistent: $neutralButtonName." }
    }
    foreach ($fieldLabelName in @('Label2','Label5','Label7','Label12','Label1','Label14','Label15','Label16','Label26','Label29')) {
        $fieldLabel = Find-Control $content $fieldLabelName
        if ($null -ne $fieldLabel -and $fieldLabel.ForeColor.ToArgb() -ne $textPrimary.ToArgb()) { throw "Sidebar field label must use the neutral body color: $fieldLabelName." }
    }
    Assert-VisibleChildrenContained $content

    $form.ClientSize = New-Object System.Drawing.Size(800, 600)
    $form.PerformLayout()
    $layoutField.GetValue($form).RefreshLeftContent($panel)
    $form.PerformLayout()
    if (-not $panel.VerticalScroll.Visible) { throw 'The left sidebar must scroll at the compact window size.' }
    if ($panel.HorizontalScroll.Visible) { throw 'The compact left sidebar must not require horizontal scrolling.' }
    Assert-VisibleChildrenContained $content

    foreach ($stationIndex in 0, 1, 2) {
        Assert-StationVariant $stationIndex
    }

    $resetScroll.Invoke($form, @()) | Out-Null
    if ($panel.AutoScrollPosition.Y -ne 0) { throw "Left panel did not reset to the top: $($panel.AutoScrollPosition.Y)" }
    Write-Output 'PASS: left sidebar runtime layout, station controls, mode controls and startup scroll reset are valid.'
}
finally {
    $form.Dispose()
}
