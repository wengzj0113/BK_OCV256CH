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
    public bool R150 = true, R151, R152;
    public int R150Reads, R151Reads, R152Reads;
    public int R160Writes, R161Writes, R162Writes;
    public int FailGradeAttempts, FailR160Writes, FailR161Writes, FailR162Writes;
    public readonly List<string> Events = new List<string>();
    public readonly List<string> GradeWrites = new List<string>();
    public readonly List<int> DtAddresses = new List<int>();
    public void Open() {}
    public void Close() {}
    public bool ReadR150() { R150Reads++; Events.Add("read:R150"); return R150; }
    public bool ReadR151() { R151Reads++; Events.Add("read:R151"); return R151; }
    public bool ReadR152() { R152Reads++; Events.Add("read:R152:" + (R152 ? "1" : "0")); return R152; }
    public void WriteR160() {
        Events.Add("write:R160");
        if (FailR160Writes > 0) { FailR160Writes--; throw new Exception("temporary R160 timeout"); }
        R160Writes++;
    }
    public void WriteR161() {
        Events.Add("write:R161");
        if (FailR161Writes > 0) { FailR161Writes--; throw new Exception("temporary R161 timeout"); }
        R161Writes++;
    }
    public void WriteR162() {
        Events.Add("write:R162");
        if (FailR162Writes > 0) { FailR162Writes--; throw new Exception("temporary R162 timeout"); }
        R162Writes++;
    }
    public void WriteNgSelection(int channel) {
        Events.Add("write:DT:" + channel + ":2");
        if (FailGradeAttempts > 0) { FailGradeAttempts--; throw new Exception("temporary DT timeout"); }
        GradeWrites.Add(channel + ":2");
        DtAddresses.Add(int.Parse(PlcAutomationProtocol.BuildNgSelectionFrame(channel).Substring(7, 5)));
    }
}
public sealed class HandoffScanner : ITrayScannerClient {
    public string Code = "TRAY-NEW";
    public int TriggerCount;
    public string[] Codes;
    public bool ThrowOnRead;
    public bool IsOpen { get { return true; } }
    public void Open() {}
    public void Close() {}
    public void TriggerScan() { TriggerCount++; }
    public string ReadTrayCode(int timeoutMs) {
        if (ThrowOnRead) throw new TimeoutException("scanner timeout");
        if (Codes != null && TriggerCount > 0 && TriggerCount <= Codes.Length) return Codes[TriggerCount - 1];
        return Code;
    }
}
public sealed class HandoffHost : IAutomaticTestHost {
public int SaveCalls, InitialCalls, ManualPrompts, PrepareCalls;
    public bool Sorting = true, NoNg, CancelManual, HoldInitialPass, FailInitialPass;
    public string ManualCode = "TRAY-NEW";
    private Action<TestPassResult> pendingInitialCompleted;
    private Action<Exception> pendingInitialFailed;
    public readonly List<string> Events = new List<string>();
    public bool IsAutomaticModeEnabled() { return true; }
    public bool IsDebugModeEnabled() { return false; }
    public bool CanStartAutomaticTest() { return true; }
    public bool ValidateTrayCode(string code) { return code == "TRAY-NEW"; }
    public bool GetNeedNgRetest() { return false; }
    public int GetRetestRoundCount() { return 0; }
    public bool RequiresNgSorting() { return Sorting; }
    public void SetTrayCode(string code) {}
    public void PrepareAutomaticTray(string code) { PrepareCalls++; }
    public void RequestManualTrayCode(Action<string> accepted, Action cancelled) {
        ManualPrompts++;
        if (CancelManual) cancelled();
        else accepted(ManualPrompts == 1 ? ManualCode : "TRAY-NEW");
    }
    public void StartInitialPass(Action<TestPassResult> completed, Action<Exception> failed) {
        InitialCalls++;
        if (FailInitialPass) { failed(new InvalidOperationException("simulated measurement fault")); return; }
        if (HoldInitialPass) { pendingInitialCompleted = completed; pendingInitialFailed = failed; return; }
        completed(CreateInitialResult());
    }
    public void CompleteInitialPass() {
        Action<TestPassResult> completed = pendingInitialCompleted;
        pendingInitialCompleted = null;
        pendingInitialFailed = null;
        if (completed == null) throw new InvalidOperationException("No initial pass is pending.");
        completed(CreateInitialResult());
    }
    public void FailPendingInitialPass() {
        Action<Exception> failed = pendingInitialFailed;
        pendingInitialCompleted = null;
        pendingInitialFailed = null;
        if (failed == null) throw new InvalidOperationException("No initial pass is pending.");
        failed(new InvalidOperationException("simulated measurement fault"));
    }
    private TestPassResult CreateInitialResult() {
        var results = new List<ChannelTestResult>();
        results.Add(new ChannelTestResult { ChannelNumber = 2, IsNg = !NoNg });
        results.Add(new ChannelTestResult { ChannelNumber = 3, IsNg = false });
        return new TestPassResult { Completed = true, Results = results };
    }
    public void StartRetestPass(IReadOnlyList<int> channels, Action<TestPassResult> completed, Action<Exception> failed) {
        throw new Exception("unexpected retest");
    }
    public bool PublishFinalCycle(AutomaticTestCycle cycle) {
        SaveCalls++;
        return NoNg ? cycle.CurrentNgChannels.Count == 0 : cycle.CurrentNgChannels.SetEquals(new int[] { 2 });
    }
    public void ReportAutomaticStatus(string message) {}
    public void ReportAutomaticEvent(string message) { Events.Add(message); }
}
'@
$sourcePath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'HandoffFakes.cs'
$helperPath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'HandoffFakes.dll'
[IO.File]::WriteAllText($sourcePath, $definition)
& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' /nologo /target:library "/out:$helperPath" "/reference:$assemblyPath" $sourcePath
if ($LASTEXITCODE -ne 0) { throw 'NG handoff fake compilation failed.' }
[void][Reflection.Assembly]::LoadFrom($helperPath)
$poll = $null
function New-Coordinator($plc, $scanner, $testHost) {
    $coordinator = [software.PlcAutomaticTestCoordinator]::new($plc, $scanner, $testHost)
    if ($null -eq $script:poll) {
        $script:poll = $coordinator.GetType().GetMethod('Poll', [Reflection.BindingFlags]'Instance,NonPublic')
    }
    return $coordinator
}
function Invoke-Poll($coordinator) { [void]$script:poll.Invoke($coordinator, @($null)) }
function Assert-EventContains($events, $text, $message) {
    $combinedEvents = [string]::Join(' | ', $events)
    if (-not $combinedEvents.Contains($text)) { throw ($message + ' Events: ' + $combinedEvents) }
}
function Start-TestCycle($coordinator, $plc) {
    Invoke-Poll $coordinator # R150 -> scan -> R160
    $plc.R150 = $false
    Invoke-Poll $coordinator # R150 reset -> arm R151
    Invoke-Poll $coordinator # observe R151=0
    $plc.R151 = $true
    Invoke-Poll $coordinator # R151=1 -> test/save/R161
}
function Prepare-TrayOnly($coordinator, $plc) {
    Invoke-Poll $coordinator # R150 -> scan -> R160
    $plc.R150 = $false
    Invoke-Poll $coordinator # clear the R150 edge before the next request
}

