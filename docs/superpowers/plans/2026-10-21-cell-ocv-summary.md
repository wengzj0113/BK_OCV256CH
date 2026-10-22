# 电芯 OCV0/1/2 汇总与 K 值 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 为每个电芯码维护一行 OCV0/1/2 当前测量值和按小时差计算的 K1/K2，并与现有结果写入保持事务一致。

**Architecture:** 新增 `dbo.CellOcvSummary` 作为跨阶段汇总表；扩展 `usp_SaveTestResults`，在原有条码锁和事务内先保存当前明细，再按 OCV 阶段 upsert 汇总并重算 K。应用保存入口传入本阶段结果采集完成时间；现代 SQL、SQL Server 2008 R2 基线和增量迁移保持一致。

**Tech Stack:** VB.NET WinForms / ADO.NET, SQL Server stored procedures, PowerShell SQL verification scripts.

---

## 代码与文档文件职责

- `docs/sql/001_create_ocv_data_schema.sql`：现代数据库中新表、索引和查询过程所需对象的基线 schema（查询过程放 002）。
- `docs/sql/002_create_core_procedures.sql`：阶段汇总写入及按条码读取过程；扩展 `usp_SaveTestResults`。
- `docs/sql/018_cell_ocv_summary.sql`：SQL Server 2008 R2+ 既有库的幂等增量迁移，创建表并替换保存/查询过程。
- `docs/sql/019_verify_cell_ocv_summary_rollback.sql`：一次性验证库运行、使用事务回滚的数据库行为校验。
- `OCV_DATA_SQLServer2008R2.sql`：同步完整安装脚本的表和存储过程定义。
- `docs/sql/README.md`：迁移运行顺序、兼容范围、旧数据无法回填的说明。
- `OcvDataService.vb`：`SaveTestResults` 传阶段采集时间，并新增按条码读取 summary 的方法。
- `frmTest.vb`：手动保存和 PLC `PublishFinalCycle` 保存路径传递同一阶段时间值。
- `docs/superpowers/specs/2026-10-21-cell-ocv-summary-design.md`：已确认的 schema、K 公式、时间口径和事务设计。

## Task 1: 建立 schema 和可重复部署迁移

**Files:**
- Modify: `docs/sql/001_create_ocv_data_schema.sql`
- Create: `docs/sql/018_cell_ocv_summary.sql`
- Modify: `OCV_DATA_SQLServer2008R2.sql`

- [ ] **Step 1: 写 schema 合约检查脚本**

在 `docs/sql/019_verify_cell_ocv_summary_rollback.sql` 初始添加元数据断言：`OBJECT_ID('dbo.CellOcvSummary','U')` 存在；`BatteryCode` 是主键；列包含 Vol0/1/2、Res0/1/2、Time0/1/2、K1/K2、TrayCode、ChannelNo、TaskId0/1/2、CreatedAt、UpdatedAt、RowVersion；三组阶段字段与 K 列允许 NULL。

- [ ] **Step 2: 执行合约脚本并确认失败**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`
Expected: 因为新表不存在，脚本以非零状态退出并报告缺少 `dbo.CellOcvSummary`。

- [ ] **Step 3: 定义新表**

在现代 schema 和 SQL Server 2008 R2 安装脚本中创建相同列：`BatteryCode nvarchar(100) NOT NULL` 主键；Vol 列 `decimal(18,9)`；Res 列 `decimal(18,6)`；Time 列 `datetime2(3)`；K 列 `decimal(18,9)`；阶段任务 ID `bigint`；TrayCode、ChannelNo；CreatedAt、UpdatedAt、RowVersion。时间/阶段/来源列允许 NULL。增加 `IX_CellOcvSummary_TrayCode_ChannelNo`，不设 `TestResultId` 外键，因为该明细 ID 会跨阶段复用。

- [ ] **Step 4: 写增量迁移并对齐 schema**

`018_cell_ocv_summary.sql` 使用 `USE OCV_DATA; GO`、`IF OBJECT_ID(...) IS NULL` 和 `IF NOT EXISTS` 建表/索引，保证 SQL Server 2008 R2 可执行。迁移不回填旧行；只新建空表。将同样定义同步到 001 和根目录完整安装脚本。

- [ ] **Step 5: 运行 schema 合约检查**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/018_cell_ocv_summary.sql -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`
Expected: 元数据断言全部通过；重复运行 018 不报错且表/索引只有一份。

## Task 2: 在 SQL 保存事务中 upsert 阶段值并计算 K

**Files:**
- Modify: `docs/sql/002_create_core_procedures.sql`
- Modify: `docs/sql/018_cell_ocv_summary.sql`
- Modify: `OCV_DATA_SQLServer2008R2.sql`
- Test: `docs/sql/019_verify_cell_ocv_summary_rollback.sql`

