$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$requiredFiles = @(
    'DebugModels.vb',
    'DebugProbeService.vb',
    'DebugDashboardControl.vb'
)

foreach ($file in $requiredFiles) {
    $path = Join-Path $root $file
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing debug dashboard file: $file"
    }
}

$models = Get-Content -LiteralPath (Join-Path $root 'DebugModels.vb') -Raw
$probe = Get-Content -LiteralPath (Join-Path $root 'DebugProbeService.vb') -Raw
$dashboard = Get-Content -LiteralPath (Join-Path $root 'DebugDashboardControl.vb') -Raw
$hostSource = Get-Content -LiteralPath (Join-Path $root 'frmDebug.vb') -Raw
$project = Get-Content -LiteralPath (Join-Path $root 'MP-60A-RV-64Mem.vbproj') -Raw

@(
    @{ Text = $models; Pattern = 'Public Enum DebugDeviceId'; Name = 'device model enum' },
    @{ Text = $models; Pattern = 'Public Class DebugDeviceResult'; Name = 'device result model' },
    @{ Text = $probe; Pattern = 'Class DebugProbeService'; Name = 'probe service' },
    @{ Text = $probe; Pattern = 'Function ProbeAll'; Name = 'probe all entry point' },
    @{ Text = $probe; Pattern = 'ConnectFor3561'; Name = '3562 probe adapter' },
    @{ Text = $probe; Pattern = 'CheckCom_34461'; Name = '34461 probe adapter' },
    @{ Text = $probe; Pattern = 'plcController'; Name = 'PLC probe adapter' },
    @{ Text = $dashboard; Pattern = 'Class DebugDashboardControl'; Name = 'dashboard control' },
    @{ Text = $dashboard; Pattern = 'TabDeviceOverview'; Name = 'device overview tab' },
    @{ Text = $dashboard; Pattern = 'TabCommunicationConsole'; Name = 'communication console tab' },
    @{ Text = $dashboard; Pattern = 'TabManualControl'; Name = 'manual control tab' },
    @{ Text = $dashboard; Pattern = 'TabRuntimeLog'; Name = 'runtime log tab' },
    @{ Text = $dashboard; Pattern = 'ParsePayload'; Name = 'payload parser' },
    @{ Text = $dashboard; Pattern = 'SetRunningState'; Name = 'running-state guard' },
    @{ Text = $dashboard; Pattern = 'MessageBox.Show'; Name = 'dangerous-action confirmation' },
    @{ Text = $hostSource; Pattern = 'InitializeModernDashboard'; Name = 'host mount point' },
    @{ Text = $hostSource; Pattern = 'legacyControl.Visible = False'; Name = 'legacy UI hiding' },
    @{ Text = $hostSource; Pattern = 'modernDashboard = New DebugDashboardControl'; Name = 'dashboard host construction' },
    @{ Text = $probe; Pattern = 'CheckCom_SCM'; Name = 'SCM probe adapter' },
    @{ Text = $hostSource; Pattern = 'ReadAvailableSerialBytes'; Name = 'raw serial receive adapter' },
    @{ Text = $hostSource; Pattern = 'EnsurePlcConnected'; Name = 'PLC manual adapter' },
    @{ Text = $project; Pattern = '<Compile Include="DebugModels.vb"'; Name = 'models project registration' },
    @{ Text = $project; Pattern = '<Compile Include="DebugProbeService.vb"'; Name = 'probe project registration' },
    @{ Text = $project; Pattern = '<Compile Include="DebugDashboardControl.vb"'; Name = 'dashboard project registration' }
) | ForEach-Object {
    if ($_.Text -notmatch [regex]::Escape($_.Pattern)) {
        throw "Missing $($_.Name): $($_.Pattern)"
    }
}

if ($dashboard -match 'Button[0-9]+') {
    throw 'Dashboard contains temporary button names.'
}

Write-Output 'Debug dashboard contract passed.'
