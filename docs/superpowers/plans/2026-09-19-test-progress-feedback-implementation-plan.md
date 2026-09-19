# 测试过程状态与进度提示实施计划

> **For agentic workers:** Execute this plan task-by-task with verification after each task.

**Goal:** 在主测试页面持续显示当前测试阶段、通道进度、已用时间和预计剩余时间，让客户能判断程序正在工作、等待设备还是发生异常。

**Architecture:** 新增无 UI 依赖的 `TestProgressState` 状态模型，负责阶段、计数、计时和 ETA 计算；`frmTest` 只负责把现有测试流程事件映射到状态模型，并通过 Designer 中的状态卡片控件展示。`FormalTestRunner` 的采集算法和数据库/PLC 业务顺序保持不变。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.8、PowerShell 静态/运行时契约测试、MSBuild。

---

### Task 1: 建立状态模型回归测试

**Files:**
- Create: `TestProgressState.vb`
- Create: `tools/Test-TestProgressState.ps1`

- [x] **Step 1: Write the failing contract test**

  Assert that the source defines the states, progress fields, ETA calculation, and completed/failed transitions required by the UI.

- [x] **Step 2: Run the contract test and verify it fails**

  Run `pwsh -NoProfile -File .\\tools\\Test-TestProgressState.ps1`.
  Expected: FAIL because `TestProgressState.vb` does not exist yet.

- [x] **Step 3: Implement the pure state model**

  Define `TestProgressStage`, `TestProgressSnapshot`, and `TestProgressState` with deterministic `Begin`, `Advance`, `SetStage`, `Complete`, `Fail`, `Elapsed`, and `EstimatedRemaining` behavior. ETA must return `Nothing` until at least one progress sample exists.

- [x] **Step 4: Run the contract test and verify it passes**

  Run the same PowerShell command and require `PASS`.

### Task 2: Add the visual status card to `frmTest` Designer

**Files:**
- Modify: `frmTest.Designer.vb`

- [x] **Step 1: Add Designer-owned controls**

  Add a `GroupBox` containing stage label, progress label, current-channel label, elapsed label, ETA label, and a progress bar. Place it above the existing event log in the left panel so the information is visible without scrolling.

- [x] **Step 2: Configure visual states**

  Use blue for active testing, orange for saving/waiting, green for completed, red for failed, and gray for idle. Keep text labels so color is never the only status signal.

- [x] **Step 3: Run the WinForms designer serialization check**

  Run `pwsh -NoProfile -File .\\tools\\Assert-WinFormsDesignerSerialization.ps1` and require no new designer errors.

### Task 3: Connect `frmTest` workflow events

**Files:**
- Modify: `frmTest.vb`

- [x] **Step 1: Add the progress timer and UI update helper**

  Use the existing WinForms timer infrastructure to refresh elapsed time and ETA once per second on the UI thread. All background or hardware callbacks must marshal through the form before changing controls.

- [x] **Step 2: Map the existing workflow boundaries**

  Publish states for task creation, instrument preparation, channel acquisition, result collection, result judgment, OCV_DATA save, local save, MES enqueue, and completion. Update the completed channel count after each mapped result is available.

- [x] **Step 3: Handle failure and cleanup centrally**

  Display the failing stage and actionable exception text, stop the timer, restore idle controls, and keep the final failure visible in the event log.

- [x] **Step 4: Add the source contract test**

  Require that every major `StarTest` branch updates progress and that the cleanup path completes or fails the state before clearing the current channel list.

### Task 4: Build and verify end-to-end behavior

**Files:**
- Modify: `tools/Test-TestProgressState.ps1`

- [x] **Step 1: Run state-model and source contracts**

  Run `pwsh -NoProfile -File .\\tools\\Test-TestProgressState.ps1` and the existing `tools/Test-FrmTestRuntimeState.ps1`.

- [x] **Step 2: Rebuild the application**

  Run MSBuild `MP-60A-RV-64Mem.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform=x86` and require exit code 0.

- [x] **Step 3: Run the existing UI and authorization checks**

  Run the relevant channel, startup, authorization, and designer serialization scripts; require all to pass.

- [x] **Step 4: Review the diff and commit**

  Run `git diff --check`, inspect the changed files, and commit with message `feat: show test progress and remaining time`.
