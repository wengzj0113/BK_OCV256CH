# 3562 Range Preparation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Force the resistance instrument into manual 300 mΩ range before every formal test while preserving signed and zero results.

**Architecture:** Keep the existing `PrepareCachedData_3562` boundary as the single pre-test instrument setup point. Add a source-level regression check for command order and reuse the existing parser/runtime tests for result preservation and integration safety.

**Tech Stack:** VB.NET 4.8 WinForms, PowerShell regression scripts, MSBuild x86.

---

### Task 1: Record and commit the design

**Files:**
- Create: `docs/superpowers/specs/2026-09-05-3562-range-preparation-design.md`

- [x] Write the design with the range command order, preserved signed values, and cache-clearing constraint.
- [x] Commit only the design document with `git add -- docs/superpowers/specs/2026-09-05-3562-range-preparation-design.md; git commit -m "docs: define 3562 range preparation"`.

### Task 2: Add the failing command-order regression

**Files:**
- Create: `tools/Test-3562RangePreparation.ps1`

- [ ] Assert that `Module1.vb` contains `:AUT OFF` before `:RES:RANG 300E-3`, and that both appear before `MEMory:CLEA` inside `PrepareCachedData_3562`.
- [ ] Run `& .\tools\Test-3562RangePreparation.ps1` and verify it fails because the current method has no per-test range command.

### Task 3: Implement the minimal pre-test range setup

**Files:**
- Modify: `Module1.vb:674-690`

- [ ] Add `:AUT OFF` and `:RES:RANG 300E-3` before trigger and memory commands.
- [ ] Log `3562 prepare range: 300 mOhm` after the writes.
- [ ] Keep `MEMory:CLEA` after the range command because changing range clears instrument memory.

### Task 4: Verify the regression and integration

**Files:**
- Test: `tools/Test-3562RangePreparation.ps1`
- Test: `tools/Test-3562CachedDataParser.ps1`
- Test: `tools/Test-MainTestPlcLifecycle.ps1`
- Test: `tools/Test-MainTestSelectionSnapshot.ps1`
- Test: `tools/Test-MainTestTrayFailureFeedback.ps1`

- [ ] Run the range-order test and confirm PASS.
- [ ] Run the parser test and confirm 0 and negative values remain numeric results.
- [ ] Rebuild `MP-60A-RV-64Mem.sln` as Debug|x86 with MSBuild.
- [ ] Run x86 runtime selection checks and finish with `git diff --check`.
