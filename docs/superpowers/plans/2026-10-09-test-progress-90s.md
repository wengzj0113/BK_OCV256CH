# 测试进度按 90 秒推进实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Execute this plan inline with review checkpoints.

**Goal:** 测试剩余时间从 90 秒倒数，进度条按同一时间基准推进，成功时归 100%。

**Architecture:** `TestProgressState` 是快照中 ETA 与运行时进度百分比的唯一来源。现有窗体刷新器继续按秒生成快照；UI 与测试流程映射不需要改变。

**Tech Stack:** VB.NET、WinForms、.NET Framework 4.8、PowerShell 运行时检查、MSBuild。

---

## 文件职责

- `tools/Test-TestProgressStateRuntime.ps1`：加载应用程序集，检查重置、开始、时间推进、超时、实际通道计数、终态等状态行为。
- `TestProgressState.vb`：在快照创建时根据开始时间计算运行百分比与剩余时间；保留真实通道计数和终态。
- `docs/superpowers/specs/2026-10-09-test-progress-countdown-design.md`：已审阅的需求来源，本任务不修改。

### Task 1: 用运行时检查锁定 90 秒行为

1. 扩展 PowerShell 检查：测试开始快照 ETA 约 90 秒，进度为 0；通过反射将私有开始时间回拨 45 秒后，快照应显示约 45 秒、进度 50%；原先传给 `SetStage` 和 `Advance` 的手动百分比/通道比例不得改变时间进度；回拨超过 90 秒后为 00:00 对应的零剩余、99%；完成后 100% 且 ETA 为 Nothing；失败/取消不再持续倒计时。
2. 使用当前基线程序集运行扩展后的脚本，确认在首个新增规则处失败，证明检查覆盖了缺失行为。

### Task 2: 实现最小状态模型改动

1. 在 `TestProgressState` 定义 90 秒预计时长常量。
2. 在 `CreateSnapshot` 中根据当前状态、运行开始时间和实际经过时长创建快照；运行中的时间百分比采用整秒向下取整并限制为 0–99，完成状态使用 100，失败/取消使用停止时的时间进度。
3. 计算运行中剩余时间为 `max(0, 90 秒 - elapsed)`；Idle/Completed/Failed/Canceled 的 ETA 为 Nothing。
4. 运行扩展后的状态运行时检查及现有状态/页面契约检查，要求全部通过。

### Task 3: 构建与回归验证

1. 使用 Visual Studio 2022 MSBuild 对 `MP-60A-RV-64Mem.vbproj` 执行 Debug/x86 Build，记录退出码与编译结果。
2. 运行 `Test-TestProgressState.ps1`、`Test-TestProgressFeedback.ps1` 和构建所得程序集上的 `Test-TestProgressStateRuntime.ps1`。
3. 执行 `git diff --check`，检查最终差异仅涉及运行时检查和状态模型。
