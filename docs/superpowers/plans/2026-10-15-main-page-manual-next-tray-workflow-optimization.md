# 主测试页手动模式测试完成后换托盘流程优化 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans` to implement this plan task by task. Steps use checkbox syntax for tracking.

**Goal:** 在主测试页手动模式下，当前托盘的最终结果保存且 R161 成功发送后，允许操作员录入、准备并启动下一托盘，同时完整保留上一托盘待处理的 NG 分选数据与 R152/R162 握手。

**Architecture:** 将“当前托盘测试周期”和“已完成托盘的待分选握手”作为两个独立生命周期。保留 `IsCycleOccupied` 对完整 PLC 流程的含义，另提供明确的手动下一周期可接受状态；界面只在当前测试上下文已释放后恢复托盘码、电芯条码准备、通道选择和启动控件。协调器继续串行处理上一托盘 R152/NG 档位/R162；若下一托盘先完成，沿用现有规则延迟其 R161，直到上一分选握手结束。

**Tech Stack:** VB.NET WinForms、PLC ASCII 串口协调器、.NET 8 VB 运行时测试项目、PowerShell 源码合约测试、现有 MSBuild 解决方案。

---

## 已确认行为与边界

1. 本方案只改主测试页 `frmTest` 左侧“手动”模式（`chkManualTest`）；独立窗体 `FrmManualTest`、自动扫码入口和调试条码规则不在范围内。
2. 主页面的 `btnGetBarcode` 根据当前托盘码查询/准备托盘内电芯码，不负责扫描托盘码。手动托盘码仍由操作员录入。
3. 手动测试仍走协调器流程：操作员提交托盘码及选中通道，完成托盘准备后发送 R160；PLC 提供有效 R151 后才开始测量。
4. NG 复测与 NG 分选是两个不同阶段。NG 复测继续由 `btnNeedNGRetest` 与 `cmbRetestNum` 控制；NG 分选继续由 `RequiresNgSorting()` 判断，目前该方法仅在 `currentOCVNum = 1`（OCV0）时返回真。
5. 需要分选的周期在 R161 成功后保存最终 NG 通道集合。收到 R152 后写入最终 NG 档位，再发 R162。无 NG 通道时跳过档位写入，仍按现有协议完成 R162。
6. 手动模式应允许在上一份 NG 分选等待 R152/R162 时准备并开始下一托盘。现有自动协调器允许下一托盘进入，同时会在上一份 R162 完成前延迟下一托盘的 R161；手动入口应遵循相同顺序。
7. 不因释放界面控件而清空 `pendingNgSelection`、提前发送 R162、绕过 R152，或让新托盘数据覆盖上一托盘的待分选集合。

## 文件职责

| 文件 | 职责 |
|---|---|
| `PlcAutomaticTestCoordinator.vb` | 区分活动托盘周期与旧托盘待分选握手；新增手动周期可接受/可停止状态；允许旧分选挂起时提交下一手动周期；保留 R161/R152/NG档位/R162 顺序。 |
| `frmTest.vb` | 将托盘输入、获取电芯条码、通道选择、启动和停止控件绑定到对应的生命周期状态；显示上一托盘分选待处理状态。 |
| `tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj` | 补充链接 PLC NG 档位帧协议源码，使当前独立运行时测试项目可编译。 |
| `tools/ManualCycleRuntimeTests/Program.vb` | 覆盖手动旧托盘分选与新托盘周期重叠、R161 延后、停止保护、重复启动、零 NG 和 R152/R162 重试。 |
| `tools/Test-ManualUnifiedCycleContract.ps1` | 校验主页面控件状态与协调器新可接受状态的绑定，并保护原手动 R160/R151 测试入口。 |
| `tools/Test-PlcV4StateMachine.ps1`、`tools/Test-StopTestContract.ps1` | 回归共享 PLC 握手状态、停止语义和原自动模式行为。 |

## 状态规则

