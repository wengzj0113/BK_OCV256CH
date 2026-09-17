# 自动测试状态机实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** 移除旧的 autoStartFlag + Timer4 触发链路，实现托盘级自动测试状态机：托盘到位、扫码、条码加载、正式测试、本地保存、PLC 完成通知；MES 后台上传，NG 复测独立执行。

**Architecture:** AutoTestWorkflow 只负责状态和单托盘锁；frmTest 负责 WinForms 和现有 StarTest(1) 测量入口；PLC、扫码器、MES 使用小型适配器。测试结果先生成不可变快照，再分别进入 MES 和 PLC 的 Outbox，后台任务不得读取下一托盘的全局状态。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.8、Modbus RTU/TCP、SqlClient、现有 OcvDataService.UploadOutbox、PowerShell 源码检查、MSBuild。

**Design reference:** docs/superpowers/specs/2026-09-17-auto-test-state-machine-design.md

---

## 范围和禁止事项

本计划只处理：

    托盘到位 → 扫码 → 查询电池条码 → StarTest(1)
    → OCV_DATA/本地保存 → PLC 完成通知
    → MES 后台上传

不新增生产工艺动作，不让上位机替代 PLC 松夹具和托盘流转，不修改 FrmManualTest 的独立调试流程。

PLC 地址表和扫码器协议未确认前，只实现接口、模拟适配器和软件检查，不猜测地址、寄存器编码、共享内存名称或扫码完成信号。

## 文件变更总览

新建：

- AutoTestWorkflowModels.vb：状态、上下文、结果快照、PLC 通知模型。
- AutoTestWorkflow.vb：状态机和单托盘锁。
- PlcAutomationContracts.vb：PLC 自动流程配置。
- TrayScannerService.vb：扫码业务接口和适配边界。
- MesDataService.vb：独立 MES 查询。
- TestResultSnapshot.vb：不可变结果快照。
- PlcCompletionWorker.vb：PLC 完成通知 Outbox worker。
- tools/Test-AutoTestWorkflow.ps1：源码契约检查。
- tools/Test-AutoTestWorkflowScenarios.ps1：无设备场景检查。

修改：

- PlcController.vb、Module1.vb：PLC 接口和配置。
- frmInit.vb、frmInit.Designer.vb：MES 配置。
- frmTest.vb、frmTest.Designer.vb：状态机接入、测试边界和复测。
- ResultDataUploadAssembly.vb、OcvDataService.vb：快照上传和 Outbox。
- FrmMesTask.vb 或主窗体生命周期：启动/停止后台 worker。
- MP-60A-RV-64Mem.vbproj：注册新文件。

保留但停用：

- autoStartFlag、旧 AnalyzeData 自动位解析、Timer4 对 StarTest(1) 的调用。
- frmInput/CWtray.exe 在新扫码适配器验证前保留，但不再作为主自动流程入口。

## Task 1：建立回归契约

**Files:**

- Create: tools/Test-AutoTestWorkflow.ps1
- Create: tools/Test-AutoTestWorkflowScenarios.ps1
- Test: frmTest.vb、PlcController.vb、ResultDataUploadAssembly.vb

- [ ] **Step 1: 写入旧链路检查**

检查以下条件：

    frmTest 中不存在 Timer4_Tick 到 StarTest(1) 的可达调用；
    frmTest 引用了 AutoTestWorkflow；
    PlcController 包含 ReadTrayInPosition 和 NotifyTestCompleted；
    ResultDataUploadAssembly 不在后台读取 RecieveData、txtTray 或 trayCodeSet；
    StarTest 的正式测试路径不递归调用 StarTest(0)。

- [ ] **Step 2: 写入场景清单**

固定检查以下场景：

    自动模式在托盘未稳定到位前不启动；
    同一托盘未离开前只处理一次；
    扫码或条码加载失败不启动测试；
    本地保存成功后不等待 MES 即通知 PLC；
    MES 上传使用原托盘快照；
    PLC 通知重试使用同一个幂等键；
    NG 复测不阻塞自动周期。

- [ ] **Step 3: 运行基线并提交**

运行：

    powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflow.ps1

预期：基线失败，失败项应准确指向尚未实现的新接口和仍存在的旧 Timer4 链路。

