# Middle Machine G1/G2 Calibration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将中位机通讯和校准通道计划升级为串口 RS485、G1/G2 双组、连续/零星列表兼容的稳定实现，并保证仪表缓存数据按本次计划顺序回填到正确通道。

**Architecture:** 保留现有 VB.NET WinForms 结构，在 `SerialMiddleMachineClient.vb` 中封装协议帧、状态复核和 G1/G2 计划生成，在 `ChannelCalibrationRunner.vb` 中只保留“选择通道 -> 生成计划 -> 读缓存 -> 回填校准值”的业务流程。`frmInit` 负责保存中位机串口参数，`Module1.vb` 负责连接初始化和 3562/34461 缓存读取工具函数。

**Tech Stack:** VB.NET WinForms, .NET Framework 4.8, `System.IO.Ports.SerialPort`, CRC16-Modbus, 3562/34461 仪表缓存读取, Visual Studio MSBuild.

---

## 当前结论

- 中位机通讯从 TCP 改为 RS485 串口，参数为 115200 bps, 8N1。
- 串口参数由 `frmInit` 设置并保存，不能在协议客户端里固定默认 `COM1` 作为正式行为。
- 1-256 物理通道分为两组：G1 为 CH1-CH128，G2 为 CH129-CH256。
- G2 下发给中位机时使用本地通道号，转换公式为 `G2LocalChannel = PhysicalChannel - 128`。
- 中位机只负责通道计划和 START，实测值仍由上位机调用现有仪表读取函数读取缓存。
- 3562 和 34461 在测试过程中缓存全部测试值，读取时一次性读出本次测试值，并按本次中位机执行顺序填入对应通道。
- 零星通道最多可选 256 个；给中位机时仍逐项写寄存器，不应在上位机限制为 16 个。
- `0x06` 写命令回显只代表收到，应在关键配置后读取复核寄存器判断配置是否写入正确。
- 夹具下压、夹具松开、夹具状态后续通过 PLC 实现，本阶段不实现。
- 选择 ACIR/OCV 功能已取消，本阶段不再发送对应指令。
- 流程 D 是手动排查第二组内阻接线使用，不纳入正式自动校准主流程。

## 文件范围

- Modify: `Module1.vb` - 增加/恢复中位机串口配置字段的持久化，调整主程序中位机连接入口从 TCP 到串口，保留仪表缓存读取函数。
- Modify: `frmInit.vb` - 保存和加载中位机串口号、波特率；将中位机通讯配置从 IP/端口调整为串口参数。
- Modify: `frmInit.Designer.vb` - 增加或复用中位机串口选择控件，显示端口和波特率。
- Modify: `MDIForm.vb` - 主窗体启动时按 `frmInit` 配置初始化 `Com_SCM`，停止主动 TCP 连接中位机。
- Modify: `SerialMiddleMachineClient.vb` - 实现 G1/G2 双组计划、连续计划、列表计划、最多 256 零星通道、配置复核、状态轮询。
- Modify: `ChannelCalibrationRunner.vb` - 将物理通道选择转换为 G1/G2 执行计划，并按计划顺序映射缓存数据。
- Modify: `FrmChannelCalibration.vb` - 优化选择确认文案，显示跨组/连续/零星计划摘要，保留完成后自动保存校准值。
- Optional Test: `Tests/DeviceCommunicationTestRunner.vb` - 如果当前测试项目可用，补充 CRC、帧生成和计划拆分的无硬件测试。
- Verify: `MP-60A-RV-64Mem.vbproj` - 确保新增或修改文件仍被项目包含。

---

## Task 1: frmInit 保存中位机串口参数

**Files:**
- Modify: `Module1.vb`
- Modify: `frmInit.vb`
- Modify: `frmInit.Designer.vb`

- [ ] **Step 1: 在初始化结构中增加中位机串口字段**

在 `Module1.vb` 的 `Initsetting` 结构中增加字段：

```vb
Dim SCMComPort As Integer
Dim SCMComRate As Integer
```

默认含义：

```text
SCMComPort = ComboBox.SelectedIndex, COM 号 = SCMComPort + 1
SCMComRate = 115200
```

- [ ] **Step 2: 修改 `SaveInitFile()` 持久化字段**

