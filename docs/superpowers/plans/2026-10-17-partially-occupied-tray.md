# 部分空位托盘跳过测试 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 托盘有空位时只测有电芯码的通道；无可测通道时停止在 R160 之前，并通过状态栏和事件日志提示；部分缺码只记事件日志并继续测试。

**Architecture:** `TrayTestPreparation` 生成有电芯码的实际测量通道。主测试页通过可选的 `IPreparedTrayChannelsProvider` 暴露过滤后的通道列表，协调器把它冻结为自动或手动周期快照；原有 host 接口和没有提供计划列表的适配器保持兼容。以专用异常区分“没有可测通道”，自动和手动边界均更新状态栏并记录事件日志，不弹窗；部分缺码时只记录跳过数和通道号。PLC NG 分档继续从实际测试结果生成。

**Tech Stack:** VB.NET、WinForms、.NET Framework、现有 PLC 周期协调器。

---

## File map

- `TrayTestPreparation.vb`：校验托盘绑定并生成自动/手动实际测量通道；增加“没有可测通道”的专用异常。
- `PlcAutomaticTestCoordinator.vb`：增加可选的已准备通道提供接口，在自动和手动准备成功后读取有效通道并冻结到周期快照；自动无可测通道时更新状态栏并写事件日志。
- `frmTest.vb`：通过已准备通道提供接口暴露自动和手动有效通道，记录跳过/未启动事件；无可测通道时更新状态栏且不弹窗，并使用实际通道数报告自动测试开始。

## Task 1: Build measurement plans from occupied channels

**Files:**
- Modify: `TrayTestPreparation.vb`

- [x] Add `NoTestableTrayChannelsException` in `TrayTestPreparation.vb`, inheriting from `InvalidOperationException`, so callers can distinguish a tray with no runnable channels from malformed tray data.
- [x] Keep the current row validation and code uniqueness checks. Treat absent rows and null/blank `BatteryCode` as empty slots.
- [x] For automatic mode, use the sorted keys in `codes` as `MeasurementChannels` instead of `Enumerable.Range(1, 256)`.
- [x] For manual mode, validate the requested list is nonempty and each channel is in 1–256, then retain only requested channels present in `codes`.
- [x] Throw `NoTestableTrayChannelsException` when `codes.Count = 0`; throw the same exception with the requested channel numbers when a manual selection has no code-bearing channel.
- [x] Set `OccupiedChannels` to the resulting `MeasurementChannels`. Preserve `BatteryCodes` as the complete valid code map returned for the tray.
- [x] Clear old channel readings/results when preparing a new tray so empty slots cannot retain prior PASS/NG values in the overview or statistics.

## Task 2: Freeze filtered channels in manual and automatic cycles

**Files:**
- Modify: `PlcAutomaticTestCoordinator.vb`
- Modify: `frmTest.vb`

- [x] Add `IPreparedTrayChannelsProvider` beside the host interfaces with `GetPreparedTrayMeasurementChannels() As IReadOnlyList(Of Integer)`. Leave `IAutomaticTestHost.PrepareAutomaticTray` and `IManualTestHost.PrepareManualTray` signatures unchanged so existing coordinator adapters remain source-compatible.
- [x] Implement `IPreparedTrayChannelsProvider` in `frmTest`; return `activeTrayPlan.MeasurementChannels` when a plan exists, otherwise return `Nothing`. Keep both existing prepare methods as `Sub` implementations.
- [x] In `PrepareTrayAndConfirm`, call the existing `PrepareAutomaticTray`, obtain the plan channels from `IPreparedTrayChannelsProvider`, and validate the nonempty 1–256 unique list. For hosts without the optional provider, keep the legacy 1–256 snapshot. Assign the read-only snapshot to `cycle.MeasurementChannels` in the same locked commit as the tray code and pending R160 confirmation.
- [x] In `RequestManualCycle`, call the existing `PrepareManualTray`, obtain the plan channels from the provider, and verify they are a nonempty subset of the requested channels. For hosts without the optional provider, keep the legacy requested-channel snapshot. Assign the sorted read-only snapshot only after tray preparation succeeds.
- [x] Keep `StartManualInitialPass` snapshot validation against `activeTrayPlan.MeasurementChannels`; it must receive the same filtered list held by the coordinator.
- [x] Change `CanStartAutomaticTest` to accept a nonempty `activeTrayPlan.MeasurementChannels` list rather than requiring exactly `MAX_CHANNEL` entries. Automatic initial measurement already consumes that plan list.

## Task 3: Report missing-channel outcomes without dialogs

**Files:**
- Modify: `PlcAutomaticTestCoordinator.vb`
- Modify: `frmTest.vb`

- [x] In `PrepareTrayForTest`, calculate requested channels minus `plan.MeasurementChannels`. If any are missing, write one cycle event containing the skipped count and sorted standard channel numbers.
- [x] In `PrepareTrayAndConfirm`, keep the existing recovery state and event entry for preparation failures. When the exception is `NoTestableTrayChannelsException`, record that testing did not start and R160 was not sent, and update the status bar without a dialog. Preserve existing status handling for other preparation errors.
- [x] In the manual `CmdStart_Click` and barcode lookup error handlers, route `NoTestableTrayChannelsException` to `ReportAutomaticEvent` and update the status bar without showing a dialog. Keep current handling for unrelated failures and cancellations.
- [x] For partial missing channels, record skipped count and numbers in the event log only; continue the remaining measurement channels without changing the status bar or showing a dialog.
- [x] Ensure the manual skipped-channel notice remains visible after `StarTest` clears the event panel, and write manual missing-channel notices to a fresh cycle log.
- [x] Prepare the manual plan before asking the repeated-tray confirmation, so the no-usable-channel path can report through status and event log without a dialog.
- [x] When automatic testing begins, format its event/status description with `measurementChannels.Count` instead of the fixed text “256通道”.
- [x] Do not alter the NG result pipeline: `BuildAutomaticPassResult` already filters by `BatteryCodes`, and the coordinator builds the final PLC NG selection from returned test results.

## Scope constraints

- Do not change R160/R161/R152/R162 ordering, PLC DT mapping, or NG grade value.
- Do not change the separate manual measurement window, which has no tray-code lookup.
- Do not add or run tests unless the user asks for test or verification work. During implementation, review changed call sites and the final diff without invoking test scripts.
