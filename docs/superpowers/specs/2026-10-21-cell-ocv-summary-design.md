# 按电芯汇总 OCV 阶段数据设计

## 目标

新增 `dbo.CellOcvSummary`，按 `BatteryCode` 唯一保存一个电芯的 OCV0、OCV1、OCV2 当前阶段值。现有 `dbo.TestResult` 仍承担当前结果明细与判定用途；汇总表用于保留跨阶段结果，因为当前 `usp_SaveTestResults` 会按电芯码覆盖 `TestResult` 行，后续阶段保存会替换前一阶段明细。

## 字段与单位

- `BatteryCode nvarchar(100)`：主键；写入前 trim，空值拒绝。
- `Vol0/Vol1/Vol2 decimal(18,9)`：电压，V，与 `TestResult.VoltageVolt` 一致。
- `Res0/Res1/Res2 decimal(18,6)`：最终内阻，mΩ，与 `TestResult.FinalResistanceMilliohm` 一致。
- `Time0/Time1/Time2 datetime2(3)`：各 OCV 阶段结果采集完成时间。每个保存批次使用一次一致的阶段时间，由保存过程接收调用端明确传入；不得用延迟写入的 `CreatedAt` 伪装测量时间。
- `K1/K2 decimal(18,9)`：分别为 V/hour；OCV0 不设 K。
- `TrayCode nvarchar(100)`、`ChannelNo smallint`：最近一次阶段结果的托盘/通道定位信息。
- `TaskId0/TaskId1/TaskId2 bigint`：阶段来源任务，便于追溯；不保存 `TestResultId`，因为该 ID 在跨阶段条码覆盖时会被原地复用。
- `CreatedAt datetime2(0)`、`UpdatedAt datetime2(3)`、`RowVersion rowversion`：记录维护字段。

阶段值列和来源任务列可空，支持阶段尚未完成的电芯。

## 计算规则

- `K1 = (Vol1 - Vol0) * 3600000 / elapsedMilliseconds(Time0, Time1)`。
- `K2 = (Vol2 - Vol1) * 3600000 / elapsedMilliseconds(Time1, Time2)`。
- 任一所需阶段值或时间缺失，或时间差小于等于零时，对应 K 为 `NULL`。SQL Server 2008 R2 以 `DATEDIFF(day, t0, t1) * 86400000 + DATEDIFF(millisecond, CONVERT(time(3), t0), CONVERT(time(3), t1))` 得到毫秒差；现代 SQL 可用 `DATEDIFF_BIG(millisecond, t0, t1)`。
- 任意阶段结果被新保存覆盖时，更新该阶段列并从当前三个阶段值重算 K1/K2，避免前序阶段变化后留下过期斜率。
- 数据库统一按 V/hour 计算；如业务最终要求 mV/hour，应另行确认后同时调整单位注释、精度和消费端展示。

## 数据流和事务

`OcvDataService.SaveTestResults` 增加阶段采集时间参数并传给 `dbo.usp_SaveTestResults`。保存过程在现有条码级应用锁和事务内完成规范化、更新 `TestResult`，然后根据任务的 `OcvStage.StageCode` 更新 `CellOcvSummary` 对应阶段列并重算 K 值。任一写入失败则两张表一起回滚。事务内的汇总 upsert 使用已规范化的 BatteryCode，不依赖更新后会被覆盖的 TestResult 内容。

所有通过 `usp_SaveTestResults` 接受的结果都会写入汇总表，包含 Formal、Manual、Retest、Debug 来源；重复保存同一阶段时该阶段按现有“当前结果覆盖”语义更新。扫码 NG 复测审计入口 `usp_SaveNgRetest` 不更新汇总表，因为它不覆盖当前 OCV 阶段结果，只有选择覆盖原结果并确实更新 `TestResult` 时才需同步更新该阶段汇总。实现计划应覆盖这个分支并保持两个写路径原子一致。

自动 PLC 流程在 `PublishFinalCycle` 以当前设备阶段创建最终任务并调用同一保存过程，因此不增加独立的汇总写逻辑。

## 查询和部署

新增 `dbo.usp_GetCellOcvSummary(@BatteryCode)`，按 trim 后条码返回一行汇总，供应用或 SQL 查询使用。部署 SQL Server 2008 R2 兼容增量迁移脚本，并同步现代 `001_create_ocv_data_schema.sql`、`002_create_core_procedures.sql` 和根目录完整安装脚本。迁移创建空汇总表，不尝试从 `TestResult` 回填 OCV0/1/2：由于当前表按条码跨阶段覆盖，旧阶段值已不可可靠恢复。

## 验收重点

验证 OCV0 → OCV1 → OCV2 分阶段保存、阶段乱序保存、缺阶段 K=NULL、正确小时差、零/负时间差、同阶段覆盖后 K 重算、不同电芯并存、条码空值/空白、事务回滚、并发同码保存串行化，以及 SQL Server 2008 R2 兼容迁移。