$faultPlc = [HandoffPlc]::new()
$faultHost = [HandoffHost]::new()
$faultHost.FailInitialPass = $true
$faultCoordinator = New-Coordinator $faultPlc ([HandoffScanner]::new()) $faultHost
Prepare-TrayOnly $faultCoordinator $faultPlc
$faultPlc.R151 = $true
Invoke-Poll $faultCoordinator
$faultPlc.R152 = $true
Invoke-Poll $faultCoordinator
if ($faultCoordinator.State.ToString() -ne 'Fault' -or $faultPlc.GradeWrites.Count -ne 0 -or
    $faultPlc.R162Writes -ne 0) {
    throw 'R152 during a failed test must not misreport the incomplete result as no NG.'
}
$faultCoordinator.RequestStopCurrentCycle()
if ($faultCoordinator.State.ToString() -ne 'Idle' -or $faultPlc.R162Writes -ne 0) {
    throw 'Stopping a faulted test must clear software requests without writing PLC-owned input bits or R162.'
}

$deferredFaultPlc = [HandoffPlc]::new()
$deferredFaultHost = [HandoffHost]::new()
$deferredFaultHost.HoldInitialPass = $true
$deferredFaultCoordinator = New-Coordinator $deferredFaultPlc ([HandoffScanner]::new()) $deferredFaultHost
Prepare-TrayOnly $deferredFaultCoordinator $deferredFaultPlc
$deferredFaultPlc.R151 = $true
Invoke-Poll $deferredFaultCoordinator
$deferredFaultPlc.R152 = $true
Invoke-Poll $deferredFaultCoordinator
$deferredFaultHost.FailPendingInitialPass()
Invoke-Poll $deferredFaultCoordinator
$sortingStateField = $deferredFaultCoordinator.GetType().GetField('sortingRequestTestStateAtCapture', [Reflection.BindingFlags]'Instance,NonPublic')
if ($deferredFaultCoordinator.State.ToString() -ne 'Fault' -or $deferredFaultPlc.GradeWrites.Count -ne 0 -or
    $deferredFaultPlc.R162Writes -ne 0 -or $sortingStateField.GetValue($deferredFaultCoordinator).ToString() -ne 'TestFaulted') {
    throw 'An R152 deferred during testing must become faulted if that test fails, without sending grades or R162.'
}

