# Left Sidebar Gray Buttons Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the approved C visual scheme so ordinary left-sidebar buttons and unselected toggle buttons use a visible warm-gray surface without changing OCV behavior or layout.

**Architecture:** Keep all visual tokens and state transitions centralized in `UiTheme.vb`. Add warm-gray neutral tokens, apply them to neutral `Button` controls and unselected button-style `RadioButton`/`CheckBox` controls, and replace the existing pure-white danger surface with the existing light-red semantic background while preserving primary, warning, success, and selected states.

**Tech Stack:** VB.NET 4.8, Windows Forms, `System.Drawing`, MSBuild, `XL-OCV-UIControls.vbproj`.

---

### Task 1: Establish the baseline and affected style paths

**Files:** Read `UiTheme.vb:19-174`, `XL-OCV-UIControls.vbproj`; preserve the existing working-tree changes.

- [ ] **Step 1: Confirm style call paths**

Run:

```powershell
rg -n "ApplyButtonStyle|ApplyToggleStateStyle|ButtonRole.Neutral|ApplyRadioButtonStyle|ApplyCheckBoxStyle" -g '*.vb' -g '!build-*'
```

Expected: neutral buttons flow through `ApplyButtonStyle`, and button-style toggles flow through `ApplyToggleStateStyle`.

- [ ] **Step 2: Capture baseline state**

Run `git status --short` and `dotnet --info`. Do not reset, clean, checkout, or stage the pre-existing `MDIForm.vb` and build-verify changes.

### Task 2: Implement the approved C visual tokens and state styling

**Files:** Modify only `UiTheme.vb:19-33,111-129,148-174`.

- [ ] **Step 1: Add centralized warm-gray tokens**

Add these fields next to the existing semantic color fields:

```vb
Public Shared ReadOnly NeutralButtonBackground As Color = ColorTranslator.FromHtml("#E7E7E5")
Public Shared ReadOnly NeutralButtonBorder As Color = ColorTranslator.FromHtml("#A7ADB5")
Public Shared ReadOnly NeutralButtonHoverBackground As Color = ColorTranslator.FromHtml("#DEDFDD")
Public Shared ReadOnly NeutralButtonPressedBackground As Color = ColorTranslator.FromHtml("#D4D6D3")
```

- [ ] **Step 2: Update unselected toggle state**

In the `Else` branch of `ApplyToggleStateStyle`, set `control.BackColor` to `NeutralButtonBackground`, keep `control.ForeColor = TextPrimary`, set each `CheckBox`/`RadioButton` border to `NeutralButtonBorder`, and set its `FlatAppearance.MouseOverBackColor` and `MouseDownBackColor` to the hover and pressed tokens. Keep the selected branch blue with white text and the existing primary border.

- [ ] **Step 3: Update neutral and danger Button states**

Replace only the `Case Else` branch in `ApplyButtonStyle` with:

```vb
Case Else
    button.BackColor = NeutralButtonBackground
    button.ForeColor = TextPrimary
    button.FlatAppearance.BorderColor = NeutralButtonBorder
    button.FlatAppearance.MouseOverBackColor = NeutralButtonHoverBackground
    button.FlatAppearance.MouseDownBackColor = NeutralButtonPressedBackground
```

Do not change the `Primary` or `Warning` branches.

In the `Danger` branch, keep `Danger` text and border, but change only the background from `Color.White` to `DangerBackground` so no styled button retains a pure-white surface:

```vb
Case ButtonRole.Danger
    button.BackColor = DangerBackground
    button.ForeColor = Danger
    button.FlatAppearance.BorderColor = Danger
```

- [ ] **Step 4: Preserve input distinction**

Leave `ApplyTextBoxStyle` unchanged so text-entry fields remain `CardBackground` white. Do not touch event handlers, layout coordinates, test logic, device logic, database logic, or MES logic.

### Task 3: Verify, review, and commit the implementation

**Files:** Verify `UiTheme.vb`, `XL-OCV-UIControls.vbproj`, and the approved design at `docs/superpowers/specs/2026-09-19-left-sidebar-gray-buttons-design.md`.

- [ ] **Step 1: Run static style assertions**

Run:

```powershell
$theme = Get-Content -Raw 'UiTheme.vb'
if ($theme -notmatch 'NeutralButtonBackground.*#E7E7E5') { throw 'Missing warm-gray background token' }
if ($theme -notmatch 'NeutralButtonBorder.*#A7ADB5') { throw 'Missing warm-gray border token' }
if ($theme -notmatch 'button\.BackColor = NeutralButtonBackground') { throw 'Neutral Button is not warm gray' }
if ($theme -notmatch 'control\.BackColor = NeutralButtonBackground') { throw 'Unselected toggle is not warm gray' }
if ($theme -notmatch 'button\.BackColor = DangerBackground') { throw 'Danger Button is still pure white' }
if ($theme -match 'button\.BackColor = Color\.White') { throw 'A styled Button still uses pure white background' }
if ($theme -notmatch 'textBox\.BackColor = CardBackground') { throw 'TextBox background changed unexpectedly' }
Write-Output 'UiTheme style assertions passed.'
```

Expected: `UiTheme style assertions passed.`

- [ ] **Step 2: Build the UI controls project**

Run `msbuild '.\XL-OCV-UIControls.vbproj' /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /m`. Expected: exit code 0. If `msbuild` is not on PATH, locate the installed executable with `Get-Command msbuild` and rerun without changing project files.

- [ ] **Step 3: Inspect scope and formatting**

Run `git diff --check -- UiTheme.vb` and `git diff -- UiTheme.vb`. Expected: only centralized color/state styling changes are present; no business or layout changes.

- [ ] **Step 4: Commit only the implementation file**

Run:

```powershell
git add -- UiTheme.vb
git commit -m "fix: improve neutral button contrast"
```

Expected: the commit contains only `UiTheme.vb`; unrelated pre-existing changes remain uncommitted.

### Task 4: Perform visual acceptance

**Files:** Inspect the runnable application or existing build output and compare against the approved design.

- [ ] **Step 1: Check ordinary controls**

Confirm ordinary controls such as `手动`, `开启托盘`, `关闭托盘`, `全选`, `清除`, `框选`, `单选`, and `报警复位` use warm-gray backgrounds with visible borders where they use the centralized neutral style.

- [ ] **Step 2: Check semantic controls and input fields**

Confirm `启动测试` remains blue, `停止测试` red, `NG点测试` orange, `运行中` green, selected options blue, and text boxes white. Confirm neutral controls visibly darken on hover/press and remain bounded when disabled.

- [ ] **Step 3: Record final evidence**

Run `git status --short` and `git log -2 --oneline`. Expected: the implementation commit is present, and only the pre-existing `MDIForm.vb`/build-verify changes remain in the working tree.
