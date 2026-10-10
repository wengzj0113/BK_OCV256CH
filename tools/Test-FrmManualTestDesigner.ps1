[CmdletBinding()]
param([switch]$RequireRuntime)
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$designerPath = Join-Path $repoRoot 'FrmManualTest.Designer.vb'
if (-not (Test-Path -LiteralPath $designerPath -PathType Leaf)) {
    throw 'Missing FrmManualTest Designer file: FrmManualTest.Designer.vb'
}

function Assert-Regex {
    param(
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$FailureMessage
    )

    if (-not [System.Text.RegularExpressions.Regex]::IsMatch(
        $Text,
        $Pattern,
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
            [System.Text.RegularExpressions.RegexOptions]::Multiline
    )) {
        throw $FailureMessage
    }
}

function Remove-VbComments {
    param([string]$Text)

    $withoutCommentLines = [System.Text.RegularExpressions.Regex]::Replace($Text, '(?m)^\s*''.*(?:\r?\n|$)', '')
    return [System.Text.RegularExpressions.Regex]::Replace($withoutCommentLines, '(?m)(?<=\s)''.*$', '')
}

function Find-ControlByName {
    param(
        $Parent,
        [string]$Name
    )

    if ($Parent.Name -eq $Name) {
        return $Parent
    }
    foreach ($child in $Parent.Controls) {
        $match = Find-ControlByName -Parent $child -Name $Name
        if ($null -ne $match) {
            return $match
        }
    }
    return $null
}

function Get-CheckedChannels {
    param([System.Collections.IDictionary]$ChannelBoxes)

    $checked = [System.Collections.Generic.List[int]]::new()
    for ($channel = 1; $channel -le 256; $channel++) {
        if ($ChannelBoxes[$channel].Checked) {
            $checked.Add($channel)
        }
    }
    return $checked.ToArray()
}

function Assert-CheckedChannels {
    param(
        [System.Collections.IDictionary]$ChannelBoxes,
        [int[]]$Expected,
        [string]$Scenario
    )

    $actual = @(Get-CheckedChannels -ChannelBoxes $ChannelBoxes)
    $expectedSorted = @($Expected | Sort-Object)
    if ($actual.Count -ne $expectedSorted.Count) {
        throw "$Scenario selected $($actual.Count) channels; expected $($expectedSorted.Count)."
    }
    for ($index = 0; $index -lt $expectedSorted.Count; $index++) {
        if ($actual[$index] -ne $expectedSorted[$index]) {
            throw "$Scenario selected channel $($actual[$index]) at position $($index + 1); expected $($expectedSorted[$index])."
        }
    }
}

$designerText = Get-Content -LiteralPath $designerPath -Raw
$classMatch = [System.Text.RegularExpressions.Regex]::Match(
    $designerText,
    '(?ims)^\s*(?:Partial\s+Public|Public\s+Partial|Partial)\s+Class\s+FrmManualTest\b(?<body>.*?)^\s*End\s+Class\b'
)
if (-not $classMatch.Success) {
    throw 'FrmManualTest.Designer.vb must declare the partial FrmManualTest class.'
}
$designerClass = Remove-VbComments $classMatch.Value
$classBody = Remove-VbComments $classMatch.Groups['body'].Value
Assert-Regex $designerClass '(?im)^\s*(?:Partial\s+Public|Public\s+Partial|Partial)\s+Class\s+FrmManualTest\b' 'Designer must declare partial FrmManualTest.'
Assert-Regex $classBody '(?im)^\s*Inherits\s+System\.Windows\.Forms\.Form\s*$' 'Designer FrmManualTest must inherit System.Windows.Forms.Form.'

$initializeMatch = [System.Text.RegularExpressions.Regex]::Match(
    $classBody,
    '(?ims)^\s*(?:(?:Private|Protected|Friend|Public)\s+)?Sub\s+InitializeComponent\s*\(\s*\)(?<body>.*?)^\s*End\s+Sub\b'
)
if (-not $initializeMatch.Success) {
    throw 'FrmManualTest class must define InitializeComponent.'
}
$initializeComponent = Remove-VbComments $initializeMatch.Groups['body'].Value

$codeBehindPath = Join-Path $repoRoot 'FrmManualTest.vb'
$codeBehind = Remove-VbComments (Get-Content -LiteralPath $codeBehindPath -Raw)
$projectPath = Join-Path $repoRoot 'MP-60A-RV-64Mem.vbproj'
$project = [xml](Get-Content -LiteralPath $projectPath -Raw)

