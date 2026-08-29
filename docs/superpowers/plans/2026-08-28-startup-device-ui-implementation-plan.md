# 启动设备检查与首屏 UI 解耦 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 让主页面在设备检查完成前先完成首屏显示，并把所有启动设备连接、重试、读取和错误反馈移出 UI 线程。

**Architecture:** 保留现有 WinForms MDI 结构，新增启动设备检查边界，后台完成检查和正式连接，UI 回调只应用状态结果。`frmTest` 采用必要首屏布局、首次绘制后的延后样式/定时器两阶段启动，并由共享 `StartupTiming` 记录阶段耗时。

**Tech Stack:** VB.NET、.NET Framework 4.8、WinForms、PowerShell 静态回归脚本、MSBuild。

---

### Task 1: 启动线程边界与 UI 回调

**Files:**
- Modify: `Module1.vb:699-820`
- Modify: `Module1.vb:587-625,1043-1085`
- Create: `StartupDeviceService.vb`
- Test: `tools/Test-StartupResizePerformance.ps1`

- [x] **Step 1: Write and run the failing static regression**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-StartupResizePerformance.ps1`

Expected: FAIL because `ApplyCheckResultsToUi` still contains `ConnectFor34461()`.

- [x] **Step 2: Add `StartupDeviceService.DeviceStartupCheck`**

The service runs SCM, 34461, 3561, PLC and MES checks sequentially on its caller thread. When the pre-check for 34461/3561 succeeds, it performs the final connection there with startup-dialog suppression. It returns a result object containing all flags and error text; it never accesses WinForms controls or shows a dialog.

- [x] **Step 3: Queue the service and make `ApplyCheckResultsToUi` state-only**

Replace the inline device calls with `StartupDeviceService.Run()` inside the existing ThreadPool callback. Remove both final reconnect calls and the startup `MessageBox.Show`; leave only status text/color, flags, logging, and bottom-status refresh in the UI callback.

- [x] **Step 4: Suppress startup-only serial dialogs**

Add an optional `suppressDialogs` argument to `CheckCom_3561` and `ConnectFor3561`. The startup service passes `True`; ordinary runtime callers retain the existing default behavior. Startup failures are returned/logged instead of displayed modally.

- [x] **Step 5: Run the regression**

Run the same PowerShell script. Expected: the device callback assertions pass; later startup-layout assertions may remain red until Tasks 2–4 are complete.

### Task 2: First-paint startup sequencing

**Files:**
- Modify: `frmTest.vb:71-164,2356-2411`
- Modify: `MDIForm.vb:358-397`
- Create or modify: `StartupTiming.vb`
- Test: `tools/Test-StartupResizePerformance.ps1`

- [x] **Step 1: Add `StartupTiming` and first-paint timing hooks**

Use one monotonic `Stopwatch` per process and a thread-safe log method. Record `MDI.Shown`, `frmTest.Load begin/end`, essential layout, deferred visual style, first `Paint`, device check begin/end, and device result callback.

- [x] **Step 2: Split `frmTest` startup**

Keep control creation, essential responsive container setup, initial values, channel-grid data initialization, and `_isLoaded` in `ApplyFrmTestEssentialLayout`. Move non-critical theme/style calls to `ApplyFrmTestDeferredVisualStyle`.

- [x] **Step 3: Defer timers and non-critical work until after first paint**

Do not call `timer3.Start()` or `Timer4.Start()` from `frmTest_Load`. Schedule `StartDeferredStartupWork` with `BeginInvoke` after the first `Paint`, guard it with a one-time flag, then apply deferred style and start timers/grade-field loading/other polling.

- [x] **Step 4: Run static regression and compile**

Run `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-StartupResizePerformance.ps1`, then build Release x86 with the existing project command. Expected: all startup hook assertions pass and the build exits 0.

### Task 3: Runtime verification

**Files:**
- Modify only if verification exposes a defect.
- Test: `tools/Test-StartupResizePerformance.ps1`, `tools/Test-MdiInitialChildSizing.ps1`, `tools/Test-MdiMaximizedResizeSync.ps1`

- [x] **Step 1: Run all static and geometry checks**

Run each script separately and record exit code/output.

- [x] **Step 2: Inspect fresh startup log**

Confirm the timing log has separate entries for first paint and device-check completion, and that device-check completion occurs independently of the first-paint timestamp.

- [x] **Step 3: Review the final diff**

Run `git diff --check` and inspect only the files touched by this plan; preserve all unrelated user changes.
