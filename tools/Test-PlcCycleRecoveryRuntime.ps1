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
    public bool R152;
    public bool X1F;
    public int CompletionWrites;
    public int SortingCompletionWrites;
    public readonly List<string> GradeWrites = new List<string>();
    public void Open() {}
    public void Close() {}
    public bool ReadR150() { return R150; }
    public bool ReadR151() { return R151; }
    public bool ReadR152() { return R152; }
    public bool ReadX1F() { return X1F; }
    public void WriteR160() {}
    public void WriteR161() { CompletionWrites++; if (CompletionWrites == 1) throw new Exception("timeout"); }
    public void WriteR162() { SortingCompletionWrites++; }
    public void WriteNgSelection(int channel, Action<string> responseReceived) {
        string frame = PlcAutomationProtocol.BuildNgSelectionFrame(channel);
        GradeWrites.Add(channel + ":2:" + frame.Substring(7, 5));
    }
}
public sealed class FakeScanner : ITrayScannerClient {
    public string Code = "TRAY-1";
    public bool IsOpen { get { return true; } }
    public void Open() {}
    public void Close() {}
    public void TriggerScan() {}
    public string ReadTrayCode(int timeoutMs) { return Code; }
}
public sealed class FakeHost : IAutomaticTestHost {
    public int InitialCalls;
    public int RetestCalls;
    public int PublishCalls;
    public int PrepareCalls;
    public bool RetestStillNg;
    public bool IsAutomaticModeEnabled() { return true; }
    public bool IsDebugModeEnabled() { return false; }
    public bool CanStartAutomaticTest() { return PrepareCalls == 1; }
    public bool ValidateTrayCode(string code) { return code == "TRAY-1"; }
    public bool GetNeedNgRetest() { return true; }
    public int GetRetestRoundCount() { return 1; }
    public bool RequiresNgSorting() { return true; }
    public void SetTrayCode(string code) {}
    public void PrepareAutomaticTray(string code) { PrepareCalls++; }
    public void RequestManualTrayCode(Action<string> accepted, Action cancelled) { accepted("TRAY-1"); }
    public void StartInitialPass(Action<TestPassResult> completed, Action<Exception> failed) {
        InitialCalls++;
        completed(new TestPassResult { Completed = true, Results = new List<ChannelTestResult> { new ChannelTestResult { ChannelNumber = 1, IsNg = true } } });
    }
    public void StartRetestPass(IReadOnlyList<int> channels, Action<TestPassResult> completed, Action<Exception> failed) {
        RetestCalls++;
        if (channels.Count != 1 || channels[0] != 1) throw new Exception("wrong retest set");
        completed(new TestPassResult { Completed = true, Results = new List<ChannelTestResult> { new ChannelTestResult { ChannelNumber = 1, IsNg = RetestStillNg } } });
    }
    public bool PublishFinalCycle(AutomaticTestCycle cycle) {
        PublishCalls++;
        return RetestStillNg ? cycle.CurrentNgChannels.SetEquals(new int[] { 1 }) : cycle.CurrentNgChannels.Count == 0;
    }
    public void ReportAutomaticStatus(string message) {}
    public void ReportAutomaticEvent(string message) {}
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
if ($coordinator.State.ToString() -ne 'Idle' -or $coordinator.IsCycleOccupied) { throw 'R161 retry did not release the completed tray after retest cleared its NG result.' }
if ($fakeHost.PrepareCalls -ne 1 -or $fakeHost.InitialCalls -ne 1 -or $fakeHost.RetestCalls -ne 1 -or $fakeHost.PublishCalls -ne 1 -or $plc.CompletionWrites -ne 2) {
    throw 'The retry repeated tray preparation, measurement, retest, or publication.'
}
if ($plc.GradeWrites.Count -ne 0 -or $plc.SortingCompletionWrites -ne 0) {
    throw 'An initial NG that became OK on retest must not be sent before R152.'
}
$stillNgPlc = [FakePlc]::new()
$stillNgHost = [FakeHost]::new()
$stillNgHost.RetestStillNg = $true
$stillNgCoordinator = [software.PlcAutomaticTestCoordinator]::new($stillNgPlc, [FakeScanner]::new(), $stillNgHost)
[void]$poll.Invoke($stillNgCoordinator, @($null))
$stillNgPlc.R150 = $false
[void]$poll.Invoke($stillNgCoordinator, @($null))
[void]$poll.Invoke($stillNgCoordinator, @($null))
$stillNgPlc.R151 = $true
[void]$poll.Invoke($stillNgCoordinator, @($null))
if ($stillNgCoordinator.State.ToString() -ne 'WaitingR161Ack' -or
    $stillNgHost.RetestCalls -ne 1 -or $stillNgHost.PublishCalls -ne 1 -or
    $stillNgPlc.GradeWrites.Count -ne 0) {
    throw 'An NG that remained NG after retest was sent before test-complete acknowledgement.'
}
[void]$poll.Invoke($stillNgCoordinator, @($null))
if ($stillNgCoordinator.State.ToString() -ne 'Idle' -or $stillNgCoordinator.IsCycleOccupied -or
    $stillNgPlc.GradeWrites.Count -ne 0 -or $stillNgPlc.CompletionWrites -ne 2) {
    throw 'R161 retry did not release the tray while preserving the separate pending R152/R162 handoff.'
}
$stillNgPlc.R152 = $true
[void]$poll.Invoke($stillNgCoordinator, @($null))
if ($stillNgCoordinator.State.ToString() -ne 'Idle' -or
    $stillNgPlc.GradeWrites.Count -ne 1 -or $stillNgPlc.GradeWrites[0] -ne '1:2:01001' -or
    $stillNgPlc.SortingCompletionWrites -ne 1) {
    throw "The final NG after retest did not write grade 2 and finish with R162. State=$($stillNgCoordinator.State); Grades=$($stillNgPlc.GradeWrites -join ','); R162=$($stillNgPlc.SortingCompletionWrites); R161=$($stillNgPlc.CompletionWrites)."
}
$invalidScanner = [FakeScanner]::new()
$invalidScanner.Code = ''
$invalidHost = [FakeHost]::new()
$invalidCycle = [software.PlcAutomaticTestCoordinator]::new([FakePlc]::new(), $invalidScanner, $invalidHost)
[void]$poll.Invoke($invalidCycle, @($null))
if ($invalidCycle.State.ToString() -ne 'WaitingFixturePressed' -or $invalidHost.PrepareCalls -ne 1) {
    throw 'Invalid automatic scan did not recover through the host manual tray-code callback.'
}
$pausePlc = [FakePlc]::new()
$pauseHost = [FakeHost]::new()
$pauseCoordinator = [software.PlcAutomaticTestCoordinator]::new($pausePlc, [FakeScanner]::new(), $pauseHost)
$requestPause = $pauseCoordinator.GetType().GetMethod('RequestPauseAtCycleBoundary', [Reflection.BindingFlags]'Instance,Public')
$resume = $pauseCoordinator.GetType().GetMethod('ResumeAfterPause', [Reflection.BindingFlags]'Instance,Public')
$isPausedProperty = $pauseCoordinator.GetType().GetProperty('IsPausedAtCycleBoundary', [Reflection.BindingFlags]'Instance,Public')
if ($null -eq $requestPause -or $null -eq $resume -or $null -eq $isPausedProperty) {
    throw 'The PLC coordinator does not expose cycle-boundary pause/resume.'
}
$pauseCallbackCount = 0
$pauseCallback = [Action]{ $script:pauseCallbackCount++ }
[void]$requestPause.Invoke($pauseCoordinator, [object[]]@($pauseCallback))
if (-not [bool]$isPausedProperty.GetValue($pauseCoordinator, $null) -or $pauseCallbackCount -ne 1) {
    throw 'An idle PLC coordinator did not pause immediately at its safe boundary.'
}
[void]$poll.Invoke($pauseCoordinator, @($null))
if ($pauseHost.PrepareCalls -ne 0 -or $pauseCoordinator.State.ToString() -ne 'Idle') {
    throw 'A paused PLC coordinator started another tray cycle.'
}
[void]$resume.Invoke($pauseCoordinator, @())
[void]$poll.Invoke($pauseCoordinator, @($null))
if ($pauseHost.PrepareCalls -ne 1 -or $pauseCoordinator.State.ToString() -ne 'WaitingFixturePressed') {
    throw 'Resuming the PLC coordinator did not restore normal polling.'
}
Write-Output 'PASS: retest final NG set, R161 retry, R152/R162 handoff, and manual tray-code fallback'
