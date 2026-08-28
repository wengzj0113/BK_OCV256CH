# PLC/MES 托盘电池条码获取改造 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 XL-OCV-256CH 的托盘/电池条码来源改为 PLC 托盘码寄存器和客户 MES 数据库，并支持自动模式与手动模式。

**Architecture:** 以 `PlcController` 作为 PLC 读取适配器，以独立的 `MesDataService` 访问配置指定的 MES 数据库，以 `TrayBarcodeWorkflow` 集中处理托盘到位、扫码完成、MES 查询、通道校验、去重和自动启动。`frmTest` 只负责 UI 状态、模式选择、通道显示和调用现有 `StarTest`，不直接解析 PLC 或拼接 SQL。

**Tech Stack:** VB.NET 4.8 WinForms、Modbus RTU/TCP、`System.Data.SqlClient`、现有 `Initset.set` 二进制配置、PowerShell 验证脚本。

**Design reference:** `docs/superpowers/specs/2026-08-28-plc-mes-tray-barcode-design.md`

---

## 文件变更总览

### 新建

- `MesDataService.vb`：MES 连接配置、`dbo.batcode` 查询、结果校验和错误上下文。
- `TrayBarcodeWorkflow.vb`：托盘条码获取状态机和自动/手动分流。
- `tools/Test-MesTrayBarcodeFlow.ps1`：无现场设备时的结构和流程回归检查。

### 修改

- `Module1.vb`：`Initset` 增加 MES 配置字段；追加配置文件读写；删除旧扫码公共变量和函数。
- `PlcController.vb`：增加托盘到位、扫码完成/有效、托盘码读取接口；地址和编码从配置对象传入。
- `frmInit.vb`：保存/读取 MES 控件，校验配置，增加 MES 连接测试入口。
- `frmInit.Designer.vb`：增加并布局 `txtMesDatabaseServerIP`、`TxtMesDatabaseName`、`TxtMesBaseUser`、`TxtMesBasePsw` 和连接测试控件。
- `frmTest.vb`：接入工作流，增加自动/手动模式，按 MES 物理通道加载，移除旧自动启动和 `frmInput` 依赖。
- `frmTest.Designer.vb`：在左侧“托盘与电池”区域、“待测试通道数”下方和测试按钮上方增加模式选择控件，并移除旧扫码按钮/首尾条码控件的运行入口。
- `ChannelGridControl.vb`：增加按通道号批量设置选择的方法。
- `MDIForm.vb`：移除旧扫码菜单和 `frmInput` 调用。
- `MP-60A-RV-64Mem.vbproj`：注册新文件并移除不再使用的扫码窗体文件。

### 删除或停用

- `frmInput.vb`、`frmInput.Designer.vb`、`frmInput.resx`：旧扫码、首尾条码生成和 OCV_DATA 托盘明细回写全部移除。
- `bar.set`：不再作为条码缓存或离线兜底。
- `frmTest.vb` 中的 `autoStartFlag`/旧 IO 自动启动链路和 `Timer4` 自动调用 `StarTest(1)`。

---

### Task 1: 建立无设备回归检查和 PLC 接口前置条件

**Files:**
- Create: `tools/Test-MesTrayBarcodeFlow.ps1`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 写入会失败的结构检查**

在 `tools/Test-MesTrayBarcodeFlow.ps1` 中读取项目根目录下的源码，并先断言计划要求的接口、MES 字段、模式控件和旧逻辑移除点存在。脚本至少包含以下断言：

