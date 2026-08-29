# Startup and Resize Performance Implementation Plan

**Goal:** Reduce initial main-page display time and prevent slow redraws while resizing the MDI window.

**Architecture:** Keep the existing WinForms MDI layout, but let Dock/Fill own child sizing. The MDI form will debounce client-area layout with a 75 ms timer and will no longer toggle maximized children. Startup will batch layout work, defer database field loading, and skip the hidden legacy bitmap. Channel-grid updates will suspend layout and invalidate once per batch.

**Tech Stack:** VB.NET, .NET Framework 4.8, WinForms, PowerShell verification scripts, MSBuild.

---

### Task 1: Add failing regression checks

- [x] Add `tools/Test-StartupResizePerformance.ps1`.
- [x] Run it and confirm it fails because the requested hooks are not implemented.

### Task 2: Optimize MDI resize lifecycle

- [ ] Remove Normal-to-Maximized synchronization.
- [ ] Remove forced `FitToMdiClient` sizing.
- [ ] Add a single 75 ms resize debounce timer.

### Task 3: Optimize startup layout and initialization

- [ ] Make responsive startup layout use one explicit layout pass.
- [ ] Load grade fields asynchronously with a 3-second SQL command timeout.
- [ ] Skip legacy `FreshBat` when `Pic_Bat` is hidden.

### Task 4: Batch channel-grid updates

- [ ] Add `BeginBatchUpdate`/`EndBatchUpdate`.
- [ ] Make `UpdateAll` and selection refresh use one final invalidate.

### Task 5: Verify

- [ ] Run regression scripts and `git diff --check`.
- [ ] Build Release x86 with an isolated output path if the normal executable is locked.
