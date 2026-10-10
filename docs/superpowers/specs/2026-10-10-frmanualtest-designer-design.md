# FrmManualTest Designer Support

Date: 2026-10-10

## Goal

Make `FrmManualTest` editable in the Visual Studio Windows Forms Designer while preserving its current appearance and behavior.

## Design

- Split `FrmManualTest` into the existing code-behind and a new `FrmManualTest.Designer.vb` partial class following the project's VB WinForms conventions.
- Move fixed form settings, the four-row root layout, command controls, result grid, and bottom controls into `InitializeComponent`.
- Keep the 16-by-16 channel buttons runtime-generated because their labels, tray mapping, tooltips, and selection behavior are data-driven. The designer owns the matrix container and its layout; runtime code fills it with the 256 channel controls.
- Keep test execution, selection, clamp, export, and logging behavior in the code-behind. Preserve existing event handlers and control names where possible.
- Add the designer file to the legacy `.vbproj` as a dependent item of `FrmManualTest.vb`. A `.resx` is unnecessary because this form has no designer resources.

## Acceptance Criteria

1. Visual Studio recognizes `FrmManualTest` as a Windows Form and offers the visual designer.
2. The designer source owns the fixed layout and static controls; runtime code only populates the channel matrix and initializes state.
3. The form retains its existing dimensions, four-row proportions, captions, control actions, channel mapping, and result columns.
4. The project compiles with the new partial class and project-file entries.

## Scope

No visual restyling or manual-test behavior changes are included. The 256 channel buttons remain generated at runtime.