| 当前状态 | 托盘码/获取电芯条码/通道 | 启动测试 | 停止 | 说明 |
|---|---|---|---|---|
| 正在准备、等待 R151、测量、NG 复测、保存最终结果或发送当前托盘 R161 | 锁定 | 禁用 | 按现有停止能力开放 | 当前托盘上下文不可被下一托盘改写。 |
| 当前托盘 R161 成功；旧托盘 NG 集合等待 R152 | 开放 | 开放 | 禁用（没有活动测量周期时） | 新手动周期可开始；旧 NG 集合仍单独保留。 |
| 旧 R152 到达，NG 档位写入或 R162 重试中，且没有活动新周期 | 开放 | 开放 | 禁用 | R152/R162 重试不占用新周期上下文；通信由协调器串行处理。 |
| 新托盘周期运行，同时旧 NG 分选仍待处理 | 锁定 | 禁用 | 按现有停止能力开放 | 旧分选仍可在 R152 到达时完成；新周期 R161 必须等旧 R162 成功。 |
| 当前托盘结果保存或 R161 发送失败、协调器处于故障状态 | 锁定 | 禁用 | 按现有恢复/停止流程 | 不允许绕过未完成的当前托盘收尾。 |

自动/手动模式、调试模式和初始化设置继续受完整未结周期状态限制。仅为操作下一托盘而放开的控件，不代表允许更改正在执行的测试快照。

## Task 1：修复手动周期测试项目并建立重叠周期回归

**Files:**
- Modify: `tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`
- Modify: `tools/ManualCycleRuntimeTests/Program.vb`
- Test: `tools/ManualCycleRuntimeTests/TestContracts.vb`

- [ ] **Step 1: 补全运行时测试项目引用**

在 `ManualCycleRuntimeTests.vbproj` 的 `<ItemGroup>` 中增加对真实 PLC 档位帧协议的源码链接：

```xml
<Compile Include="..\..\PlcAutomationProtocol.vb" Link="PlcAutomationProtocol.vb" />
```

保留当前测试项目链接的协调器、状态和事件日志源码。执行 `dotnet run --project tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`；基线应越过当前 `PlcAutomationProtocol` 未声明编译错误，继续运行现有用例。

- [ ] **Step 2: 扩展假主页面以支持两个不同托盘**

在 `Program.vb` 的 `FakeHost` 中允许 `TRAY-1` 和 `TRAY-2`，记录每次准备的托盘码、通道集合及最终结果通道；为第二个手动初测设置可辨识的最终 NG 通道集合。现有 `TestManualCycleUsesSharedNgSortingHandoff` 保持第一托盘 NG 通道为 3。

- [ ] **Step 3: 编写失败用例，验证旧分选挂起时仍可接受下一手动托盘**

新增 `TestManualNextCycleCanStartWhilePreviousNgSelectionWaitsForR152`。第一托盘完成并成功发送 R161、旧 NG 通道 3 等待 R152 后，直接断言 `RequestManualCycle("TRAY-2", channels)` 返回真，且只接受一个第二周期；当前版本预期在 `RequestManualCycle` 断言处失败。新增的 `CanAcceptManualCycle` 属性断言放到 Task 2 实现后，避免基线用例因尚未定义成员而编译失败。

- [ ] **Step 4: 编写失败用例，验证旧分选与新周期收尾顺序**

在上一步中让第二托盘收到 R151 并完成测量。在 PLC 置 R152=1 后，断言先只为第一托盘写 NG 通道 3 的档位并发送一次 R162；该次轮询结束时第二托盘 R161 尚未发送。下一次轮询才允许重试第二托盘 R161。PLC 将 R152 复位为 0 后再次置 1，断言第二托盘最终 NG 集合单独写入，再发送第二次 R162。整个过程不得再次测量或重复保存任一托盘结果。

- [ ] **Step 5: 运行定向基线并记录失败点**

运行 `dotnet run --project tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`。新行为断言应因当前入口把 `pendingNgSelection` 当成禁止启动条件而失败；已有手动 R152/R162 和自动模式测试不得出现新的编译失败。

