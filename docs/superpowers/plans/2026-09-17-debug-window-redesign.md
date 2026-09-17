# 调试中心窗口重设计 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在不改变正式测试协议和业务流程的前提下，把 `frmDebug` 改造成包含设备总览、通信控制台、手动控制和运行日志的安全调试中心。

**Architecture:** 用 `DebugModels` 定义跨层数据，用 `DebugProbeService` 复用现有设备检查入口并返回无 UI 副作用的结果，用 `DebugDashboardControl` 动态构建 WinForms 界面；`frmDebug` 只做宿主适配。旧 Designer 控件保留但隐藏，降低对现有引用和窗体序列化的影响。

**Tech Stack:** VB.NET, .NET Framework 4.8, WinForms, PowerShell contract tests, existing `UiTheme`, `SerialMiddleMachineClient`, `IPlcController`.

---

### Task 1: Add a red contract test for the new debug center boundaries

**Files:**
- Create: `tools/Test-DebugDashboardContract.ps1`

- [ ] **Step 1: Write the failing test**

Create a PowerShell test that asserts the new model, probe service, dashboard control, project entries, and host mount point exist. The test must fail before production files are added.

- [ ] **Step 2: Run test to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugDashboardContract.ps1`

Expected: FAIL because the new files and project entries do not yet exist.

- [ ] **Step 3: Commit the test**

Run: `git add tools/Test-DebugDashboardContract.ps1 && git commit -m "test: define debug dashboard contract"`

### Task 2: Add shared debug models and device probe service

**Files:**
- Create: `DebugModels.vb`
- Create: `DebugProbeService.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: Implement the smallest model surface required by the failing contract**

Add the device enum, result object, log entry, and event argument types. Keep them free of WinForms controls.

- [ ] **Step 2: Implement safe probes**

Add synchronous `Probe` and `ProbeAll` methods. Reuse current checks, return structured failures, never show dialogs, and stop immediately when `isRuning` is true. Add a serial-state helper that reports configured port and baud rate without opening an unrelated port.

- [ ] **Step 3: Run the contract test**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugDashboardContract.ps1`

Expected: PASS for models, service, and project registration; host mount assertion remains red until Task 4.

- [ ] **Step 4: Commit the model/service slice**

Run: `git add DebugModels.vb DebugProbeService.vb MP-60A-RV-64Mem.vbproj tools/Test-DebugDashboardContract.ps1 && git commit -m "feat: add debug device probe service"`

### Task 3: Build the dashboard control and its four functional pages

**Files:**
- Create: `DebugDashboardControl.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: Add the dashboard surface**

Build a dock-filled user control with a header, four tabs, a device list, counters, per-device test buttons, a communication console, guarded manual controls, and a log tab. Use named controls and explicit event callbacks; do not use temporary button names.

- [ ] **Step 2: Add communication parsing and display**

Support Hex/ASCII mode, normalize hex input, display timestamped TX/RX lines, and disable sending when the selected interface is not supported or the test is running. Keep raw exchange in the host callback so the control has no global I/O dependency.

- [ ] **Step 3: Add status updates and UI-thread guards**

Expose `SetDeviceResult`, `SetDeviceTesting`, `AppendLog`, `SetSummary`, and `SetRunningState`; marshal updates with `InvokeRequired`/`BeginInvoke` and ignore updates after disposal.

- [ ] **Step 4: Run the contract test**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugDashboardContract.ps1`

Expected: PASS for the complete dashboard surface and project registration.

- [ ] **Step 5: Commit the dashboard slice**

Run: `git add DebugDashboardControl.vb MP-60A-RV-64Mem.vbproj tools/Test-DebugDashboardContract.ps1 && git commit -m "feat: add debug center dashboard"`

### Task 4: Mount the dashboard into `frmDebug` without breaking legacy references

**Files:**
- Modify: `frmDebug.vb`

- [ ] **Step 1: Add the host adapter**

After administrator authorization in `frmDebug_Load`, hide legacy controls, resize the form, mount the dashboard, and connect dashboard events to existing checks, safe middle-machine methods, serial raw exchange, and PLC actions.

- [ ] **Step 2: Preserve shared state safely**

Keep `mSock`/`mSocket` public behavior intact. Do not close/recreate shared communication sessions from a dashboard read unless the existing check method owns that lifecycle. Block actions while formal testing is running and log the block instead of showing modal errors.

- [ ] **Step 3: Add lifecycle cleanup**

Detach event handlers and dispose the dashboard on form disposal/closing; do not dispose globally owned serial ports or PLC instances from the child form.

- [ ] **Step 4: Run the contract and serialization checks**

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugDashboardContract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Assert-WinFormsDesignerSerialization.ps1
```

Expected: both PASS.

- [ ] **Step 5: Commit the host integration**

Run: `git add frmDebug.vb tools/Test-DebugDashboardContract.ps1 && git commit -m "feat: mount debug center in frmDebug"`

### Task 5: Verify the full implementation and review the diff

**Files:**
- Verify: all files changed by Tasks 1-4

- [ ] **Step 1: Run focused source checks**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugDashboardContract.ps1` and inspect the complete output.

- [ ] **Step 2: Run relevant existing checks**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Assert-UiDataNullSafety.ps1` and `powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify-control-board-status.ps1`.

- [ ] **Step 3: Attempt a clean project build**

Run: `dotnet msbuild MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal`. Record missing full-framework MSBuild or vendor dependencies separately from source errors.

- [ ] **Step 4: Review the diff and status**

Run: `git diff HEAD~4..HEAD --stat`, `git diff HEAD~4..HEAD --check`, and `git status --short`. Confirm no unrelated files changed and no temporary output is tracked.

- [ ] **Step 5: Commit any verification-only corrections**

If the review finds a real issue, add a focused fix and rerun the affected checks before committing; do not alter unrelated code.
