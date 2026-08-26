# Channel Inline Hover Details Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move channel hover details into the channel-grid title row and keep them visible within the grid bounds.

**Architecture:** Channel tiles publish hover data; the parent `ChannelGridControl` owns one title-row details label and renders the text. The existing click selection event remains unchanged.

**Tech Stack:** VB.NET WinForms, MSBuild, PowerShell static regression tests.

---

### Task 1: Add a failing regression test

**Files:**
- Create: `tools/Test-ChannelInlineHoverDetails.ps1`

- [x] Assert that `ChannelGridControl.vb` contains a title details label and handles tile hover data.
- [x] Assert that `ChannelTileControl.vb` no longer calls `ToolTip.Show`.
- [x] Run the script and confirm it fails before implementation.

### Task 2: Move hover rendering to the grid title row

**Files:**
- Modify: `ChannelTileControl.vb`
- Modify: `ChannelGridControl.vb`

- [x] Add a tile hover event carrying `ChannelDisplayData` and clear it on mouse leave.
- [x] Replace per-tile `ToolTip` creation and screen-position calculation with the hover event.
- [x] Add a right-side details label to the title row and format the channel details there.
- [x] Clear the title details when no tile is hovered.

### Task 3: Verify behavior and build

**Files:**
- Test: `tools/Test-ChannelInlineHoverDetails.ps1`

- [x] Run the regression script and existing tooltip lifecycle/position tests.
- [x] Rebuild `Battery Testing.sln` with `Debug|x86`.
- [x] Rebuild `Battery Testing.sln` with `Debug|Any CPU` and verify it maps to x86.