在写入 `SCMIpAdd` 和 `SCM_port` 后面追加：

```vb
BW.Write(Initsetting.SCMComPort)
BW.Write(Initsetting.SCMComRate)
```

兼容注意：如果现场已有旧 `Initset.set`，读取时可能没有这两个字段。读取逻辑必须按 `FS.Position < FS.Length` 判断后再读，不能直接读到文件末尾外。

- [ ] **Step 3: 修改 `ReadInitFile()` 兼容旧配置**

在读取 `SCMIpAdd` 和 `SCM_port` 后面增加：

```vb
If FS.Position < FS.Length Then
    Initsetting.SCMComPort = BR.ReadInt32()
Else
    Initsetting.SCMComPort = 0
End If

If FS.Position < FS.Length Then
    Initsetting.SCMComRate = BR.ReadInt32()
Else
    Initsetting.SCMComRate = 115200
End If
```

- [ ] **Step 4: 在 `frmInit` 中增加中位机串口控件**

如果 `frmInit.Designer.vb` 已有可复用的中位机串口控件，则复用；否则在中位机通讯区域增加：

```text
ComboBox: cmb_SCMPort
ComboBox: cmb_SCMRate
Label: 中位机串口
Label: 波特率
```

`cmb_SCMPort` 填充 COM1-COM64。`cmb_SCMRate` 至少包含 `115200`，默认选中 `115200`。

- [ ] **Step 5: 保存界面值**

在 `frmInit.vb` 当前保存参数的位置增加：

```vb
TempInitsetting.SCMComPort = cmb_SCMPort.SelectedIndex
TempInitsetting.SCMComRate = CInt(cmb_SCMRate.Items.Item(cmb_SCMRate.SelectedIndex))
```

- [ ] **Step 6: 加载界面值**

在 `frmInit.vb` 当前加载参数的位置增加：

```vb
cmb_SCMPort.SelectedIndex = Math.Max(0, Initsetting.SCMComPort)
For intLoop = 0 To cmb_SCMRate.Items.Count - 1
    If CInt(cmb_SCMRate.Items.Item(intLoop)) = Initsetting.SCMComRate Then
        cmb_SCMRate.SelectedIndex = intLoop
        Exit For
    End If
Next
If cmb_SCMRate.SelectedIndex < 0 Then
    cmb_SCMRate.SelectedIndex = cmb_SCMRate.Items.IndexOf(115200)
End If
```

---

## Task 2: 主程序中位机连接改为串口

**Files:**
- Modify: `Module1.vb`
- Modify: `MDIForm.vb`
- Modify: `SerialMiddleMachineClient.vb`

- [ ] **Step 1: 恢复并改造 `CheckCom_SCM()`**

在 `Module1.vb` 中实现中位机串口连接：

```vb
Public Function CheckCom_SCM() As Short
    Try
        With frmMDI.Com_SCM
            If .IsOpen Then
                .Close()
            End If
            .PortName = "COM" & (Initsetting.SCMComPort + 1).ToString()
            .BaudRate = If(Initsetting.SCMComRate > 0, Initsetting.SCMComRate, 115200)
            .Parity = Parity.None
            .DataBits = 8
            .StopBits = StopBits.One
            .ReadTimeout = 1000
            .WriteTimeout = 1000
            .Open()
        End With
        Return 1
    Catch ex As Exception
        MessageBox.Show(ex.Message)
        Return -1
    End Try
End Function
```

- [ ] **Step 2: 主初始化入口调用串口连接**

在 `MainwindowsShow()` 或当前设备连接入口中，将：

```vb
reflag_SCM = CheckTcp_SCM()
```

改为：

```vb
reflag_SCM = CheckCom_SCM()
```

保留 `CheckTcp_SCM()` 函数但不作为默认连接路径，避免影响旧代码引用。

- [ ] **Step 3: `MDIForm` 启动时停止主动 TCP 连接**

检查 `MDIForm.vb` 中启动时创建 `SocketCtrolCmd` 的逻辑。中位机已改为串口后，默认启动路径不应再执行：

```vb
mSock = New SocketCtrolCmd(...)
```

处理方式：用明确条件隔离旧 TCP 调试路径，正式路径只打开 `Com_SCM`。