提交：

    git add tools/Test-AutoTestWorkflow.ps1 tools/Test-AutoTestWorkflowScenarios.ps1
    git commit -m "test: lock automatic test workflow boundaries"

## Task 2：建立状态机核心和不可变上下文

**Files:**

- Create: AutoTestWorkflowModels.vb
- Create: AutoTestWorkflow.vb
- Modify: MP-60A-RV-64Mem.vbproj
- Test: tools/Test-AutoTestWorkflowScenarios.ps1

- [ ] **Step 1: 定义状态**

状态必须包含：

    WaitingForTray
    TrayDetected
    Scanning
    LoadingCellBarcodes
    ReadyToTest
    Testing
    SavingLocalResult
    PlcCompletionPending
    WaitingForPlcAck
    Completed
    Error

- [ ] **Step 2: 定义托盘上下文**

AutoTestContext 必须保存：

    CycleId
    TrayCode
    ChannelBarcodes
    SelectedChannels
    TestTaskId
    LocalSaveCompleted
    PlcCompletionQueued

上下文创建后，流程不得从 txtTray、RecieveData 或通道控件重新读取当前任务数据。

- [ ] **Step 3: 定义最小依赖接口**

在 AutoTestWorkflowModels.vb 定义：

    ITraySignalSource.ReadTrayInPosition()
    ITraySignalSource.ReadTestAllowed()
    ITrayScanner.StartScan(cancellationToken)
    ITrayCellBarcodeProvider.GetTrayCellBarcodes(trayCode)
    IPlcCompletionNotifier.NotifyTestCompleted(notice)
    IPlcCompletionNotifier.IsCompletionAcknowledged(notice)

接口不得引用 WinForms 控件。

- [ ] **Step 4: 实现状态转换和单任务锁**

只允许以下主流程：

    WaitingForTray → TrayDetected → Scanning
    → LoadingCellBarcodes → ReadyToTest → Testing
    → SavingLocalResult → PlcCompletionPending
    → WaitingForPlcAck → Completed → WaitingForTray

使用 SemaphoreSlim 或等价锁，保证 PLC 轮询、扫码回调、MES 查询和 PLC 通知不会并发处理多个托盘。

- [ ] **Step 5: 实现到位/离开采样**

连续采样达到稳定次数才创建 CycleId。托盘持续到位时不重复扫码；托盘连续离开达到离开次数后才释放上下文，允许下一托盘。

- [ ] **Step 6: 注册、检查、提交**

运行：

    powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflowScenarios.ps1

然后提交：

    git add AutoTestWorkflowModels.vb AutoTestWorkflow.vb MP-60A-RV-64Mem.vbproj tools/Test-AutoTestWorkflowScenarios.ps1
    git commit -m "feat: add automatic test workflow state model"

## Task 3：扩展 PLC 自动生命周期接口

**Files:**

- Create: PlcAutomationContracts.vb
- Modify: PlcController.vb、Module1.vb、MP-60A-RV-64Mem.vbproj
- Test: tools/Test-AutoTestWorkflow.ps1

- [ ] **Step 1: 定义 PLC 配置**

PlcAutomationSettings 必须包含：

    TrayInPositionAddress
    TestAllowedAddress
    TestCompletedCommandAddress
    TestCompletedAckAddress
    StableSamples
    LeaveSamples
    PollIntervalMs

地址未配置时禁止启用自动模式。稳定次数和轮询间隔可以使用安全的软件默认值。

- [ ] **Step 2: 扩展 IPlcController**

增加：

    ReadTrayInPosition()
    ReadTestAllowed()
    NotifyTestCompleted(PlcTestCompletedNotice)
    IsTestCompletedAcknowledged(PlcTestCompletedNotice)

保留现有夹具方法，但状态机不得调用 OpenTray 或 CloseTray。

- [ ] **Step 3: 统一 RTU/TCP 地址访问**

地址和命令值只能从 PlcAutomationSettings 读取。RTU/TCP 只保留传输差异，不在两个实现中复制业务地址常量。

完成通知必须携带稳定 IdempotencyKey，PLC 重发相同 key 时不能重复执行机械动作。

- [ ] **Step 4: 配置兼容和检查**

