# frmTest Designer Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move frmTest's visible UI construction and layout into frmTest.Designer.vb so startup code only updates data and does not reposition controls.

**Architecture:** StatisticsControl and ChannelGridControl become designer-owned child controls of frmTest. Their internal 256-tile construction remains inside ChannelGridControl and does not change channel mapping. Runtime code retains event wiring and data refresh, but removes dynamic creation and coordinate recalculation.

**Tech Stack:** VB.NET WinForms, Visual Studio designer partial class, MSBuild.

---

### Task 1: Add designer-owned custom controls

**Files:**
- Modify: `frmTest.Designer.vb`

- [ ] Add `statisticsControl` and `channelGrid` field declarations.
- [ ] Instantiate them in `InitializeComponent`.
- [ ] Set their designer-owned `Location`, `Size`, `Name`, and visibility.
- [ ] Add them to `frmTest.Controls` in z-order.
- [ ] Keep `Pic_Bat` hidden as a legacy placeholder.

### Task 2: Remove runtime layout ownership

**Files:**
- Modify: `frmTest.vb`

- [ ] Keep field references compatible with designer fields.
- [ ] Replace Load/Resize layout calls with one-time data/event initialization.
- [ ] Remove `EnsureStatisticsControl`, `EnsureChannelGrid`, and `ApplyFrmTestLayout` coordinate writes.
- [ ] Preserve `ApplyFrmTestVisualStyle` and all existing test/data operations.
- [ ] Preserve channel selection event handling and `UiUpdateService` updates.

### Task 3: Verify

**Files:**
- Test: project build and static layout assertions.

- [ ] Confirm no runtime `New StatisticsControl`, `New ChannelGridControl`, `Controls.Add(statisticsControl)`, or coordinate writes remain.
- [ ] Rebuild Debug configuration.
- [ ] Confirm the built executable is produced without compilation errors.
- [ ] Confirm the 256-channel grid mapping code is unchanged.
