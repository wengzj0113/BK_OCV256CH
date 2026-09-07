$ErrorActionPreference = 'Stop'

$designerFiles = @(
    'frmTest.Designer.vb',
    'ChannelGridControl.Designer.vb',
    'StatisticsControl.Designer.vb',
    'BottomStatusControl.Designer.vb'
)

$projectFile = 'MP-60A-RV-64Mem.vbproj'
if (-not (Test-Path -LiteralPath $projectFile)) {
    throw "Missing project file: $projectFile"
}
$projectContent = Get-Content -LiteralPath $projectFile -Raw
if ($projectContent -notmatch '<UseWinFormsOutOfProcDesigner>True</UseWinFormsOutOfProcDesigner>') {
    $projectViolations = "${projectFile}: WinForms out-of-process Designer must be enabled for the x86 .NET Framework project"
}
$anyCpuPattern = 'Condition="[^"]*Debug\|AnyCPU[^"]*"[\s\S]*?<PlatformTarget>AnyCPU</PlatformTarget>'
if ($projectContent -notmatch $anyCpuPattern) {
    $projectViolations = "${projectFile}: Debug|AnyCPU must be available as a separate Designer-compatible configuration"
}
if ($projectContent -notmatch '<ProjectReference Include="XL-OCV-UIControls\.vbproj">[\s\S]*?<ReferenceOutputAssembly>True</ReferenceOutputAssembly>') {
    $projectViolations = "${projectFile}: must reference XL-OCV-UIControls as a project output for Designer type resolution"
}

$controlProjectFile = 'XL-OCV-UIControls.vbproj'
if (-not (Test-Path -LiteralPath $controlProjectFile)) {
    $projectViolations = "${controlProjectFile}: missing custom-control project required by frmTest Designer"
} else {
    $controlProjectContent = Get-Content -LiteralPath $controlProjectFile -Raw
    if ($controlProjectContent -notmatch '<RootNamespace>software</RootNamespace>') {
        $projectViolations = "${controlProjectFile}: RootNamespace must remain software for Global.software Designer types"
    }
}

foreach ($solutionFile in @('MP-60A-RV-64Mem.sln', 'Battery Testing.sln')) {
    if (Test-Path -LiteralPath $solutionFile) {
        $solutionContent = Get-Content -LiteralPath $solutionFile -Raw
        if ($solutionContent -notmatch '\{A53D5F1E-2C7A-4D73-9B95-2F248E89B3C4\}\.Debug\|x86\.ActiveCfg\s*=\s*Debug\|AnyCPU') {
            $projectViolations = "${solutionFile}: x86 Designer configuration must load XL-OCV-UIControls as AnyCPU"
        }
    }
}

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
if ($projectViolations) {
    $violations.Add($projectViolations)
}
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
        if ($lines[$index] -match '\bUiTheme\.') {
            $violations.Add("${file}:$($index + 1): Designer code must use serializable literal theme values instead of UiTheme references")
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

$channelGridDesignerContent = Get-Content -LiteralPath 'ChannelGridControl.Designer.vb' -Raw
if ($channelGridDesignerContent -notmatch 'Me\.gridSurface\s*=\s*New\s+System\.Windows\.Forms\.Panel\s*\(\)') {
    $violations.Add('ChannelGridControl.Designer.vb: gridSurface must use the framework Panel type so the CodeDOM Designer has no same-project type dependency')
}
if ($channelGridDesignerContent -notmatch 'Private\s+gridSurface\s+As\s+System\.Windows\.Forms\.Panel') {
    $violations.Add('ChannelGridControl.Designer.vb: gridSurface field must be declared as System.Windows.Forms.Panel')
}
$channelGridCodeContent = Get-Content -LiteralPath 'ChannelGridControl.vb' -Raw
if ($channelGridCodeContent -match '(?m)^Public\s+(?:NotInheritable\s+)?Class\s+ChannelGridSurface\b') {
    $violations.Add('ChannelGridControl.vb: custom ChannelGridSurface reintroduces a same-project CodeDOM Designer dependency')
}

$frmLines = Get-Content -LiteralPath 'frmTest.Designer.vb'
$frmContent = $frmLines -join [Environment]::NewLine
if ($frmContent -notmatch 'New\s+Global\.software\.ChannelGridControl\s*\(\)') {
    $violations.Add('frmTest.Designer.vb: ChannelGridControl constructor must use Global.software qualification for CodeDOM Designer resolution')
}
if ($frmContent -notmatch 'New\s+Global\.software\.StatisticsControl\s*\(\)') {
    $violations.Add('frmTest.Designer.vb: StatisticsControl constructor must use Global.software qualification for CodeDOM Designer resolution')
}
if ($frmContent -notmatch 'SelectionMode\s*=\s*Global\.software\.ChannelSelectionMode\.SingleChannel') {
    $violations.Add('frmTest.Designer.vb: ChannelSelectionMode must use Global.software qualification for CodeDOM Designer resolution')
}
foreach ($typeName in @('ChannelGridControl', 'StatisticsControl')) {
    if ($frmContent -notmatch "New\s+(?:Global\.)?software\.$([regex]::Escape($typeName))\s*\(") {
        $violations.Add("frmTest.Designer.vb: external control '$typeName' must use its fully qualified software Designer type name")
    }
}

$rootFormAddIndex = -1
for ($index = 0; $index -lt $frmLines.Count; $index++) {
    if ($frmLines[$index] -match 'Me\.Controls\.Add\(Me\.rootLayout\)') {
        $rootFormAddIndex = $index
        break
    }
}
if ($rootFormAddIndex -lt 0) {
    $violations.Add('frmTest.Designer.vb: missing final form-level rootLayout add')
} else {
    $rootChildNames = @('Panel1', 'rightLayout')
    foreach ($child in $rootChildNames) {
        $rootChildIndex = -1
        for ($index = 0; $index -lt $rootFormAddIndex; $index++) {
            if ($frmLines[$index] -match "Me\.rootLayout\.Controls\.Add\(Me\.$([regex]::Escape($child)),") {
                $rootChildIndex = $index
                break
            }
        }
        if ($rootChildIndex -lt 0) {
            $violations.Add("frmTest.Designer.vb: missing rootLayout child '$child' add before the form-level rootLayout add")
        }
    }

    foreach ($child in @('channelGrid', 'statisticsControl', 'Panel1')) {
        for ($index = 0; $index -lt $frmLines.Count; $index++) {
            if ($frmLines[$index] -match "Me\.Controls\.Add\(Me\.$([regex]::Escape($child))\)") {
                $violations.Add("frmTest.Designer.vb:$($index + 1): '$child' is added directly to the form; it must only be under rootLayout")
            }
        }
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Output "FAIL: $_" }
    exit 1
}

Write-Output 'PASS: WinForms Designer files contain no custom reconstruction calls and required controls are named.'
