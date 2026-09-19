# 主页面手动/自动测试前置条件优化实施方案

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 统一手动和自动模式的托盘码入口，要求两种模式在正式测试前都完成托盘码校验、电芯信息加载和 PLC 托盘压合确认。

**Architecture:** 保留现有自动状态机和 `StarTest(1)` 测试入口，不新增第二套测试引擎。将“托盘码校验、电芯条码加载、PLC 压合检查”抽成共用的测试前置检查；自动模式在扫码成功后把条码写入 `txtTray`，再从 `txtTray` 生成当前测试上下文；手动模式点击开始后从 `txtTray` 读取托盘码并查询数据库。正式测试前再次读取 PLC 压合信号，只有通过后才调用 `StarTest`。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.8、Modbus RTU/TCP、SQL Server、现有 `OcvDataService`、现有 `MesDataService`、PowerShell 结构和场景测试。

---

## 1. 目标流程和边界

### 1.1 手动正式测试目标流程

```text
选择手动
  ↓
点击开始测试
  ↓
读取 txtTray
  ├─ 空：弹框并停止
  └─ 有值：查询托盘电芯信息
              ↓
        校验电芯信息
              ↓
        读取 PLC 压合状态
        ├─ 未压合/读取失败：提示并停止
        └─ 已压合：调用 StarTest(1)
                      ↓
                OCV_DATA 保存
                      ↓
                本地文件保存
                      ↓
                MES 后台上传
```

### 1.2 自动正式测试目标流程

```text
自动模式
  ↓
PLC 托盘到位且允许测试
  ↓
扫码
  ↓
扫码结果写入 txtTray
  ↓
从 txtTray 读取托盘码
  ↓
查询托盘电芯信息
  ↓
PLC 确认托盘已压合
  ├─ 未压合：保持等待，不调用 StarTest
  └─ 已压合：调用 StarTest(1)
                      ↓
                本地保存成功
                      ↓
                通知 PLC 测试完成
                      ↓
                等待 PLC ACK
```

### 1.3 不改变的行为

- 测试正式入口仍然是 `StarTest(1)`。
- 手动 NG 复测仍然使用 `StarTest(0)`，不自动递归复测。
- MES 上传仍然是后台任务，不阻塞 PLC 完成通知。
- PLC 后续松夹具、放行和下一步工艺仍由 PLC 负责。
- 不猜测 PLC 地址；压合状态必须配置为独立的 PLC 点位。

## 2. 文件职责和修改范围

### 2.1 新增文件

- `TestStartPreflight.vb`：共用测试前置校验结果、错误码和电芯数据加载协调，不执行正式测试。
- `tools/Test-TestStartPreflight.ps1`：验证空托盘码、数据库查询顺序、压合失败阻止测试等场景。

### 2.2 修改文件

- `frmTest.vb`：手动开始入口、自动扫码写入 `txtTray`、共用前置校验调用、UI 提示和测试锁。
- `AutoTestWorkflow.vb`：自动流程增加“等待压合”阶段，并保证数据库查询使用扫码后写入的 `txtTray` 值。
- `AutoTestWorkflowModels.vb`：增加托盘码写入适配器或等价接口，以及等待压合状态所需的上下文定义。
- `PlcAutomationContracts.vb`：增加独立的托盘压合信号配置和接口。
- `PlcController.vb`：实现读取托盘压合 PLC 点位，并与现有托盘到位、测试允许信号分开。
- `Module1.vb`：保存和读取托盘压合地址，旧配置缺少该字段时使用未配置值，不得默认为地址 0。
- `frmInit.vb`、`frmInit.Designer.vb`：增加托盘压合 PLC 地址配置项；如果当前 PLC 点位配置区已有统一布局，直接追加字段，不重做整个设置页。
- `MesDataService.vb`：复用现有托盘电芯查询，补齐统一的托盘码和 1～256 通道校验。
- `ResultDataUploadAssembly.vb`：保持不可变快照和后台 outbox 逻辑，确保手动和自动结果都使用同一上传数据结构。
- `MP-60A-RV-64Mem.vbproj`：把新增 VB 文件加入编译项。

