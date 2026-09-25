# PLC Automatic Test v4.0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a safe PLC-driven automatic test cycle that uses the successful ASCII serial protocol, owns each physical COM port exactly once, runs a non-interactive 256-channel test and configured NG retests, publishes final data only after all retests, and sends R161 only after the final PLC grade state is complete.

**Architecture:** Separate transport, cycle orchestration, one-pass testing, result publication, and UI adapters. `PlcAsciiTransport` owns the PLC COM port and serializes every PLC request; `TrayScannerTransport` owns the scanner port; `AutomaticTestCoordinator` owns the cycle state; `TestPassRunner` executes one measurement pass without dialogs or final publication; `FinalResultPublisher` saves/uploads the final result once. The existing manual test path remains available through its UI entry point.

**Tech Stack:** VB.NET WinForms targeting .NET Framework 4.8, `System.IO.Ports.SerialPort`, existing `FormalTestRunner`, existing OCV_DATA/MES services, PowerShell checks, MSBuild.

---

## Non-negotiable v4.0 rules

- In PLC COM mode, one shared ASCII transport owns the physical PLC port. The existing private Modbus serial port cannot be opened at the same time. TCP mode remains isolated.
- `autoStartFlag` is not part of PLC automation.
- The PLC automatic path does not display the existing NG-retest decision dialog.
- Automatic testing always uses channels 1–256, independent of the current UI selection.
- R150 starts tray-code acquisition. A valid code is assigned to `txtTray.Text` before R160=1 is sent. PLC resets R150 after R160.
- R151=1 is accepted only after the R150/R160 phase is complete and the R151 cycle-arm rule is satisfied.
- `btnNeedNGRetest.Checked` and `cmbRetestNum` are snapshotted for the cycle. `cmbRetestNum` means additional NG-retest rounds after the initial test.
- Initial testing is not final completion. OCV_DATA finalization, local final-file output, MES upload, DT finalization, and R161 happen only after all configured retests finish.
- R161 means “the complete cycle is finished”; it does not mean “all channels passed”.
- No DT value for OK/clear may be invented. The exact PLC clear policy is a pre-implementation gate.

## Required pre-implementation gates

### Gate A: Select the PLC serial owner

For `PLCMode = 0`, `PlcAsciiTransport` becomes the sole owner of `serialPortPLC`. Update startup checks, diagnostics, and automatic flow to use it. Do not instantiate or connect the existing `ModbusRtuPlcController` on the same COM port. Keep the existing TCP controller available only for `PLCMode = 1`.

### Gate B: Confirm the R151 signal contract

The implementation will arm a new cycle only after observing R151=0, then accept the next R151=1. This prevents a held high or stale signal from starting a duplicate test. Hardware acceptance must confirm that PLC returns R151 to 0 after the fixture cycle.

### Gate C: Confirm DT final-grade behavior

The confirmed protocol defines NG write `grade=2` and no OK write. Before implementing NG-to-OK retest reconciliation, verify on the PLC whether DT1001–DT1256 are cleared by PLC at cycle/retest start or whether a documented clear frame exists. Implement only that verified behavior. Until then, the coordinator must not report DT synchronization as complete after an NG-to-OK retest.

## Files and responsibilities