Assert-Regex $initializeComponent '(?im)\bMe\.Text\s*=\s*"手动测试"' 'Designer must preserve the 手动测试 form title.'
Assert-Regex $initializeComponent '(?im)\bMe\.Size\s*=\s*New\s+(?:System\.Drawing\.)?Size\s*\(\s*1475\s*,\s*975\s*\)' 'Designer must preserve the 1475 x 975 form size.'
Assert-Regex $initializeComponent '(?im)\bMe\.MinimumSize\s*=\s*New\s+(?:System\.Drawing\.)?Size\s*\(\s*1225\s*,\s*850\s*\)' 'Designer must preserve the 1225 x 850 minimum size.'

Assert-Regex $initializeComponent '(?im)^\s*(?:Me\.)?rootLayoutPanel\s*=\s*New\s+(?:System\.Windows\.Forms\.)?TableLayoutPanel\b' 'Designer must create rootLayoutPanel as a TableLayoutPanel.'
Assert-Regex $initializeComponent '(?im)\b(?:Me\.)?rootLayoutPanel\.RowCount\s*=\s*4\b' 'Designer rootLayoutPanel must have four rows.'
$rootNamePattern = '(?im)\b(?:Me\.)?rootLayoutPanel\.Name\s*=\s*"rootLayoutPanel"'
Assert-Regex $initializeComponent $rootNamePattern 'Designer must name the root table rootLayoutPanel.'

$rowStylePattern = '(?im)\brootLayoutPanel\.RowStyles\.Add\(\s*New\s+(?:System\.Windows\.Forms\.)?RowStyle\s*\(\s*(?:System\.Windows\.Forms\.)?SizeType\.(?<type>Absolute|Percent)\s*,\s*(?<value>\d+(?:\.\d+)?(?:[FfRrDd!#])?)\s*\)\s*\)'
$rowStyles = [System.Text.RegularExpressions.Regex]::Matches($initializeComponent, $rowStylePattern)
if ($rowStyles.Count -ne 4) {
    throw "Designer rootLayoutPanel defines $($rowStyles.Count) explicit row styles; expected four."
}
$expectedRows = @(
    @{ Type = 'Absolute'; Value = 112 },
    @{ Type = 'Percent'; Value = 58 },
    @{ Type = 'Percent'; Value = 42 },
    @{ Type = 'Absolute'; Value = 46 }
)
for ($index = 0; $index -lt $expectedRows.Count; $index++) {
    $actualType = $rowStyles[$index].Groups['type'].Value
    $actualValueText = $rowStyles[$index].Groups['value'].Value.TrimEnd([char[]]@('F', 'f', 'R', 'r', 'D', 'd', '!', '#'))
    $actualValue = [double]::Parse($actualValueText, [Globalization.CultureInfo]::InvariantCulture)
    if ($actualType -ne $expectedRows[$index].Type -or $actualValue -ne $expectedRows[$index].Value) {
        throw "Designer root row $($index + 1) is $actualType $actualValue; expected $($expectedRows[$index].Type) $($expectedRows[$index].Value)."
    }
}

Assert-Regex $initializeComponent '(?im)\bmatrixPanel\.ColumnCount\s*=\s*16\b' 'Designer matrixPanel must define 16 columns.'
Assert-Regex $initializeComponent '(?im)\bmatrixPanel\.RowCount\s*=\s*16\b' 'Designer matrixPanel must define 16 rows.'

$staticControls = @(
    @{ Name = 'commandPanel'; Type = 'FlowLayoutPanel'; Row = 0 },
    @{ Name = 'matrixPanel'; Type = 'TableLayoutPanel'; Row = 1 },
    @{ Name = 'resultGrid'; Type = 'DataGridView'; Row = 2 },
    @{ Name = 'bottomPanel'; Type = 'TableLayoutPanel'; Row = 3 }
)
$doubleQuote = [char]34
foreach ($control in $staticControls) {
    $name = $control.Name
    $type = $control.Type
    $row = $control.Row
    Assert-Regex $initializeComponent "(?im)^\s*(?:Me\.)?$name\s*=\s*New\s+(?:System\.Windows\.Forms\.)?$type\b" "Designer must create $name as $type."
    $namePattern = '(?im)\b(?:Me\.)?' + [regex]::Escape($name) + '\.Name\s*=\s*' + $doubleQuote + [regex]::Escape($name) + $doubleQuote
    Assert-Regex $initializeComponent $namePattern "Designer must name the $name control."
    Assert-Regex $initializeComponent "(?im)\brootLayoutPanel\.Controls\.Add\(\s*(?:Me\.)?$name\s*,\s*0\s*,\s*$row\s*\)" "Designer must place $name in rootLayoutPanel row $($row + 1)."
}

