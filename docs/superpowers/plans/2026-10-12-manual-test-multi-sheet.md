# 手动多次测试工作簿 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** 将一次手动多次测试的各轮结果保存到独立 Excel 工作表，并生成按通道统计电压、内阻范围的汇总表。

**Architecture:** 新建 `ManualTestWorkbookWriter`，封装 Excel Interop 生命周期、逐轮工作表、汇总和每轮保存。`FrmManualTest` 继续管理前置检查、测量、状态和错误提示，改为将每轮的类型化快照交给 writer；不改变单次测量和手动 CSV 导出。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.8、项目现有 `Microsoft.Office.Interop.Excel`、`FormalTestRunner`。

---

## 文件与职责

- Create `ManualTestWorkbookWriter.vb`：定义单轮数据记录类型及 Excel 工作簿写入器；保存 `testData/ManualTest_Multi_yyyyMMdd_HHmmss.xlsx`，维护 `汇总` 和 `yyyyMMdd第N次` 工作表。
- Modify `MP-60A-RV-64Mem.vbproj`：在旧式项目的 `<Compile>` 列表登记新 VB 文件；保留当前工作区已有的 `FrmManualTest.resx` 登记。
- Modify `FrmManualTest.vb:283-430`：将多轮 CSV 写入替换为 workbook writer，改单轮快照为类型化数值记录，保留测量循环和结果网格行为。

## Task 1：新增工作簿写入器

**Files:**
- Create: `ManualTestWorkbookWriter.vb`
- Modify: `MP-60A-RV-64Mem.vbproj:409-415`

- [x] 定义单轮快照，使统计使用数值类型而不是格式化后的文本：

```vb
Friend NotInheritable Class ManualTestWorkbookRow
    Public Property ChannelNumber As Integer
    Public Property GroupText As String
    Public Property VoltageMillivolts As Double
    Public Property ResistanceMilliohms As Double
    Public Property ResultText As String
End Class
```

- [x] 新建 `ManualTestWorkbookWriter(outputPath As String, batchDate As DateTime)`，创建单工作表 workbook，将初始工作表命名为“汇总”，把 Excel 设为不可见并关闭提示框。`AddRound(roundNumber As Integer, capturedAt As DateTime, rows As IList(Of ManualTestWorkbookRow))` 应创建名称为 `batchDate.ToString("yyyyMMdd", InvariantCulture) & "第" & roundNumber.ToString(InvariantCulture) & "次"` 的单轮工作表。
- [x] writer 用私有 `List(Of ManualTestWorkbookRow)` 保存已成功写入的轮次快照；每次 `AddRound` 用“既有快照 + 本轮快照”生成临时候选数据，写入轮次工作表和完整汇总并保存 workbook，保存成功后才将候选数据设为已完成快照。
- [x] 单轮工作表用二维数组一次写入表头和数据列：`通道、分组、采集时间、电压 mV、内阻 mΩ、判定`。用 `NumberFormat = "0.0000"` 显示测量值，数据仍写数值而非字符串。
- [x] 每次 `AddRound` 后，按 `ChannelNumber` 汇集此前成功轮次，重写“汇总”表：`通道、分组、统计轮数、电压最小值 mV、电压最大值 mV、电压偏离度 mV、内阻最小值 mΩ、内阻最大值 mΩ、内阻偏离度 mΩ`。两种偏离度均为最大值减最小值。
- [x] 第一次 `AddRound` 后用 `Workbook.SaveAs(..., FileFormat:=Excel.XlFileFormat.xlOpenXMLWorkbook)` 建立文件；后续轮次用 `Workbook.Save()`。每轮单独保存成功后 `AddRound` 才返回。
- [x] 实现 `IDisposable`：关闭 workbook、退出 Excel，并释放 worksheet、workbook、worksheets 和 application 等 COM 对象。不要在 `Dispose` 中再次保存；保存必须发生在每次成功的 `AddRound` 中。
- [x] 在 `MP-60A-RV-64Mem.vbproj` 的 `FrmManualTest.vb` 编译项附近增加 `<Compile Include="ManualTestWorkbookWriter.vb" />`，不改动其他现有项目项。

## Task 2：接入手动多次测试

**Files:**
- Modify: `FrmManualTest.vb:283-430`

- [x] 将输出扩展名从 `.csv` 改为 `.xlsx`，路径仍在应用目录 `testData`，文件名仍含毫秒时间戳以避免覆盖。
- [x] 将 `SnapshotManualTestRows` 改为返回 `List(Of ManualTestWorkbookRow)`；保留当前通道排序、设备通道映射、分组判断和 OK/NG 判定。电压仍使用 `RecieveData(index0).Voltage * 1000.0F`，内阻使用 `RecieveData(index0).Resistance`，两个数值以 `Double` 交给 writer。
- [x] 在多轮循环中保留 `FormalTestRunner.RunSparseMeasurement(...)`。每轮测量成功后，采集一个统一时间戳，快照，再按下列顺序保存和更新界面：

```vb
Dim capturedAt As DateTime = DateTime.Now
Dim roundRows As List(Of ManualTestWorkbookRow) = SnapshotManualTestRows(channels)
workbookWriter.AddRound(currentRound, capturedAt, roundRows)
completedRounds = currentRound
DisplayManualTestResult(channels)
```

- [x] 在 `Finally` 路径释放 writer 并恢复控件；保留“停止于第一个失败轮次”的行为。仅在 `completedRounds > 0` 时自动打开 XLSX；成功、部分失败的提示继续包含输出路径和轮数。
- [x] 删除仅服务于多轮 CSV 的 CSV 表头、逐行写入和快照字符串序列化；保留 `ExportManualTestResults` 的 CSV 导出流程。

## Task 3：检查实现并构建

**Files:**
- Inspect: `ManualTestWorkbookWriter.vb`
- Inspect: `FrmManualTest.vb`
- Inspect: `MP-60A-RV-64Mem.vbproj`

- [x] 静态核对：10 轮可得到 10 张单轮表和 1 张“汇总”表；统计包含所有成功完成轮次；每轮保存后再运行下一轮；失败不继续；COM 清理和窗体恢复覆盖成功与异常路径。
- [x] 运行 `MSBuild.exe MP-60A-RV-64Mem.vbproj /p:Configuration=Debug /p:Platform=x86`，确认编译成功。
- [x] 不添加或运行测试套件；用户没有提出测试请求。本计划只执行静态检查和编译检查。
- [x] 检查最终 diff，确认没有覆盖或提交工作区中本任务开始前已有的用户修改。