```powershell
$root = Split-Path -Parent $PSScriptRoot
$plc = Get-Content -Raw (Join-Path $root 'PlcController.vb')
$frmTest = Get-Content -Raw (Join-Path $root 'frmTest.vb')
$frmInit = Get-Content -Raw (Join-Path $root 'frmInit.vb')
$grid = Get-Content -Raw (Join-Path $root 'ChannelGridControl.vb')

if ($plc -notmatch 'ReadTrayInPosition') { throw 'PLC tray-in-position interface is missing.' }
if ($plc -notmatch 'ReadTrayCode') { throw 'PLC tray-code interface is missing.' }
if ($frmInit -notmatch 'txtMesDatabaseServerIP|TxtMesDatabaseName|TxtMesBaseUser|TxtMesBasePsw') { throw 'MES database controls are missing.' }
if ($frmTest -notmatch '自动模式|手动模式') { throw 'Test mode controls are missing.' }
if ($grid -notmatch 'SetSelectedChannels') { throw 'Batch channel selection API is missing.' }
if ($frmTest -match 'Timer4_Tick[\s\S]{0,500}StarTest\(1\)') { throw 'Legacy timer auto-start must be removed.' }

Write-Output 'MES_TRAY_BARCODE_FLOW: PASS'
```

- [ ] **Step 2: 运行检查确认当前基线失败**

运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Test-MesTrayBarcodeFlow.ps1
```

预期：FAIL，至少报告 PLC 托盘码接口、MES 配置控件或批量通道选择接口尚不存在。

- [ ] **Step 3: 记录 PLC 地址表作为硬件接入前置条件**

在 `PlcController.vb` 的新接口中使用 `PlcTrayBarcodeSettings` 传入地址、Modbus 区域、寄存器数量、编码和超时，不在 `frmTest` 中出现地址常量。具体硬件实现必须等待客户提供以下值后启用：托盘到位地址、夹具闭合/测试允许地址、托盘码起始地址/数量、扫码完成或数据有效地址、托盘离开地址和编码方式。

若地址表尚未提供，本任务只允许完成接口、模拟适配器和软件验证；生产配置不得使用猜测地址。

- [ ] **Step 4: 运行脚本确认接口检查仍处于失败状态**

运行同一个 PowerShell 命令，预期仍为 FAIL，但失败项应只反映尚未实施的接口或控件。

- [ ] **Step 5: 提交基线测试脚本**

```powershell
git add tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "test: add MES tray barcode flow checks"
```

### Task 2: 增加独立 MES 数据库配置并保持 OCV_DATA 隔离

**Files:**
- Modify: `Module1.vb`
- Modify: `frmInit.vb`
- Modify: `frmInit.Designer.vb`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 增加独立配置字段**

在 `Module1.vb` 的 `Initset` 结构中增加以下字段，字段不复用 `strDatabase`、`strUserID`、`strpwd`：

```vb
Dim MesDatabaseServerIP As String
Dim MesDatabaseName As String
Dim MesBaseUser As String
Dim MesBasePsw As String
```

- [ ] **Step 2: 把四个控件加入 `frmInit` 数据库设置页**

在 `frmInit.Designer.vb` 的 `TabPage3` 增加以下控件和中文标签：

```text
txtMesDatabaseServerIP  MES数据库服务器
TxtMesDatabaseName      MES数据库名称
TxtMesBaseUser          MES数据库用户名
TxtMesBasePsw           MES数据库密码
```

`TxtMesBasePsw.UseSystemPasswordChar = True`。控件放在现有 OCV_DATA 配置项之后，并保证 TabPage3 的高度和滚动区域能完整显示。

- [ ] **Step 3: 实现控件到配置结构的保存和读取**

在 `frmInit.vb` 的保存逻辑中使用以下对应关系：

```vb
TempInitsetting.MesDatabaseServerIP = txtMesDatabaseServerIP.Text.Trim()
TempInitsetting.MesDatabaseName = TxtMesDatabaseName.Text.Trim()
TempInitsetting.MesBaseUser = TxtMesBaseUser.Text.Trim()
TempInitsetting.MesBasePsw = TxtMesBasePsw.Text
```

在 `structToText()` 中反向赋值。MES 数据库名称不使用代码默认值；四项为空时由 MES 连接/查询前的校验报告配置不完整。

- [ ] **Step 4: 追加配置文件字段并兼容旧文件**

在 `SaveInitFile()` 末尾追加四个字符串：

```vb
BW.Write(If(Initsetting.MesDatabaseServerIP, String.Empty))
BW.Write(If(Initsetting.MesDatabaseName, String.Empty))
BW.Write(If(Initsetting.MesBaseUser, String.Empty))
BW.Write(If(Initsetting.MesBasePsw, String.Empty))
```

在 `ReadInitFile()` 读取末尾字段前逐次检查 `FS.Position < FS.Length`，旧配置文件缺少字段时使用 `String.Empty`。不得改变现有字段顺序。

- [ ] **Step 5: 增加配置校验入口**

增加 `MesDatabaseSettings.Validate()` 的调用入口，至少拒绝服务器、数据库名、用户名或密码为空。连接测试按钮及正式 `MesDataService.TestConnection()` 在 Task 3 中实现；Task 2 只负责控件、配置字段和持久化，避免设置页面依赖尚未加入项目的服务文件。

- [ ] **Step 6: 更新结构检查并运行**

为脚本增加以下断言：四个 `Initset` 字段、四个控件名、密码掩码、`SaveInitFile` 和 `ReadInitFile` 均出现，且 `MesDatabaseName` 没有固定数据库常量替代。

运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Test-MesTrayBarcodeFlow.ps1
```