$designerControls = @(
    @{ Name = 'btnSelectAll'; Type = 'Button'; Caption = '全选'; Owner = 'commandPanel' },
    @{ Name = 'btnClearSelection'; Type = 'Button'; Caption = '清空'; Owner = 'commandPanel' },
    @{ Name = 'btnInvertSelection'; Type = 'Button'; Caption = '反选'; Owner = 'commandPanel' },
    @{ Name = 'btnSelectG1'; Type = 'Button'; Caption = 'G1全选'; Owner = 'commandPanel' },
    @{ Name = 'btnSelectG2'; Type = 'Button'; Caption = 'G2全选'; Owner = 'commandPanel' },
    @{ Name = 'btnClampClose'; Type = 'Button'; Caption = '夹具压合'; Owner = 'commandPanel' },
    @{ Name = 'btnClampOpen'; Type = 'Button'; Caption = '夹具松开'; Owner = 'commandPanel' },
    @{ Name = 'lblRangeStart'; Type = 'Label'; Caption = '起始'; Owner = 'commandPanel' },
    @{ Name = 'txtRangeStart'; Type = 'TextBox'; Caption = $null; Owner = 'commandPanel' },
    @{ Name = 'lblRangeEnd'; Type = 'Label'; Caption = '结束'; Owner = 'commandPanel' },
    @{ Name = 'txtRangeEnd'; Type = 'TextBox'; Caption = $null; Owner = 'commandPanel' },
    @{ Name = 'btnAddRange'; Type = 'Button'; Caption = '添加范围'; Owner = 'commandPanel' },
    @{ Name = 'lblChannelList'; Type = 'Label'; Caption = '零星通道'; Owner = 'commandPanel' },
    @{ Name = 'txtChannelList'; Type = 'TextBox'; Caption = $null; Owner = 'commandPanel' },
    @{ Name = 'btnAddList'; Type = 'Button'; Caption = '添加列表'; Owner = 'commandPanel' },
    @{ Name = 'lblSummary'; Type = 'Label'; Caption = $null; Owner = 'commandPanel' },
    @{ Name = 'lblStatus'; Type = 'Label'; Caption = '空闲'; Owner = 'bottomPanel' },
    @{ Name = 'lblRepeatCount'; Type = 'Label'; Caption = '重复次数'; Owner = 'bottomPanel' },
    @{ Name = 'nudRepeatCount'; Type = 'NumericUpDown'; Caption = $null; Owner = 'bottomPanel' },
    @{ Name = 'btnMultiple'; Type = 'Button'; Caption = '多次测试'; Owner = 'bottomPanel' },
    @{ Name = 'btnExport'; Type = 'Button'; Caption = '导出CSV'; Owner = 'bottomPanel' },
    @{ Name = 'btnStart'; Type = 'Button'; Caption = '开始测试'; Owner = 'bottomPanel' },
    @{ Name = 'btnClose'; Type = 'Button'; Caption = '关闭'; Owner = 'bottomPanel' }
)
foreach ($control in $designerControls) {
    $name = $control.Name
    $type = $control.Type
    $owner = $control.Owner
    Assert-Regex $initializeComponent "(?im)^\s*(?:Me\.)?$name\s*=\s*New\s+(?:System\.Windows\.Forms\.)?$type\b" "Designer must create $name as $type."
    $namePattern = '(?im)\b(?:Me\.)?' + [regex]::Escape($name) + '\.Name\s*=\s*' + $doubleQuote + [regex]::Escape($name) + $doubleQuote
    Assert-Regex $initializeComponent $namePattern "Designer must name $name."
    if ($null -ne $control.Caption) {
        $captionPattern = '(?im)\b(?:Me\.)?' + [regex]::Escape($name) + '\.Text\s*=\s*"' + [regex]::Escape($control.Caption) + '"'
        Assert-Regex $initializeComponent $captionPattern "Designer must preserve $name caption '$($control.Caption)'."
    }

    $directOwnerPattern = "(?im)\b(?:Me\.)?$owner\.Controls\.Add\(\s*(?:Me\.)?$name(?:\s*,\s*\d+\s*,\s*\d+)?\s*\)"
    $addedDirectly = [System.Text.RegularExpressions.Regex]::IsMatch($initializeComponent, $directOwnerPattern)
    $ownerArrayPattern = '(?is)\b(?:Me\.)?' + [regex]::Escape($owner) + '\.Controls\.AddRange\s*\(.*?\{(?<controls>[^}]*)\}'
    $ownerArrays = [System.Text.RegularExpressions.Regex]::Matches($initializeComponent, $ownerArrayPattern)
    $addedByRange = $false
    foreach ($ownerArray in $ownerArrays) {
        $memberPattern = '(?im)(?:Me\.)?' + [regex]::Escape($name) + '\b'
        if ([System.Text.RegularExpressions.Regex]::IsMatch($ownerArray.Groups['controls'].Value, $memberPattern)) {
            $addedByRange = $true
            break
        }
    }
    if (-not $addedDirectly -and -not $addedByRange) {
        throw "Designer must add $name to its owning panel $owner."
    }
}