$earlyPlc = [HandoffPlc]::new()
$earlyHost = [HandoffHost]::new()
$earlyCoordinator = New-Coordinator $earlyPlc ([HandoffScanner]::new()) $earlyHost
Prepare-TrayOnly $earlyCoordinator $earlyPlc
$earlyPlc.R152 = $true
Invoke-Poll $earlyCoordinator
if ($earlyPlc.R162Writes -ne 1 -or $earlyPlc.GradeWrites.Count -ne 0 -or
    $earlyPlc.R161Writes -ne 0 -or $earlyHost.InitialCalls -ne 0) {
    throw 'R152 before any test must skip NG grades and write R162 without waiting for R151.'
}
Assert-EventContains $earlyHost.Events 'R162=1' 'The event log must show R162 write and confirmation.'

$simultaneousPlc = [HandoffPlc]::new()
$simultaneousHost = [HandoffHost]::new()
$simultaneousCoordinator = New-Coordinator $simultaneousPlc ([HandoffScanner]::new()) $simultaneousHost
Prepare-TrayOnly $simultaneousCoordinator $simultaneousPlc
$simultaneousPlc.R151 = $true
$simultaneousPlc.R152 = $true
Invoke-Poll $simultaneousCoordinator
if ($simultaneousPlc.R162Writes -ne 1 -or $simultaneousPlc.GradeWrites.Count -ne 0 -or
    $simultaneousPlc.R161Writes -ne 1 -or $simultaneousHost.InitialCalls -ne 1 -or
    $simultaneousPlc.Events.IndexOf('write:R162') -ge $simultaneousPlc.Events.IndexOf('write:R161')) {
    throw 'Simultaneous R151/R152 must classify R152 before starting the newly requested test.'
}

$duringPlc = [HandoffPlc]::new()
$duringHost = [HandoffHost]::new()
$duringHost.HoldInitialPass = $true
$duringCoordinator = New-Coordinator $duringPlc ([HandoffScanner]::new()) $duringHost
Prepare-TrayOnly $duringCoordinator $duringPlc
$duringPlc.R151 = $true
Invoke-Poll $duringCoordinator
$duringPlc.R152 = $true
Invoke-Poll $duringCoordinator
if ($duringPlc.R162Writes -ne 0 -or $duringPlc.GradeWrites.Count -ne 0 -or $duringHost.SaveCalls -ne 0) {
    throw 'R152 during an active measurement must wait for its final result.'
}
$duringHost.CompleteInitialPass()
Invoke-Poll $duringCoordinator
if ($duringPlc.R161Writes -ne 1 -or $duringPlc.GradeWrites.Count -ne 1 -or
    $duringPlc.R162Writes -ne 1 -or
    $duringPlc.Events.IndexOf('write:R161') -ge $duringPlc.Events.IndexOf('write:DT:2:2') -or
    $duringPlc.Events.IndexOf('write:DT:2:2') -ge $duringPlc.Events.IndexOf('write:R162')) {
    throw 'A deferred R152 must publish final results, then NG grade, then R162.'
}
Assert-EventContains $duringHost.Events 'R161=1' 'The event log must show R161 write and confirmation.'
Assert-EventContains $duringHost.Events 'DT1002=2' 'The event log must identify the acknowledged NG grade address and value.'

