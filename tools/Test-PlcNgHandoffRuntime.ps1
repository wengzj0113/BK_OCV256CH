$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assemblyPath = (Resolve-Path $assemblyPath).Path
[void][Reflection.Assembly]::LoadFrom($assemblyPath)
$definition = @'
using System;
using System.Collections.Generic;
using software;
public sealed class HandoffPlc : IPlcAutomationClient {
    public bool IsOpen { get { return true; } }
    public bool R150 = true, R151, R170;
    public int CompletionWrites;
    public int R170Reads;
    public bool FailNextGrade;
    public int FailAtWrite;
    public int WriteAttempts;
    public readonly List<string> GradeWrites = new List<string>();
    public void Open() {}
    public void Close() {}
    public bool ReadR150() { return R150; }
    public bool ReadR151() { return R151; }
    public bool ReadR170() { R170Reads++; return R170; }
    public void WriteR160() {}
    public void WriteR161() { CompletionWrites++; }
    public void WriteChannelGrade(int channel, int grade) {
        if (grade != 0 && grade != 2) throw new Exception("wrong grade");
        WriteAttempts++;
        if (FailAtWrite == WriteAttempts) { FailAtWrite = 0; throw new Exception("temporary NG timeout"); }
        if (FailNextGrade) { FailNextGrade = false; throw new Exception("temporary DT timeout"); }
        GradeWrites.Add(channel + ":" + grade);
    }
}
public sealed class HandoffScanner : ITrayScannerClient {
    public bool IsOpen { get { return true; } }
    public void Open() {}
    public void Close() {}
    public string ReadTrayCode(int timeoutMs) { return "TRAY-NEW"; }
}
public sealed class HandoffHost : IAutomaticTestHost {
    public int SaveCalls, InitialCalls;
    public bool Sorting = true;
    public bool NoNg;
    public bool IsAutomaticModeEnabled() { return true; }
    public bool CanStartAutomaticTest() { return true; }
    public bool ValidateTrayCode(string code) { return code == "TRAY-NEW"; }
    public bool GetNeedNgRetest() { return false; }
    public int GetRetestRoundCount() { return 0; }
    public bool RequiresNgSorting() { return Sorting; }
    public void SetTrayCode(string code) {}
    public void PrepareAutomaticTray(string code) {}
    public void RequestManualTrayCode(Action<string> accepted, Action cancelled) {}
    public void StartInitialPass(Action<TestPassResult> completed, Action<Exception> failed) {
        InitialCalls++;
        if (NoNg) {
            completed(new TestPassResult { Completed = true, Results = new List<ChannelTestResult> {
                new ChannelTestResult { ChannelNumber = 2, IsNg = false }
            }});
            return;
        }
        completed(new TestPassResult { Completed = true, Results = new List<ChannelTestResult> {
            new ChannelTestResult { ChannelNumber = 2, IsNg = true },
            new ChannelTestResult { ChannelNumber = 17, IsNg = true }
        }});
    }
    public void StartRetestPass(IReadOnlyList<int> channels, Action<TestPassResult> completed, Action<Exception> failed) { throw new Exception("unexpected retest"); }
    public bool PublishFinalCycle(AutomaticTestCycle cycle) {
        SaveCalls++;
        return NoNg ? cycle.CurrentNgChannels.Count == 0 : cycle.CurrentNgChannels.SetEquals(new int[] { 2, 17 });
    }
    public void ReportAutomaticStatus(string message) {}
}
'@
$sourcePath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'HandoffFakes.cs'
$helperPath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'HandoffFakes.dll'
[IO.File]::WriteAllText($sourcePath, $definition)
& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' /nologo /target:library "/out:$helperPath" "/reference:$assemblyPath" $sourcePath
if ($LASTEXITCODE -ne 0) { throw 'NG handoff fake compilation failed.' }
[void][Reflection.Assembly]::LoadFrom($helperPath)
$plc = [HandoffPlc]::new()
$fakeHost = [HandoffHost]::new()
$coordinator = [software.PlcAutomaticTestCoordinator]::new($plc, [HandoffScanner]::new(), $fakeHost)
$poll = $coordinator.GetType().GetMethod('Poll', [Reflection.BindingFlags]'Instance,NonPublic')
[void]$poll.Invoke($coordinator, @($null))
$plc.R150 = $false
[void]$poll.Invoke($coordinator, @($null))
[void]$poll.Invoke($coordinator, @($null))
$plc.R151 = $true
[void]$poll.Invoke($coordinator, @($null))
if ($coordinator.State.ToString() -ne 'WaitingNgSlotReady' -or $fakeHost.SaveCalls -ne 1 -or
    $plc.GradeWrites.Count -ne 0 -or $plc.CompletionWrites -ne 0) {
    throw 'Final result was not saved while R170=0 or the occupied DT area was overwritten.'
}
[void]$poll.Invoke($coordinator, @($null))
if ($fakeHost.SaveCalls -ne 1 -or $plc.GradeWrites.Count -ne 0 -or $plc.CompletionWrites -ne 0) {
    throw 'Waiting for R170 repeated saving or wrote to an occupied DT area.'
}
$plc.R170 = $true
[void]$poll.Invoke($coordinator, @($null))
if ($coordinator.State.ToString() -ne 'WaitingCycleReset' -or
    $fakeHost.SaveCalls -ne 1 -or $fakeHost.InitialCalls -ne 1 -or
    $plc.GradeWrites.Count -ne 258 -or $plc.GradeWrites[0] -ne '1:0' -or
    $plc.GradeWrites[255] -ne '256:0' -or $plc.GradeWrites[256] -ne '2:2' -or
    $plc.GradeWrites[257] -ne '17:2' -or $plc.CompletionWrites -ne 1) {
    throw 'R170=1 did not publish the saved NG set exactly once before R161.'
}
$plc.R151 = $false
[void]$poll.Invoke($coordinator, @($null))
$plc.R150 = $true
[void]$poll.Invoke($coordinator, @($null))
$plc.R150 = $false
[void]$poll.Invoke($coordinator, @($null))
[void]$poll.Invoke($coordinator, @($null))
$plc.R151 = $true
[void]$poll.Invoke($coordinator, @($null))
if ($coordinator.State.ToString() -ne 'WaitingNgSlotReady' -or $fakeHost.SaveCalls -ne 2 -or
    $plc.GradeWrites.Count -ne 258 -or $plc.CompletionWrites -ne 1) {
    throw 'A lingering R170=1 was reused for a second tray before the 1->0->1 handshake.'
}
$plc.R170 = $false
[void]$poll.Invoke($coordinator, @($null))
if ($plc.GradeWrites.Count -ne 258 -or $plc.CompletionWrites -ne 1) {
    throw 'The second tray overwrote the DT area while R170=0.'
}
$plc.R170 = $true
[void]$poll.Invoke($coordinator, @($null))
if ($coordinator.State.ToString() -ne 'WaitingCycleReset' -or $fakeHost.SaveCalls -ne 2 -or
    $plc.GradeWrites.Count -ne 516 -or $plc.CompletionWrites -ne 2) {
    throw 'The second tray was not handed off after the next R170 rising edge.'
}
$retryPlc = [HandoffPlc]::new()
$retryPlc.R170 = $true
$retryPlc.FailNextGrade = $true
$retryHost = [HandoffHost]::new()
$retryCoordinator = [software.PlcAutomaticTestCoordinator]::new($retryPlc, [HandoffScanner]::new(), $retryHost)
[void]$poll.Invoke($retryCoordinator, @($null))
$retryPlc.R150 = $false
[void]$poll.Invoke($retryCoordinator, @($null))
[void]$poll.Invoke($retryCoordinator, @($null))
$retryPlc.R151 = $true
[void]$poll.Invoke($retryCoordinator, @($null))
if ($retryCoordinator.State.ToString() -ne 'WaitingNgSlotReady' -or $retryHost.SaveCalls -ne 1 -or
    $retryPlc.CompletionWrites -ne 0) {
    throw 'A failed DT write incorrectly completed handoff or repeated result saving.'
}
[void]$poll.Invoke($retryCoordinator, @($null))
if ($retryCoordinator.State.ToString() -ne 'WaitingCycleReset' -or $retryHost.SaveCalls -ne 1 -or
    $retryPlc.GradeWrites.Count -ne 258 -or $retryPlc.CompletionWrites -ne 1) {
    throw 'DT retry did not complete without repeating measurement or result save.'
}
$partialPlc = [HandoffPlc]::new()
$partialPlc.R170 = $true
$partialPlc.FailAtWrite = 258
$partialHost = [HandoffHost]::new()
$partialCoordinator = [software.PlcAutomaticTestCoordinator]::new($partialPlc, [HandoffScanner]::new(), $partialHost)
[void]$poll.Invoke($partialCoordinator, @($null))
$partialPlc.R150 = $false
[void]$poll.Invoke($partialCoordinator, @($null))
[void]$poll.Invoke($partialCoordinator, @($null))
$partialPlc.R151 = $true
[void]$poll.Invoke($partialCoordinator, @($null))
if ($partialCoordinator.State.ToString() -ne 'WaitingNgSlotReady' -or
    $partialPlc.GradeWrites.Count -ne 257 -or $partialPlc.CompletionWrites -ne 0) {
    throw 'Partial NG write incorrectly signalled PLC completion.'
}
[void]$poll.Invoke($partialCoordinator, @($null))
if ($partialCoordinator.State.ToString() -ne 'WaitingCycleReset' -or
    $partialHost.SaveCalls -ne 1 -or $partialPlc.GradeWrites.Count -ne 515 -or
    $partialPlc.GradeWrites[257] -ne '1:0' -or $partialPlc.CompletionWrites -ne 1) {
    throw 'Partial NG write retry did not restart from full DT clear.'
}
$nonSortingPlc = [HandoffPlc]::new()
$nonSortingHost = [HandoffHost]::new()
$nonSortingHost.Sorting = $false
$nonSortingCoordinator = [software.PlcAutomaticTestCoordinator]::new($nonSortingPlc, [HandoffScanner]::new(), $nonSortingHost)
[void]$poll.Invoke($nonSortingCoordinator, @($null))
$nonSortingPlc.R150 = $false
[void]$poll.Invoke($nonSortingCoordinator, @($null))
[void]$poll.Invoke($nonSortingCoordinator, @($null))
$nonSortingPlc.R151 = $true
[void]$poll.Invoke($nonSortingCoordinator, @($null))
if ($nonSortingCoordinator.State.ToString() -ne 'WaitingCycleReset' -or
    $nonSortingHost.SaveCalls -ne 1 -or $nonSortingPlc.GradeWrites.Count -ne 0 -or
    $nonSortingPlc.R170Reads -ne 0 -or $nonSortingPlc.CompletionWrites -ne 1) {
    throw 'OCV1/OCV2 incorrectly waited for R170 or wrote the sorting DT area.'
}
$noNgPlc = [HandoffPlc]::new()
$noNgPlc.R170 = $true
$noNgHost = [HandoffHost]::new()
$noNgHost.NoNg = $true
$noNgCoordinator = [software.PlcAutomaticTestCoordinator]::new($noNgPlc, [HandoffScanner]::new(), $noNgHost)
[void]$poll.Invoke($noNgCoordinator, @($null))
$noNgPlc.R150 = $false
[void]$poll.Invoke($noNgCoordinator, @($null))
[void]$poll.Invoke($noNgCoordinator, @($null))
$noNgPlc.R151 = $true
[void]$poll.Invoke($noNgCoordinator, @($null))
if ($noNgCoordinator.State.ToString() -ne 'WaitingCycleReset' -or
    $noNgPlc.GradeWrites.Count -ne 256 -or $noNgPlc.GradeWrites[0] -ne '1:0' -or
    $noNgPlc.GradeWrites[255] -ne '256:0' -or $noNgPlc.CompletionWrites -ne 1) {
    throw 'OCV0 without NG did not clear all stale PLC sorting results.'
}
Write-Output 'PASS: final NG waits for R170 without repeating measurement or result save'
