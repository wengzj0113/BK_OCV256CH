$ErrorActionPreference = 'Stop'
$assemblyPath = if ($env:FRMTEST_EXE) { $env:FRMTEST_EXE } else { Join-Path $PSScriptRoot '..\bin\Debug\XL_OCV_256CH.exe' }
$assemblyPath = (Resolve-Path $assemblyPath).Path
$assembly = [Reflection.Assembly]::LoadFrom($assemblyPath)
$stateType = $assembly.GetType('software.PlcAutomaticTestState', $true)
if ([Enum]::GetNames($stateType) -notcontains 'WaitingTrayPreparationRecovery') {
    throw 'The coordinator does not expose a tray-preparation recovery state.'
}

$definition = @'
using System;
using System.Collections.Generic;
using software;

public sealed class RecoveryPlc : IPlcAutomationClient {
    public bool IsOpen { get { return true; } }
    public bool R150 = true, R151, R152;
    public int R160Writes, R161Writes, R162Writes;
    public void Open() {}
    public void Close() {}
    public bool ReadR150() { return R150; }
    public bool ReadR151() { return R151; }
    public bool ReadR152() { return R152; }
    public void WriteR160() { R160Writes++; }
    public void WriteR161() { R161Writes++; }
    public void WriteR162() { R162Writes++; }
    public void WriteNgSelection(int channel) {}
}

public sealed class RecoveryScanner : ITrayScannerClient {
    public string[] Codes = new string[] { "TRAY-BAD", "TRAY-NEXT" };
    public int TriggerCount;
    public bool IsOpen { get { return true; } }
    public void Open() {}
    public void Close() {}
    public void TriggerScan() { TriggerCount++; }
    public string ReadTrayCode(int timeoutMs) {
        int index = Math.Min(TriggerCount - 1, Codes.Length - 1);
        return Codes[index];
    }
}

public sealed class RecoveryHost : IAutomaticTestHost {
    public bool DebugMode;
    public string CurrentTrayCode;
    public int PrepareCalls, InitialCalls;
    public readonly Queue<Exception> PrepareFailures = new Queue<Exception>();
    public readonly List<string> PreparedCodes = new List<string>();
    public readonly List<string> Statuses = new List<string>();
    public bool IsAutomaticModeEnabled() { return true; }
    public bool IsDebugModeEnabled() { return DebugMode; }
    public bool CanStartAutomaticTest() { return true; }
    public bool ValidateTrayCode(string code) { return !String.IsNullOrWhiteSpace(code); }
    public bool GetNeedNgRetest() { return false; }
    public int GetRetestRoundCount() { return 0; }
    public bool RequiresNgSorting() { return false; }
    public void SetTrayCode(string code) { CurrentTrayCode = code; }
    public void PrepareAutomaticTray(string code) {
        PrepareCalls++;
        PreparedCodes.Add(code);
        if (PrepareFailures.Count > 0) throw PrepareFailures.Dequeue();
    }
    public void RequestManualTrayCode(Action<string> accepted, Action cancelled) { throw new Exception("Unexpected manual barcode prompt."); }
    public void StartInitialPass(Action<TestPassResult> completed, Action<Exception> failed) { InitialCalls++; completed(new TestPassResult { Completed = true }); }
    public void StartRetestPass(IReadOnlyList<int> channels, Action<TestPassResult> completed, Action<Exception> failed) { throw new Exception("Unexpected retest."); }
    public bool PublishFinalCycle(AutomaticTestCycle cycle) { return true; }
    public void ReportAutomaticStatus(string message) { Statuses.Add(message); }
    public void ReportAutomaticEvent(string message) {}
}
'@

$sourcePath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'TrayPreparationRecoveryFakes.cs'
$helperPath = Join-Path ([IO.Path]::GetDirectoryName($assemblyPath)) 'TrayPreparationRecoveryFakes.dll'
[IO.File]::WriteAllText($sourcePath, $definition)
& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe' /nologo /target:library "/out:$helperPath" "/reference:$assemblyPath" $sourcePath
if ($LASTEXITCODE -ne 0) { throw 'Tray-preparation recovery fakes failed to compile.' }
[void][Reflection.Assembly]::LoadFrom($helperPath)