## 3. 任务一：定义测试前置检查契约

**Files:**

- Create: `TestStartPreflight.vb`
- Modify: `PlcAutomationContracts.vb`
- Test: `tools/Test-TestStartPreflight.ps1`

- [ ] **Step 1: 定义明确的前置检查结果**

在 `TestStartPreflight.vb` 中定义以下类型，调用方根据结果决定是否显示提示或等待：

```vb
Public Enum TestStartFailure
    None
    TrayCodeMissing
    TrayCodeInvalid
    CellBarcodesMissing
    CellBarcodesInvalid
    TrayNotClamped
    PlcUnavailable
    AlarmActive
    NoSelectedChannel
End Enum

Public NotInheritable Class TestStartPreflightResult
    Public ReadOnly Property IsAllowed As Boolean
    Public ReadOnly Property Failure As TestStartFailure
    Public ReadOnly Property Message As String
    Public ReadOnly Property TrayCode As String
    Public ReadOnly Property CellBarcodes As IReadOnlyDictionary(Of Integer, String)
End Class
```

结果对象必须是只读快照；不允许把 `txtTray.Text` 或 `RecieveData` 的后续变化带入已经通过的前置检查。

- [ ] **Step 2: 固定统一校验规则**

托盘码校验规则：先 `Trim()`，不能为空；空值必须返回 `TrayCodeMissing`。

电芯信息校验规则：通道必须为 `1~256`，通道不能重复，条码不能为空，条码重复时返回明确错误通道。

PLC 压合规则：只能通过 `ReadTrayClamped()` 判断；读取异常返回 `PlcUnavailable`，读取到未压合返回 `TrayNotClamped`。

- [ ] **Step 3: 写失败场景测试**

`tools/Test-TestStartPreflight.ps1` 至少声明并验证：

```text
EmptyTrayCodeStopsBeforeDatabase
ValidTrayCodeLoadsCellBarcodes
InvalidCellBarcodeSetStopsBeforeTest
UnclampedTrayStopsBeforeStarTest
PlcReadFailureStopsBeforeStarTest
```

- [ ] **Step 4: 将新增文件加入项目并运行结构测试**

运行：

```powershell
MSBuild.exe .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU
```

预期：编译通过，新增类型可被 `frmTest` 和自动状态机引用。

## 4. 任务二：增加独立 PLC 托盘压合点位

**Files:**

- Modify: `PlcAutomationContracts.vb`
- Modify: `PlcController.vb`
- Modify: `Module1.vb`
- Modify: `frmInit.vb`
- Modify: `frmInit.Designer.vb`

- [ ] **Step 1: 增加配置字段和接口**

在 `PlcAutomationSettings` 和 `Initset` 中增加：

```vb
TrayClampedAddress As Integer = -1
```

在 `IPlcAutomationController` 中增加：

```vb
Function ReadTrayClamped() As Boolean
```

`TrayInPositionAddress` 只表示托盘到位；`TrayClampedAddress` 只表示夹具压合，禁止用一个地址同时承担两个语义。

- [ ] **Step 2: 实现 Modbus 读取**

`ModbusPlcControllerBase.ReadTrayClamped()` 使用已有的 `ReadAutomationCoil()` 和配置地址。读取异常直接抛出，由手动入口显示错误、由自动状态机进入等待或错误状态。

- [ ] **Step 3: 兼容旧配置**

在 `ReadInitFile()` 读取追加字段前先设置：

```vb
Initsetting.TrayClampedAddress = -1
```

旧配置或空配置不得把地址默认为 0。保存配置时把该字段追加到现有配置末尾，避免破坏旧字段顺序。

- [ ] **Step 4: 增加配置 UI**

在 `frmInit` 的 PLC 配置区增加托盘压合地址输入框，并在保存时验证为非负整数。未配置时显示“自动测试不可用”，不能静默使用地址 0。

## 5. 任务三：统一数据库电芯信息加载

**Files:**

- Modify: `MesDataService.vb`
- Modify: `TestStartPreflight.vb`
- Test: `tools/Test-TestStartPreflight.ps1`