在 Module1.vb 末尾追加配置字段，旧配置缺字段时使用空值。运行源码检查，确认没有猜测地址常量。

提交：

    git add PlcAutomationContracts.vb PlcController.vb Module1.vb MP-60A-RV-64Mem.vbproj tools/Test-AutoTestWorkflow.ps1
    git commit -m "feat: add PLC automatic lifecycle contract"

## Task 4：接入扫码器和 MES 条码查询

**Files:**

- Create: TrayScannerService.vb、MesDataService.vb
- Modify: frmInit.vb、frmInit.Designer.vb、Module1.vb、MP-60A-RV-64Mem.vbproj
- Test: tools/Test-AutoTestWorkflow.ps1

- [ ] **Step 1: 固定扫码业务接口**

TrayScannerService 只暴露：

    StartScan(cancellationToken) As Task(Of TrayScanResult)

TrayScanResult 包含 IsSuccess、TrayCode、ErrorMessage。frmTest 不得直接启动进程、读取共享内存或拼接扫码协议。

- [ ] **Step 2: 封装现有扫码实现**

如果现场确认继续使用 CWtray.exe，则把现有启动和结果读取封装为 LegacyTrayScannerAdapter；不复制 frmInput 整个窗体，不猜测路径、共享内存名或完成标志。缺少配置时返回失败。

- [ ] **Step 3: 增加独立 MES 配置**

在 Initset 和 frmInit 中增加：

    MesDatabaseServerIP
    MesDatabaseName
    MesBaseUser
    MesBasePsw

密码控件启用掩码。四项为空时拒绝正式查询。连接测试只测试连接，不修改任何数据。

- [ ] **Step 4: 实现参数化查询**

MesDataService 使用独立连接和参数化托盘码查询，返回：

    IReadOnlyDictionary(Of Integer, String)

校验通道 1 到 256、通道唯一、电池条码非空、条码不重复，并报告缺失/重复/非法通道。不得使用 OcvDataService 访问客户 MES，也不得将客户 MES 明细写回 OCV_DATA TrayCell。

- [ ] **Step 5: 构建并提交**

运行：

    powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflow.ps1

提交：

    git add TrayScannerService.vb MesDataService.vb frmInit.vb frmInit.Designer.vb Module1.vb MP-60A-RV-64Mem.vbproj tools/Test-AutoTestWorkflow.ps1
    git commit -m "feat: add scanner and MES tray barcode adapters"

## Task 5：固化结果快照和本地保存

**Files:**

- Create: TestResultSnapshot.vb
- Modify: frmTest.vb、MP-60A-RV-64Mem.vbproj
- Test: tools/Test-AutoTestWorkflowScenarios.ps1

- [ ] **Step 1: 定义快照**

TestResultSnapshot 必须复制：

    TestTaskId、CycleId、TrayCode、OcvStageCode、TestType、CreatedAt
    环境温度、探针剩余次数
    每个通道的通道号、电池条码、电压、内阻、判定和 NG 原因

后台任务只接收快照，不读取 RecieveData、txtTray、trayCodeSet、currentOCVNum 或当前通道选择。

- [ ] **Step 2: 固定保存顺序**

正式测试后执行：

    结果回填
    → 判定 OK/NG
    → SaveOcvDataTestResults
    → CompleteOcvDataTestTask(Completed)
    → 创建 TestResultSnapshot
    → 保存本地文件
    → 入队 MES
    → 入队 PLC 完成通知

OCV_DATA 或本地文件任一保存失败，都不得通知 PLC。

- [ ] **Step 3: 改造本地文件保存**

将 SaveTestDataLocalFile 改为接收 TestResultSnapshot。先写：

    d:\OcvTestData\yyyy-MM-dd\<TrayCode>_<TestTaskId>.csv.tmp

关闭成功后替换正式 CSV。相同 TestTaskId 的完整文件视为已保存，不生成 _1、_2 副本。

- [ ] **Step 4: 提交快照边界**

场景检查必须覆盖：先创建托盘 A 快照，再修改模拟全局数据为托盘 B，后台消费内容仍然是托盘 A。

提交：

    git add TestResultSnapshot.vb frmTest.vb MP-60A-RV-64Mem.vbproj tools/Test-AutoTestWorkflowScenarios.ps1
    git commit -m "feat: persist immutable formal test snapshots"

