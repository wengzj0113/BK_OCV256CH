# PLC-owned test lifecycle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the upper-computer test page rely on the PLC-owned lifecycle and stop issuing PLC fixture commands during formal testing.

**Architecture:** Keep `frmTest.StarTest` responsible for channel selection, middle-machine measurement, result mapping, persistence, and UI state. Remove only the PLC preflight and post-test fixture control from that path; do not invent a PLC completion command that is absent from the current interfaces.

**Tech Stack:** VB.NET WinForms, .NET Framework 4.8, PowerShell regression scripts, Visual Studio MSBuild x86.

---

### Task 1: Lock the lifecycle boundary with a failing source regression test

**Files:**
- Create: `tools/Test-MainTestPlcLifecycle.ps1`
- Test: `frmTest.vb`

- [ ] **Step 1: Add assertions for the intended StarTest boundary**

The script must read `frmTest.vb`, isolate `Public Function StarTest` through `End Function`, and fail if that function contains `plcConnected`, `plcController`, `GetSignalTrayStatus`, `CloseTray`, or `TrayOpen`.

- [ ] **Step 2: Run the new test before implementation**

Run:

```powershell
& 'C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe' -NoProfile -ExecutionPolicy Bypass -File '.\tools\Test-MainTestPlcLifecycle.ps1'
```

Expected: FAIL because the current `StarTest` contains the PLC preflight and `TrayOpen` calls.

### Task 2: Remove PLC ownership conflicts from the main test path

**Files:**
- Modify: `frmTest.vb:595-618`
- Modify: `frmTest.vb:682-685`
- Modify: `frmTest.vb:756-768`

- [ ] **Step 1: Remove the pre-test PLC block**

Delete the `plcConnected`/`plcController` guard, `GetSignalTrayStatus`, `CloseTray`, nine-second wait, second status read, and `fixtureClose` validation from `StarTest`. Keep the preceding `CheckCom_SCM()` check and continue directly to measurement preparation.

- [ ] **Step 2: Remove normal and failure cleanup PLC actions**

Delete the post-measurement `isOpenOk = TrayOpen()` call and set `fixtureDownForRun` to `False` or remove the variable and its cleanup block so `exit_sub` cannot call `TrayOpen()`.

- [ ] **Step 3: Preserve non-PLC cleanup**

Keep `testingChannelNumbers.Clear()`, `RefreshChannelOverview()`, `currentTestChannels = Nothing`, control re-enabling, cursor restoration, and `isRuning = False`.

### Task 3: Verify the implementation

**Files:**
- Test: `tools/Test-MainTestPlcLifecycle.ps1`
- Test: existing channel-selection and Designer scripts

- [ ] **Step 1: Run the lifecycle regression test and existing source checks**

Run the lifecycle script, `Test-MainTestSelectionSnapshot.ps1`, `Test-MainTestTrayFailureFeedback.ps1` after updating the latter to assert no silent PLC branch is required, `Test-ChannelSelectionFeature.ps1`, and `Assert-WinFormsDesignerSerialization.ps1`.

- [ ] **Step 2: Rebuild the solution**

Run:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' '.\MP-60A-RV-64Mem.sln' /t:Rebuild /p:Configuration=Debug /p:Platform=x86 /m
```

Expected: `0 个错误` and `0 个警告`.

- [ ] **Step 3: Run runtime regressions**

Run `Test-ChannelSelectionRuntime.ps1` and `Test-FrmTestChannelSelectionRuntime.ps1` against `bin\Debug\XL_OCV_256CH.exe`, then run `git diff --check`.