$expectedHeaders = @('通道', '分组', '电压 mV', '内阻 mΩ', '判定')
$directColumnAdds = [System.Text.RegularExpressions.Regex]::Matches(
    $initializeComponent,
    '(?im)\bresultGrid\.Columns\.Add\(\s*"[^"]+"\s*,\s*"(?<header>[^"]*)"\s*\)'
)
$addRangePattern = '(?is)\bresultGrid\.Columns\.AddRange\s*\(.*?\{(?<columns>[^}]*)\}'
$addRangeMatches = [System.Text.RegularExpressions.Regex]::Matches($initializeComponent, $addRangePattern)
if ($directColumnAdds.Count -eq 0 -and $addRangeMatches.Count -eq 0) {
    throw 'Designer must add columns to resultGrid directly or through Columns.AddRange.'
}
$includedColumnNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($addRange in $addRangeMatches) {
    $columnReferences = [System.Text.RegularExpressions.Regex]::Matches(
        $addRange.Groups['columns'].Value,
        '(?:Me\.)?(?<name>[A-Za-z_]\w*)'
    )
    foreach ($reference in $columnReferences) {
        [void]$includedColumnNames.Add($reference.Groups['name'].Value)
    }
}
$headerAssignments = [System.Text.RegularExpressions.Regex]::Matches(
    $initializeComponent,
    '(?im)\b(?:Me\.)?(?<column>[A-Za-z_]\w*)\.HeaderText\s*=\s*"(?<header>[^"]*)"'
)
foreach ($header in $expectedHeaders) {
    $isDirectResultGridHeader = $false
    foreach ($columnAdd in $directColumnAdds) {
        if ($columnAdd.Groups['header'].Value -ceq $header) {
            $isDirectResultGridHeader = $true
            break
        }
    }

    $isIncludedResultGridHeader = $false
    foreach ($assignment in $headerAssignments) {
        if ($assignment.Groups['header'].Value -ceq $header -and
            $includedColumnNames.Contains($assignment.Groups['column'].Value)) {
            $isIncludedResultGridHeader = $true
            break
        }
    }

    if (-not $isDirectResultGridHeader -and -not $isIncludedResultGridHeader) {
        throw "Designer resultGrid must include a column with HeaderText '$header'."
    }
}

$selectableButtonEvents = @(
    @{ Button = 'btnSelectAll'; Handler = 'SelectAllChannels' },
    @{ Button = 'btnClearSelection'; Handler = 'ClearSelection' },
    @{ Button = 'btnInvertSelection'; Handler = 'InvertSelection' },
    @{ Button = 'btnSelectG1'; Handler = 'SelectG1Channels' },
    @{ Button = 'btnSelectG2'; Handler = 'SelectG2Channels' },
    @{ Button = 'btnAddRange'; Handler = 'AddRangeSelection' },
    @{ Button = 'btnAddList'; Handler = 'AddListSelection' }
)
foreach ($event in $selectableButtonEvents) {
    $eventPattern = '(?im)\bAddHandler\s+(?:Me\.)?' + [regex]::Escape($event.Button) + '\.Click\s*,\s*AddressOf\s+(?:Me\.)?' + [regex]::Escape($event.Handler) + '\b'
    Assert-Regex $initializeComponent $eventPattern "Designer must connect $($event.Button).Click to $($event.Handler)."
}