$plc = [HandoffPlc]::new()
$testHost = [HandoffHost]::new()
$scanner = [HandoffScanner]::new()
$coordinator = New-Coordinator $plc $scanner $testHost
Start-TestCycle $coordinator $plc
if ($coordinator.State.ToString() -ne 'WaitingR152' -or $plc.R160Writes -ne 1 -or
    $plc.R161Writes -ne 1 -or $plc.R152Reads -eq 0 -or $plc.GradeWrites.Count -ne 0 -or
    $plc.R162Writes -ne 0 -or $testHost.SaveCalls -ne 1) {
    throw 'R161 must follow saved test results; R152 must remain independently polled while waiting for NG handoff.'
}
$plc.R152 = $false
$readsBeforeR152 = $plc.R151Reads
Invoke-Poll $coordinator
if ($plc.GradeWrites.Count -ne 0 -or $plc.R162Writes -ne 0 -or
    $plc.R151Reads -ne ($readsBeforeR152 + 1) -or $coordinator.State.ToString() -ne 'WaitingR152') {
    throw 'R152=0 must leave NG grades/R162 untouched while R150/R151/R152 continue independent polling.'
}
$plc.R152 = $true
Invoke-Poll $coordinator
if ($coordinator.State.ToString() -ne 'Idle' -or $plc.GradeWrites.Count -ne 1 -or
    $plc.GradeWrites[0] -ne '2:2' -or $plc.DtAddresses[0] -ne 1002 -or
    $plc.R162Writes -ne 1 -or $plc.R161Writes -ne 1 -or
    $plc.Events.IndexOf('write:R161') -ge $plc.Events.IndexOf('read:R152:1') -or
    $plc.Events.IndexOf('read:R152:1') -ge $plc.Events.IndexOf('write:DT:2:2') -or
    $plc.Events.IndexOf('write:DT:2:2') -ge $plc.Events.IndexOf('write:R162')) {
    throw 'The R161 -> R152 -> NG=2 -> R162 order or sequential DT address is wrong.'
}
$readsAfterR162 = $plc.R151Reads
$plc.R150 = $false
Invoke-Poll $coordinator
if ($plc.R151Reads -ne ($readsAfterR162 + 1)) { throw 'R151 must remain independently polled after R162.' }

$retryPlc = [HandoffPlc]::new()
$retryPlc.FailGradeAttempts = 1
$retryHost = [HandoffHost]::new()
$retryCoordinator = New-Coordinator $retryPlc ([HandoffScanner]::new()) $retryHost
Start-TestCycle $retryCoordinator $retryPlc
$retryPlc.R152 = $true
Invoke-Poll $retryCoordinator
if ($retryCoordinator.State.ToString() -ne 'WaitingR152' -or $retryPlc.R162Writes -ne 0 -or $retryHost.SaveCalls -ne 1) {
    throw 'A failed NG write must not send R162 or repeat final result publication.'
}
Assert-EventContains $retryHost.Events 'temporary DT timeout' 'The event log must show the failed NG register write and reason.'
Invoke-Poll $retryCoordinator
if ($retryCoordinator.State.ToString() -ne 'Idle' -or $retryPlc.GradeWrites.Count -ne 1 -or
    $retryPlc.R162Writes -ne 1 -or $retryHost.InitialCalls -ne 1) {
    throw 'The NG write retry did not finish the handoff exactly once.'
}

$ackPlc = [HandoffPlc]::new()
$ackPlc.FailR162Writes = 1
$ackHost = [HandoffHost]::new()
$ackCoordinator = New-Coordinator $ackPlc ([HandoffScanner]::new()) $ackHost
Start-TestCycle $ackCoordinator $ackPlc
$ackPlc.R152 = $true
Invoke-Poll $ackCoordinator
if ($ackCoordinator.State.ToString() -ne 'WaitingR162Ack' -or $ackPlc.GradeWrites.Count -ne 1) {
    throw 'An unacknowledged R162 must remain retryable after NG grades are written.'
}
Invoke-Poll $ackCoordinator
if ($ackCoordinator.State.ToString() -ne 'Idle' -or $ackPlc.GradeWrites.Count -ne 1 -or
    $ackPlc.R162Writes -ne 1 -or $ackPlc.R161Writes -ne 1) {
    throw 'R162 retry repeated test, saved results, or NG writes.'
}
Assert-EventContains $ackHost.Events 'temporary R162 timeout' 'The event log must show failed PLC writes and the reason.'