- [ ] **Step 1: 为验证脚本加入保存行为断言**

在事务中准备 3 个唯一测试条码和 OCV0/1/2 任务；按指定时刻保存 `Vol0=3.500000000 @ 08:00`、`Vol1=3.510000000 @ 10:00`、`Vol2=3.530000000 @ 14:00`。断言阶段列分别为输入值，`K1=0.005 V/hour`、`K2=0.005 V/hour`；最后 rollback。

- [ ] **Step 2: 执行用例并确认失败**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`
Expected: 新表列存在，但行为断言因保存过程还未更新 summary 而失败。

- [ ] **Step 3: 扩展保存过程的阶段时间接口**

给 `usp_SaveTestResults` 增加 `@StageTestTime datetime2(3)`，必填且非未来任意假默认；应用每次保存提供一个批次一致时间。读取任务的 `StageCode`，只接受 OCV0/OCV1/OCV2 写汇总；若任务是其他阶段，保留 TestResult 原行为并不碰 CellOcvSummary。

- [ ] **Step 4: 在原事务中写入汇总并重算**

保留 `OCV_DATA.TestResult.BarcodeIdentity` 应用锁。完成 TestResult insert/update 后，对 `@Normalized` 每个条码 upsert `CellOcvSummary`。使用 `UPDLOCK, HOLDLOCK` 按 BatteryCode 串行化不存在行的首次插入。只覆盖本次 OCV 阶段对应的 Vol/Res/Time/TaskId 和 Tray/Channel；保留其他阶段。通过统一 UPDATE 从当前列计算 K：时间缺失、阶段缺失或间隔 `DATEDIFF_BIG(millisecond, ...) <= 0` 时写 NULL，否则用电压差乘 `3600000.0 / 毫秒差`。SQL Server 2008 R2 以 `CAST(DATEDIFF(day, @t0, @t1) AS decimal(18,0)) * 86400000 + DATEDIFF(millisecond, CONVERT(time(3), @t0), CONVERT(time(3), @t1))` 得到毫秒差；现代 SQL 使用 `DATEDIFF_BIG(millisecond, @t0, @t1)`。K 用 `(@v1 - @v0) * 3600000.0 / @elapsedMilliseconds`，先转 decimal 防止整数除法。验证跨日、负间隔和毫秒级间隔。不要在事务提交后另行更新 summary。

- [ ] **Step 5: 加入边界、覆盖和回滚用例**

验证 OCV1 先于 OCV0、只有单阶段时 K 为 NULL；相同阶段二次保存后只更新该阶段并重算 K；零/负时间差 K 为 NULL；缺少电压时 K 为 NULL；第二个条码不影响第一个；人为触发 summary 写失败时 TestResult 也回滚。验证脚本末尾回滚所有任务、明细和 summary 行。

- [ ] **Step 6: 执行 SQL 验证**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`
Expected: 所有断言通过；数据库中无 `CELL_OCV_VERIFY_%` 条码残留。

## Task 3: 同步 NG 复测覆盖写路径

**Files:**
- Modify: `docs/sql/002_create_core_procedures.sql`
- Modify: `docs/sql/018_cell_ocv_summary.sql`
- Modify: `OCV_DATA_SQLServer2008R2.sql`
- Test: `docs/sql/019_verify_cell_ocv_summary_rollback.sql`

- [ ] **Step 1: 增加覆盖路径回归用例**

创建一个已有 OCV0 summary 的 NG `TestResult`，调用 `usp_SaveNgRetest @OverwriteOriginal=1`，断言 `TestResult.VoltageVolt/FinalResistanceMilliohm` 和 `CellOcvSummary.Vol0/Res0/Time0/K1/K2` 在同一事务更新；调用 `@OverwriteOriginal=0` 时断言 summary 完全不变。

- [ ] **Step 2: 执行覆盖回归并确认失败**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`
Expected: 覆盖路径的 summary 断言失败，审计路径断言通过。

- [ ] **Step 3: 只在实际覆盖时同步 summary**

在 `usp_SaveNgRetest` 成功更新来源 `TestResult` 的同一事务中，按原始行的 `OcvStageId` 映射 OCV0/1/2，使用 `@RetestTime` 写对应 Time、复测电压和最终内阻、现有 task/tray/channel 来源，并重算 K1/K2。审计型 `@OverwriteOriginal=0` 不更新 summary。SQL Server 2008 R2 版本使用同一兼容小时差表达式。

- [ ] **Step 4: 验证覆盖与仅审计两条路径**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`
Expected: 覆盖写同步通过；审计不改 summary；任一 summary 失败都会连同 TestResult/审计操作一并回滚。

