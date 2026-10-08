# 手动测试多次执行与合并 CSV 实施计划

> **For agentic workers:** 本计划在当前会话中由主代理按步骤内联执行。

**Goal:** 在手动测试窗体增加重复次数和多次测试入口，将每轮所有通道结果追加到同一个 CSV，并在产生至少一轮有效数据后自动打开文件。

**Architecture:** 复用 `FormalTestRunner.RunSparseMeasurement` 做同步单轮采集。窗体每轮立刻把 `RecieveData` 转为不可变的 CSV 行快照，写入批次文件并 flush；结果表更新为最近完成轮次。批次统一管理控件状态、失败停止、文件关闭和自动打开。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.8、`NumericUpDown`、`StreamWriter`、`Process.Start(UseShellExecute=True)`。

---

## 文件与职责

- 修改 `FrmManualTest.vb`：重复次数控件、底部布局、多次测试入口、批次写 CSV、每轮快照、自动打开及错误恢复。单次测试和现有手动导出流程保持原样。
- `FormalTestRunner.vb`：仅调用现有单轮入口，不修改仪器测量逻辑。

## Task 1：扩展底部批次控件

**文件：** `FrmManualTest.vb`

- [x] 新增 `NumericUpDown` 和“多次测试”按钮字段。
- [x] 在底部面板加入“重复次数”、默认 10、最小 1、最大 1000 的数字选择框以及多次测试按钮；保留 CSV 导出、单次测试和关闭按钮。
- [x] 将批次按钮和次数框纳入 `SetTestingState`，测试期间禁用，完成或失败后恢复。

## Task 2：实现批次检查和逐轮测量

**文件：** `FrmManualTest.vb`

- [x] 新增 `StartMultipleManualTests`，依次复用单次测试的登录、自动流程占用、通道、运行状态和中位机通信检查。
- [x] 在开始前复制所选通道列表和重复次数；清空旧表格并进入测试状态。
- [x] 创建时间戳唯一到毫秒的 `testData/ManualTest_Multi_*.csv`，以 UTF-8 BOM 写指定表头。
- [x] 每轮更新进度，调用一个 `FormalTestRunner` 的 `RunSparseMeasurement(channels)`；完成后快照本轮字段，逐行追加并 `Flush()`。
- [x] 数值使用 `CultureInfo.InvariantCulture`；字段通过现有 `EscapeCsvField` 转义。
- [x] 每轮成功后调用已有 `DisplayManualTestResult(channels)`，最终展示最后完成轮次。
- [x] 任一轮异常时记录失败轮次、停止剩余轮次并保留已写入数据；`Finally` 关闭写入器并恢复界面状态。

## Task 3：批次结束后打开文件并反馈结果

**文件：** `FrmManualTest.vb`

- [x] 至少一轮数据成功写入后，关闭 writer 并自动打开 CSV。
- [x] 全部完成时提示批次完成及路径；部分失败时打开已完成数据并提示失败轮次；零轮完成时不打开只有表头的文件。
- [x] 如果 CSV 打开失败，保留文件，日志和提示中包含文件路径与打开异常。
- [x] CSV 创建或写入失败时释放资源、恢复按钮状态并提示错误。

## 验收与检查

- [x] 代码检查确认重复次数默认 10、范围 1–1000，底部包含新旧控件。
- [x] 代码检查确认使用启动时复制的通道，所有轮次记录在同一个文件并带轮次编号。
- [x] 每轮写入后 `Flush()`；异常时跳出循环并保留已完成轮次。
- [x] 代码检查确认结果表展示最近完成轮次，单测、导出、关闭和夹具入口仍保留。
- [x] 文件打开发生在 writer 释放之后。
- [x] 未连接仪器或运行测试；Visual Studio .NET Framework MSBuild Debug/AnyCPU 编译成功，0 错误、3 个 x86/MSIL 架构警告。
