# Darker Left Sidebar Buttons Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the selected C deep-slate palette to every left-sidebar action button and close the bypass in `frmTest.vb` that restores white backgrounds.

**Architecture:** Keep palette tokens in `UiTheme.vb`. Use one `InitializeLeftPanelButtonStyles` entry point in `frmTest_Load` for static semantic roles, and keep `SetSelectionModeButton` as the single state-refresh path for channel selection buttons.

**Tech Stack:** VB.NET 4.8, Windows Forms, `System.Drawing`, MSBuild.

---

### Task 1: Update the deep-slate theme tokens

**Files:** Modify `UiTheme.vb:22-25`.

- [ ] Add `#C1C8D2`, `#5D6B7A`, `#B4BCC7`, and `#A6B0BC` as the ordinary-button background, border, hover, and pressed colors.
- [ ] Keep `DangerBackground`, `Primary`, `Warning`, and `ApplyTextBoxStyle` behavior unchanged except for the already-approved light-red danger surface.
- [ ] Add or preserve button-style toggle hover/pressed assignments so unselected toggles use deep slate and selected toggles remain blue.

### Task 2: Apply semantic styles to every left-sidebar button

**Files:** Modify `frmTest.vb` near `frmTest_Load`, `InitializeChannelSelectionControls`, and `SetSelectionModeButton`.

- [ ] Add `InitializeLeftPanelButtonStyles()` and call it immediately after `InitializeChannelSelectionControls()` in `frmTest_Load`.
- [ ] Apply these roles without changing handlers:

```vb
UiTheme.ApplyButtonStyle(btnGetBarcode, UiTheme.ButtonRole.Primary)
UiTheme.ApplyButtonStyle(CmdStart, UiTheme.ButtonRole.Primary)
UiTheme.ApplyButtonStyle(btnStop, UiTheme.ButtonRole.Danger)
UiTheme.ApplyButtonStyle(btnNGTest, UiTheme.ButtonRole.Warning)
UiTheme.ApplyButtonStyle(btnTrayState, UiTheme.ButtonRole.Neutral)
UiTheme.ApplyButtonStyle(btnOpen, UiTheme.ButtonRole.Neutral)
UiTheme.ApplyButtonStyle(btnClose, UiTheme.ButtonRole.Neutral)
UiTheme.ApplyButtonStyle(Button2, UiTheme.ButtonRole.Danger)
```

- [ ] Change `SetSelectionModeButton` so its active button receives `Primary` and every inactive button receives `Neutral` through `UiTheme.ApplyButtonStyle`, instead of assigning `CardBackground` and `CardBorder` directly.
- [ ] Do not change the selection event handlers or channel-grid calls.

### Task 3: Verify the visual regression and build

**Files:** Verify `UiTheme.vb`, `frmTest.vb`, and `XL-OCV-UIControls.vbproj`.

- [ ] Run a static assertion that the deep-slate tokens exist, `SetSelectionModeButton` no longer assigns `UiTheme.CardBackground`, and all eight left-panel controls are styled.
- [ ] Build `XL-OCV-UIControls.vbproj` with Visual Studio 2022 MSBuild in Debug/AnyCPU.
- [ ] Run a WinForms style smoke harness against `UiTheme` and assert neutral RGB is `193,200,210`, danger RGB is `255,224,222`, selected RGB is `23,105,232`, and TextBox remains white.
- [ ] Run `git diff --check` and inspect that no business logic or Designer input-field changes are present.
- [ ] Commit only `UiTheme.vb` and `frmTest.vb` plus the two design documents.
