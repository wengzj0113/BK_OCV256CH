param([string]$AssemblyPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$exePath = if ([string]::IsNullOrWhiteSpace($AssemblyPath)) { Join-Path $root 'bin\AnyCPU\XL_OCV_256CH.exe' } else { $AssemblyPath }
$assembly = [Reflection.Assembly]::LoadFrom($exePath)
$settingsType = $assembly.GetType('software.ChannelColorSettings', $true)
$initType = $assembly.GetType('software.Module1+Initset', $true)
$fromInitset = $settingsType.GetMethod('FromInitset', [Reflection.BindingFlags] 'Static,Public,NonPublic')
$init = [Activator]::CreateInstance($initType)
$palette = $fromInitset.Invoke($null, [object[]]@($init))

$gray = [Drawing.Color]::FromArgb(255, 128, 128, 128).ToArgb()
$ironGray = [Drawing.Color]::FromArgb(255, 101, 113, 132).ToArgb()
if ($palette.IdleColor.ToArgb() -ne $gray) { throw '空闲默认颜色不是灰色' }
if ($palette.SelectedColor.ToArgb() -ne $ironGray) { throw '选定默认颜色不是铁灰色' }

$idleField = $initType.GetField('ChannelIdleColorArgb')
$idleField.SetValue($init, $ironGray)
$palette = $fromInitset.Invoke($null, [object[]]@($init))
if ($palette.IdleColor.ToArgb() -ne $gray) { throw '旧版空闲铁灰色未迁移为灰色' }

Write-Output 'Channel color defaults and migration: PASS'