$noNgPlc = [HandoffPlc]::new()
$noNgHost = [HandoffHost]::new()
$noNgHost.NoNg = $true
$noNgCoordinator = New-Coordinator $noNgPlc ([HandoffScanner]::new()) $noNgHost
Start-TestCycle $noNgCoordinator $noNgPlc
$noNgPlc.R152 = $true
Invoke-Poll $noNgCoordinator
if ($noNgCoordinator.State.ToString() -ne 'Idle' -or $noNgPlc.GradeWrites.Count -ne 0 -or
    $noNgPlc.R162Writes -ne 1) {
    throw 'A zero-NG tray must send no DT grades but still acknowledge R152 with R162.'
}

$nonSortingPlc = [HandoffPlc]::new()
$nonSortingHost = [HandoffHost]::new()
$nonSortingHost.Sorting = $false
$nonSortingHost.NoNg = $true
$nonSortingCoordinator = New-Coordinator $nonSortingPlc ([HandoffScanner]::new()) $nonSortingHost
Start-TestCycle $nonSortingCoordinator $nonSortingPlc
if ($nonSortingCoordinator.State.ToString() -ne 'WaitingR152' -or
    $nonSortingPlc.R161Writes -ne 1 -or $nonSortingPlc.R152Reads -eq 0 -or
    $nonSortingPlc.GradeWrites.Count -ne 0 -or $nonSortingPlc.R162Writes -ne 0) {
    throw 'The coordinator must independently keep polling R152 after R161, even when the stage has no NG grades.'
}
$nonSortingPlc.R152 = $true
Invoke-Poll $nonSortingCoordinator
if ($nonSortingCoordinator.State.ToString() -ne 'Idle' -or $nonSortingPlc.GradeWrites.Count -ne 0 -or
    $nonSortingPlc.R162Writes -ne 1) {
    throw 'An empty final NG list must skip grade writes and acknowledge R152 with R162.'
}

$manualPlc = [HandoffPlc]::new()
$manualHost = [HandoffHost]::new()
$manualScanner = [HandoffScanner]::new()
$manualScanner.Code = ''
$manualHost.ManualCode = 'BAD-LENGTH'
$manualCoordinator = New-Coordinator $manualPlc $manualScanner $manualHost
Invoke-Poll $manualCoordinator
if ($manualCoordinator.State.ToString() -ne 'WaitingFixturePressed' -or $manualHost.ManualPrompts -ne 2 -or
    $manualPlc.R160Writes -ne 1 -or $manualHost.PrepareCalls -ne 1) {
    throw 'An invalid scan must invoke manual entry and only prepare after a valid manual code.'
}
Assert-EventContains $manualHost.Events 'R160=1' 'The event log must show R160 write and confirmation.'
$cancelPlc = [HandoffPlc]::new()
$cancelHost = [HandoffHost]::new()
$cancelHost.CancelManual = $true
$cancelScanner = [HandoffScanner]::new()
$cancelScanner.Codes = [string[]]@('', 'TRAY-NEW')
$cancelCoordinator = New-Coordinator $cancelPlc $cancelScanner $cancelHost
Invoke-Poll $cancelCoordinator
if ($cancelCoordinator.State.ToString() -ne 'WaitingTrayScanRetry' -or $cancelHost.ManualPrompts -ne 1 -or
    $cancelPlc.R160Writes -ne 0 -or $cancelHost.PrepareCalls -ne 0) {
    throw 'A failed initial scan must show the manual-code prompt without acknowledging R160.'
}
Start-Sleep -Milliseconds 2100
Invoke-Poll $cancelCoordinator
if ($cancelScanner.TriggerCount -ne 2 -or $cancelHost.ManualPrompts -ne 1 -or
    $cancelPlc.R160Writes -ne 1 -or $cancelHost.PrepareCalls -ne 1 -or
    $cancelCoordinator.State.ToString() -ne 'WaitingFixturePressed') {
    throw 'Cancelling manual entry must retry scanning after about two seconds and continue when a barcode is then read.'
}

Write-Output 'PASS: customer PLC handshake order, NG-only sequential DT writes, retry gates, and manual tray-code fallback'
