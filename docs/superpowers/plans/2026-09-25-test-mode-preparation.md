# Test Mode Preparation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make automatic and main-page manual production tests share validated tray preparation while preserving their distinct triggers and channel plans.

**Architecture:** Add a tray preparation service that validates the database mapping and returns an immutable channel-to-cell snapshot. The form applies that snapshot before measuring. The PLC coordinator carries one cycle through initial and NG passes and separates result publication from PLC completion notification.

**Tech Stack:** VB.NET WinForms, .NET Framework 4.8, PowerShell regression checks, MSBuild.

---

## Files and responsibilities

- `TrayTestPreparation.vb`: pure validation and channel planning for a queried tray.
- `frmTest.vb`: barcode acquisition adapter, shared preflight, measurement/result filtering, mode ownership.
- `PlcAutomaticTestCoordinator.vb`: cycle transitions and recovery after publication.
- `PlcAutomaticTestState.vb`: cycle snapshot and publication state.
- `PlcAutomationProtocol.vb`: strict PLC write response validation.
- `tools/Test-TrayTestPreparationRuntime.ps1`, `tools/Test-AutomaticOccupiedResultRuntime.ps1`: tray planning and result filtering checks.
- `tools/Test-PlcCycleRecoveryRuntime.ps1`, `tools/Test-PlcWriteAckRuntime.ps1`: state-machine and protocol checks.
- `tools/Test-MesUploadSnapshotRuntime.ps1`, `tools/Test-ManualPlcAsciiContract.ps1`: upload and manual PLC ownership checks.

## Task 1: Tray mapping and channel plan

- [ ] Write a failing test that feeds sparse tray cells with duplicated/invalid codes into `TrayTestPreparation` and checks automatic 256-channel measurement plus occupied-channel publication, and manual selected-channel validation.
- [ ] Run the targeted test and confirm the intended failure.
- [ ] Implement the validator and immutable snapshot; run the test again.
- [ ] Wire both entry points to query `usp_GetTrayCellsByTrayCode`, clear stale codes, apply the snapshot, and reject invalid data before measurement.

## Task 2: Shared preflight and mode ownership

- [ ] Write failing tests for starting manual while automatic owns PLC, switching modes mid-cycle, and missing manual clamp/device checks.
- [ ] Add a shared preflight that checks authentication, alarm, channel plan, middle-machine/instrument readiness and fixture state through the owning PLC client.
- [ ] Route automatic and manual starts through that preflight; disable conflicting entry points for the duration of a cycle; rerun tests.

## Task 3: Measurement and persistence

- [ ] Write failing tests for same-tray automatic NG retest, sparse automatic results, and manual save failure followed by NG.
- [ ] Scope duplicate tray detection to new cycles, filter empty slots from business results and NG retest, and suppress retest after save failure.
- [ ] Rerun tests and verify the database task channel list and MES list use occupied channels while the automatic runner measures 256.

## Task 4: PLC completion and recovery

- [ ] Write failing tests for malformed PLC ACK and final publication followed by R161 failure.
- [ ] Validate the expected PLC success response; keep published-cycle identity on notification failure and retry R161 without measuring or saving again.
- [ ] Rerun tests, then execute existing PLC and main-test checks.

## Task 5: Integration

- [ ] Run all targeted scripts, x86 Debug build, and inspect `git diff --check` plus the final diff.
- [ ] Document any real PLC or database behavior that still requires station acceptance testing.