- [ ] **Step 4: `SerialMiddleMachineClient.ConfigurePort()` 不覆盖已保存端口**

将 `SerialMiddleMachineClient.vb` 中的端口默认行为调整为：

```vb
If String.IsNullOrWhiteSpace(serialPort.PortName) Then
    serialPort.PortName = "COM" & (Initsetting.SCMComPort + 1).ToString()
End If
serialPort.BaudRate = If(Initsetting.SCMComRate > 0, Initsetting.SCMComRate, 115200)
```

固定保留：

```vb
serialPort.Parity = Parity.None
serialPort.DataBits = 8
serialPort.StopBits = StopBits.One
```

---

## Task 3: 中位机协议客户端支持 G1/G2 双组计划

**Files:**
- Modify: `SerialMiddleMachineClient.vb`

- [ ] **Step 1: 增加寄存器常量**

在 `SerialMiddleMachineClient` 中集中定义：

```vb
Private Const RegStartStop As UShort = &H30
Private Const RegG1Start As UShort = &H31
Private Const RegG1End As UShort = &H32
Private Const RegTwoPass As UShort = &H35
Private Const RegSpeed As UShort = &H38
Private Const RegG1PlanMode As UShort = &H60
Private Const RegG1ListCount As UShort = &H61
Private Const RegG1ListClear As UShort = &H62
Private Const RegSyncMode As UShort = &H63
Private Const RegG2PlanMode As UShort = &H64
Private Const RegG2ListCount As UShort = &H65
Private Const RegG2ListClear As UShort = &H66
Private Const RegG2Start As UShort = &H67
Private Const RegG2End As UShort = &H68
Private Const RegGroupEnable As UShort = &H6A
Private Const RegMeasureItems As UShort = &H6B
Private Const RegG1ListBase As UShort = &H70
Private Const RegG2ListBase As UShort = &H100
```

- [ ] **Step 2: 去掉零星列表 16 个限制**

将当前 `ConfigureChannelList` 中：

```vb
If channelNumbers.Count > 16 Then
    Throw New InvalidOperationException("Channel list supports at most 16 channels.")
End If
```

改为：

```vb
If channelNumbers.Count > CalibrationService.ChannelCount Then
    Throw New InvalidOperationException("Channel list supports at most 256 channels.")
End If
```

- [ ] **Step 3: 拆分 G1/G2 列表配置函数**

新增：

```vb
Public Sub ConfigureG1List(localChannels As IList(Of Integer))
    ConfigureGroupList(localChannels, RegG1ListClear, RegG1PlanMode, RegG1ListCount, RegG1ListBase)
End Sub

Public Sub ConfigureG2List(localChannels As IList(Of Integer))
    ConfigureGroupList(localChannels, RegG2ListClear, RegG2PlanMode, RegG2ListCount, RegG2ListBase)
End Sub
```

其中 `ConfigureGroupList` 逐项写入：

```vb
WriteRegister(clearRegister, &H1)
For i As Integer = 0 To localChannels.Count - 1
    ValidateLocalChannel(localChannels(i))
    WriteRegister(CUShort(listBase + i), CUShort(localChannels(i)))
Next
WriteRegister(countRegister, CUShort(localChannels.Count))
WriteRegister(planModeRegister, &H1)
```

顺序按用户资料：先清空、逐项写入、写数量、再切列表模式。

- [ ] **Step 4: 增加 G1/G2 连续配置函数**

新增：

```vb
Public Sub ConfigureG1Range(startLocalChannel As Integer, endLocalChannel As Integer)
    WriteRegister(RegG1PlanMode, &H0)
    WriteRegister(RegG1Start, CUShort(startLocalChannel))
    WriteRegister(RegG1End, CUShort(endLocalChannel))
End Sub

Public Sub ConfigureG2Range(startLocalChannel As Integer, endLocalChannel As Integer)
    WriteRegister(RegG2PlanMode, &H0)
    WriteRegister(RegG2Start, CUShort(startLocalChannel))
    WriteRegister(RegG2End, CUShort(endLocalChannel))
End Sub
```

- [ ] **Step 5: 增加统一计划入口**

新增 `ConfigurePlan(plan As MiddleMachineChannelPlan, stableSpeed As Boolean)`。规则：