## Task 6：改造 MES 后台上传

**Files:**

- Modify: ResultDataUploadAssembly.vb、OcvDataService.vb、frmTest.vb
- Test: tools/Test-AutoTestWorkflow.ps1、tools/Test-AutoTestWorkflowScenarios.ps1

- [ ] **Step 1: 增加快照入队方法**

增加 EnqueueSnapshot(snapshot)，在本地保存完成后立即把完整 JSON 写入现有 UploadOutbox：

    UploadType = ResultDataUploadAssembly
    BusinessKey = TrayCode + ":" + TestTaskId
    IdempotencyKey = ResultDataUploadAssembly:<TestTaskId>:<TestType>

- [ ] **Step 2: 删除全局状态读取**

ResultDataUploadAssembly 的后台上传只能消费 UploadOutbox.PayloadJson。删除在 UploadDataAsync 中读取 RecieveData、trayCodeSet、currentOCVNum 和 frmTest 的逻辑。

- [ ] **Step 3: 保留失败重试**

HTTP/MES 返回失败时调用现有 MarkUploadFailed；成功时调用 MarkUploadSent。重试消费同一 PayloadJson 和同一幂等键，不重新生成当前托盘数据。

- [ ] **Step 4: 验证并提交**

运行场景检查，确认 MES 上传不等待响应、不阻塞 PLC 通知，并提交：

    git add ResultDataUploadAssembly.vb OcvDataService.vb frmTest.vb tools/Test-AutoTestWorkflow.ps1 tools/Test-AutoTestWorkflowScenarios.ps1
    git commit -m "feat: upload immutable test snapshots in background"

## Task 7：增加 PLC 完成通知 worker

**Files:**

- Create: PlcCompletionWorker.vb
- Modify: frmTest.vb、PlcController.vb、FrmMesTask.vb 或主生命周期文件、MP-60A-RV-64Mem.vbproj
- Test: tools/Test-AutoTestWorkflowScenarios.ps1

- [ ] **Step 1: 将完成通知写入现有 Outbox**

本地保存成功后写入：

    UploadType = PlcTestCompleted
    IdempotencyKey = PlcTestCompleted:<TestTaskId>
    BusinessKey = TrayCode + ":" + TestTaskId
    PayloadJson = PlcTestCompletedNotice JSON
    TestTaskId = 当前正式任务

不使用随机 GUID，否则重试无法识别为同一个 PLC 事件。

- [ ] **Step 2: 实现 worker**

worker 每轮只读取 PlcTestCompleted 类型。发送成功且收到 PLC 应答后 MarkUploadSent；发送失败或应答超时后 MarkUploadFailed。重试使用原始 JSON 和幂等键，禁止重新测量。

- [ ] **Step 3: 隔离两个 worker**

ResultDataUploadAssembly 只消费 ResultDataUploadAssembly。PlcCompletionWorker 只消费 PlcTestCompleted。MES 失败不得改变 PLC 通知状态，PLC 失败不得重复上传 MES。

- [ ] **Step 4: 生命周期和恢复**

主页面加载后启动单实例 worker，应用关闭时停止。应用重启后继续处理 Pending、Failed 和超时 Uploading 记录。

- [ ] **Step 5: 提交**

场景检查必须覆盖 PLC 断线、应答超时、相同幂等键重发和应用重启恢复。

    git add PlcCompletionWorker.vb frmTest.vb PlcController.vb FrmMesTask.vb MP-60A-RV-64Mem.vbproj tools/Test-AutoTestWorkflowScenarios.ps1
    git commit -m "feat: notify PLC after local result persistence"

## Task 8：接入 frmTest，移除旧自动触发并隔离复测

**Files:**

- Modify: frmTest.vb、frmTest.Designer.vb、AutoTestWorkflow.vb、AutoTestWorkflowModels.vb
- Test: tools/Test-AutoTestWorkflow.ps1、tools/Test-AutoTestWorkflowScenarios.ps1

- [ ] **Step 1: 接入自动/手动分流**

复用现有 chkAutoTest 和 chkManualTest：

    自动：Loaded 后只调用一次 StarTest(1)
    手动：Loaded 后只加载和显示，等待 CmdStart_Click
    Testing、Saving、PlcCompletionPending、WaitingForPlcAck：模式控件禁用