预期：MES 配置相关检查 PASS，PLC/流程相关检查继续失败。

- [ ] **Step 7: 提交配置改造**

```powershell
git add Module1.vb frmInit.vb frmInit.Designer.vb tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "feat: add independent MES database settings"
```

### Task 3: 实现 `MesDataService` 和 `batcode` 数据校验

**Files:**
- Create: `MesDataService.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 定义独立连接配置和查询结果模型**

在 `MesDataService.vb` 定义以下模型和接口：

```vb
Public NotInheritable Class MesDatabaseSettings
    Public Property ServerIP As String
    Public Property DatabaseName As String
    Public Property UserName As String
    Public Property Password As String

    Public Sub Validate()
        If String.IsNullOrWhiteSpace(ServerIP) Then Throw New InvalidOperationException("MES数据库服务器未配置。")
        If String.IsNullOrWhiteSpace(DatabaseName) Then Throw New InvalidOperationException("MES数据库名称未配置。")
        If String.IsNullOrWhiteSpace(UserName) Then Throw New InvalidOperationException("MES数据库用户名未配置。")
        If String.IsNullOrWhiteSpace(Password) Then Throw New InvalidOperationException("MES数据库密码未配置。")
    End Sub
End Class

Public NotInheritable Class MesTrayCellBarcode
    Public Property ChannelNo As Integer
    Public Property CellSn As String
End Class

Public Class MesDataService
    Public Function GetTrayCellBarcodes(trayCode As String) As IReadOnlyList(Of MesTrayCellBarcode)
    Public Sub TestConnection()
