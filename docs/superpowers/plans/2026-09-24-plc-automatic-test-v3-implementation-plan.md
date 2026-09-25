# PLC Automatic Test v3.0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a serialized PLC/scanner-driven automatic test workflow that reads R150/R151 every second, confirms the tray code through `serialPortTPM`, writes live and final NG grades to DT1001–DT1256, runs configured NG retests, and writes R161 only after the complete test cycle finishes.

**Architecture:** Keep the successful project's ASCII serial protocol inside a dedicated PLC/scanner transport layer. Keep the business sequence inside a non-reentrant state-machine coordinator; `frmTest` only supplies UI callbacks and invokes the existing test engine. One owner must control the PLC COM port in PLC serial mode so the new ASCII client cannot race the existing PLC connection.

**Tech Stack:** VB.NET WinForms targeting .NET Framework 4.8, `System.IO.Ports.SerialPort`, existing `frmTest.StarTest`, existing `Initsetting` persistence, PowerShell repository checks, MSBuild.

---

## Scope and invariants

- PLC serial protocol is the successful-case ASCII protocol, not the existing Modbus frame format, for the PLC COM-mode automation path.
- `autoStartFlag` is not used by the PLC automation path. The only automatic trigger chain is `R150 → tray code → R160 → R151 → test/retest → R161`.
- `R160=1` is written only after a valid tray code is accepted and assigned to `txtTray.Text`; PLC resets R150 to 0.
- `R151=1` is required before entering test preparation.
- `btnNeedNGRetest.Checked` and `cmbRetestNum` are snapshotted at cycle start. The number is the maximum number of additional NG-retest rounds after the initial test.
- The automatic path does not show the existing “是否复测” dialog. Manual test behavior remains unchanged unless a shared helper must be extracted.
- R161 is a final-cycle-complete signal, not a “全部合格” signal.
- A failed PLC write, incomplete retest, test exception, or unresolved final grade prevents R161.

## Files and responsibilities

- Create `PlcAutomationProtocol.vb`: command constants, response parsing, R/DT address validation, and successful-case DT frame construction.
- Create `PlcAutomationClient.vb`: owns the ASCII PLC `SerialPort`, serial configuration, synchronized request/response exchange, open/close lifecycle, `ReadR150`, `ReadR151`, `WriteR160`, `WriteR161`, and `WriteChannelGrade`.
- Create `TrayScannerClient.vb`: owns the scanner `SerialPort` (`serialPortTPM`), configures 9600/None/8/1, drains stale bytes before a scan, reads one complete scan with timeout, and normalizes CR/LF.
- Create `PlcAutomaticTestState.vb`: state enum and immutable cycle snapshot/result types; no UI or serial access.
- Create `PlcAutomaticTestCoordinator.vb`: one-second non-reentrant polling, state transitions, tray-code callback, test/retest callbacks, final DT synchronization, and R161 gating.
- Modify `Module1.vb`: expose default serial settings and shared automation instances/state without adding new global business flags.
- Modify `frmInit.vb`: preserve scanner/PLC port and baud-rate settings, apply defaults 9600 and 38400, and keep existing port-conflict validation.
- Modify `MDIForm.vb` and `MDIForm.Designer.vb`: create the single PLC/scanner transport owner at application startup and close it during shutdown; do not open the same COM port through two clients.
- Modify `StartupDeviceService.vb` and `frmDebug.vb`: use the same automation transport for PLC serial health checks and diagnostics; keep TCP/legacy paths isolated.
- Modify `frmTest.vb`: remove PLC automation responsibility from `Timer4_Tick`, connect coordinator callbacks, assign `txtTray.Text`, start the existing initial test, orchestrate automatic retest rounds, and report state/errors.
- Modify `MP-60A-RV-64Mem.vbproj`: include the new VB files in compile order.
- Create `tools\Test-PlcAutomationProtocol.ps1`: static protocol and address-mapping checks.
- Create `tools\Test-PlcAutomationStateMachine.ps1`: fake-transport state-transition checks.

## Task 1: Lock serial defaults and shared transport ownership

**Files:**
- Modify: `Module1.vb`
- Modify: `frmInit.vb`
- Modify: `MDIForm.vb`
- Modify: `MDIForm.Designer.vb`
- Modify: `StartupDeviceService.vb`
- Modify: `frmDebug.vb`
- Test: `tools\Test-PlcAutomationProtocol.ps1`

