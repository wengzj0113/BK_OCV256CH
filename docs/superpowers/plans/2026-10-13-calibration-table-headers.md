# Calibration Table Headers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Show clear Chinese labels for all eight columns in the channel calibration table.

**Architecture:** Set `HeaderText` on the existing WinForms designer columns in their current display order. Preserve column names, ordering, bindings, calculations, and persistence behavior.

**Tech Stack:** VB.NET WinForms, .NET Framework.

---

## File structure

- Modify `FrmChannelCalibration.Designer.vb`: assign the eight agreed Chinese header strings to the existing checkbox and textbox columns.
- No production logic, file format, data model, or automated test files need to change.

### Task 1: Set Chinese calibration table headers

**Files:**
- Modify: `FrmChannelCalibration.Designer.vb`

- [x] **Step 1: Add the exact header labels to the existing columns**

```vb
Me.DataGridViewCheckBoxColumn1.HeaderText = "选择"
Me.DataGridViewTextBoxColumn1.HeaderText = "通道号"
Me.DataGridViewTextBoxColumn2.HeaderText = "当前补偿值（mΩ）"
Me.DataGridViewTextBoxColumn3.HeaderText = "实测电阻（mΩ）"
Me.DataGridViewTextBoxColumn4.HeaderText = "标准电阻（mΩ）"
Me.DataGridViewTextBoxColumn5.HeaderText = "本次补偿值（mΩ）"
Me.DataGridViewTextBoxColumn6.HeaderText = "校准时间"
Me.DataGridViewTextBoxColumn7.HeaderText = "校准状态"
```

Place each assignment beside that column's existing initialization, leaving every `.Name` and the `Columns.AddRange` order unchanged.

- [x] **Step 2: Check the focused diff and whitespace**

Run: `git diff --check -- FrmChannelCalibration.Designer.vb`

Expected: no output and exit code 0. Inspect the designer column section to confirm each agreed string occurs once and is assigned to the correct existing column.

No automated tests are needed for these static display strings; do not alter or run unrelated tests.