```text
仅 G1:
  STOP
  同步模式 0x0063=0
  配置 G1 连续或 G1 列表
  两遍 0x0035=0
  四项全测 0x006B=0x000F
  速度 0x0038=1 或 0

仅 G2:
  STOP
  独立模式 0x0063=1
  两组使能 0x006A=0x0002
  配置 G2 连续或 G2 列表
  两遍、四项全测、速度

G1 + G2:
  STOP
  独立模式 0x0063=1
  两组使能 0x006A=0x0003
  分别配置 G1/G2 连续或列表
  两遍、四项全测、速度
```

如果中位机要求“仅 G2”也必须 `0x006A=3`，实机联调时改为 `3`，计划代码应把组使能值集中在一个函数中，便于调整。

- [ ] **Step 6: 配置后读取复核寄存器**

关键配置完成后读取：

```vb
Dim basic As UShort() = ReadRegisters(&H31, 8)
Dim advanced As UShort() = ReadRegisters(&H60, 13)
```

复核连续计划时至少检查：

```text
G1: 0x0060, 0x0031, 0x0032
G2: 0x0064, 0x0067, 0x0068
同步/独立: 0x0063
两组使能: 0x006A
测量项: 0x006B
两遍: 0x0035
速度: 0x0038
```

列表计划复核时至少检查：

```text
G1: 0x0060=1, 0x0061=N
G2: 0x0064=1, 0x0065=N
```

列表项很多时不必每项都读回；优先读数量、模式、首项、末项，现场如发现中位机支持按块读 `0x0070` 和 `0x0100`，再扩展逐项复核。

---

## Task 4: 物理通道转换为执行计划

**Files:**
- Modify: `ChannelCalibrationRunner.vb`
- Modify: `SerialMiddleMachineClient.vb`

- [ ] **Step 1: 增加计划数据结构**

在 `SerialMiddleMachineClient.vb` 或独立新文件中定义：

```vb
Public Class MiddleMachineChannelPlan
    Public Property G1PhysicalChannels As List(Of Integer)
    Public Property G2PhysicalChannels As List(Of Integer)
    Public Property ExecutionOrder As List(Of Integer)
End Class
```

`ExecutionOrder` 保存物理通道号，后续仪表缓存第 N 组数据就写回 `ExecutionOrder(N - 1)`。

- [ ] **Step 2: 从选中物理通道生成计划**

在 `ChannelCalibrationRunner.vb` 中新增：

```vb
Private Function BuildChannelPlan(channelIndexes0 As IList(Of Integer)) As MiddleMachineChannelPlan
    Dim sortedPhysicalChannels As New List(Of Integer)()
    For Each channelIndex0 As Integer In channelIndexes0
        sortedPhysicalChannels.Add(channelIndex0 + 1)
    Next
    sortedPhysicalChannels.Sort()

    Dim plan As New MiddleMachineChannelPlan With {
        .G1PhysicalChannels = New List(Of Integer)(),
        .G2PhysicalChannels = New List(Of Integer)(),
        .ExecutionOrder = New List(Of Integer)(sortedPhysicalChannels)
    }

    For Each physicalChannel As Integer In sortedPhysicalChannels
        If physicalChannel <= 128 Then
            plan.G1PhysicalChannels.Add(physicalChannel)
        Else
            plan.G2PhysicalChannels.Add(physicalChannel)
        End If
    Next

    Return plan
End Function
```

- [ ] **Step 3: G2 下发前转换成本地通道**

新增：

```vb
Private Shared Function ToG2LocalChannels(physicalChannels As IList(Of Integer)) As List(Of Integer)
    Dim result As New List(Of Integer)()
    For Each physicalChannel As Integer In physicalChannels
        result.Add(physicalChannel - 128)
    Next
    Return result
End Function
```

示例：

```text
物理 CH151 -> G2 本地 CH23
物理 CH152 -> G2 本地 CH24
```

- [ ] **Step 4: 连续判断按组内通道判断**

G1 用物理通道判断连续即可。G2 必须先转换成本地通道再判断连续。