- [ ] **Step 6: 提交测试基线**

仅提交本任务文件：`git add tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj tools/ManualCycleRuntimeTests/Program.vb`，检查 `git diff --cached --name-only` 仅包含上述两个文件，再提交 `git commit -m "test: cover manual next tray during pending ng sort"`。不要把工作区其他已有改动加入提交。

## Task 2：将下一手动周期资格与待分选握手状态分离

**Files:**
- Modify: `PlcAutomaticTestCoordinator.vb`
- Test: `tools/ManualCycleRuntimeTests/Program.vb`

- [ ] **Step 1: 新增只读手动周期资格属性**

增加 `CanAcceptManualCycle`，由状态锁内的单一判断方法计算。它只在手动轮询已启动、协调器未停止/故障、当前 `cycle` 没有托盘码或测量请求、没有托盘确认/R160/R161 写入、没有当前准备/启动请求时返回真。允许存在属于上一托盘的 `pendingNgSelection`，也允许状态停留在该旧分选的 `WaitingR152` 或 `WaitingR162Ack`。若没有旧 `pendingNgSelection` 而仍有未确认的早到 R152/R162 请求，则保持不可启动。

用下列逻辑约束实现，具体辅助字段沿用协调器当前字段名：

```vb
Private Function CanAcceptManualCycleLocked() As Boolean
    If stopped OrElse Not manualPolling OrElse manualCycleRequestInProgress Then Return False
    If Not String.IsNullOrWhiteSpace(cycle.TrayCode) OrElse cycle.TestStarted OrElse cycle.FinalResultPublished Then Return False
    If trayConfirmationPending OrElse r160WriteInProgress OrElse r161WriteInProgress OrElse
       testRequestPending OrElse trayPreparationInProgress OrElse trayRequestPending Then Return False
    If sortingRequestPending AndAlso pendingNgSelection Is Nothing Then Return False
    If currentState = PlcAutomaticTestState.Idle Then Return True
    Return pendingNgSelection IsNot Nothing AndAlso
           (currentState = PlcAutomaticTestState.WaitingR152 OrElse
            currentState = PlcAutomaticTestState.WaitingR162Ack)
End Function
```

- [ ] **Step 2: 让手动入口按新资格接收周期**

将 `RequestManualCycle` 中基于 `IsCycleOccupiedLocked()` 和 `currentState <> Idle` 的拒绝判断，改为同一把 `stateLock` 下调用 `CanAcceptManualCycleLocked()`。登录、托盘码、通道快照、设备预检、托盘数据准备及成功后才建立周期的顺序保持不变。准备失败不得改变旧 `pendingNgSelection` 或旧 R152/R162 状态。

- [ ] **Step 3: 保留分选对象与下一托盘对象隔离**

继续将 R161 成功后的最终 NG 通道复制进 `PendingNgSelection.TrayCode` 和 `.Channels`，随后只重置当前 `cycle`。`PublishNgGrades` 只能消费该固定待分选快照；新托盘启动/准备不得从窗体当前托盘码、`activeTrayPlan` 或新测量结果重建旧选择。重试状态只记录在该待分选对象自身的 `WrittenChannels`/`GradesWritten` 中。

- [ ] **Step 4: 保持“新周期先完成”时的 R161 延后规则**

保留 `SendTestCompletionSignal` 对已有 `pendingNgSelection` 的等待条件：上一分选未完成时，不发送新周期 R161；上一 R162 成功清除该待分选对象后，由轮询重试新周期 R161 一次。不得重复调用 `PublishFinalCycle`、重复创建 OCV_DATA 任务或重复测量。

- [ ] **Step 5: 处理新周期停止时的旧分选请求**

调整 `RequestStopCurrentCycle` 的清理范围：停止第二个活动周期时只取消第二个当前 `cycle`；若存在属于已完成托盘的 `pendingNgSelection`，保留其对象及已捕获的 `TestCompleted` R152 请求，确保 R152/R162 后续仍可完成。仅在没有该待分选对象时，按现有规则清除未归属当前完成托盘的分选请求。用运行时测试覆盖停止期间旧 NG 选择不丢失。

