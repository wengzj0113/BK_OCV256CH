param([string]$AssemblyPath)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Add-Type -AssemblyName System.Drawing
$assemblyPath = if ([string]::IsNullOrWhiteSpace($AssemblyPath)) { Join-Path $root 'bin\AnyCPU\Debug\XL_OCV_UIControls.dll' } else { $AssemblyPath }
if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "找不到 UIControls 构建产物: $assemblyPath"
}

$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$paletteType = $assembly.GetType('software.ChannelStatusColors', $true)
$kindType = $assembly.GetType('software.ChannelStatusKind', $true)
$palette = [Activator]::CreateInstance(
    $paletteType,
    [object[]]@(
        [Drawing.Color]::FromArgb(255, 1, 2, 3),
        [Drawing.Color]::FromArgb(255, 4, 5, 6),
        [Drawing.Color]::FromArgb(255, 7, 8, 9),
        [Drawing.Color]::FromArgb(255, 10, 11, 12),
        [Drawing.Color]::FromArgb(255, 13, 14, 15),
        [Drawing.Color]::FromArgb(255, 16, 17, 18)))
$forStatus = $paletteType.GetMethod('ForStatus')
$expected = @{
    Pass = @(1, 2, 3)
    Testing = @(4, 5, 6)
    Idle = @(7, 8, 9)
    ResistanceNg = @(10, 11, 12)
    VoltageNg = @(13, 14, 15)
}

foreach ($name in $expected.Keys) {
    $foreground = [Drawing.Color]::Black
    $kind = [Enum]::Parse($kindType, $name)
    $result = $forStatus.Invoke($null, [object[]]@($kind, $palette, $foreground))
    $rgb = @($result.R, $result.G, $result.B)
    if (($rgb -join ',') -ne ($expected[$name] -join ',')) {
        throw "$name 颜色映射错误: $($rgb -join ',')"
    }
}

$selected = $palette.SelectedColor
$selectedRgb = @($selected.R, $selected.G, $selected.B)
if (($selectedRgb -join ',') -ne '16,17,18') {
    throw "Selected 颜色映射错误: $($selectedRgb -join ',')"
}

Write-Output 'Channel color runtime mapping: PASS'
