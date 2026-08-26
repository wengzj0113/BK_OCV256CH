# Responsive Test UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace fixed right-side form positioning with a responsive card-and-grid layout that fills the MDI client area and supports narrow screens.

**Architecture:** A focused `ResponsiveTestLayout` helper will own only the parent/child layout containers. It will reparent the existing left panel, statistics control, and channel grid without changing their business events. Percent-based `TableLayoutPanel` rows and columns will allocate space; the left panel remains independently scrollable and the right matrix fills the remaining area.

**Tech Stack:** VB.NET WinForms, TableLayoutPanel, AutoScroll, PowerShell runtime probes, MSBuild.

---

### Task 1: Add a failing runtime layout probe

**Files:**
- Create: `tools/Test-ResponsiveTestLayout.ps1`

- [ ] **Step 1: Write the failing probe**

  Load the x86 application assembly in 32-bit PowerShell, instantiate `frmTest`, call its layout method, and assert that the responsive root has two columns, the right column is wider than zero, `Panel1.AutoScroll` is enabled, and the matrix's final tile is inside the right viewport.

- [ ] **Step 2: Run the probe**

  Run `& "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -ExecutionPolicy Bypass -File tools\Test-ResponsiveTestLayout.ps1`.
  Expected: FAIL because the current form has no responsive root layout.

### Task 2: Implement the responsive layout helper

**Files:**
- Create: `ResponsiveTestLayout.vb`
- Modify: `frmTest.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: Add `ResponsiveTestLayout`**

  Create a helper with `Apply(host, leftPanel, statistics, channelGrid, legacyPicture)` that creates a fill-docked root `TableLayoutPanel`, assigns percent columns, creates a fill-docked right layout with percent rows, reparents existing controls, hides the legacy picture, and preserves the left panel's AutoScroll.

- [ ] **Step 2: Call the helper once from `frmTest`**

  Store a private helper instance and call `Apply` from `ApplyFrmTestVisualStyle` before styling; avoid changing event handlers or data update methods.

- [ ] **Step 3: Run the probe**

  Expected: PASS with the root, left scroll region, right fill region, and final matrix tile all within bounds.

### Task 3: Remove conflicting fixed dimensions from child controls

**Files:**
- Modify: `StatisticsControl.vb`
- Modify: `ChannelGridControl.vb`
- Modify: `ChannelTileControl.vb`

- [ ] **Step 1: Make card controls fill their parent cells**

  Remove fixed control height assumptions, set `Dock = Fill`, use proportional internal rows, and retain no fixed tile minimum size or wrapping label preferred height.

- [ ] **Step 2: Run static and runtime layout probes**

  Run the responsive probe plus the existing MDI and matrix probes. Expected: all PASS.

### Task 4: Build and verify

**Files:**
- Inspect: `frmTest.vb`, `ResponsiveTestLayout.vb`, `StatisticsControl.vb`, `ChannelGridControl.vb`, `ChannelTileControl.vb`

- [ ] **Step 1: Build to an isolated output directory**

  Run MSBuild with `OutputPath=responsive-layout-build\\` and a matching isolated intermediate directory. Expected: 0 warnings and 0 errors.

- [ ] **Step 2: Verify target properties and runtime bounds**

  Confirm the right layout consumes the root's remaining width, the left panel scrolls on narrow widths, and the 16×16 grid has no clipped final tile.