End Class
```

`MesDataService` 构造函数接收 `MesDatabaseSettings`，不得在类中读取 OCV_DATA 字段或创建默认 MES 连接信息。

- [ ] **Step 2: 实现参数化查询**

使用 `SqlConnectionStringBuilder`，将 `ServerIP`、`DatabaseName`、`UserName` 和 `Password` 分别写入 `DataSource`、`InitialCatalog`、`UserID` 和 `Password`，设置有限的 `ConnectTimeout` 和 `CommandTimeout`。

查询必须使用以下 SQL 和参数：

```sql
SELECT Tf_ChannelNo, Tf_CellSn
FROM dbo.batcode
WHERE Tf_TrayId = @TrayCode
```

```vb
command.Parameters.Add("@TrayCode", SqlDbType.NVarChar, 100).Value = trayCode.Trim()
```

不拼接托盘号，不执行 INSERT、UPDATE 或 DELETE。

- [ ] **Step 3: 实现严格数据校验**

读取每行后执行以下规则：

```text
Tf_ChannelNo 必须是 1～256 的整数
Tf_CellSn 必须非空
(Tf_TrayId, Tf_ChannelNo) 不得重复
有效记录必须正好覆盖 1～256 全部通道
```

异常必须携带缺失通道、重复通道或非法通道列表，供界面显示。托盘号作为字符串保留首尾空白清理后的原值，不转换为数值。

- [ ] **Step 4: 实现 MES 同步延迟重试**

单次托盘任务内，空结果或暂时查不到记录时按固定的任务设置重试；每次重试重新打开查询连接，不复用已失败的 `SqlDataReader`。最终失败时抛出包含托盘号和重试次数的异常。不得把失败结果当作空托盘继续测试。

- [ ] **Step 5: 注册项目文件并增加静态检查**

将 `MesDataService.vb` 加入 `MP-60A-RV-64Mem.vbproj`。为脚本增加对 `MES` 字段、`dbo.batcode`、`Tf_TrayId`、`Tf_ChannelNo`、`Tf_CellSn`、`@TrayCode` 和 `OcvDataService` 隔离的断言。

在 `frmInit.vb` 增加“测试 MES 数据库连接”按钮事件，事件只构造当前四项配置并调用 `MesDataService.TestConnection()`；成功或失败都在 UI 线程显示结果，绝不执行条码查询写入。

运行脚本，预期 MES 服务检查 PASS。

- [ ] **Step 6: 提交 MES 数据模块**

```powershell
git add MesDataService.vb MP-60A-RV-64Mem.vbproj tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "feat: query tray barcodes from MES database"
```

### Task 4: 扩展 PLC 读取接口并实现扫码完成保护

**Files:**
- Modify: `PlcController.vb`
- Modify: `Module1.vb`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 扩展 `IPlcController` 的小接口**

在现有接口保留夹具动作，并增加：

```vb
Function ReadTrayInPosition() As Boolean
Function ReadTrayScanComplete() As Boolean
Function ReadTrayCode() As String
```

`ReadTrayStatus()` 继续表示夹具状态，不能改名复用为托盘到位。

- [ ] **Step 2: 定义 PLC 托盘条码配置对象**

定义 `PlcTrayBarcodeSettings`，包含托盘到位区域/地址、扫码完成区域/地址、托盘码寄存器起始地址、寄存器数量、编码方式、从站号、轮询间隔、扫码超时和连续一致次数。对象校验未配置地址时抛出“PLC托盘条码地址未配置”，不得使用猜测地址。

生产实例由一个集中工厂从 `Initsetting` 或现场配置对象创建；在 PLC 地址表交付前，工厂不得启用真实托盘条码轮询。这样地址变化只修改一个适配器/配置位置，不会扩散到 `frmTest` 或工作流。

- [ ] **Step 3: 在 RTU/TCP 两个实现中复用同一读取实现**

将 Modbus 区域读写差异集中在 `ModbusPlcControllerBase` 的受保护方法中；RTU 和 TCP 只提供各自的 master。托盘码读取按配置的寄存器数量读回并按编码方式转换为字符串，去除填充空字节但不修改有效字符。

- [ ] **Step 4: 增加模拟 PLC 适配器测试入口**

在脚本或独立测试辅助代码中验证：托盘到位为假不读码、扫码完成为假不接受新码、寄存器空值超时、连续两次/配置次数一致后才接受、托盘离开后允许下一托盘。

- [ ] **Step 5: 更新结构检查并提交**

运行脚本，确认 `ReadTrayInPosition`、`ReadTrayScanComplete`、`ReadTrayCode` 和配置对象存在；提交：

```powershell
git add PlcController.vb Module1.vb tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "feat: add PLC tray barcode read interface"
```

### Task 5: 增加通道批量选择和托盘条码工作流

**Files:**
- Modify: `ChannelGridControl.vb`
- Create: `TrayBarcodeWorkflow.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 为通道控件增加精确批量选择接口**