- [ ] **Step 1: Add explicit default constants and normalize persisted settings.**

  Define the defaults in one place:

  ```vbnet
  Public Const DefaultPlcBaudRate As Integer = 38400
  Public Const DefaultScannerBaudRate As Integer = 9600
  Public Const PlcPortLogicalName As String = "serialPortPLC"
  Public Const ScannerPortLogicalName As String = "serialPortTPM"
  ```

  Keep `Initsetting.PLCComPort`, `Initsetting.PLCComRate`, `Initsetting.ScanerPort`, and `Initsetting.ScanerRate` as the persisted values. Normalize non-positive rates to the constants before opening ports.

- [ ] **Step 2: Correct `frmInit` default selection and scanner-rate loading.**

  Make the PLC COM rate default to 38400 and the scanner rate default to 9600. Fix the existing scanner-rate restore comparison so it compares against `Initsetting.ScanerRate`, not `Initsetting.ScanerPort`. Preserve the existing scanner/PLC/retest-port collision validation.

- [ ] **Step 3: Establish one owner per physical port.**

  The application must not simultaneously open the PLC COM port through the existing private `PlcController` serial port and the new ASCII client. In PLC serial mode, route health checks and automation through the shared ASCII transport; leave the existing TCP path isolated. Reuse the existing `Com_scaner` object only through the new logical `serialPortTPM` owner or replace it with one shared instance, never a second scanner port.

- [ ] **Step 4: Add startup/shutdown wiring.**

  Open both ports once after settings are loaded and before automation polling starts. Close them from the existing MDI shutdown path and from failed startup cleanup. Do not close a port owned by another component.

- [ ] **Step 5: Run the static configuration checks.**

  Run:

  ```powershell
  powershell -ExecutionPolicy Bypass -File .\tools\Test-PlcAutomationProtocol.ps1
  ```

  Expected: PASS for default baud rates, logical names, and single-owner lifecycle references.

## Task 2: Implement and test the ASCII PLC/scanner protocol layer

**Files:**
- Create: `PlcAutomationProtocol.vb`
- Create: `PlcAutomationClient.vb`
- Create: `TrayScannerClient.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools\Test-PlcAutomationProtocol.ps1`

- [ ] **Step 1: Add protocol constants and frame builders.**

  Use the successful-case frames exactly once in the protocol layer:

  ```vbnet
  ReadR150Frame = "%01#CSR0150**" & vbCr
  ReadR151Frame = "%01#RCSR0151**" & vbCr
  WriteR160Frame = "%01#WCSR01601**" & vbCr
  WriteR161Frame = "%01#WCSR01611**" & vbCr
  ```

  Build the DT frame from `channelNumber` and `grade`, validating channels 1–256 and deriving `1000 + channelNumber`. Do not allow callers to pass arbitrary DT address strings.

- [ ] **Step 2: Implement response parsing without unsafe `Substring(6, 1)`.**

  Require a minimum response length, validate the response prefix/status shape used by the successful PLC, and accept only signal values `0` or `1`. Throw a typed protocol exception for malformed or PLC-error responses. Keep the response offset in one protocol constant so it is not duplicated.

- [ ] **Step 3: Implement serialized request/response exchange.**

  `PlcAutomationClient` must use one `SyncLock` around discard, write, response read, and response-buffer cleanup. Configure `NewLine = vbCr`, read/write timeouts, odd parity/8/1, and exactly one CR per frame. Do not combine `WriteLine` with a frame that already contains `vbCr`.

- [ ] **Step 4: Implement scanner reads with a complete-frame timeout.**

  `TrayScannerClient` must discard stale input before a scan, wait for incoming bytes, accumulate until CR/LF or an inter-byte quiet period, trim the result, and return a single normalized string. A late or empty scan must return a controlled validation failure rather than silently discarding newly arrived bytes.

- [ ] **Step 5: Add protocol tests.**

  Test the following exact cases in `tools\Test-PlcAutomationProtocol.ps1`:

  ```text
  R150 frame ends with one CR
  R151 frame ends with one CR
  R160/R161 frames contain the correct register/value
  channel 1 maps to DT1001
  channel 256 maps to DT1256
  channel 0 and 257 are rejected
  malformed signal responses are rejected
  ```

  Run the script and require PASS before continuing.