## Task 4: 增加按条码查询过程和 ADO.NET 接口

**Files:**
- Modify: `docs/sql/002_create_core_procedures.sql`
- Modify: `docs/sql/018_cell_ocv_summary.sql`
- Modify: `OCV_DATA_SQLServer2008R2.sql`
- Modify: `OcvDataService.vb`

- [ ] **Step 1: 定义读取接口**

创建 `dbo.usp_GetCellOcvSummary @BatteryCode nvarchar(100)`；trim 条码；空值抛出清楚错误；按唯一键返回 0 或 1 行，字段只投影 summary 的公开列。

- [ ] **Step 2: 接入 OcvDataService**

添加 `Public Function GetCellOcvSummary(ByVal batteryCode As String) As DataTable`，使用现有 `CreateStoredProcedureCommand`、`AddParameter`、`ExecuteDataTable` 模式调用 `dbo.usp_GetCellOcvSummary`；新增参数不得拼 SQL 字符串。

- [ ] **Step 3: 把查询断言接入数据库验证**

验证脚本调用读取过程检查已存在条码恰好返回一行、未知条码返回 0 行、首尾空白输入归一化、空输入失败，并回滚夹具数据。

## Task 5: 从手动和自动测试入口传入阶段采集时间

**Files:**
- Modify: `OcvDataService.vb`
- Modify: `frmTest.vb`

- [ ] **Step 1: 更新服务签名与参数绑定**

将 `SaveTestResults(testTaskId, results)` 改为要求 `stageTestTime As DateTime`；通过 `@StageTestTime` 绑定 `SqlDbType.DateTime2`，不在服务内部重新读取当前时间。保持唯一调用点显式传值。

- [ ] **Step 2: 更新手动测试保存路径**

在 `frmTest.vb` 测量整理成功后、调用 `SaveOcvDataTestResults` 前记录本阶段结果采集完成时刻；把同一 DateTime 经 `SaveOcvDataTestResults` 传给服务。每批所有电芯共用该时间，确保同一 OCV 阶段时间口径一致。

- [ ] **Step 3: 更新 PLC 自动最终发布路径**

在 `PublishFinalCycle` 保存最终结果前取一次 `DateTime.Now`，显式传入 `SaveOcvDataTestResults`。初测/复测中间轮次仍不落库；仅最终结果生成该阶段汇总值。

- [ ] **Step 4: 静态检查全部调用点**

Run: `rg -n "SaveTestResults\(|SaveOcvDataTestResults\(" --glob "*.vb"`
Expected: 所有调用都显式提供阶段时间；无旧签名调用。

## Task 6: 部署文档、复核和交付

**Files:**
- Modify: `docs/sql/README.md`
- Modify: `docs/sql/001_create_ocv_data_schema.sql`
- Modify: `docs/sql/002_create_core_procedures.sql`
- Modify: `docs/sql/018_cell_ocv_summary.sql`
- Modify: `docs/sql/019_verify_cell_ocv_summary_rollback.sql`
- Modify: `OCV_DATA_SQLServer2008R2.sql`
- Modify: `OcvDataService.vb`
- Modify: `frmTest.vb`

- [ ] **Step 1: 记录部署顺序**

README 写明既有库在部署应用前运行 `018_cell_ocv_summary.sql`，再执行 `019_verify_cell_ocv_summary_rollback.sql` 于一次性验证库；现代新库从 001/002 得到对象；SQL Server 2008 R2 新库使用根目录完整安装脚本。注明升级不回填历史阶段：原 TestResult 已按条码跨阶段覆盖，旧阶段值无法可靠恢复；新表从迁移后新保存结果开始累计。

- [ ] **Step 2: 跨脚本定义一致性检查**

Run: `rg -n "CellOcvSummary|usp_GetCellOcvSummary|StageTestTime" docs/sql/001_create_ocv_data_schema.sql docs/sql/002_create_core_procedures.sql docs/sql/018_cell_ocv_summary.sql OCV_DATA_SQLServer2008R2.sql`
Expected: 新表定义、保存参数和读取过程都存在；对比现代、迁移、SQL 2008 R2 中列名、精度、计算公式一致。

- [ ] **Step 3: 编译应用**

Run: `MSBuild.exe 'XL-OCV-UIControls.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86`
Expected: Build succeeded，0 errors。

- [ ] **Step 4: 执行完整数据库验证并检查工作区差异**

Run: `sqlcmd -S <验证实例> -d OCV_DATA -b -i docs/sql/019_verify_cell_ocv_summary_rollback.sql`，随后运行 `git diff --check`。
Expected: 所有事务回滚验证通过；`git diff --check` 无空白错误。若无 SQL Server 2008 R2 实例，应将实际运行的版本与未覆盖的引擎兼容性明确记录。

