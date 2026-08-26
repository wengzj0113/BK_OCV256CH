# 34461 Cached Read Optimization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Keysight 34461 external-trigger workflow explicitly collect one sample per trigger and read the completed reading memory once after the middle-machine test.

**Architecture:** Keep the existing VISA session and `FormalTestRunner` boundary. `PrepareCachedData_34461` only arms the instrument; `Trans_ReadCachedData_34461` performs one post-test `FETC?` read. Remove polling and unrelated VM-complete output configuration, while preserving trigger slope as a hardware-dependent setting.

**Tech Stack:** VB.NET WinForms, IVI VISA COM, PowerShell repository verification scripts, MSBuild.

---

### Task 1: Lock down the 34461 command contract

**Files:**
- Create: `tools/Test-34461CommandSequence.ps1`
- Modify: `Module1.vb:1599-1704`

- [ ] Write a static regression test asserting `SAMP:COUN 1`, external trigger setup, `INIT`, one `FETC?`, and absence of `DATA:POINTS?` polling and `OUTP:TRIG:SLOP`.
- [ ] Run the test and confirm it fails against the current implementation because the required command contract is not yet present.
- [ ] Change the preparation sequence to explicitly send `SAMP:COUN 1`, retain `TRIG:COUN N`, retain `TRIG:SOUR EXT` and `TRIG:SLOP POS`, and remove `OUTP:TRIG:SLOP POS`.
- [ ] Replace the point-count polling with a single `FETC?`/read operation after test completion; retain numeric parsing and count validation.
- [ ] Run the regression test and confirm it passes.

### Task 2: Preserve diagnostics and verify the build

**Files:**
- Modify: `Module1.vb:1637-1678`

- [ ] Log the expected count, raw response length, parsed count, and VISA exception as before, without changing unrelated test flow.
- [ ] Run the PowerShell regression test and compile `MP-60A-RV-64Mem.vbproj` with the repository's available MSBuild.
- [ ] Report that real instrument validation still requires one hardware run with the 34461 and middle machine connected.