在 `ChannelGridControl` 增加：

```vb
Public Sub SetSelectedChannels(channelNumbers As IEnumerable(Of Integer))
```

方法先清空选择，再只接受 1～256 的通道号，更新 tile 状态并触发一次 `SelectedChannelsChanged`。不改变 `SelectAll()` 和 `ClearSelection()` 的既有行为。

- [ ] **Step 2: 定义工作流状态和结果**

`TrayBarcodeWorkflow` 至少包含以下状态：`WaitingForTray`、`WaitingForScan`、`ReadingTrayCode`、`QueryingMes`、`Loaded`、`Testing`、`Error`。对外暴露开始/停止轮询、当前状态、当前托盘码、加载通道和错误信息。

- [ ] **Step 3: 实现单任务锁和托盘生命周期**

工作流使用一个进行中锁，保证 PLC 轮询、MES 查询和自动启动不会并发处理多个托盘。成功处理的托盘码在托盘到位信号恢复为假前不重复处理；只有测试结束、结果处理完成且检测到托盘离开后才清除锁。

- [ ] **Step 4: 实现 MES 数据应用结果**

工作流返回 `TrayBarcodeLoadResult`，包含托盘码、256 条物理通道映射、缺失/重复/非法通道和查询耗时。应用到主窗体时执行：

```text
清空 RecieveData 的旧 BatCode
按 channelNo - 1 写入新 CellSn
设置 SelectedChannelNumbers 为 1～256 全部通道
刷新 lblBarNum、通道矩阵和状态栏
```

- [ ] **Step 5: 增加自动/手动分流回调**

工作流在 `Loaded` 后只发布“数据已加载”事件；由 `frmTest` 根据当前模式决定是否调用 `StarTest(1)`。这样工作流不直接依赖窗体，也不会在手动模式意外启动测试。

- [ ] **Step 6: 更新脚本并提交**

运行结构检查，预期通道批量选择和工作流检查 PASS。提交：

```powershell
git add ChannelGridControl.vb TrayBarcodeWorkflow.vb MP-60A-RV-64Mem.vbproj tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "feat: add tray barcode workflow and channel mapping"
```

### Task 6: 接入 `frmTest`，实现两种模式并拆除旧自动启动

**Files:**
- Modify: `frmTest.vb`
- Modify: `frmTest.Designer.vb`
- Modify: `ChannelGridControl.vb`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 增加左侧模式控件**

在图片箭头位置增加“测试模式”容器和两个互斥控件：`rdoAutoMode`、`rdoManualMode`。默认 `rdoManualMode.Checked = True`。测试运行期间禁用模式控件；模式不保存到 `Initset.set`。

- [ ] **Step 2: 初始化时清空通道选择**

替换 `frmTest_Load` 中的 `channelGrid.SelectAll()` 为 `channelGrid.ClearSelection()`，避免没有 MES 条码时默认选中全部 256 通道。

- [ ] **Step 3: 移除 `frmInput` 范围依赖**

删除 `StarTest` 中读取 `frmInput.txtFirst` 和 `frmInput.txtLast` 的逻辑，使用当前 `SelectedChannelNumbers` 计算 `firstCellNumToTest`、`lastCellNumToTest` 和 `totalCellNumToTest`。正式测试统一以实际物理通道列表调用 `RunSparseMeasurement`，不压缩通道号。

将测试入口改为 `Public Function StarTest(ByVal testType As Integer, Optional ByVal isAutomaticStart As Boolean = False)`；自动模式传入 `True`，手动按钮和 NG 复测传入 `False`。OCV 次数确认只在 `isAutomaticStart = False` 时弹出。

- [ ] **Step 4: 接入工作流事件并实现模式分流**

