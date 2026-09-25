$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assemblyPath = (Resolve-Path $assemblyPath).Path
[void][Reflection.Assembly]::LoadFrom($assemblyPath)
$definition = @'
using System;
using System.Collections.Generic;
using software;
public sealed class FakePlc : IPlcAutomationClient {
    public bool IsOpen { get { return true; } }
    public bool R150 = true;
    public bool R151;
    public bool R170 = true;
    public int CompletionWrites;
    public void Open() {}
    public void Close() {}
    public bool ReadR150() { return R150; }
    public bool ReadR151() { return R151; }
    public bool ReadR170() { return R170; }
    public void WriteR160() {}
    public void WriteR161() { CompletionWrites++; if (CompletionWrites == 1) throw new Exception("timeout"); }
    public void WriteChannelGrade(int channel, int grade) {}
}
public sealed class FakeScanner : ITrayScannerClient {
    public string Code = "TRAY-1";
    public bool IsOpen { get { return true; } }
    public void Open() {}
    public void Close() {}
    public string ReadTrayCode(int timeoutMs) { return Code; }
}
public sealed class FakeHost : IAutomaticTestHost {
    public int InitialCalls;
    public int RetestCalls;
    public int PublishCalls;
    public int PrepareCalls;
    public bool IsAutomaticModeEnabled() { return true; }
    public bool CanStartAutomaticTest() { return PrepareCalls == 1; }
    public bool ValidateTrayCode(string code) { return code == "TRAY-1"; }
    public bool GetNeedNgRetest() { return true; }
    public int GetRetestRoundCount() { return 1; }
    public bool RequiresNgSorting() { return true; }
    public void SetTrayCode(string code) {}
    public void PrepareAutomaticTray(string code) { PrepareCalls++; }
    public void RequestManualTrayCode(Action<string> accepted, Action cancelled) { throw new Exception("manual fallback invoked"); }
    public void StartInitialPass(Action<TestPassResult> completed, Action<Exception> failed) {
        InitialCalls++;
        completed(new TestPassResult { Completed = true, Results = new List<ChannelTestResult> { new ChannelTestResult { ChannelNumber = 1, IsNg = true } } });
    }
    public void StartRetestPass(IReadOnlyList<int> channels, Action<TestPassResult> completed, Action<Exception> failed) {
        RetestCalls++;
        if (channels.Count != 1 || channels[0] != 1) throw new Exception("wrong retest set");
        completed(new TestPassResult { Completed = true, Results = new List<ChannelTestResult> { new ChannelTestResult { ChannelNumber = 1, IsNg = false } } });
    }
    public bool PublishFinalCycle(AutomaticTestCycle cycle) { PublishCalls++; return cycle.CurrentNgChannels.Count == 0; }
    public void ReportAutomaticStatus(string message) {}
}
'@
$sourcePath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'FakePlcCycle.cs'
$helperPath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'FakePlcCycle.dll'
[IO.File]::WriteAllText($sourcePath, $definition)
& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' /nologo /target:library "/out:$helperPath" "/reference:$assemblyPath" $sourcePath
if ($LASTEXITCODE -ne 0) { throw 'Fake PLC cycle helper failed to compile.' }
[void][Reflection.Assembly]::LoadFrom($helperPath)
$plc = [FakePlc]::new()
$fakeHost = [FakeHost]::new()
$coordinator = [software.PlcAutomaticTestCoordinator]::new($plc, [FakeScanner]::new(), $fakeHost)
$poll = $coordinator.GetType().GetMethod('Poll', [Reflection.BindingFlags]'Instance,NonPublic')
[void]$poll.Invoke($coordinator, @($null))
$plc.R150 = $false
[void]$poll.Invoke($coordinator, @($null))
[void]$poll.Invoke($coordinator, @($null))
$plc.R151 = $true
[void]$poll.Invoke($coordinator, @($null))
if ($coordinator.State.ToString() -ne 'WaitingR161Ack') { throw "Expected pending R161; got $($coordinator.State)" }
[void]$poll.Invoke($coordinator, @($null))
if ($coordinator.State.ToString() -ne 'WaitingCycleReset') { throw 'R161 retry did not complete.' }
if ($fakeHost.PrepareCalls -ne 1 -or $fakeHost.InitialCalls -ne 1 -or $fakeHost.RetestCalls -ne 1 -or $fakeHost.PublishCalls -ne 1 -or $plc.CompletionWrites -ne 2) {
    throw 'The retry repeated tray preparation, measurement, retest, or publication.'
}
$invalidScanner = [FakeScanner]::new()
$invalidScanner.Code = ''
$invalidHost = [FakeHost]::new()
$invalidCycle = [software.PlcAutomaticTestCoordinator]::new([FakePlc]::new(), $invalidScanner, $invalidHost)
[void]$poll.Invoke($invalidCycle, @($null))
if ($invalidCycle.State.ToString() -ne 'Fault' -or $invalidHost.PrepareCalls -ne 0) { throw 'Invalid automatic scan entered tray preparation.' }
Write-Output 'PASS: same-tray NG retest and R161-only recovery'
