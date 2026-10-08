# 仪表速率档位自动匹配实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 根据初始化页选定的速率档位，使控制板、HIOKI 3562 与 Keysight 34461A 使用匹配的采样/触发参数。

**Architecture:** 使用纯数据 `InstrumentRateProfile` 集中描述四档映射；应用启动和每轮正式测量都从同一配置设置两台仪表，正式测量将同一控制板速度值传入 RunPlan。保留用户已保存的档位，首次启动默认超快档。

**Tech Stack:** VB.NET、.NET Framework 4.8、PowerShell 契约脚本、MSBuild。

---

### Task 1: 建立四档 profile 映射

**Files:**
- Create: `InstrumentRateProfile.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools/Test-InstrumentRateProfiles.ps1`

- [x] 写四档映射与未知档处理的失败测试，并先运行确认失败。
- [x] 加入 profile 数据类型和项目编译项。
- [x] 运行映射测试确认通过。

### Task 2: 3562 初始化与每轮重设

**Files:**
- Modify: `Module1.vb`
- Test: `tools/Test-3562RangePreparation.ps1`

- [x] 扩展契约测试，要求初始化及每轮准备都发送选中 profile 的采样率。
- [x] 移除一次性初始化门闩；InitRes 与每轮准备共享 profile。
- [x] 运行 3562 相关契约测试。

### Task 3: 34461A 每轮配置

**Files:**
- Modify: `Module1.vb`
- Test: `tools/Test-34461CommandSequence.ps1`

- [x] 扩展测试覆盖 profile NPLC、Autozero、量程锁定、外部触发沿和零延迟。
- [x] 根据电压判定上下限选量程，并在每次复位后应用该档位配置。
- [x] 运行 34461A 命令序列测试。

### Task 4: UI 保存值、控制板和仪表统一

**Files:**
- Modify: `frmInit.vb`
- Modify: `SerialMiddleMachineClient.vb`
- Modify: `FormalTestRunner.vb`
- Test: `tools/Test-InstrumentRateProfiles.ps1`

- [x] 增加对保存速率档位的恢复和首次启动默认超快档测试。
- [x] RunPlan 接受 profile 的速度值，正式测试传递该值。
- [x] 运行档位联动测试。

### Task 5: 回归与构建

**Files:**
- Verify: `tools/Test-3562CachedDataParser.ps1`
- Verify: `tools/Test-3562RangePreparation.ps1`
- Verify: `tools/Test-34461CommandSequence.ps1`
- Verify: `MP-60A-RV-64Mem.vbproj`

- [x] 运行相关脚本和现有时序配置验证脚本。
- [x] 构建 .NET Framework 4.8 项目。
- [x] 核对最终差异，保留当前工作区中与本方案无关的既有用户改动。
