# Full-Channel Timeout and Fast Speed Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Increase the formal-test wait limit to 300 seconds and select the fastest middle-machine channel-switching mode.

**Architecture:** Keep timeout ownership in `FormalTestRunner` and keep speed-register ownership in `SerialMiddleMachineClient`. Only defaults and the existing configuration argument change.

**Tech Stack:** VB.NET, Visual Studio solution, PowerShell verification script.

---

### Task 1: Add a regression check

**Files:**
- Create: `tools/Test-FullChannelTimeoutFastSpeed.ps1`

- [ ] Assert `FormalTestRunner.vb` contains `Optional timeoutMs As Integer = 300000`.
- [ ] Assert all middle-machine plan configuration calls pass `False` for `stableSpeed`.
- [ ] Run the script and observe failure before production changes.

### Task 2: Apply the two configuration changes

**Files:**
- Modify: `FormalTestRunner.vb`
- Modify: `SerialMiddleMachineClient.vb`

- [ ] Change the three formal-test timeout defaults to `300000`.
- [ ] Change `ConfigurePlan` calls used by `RunPlan`, `RunSingleRange`, `RunChannelList`, and `RunTwoRanges` to pass `False`.

### Task 3: Verify

- [ ] Run `tools/Test-FullChannelTimeoutFastSpeed.ps1` and confirm it passes.
- [ ] Build `Battery Testing.sln` and confirm exit code 0.
- [ ] Inspect the diff and confirm no unrelated files changed.