$clampButtonEvents = @(
    @{ Button = 'btnClampClose'; Handler = 'ClampClose' },
    @{ Button = 'btnClampOpen'; Handler = 'ClampOpen' }
)
foreach ($event in $clampButtonEvents) {
    $eventPattern = '(?im)\bAddHandler\s+(?:Me\.)?' + [regex]::Escape($event.Button) + '\.Click\s*,\s*AddressOf\s+(?:Me\.)?' + [regex]::Escape($event.Handler) + '\b'
    Assert-Regex $initializeComponent $eventPattern "Designer must connect $($event.Button).Click to $($event.Handler)."
}
$bottomButtonEvents = @(
    @{ Button = 'btnMultiple'; Handler = 'StartMultipleManualTests' },
    @{ Button = 'btnExport'; Handler = 'ExportManualTestResults' },
    @{ Button = 'btnStart'; Handler = 'StartManualTest' }
)
foreach ($event in $bottomButtonEvents) {
    $eventPattern = '(?im)\bAddHandler\s+(?:Me\.)?' + [regex]::Escape($event.Button) + '\.Click\s*,\s*AddressOf\s+(?:Me\.)?' + [regex]::Escape($event.Handler) + '\b'
    Assert-Regex $initializeComponent $eventPattern "Designer must connect $($event.Button).Click to $($event.Handler)."
}
Assert-Regex $initializeComponent '(?im)\bAddHandler\s+(?:Me\.)?btnClose\.Click\s*,\s*Sub\s*\([^)]*\)[ \t]*Me\.Close\s*\(\s*\)' 'Designer must wire btnClose.Click to a lambda that calls Me.Close().'

$disposeMethod = [System.Text.RegularExpressions.Regex]::Match(
    $classBody,
    '(?ims)^\s*Protected\s+Overrides\s+Sub\s+Dispose\s*\(\s*(?:ByVal\s+)?disposing\s+As\s+Boolean\s*\)(?<body>.*?)^\s*End\s+Sub\b'
)
if (-not $disposeMethod.Success) {
    throw 'Designer must override Dispose(disposing As Boolean).'
}
$disposeBody = $disposeMethod.Groups['body'].Value
$singleLineDispose = [System.Text.RegularExpressions.Regex]::IsMatch(
    $disposeBody,
    '(?im)\bIf\s+disposing\b[^\r\n]*\bcomponents\.Dispose\s*\(\s*\)'
)
$disposeGuard = [System.Text.RegularExpressions.Regex]::Match($disposeBody, '(?im)\bIf\s+disposing\b')
$disposeCall = [System.Text.RegularExpressions.Regex]::Match($disposeBody, '(?im)\bcomponents\.Dispose\s*\(\s*\)')
$endIfAfterGuard = $null
if ($disposeGuard.Success) {
    $endIfAfterGuard = [System.Text.RegularExpressions.Regex]::Match(
        $disposeBody.Substring($disposeGuard.Index + $disposeGuard.Length),
        '(?im)^\s*End\s+If\b'
    )
}
$blockDispose = $disposeGuard.Success -and $disposeCall.Success -and $null -ne $endIfAfterGuard -and
    $endIfAfterGuard.Success -and
    $disposeCall.Index -lt ($disposeGuard.Index + $disposeGuard.Length + $endIfAfterGuard.Index)
if (-not $singleLineDispose -and -not $blockDispose) {
    throw 'Designer Dispose must call components.Dispose() inside the disposing guard.'
}

$constructor = [System.Text.RegularExpressions.Regex]::Match(
    $codeBehind,
    '(?ims)^\s*Public\s+Sub\s+New\s*\(\s*\)(?<body>.*?)^\s*End\s+Sub\b'
)
if (-not $constructor.Success) {
    throw 'FrmManualTest must define a Public parameterless constructor.'
}
$constructorBody = $constructor.Groups['body'].Value
$constructorCalls = @()
foreach ($call in @('InitializeComponent', 'PopulateChannelMatrix', 'UpdateSelectionSummary')) {
    $callMatch = [System.Text.RegularExpressions.Regex]::Match(
        $constructorBody,
        '(?im)^\s*(?:Me\.)?' + [regex]::Escape($call) + '\s*\(\s*\)\s*(?:''.*)?$'
    )
    if (-not $callMatch.Success) {
        throw "FrmManualTest constructor must call $call."
    }
    $constructorCalls += $callMatch.Index
}
if (-not ($constructorCalls[0] -lt $constructorCalls[1] -and $constructorCalls[1] -lt $constructorCalls[2])) {
    throw 'FrmManualTest constructor must call InitializeComponent, PopulateChannelMatrix, then UpdateSelectionSummary in that order.'
}

