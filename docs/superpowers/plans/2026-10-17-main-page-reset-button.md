# Main Page Reset Button Implementation Plan

> **For agentic workers:** Follow the approved reset behavior below. Preserve unrelated workspace changes.

**Goal:** Make the main-page reset button safely cancel the current test cycle, reset test equipment, clear only current tray state, and release alarm locks only after their sources are confirmed normal.

**Architecture:** Keep PLC cycle invalidation in `PlcAutomaticTestCoordinator`, use the existing `FormalTestRunner` cleanup path, and run its cancellable measurement stage off the UI thread while pumping UI messages so Stop/Reset remains available. Collect device reset failures in `TestEnvironmentResetService`; keep temperature, PLC smoke input, other IO, and reset-failure latches separate so unverified alarm sources remain locked.

**Tech Stack:** VB.NET WinForms, .NET Framework 4.8, `System.Threading.Tasks`, existing serial/VISA clients, `BusinessLogService`.

---

### Task 1: Make device reset outcomes observable

**Files:**
- Modify: `SerialMiddleMachineClient.vb`
- Modify: `Module1.vb`
- Modify: `TestEnvironmentResetService.vb`

- [x] Treat a failed middle-machine stop/write, closed 3562 port, closed 34461 VISA session, or failed SCPI reset operation as a reset failure.
- [x] Return per-device failure details from `ResetAllWithResult` while preserving existing callers of `ResetAll`.

### Task 2: Keep the cancellable measurement responsive

**Files:**
- Modify: `FormalTestRunner.vb`
- Modify: `frmTest.vb`

- [x] Run sparse measurement I/O in a worker task and process UI messages while it is active.
- [x] Marshal result mapping back to the UI thread and check cancellation before mapping/saving.
- [x] Keep the reset button available during a test and block new test starts while reset is in progress.

### Task 3: Implement full software and alarm reset

**Files:**
- Modify: `frmTest.vb`
- Modify: `PlcAutomationProtocol.vb`
- Modify: `PlcAutomationClient.vb`
- Modify: `PlcAutomaticTestCoordinator.vb`

- [x] Stop the current PLC cycle, request cooperative worker cancellation, wait up to 30 seconds, and leave state intact if the worker does not exit.
- [x] Clear the active tray's incomplete measurements/barcodes and progress while preserving selections, operating settings, event history, and persisted results.
- [x] Continue temperature polling while latched and require fresh normal readings to clear temperature alarms; read PLC smoke input `X1F` every 2 seconds, show alarm/read failures on the main page, and clear its latch only after a fresh normal read and successful device reset. Keep unavailable temperature/emergency-stop sources and reset failures locked.
- [x] Record reset outcomes in the event and business logs and tell the operator to rearm PLC inputs.

### Task 4: Review and compile

**Files:**
- Review: `frmTest.vb`, `FormalTestRunner.vb`, `SerialMiddleMachineClient.vb`, `Module1.vb`, `TestEnvironmentResetService.vb`, `PlcAutomationProtocol.vb`, `PlcAutomationClient.vb`, `PlcAutomaticTestCoordinator.vb`

- [x] Check the scoped diff and compile the VB.NET project. No tests were added or run.
