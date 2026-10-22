# 自动模式全选通道 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Every time the main test page enters automatic mode, select all 256 channels immediately.

**Architecture:** Reuse `ChannelGridControl.SelectAll()` from `frmTest` after automatic/manual checkbox state is normalized. Also apply it from `SetTestMode(True)` so programmatic restoration to automatic mode follows the same rule.

**Tech Stack:** VB.NET WinForms, existing `ChannelGridControl` selection event.

---

## File map

- Modify `frmTest.vb`: the user-driven mode toggle and programmatic `SetTestMode` helper are the only mode entry paths. `ChannelGridControl.SelectAll()` already owns channel state, repaint, and selection-change notification.

## Task 1: Select all channels whenever automatic mode is applied

**Files:**
- Modify: `frmTest.vb` (`TestModeCheckedChanged` and `SetTestMode`)

- [x] After `TestModeCheckedChanged` finishes enforcing mutual exclusivity, call `channelGrid.SelectAll()` when `chkAutoTest.Checked` is true. This ensures the user-driven switch happens before the PLC coordinator is restarted.
- [x] In `SetTestMode`, call `channelGrid.SelectAll()` when `autoMode` is true. This covers automatic-mode restoration after authorization or busy-state rejection.
- [x] Leave the manual-mode path unchanged so manual partial selection remains available.
- [x] Review `git diff -- frmTest.vb` and run `git diff --check`. Do not add or run tests unless requested.

The existing form-load call to `channelGrid.SelectAll()` remains as the startup behavior. No selection logic is added to the automatic tray-plan builder or PLC coordinator.