```vb
Private Shared Function IsContinuous(channelNumbers As IList(Of Integer)) As Boolean
    If channelNumbers.Count <= 1 Then Return True
    For i As Integer = 1 To channelNumbers.Count - 1
        If channelNumbers(i) <> channelNumbers(i - 1) + 1 Then
            Return False
        End If
    Next
    Return True
End Function
```

---

## Task 5: 校准批量流程按执行顺序回填缓存数据

**Files:**
- Modify: `ChannelCalibrationRunner.vb`
- Modify: `FrmChannelCalibration.vb`
- Modify: `Module1.vb`

- [ ] **Step 1: `CalibrateChannels` 使用统一计划入口**

将当前逻辑：

```vb
If IsContinuous(sortedIndexes) Then
    client.RunSingleRange(...)
Else
    client.RunChannelList(...)
End If
```

替换为：

```vb
Dim plan As MiddleMachineChannelPlan = BuildChannelPlan(channelIndexes0)
client.RunPlan(plan, MiddleMachineTimeoutMs)
```

- [ ] **Step 2: 读取缓存数量等于执行通道数**

保持：

```vb
Dim readFlag As Integer = Trans_ReadCachedData_3562(plan.ExecutionOrder.Count, voltages, resistances)
```

如果后续校准需要同时用 34461，则并行增加：

```vb
Dim dmmVoltages As List(Of Single) = Nothing
Dim readDmmFlag As Integer = Trans_ReadCachedData_34461(plan.ExecutionOrder.Count, dmmVoltages)
```

本阶段校准内阻仍以 3562 电阻值计算。

- [ ] **Step 3: 按 `ExecutionOrder` 回填**

回填代码必须按计划执行顺序：

```vb
For i As Integer = 0 To plan.ExecutionOrder.Count - 1
    Dim physicalChannel As Integer = plan.ExecutionOrder(i)
    Dim channelIndex0 As Integer = physicalChannel - 1
    Dim measuredResistance As Double = resistances(i)
    Dim compensation As Double = service.CalculateCompensation(standardResistance, measuredResistance)
    service.SetGlobalValue(channelIndex0, compensation)
Next
```

不能按 1-256 全局通道位置去取缓存值。

- [ ] **Step 4: 界面提示执行计划摘要**

在 `FrmChannelCalibration.vb` 的批量确认框中显示：

```text
将校准 N 个通道
G1: CH...
G2: CH...，下发为本地 CH...
模式: 连续/零星列表/双组独立
完成后自动保存校准值
```

---

## Task 6: 状态等待和错误处理闭环

**Files:**
- Modify: `SerialMiddleMachineClient.vb`
- Modify: `FrmChannelCalibration.vb`

- [ ] **Step 1: START 后等待完成事件**

保留当前完成事件判断：

```text
AA 41 00 40 00 06 A5 C8 -> 完成
AA 41 00 40 00 64 24 21 -> 异常
```

`WaitForCompletion()` 遇到 `0x0064` 必须抛错，中止本次校准，不保存新的校准值。

- [ ] **Step 2: 支持轮询基础状态**

在等待事件期间可增加轮询：

```vb
Dim status As UShort() = ReadRegisters(&H40, 7)
```

轮询失败不能覆盖主动事件成功；如果串口读冲突明显，先保留事件等待，实机再决定是否打开轮询。

- [ ] **Step 3: 停止按钮发送 STOP**

当前停止按钮只设置 `stopRequested`。下一步应让它调用：

```vb
runner.StopCurrentCalibration()
```

由 Runner 调用：

```vb
GetMiddleMachineClient().StopTest()
```

停止后界面显示“已发送停止指令，等待当前串口操作返回”。

---

## Task 7: 验证计划

**Files:**
- Verify: `SerialMiddleMachineClient.vb`
- Verify: `ChannelCalibrationRunner.vb`
- Verify: `FrmChannelCalibration.vb`
- Verify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: CRC 和帧生成离线验证**

验证以下帧必须一致：

```text
Write 0x0030=0 -> AA 06 00 30 00 00 90 1E
Write 0x0030=1 -> AA 06 00 30 00 01 51 DE
Write 0x0063=0 -> AA 06 00 63 00 00 60 0F
Write 0x0063=1 -> AA 06 00 63 00 01 A1 CF
Write 0x0060=1 -> AA 06 00 60 00 01 51 CF
Write 0x0100=23 -> AA 06 01 00 00 17 D1 E3
```