- Create `PlcAsciiProtocol.vb`: successful-case command frames, response validation, R/DT address validation, and DT frame construction.
- Create `PlcAsciiTransport.vb`: sole ASCII PLC `SerialPort` owner, 38400/Odd/8/1 configuration, synchronized request/response exchange, open/close, R150/R151 reads, R160/R161 writes, and grade writes.
- Create `TrayScannerTransport.vb`: sole scanner `SerialPort` owner (`serialPortTPM`), 9600/None/8/1 configuration, complete-frame reads, and normalized barcode output.
- Create `AutomaticTestState.vb`: states, cycle snapshot, pass result, final result, and fault records; no UI or serial access.
- Create `AutomaticTestCoordinator.vb`: 1000 ms non-reentrant polling, R150/R151 transitions, low-to-high R151 arming, manual barcode continuation, test/retest loop, finalization gate, and R161.
- Create `TestPassRunner.vb`: non-interactive one-pass adapter around the existing measurement flow; accepts an explicit channel set and returns a pass result without database finalization, MES upload, or retest dialogs.
- Create `PlcGradeWriteQueue.vb`: serialized, acknowledged DT write queue with a drain barrier for finalization.
- Create `FinalResultPublisher.vb`: one final OCV_DATA save, local file save, and MES upload after all testing/retesting is complete.
- Modify `Module1.vb`: default serial constants and shared runtime references without adding new trigger flags.
- Modify `frmInit.vb`: correct scanner/PLC default rate loading and preserve port conflict checks.
- Modify `MDIForm.vb`, `MDIForm.Designer.vb`, `StartupDeviceService.vb`, and `frmDebug.vb`: shared port lifecycle, diagnostics, and no duplicate PLC connection.
- Modify `FormalTestRunner.vb` and `frmTest.vb`: expose per-channel result callbacks, extract non-interactive pass behavior, preserve manual UI behavior, and connect the coordinator.
- Modify `MP-60A-RV-64Mem.vbproj`: include the new files.
- Create `tools\Test-PlcV4Protocol.ps1`: protocol, settings, and address checks.
- Create `tools\Test-PlcV4StateMachine.ps1`: fake transport/runner/publisher transition checks.

## Task 0: Resolve the three integration gates

**Files:**
- Inspect: `PlcController.vb`
- Inspect: `StartupDeviceService.vb`
- Inspect: `frmDebug.vb`
- Inspect: `frmTest.vb`
- Document: `docs\superpowers\plans\2026-09-24-plc-automatic-test-v4-implementation-plan.md`

- [ ] **Step 1: Confirm the runtime PLC mode.**

  Confirm the production station uses the ASCII serial protocol in `PLCMode=0`. If TCP mode is selected, automatic ASCII polling must remain disabled and show a configuration fault instead of silently opening a COM port.

- [ ] **Step 2: Confirm R151 low-to-high behavior.**

  On the actual PLC, observe one fixture cycle and verify the sequence `R151=0 → R151=1 → R151=0`. Record the observed result in the implementation log before enabling automatic re-arm.

- [ ] **Step 3: Confirm DT clear/OK behavior.**

  Test a channel with initial NG followed by retest OK. Verify whether PLC clears the previous DT=2 automatically or supplies a documented clear command. Encode the verified behavior in `PlcAsciiProtocol.vb` and add a protocol test; do not use an assumed value.

## Task 1: Implement one-owner serial configuration and lifecycle

