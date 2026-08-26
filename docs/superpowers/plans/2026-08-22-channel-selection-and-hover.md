# 256 通道选择与悬停信息优化 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 在现有 16×16 通道矩阵中增加单选、框选、全选、清除和待测试数量显示，并增强悬停信息可读性，同时将选择结果接入正式测试通道计划。

**Architecture:** `ChannelGridControl` 维护选中通道集合和选择模式，`ChannelTileControl` 只负责单通道的选中视觉状态与鼠标事件；`frmTest` 负责左侧按钮、数量显示和测试计划同步。保留现有状态颜色，在其上叠加选中边框。

**Tech Stack:** VB.NET WinForms、TableLayoutPanel、现有 `ChannelDisplayData`、`SelectedChannelNumbers` 和 `FormalTestRunner`。

---

### Task 1: Selection state model and tile visuals

**Files:**
- Modify: `ChannelTileControl.vb`
- Modify: `ChannelGridControl.vb`

- [ ] Add selected state to each tile and expose a grid-level selected channel set.
- [ ] Preserve PASS/NG/TESTING/IDLE background colors while drawing a high-contrast selected border.
- [ ] Add single-selection toggle, select-all, clear-selection, and selected-count change event.

### Task 2: Rectangle selection

**Files:**
- Modify: `ChannelGridControl.vb`

- [ ] Add explicit Single/Rectangle selection modes.
- [ ] Track mouse-down, mouse-move and mouse-up coordinates over the grid.
- [ ] Compute intersection with the 16×16 tile bounds and replace the selection on rectangle completion.
- [ ] Render the selection rectangle without interfering with tile status colors.

### Task 3: Left-side controls and test-plan integration

**Files:**
- Modify: `frmTest.Designer.vb`
- Modify: `frmTest.vb`

- [ ] Add 全选、清除、框选、单选 controls in the tray/battery section.
- [ ] Rename the existing battery-count caption to 待测试通道数 and update the value live.
- [ ] Synchronize selected channel numbers with `SelectedChannelNumbers` and reject test start when the set is empty.
- [ ] Disable selection controls while testing and restore them afterward.

### Task 4: Hover details and contrast

**Files:**
- Modify: `ChannelGridControl.vb`
- Modify: `UiTheme.vb` only if the existing semantic colors cannot provide sufficient contrast.

- [ ] Display the field label 电池条码 explicitly.
- [ ] Use a dark, high-contrast foreground and preserve ellipsis for long barcode text.

### Task 5: Verification

**Files:**
- Add or extend a lightweight test/verification harness matching the repository's available VB.NET build tooling.

- [ ] Verify all selection operations and counts: 0, 1, partial, and 256.
- [ ] Verify rectangle boundaries and channel-number mapping.
- [ ] Build the solution and inspect compiler output.
- [ ] Run existing UI/build verification scripts where available and manually inspect the generated UI artifact.