- [ ] **Step 6: 运行手动周期运行时测试**

运行 `dotnet run --project tools/ManualCycleRuntimeTests/ManualCycleRuntimeTests.vbproj`。预期全部通过，并覆盖 R160/R151、跨托盘 NG/R152/R162 次序、重复启动拒绝、停止取消、R161/R162 重试和自动流程原行为。

- [ ] **Step 7: 提交协调器变更**

仅提交 `PlcAutomaticTestCoordinator.vb` 与 `tools/ManualCycleRuntimeTests/Program.vb` 的本任务改动；检查暂存文件名后提交 `git commit -m "fix: allow manual next tray during ng handoff"`，保留工作区其他既有变更。

## Task 3：按生命周期开放主测试页操作控件

**Files:**
- Modify: `frmTest.vb`
- Test: `tools/Test-ManualUnifiedCycleContract.ps1`
- Test: `tools/ManualCycleRuntimeTests/Program.vb`

- [ ] **Step 1: 将界面状态拆成当前周期状态与下一周期资格**

在 `UpdateTestModeVisuals` 中保留完整 `cycleOccupied` 用于模式切换、调试开关及初始化设置保护；另读取协调器 `CanAcceptManualCycle`，形成 `manualEntryEnabled = chkManualTest.Checked AndAlso CanAcceptManualCycle AndAlso Not initializationApplyInProgress AndAlso Not shutdownPrepared`。不要简单从 `IsCycleOccupiedLocked` 删除 `pendingNgSelection`，以免其他安全门控被意外放开。

- [ ] **Step 2: 按手动资格恢复托盘和通道控件**

手动模式且 `manualEntryEnabled` 时允许编辑 `txtTray`、点击 `btnGetBarcode`、调整通道选择；同一条件下允许 `CmdStart`，但仍要求当前校准快照有效。自动模式下保持托盘输入只读，由自动扫码流程取码。活动托盘从准备、R160、R151、测量到 R161 的任一阶段，继续禁用以上控件。

- [ ] **Step 3: 防止“仅有待分选握手”触发停止当前周期**

在协调器增加明确的 `CanStopCurrentCycle`/当前活动周期状态，供 `btnStop` 启用态与 `btnStop_Click` 共同使用。只存在上一托盘待 R152/R162、没有当前测量/托盘周期时，停止按钮不代表可取消待分选结果；当前新周期运行时，停止只取消当前周期并保留旧待分选状态。若停止时旧 `pendingNgSelection` 已捕获 R152 请求为 `TestCompleted`，`RequestStopCurrentCycle` 保留该请求及其消费标记，并将状态恢复为 `WaitingR152` 或 `WaitingR162Ack`；停止当前周期不得把它重置成 `NoTestStarted`。

- [ ] **Step 4: 明确显示上一托盘分选等待状态**

周期状态文本至少区分“当前托盘测试/保存中”“测试及结果保存完成，等待上一托盘 NG 分选请求 R152”“NG 档位已写入，等待 R162 重试/完成”“可录入下一托盘”。保留现有 `txtAlarm` 自动事件记录，不把 PLC 握手待处理误显示为测量仍在运行。

- [ ] **Step 5: 为手动入口失败提供明确反馈**

`CmdStart_Click` 收到协调器拒绝时，依据资格状态显示“当前托盘周期尚未释放”或 PLC/分选恢复状态；`btnGetBarcode_Click` 只在 `manualEntryEnabled` 时执行准备。再次点击启动不得创建第二个当前周期。准备失败仍保留操作员输入的新托盘码，便于修改后重试。

- [ ] **Step 6: 添加界面源码合约断言**

