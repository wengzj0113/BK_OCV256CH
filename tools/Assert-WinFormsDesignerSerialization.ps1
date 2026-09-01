$ErrorActionPreference = 'Stop'

$designerFiles = @(
    'frmTest.Designer.vb',
    'ChannelGridControl.Designer.vb',
    'StatisticsControl.Designer.vb',
    'BottomStatusControl.Designer.vb'
)

$forbiddenCalls = @(
    'BuildDesignerLayout',
    'BuildDesignerTestModeLayout',
    'BuildDesignerControlBoardLayout',
    'BuildDesignerChannelSelectionLayout',
    'BuildDesignerRangeLayout',
    'BuildDesignerAlarmLayout',
    'BuildDesignerTemperatureLayout',
    'BuildDesignerAlarmStatusLayout',
    'BuildDesignerProbeUsageLayout',
    'BuildDesignerEventLogLayout',
    'ConfigureDesignerCard',
    'ConfigureDesignerInput',
    'ConfigureDesignerLabel',
    'ConfigureDesignerButton',
    'ConfigureDesignerTable',
    'PutDesignerControl',
    'AddLeftContentItem',
    'ConfigureCoordinateLabel',
    'AddColumnLabels',
    'AddRowLabels',
    'CreateValueLabel',
    'ConfigureTitleLabel',
    'ConfigureLegendItem',
    'ConfigureStatusLabel'
)

$violations = New-Object System.Collections.Generic.List[string]
foreach ($file in $designerFiles) {
    if (-not (Test-Path -LiteralPath $file)) {
        $violations.Add("Missing Designer file: $file")
        continue
    }

    $lines = Get-Content -LiteralPath $file
    for ($index = 0; $index -lt $lines.Count; $index++) {
        foreach ($call in $forbiddenCalls) {
            if ($lines[$index] -match "\b$([regex]::Escape($call))\s*\(") {
                $violations.Add("${file}:$($index + 1): non-serializable helper call '$call'")
            }
        }
        if ($lines[$index] -match '\.Controls\.(Clear|Remove)\s*\(') {
            $violations.Add("${file}:$($index + 1): control tree is rebuilt with Controls.Clear/Remove")
        }
    }
}

$requiredNames = @{
    'frmTest.Designer.vb' = @(
        'rootLayout', 'rightLayout', 'leftContentPanel', 'resetAlarmLayout',
        'testModeLayout', 'controlBoardLayout', 'channelSelectionLayout',
        'rangeSettingsLayout', 'alarmSectionLayout', 'alarmTemperatureLayout',
        'alarmStatusLayout', 'smokeOneLayout', 'smokeTwoLayout',
        'deviceStatusLayout', 'grpTestMode', 'chkAutoTest', 'chkManualTest',
        'currentStationButton'
    )
    'ChannelGridControl.Designer.vb' = @(
        'lblColumn1', 'lblColumn16', 'lblRow1', 'lblRow16'
    )
    'StatisticsControl.Designer.vb' = @(
        'totalValue', 'passValue', 'ngValue', 'testingValue', 'idleValue',
        'totalTitle', 'passTitle', 'ngTitle', 'testingTitle', 'idleTitle',
        'passSwatch', 'ngSwatch', 'testingSwatch', 'idleSwatch',
        'passLegendText', 'ngLegendText', 'testingLegendText', 'idleLegendText'
    )
    'BottomStatusControl.Designer.vb' = @(
        'stationLabel', 'controlBoardLabel', 'plcLabel', 'voltageLabel',
        'resistanceLabel', 'databaseLabel', 'mesLabel', 'userLabel', 'timeLabel'
    )
}

foreach ($entry in $requiredNames.GetEnumerator()) {
    $file = $entry.Key
    $content = Get-Content -LiteralPath $file -Raw
    foreach ($name in $entry.Value) {
        $namePattern = '\.Name\s*=\s*"' + [regex]::Escape($name) + '"'
        if ($content -notmatch $namePattern) {
            $violations.Add("${file}: missing serialized Name for '$name'")
        }
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Output "FAIL: $_" }
    exit 1
}

Write-Output 'PASS: WinForms Designer files contain no custom reconstruction calls and required controls are named.'