$removedBuilders = @(
    'InitializeManualForm',
    'BuildCommandPanel',
    'BuildMatrixPanel',
    'BuildResultGrid',
    'BuildBottomPanel',
    'CreateButton'
)
foreach ($method in $removedBuilders) {
    if ([System.Text.RegularExpressions.Regex]::IsMatch(
        $codeBehind,
        "(?im)^\s*(?:(?:Private|Protected|Friend|Public)\s+)*(?:Overloads\s+|Shared\s+|Overrides\s+)*(?:Sub|Function)\s+$method\b"
    )) {
        throw "Fixed UI builder $method must be removed from FrmManualTest.vb."
    }
}

$namespaceManager = [System.Xml.XmlNamespaceManager]::new($project.NameTable)
$namespaceManager.AddNamespace('msb', 'http://schemas.microsoft.com/developer/msbuild/2003')
$designerCompile = $project.SelectSingleNode(
    "/msb:Project/msb:ItemGroup/msb:Compile[@Include='FrmManualTest.Designer.vb']",
    $namespaceManager
)
if ($null -eq $designerCompile) {
    throw 'Project must compile FrmManualTest.Designer.vb.'
}
$dependentUpon = $designerCompile.SelectSingleNode('msb:DependentUpon', $namespaceManager)
if ($null -eq $dependentUpon -or $dependentUpon.InnerText -ne 'FrmManualTest.vb') {
    throw 'FrmManualTest.Designer.vb must be nested under FrmManualTest.vb with DependentUpon.'
}
$codeBehindCompile = $project.SelectSingleNode(
    "/msb:Project/msb:ItemGroup/msb:Compile[@Include='FrmManualTest.vb']",
    $namespaceManager
)
$subType = if ($null -eq $codeBehindCompile) { $null } else { $codeBehindCompile.SelectSingleNode('msb:SubType', $namespaceManager) }
if ($null -eq $subType -or $subType.InnerText -ne 'Form') {
    throw 'FrmManualTest.vb must retain SubType Form in the project.'
}

$runtimeRequired = $RequireRuntime.IsPresent -or [bool]$env:FRMTEST_EXE
$runtimeExe = if ($env:FRMTEST_EXE) {
    $env:FRMTEST_EXE
} else {
    Join-Path $repoRoot 'bin\AnyCPU\XL_OCV_256CH.exe'
}
if (-not (Test-Path -LiteralPath $runtimeExe -PathType Leaf)) {
    if ($env:FRMTEST_EXE) {
        $runtimeUnavailableReason = "FRMTEST_EXE does not exist: $runtimeExe"
    } else {
        $x86Candidate = Join-Path $repoRoot 'bin\Debug\XL_OCV_256CH.exe'
        $x86OnlyAvailable = $false
        if (Test-Path -LiteralPath $x86Candidate -PathType Leaf) {
            try {
                $x86CandidateName = [Reflection.AssemblyName]::GetAssemblyName((Resolve-Path -LiteralPath $x86Candidate).Path)
                $x86OnlyAvailable = $x86CandidateName.ProcessorArchitecture.ToString() -eq 'X86'
            } catch {
                $x86OnlyAvailable = $false
            }
        }

        if ($x86OnlyAvailable) {
            $runtimeUnavailableReason = "AnyCPU runtime assembly is missing at $runtimeExe; only an x86 assembly is available at $x86Candidate."
        } else {
            $runtimeUnavailableReason = "Runtime assembly is missing at $runtimeExe."
        }
    }

    if ($runtimeRequired) {
        throw "$runtimeUnavailableReason Build the AnyCPU configuration or set FRMTEST_EXE to a compatible assembly."
    }
    Write-Output "SKIP: $runtimeUnavailableReason Build AnyCPU or set FRMTEST_EXE to run runtime checks."
    Write-Output 'PASS: static FrmManualTest Designer contract verified.'
    return
}