- [ ] **Step 2: 计划拆分离线验证**

输入和期望：

```text
CH1-CH128 -> 同步模式，G1 连续 CH1-128
CH4-CH8 -> 同步模式，G1 连续 CH4-8
CH151-CH152 -> 独立模式，仅 G2，本地 CH23-24
CH5,CH6,CH77,CH151,CH152 -> 独立模式，G1 列表 5,6,77；G2 列表 23,24
CH3,CH8,CH25,CH129 -> 独立模式，G1 列表 3,8,25；G2 列表 1
```

- [ ] **Step 3: 编译验证**

运行：

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'MP-60A-RV-64Mem.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86
```

期望：

```text
CoreCompile 无错误。
如果 CopyFilesToOutputDirectory 失败，确认是否为 bin\Debug\MP-60A-RV-128.exe 被 Visual Studio 或正在运行的程序占用。
```

- [ ] **Step 4: 实机流程 B 验证**

选择 CH4-CH8，观察档，两遍。期望：

```text
中位机完成事件 0x0006。
每台仪表得到 10 条数据。
上位机按 CH4,CH5,CH6,CH7,CH8 的执行顺序回填。
```

- [ ] **Step 5: 实机流程 E 验证**

选择 G1 CH5,CH6,CH77 和 G2 物理 CH151,CH152。期望：

```text
G1 下发 5,6,77。
G2 下发本地 23,24。
完成事件 0x0006。
缓存值按执行顺序写回物理 CH5,CH6,CH77,CH151,CH152。
```

---

## 风险和处理

- `Initset.set` 是二进制顺序读写文件，新增字段必须兼容旧文件，否则旧现场配置会读取失败。
- G2 仅单组执行时，`0x006A` 使用 `0x0002` 还是 `0x0003` 需要实机确认；实现时集中封装，便于一处调整。
- 两组列表长度不同，仪表缓存顺序必须以中位机实际触发顺序为准；若实机发现两组交错触发顺序不是简单 G1 后 G2，需要结合 `0x004A/0x004B/0x0054/0x0055` 状态寄存器记录实际触发顺序。
- 3562 缓存解析当前假设每组为 `Resistance, Voltage`；若实机返回格式不同，需要按真实返回格式修正 `Trans_ReadCachedData_3562()`。
- 当前停止按钮如果不直接发送 STOP，只能在阻塞调用返回后停止界面流程；应在 Task 6 修正。

## 不纳入本轮

- PLC 夹具下压、松开、状态。
- 手动排查流程 D 的专用按钮和自动化流程。
- ACIR/OCV 选择指令。
- MES/数据库逻辑重构。

---

## 2026-07-31 Execution Status

Code implementation completed:

- `frmInit` now saves middle-machine serial port and baud rate settings.
- Main startup uses serial `CheckCom_SCM()` for the middle machine instead of the TCP default path.
- `SerialMiddleMachineClient` supports RS485 frames, G1/G2 grouping, range plans, list plans, 256 selected channels, write echo checking, configuration verification, START/STOP, and event completion/error handling.
- `ChannelCalibrationRunner` converts physical channels into G1/G2 plans and maps cached 3562 values back by execution order.
- `FrmChannelCalibration` shows a plan summary before batch calibration and sends STOP on cancel.
- Middle-machine TX/RX, register verification, event frames, and 3562 cache reads are logged to `logFile/MiddleMachine_yyyyMMdd.log`.
- Missing MES alarm provider properties on `frmTest` were restored for build compatibility.
- Duplicate UTF-8 BOM bytes were removed from legacy files that blocked compilation.

Verification completed:

- Visual Studio MSBuild command succeeded:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'MP-60A-RV-64Mem.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86
```

- Result: build succeeded with 17 existing warnings and 0 errors.

Still requires real-machine validation:

- Run CH4-CH8 range plan and confirm each instrument receives the expected cached data count.
- Run mixed sparse plan such as CH3, CH8, CH25, CH129 and confirm G2 local channel conversion.
- Run G2 physical CH151-CH152 and confirm it is sent as local CH23-CH24.
- Confirm actual 3562/34461 raw cache output order matches the execution-order mapping.