- [ ] **Step 2: 冻结自动通道集合**

自动条码加载成功后按查询结果设置 SelectedChannelNumbers，并禁用通道选择。测试入口只使用上下文中的通道集合，不重新从控件读取。

- [ ] **Step 3: 改造 StarTest 边界**

保留 FormalTestRunner 作为测量核心。自动流程负责一次性调用 StarTest(1)，结果保存、快照、MES 入队和 PLC 入队由外层顺序控制。

移除正式测试完成后的自动确认和递归 StarTest(0)。NG 只记录 retestArray；btnNGTest_Click 仍作为单独的人工复测入口。

- [ ] **Step 4: 删除旧 Timer4 链路**

删除 autoStartFlag 的业务赋值和 Timer4_Tick 对 StarTest(1) 的调用，停止启动 Timer4。timer3 如果仍用于温度、急停和烟感监测可以保留，但不能再设置自动测试启动标志。

- [ ] **Step 5: 保持自动周期锁**

StarTest 返回后，工作流仍保持当前 CycleId，直到 PLC 完成通知已入队、PLC 应答已处理并检测到托盘离开。isRuning 只用于兼容 UI，不作为自动周期唯一锁。

- [ ] **Step 6: 独立复测**

btnNGTest_Click 只能在没有自动 Testing、Saving 或 WaitingForPlcAck 任务时调用 StarTest(0)。复测生成独立测试任务、独立快照和独立 MES 幂等键，不再次触发 PLC 托盘流转。

- [ ] **Step 7: 提交接入改造**

运行：

    powershell -ExecutionPolicy Bypass -File .\tools/Test-AutoTestWorkflow.ps1

预期：不存在可达的 Timer4 → StarTest(1)，自动入口只有 AutoTestWorkflow，正式测试不递归 StarTest(0)。

    git add frmTest.vb frmTest.Designer.vb AutoTestWorkflow.vb AutoTestWorkflowModels.vb tools/Test-AutoTestWorkflow.ps1 tools/Test-AutoTestWorkflowScenarios.ps1
    git commit -m "feat: connect automatic workflow and isolate retest"

## Task 9：构建、回归和现场联调前检查

**Files:**

- Test: tools/Test-AutoTestWorkflow.ps1
- Test: tools/Test-AutoTestWorkflowScenarios.ps1
- Verify: MP-60A-RV-64Mem.vbproj

- [ ] **Step 1: 运行新检查**

    powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflow.ps1
    powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflowScenarios.ps1

预期：源码契约和七个无设备场景全部 PASS。

- [ ] **Step 2: 运行现有回归**

    powershell -ExecutionPolicy Bypass -File .\tools\Test-MainTestSelectionSnapshot.ps1
    powershell -ExecutionPolicy Bypass -File .\tools\Test-MainTestTrayFailureFeedback.ps1
    powershell -ExecutionPolicy Bypass -File .\tools\verify-manual-test-protocol-log.ps1
    powershell -ExecutionPolicy Bypass -File .\tools\Assert-WinFormsDesignerSerialization.ps1

- [ ] **Step 3: 重建项目**

    msbuild .\MP-60A-RV-64Mem.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform="AnyCPU"

预期：退出码 0，无新编译错误，无 Designer 序列化错误。

- [ ] **Step 4: 现场协议确认**

真实设备联调前必须取得并记录：

    PLC 托盘到位、测试允许、完成命令、完成应答、托盘离开地址；
    PLC 有效电平、寄存器顺序、字节序和幂等键承载方式；
    扫码启动、成功、超时、取消协议；
    MES 数据库、字段、查询超时和错误码。

协议未确认时只运行模拟适配器，不宣称自动流程现场验收通过。

- [ ] **Step 5: 检查最终变更范围**

    git diff --check
    git status --short

确认没有改动 FrmManualTest、FormalTestRunner、仪器通信和无关 UI。

## 执行原则

严格按 Task 1 至 Task 9 顺序执行，每个 Task 单独提交并通过对应检查后才进入下一项。不得顺手删除旧窗体、重写 FormalTestRunner 或改动独立手动测试模块。