工作流成功加载后更新 `txtTray`、`RecieveData`、`channelGrid`、`lblBarNum` 和日志。自动模式在 `alarmStatus = False`、`isRuning = False`、OCV 阶段有效且加载结果完整时只调用一次 `StarTest(1)`；手动模式不调用 `StarTest`。

- [ ] **Step 5: 移除旧 IO 自动启动冲突**

删除或停用 `autoStartFlag` 的赋值、`AnalyzeData`/`checkJIGSignal` 中旧自动启动位解析和 `Timer4_Tick` 中直接调用 `StarTest(1)` 的逻辑。保留温度、急停和烟感报警处理，但报警处理不得再改变新模式选择。

- [ ] **Step 6: 处理现有交互弹窗**

自动模式调用测试前使用当前 OCV 选择，不弹出 OCV 次数确认框；NG 复测仍由现有人工确认流程处理。手动模式保留现有启动按钮和校验。所有选中通道在启动前重新检查 `BatCode` 非空。

- [ ] **Step 7: 锁定自动模式的通道选择**

自动条码加载成功后禁用通道选择控件；手动模式允许操作员在启动前调整选择，但只能选择已经加载的 256 个有效通道。测试开始后两种模式都禁用选择。

- [ ] **Step 8: 运行结构检查并提交**

运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Test-MesTrayBarcodeFlow.ps1
```

预期：自动/手动模式、旧自动启动删除、通道同步和 `frmInput` 依赖检查 PASS。提交：

```powershell
git add frmTest.vb frmTest.Designer.vb ChannelGridControl.vb tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "feat: add automatic and manual tray test modes"
```

### Task 7: 移除旧扫码入口、缓存和窗体依赖

**Files:**
- Modify: `MDIForm.vb`
- Modify: `MDIForm.Designer.vb`
- Modify: `Module1.vb`
- Delete: `frmInput.vb`
- Delete: `frmInput.Designer.vb`
- Delete: `frmInput.resx`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 删除 MDI 旧扫码入口**

移除 `TStripMenuCode_Click`、`TStripbtn_Barcode_Click` 中对 `frmInput` 的打开、赋值和回写；对应菜单/工具栏按钮保持不可见或从 Designer 中移除，不能再有可达的旧扫码入口。

- [ ] **Step 2: 删除 `frmInput` 的旧逻辑**

移除 `CWtray.exe`、`MpScanGui_ReturnInfo`、`ThreadSaveCode`、`MapReadBarCode`、`SaveBarcode`、`GetBarCodeFile`、`ScanBarCode`、首尾条码自动生成和 `SaveCurrentTrayCellsToOcvData`。客户 MES 数据不得回写到 OCV_DATA 托盘明细。

- [ ] **Step 3: 清理扫码串口代码但保留仪器串口**

确认 `Com_scaner` 只被旧扫码职责使用后，再删除其公共变量、`CheckCom_scaner`、相关 Initset 字段、初始化和冲突校验。不得删除 `Com_3561`、中位机、34461A、IO 卡或 PLC 的通讯代码。

- [ ] **Step 4: 删除工程文件引用和本地缓存入口**

从 `MP-60A-RV-64Mem.vbproj` 删除三个 `frmInput` 文件条目；确认 `bar.set` 不再被读取或写入。用 `rg` 检查剩余引用：

```powershell
rg -n "frmInput|CWtray|MpScanGui_ReturnInfo|ThreadSaveCode|MapReadBarCode|ScanBarCode|CommunicationScan|bar\.set|autoStartFlag" --glob '*.vb' --glob '*.vbproj' .
```

预期：只允许设计文档或历史归档出现，生产源码和项目文件不得出现。

- [ ] **Step 5: 运行脚本并提交**

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Test-MesTrayBarcodeFlow.ps1
git add MDIForm.vb MDIForm.Designer.vb Module1.vb MP-60A-RV-64Mem.vbproj
git rm frmInput.vb frmInput.Designer.vb frmInput.resx
git commit -m "refactor: remove legacy barcode scanning flow"
```

