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

$designer = Get-Content -LiteralPath $designerPath -Raw
$codeBehindPath = Join-Path $repoRoot 'FrmManualTest.vb'
$codeBehind = Get-Content -LiteralPath $codeBehindPath -Raw
$projectPath = Join-Path $repoRoot 'MP-60A-RV-64Mem.vbproj'
$project = [xml](Get-Content -LiteralPath $projectPath -Raw)

Assert-Regex $designer '(?im)^\s*(?:Partial\s+Public|Public\s+Partial|Partial)\s+Class\s+FrmManualTest\b' 'Designer must declare partial FrmManualTest.'
Assert-Regex $designer '(?im)^\s*Inherits\s+System\.Windows\.Forms\.Form\s*$' 'Designer FrmManualTest must inherit System.Windows.Forms.Form.'
Assert-Regex $designer '(?im)^\s*(?:(?:Private|Protected|Friend|Public)\s+)?Sub\s+InitializeComponent\s*\(' 'Designer must define InitializeComponent.'

Assert-Regex $designer '(?im)\bMe\.Text\s*=\s*"手动测试"' 'Designer must preserve the 手动测试 form title.'
Assert-Regex $designer '(?im)\bMe\.Size\s*=\s*New\s+(?:System\.Drawing\.)?Size\s*\(\s*1475\s*,\s*975\s*\)' 'Designer must preserve the 1475 x 975 form size.'
Assert-Regex $designer '(?im)\bMe\.MinimumSize\s*=\s*New\s+(?:System\.Drawing\.)?Size\s*\(\s*1225\s*,\s*850\s*\)' 'Designer must preserve the 1225 x 850 minimum size.'

Assert-Regex $designer '(?im)^\s*(?:Me\.)?rootLayoutPanel\s*=\s*New\s+(?:System\.Windows\.Forms\.)?TableLayoutPanel\b' 'Designer must create rootLayoutPanel as a TableLayoutPanel.'
Assert-Regex $designer '(?im)\b(?:Me\.)?rootLayoutPanel\.RowCount\s*=\s*4\b' 'Designer rootLayoutPanel must have four rows.'
$quoteClass = '[' + '"' + "'" + ']'
$rootNamePattern = '(?im)\b(?:Me\.)?rootLayoutPanel\.Name\s*=\s*' + $quoteClass + 'rootLayoutPanel' + $quoteClass
Assert-Regex $designer $rootNamePattern 'Designer must name the root table rootLayoutPanel.'

$staticControls = @(
    @{ Name = 'commandPanel'; Type = 'FlowLayoutPanel'; Row = 0 },
    @{ Name = 'matrixPanel'; Type = 'TableLayoutPanel'; Row = 1 },
    @{ Name = 'resultGrid'; Type = 'DataGridView'; Row = 2 },
    @{ Name = 'bottomPanel'; Type = 'TableLayoutPanel'; Row = 3 }
)
foreach ($control in $staticControls) {
    $name = $control.Name
    $type = $control.Type
    $row = $control.Row
    Assert-Regex $designer "(?im)^\s*(?:Me\.)?$name\s*=\s*New\s+(?:System\.Windows\.Forms\.)?$type\b" "Designer must create $name as $type."
    $namePattern = '(?im)\b(?:Me\.)?' + [regex]::Escape($name) + '\.Name\s*=\s*' + $quoteClass + [regex]::Escape($name) + $quoteClass
    Assert-Regex $designer $namePattern "Designer must name the $name control."
    Assert-Regex $designer "(?im)\brootLayoutPanel\.Controls\.Add\(\s*(?:Me\.)?$name\s*,\s*0\s*,\s*$row\s*\)" "Designer must place $name in rootLayoutPanel row $($row + 1)."
}

$expectedHeaders = @('通道', '分组', '电压 mV', '内阻 mΩ', '判定')
$directColumnAdds = [System.Text.RegularExpressions.Regex]::Matches(
    $designer,
    '(?im)\bresultGrid\.Columns\.Add\(\s*"[^"]+"\s*,\s*"(?<header>[^"]*)"\s*\)'
)
$addRangePattern = '(?is)\bresultGrid\.Columns\.AddRange\s*\(.*?\{(?<columns>[^}]*)\}'
$addRangeMatches = [System.Text.RegularExpressions.Regex]::Matches($designer, $addRangePattern)
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
    $designer,
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
$constructor = [System.Text.RegularExpressions.Regex]::Match(
    $codeBehind,
    '(?ims)^\s*Public\s+Sub\s+New\s*\(\s*\)(?<body>.*?)^\s*End\s+Sub'
)
if (-not $constructor.Success) {
    throw 'FrmManualTest must define a Public parameterless constructor.'
}
foreach ($call in @('InitializeComponent', 'PopulateChannelMatrix', 'UpdateSelectionSummary')) {
    Assert-Regex $constructor.Groups['body'].Value "(?im)^\s*(?:Me\.)?$call\s*\(\s*\)\s*$" "FrmManualTest constructor must call $call."
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

$runtimeExe = if ($env:FRMTEST_EXE) {
    $env:FRMTEST_EXE
} else {
    Join-Path $repoRoot 'bin\AnyCPU\XL_OCV_256CH.exe'
}
if ($env:FRMTEST_EXE -and -not (Test-Path -LiteralPath $runtimeExe -PathType Leaf)) {
    throw "FRMTEST_EXE does not exist: $runtimeExe"
}

if (Test-Path -LiteralPath $runtimeExe -PathType Leaf) {
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    $resolvedAssemblyPath = (Resolve-Path -LiteralPath $runtimeExe).Path
    $assembly = [Reflection.Assembly]::LoadFrom($resolvedAssemblyPath)
    $formType = $assembly.GetType('software.FrmManualTest', $false)
    if ($null -eq $formType) {
        throw "Built assembly does not contain software.FrmManualTest: $resolvedAssemblyPath"
    }

    function Find-ControlByName {
        param(
            [System.Windows.Forms.Control]$Parent,
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

    $form = $null
    try {
        $form = [Activator]::CreateInstance($formType)
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
        if ($rootLayoutPanel.RowCount -ne 4) {
            throw "Runtime rootLayoutPanel has $($rootLayoutPanel.RowCount) rows; expected 4."
        }

        $matrixPanel = Find-ControlByName -Parent $form -Name 'matrixPanel'
        if ($null -eq $matrixPanel -or -not ($matrixPanel -is [System.Windows.Forms.TableLayoutPanel])) {
            throw 'Runtime form must contain the matrixPanel TableLayoutPanel.'
        }
        if ($matrixPanel.ColumnCount -ne 16 -or $matrixPanel.RowCount -ne 16) {
            throw "Runtime matrixPanel is $($matrixPanel.ColumnCount)x$($matrixPanel.RowCount); expected 16x16."
        }
        $matrixCheckBoxes = @($matrixPanel.Controls | Where-Object { $_ -is [System.Windows.Forms.CheckBox] })
        if ($matrixPanel.Controls.Count -ne 256 -or $matrixCheckBoxes.Count -ne 256) {
            throw "Runtime matrixPanel contains $($matrixPanel.Controls.Count) controls and $($matrixCheckBoxes.Count) CheckBoxes; expected exactly 256 CheckBoxes."
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
    } finally {
        if ($null -ne $form) {
            $form.Dispose()
        }
    }

    Write-Output "PASS: runtime FrmManualTest layout verified from $resolvedAssemblyPath."
} else {
    Write-Output "SKIP: runtime assembly not found at $runtimeExe; set FRMTEST_EXE to verify the built form."
}

Write-Output 'FrmManualTest Designer contract passed.'
