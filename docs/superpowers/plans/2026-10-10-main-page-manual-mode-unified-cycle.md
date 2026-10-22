# 主测试页手动模式统一周期实施计划

> 2026-10-15 协议纠正：本文件记录历史实施过程，其中“R160 ACK 后手动直启”和“手动不读 R151”的任务已被用户明确纠正。当前应执行：手动准备 → 仅发送 R160=1（无 ACK）→ PLC 控制托盘到位 → 有效 R151=1 → 手选通道测试；以同名设计文档的修订内容为准。

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans` to implement this plan task by task. Steps use checkbox syntax for tracking.

**Goal:** 让主测试页手动入口跳过扫码、R150 和 R151 门控，完成托盘准备后由统一 PLC 周期协调器发送 R160=1；ACK 成功即按手选通道开始测试，并与自动模式共用测量、复测、最终保存及 PLC 收尾。

**Architecture:** 在现有 `PlcAutomaticTestCoordinator` 上增加手动周期入口和手动轮询生命周期，保留自动入口原有状态机。手动启动时把托盘码和标准通道集合冻结到周期上下文；窗体只负责预检适配、提交入口请求和显示状态，所有 PLC 握手及共同测试后处理仍由协调器驱动。

**Tech Stack:** VB.NET WinForms、.NET Framework 项目、PowerShell 回归脚本、独立 .NET 8 VB 运行时测试项目。

---

## 文件职责

| 文件 | 职责 |
|---|---|
| `PlcAutomaticTestCoordinator.vb` | 保持自动周期入口，新增不打开扫码器、不读 R150/R151 的手动启动、R152 轮询和共享周期分派 |
| `PlcAutomaticTestState.vb` | 保存周期入口类型与手动标准通道快照，并在周期重置时清理 |
| `frmTest.vb` | 将手动按钮改为协调器入口；实现手动预检/托盘准备宿主；按周期状态锁定启动、通道和模式控件 |
| `tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj` | 链接实际协调器/状态源码，以轻量假 PLC、扫码器和宿主执行不依赖仪器的周期测试 |
| `tools/ManualCycleRuntimeTests/Program.vb` | 覆盖手动准备失败、R160 ACK 后直启、手选通道快照、重复启动、停止和自动 R151 周期回归 |
| `tools/Test-ManualUnifiedCycleContract.ps1` | 核对 WinForms 启动入口、手动 PLC 生命周期、调试准备和通道快照绑定 |
| `tools/Test-PlcV4StateMachine.ps1`、`tools/Test-MainTestSelectionSnapshot.ps1` | 若当前断言仅与源码现有命名/格式不一致，修正断言以验证实际状态机与通道映射行为 |

## Task 1：先建立独立运行时回归

**Files:**
- Create: `tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`
- Create: `tools/ManualCycleRuntimeTests/Program.vb`
- Create: `tools/ManualCycleRuntimeTests/TestContracts.vb`

- [x] 创建 `net8.0` VB 控制台测试项目，链接仓库真实的 `PlcAutomaticTestCoordinator.vb` 和 `PlcAutomaticTestState.vb`；在测试项目内定义最小 `IPlcAutomationClient` 与 `ITrayScannerClient` 合约，不链接 WinForms、仪器或第三方设备 DLL。
- [x] 编写假 PLC/扫码器/主页面宿主，记录 R150/R151/R160/R161、扫码调用、托盘准备、初测通道、最终发布及错误；测试通过反射手动驱动协调器私有 `Poll`，关闭测试实例的定时轮询，避免与假 PLC 并发。
- [x] 原周期测试覆盖手动入口等待 R151 的旧假设；2026-10-10 用户澄清后以新回归替换：手动不读取 R150/R151、不打开/触发扫码器；托盘准备异常时 R160 和初测次数均为 0；准备成功并获 R160 ACK 后立即按冻结通道开始初测；重复启动被拒绝；R160 写失败不测量；停止后保持零重复测量。自动入口继续由有效 R151 启动。
- [x] 运行 `dotnet run --project tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`，确认基线按预期因手动协调器 API/行为缺失而失败：编译器报告 `IManualTestHost` 尚未定义。

## Task 2：在协调器中加入手动入口并复用周期后半段

**Files:**
- Modify: `PlcAutomaticTestCoordinator.vb`
- Modify: `PlcAutomaticTestState.vb`
- Test: `tools/ManualCycleRuntimeTests/Program.vb`

- [x] 定义单独的 `IManualTestHost` 能力接口，提供当前模式判定、手动启动预检、手动托盘准备和带通道快照的初测入口；初测入口还接收周期代次授权函数，以丢弃停止后仍排队的过期 UI 回调；不扩充 `IAutomaticTestHost`，避免破坏现有自动流程宿主及其测试替身。
- [x] 在 `AutomaticTestCycle` 保存 `IsManualCycle` 和只读标准通道快照；`Reset()` 清空入口类型和通道集合。
- [x] 增加 `StartManual()`：打开 PLC 串口并启用既有轮询定时器，不调用 `scanner.Open()`；自动 `Start()` 保持 PLC 与扫码器原有启动行为；`Stop()` 对未开启的扫码器安全关闭。
- [x] 增加 `RequestManualCycle(trayCode, selectedChannels)`：只在手动模式、无活动周期时接受；校验托盘码和 1–256 的非空通道快照；执行宿主预检与托盘准备成功后才建立周期、冻结入口数据并排入现有 R160 ACK 流程；准备异常不留活动周期且不写 R160。
- [x] 使轮询按入口区分输入：自动模式读取并处理 R150/R151/R152；手动模式不读取 R150/R151，不扫描、不检查压合、不等待 PLC 启动位；手动 R160 ACK 后直接触发同一初测分派；复测、发布、R161/R152/NG 档位/R162 与恢复逻辑继续复用。
- [x] 在初测放行处按周期入口选择宿主能力：自动仍调用现有 `StartInitialPass`；手动在设备条件再次满足后调用 `StartManualInitialPass`，参数只取周期通道快照。
- [x] 以 RED 测试为驱动实现最小代码，并运行 `dotnet run --project tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`，确认上述周期测试全部通过。

## Task 3：把主测试页手动启动接到周期协调器

**Files:**
- Modify: `frmTest.vb`
- Test: `tools/Test-ManualUnifiedCycleContract.ps1`
- Test: `tools/ManualCycleRuntimeTests/Program.vb`

- [x] 让手动模式下的 `StartManualPlcConnection()` 创建同一个 PLC 周期协调器并调用 `StartManual()`；自动/手动切换都先停止并释放旧协调器，再按所选入口重建，避免手动协调器残留导致自动入口不能启动。
- [x] 实现 `IManualTestHost`：启动前检查已登录、无报警、手动模式、无测量/初始化应用、PLC 为支持 R160 的 ASCII 串口模式且连接可用；查询设备通信条件；托盘准备沿用现有 `PrepareTrayForTest` 与调试临时代码规则。
- [x] 为 `PrepareTrayForTest` 和调试计划增加可选的标准通道输入；手动准备使用按钮提交的快照，不在准备或测量时重新读取 `SelectedChannelNumbers`。
- [x] 实现手动初测宿主回调，调用现有 `StarTest(1, True, ..., snapshotChannels)`，使测量继续走 `FormalTestRunner` 与协调器结果回调，不走主页面手动分支的即时保存和 NG 弹窗；复测继续用现有 `StartRetestPass`。
- [x] 将 `CmdStart_Click` 改为提交托盘码和当前通道快照至 `RequestManualCycle`；移除该手动入口的 `GetSignalTrayStatus()`、窗体直写 PLC、同步 `StarTest(1)` 路径。协调器接受请求后再统一进行准备、R160 ACK 和手动直启。
- [x] 根据协调器活动周期状态禁用重复启动、自动/手动切换和通道控件；停止先于已排队的手动初测 UI 回调执行时调用 `RequestStopCurrentCycle()` 并废弃回调；测量结束但周期握手未结案时不得提前恢复选择。
- [x] 最终发布按入口快照（自动计划或手选计划）确定通道，并显式传递周期托盘码至 OCV_DATA 任务代码、幂等键和本地 CSV，避免最终发布再依赖界面值。
- [x] 添加并运行 `tools/Test-ManualUnifiedCycleContract.ps1`，确认手动按钮只走协调器、启动前无压合检查、手动启动没有扫码/R150/R151/直接测量调用、R160 ACK 后立即触发测量，且自动入口仍保留现有 R150→扫码→R160→R151。

## Task 4：完成回归与构建核验

**Files:**
- Test: `tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`
- Test: `tools/Test-ManualUnifiedCycleContract.ps1`
- Test: `tools/Test-PlcV4StateMachine.ps1`
- Test: `tools/Test-MainTestSelectionSnapshot.ps1`
- Build: `MP-60A-RV-64Mem.sln`

- [x] 运行独立周期测试和手动入口合约测试。
- [x] 运行 `tools/Test-PlcV4StateMachine.ps1`、`tools/Test-MainTestSelectionSnapshot.ps1`、`tools/Test-MainTestPlcLifecycle.ps1`、`tools/Test-TrayTestPreparationRuntime.ps1` 与 `tools/Test-ManualPlcAsciiContract.ps1`；另验证 PLC ACK、周期恢复、NG 交接与自动结果通道回归。对实现前已失败或与现有代码契约不一致的脚本，只修正已确认过时的事件文案、调用格式、R161 后状态名和固定宽度 DT 地址断言，不改变原有业务行为。
- [x] 使用 `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe MP-60A-RV-64Mem.sln /t:Build /p:Configuration=Debug /p:Platform=x86 /v:minimal` 构建完整解决方案；外部设备 DLL/Modbus 与 HttpClient 依赖从主工作区的既有包/忽略构建缓存复制到隔离工作区，未纳入版本控制。
- [x] 审阅完整 diff，确认主工作区既有 `frmTest.vb` 修改未混入，PLC TCP 没有虚构 R160 映射，自动 R150/扫码流程未被手动入口改变。