**Files:**
- Modify: `Module1.vb`
- Modify: `frmInit.vb`
- Modify: `MDIForm.vb`
- Modify: `MDIForm.Designer.vb`
- Modify: `StartupDeviceService.vb`
- Modify: `frmDebug.vb`
- Create: `PlcAsciiTransport.vb`
- Create: `TrayScannerTransport.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: Normalize settings and defaults.**

  Define and use:

  ```vbnet
  Public Const DefaultPlcBaudRate As Integer = 38400
  Public Const DefaultScannerBaudRate As Integer = 9600
  Public Const PlcPortLogicalName As String = "serialPortPLC"
  Public Const ScannerPortLogicalName As String = "serialPortTPM"
  ```

  Keep `Initsetting.PLCComPort`, `Initsetting.PLCComRate`, `Initsetting.ScanerPort`, and `Initsetting.ScanerRate`. Correct the existing scanner-rate restore comparison to use `Initsetting.ScanerRate`. Normalize non-positive rates before port creation.

- [ ] **Step 2: Make the ASCII transport the only PLC COM owner.**

  In PLC serial mode, prevent `PlcControllerFactory.Create`/`ModbusRtuPlcController.Connect` from opening the same port. Route startup checks and `frmDebug` PLC diagnostics through `PlcAsciiTransport`. Leave the TCP controller path unchanged.

- [ ] **Step 3: Configure ports exactly once.**

  Configure PLC as 38400/Odd/8/One and scanner as 9600/None/8/One. Set `NewLine=vbCr`, read/write timeouts, and one owner per physical port. Open after settings load and close during the existing MDI shutdown path.

- [ ] **Step 4: Add settings/lifecycle checks.**

  Verify old settings load without exception, defaults are applied only when values are missing/invalid, duplicate-open attempts are rejected, and shutdown closes each owned port once.

## Task 2: Implement the ASCII protocol and safe frame parsing

**Files:**
- Create: `PlcAsciiProtocol.vb`
- Modify: `PlcAsciiTransport.vb`
- Modify: `TrayScannerTransport.vb`
- Test: `tools\Test-PlcV4Protocol.ps1`

- [ ] **Step 1: Add successful-case command frames.**

  Centralize these frames with exactly one trailing CR:

  ```vbnet
  ReadR150Frame = "%01#CSR0150**" & vbCr
  ReadR151Frame = "%01#RCSR0151**" & vbCr
  WriteR160Frame = "%01#WCSR01601**" & vbCr
  WriteR161Frame = "%01#WCSR01611**" & vbCr
  ```

- [ ] **Step 2: Add safe response validation.**

  Validate response length, protocol prefix, success/error shape, and signal value. Accept only `0` or `1`; replace direct `Substring(6, 1)` calls with one parser. Return a typed protocol error for malformed or error responses.

- [ ] **Step 3: Add serialized request/response exchange.**

  Wrap buffer discard, write, read, and response cleanup in one lock. Use a bounded timeout and never call `WriteLine` with a frame that already contains CR. Require the expected write acknowledgement before reporting R160, DT, or R161 success.

- [ ] **Step 4: Add complete scanner-frame reads.**

  Clear stale scanner input before a new scan, wait for data, collect through CR/LF or inter-byte quiet timeout, normalize the string, and return a controlled timeout/empty result. Do not discard the buffer after the first premature `ReadExisting` call.

- [ ] **Step 5: Add protocol tests.**

  Verify R150/R151/R160/R161 frames, one CR only, DT1001 and DT1256 mapping, invalid channel rejection, malformed responses, duplicate-open rejection, and scanner CR/LF normalization.

## Task 3: Extract a non-interactive one-pass test runner

**Files:**
- Create: `TestPassRunner.vb`
- Modify: `FormalTestRunner.vb`
- Modify: `frmTest.vb`
- Test: `tools\Test-PlcV4StateMachine.ps1`

- [ ] **Step 1: Define an explicit pass contract.**

  Use an explicit request/result shape:

  ```vbnet
  Public Class TestPassRequest
      Public Property Channels As IReadOnlyList(Of Integer)
      Public Property IsNgRetest As Boolean
  End Class

  Public Class TestPassResult
      Public Property Completed As Boolean
      Public Property ChannelResults As IReadOnlyDictionary(Of Integer, ChannelResult)
      Public Property ErrorMessage As String
  End Class
  ```

  The runner must never show the OCV confirmation, duplicate-tray, empty-selection, or NG-retest decision dialogs.

- [ ] **Step 2: Make the automatic initial request explicit for all 256 channels.**

  Construct `Enumerable.Range(1, 256)` for the automatic initial pass. Do not use `SelectedChannelNumbers` or the current UI selection. Validate that the request contains exactly channels 1–256 before starting.

- [ ] **Step 3: Keep test setup and measurement, remove final publication.**

  Reuse the existing `FormalTestRunner` measurement path, but move these operations out of the one-pass runner: `SaveOcvDataTestResults`, final task completion, local final-file publication, MES upload, and the NG decision dialog.

- [ ] **Step 4: Replace global completion assumptions.**

  Return explicit success/failure and channel results instead of relying on `isRuning`, `haveNgPoint`, or a `StarTest` return value that cannot distinguish cancellation from success.

- [ ] **Step 5: Test pass behavior.**

  Verify the automatic pass uses all 256 channels, does not show a modal dialog, returns a failure result on device error, and does not mark OCV_DATA/MES complete by itself.

## Task 4: Add per-channel results and serialized DT writes

**Files:**
- Create: `PlcGradeWriteQueue.vb`
- Modify: `FormalTestRunner.vb`
- Modify: `TestPassRunner.vb`
- Modify: `PlcAsciiTransport.vb`
- Modify: `AutomaticTestCoordinator.vb`
- Test: `tools\Test-PlcV4Protocol.ps1`
- Test: `tools\Test-PlcV4StateMachine.ps1`

- [ ] **Step 1: Add a per-channel result callback.**

  Publish a result after voltage/resistance are available and before the pass is reported complete:

  ```vbnet
  ChannelResultReady(channelNumber, voltage, resistance, result)
  ```

  The callback must be safe when invoked from a worker thread and must not update WinForms controls directly.

- [ ] **Step 2: Queue NG grade writes.**

  When a result is NG, enqueue `channelNumber → grade 2`. The queue owns the PLC write lock, records acknowledgement/failure, and exposes `AwaitDrained()` for finalization. The one-second R150/R151 poll must use the same serialized transport lock.

- [ ] **Step 3: Avoid measurement-thread blocking.**

  A DT write timeout must not deadlock the measurement engine. The queue records the error and causes the cycle to fail at the finalization barrier; it does not silently drop the NG write.

- [ ] **Step 4: Implement the verified final DT policy.**

  After the Gate C decision, implement either the PLC cycle/retest clear behavior or the documented clear frame. Add a test for initial NG followed by retest OK. Do not mark final DT synchronization complete without this verified behavior.

## Task 5: Implement the v4.0 cycle coordinator

**Files:**
- Create: `AutomaticTestState.vb`
- Create: `AutomaticTestCoordinator.vb`
- Modify: `frmTest.vb`
- Test: `tools\Test-PlcV4StateMachine.ps1`

- [ ] **Step 1: Define the states.**

  Use:

  ```text
  Idle
  ReadingTrayCode
  WaitingManualTrayCode
  WaitingR150Reset
  ArmingR151
  WaitingFixturePressed
  PreparingTest
  InitialTesting
  AutoRetesting
  Finalizing
  WaitingCycleReset
  Fault
  ```

- [ ] **Step 2: Implement the 1000 ms non-reentrant poll.**

  The coordinator timer only performs short state checks. Use a busy guard, route UI callbacks through the UI synchronization context, and never invoke the blocking test pass on the timer callback thread.

- [ ] **Step 3: Implement R150 and scanner flow.**

  On R150=1, acquire and validate a complete tray code. On success, assign `txtTray.Text`, write R160=1, wait for the PLC acknowledgement and R150 reset, then enter `ArmingR151`. Invalid input stays in the scan/manual-input state and does not write R160.

- [ ] **Step 4: Implement R151 low-to-high arming.**

  Observe R151=0 before accepting R151=1. Only then enter `PreparingTest`. R151=0 never starts a test. A held R151 cannot retrigger the cycle.

- [ ] **Step 5: Snapshot the retest controls.**

  Capture `btnNeedNGRetest.Checked` and a validated non-negative `cmbRetestNum` value at the beginning of `PreparingTest`. The snapshot cannot change during the cycle.

- [ ] **Step 6: Implement the initial/retest loop.**

  Run one initial pass for all 256 channels. Build the current NG set from returned results. If retest is enabled and the set is non-empty, run only that set for each additional round until the set is empty or the configured limit is reached. Do not show the existing manual retest dialog.

- [ ] **Step 7: Add coordinator transition tests.**

  Verify invalid scans, R160 failure, stale R151 high, R151=0, R151=1, held R151, all-OK initial pass, zero NG, early retest exit, maximum retest rounds, pass failure, and no R161 before finalization.

## Task 6: Publish final results exactly once and gate R161

**Files:**
- Create: `FinalResultPublisher.vb`
- Modify: `frmTest.vb`
- Modify: `AutomaticTestCoordinator.vb`
- Modify: existing OCV_DATA/MES integration call sites in `frmTest.vb`
- Test: `tools\Test-PlcV4StateMachine.ps1`

- [ ] **Step 1: Move publication after the retest loop.**

  Do not call final `SaveOcvDataTestResults`, final OCV task completion, `SaveTestDataLocalFile`, or MES upload after the initial pass. Call them once from `FinalResultPublisher` after all configured retests and final DT synchronization succeed.

- [ ] **Step 2: Publish the final channel result set.**

  Build the final `RecieveData`/channel result snapshot from the initial and retest results. Ensure a retest result replaces the corresponding final channel result without losing unrelated channels.

- [ ] **Step 3: Add the R161 completion barrier.**

  Write R161 only after:

  ```text
  final test pass completed
  all configured retests completed
  final channel results saved
  final DT writes acknowledged
  OCV_DATA finalization succeeded
  local final file succeeded
  MES enqueue succeeded or the existing upload policy explicitly allows background enqueue
  ```

  A failure moves the coordinator to `Fault` and does not write R161.

- [ ] **Step 4: Test exactly-once finalization.**

  Verify that initial completion, each retest completion, retrying R150, or a held R151 cannot duplicate database writes, MES submissions, DT writes, or R161.

## Task 7: Re-arm, recovery, diagnostics, and hardware verification

**Files:**
- Modify: `MDIForm.vb`
- Modify: `frmTest.vb`
- Modify: `StartupDeviceService.vb`
- Modify: `frmDebug.vb`
- Modify: `AutomaticTestCoordinator.vb`
- Create/modify: `tools\Test-PlcV4Protocol.ps1`
- Create/modify: `tools\Test-PlcV4StateMachine.ps1`

- [ ] **Step 1: Re-arm only after the cycle reset.**

  After R161 succeeds, wait for R151=0, clear the cycle snapshot, and return to Idle. Do not accept a new R150 while the previous cycle is finalizing or faulted.

- [ ] **Step 2: Define fault recovery.**

  On PLC/test/DT/final-publication failure, stop automatic progression, record the state and reason, prevent R161, and require operator reset/retry. A retry must not silently reuse stale NG sets, stale barcode data, or a partially published cycle.

- [ ] **Step 3: Handle shutdown and form disposal.**

  Stop polling, cancel pending callbacks, drain or cancel the grade queue safely, prevent late UI updates, and close `serialPortPLC`/`serialPortTPM` once.

- [ ] **Step 4: Run automated checks and build.**

  Run:

  ```powershell
  powershell -ExecutionPolicy Bypass -File .\tools\Test-PlcV4Protocol.ps1
  powershell -ExecutionPolicy Bypass -File .\tools\Test-PlcV4StateMachine.ps1
  msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform="AnyCPU"
  ```

  Expected: both scripts PASS and MSBuild completes without errors.

- [ ] **Step 5: Perform hardware acceptance.**

  Verify on the actual station:

  ```text
  one PLC/scanner COM owner is active
  R150=1 obtains a complete valid tray code
  txtTray.Text is assigned before R160
  invalid scan does not write R160
  PLC resets R150 after R160
  R151 stale high cannot start a cycle
  R151 0→1 starts exactly one all-256-channel initial pass
  NG channels write DT1001..DT1256=2
  configured retests run without the dialog
  retest-to-OK follows the verified DT clear policy
  final data is saved/uploaded once
  R161 is written only after finalization
  held R151 does not start another cycle
  ```

## Implementation checkpoints

1. Do not code the coordinator until Gates A–C are recorded.
2. Do not integrate `StarTest` until the non-interactive one-pass contract is tested.
3. Do not enable live DT writes until the per-channel callback and write queue are tested.
4. Do not send R161 until final publication and DT synchronization have a tested barrier.
5. Do not begin production hardware testing until the protocol script, state-machine script, and MSBuild all pass.

Each task should be committed separately with a focused message. Keep v3.0 as the previous design record; v4.0 is the implementation baseline.