$resolvedAssemblyPath = (Resolve-Path -LiteralPath $runtimeExe).Path
$runtimeAssemblyName = [Reflection.AssemblyName]::GetAssemblyName($resolvedAssemblyPath)
if ([Environment]::Is64BitProcess -and $runtimeAssemblyName.ProcessorArchitecture.ToString() -eq 'X86') {
    $x86Reason = "Runtime assembly is x86-only: $resolvedAssemblyPath."
    if ($runtimeRequired) {
        throw "$x86Reason Use an AnyCPU build or set FRMTEST_EXE to an AnyCPU assembly."
    }
    Write-Output "SKIP: $x86Reason Use an AnyCPU build or set FRMTEST_EXE to run runtime checks."
    Write-Output 'PASS: static FrmManualTest Designer contract verified.'
    return
}

$assemblyWriteTime = (Get-Item -LiteralPath $resolvedAssemblyPath).LastWriteTimeUtc
$sourcePaths = @(
    $codeBehindPath,
    $designerPath,
    $projectPath
)
$staleSourcePaths = @(
    foreach ($sourcePath in $sourcePaths) {
        $sourceWriteTime = (Get-Item -LiteralPath $sourcePath).LastWriteTimeUtc
        if ($sourceWriteTime -gt $assemblyWriteTime) {
            $sourcePath
        }
    }
)
if ($staleSourcePaths.Count -gt 0) {
    $staleReason = "Runtime assembly $resolvedAssemblyPath is older than source files: $($staleSourcePaths -join ', ')."
    if ($runtimeRequired) {
        throw "$staleReason Rebuild AnyCPU or point FRMTEST_EXE to a fresh assembly."
    }
    Write-Output "SKIP: $staleReason Rebuild AnyCPU or set FRMTEST_EXE to run runtime checks."
    Write-Output 'PASS: static FrmManualTest Designer contract verified.'
    return
}
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
try {
    $assembly = [Reflection.Assembly]::LoadFrom($resolvedAssemblyPath)
} catch [System.BadImageFormatException] {
    throw "Could not load runtime assembly $resolvedAssemblyPath; it may be x86-only in this 64-bit PowerShell host. Use AnyCPU or set FRMTEST_EXE to a compatible assembly."
}
$formType = $assembly.GetType('software.FrmManualTest', $false)
if ($null -eq $formType) {
    throw "Built assembly does not contain software.FrmManualTest: $resolvedAssemblyPath"
}