- [ ] **Step 1: 保留参数化查询**

查询必须继续使用参数 `@TrayCode`，不得拼接用户输入。返回结果按通道号建立不可变字典。

- [ ] **Step 2: 固定查询后的加载顺序**

先查询并完成全量校验，再清理上一托盘的电芯条码，最后按通道写入：

```vb
For channelNo As Integer = 1 To MAX_CHANNEL
    RecieveData(channelNo - 1).BatCode = String.Empty
Next

For Each item In cellBarcodes
    RecieveData(item.Key - 1).BatCode = item.Value
Next
```

校验失败时不得先写入部分数据。

- [ ] **Step 3: 区分查询失败和数据不完整**

数据库连接失败、查询异常、返回 0 行、通道缺失、重复条码必须分别写入日志，界面显示能够定位原因的提示；所有情况均不得调用 `StarTest`。

## 6. 任务四：修改手动模式开始测试流程

**Files:**

- Modify: `frmTest.vb`
- Modify: `TestStartPreflight.vb`
- Test: `tools/Test-TestStartPreflight.ps1`

- [ ] **Step 1: 将开始按钮改为异步协调入口**

`CmdStart_Click` 保持 WinForms UI 线程入口，但数据库查询使用 `Await`，不能使用 `Task.Run` 直接访问控件。顺序固定为：

```vb
Dim trayCode = txtTray.Text.Trim()
If String.IsNullOrWhiteSpace(trayCode) Then
    MessageBox.Show("请先输入托盘码。", "提示", MessageBoxButtons.OK)
    Return
End If

Dim cellBarcodes = Await LoadTrayCellBarcodesAsync(trayCode)
If cellBarcodes Is Nothing Then Return

Dim preflight = ValidateBeforeTest(trayCode, cellBarcodes, selectedChannels)
If Not preflight.IsAllowed Then
    MessageBox.Show(preflight.Message, "不能开始测试", MessageBoxButtons.OK)
    Return
End If

StarTest(1)
```

具体实现中，`LoadTrayCellBarcodesAsync` 必须在 `StarTest(1)` 前完成；不能把数据库查询放到 `StarTest` 内部，也不能在查询失败后继续执行。

- [ ] **Step 2: 先检查托盘码，再访问数据库**

空 `txtTray` 时不得访问数据库、不得读取电芯数据、不得改变测试状态、不得生成 OCV_DATA 任务。

- [ ] **Step 3: 在调用 `StarTest(1)` 前读取 PLC 压合状态**

手动模式只要读取结果为未压合或 PLC 读取失败，就弹框提示并返回。该路径不得改变 `isRuning` 为测试中，也不得写入 PLC 完成队列。

- [ ] **Step 4: 保留手动通道选择语义**

数据库加载的是托盘完整电芯信息；正式测试仍只测试操作员选择的通道。没有选择通道时提示并停止。

- [ ] **Step 5: 保留手动结果处理**

手动 `StarTest(1)` 成功后执行 OCV_DATA 保存、本地保存和 MES 后台入队；调用 `PersistCompletedResult(snapshot, False)`，明确不发送自动 PLC 完成通知。

## 7. 任务五：修改自动模式使用 `txtTray` 和压合检查

**Files:**

- Modify: `AutoTestWorkflow.vb`
- Modify: `AutoTestWorkflowModels.vb`
- Modify: `frmTest.vb`
- Modify: `PlcAutomationContracts.vb`
- Test: `tools/Test-AutoTestWorkflowScenarios.ps1`

- [ ] **Step 1: 扫码成功后先写入 `txtTray`**

扫码结果不能直接绕过界面控件进入数据库查询。增加托盘码写入适配器，扫码成功后执行：

```vb
trayCodeSink.SetTrayCode(scan.TrayCode)
Dim trayCode = trayCodeSink.GetTrayCode().Trim()
If String.IsNullOrWhiteSpace(trayCode) Then
    Throw New InvalidOperationException("扫码结果写入 txtTray 失败。")
End If
```

实际 WinForms 适配器负责在 UI 线程更新 `txtTray`；状态机随后使用读取到的托盘码创建当前周期上下文。

