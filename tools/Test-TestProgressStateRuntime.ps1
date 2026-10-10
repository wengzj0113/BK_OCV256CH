param(
    [string]$AssemblyPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'bin\AnyCPU\XL_OCV_256CH.exe')
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $AssemblyPath)) { throw "Assembly not found: $AssemblyPath" }

$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))
$stateType = $assembly.GetType('software.TestProgressState', $true)
$stageType = $assembly.GetType('software.TestProgressStage', $true)
$startField = $stateType.GetField('_startedAtUtc', [Reflection.BindingFlags]'Instance,NonPublic')
if ($null -eq $startField) { throw 'Could not find the state start-time field.' }
$testing = [Enum]::Parse($stageType, 'Testing')

function Set-TestStartedAt($state, [DateTime]$value) {
    $startField.SetValue($state, $value.ToUniversalTime())
}

$state = [Activator]::CreateInstance($stateType)
$state.Reset('等待开始')
$snapshot = $state.CreateSnapshot()
if ($snapshot.Stage.ToString() -ne 'Idle') { throw 'Reset did not restore Idle stage.' }
if ($snapshot.ProgressPercent -ne 0) { throw 'Reset did not clear progress.' }
if ($null -ne $snapshot.EstimatedRemaining) { throw 'Idle state must not show an ETA.' }

$state.Begin(10, '准备测试')
$snapshot = $state.CreateSnapshot()
if ($snapshot.ProgressPercent -ne 0) { throw 'A new test must start at 0%.' }
if ($null -eq $snapshot.EstimatedRemaining -or $snapshot.EstimatedRemaining.TotalSeconds -ne 90) {
    throw 'A new test must start with 01:30 remaining.'
}

Set-TestStartedAt $state ([DateTime]::UtcNow.AddSeconds(-45))
$state.SetStage($testing, '正在测试', $null, 25)
$state.Advance(5, 6, '正在测试第 6 通道')
$snapshot = $state.CreateSnapshot()
if ($snapshot.Stage.ToString() -ne 'Testing') { throw 'Testing stage was not retained.' }
if ($snapshot.CompletedCount -ne 5 -or $snapshot.TotalCount -ne 10) { throw 'Channel progress counts are incorrect.' }
if ($snapshot.ProgressPercent -ne 50) { throw 'Progress must follow elapsed time, not the stage or channel percentage.' }
if ($null -eq $snapshot.EstimatedRemaining -or $snapshot.EstimatedRemaining.TotalSeconds -ne 45) {
    throw 'The remaining time must be 00:45 after 45 seconds elapsed.'
}

Set-TestStartedAt $state ([DateTime]::UtcNow.AddSeconds(-95))
$snapshot = $state.CreateSnapshot()
if ($snapshot.ProgressPercent -ne 99) { throw 'Running progress must stop at 99% after 90 seconds.' }
if ($null -eq $snapshot.EstimatedRemaining -or $snapshot.EstimatedRemaining.TotalSeconds -ne 0) {
    throw 'Remaining time must stop at zero after 90 seconds.'
}

$state.Complete('测试完成')
$snapshot = $state.CreateSnapshot()
if ($snapshot.Stage.ToString() -ne 'Completed' -or $snapshot.ProgressPercent -ne 100) { throw 'Complete did not finalize progress.' }
if ($null -ne $snapshot.EstimatedRemaining) { throw 'Completed progress must not retain an ETA.' }

foreach ($terminalStage in @('Fail', 'Cancel')) {
    $state = [Activator]::CreateInstance($stateType)
    $state.Begin(10, '准备测试')
    Set-TestStartedAt $state ([DateTime]::UtcNow.AddSeconds(-30))
    if ($terminalStage -eq 'Fail') { $state.Fail('测试失败') } else { $state.Cancel('测试已停止') }
    $snapshot = $state.CreateSnapshot()
    $terminalPercent = $snapshot.ProgressPercent
    $terminalElapsed = $snapshot.Elapsed.TotalSeconds
    if ($null -ne $snapshot.EstimatedRemaining) { throw "$terminalStage state must not show an ETA." }
    Start-Sleep -Milliseconds 150
    $laterSnapshot = $state.CreateSnapshot()
    if ($laterSnapshot.ProgressPercent -ne $terminalPercent) { throw "$terminalStage progress continued after termination." }
    if ([Math]::Abs($laterSnapshot.Elapsed.TotalSeconds - $terminalElapsed) -gt 0.1) { throw "$terminalStage elapsed time continued after termination." }
}

Write-Output 'Test progress state runtime: PASS'
