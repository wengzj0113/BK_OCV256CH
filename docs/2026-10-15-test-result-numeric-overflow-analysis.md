# 测试完成后 @Results 数值溢出排查

> 后续结论：用户随后确认内阻读数为 `1E+13 mΩ`，超过项目 `decimal(18,6)` 的容量；该具体值已复现 SQL 错误 8115。经用户确认，现已实现异常大数、缺测或解析失败时使用 10000 mΩ，规则见 `superpowers/specs/2026-10-15-3562-resistance-fallback-design.md`。下文“当前没有固定缺测赋值”等描述为修复前的排查记录。

## 已确认的故障位置

现场截图时间为 16:14:54：初次测试完成、NG 通道 7 个，随后出现 numeric 到 numeric 算术溢出，并明确指出表值参数 `@Results` 与表类型不符，SQL 错误 8115。

调用链：

`InitialPassCompleted → FinalizeCycle → frmTest.PublishFinalCycle → SaveOcvDataTestResults → OcvDataService.SaveTestResults → dbo.usp_SaveTestResults(@Results)`。

失败发生在测试结果整批传入 SQL Server 的数值转换阶段。该错误不能用 PLC 时序或“有 7 个 NG”来解释；NG 测试结果本来就允许入库。本批结果未保存成功，流程不会发送成功的 R161；CSV 和 MES 调度也位于数据库保存之后。

## 10000 的核验

用户确认页面最大值为 10000。程序定义和本地开发数据库的实际元数据一致：

| 字段 | SQL 类型 | 最大绝对值上界（不包含） |
|---|---|---|
| VoltageVolt | decimal(18,9) | 1,000,000,000 |
| RawResistanceMilliohm | decimal(18,6) | 1,000,000,000,000 |
| CalibrationValueMilliohm | decimal(18,6) | 1,000,000,000,000 |
| FinalResistanceMilliohm | decimal(18,6) | 1,000,000,000,000 |

使用实际 `BuildTestResultTable` 和 SQL Server 表值参数做只读测试，10000 mΩ 可以传入。10000 本身不足以解释现场溢出。

程序分别保存原始内阻、校准补偿值和校准后内阻；页面显示值不足以证明这些字段的实际待写值都在范围内。仪表解析器接受可解析的科学计数值，仅校验格式和条数，因此异常大的原始内阻在代码上确实可能到达入库阶段，但没有现场原始记录，不能据此认定本次属于这种情况。

补充缺测赋值检查：当前主页面正式测量的内阻链路没有“检测不到就固定设为 10000 或 100000”的规则。`InstrumentCacheParser` 将仪表 Ω 读数乘 1000 转成 mΩ；`CalibrationSnapshot.ApplyResistance` 加补偿；`TwoPassResultMapper.ApplyResistance` 直接保存原始值和校准后值。主页面相关代码里明确的 `>10000 → 10000` 限幅针对 `Voltage` 电压字段。解析失败返回采集失败；可解析但过大的内阻数字并不等同于解析失败。

## 复现实验及其限度

在本地实际 `OcvDataService.SaveTestResults` 调用中，构造标准通道 7 的 `RawResistanceMilliohm=1000000000000`，使用不存在的任务号 -1。SQL Server 在执行存储过程前拒绝 TVP，重现完整报错：numeric 溢出、`@Results` 表类型不符、错误 8115。本实验未写入持久化测试数据。

这是用于验证故障机制的人工样本，不是从现场读取到的数值。与现场相同的错误只说明存在相同的失败机制，不能证明现场通道号、字段或数值与样本一致。

## 当前尚待现场确认的两项

1. **现场表类型精度不同。** 开发机的数据库不是截图对应的现场数据库；本地最近任务早于此次测试。必须查询现场 `dbo.TestResultInputType`，只看 `dbo.TestResult` 不够。
2. **实际待写值与页面显示不同。** 必须核对同一批次各通道的 VoltageVolt、RawResistanceMilliohm、CalibrationValueMilliohm、FinalResistanceMilliohm，尤其是未在页面完整展示的字段。

## 已增加的定位能力

`SaveTestResults` 仅在捕获 SQL 错误 8115 时读取当前连接对应数据库的真实表类型精度，逐个检查本批数值（包括按目标小数位舍入后越界的情况），输出标准通道号、字段名、待写值和 `decimal(p,s)`，并保留原始 SQL 异常。

若未找到越界值，则输出现场字段定义及本批各字段最大绝对值所在通道，帮助继续检查 TVP 版本或元数据。元数据读取失败不会覆盖原始数据库错误。诊断不改变测量值、NG 结果或数据库结构，正常保存不增加查询。

只读现场检查脚本：`tools/Diagnose-TestResultNumericOverflow.sql`。它查询现场 TVP 精度、目标表精度、存储过程参数类型和最近失败任务，包含实际服务器名以确认查对数据库。

回归脚本：`tools/Test-OcvNumericOverflowDiagnostics.ps1`。使用 32 位 Windows PowerShell，指定新构建的 `-AssemblyPath`；本地集成身份连接测试数据库，验证 10000 可传入及溢出定位详情。

现场日志目录：`%LOCALAPPDATA%\XK-OCV-256CH\logFile`。`Business_YYYYMMDD.log` 包含保存失败异常链，`MiddleMachine_YYYYMMDD.log` 包含仪表缓存预览。当前事件截图不足以确认具体越界字段。