更新 `Test-ManualUnifiedCycleContract.ps1`，断言托盘输入、获取条码、通道选择和 `CmdStart` 使用手动下一周期资格；断言模式/调试仍使用完整占用状态；断言自动模式输入仍只读；断言待分选对象未清空且 NG 写入仍由 R152 触发。

- [ ] **Step 7: 提交主页面变更**

仅提交 `frmTest.vb` 与 `tools/Test-ManualUnifiedCycleContract.ps1` 的本任务改动；检查暂存范围后提交 `git commit -m "fix: reopen manual tray controls after test completion"`。

## Task 4：覆盖零 NG、异常恢复和自动流程回归

**Files:**
- Modify: `tools/ManualCycleRuntimeTests/Program.vb`
- Test: `tools/Test-PlcV4StateMachine.ps1`
- Test: `tools/Test-StopTestContract.ps1`
- Build: `MP-60A-RV-64Mem.sln`

- [ ] **Step 1: 覆盖零 NG 的手动分选收尾**

当 OCV0 手动周期最终 NG 通道为零时，收到 R152 不写任何通道档位，仍发送一次 R162 并释放旧待分选对象；确认可接受下一手动周期状态更新为真。

- [ ] **Step 2: 覆盖 NG 档位及 R162 重试**

模拟某通道档位写入失败和 R162 写入失败。确认只重试未成功的旧 NG 通道/握手，不重新测量、不重复最终保存、不混入新托盘 NG 通道；旧分选完成前新周期 R161 保持待发。

- [ ] **Step 3: 覆盖手动与自动行为隔离**

运行 `tools/Test-ManualUnifiedCycleContract.ps1`、`tools/Test-PlcV4StateMachine.ps1`、`tools/Test-StopTestContract.ps1`。确认自动 R150 扫码及 R151 启动流程未变，手动入口仍不启动扫码器/读取 R150，手动只在有效 R151 后测量。

- [ ] **Step 4: 构建完整解决方案**

运行 `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe MP-60A-RV-64Mem.sln /t:Build /p:Configuration=Debug /p:Platform=x86 /v:minimal`。预期解决方案构建成功；若本机 VS 路径不同，使用已安装 VS 2022 的 MSBuild 完成同一配置构建。

- [ ] **Step 5: 审阅变更边界**

审阅 `PlcAutomaticTestCoordinator.vb` 与 `frmTest.vb` 的最终 diff，确认只改变主测试页手动周期的入口可用时点与状态提示；NG 分选仍由 R152 驱动并以 R162 收尾；自动流程、PLC 寄存器协议、MES 上传、结果格式和独立 `FrmManualTest` 均未改变。

- [ ] **Step 6: 提交最终回归更新**

仅提交本任务实际修改的测试文件，检查暂存范围不含其他工作区文件后提交 `git commit -m "test: verify manual next tray and ng sorting lifecycle"`。

## 验收标准

1. 当前托盘尚未完成最终保存或 R161 发送时，托盘码、条码准备、通道选择和启动均保持禁用。
2. 当前托盘 R161 成功后，即使上一托盘仍等待 R152/R162，手动页面也能录入新托盘码、查询/准备电芯条码、调整通道并提交下一周期。
3. 旧托盘的 NG 分选集合始终使用其自身托盘码和最终 NG 通道快照；准备新托盘不会覆盖它。
4. PLC 对旧托盘发出 R152 后，上位机只写旧托盘最终 NG 档位并发送 R162；无 NG 时跳过档位写入但仍发送 R162。
5. 新周期可以在旧 R152/R162 处理期间测量；若新周期先完成，其最终结果只保存一次，R161 延迟到旧 R162 成功之后。
6. 停止新周期不会清除或漏掉旧托盘待分选对象及已经捕获的 R152 请求。
7. 重复点击启动、设备故障、准备失败、R160/R161/R162 写入失败均不会重复测量、丢弃待分选数据或覆盖活动托盘快照。
8. 自动模式、OCV0 NG 分选协议、非 OCV0 无分选行为、NG 复测设置和独立 `FrmManualTest` 行为保持原样。