### Task 8: 完成日志、异常恢复和配置安全检查

**Files:**
- Modify: `TrayBarcodeWorkflow.vb`
- Modify: `frmTest.vb`
- Modify: `MesDataService.vb`
- Modify: `frmInit.vb`
- Test: `tools/Test-MesTrayBarcodeFlow.ps1`

- [ ] **Step 1: 统一阶段化错误信息**

所有错误消息必须标识阶段：PLC 托盘到位、PLC 托盘码、MES 连接、MES 查询、通道校验或自动启动。UI 更新使用 `Invoke`/`BeginInvoke` 切回窗体线程，后台线程不得直接访问控件。

- [ ] **Step 2: 增加运行日志字段**

每个托盘至少记录托盘到位时间、托盘码、MES 有效记录数、加载通道数、运行模式、自动启动结果和失败原因。禁止记录 MES 密码和完整连接字符串。

- [ ] **Step 3: 处理失败后的恢复条件**

PLC/MES 失败时不清除仍在测试中的有效数据，不启动测试；失败任务释放当前任务锁后等待托盘离开或新托盘边沿。同一个托盘不能通过轮询重复启动。MES 暂时无数据只在任务内重试，不能无限重试。

- [ ] **Step 4: 检查配置文件兼容和密码显示**

验证旧 `Initset.set` 可启动，MES 配置为空时明确提示；验证 `TxtMesBasePsw` 显示为掩码。沿用现有配置文件保存机制时，不在源码中出现任何客户实际 IP、数据库名、账号或密码。

- [ ] **Step 5: 提交异常处理改造**

```powershell
git add TrayBarcodeWorkflow.vb frmTest.vb MesDataService.vb frmInit.vb tools/Test-MesTrayBarcodeFlow.ps1
git commit -m "feat: harden tray barcode recovery and diagnostics"
```

### Task 9: 构建、静态验证和现场联调准备

**Files:**
- Modify: `tools/Test-MesTrayBarcodeFlow.ps1`
- Verify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: 运行静态流程检查**

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Test-MesTrayBarcodeFlow.ps1
```

预期输出：

```text
MES_TRAY_BARCODE_FLOW: PASS
```

- [ ] **Step 2: 编译项目**

使用现有 Visual Studio/MSBuild 环境执行：

```powershell
msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform="AnyCPU"
```

预期：退出码 0，无 `frmInput`、MES 配置字段、PLC 接口或 Designer 控件相关编译错误。

- [ ] **Step 3: 运行无设备软件验证**

验证以下场景：

```text
旧 Initset.set → 能启动但 MES 加载前提示未配置
手动模式 + 模拟完整 256 条 MES 数据 → 只加载，不启动
自动模式 + 模拟完整 256 条 MES 数据 → 只启动一次
少于 256 条 → 两种模式都禁止启动并显示缺失通道
重复/非法/空条码 → 禁止启动并显示具体原因
PLC 到位但扫码未完成 → 不查询 MES
同一托盘持续到位 → 不重复查询/启动
托盘离开后新托盘到位 → 可以处理新托盘
旧 IO 自动启动位变化 → 不改变新模式行为
```

- [ ] **Step 4: 现场联调前填入 PLC 参数**

取得 PLC 地址表后，只在 `PlcTrayBarcodeSettings` 的生产配置中填写托盘到位、夹具闭合/测试允许、托盘码、扫码完成/有效和托盘离开定义；同步验证 Modbus 区域、从站号、寄存器顺序、字节序和字符串编码。未取得地址表时不进行真实设备宣称验收。

- [ ] **Step 5: 完成最终回归检查**

检查 OCV_DATA 仍只由 `OcvDataService` 使用，测试结果保存和 MES 结果上传未被条码改造破坏；确认工作区中只包含本次批准的文档或实施变更后，再提交最终验证记录。