- [ ] **Step 2: 自动数据库查询必须使用 `txtTray` 的最终值**

禁止继续直接使用扫描器返回值作为唯一数据库参数。扫描器返回值只是写入 `txtTray` 的输入，数据库查询使用写入后重新读取并 Trim 的托盘码。

- [ ] **Step 3: 增加等待压合阶段**

自动状态机在电芯信息加载成功后进入 `WaitingForClamp`，而不是直接调用测试：

```text
LoadingCellBarcodes
    ↓
WaitingForClamp
    ├─ 托盘离开 → 放弃当前周期并回到 WaitingForTray
    ├─ PLC读取失败 → 保持等待并记录错误
    ├─ 未压合 → 保持等待，不调用 StarTest
    └─ 已压合 → Testing → StarTest(1)
```

自动模式不因瞬时未压合弹出阻塞式对话框；界面显示“等待托盘压合”。

- [ ] **Step 4: 测试前再次确认压合状态**

即使 `WaitingForClamp` 已经检测到压合，在真正调用 `StarTest(1)` 前仍调用一次 `ReadTrayClamped()`，防止等待期间夹具状态变化。

- [ ] **Step 5: 记录统一周期上下文**

上下文至少包含最终 `txtTray` 托盘码、1～256 电芯条码、有效测试通道、周期号。正式测试、本地保存、MES 上传和 PLC 通知均使用同一个上下文，禁止在后台重新读取当前窗体数据。

## 8. 任务六：统一结果保存、MES 和 PLC 顺序

**Files:**

- Modify: `frmTest.vb`
- Modify: `ResultDataUploadAssembly.vb`
- Modify: `PlcCompletionWorker.vb`
- Test: `tools/Test-AutoTestWorkflowScenarios.ps1`

- [ ] **Step 1: 固定成功顺序**

正式测试后的顺序必须是：

```text
OCV_DATA 保存成功
  ↓
本地 CSV 保存成功
  ↓
MES 结果入本地 outbox
  ↓
自动模式发送 PLC 完成通知
```

手动模式执行到 MES outbox 后结束，不发送自动 PLC 完成通知。

- [ ] **Step 2: 明确本地保存失败行为**

本地文件采用临时文件写入，关闭成功后再替换正式文件。保存失败时：

- 自动模式不发送 PLC 完成通知；
- 手动模式提示保存失败；
- 不重新执行测量；
- 保留测试任务号，允许人工重试保存。

- [ ] **Step 3: 保持 MES 后台异步**

MES 网络请求不得位于 `StarTest` 或 PLC 完成通知的同步路径中。后台只消费不可变 `TestResultSnapshot`，上传失败使用同一幂等键重试。

- [ ] **Step 4: 保持 PLC 通知幂等**

PLC 完成通知使用测试任务号或周期号生成稳定幂等键。重试时允许重复发送同一完成命令，但不得重新测试；ACK 由 PLC worker 唯一读取。

## 9. 任务七：状态锁、界面提示和复测隔离

**Files:**

- Modify: `frmTest.vb`
- Modify: `AutoTestWorkflow.vb`

- [ ] **Step 1: 自动状态期间锁定手动操作**

在 `Scanning`、`LoadingCellBarcodes`、`WaitingForClamp`、`Testing`、`SavingLocalResult`、`WaitingForPlcAck` 状态禁止开始手动正式测试和 NG 复测。

- [ ] **Step 2: 手动失败不污染自动状态**

手动空托盘码、数据库失败、未压合等情况只结束本次点击操作，不修改自动状态机的当前周期。

- [ ] **Step 3: 自动失败不重复测量**

自动前置检查失败时只进入等待或错误状态；重新尝试必须重新验证压合和当前托盘上下文，不能直接再次调用 `StarTest`。

- [ ] **Step 4: 保持复测独立**

`btnNGTest_Click` 继续调用 `StarTest(0)`，复测仍需通过压合检查，但不重新扫码、不重新创建自动 PLC 完成通知。

## 10. 任务八：测试和验收

**Files:**