$poll = $null
function New-RecoveryCoordinator($plc, $scanner, $hostAdapter) {
    $coordinator = [software.PlcAutomaticTestCoordinator]::new($plc, $scanner, $hostAdapter)
    if ($null -eq $script:poll) {
        $script:poll = $coordinator.GetType().GetMethod('Poll', [Reflection.BindingFlags]'Instance,NonPublic')
    }
    return $coordinator
}
function Invoke-RecoveryPoll($coordinator) { [void]$script:poll.Invoke($coordinator, @($null)) }

# A failed preparation retains the same tray and waits without writing R160.
$plc = [RecoveryPlc]::new()
$scanner = [RecoveryScanner]::new()
$hostAdapter = [RecoveryHost]::new()
$hostAdapter.PrepareFailures.Enqueue([InvalidOperationException]::new('No cell mapping.'))
$coordinator = New-RecoveryCoordinator $plc $scanner $hostAdapter
Invoke-RecoveryPoll $coordinator
if ($coordinator.State.ToString() -ne 'WaitingTrayPreparationRecovery' -or $plc.R160Writes -ne 0 -or
    $hostAdapter.InitialCalls -ne 0 -or $hostAdapter.PreparedCodes[0] -ne 'TRAY-BAD' -or
    $hostAdapter.CurrentTrayCode -ne 'TRAY-BAD') {
    throw 'A preparation failure must retain the tray and wait without confirming R160 or starting a test.'
}
Invoke-RecoveryPoll $coordinator
if ($hostAdapter.PrepareCalls -ne 1 -or $scanner.TriggerCount -ne 1) {
    throw 'A held R150 high level must not repeatedly scan or retry a failed preparation.'
}

# Enabling debug mode retries the saved tray once, then the coordinator proceeds through R160.
$hostAdapter.DebugMode = $true
$coordinator.RequestTrayPreparationRetry()
Invoke-RecoveryPoll $coordinator
if ($hostAdapter.PrepareCalls -ne 2 -or $scanner.TriggerCount -ne 1 -or $plc.R160Writes -ne 1 -or
    $coordinator.State.ToString() -ne 'WaitingFixturePressed' -or $hostAdapter.CurrentTrayCode -ne 'TRAY-BAD') {
    throw 'Enabling debug mode must retry the retained tray once and continue through R160 without rescanning.'
}
Invoke-RecoveryPoll $coordinator
if ($hostAdapter.PrepareCalls -ne 2 -or $scanner.TriggerCount -ne 1 -or $plc.R160Writes -ne 1) {
    throw 'A held R150 high level must not duplicate the accepted tray request.'
}

# R150 low releases the failed tray so a later rising edge can scan the next one.
$plc2 = [RecoveryPlc]::new()
$scanner2 = [RecoveryScanner]::new()
$host2 = [RecoveryHost]::new()
$host2.PrepareFailures.Enqueue([InvalidOperationException]::new('No cell mapping.'))
$coordinator2 = New-RecoveryCoordinator $plc2 $scanner2 $host2
Invoke-RecoveryPoll $coordinator2
$plc2.R150 = $false
Invoke-RecoveryPoll $coordinator2
if ($coordinator2.State.ToString() -ne 'Idle') { throw 'R150 low must release the unconfirmed preparation request.' }
$plc2.R150 = $true
Invoke-RecoveryPoll $coordinator2
if ($scanner2.TriggerCount -ne 2 -or $host2.PreparedCodes.Count -ne 2 -or
    $host2.PreparedCodes[1] -ne 'TRAY-NEXT' -or $host2.CurrentTrayCode -ne 'TRAY-NEXT' -or $plc2.R160Writes -ne 1) {
    throw 'A new R150 rising edge must scan and prepare the next tray instead of reusing the failed tray.'
}

Write-Output 'PLC tray-preparation recovery runtime: PASS'
