# Channel Grid Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Keep the right-side channel matrix inside the visible MDI client area so the bottom status bar never covers its final rows.

**Architecture:** The MDI parent owns the bottom status control and will reserve its height from the MDI client area during layout changes. The existing `frmTest` anchors and `ChannelGridControl` fill behavior remain unchanged, so the matrix resizes with the remaining client area. A source-level regression check guards the reservation logic because this legacy project has no automated UI test project.

**Tech Stack:** VB.NET WinForms, PowerShell verification script, MSBuild/Visual Studio project build.

---

### Task 1: Add a failing layout regression check

**Files:**
- Create: `tools/Test-MdiStatusLayout.ps1`

- [ ] **Step 1: Write the failing check**

  Assert that `MDIForm.vb` contains a layout method and that the method reduces the MDI client height by the bottom status control height.

- [ ] **Step 2: Run the check and verify it fails**

  Run `powershell -ExecutionPolicy Bypass -File tools/Test-MdiStatusLayout.ps1`.
  Expected: FAIL because the current source has no MDI client reservation method.

### Task 2: Reserve the status bar area in the MDI parent

**Files:**
- Modify: `MDIForm.vb`

- [ ] **Step 1: Add the minimal layout implementation**

  Find the runtime `MdiClient`, keep its top and left edges, set its height to end at `bottomStatusControl.Top`, and invoke the method from `Load` and `Layout`.

- [ ] **Step 2: Run the regression check**

  Run `powershell -ExecutionPolicy Bypass -File tools/Test-MdiStatusLayout.ps1`.
  Expected: PASS.

- [ ] **Step 3: Build the VB.NET project**

  Run `msbuild "MP-60A-RV-64Mem.vbproj" /t:Build /p:Configuration=Debug`.
  Expected: Build succeeds without new errors.

### Task 3: Verify final state

**Files:**
- Inspect: `MDIForm.vb`, `BottomStatusControl.vb`, `frmTest.Designer.vb`, `ChannelGridControl.vb`

- [ ] **Step 1: Verify anchors and control behavior**

  Confirm `channelGrid` remains anchored on all four sides and the status control remains `DockStyle.Bottom` at 40px height.

- [ ] **Step 2: Re-run regression and build checks**

  Run the regression script and the project build again; both must pass before reporting completion.
