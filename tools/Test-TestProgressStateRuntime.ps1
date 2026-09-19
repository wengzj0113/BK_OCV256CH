param(
    [string]$AssemblyPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'bin\AnyCPU\XL_OCV_256CH.exe')
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $AssemblyPath)) { throw "Assembly not found: $AssemblyPath" }

$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))
$stateType = $assembly.GetType('software.TestProgressState', $true)
$stageType = $assembly.GetType('software.TestProgressStage', $true)
$state = [Activator]::CreateInstance($stateType)
$idle = [Enum]::Parse($stageType, 'Idle')
$testing = [Enum]::Parse($stageType, 'Testing')

$state.Reset('等待开始')
$snapshot = $state.CreateSnapshot()
if ($snapshot.Stage.ToString() -ne 'Idle') { throw 'Reset did not restore Idle stage.' }
if ($snapshot.ProgressPercent -ne 0) { throw 'Reset did not clear progress.' }

$state.Begin(10, '准备测试')
$state.SetStage($testing, '正在测试', $null, 25)
Start-Sleep -Milliseconds 100
$state.Advance(5, 6, '正在测试第 6 通道')
$snapshot = $state.CreateSnapshot()
if ($snapshot.Stage.ToString() -ne 'Testing') { throw 'Testing stage was not retained.' }
if ($snapshot.CompletedCount -ne 5 -or $snapshot.TotalCount -ne 10) { throw 'Channel progress counts are incorrect.' }
if ($snapshot.ProgressPercent -ne 50) { throw 'Channel progress percent is incorrect.' }
if ($null -eq $snapshot.EstimatedRemaining) { throw 'ETA was not calculated after progress advanced.' }

$state.Complete('测试完成')
$snapshot = $state.CreateSnapshot()
if ($snapshot.Stage.ToString() -ne 'Completed' -or $snapshot.ProgressPercent -ne 100) { throw 'Complete did not finalize progress.' }
if ($null -ne $snapshot.EstimatedRemaining) { throw 'Completed progress must not retain an ETA.' }

Write-Output 'Test progress state runtime: PASS'