- Create/Modify: `tools/Test-TestStartPreflight.ps1`
- Modify: `tools/Test-AutoTestWorkflowScenarios.ps1`

- [ ] **Step 1: 添加手动模式场景**

必须通过以下场景：

```text
ManualEmptyTxtTrayShowsPromptAndDoesNotQuery
ManualValidTxtTrayQueriesBeforeStarTest
ManualIncompleteCellBarcodesStopsTest
ManualUnclampedTrayShowsPromptAndDoesNotStart
ManualPlcReadFailureDoesNotStart
ManualClampedTrayStartsStarTestOnce
```

- [ ] **Step 2: 添加自动模式场景**

必须通过以下场景：

```text
AutomaticScanWritesTxtTrayBeforeDatabaseLookup
AutomaticDatabaseLookupUsesTxtTrayValue
AutomaticUnclampedTrayWaitsWithoutStarting
AutomaticClampBecomesValidThenStartsOnce
AutomaticTrayLeavesWhileWaitingForClampCancelsCycle
AutomaticClampReadFailureDoesNotStart
```

- [ ] **Step 3: 添加结果顺序场景**

验证：

```text
LocalSaveFailureDoesNotNotifyPlc
MesUploadFailureDoesNotBlockPlcNotification
ManualCompletionDoesNotEnqueueAutomaticPlcNotice
PlcAckTimeoutRetriesSameIdempotencyKey
```

- [ ] **Step 4: 运行完整回归**

运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Test-TestStartPreflight.ps1
powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflow.ps1
powershell -ExecutionPolicy Bypass -File .\tools\Test-AutoTestWorkflowScenarios.ps1
powershell -ExecutionPolicy Bypass -File .\tools\Test-MainTestSelectionSnapshot.ps1
powershell -ExecutionPolicy Bypass -File .\tools\Test-MainTestTrayFailureFeedback.ps1
powershell -ExecutionPolicy Bypass -File .\tools\verify-manual-test-protocol-log.ps1
powershell -ExecutionPolicy Bypass -File .\tools\Assert-WinFormsDesignerSerialization.ps1
MSBuild.exe .\MP-60A-RV-64Mem.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /m
git diff --check
```

预期结果：所有脚本 PASS，工程 0 个编译错误，只有已知的第三方 x86/MSIL 架构警告；不存在 `autoStartFlag` 或主工程 `Timer4` 触发路径。

## 11. 现场验收顺序

1. 配置并核对 PLC 四类点位：托盘到位、托盘压合、测试完成命令、完成 ACK。
2. 手动模式输入空 `txtTray`，确认弹框且不访问数据库、不启动测试。
3. 手动模式输入有效托盘码但不压合，确认提示并不启动测试。
4. 手动模式压合后测试，确认电芯信息来自数据库且只测试选中通道。
5. 自动模式扫码，确认扫码结果先显示在 `txtTray`，数据库查询使用该值。
6. 自动模式扫码后不压合，确认界面等待，不调用 `StarTest(1)`。
7. 压合后确认只启动一次正式测试。
8. 拔掉 MES 网络，确认本地保存成功、PLC 仍能收到完成通知，MES 进入重试队列。
9. 模拟 PLC ACK 超时，确认只重发相同任务，不重复测量。
10. 完成后确认 PLC 接管下一步动作，PC 不自行操作下一托盘。

## 12. 实现顺序和提交边界

按以下顺序实施，避免一次性修改主流程导致难以回滚：

1. `TestStartPreflight.vb` 契约和失败场景测试；
2. PLC 压合地址配置、读取接口和配置兼容；
3. 手动 `txtTray` 查询和压合前置检查；
4. 自动扫码写入 `txtTray` 和 `WaitingForClamp` 状态；
5. 结果保存、MES、PLC 顺序回归；
6. UI 锁定、复测隔离和日志；
7. 完整编译、场景测试和现场验收清单。

每一组完成后单独提交，提交信息分别使用：

```text
test: add tray preflight scenarios
feat: add PLC tray clamp signal
feat: validate manual tray code and cell data
feat: gate automatic testing on tray clamp
test: cover manual and automatic start preconditions
```