## Task 3: Add the state model and one-second coordinator

**Files:**
- Create: `PlcAutomaticTestState.vb`
- Create: `PlcAutomaticTestCoordinator.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools\Test-PlcAutomationStateMachine.ps1`

- [ ] **Step 1: Define the states and cycle snapshot.**

  Define these states:

  ```vbnet
  Idle
  ReadingTrayCode
  WaitingManualTrayCode
  WaitingFixturePressed
  PreparingTest
  InitialTesting
  AutoRetesting
  Finalizing
  WaitingCycleReset
  Fault
  ```

  The cycle snapshot must contain the tray code, `NeedNgRetest`, `MaxRetestRounds`, current retest round, current NG channel set, and final DT write status.

- [ ] **Step 2: Implement the 1000 ms non-reentrant poll.**

  Add one coordinator timer with `Interval = 1000`. Use an atomic/busy guard so a slow PLC request cannot overlap the next tick. Poll R150 only in `Idle` and poll R151 only in `WaitingFixturePressed`.

- [ ] **Step 3: Implement R150-to-R160 transitions.**

  On R150=1, read and validate the scanner result. On success, invoke the UI callback that assigns `txtTray.Text`, snapshot the tray code, write R160=1, and transition to `WaitingFixturePressed`. Never write R160 for an empty or invalid code. Do not write R150 from the application because PLC resets it after R160.

- [ ] **Step 4: Implement the R151 gate.**

  Only R151=1 can transition `WaitingFixturePressed` to `PreparingTest`. Validate PLC, test-instrument, control-board, alarm, and automatic-mode preconditions before invoking the initial-test callback. A failed precondition leaves the cycle waiting or moves it to `Fault` according to the existing device-error policy; it never sends R161.

- [ ] **Step 5: Add fake-transport state tests.**

  `tools\Test-PlcAutomationStateMachine.ps1` must verify:

  ```text
  R150=0 does nothing
  R150=1 with invalid scan does not write R160
  valid scan assigns tray code before R160
  R160 success moves to WaitingFixturePressed
  R151=0 does not start a test
  R151=1 starts exactly one initial test
  a held R151 does not start a second test
  R161 is never emitted before finalization
  ```

## Task 4: Integrate initial testing, automatic NG retest, and finalization

**Files:**
- Modify: `frmTest.vb`
- Modify: `PlcAutomaticTestCoordinator.vb`
- Test: `tools\Test-PlcAutomationStateMachine.ps1`

- [ ] **Step 1: Remove PLC automation triggering from `Timer4_Tick`.**

  Stop using `autoStartFlag` and the existing timer branch that calls `StarTest(1)` from the legacy control-board signal. Keep manual button behavior separate. The coordinator becomes the only PLC automatic caller of the initial test entry point.

- [ ] **Step 2: Snapshot retest controls at cycle start.**

  Read `btnNeedNGRetest.Checked` and the integer value of `cmbRetestNum` when `PreparingTest` begins. Treat the value as additional retest rounds. Reject negative values; treat zero as no additional round and display a configuration warning if retest is selected with zero rounds.

- [ ] **Step 3: Start the existing initial test through a completion callback.**

  Invoke the existing `StarTest(1)` path without changing its measurement algorithm. The automatic coordinator must receive a completion/failure callback only after result saving and initial NG determination have completed. Do not send R161 from the initial-test completion path.

- [ ] **Step 4: Run retests without the existing decision dialog.**

  If retest is disabled, the NG set is empty, or the round limit is zero, transition directly to `Finalizing`. Otherwise invoke the existing NG-test path for the current NG set, increment the round, recalculate the remaining NG set, and repeat until the set is empty or the configured round limit is reached. The PLC automatic path must not show the existing “是否复测” dialog.

- [ ] **Step 5: Preserve manual behavior.**

  Keep the current manual `CmdStart`/NG-dialog behavior unless extracting a shared result-finalization helper is necessary. Ensure the automatic path has an explicit flag or separate callback so manual dialogs cannot block the PLC coordinator.

- [ ] **Step 6: Test retest transitions.**

  Add cases for:

  ```text
  retest unchecked => no retest and one finalization
  retest checked with no NG => no retest
  retest checked with N=2 => at most two additional rounds
  an all-OK retest exits early
  remaining NG after the last round still finalizes as NG
  initial completion never sends R161
  ```

