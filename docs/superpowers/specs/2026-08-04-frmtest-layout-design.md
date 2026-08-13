# frmTest Layout Design

## Goal

Fill the right side of `frmTest` with the battery matrix when the MDI child is maximized or resized. Keep the left control panel at its existing fixed width.

## Root Cause

`Pic_Bat` is initialized by the Designer with a fixed size of `969 x 890`. `MDIForm` sets the child window to maximized before showing it, so the first `Resize` event occurs while `_isLoaded` is false. The handler exits, and no later layout pass updates `Pic_Bat` after `Load` sets `_isLoaded` to true.

## Design

- Keep `Panel1` docked to the left with its existing width.
- Set `Pic_Bat` to fill the remaining client area through `DockStyle.Fill`.
- Keep the existing resize-time redraw behavior, but route it through one layout helper.
- Invoke the layout helper after `frmTest_Load` finishes initialization and `_isLoaded` becomes true.
- Use `ClientSize` and the actual control bounds; do not use form non-client dimensions or a 95% height multiplier.
- Preserve the existing minimum form size and avoid changing the battery matrix drawing logic.

## Verification

- Inspect the generated Designer/runtime properties to ensure `Panel1` is left-docked and `Pic_Bat` is fill-docked.
- Build with the project-compatible .NET Framework MSBuild when available.
- Confirm that the layout helper is called after load and on subsequent resize events.
