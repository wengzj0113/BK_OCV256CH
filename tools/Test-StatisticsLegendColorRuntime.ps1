$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$assemblyPath = Join-Path $root 'bin\AnyCPU\Debug\XL_OCV_UIControls.dll'
if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "找不到 UIControls 构建产物: $assemblyPath"
}

$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$controlType = $assembly.GetType('software.StatisticsControl', $true)
$paletteType = $assembly.GetType('software.ChannelStatusColors', $true)
$applyMethod = $controlType.GetMethod('ApplyStatusColors')
if ($null -eq $applyMethod) {
    throw 'StatisticsControl 未提供 ApplyStatusColors，图例无法同步配置颜色。'
}

$palette = [Activator]::CreateInstance(
    $paletteType,
    [object[]]@(
        [Drawing.Color]::FromArgb(255, 1, 2, 3),
        [Drawing.Color]::FromArgb(255, 4, 5, 6),
        [Drawing.Color]::FromArgb(255, 7, 8, 9),
        [Drawing.Color]::FromArgb(255, 10, 11, 12),
        [Drawing.Color]::FromArgb(255, 13, 14, 15)))

$control = [Activator]::CreateInstance($controlType)
try {
    $applyMethod.Invoke($control, [object[]]@($palette))
    $flags = [Reflection.BindingFlags] 'Instance,NonPublic'
    $expected = @{
        passSwatch = @(1, 2, 3)
        testingSwatch = @(4, 5, 6)
        idleSwatch = @(7, 8, 9)
        ngSwatch = @(10, 11, 12)
        voltageSwatch = @(13, 14, 15)
    }
    foreach ($name in $expected.Keys) {
        $field = $controlType.GetField($name, $flags)
        if ($null -eq $field) { throw "StatisticsControl 缺少图例色块: $name" }
        $color = $field.GetValue($control).BackColor
        $rgb = @($color.R, $color.G, $color.B)
        if (($rgb -join ',') -ne ($expected[$name] -join ',')) {
            throw "$name 图例颜色错误: $($rgb -join ',')"
        }
    }
    Write-Output 'Statistics legend color runtime: PASS'
}
finally {
    $control.Dispose()
}