## Task 5: Write live and final DT grades and gate R161

**Files:**
- Modify: `frmTest.vb`
- Modify: `PlcAutomaticTestCoordinator.vb`
- Modify: `PlcAutomationClient.vb`
- Test: `tools\Test-PlcAutomationProtocol.ps1`
- Test: `tools\Test-PlcAutomationStateMachine.ps1`

- [ ] **Step 1: Connect channel-result callbacks to live NG writes.**

  When an initial or retest channel result becomes NG, call `WriteChannelGrade(channel, 2)` through the serialized PLC client and record the channel in the cycle's pending/final NG set. Do not write grades from the one-second poll callback.

- [ ] **Step 2: Apply the verified PLC final-grade policy after retest.**

  The confirmed protocol defines only the NG write (`grade = 2`) and explicitly says that OK channels are not written. Before implementing a retest-to-OK overwrite, verify which concrete PLC behavior is used on the machine: either the PLC clears DT1001–DT1256 at the start of the cycle/retest, or the PLC provides a documented OK/clear frame. Implement only the verified behavior in the protocol helper; never invent an OK value. Until that behavior is verified, the coordinator must still track the software final result and must not claim that DT values were fully synchronized after an NG-to-OK retest.

- [ ] **Step 3: Gate R161 on all completion conditions.**

  `Finalizing` may write R161 only when the test engine reports completion, all configured retest rounds are done, final data is saved, and every required DT write has succeeded. Record the R161 write as a one-time cycle action.

- [ ] **Step 4: Add failure tests.**

  Verify that a failed DT write, failed final save, failed retest, or failed R161 write moves the coordinator to `Fault` and never reports the cycle as complete.

## Task 6: Re-arm, shutdown, diagnostics, and build verification

**Files:**
- Modify: `MDIForm.vb`
- Modify: `frmTest.vb`
- Modify: `StartupDeviceService.vb`
- Modify: `frmDebug.vb`
- Modify: `PlcAutomaticTestCoordinator.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools\Test-PlcAutomationProtocol.ps1`
- Test: `tools\Test-PlcAutomationStateMachine.ps1`

- [ ] **Step 1: Re-arm only after R151 returns to zero.**

  After R161 succeeds, enter `WaitingCycleReset`. Do not accept another R150 cycle until R151=0 and the prior cycle has been cleared. R150 is expected to have already been reset by PLC after R160.

- [ ] **Step 2: Add concise state and fault diagnostics.**

  Report state transitions and protocol failures through the existing test-status/logging path. Avoid a message box on every one-second retry. Show user action only for invalid tray input, unrecoverable configuration, or a fault requiring intervention.

- [ ] **Step 3: Stop safely during shutdown.**

  Stop the coordinator before closing ports, cancel any pending automatic cycle, prevent late UI callbacks, and close `serialPortPLC`/`serialPortTPM` exactly once.

- [ ] **Step 4: Run repository checks and build.**

  Run:

  ```powershell
  powershell -ExecutionPolicy Bypass -File .\tools\Test-PlcAutomationProtocol.ps1
  powershell -ExecutionPolicy Bypass -File .\tools\Test-PlcAutomationStateMachine.ps1
  msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform="AnyCPU"
  ```

  Expected: both scripts PASS and MSBuild completes without errors.

- [ ] **Step 5: Perform hardware acceptance checks.**

  With the actual PLC and scanner connected, verify:

  ```text
  scanner input populates txtTray
  invalid scan opens manual input and does not write R160
  valid scan writes R160 and PLC resets R150
  R151=0 never starts testing
  R151=1 starts exactly one initial test
  NG channels write DT1001..DT1256=2
  configured retests run automatically without the dialog
  R161 is written only after the final retest/finalization
  held R151 does not start another cycle
  ```

## Implementation order and checkpoints

1. Complete Task 1 and verify there is one owner per physical PLC/scanner port.
2. Complete Task 2 and verify exact frames and response parsing with a fake serial transport.
3. Complete Task 3 and verify the state machine without invoking the real test engine.
4. Complete Task 4 and verify initial/retest lifecycle with fake callbacks.
5. Complete Task 5 and verify DT/R161 gating.
6. Complete Task 6, build, then perform hardware acceptance.

Each task should be committed separately with a focused message. Do not begin hardware testing until the protocol and state-machine scripts pass.