$form = $null
try {
    $form = [Activator]::CreateInstance($formType)
    $form.ShowInTaskbar = $false
    $form.Opacity = 0
    $form.Show()
    [System.Windows.Forms.Application]::DoEvents()

    if ($form.Text -ne '手动测试' -or $form.Width -ne 1475 -or $form.Height -ne 975) {
        throw "Runtime form settings were $($form.Text), $($form.Width)x$($form.Height); expected 手动测试, 1475x975."
    }
    if ($form.MinimumSize.Width -ne 1225 -or $form.MinimumSize.Height -ne 850) {
        throw "Runtime minimum size was $($form.MinimumSize.Width)x$($form.MinimumSize.Height); expected 1225x850."
    }

    $rootLayoutPanel = Find-ControlByName -Parent $form -Name 'rootLayoutPanel'
    if ($null -eq $rootLayoutPanel -or -not ($rootLayoutPanel -is [System.Windows.Forms.TableLayoutPanel])) {
        throw 'Runtime form must contain the rootLayoutPanel TableLayoutPanel.'
    }
    if ($rootLayoutPanel.RowCount -ne 4 -or $rootLayoutPanel.RowStyles.Count -ne 4) {
        throw "Runtime rootLayoutPanel has $($rootLayoutPanel.RowCount) rows and $($rootLayoutPanel.RowStyles.Count) row styles; expected four of each."
    }

    $matrixPanel = Find-ControlByName -Parent $form -Name 'matrixPanel'
    if ($null -eq $matrixPanel -or -not ($matrixPanel -is [System.Windows.Forms.TableLayoutPanel])) {
        throw 'Runtime form must contain the matrixPanel TableLayoutPanel.'
    }
    if ($matrixPanel.ColumnCount -ne 16 -or $matrixPanel.RowCount -ne 16) {
        throw "Runtime matrixPanel is $($matrixPanel.ColumnCount)x$($matrixPanel.RowCount); expected 16x16."
    }
    $channelBoxes = @{}
    foreach ($control in $matrixPanel.Controls) {
        if ($control -is [System.Windows.Forms.CheckBox]) {
            $channel = [int]$control.Tag
            if ($channel -lt 1 -or $channel -gt 256 -or $channelBoxes.ContainsKey($channel)) {
                throw "Runtime matrixPanel contains an invalid or duplicate channel CheckBox tag: $channel."
            }
            $channelBoxes[$channel] = $control
        }
    }
    if ($matrixPanel.Controls.Count -ne 256 -or $channelBoxes.Count -ne 256) {
        throw "Runtime matrixPanel contains $($matrixPanel.Controls.Count) controls and $($channelBoxes.Count) tagged CheckBoxes; expected exactly 256."
    }

    $resultGrid = Find-ControlByName -Parent $form -Name 'resultGrid'
    if ($null -eq $resultGrid -or -not ($resultGrid -is [System.Windows.Forms.DataGridView])) {
        throw 'Runtime form must contain the resultGrid DataGridView.'
    }
    $actualHeaders = @($resultGrid.Columns | ForEach-Object { $_.HeaderText })
    if ($actualHeaders.Count -ne $expectedHeaders.Count) {
        throw "Runtime resultGrid has $($actualHeaders.Count) columns; expected five."
    }
    for ($index = 0; $index -lt $expectedHeaders.Count; $index++) {
        if ($actualHeaders[$index] -ne $expectedHeaders[$index]) {
            throw "Runtime resultGrid column $($index + 1) is '$($actualHeaders[$index])'; expected '$($expectedHeaders[$index])'."
        }
    }

    $btnSelectAll = Find-ControlByName -Parent $form -Name 'btnSelectAll'
    $btnClearSelection = Find-ControlByName -Parent $form -Name 'btnClearSelection'
    $btnInvertSelection = Find-ControlByName -Parent $form -Name 'btnInvertSelection'
    $btnAddRange = Find-ControlByName -Parent $form -Name 'btnAddRange'
    $btnAddList = Find-ControlByName -Parent $form -Name 'btnAddList'
    $txtRangeStart = Find-ControlByName -Parent $form -Name 'txtRangeStart'
    $txtRangeEnd = Find-ControlByName -Parent $form -Name 'txtRangeEnd'
    $txtChannelList = Find-ControlByName -Parent $form -Name 'txtChannelList'
    foreach ($buttonName in @('btnSelectAll', 'btnClearSelection', 'btnInvertSelection', 'btnAddRange', 'btnAddList')) {
        $button = Find-ControlByName -Parent $form -Name $buttonName
        if ($null -eq $button -or -not ($button -is [System.Windows.Forms.Button])) {
            throw "Runtime form must contain the $buttonName Button."
        }
    }
    foreach ($textBoxName in @('txtRangeStart', 'txtRangeEnd', 'txtChannelList')) {
        $textBox = Find-ControlByName -Parent $form -Name $textBoxName
        if ($null -eq $textBox -or -not ($textBox -is [System.Windows.Forms.TextBox])) {
            throw "Runtime form must contain the $textBoxName TextBox."
        }
    }

    $btnSelectAll.PerformClick()
    Assert-CheckedChannels -ChannelBoxes $channelBoxes -Expected ([int[]](1..256)) -Scenario 'Select all'
    $btnClearSelection.PerformClick()
    Assert-CheckedChannels -ChannelBoxes $channelBoxes -Expected ([int[]]@()) -Scenario 'Clear selection'

    $channelBoxes[1].Checked = $true
    $channelBoxes[256].Checked = $true
    $btnInvertSelection.PerformClick()
    Assert-CheckedChannels -ChannelBoxes $channelBoxes -Expected ([int[]](2..255)) -Scenario 'Invert selection'
    $btnClearSelection.PerformClick()

    $txtRangeStart.Text = '5'
    $txtRangeEnd.Text = '8'
    $btnAddRange.PerformClick()
    Assert-CheckedChannels -ChannelBoxes $channelBoxes -Expected ([int[]](5..8)) -Scenario 'Range selection'
    $btnClearSelection.PerformClick()

    $txtChannelList.Text = '1, 4, 256'
    $btnAddList.PerformClick()
    Assert-CheckedChannels -ChannelBoxes $channelBoxes -Expected ([int[]]@(1, 4, 256)) -Scenario 'List selection'
    $btnClearSelection.PerformClick()
} finally {
    if ($null -ne $form) {
        $form.Dispose()
    }
}

Write-Output "PASS: FrmManualTest Designer contract and runtime layout/selection behavior verified from $resolvedAssemblyPath."
