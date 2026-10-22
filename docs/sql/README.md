# OCV_DATA SQL Scripts

## 运行时数据库说明

应用运行时已经不再访问 `CWSQL`，统一通过 `OcvDataService` 使用 `OCV_DATA`。旧库迁移脚本已移至 `docs/archive/legacy-cwsql`，仅作为历史记录保留，不属于正常启动或测试流程。

## SQL Server 2008 R2 新电脑部署

- 在目标实例尚无 `OCV_DATA` 数据库时，使用仓库根目录的 `OCV_DATA_SQLServer2008R2.sql`，在 SSMS 中连接目标实例后打开并执行整份脚本。脚本包含兼容的表和存储过程、已存在 `xlsuper`/`cwsuper` 登录的数据库映射、OCV 阶段与等级字段种子数据，以及最后的健康检查。
- 使用具有建库和建对象权限的账号执行。脚本末尾的健康检查应全部返回 `IsOk = 1`。
- 该安装脚本不复制旧库里的托盘、测试结果等业务历史记录；这些记录需要另行迁移。
- 已经有 `OCV_DATA` 数据库但缺少基线种子的实例，使用 `011_seed_ocv_data_sqlserver2008r2_baseline.sql`；不要重跑会创建数据库和表的完整安装脚本。

## 业务部署运行顺序(必须按顺序执行)

1. `000_init_database.sql` — 创建 OCV_DATA 数据库,设置兼容级别(150 = SQL Server 2019)、恢复模式(SIMPLE)、启用 Query Store。需要 sysadmin 权限。幂等,已存在则跳过 CREATE DATABASE。
2. `001_create_ocv_data_schema.sql` — 创建所有表、索引、约束、视图、用户定义表类型(18 张表 + 1 视图 + 2 UDDT)、种子数据(OcvStage / GradeSelectableField)。幂等,缺失对象才创建。
3. `002_create_core_procedures.sql` — 创建所有存储过程(`usp_RegisterTray` / `usp_CreateTestTask` / `usp_SaveTestResults` / `usp_CompleteTestTask` / `usp_GetActiveCalibrationValues` / `usp_SaveChannelCalibration` / `usp_EnqueueUpload` / `usp_GetPendingUploads` / `usp_MarkUploadSent` / `usp_MarkUploadFailed` / `usp_SaveMesUploadLog` / `usp_GetTrayListByDate` / `usp_GetTrayLatestStageResults` / `usp_GetNgRetestSourceByBatteryCode` / `usp_SaveNgRetest` / `usp_CheckDatabaseHealth` / `usp_GetSchemaVersion` 等)。幂等,使用 `CREATE OR ALTER`。

## 业务数据迁移(R2-15,旧 CWSQL → 新 OCV_DATA)

> 重要:此步骤会从旧库 `CWSQL` 读取历史数据,需要 `CWSQL` 数据库仍可访问,且 `CWSQL` 与 `OCV_DATA` 在同一实例。如果跨实例,需要先用 SSMS 的"导出数据"或 Linked Server 桥接。

4. `006_migrate_unsent_data_to_upload_outbox.sql` — 把 `CWSQL.UnsentData (Is_Sent=0)` 和 `CWSQL.PendingUploads` 报迁到 `OCV_DATA.UploadOutbox`。存储过程 `usp_MigrateLegacyUnsentDataToUploadOutbox` 支持 `@DryRun=1` 演练模式,先 DryRun 确认条数,再改为 0 正式执行。
5. `007_migrate_grade_rules.sql` — 把 `CWSQL.CW_tblCondition` + `CWSQL.CW_tblSelFld` 报迁到 `OCV_DATA.GradeScheme` + `GradeRule` + `GradeSelectableField`。存储过程 `usp_MigrateLegacyGradeRules` 支持 `@DryRun=1` 演练模式,字段名从旧全限定列名(`CW_tblData_Info.Fld_VolA`)反向映射为新字段名(`VoltageVolt`)。

## 迁移后验证

6. `008_post_migration_health_check.sql` — 8 项检查:SchemaVersion 完整性、必备存储过程、关键表行数、OcvStage 种子数据、迁移标识、索引、视图、`usp_CheckDatabaseHealth` 综合检查。**所有检查通过后才能切换应用连接**。

## SQL Server 2008 R2 完整导出脚本

- `011_seed_ocv_data_sqlserver2008r2_baseline.sql` — 在 `OCV_DATA_SQLServer2008R2.sql` 成功创建表和存储过程后运行。该兼容脚本补齐导出文件不包含的 OCV0/OCV1/OCV2 阶段、等级字段及 001/002 版本记录，并在末尾执行数据库健康检查。可重复运行。
- `012_debug_mode_result_queries_sqlserver2008r2.sql` — 在已部署的 SQL Server 2008 R2 `OCV_DATA` 上安装调试/生产结果隔离查询和 NG 来源过滤。该脚本只重建两个存储过程，不改表和业务数据；将调试模式应用版本部署到已有 2008 R2 数据库前必须运行。全新部署仍使用根目录完整安装脚本，该脚本也包含相同过程定义。
- `013_seed_demo_trays.sql` — 在已部署 `OCV_DATA` 表结构及 `dbo.usp_SaveTrayCells` 后运行，写入 `TP001`（`BAT01001`–`BAT01256`）和 `TP002`（`BAT02001`–`BAT02256`）各 256 个通道映射。重复运行会更新这两个托盘的映射；脚本末尾会查询并显示结果。
- `014_ng_retest_source_identity.sql` — 在既有 `OCV_DATA` 上部署并列 NG 复测版本前运行；SQL Server 2008 R2 及更高版本可用。脚本幂等补齐 `OCV_RETEST_DATA` 的来源/操作 ID、唯一操作索引以及 NG 来源查询和保存过程。新装库通过 001/002（现代 SQL Server）或根目录完整安装脚本（SQL Server 2008 R2）得到相同定义。
- `015_ng_retest_calibration_snapshot.sql` — 部署带校准快照的扫码 NG 复测版本前运行，且须在 014 之后执行；SQL Server 2008 R2 及更高版本可用。保留历史复测值并回填为“原始值=旧复测值、补偿=0、最终值=旧复测值”，然后更新复测保存过程以保存原始内阻、通道补偿、最终内阻及快照指纹/来源。新装库通过 001/002 或根目录完整安装脚本直接建立相同结构。

## 调试模式查询部署

- SQL Server 2016 SP1 或更高版本的 `OCV_DATA`：重新运行 `002_create_core_procedures.sql`，然后运行 `008_post_migration_health_check.sql`。
- SQL Server 2008 R2 的既有 `OCV_DATA`：运行 `012_debug_mode_result_queries_sqlserver2008r2.sql`。根目录完整安装脚本已同步更新，后续新建数据库无需另外运行 012。
- 不需要增加字段或改写历史任务/结果行。生产最新结果查询和 NG 复测来源查询会排除 `TestType='Debug'`；调试结果查询显式选择调试任务。

## 诊断与辅助脚本(独立运行,无顺序)

- `003_database_verification_cases.sql` — 用例化测试数据验证 schema 和存储过程(走事务回滚,不改真实数据)。用于发布前烟测。
- `004_debug_cwsuper_login.sql` — 排查 "用户 'cwsuper' 登录失败" 问题。只读,报告认证模式、登录态、用户映射、必备存储过程、端口等。兼容 SQL Server 2012+。
- `005_create_xlsuper_login.sql` — 创建 `xlsuper / 123` 登录并映射为 OCV_DATA 的 db_owner(便于应用以非 sysadmin 账户连接)。需要 sysadmin。幂等,拒绝在纯 Windows 认证模式下运行。运行后需在 `frmInit` 设置 `Initsetting.strUserID = "xlsuper"`、`strpwd = "123"`。
- `009_check_uniqueness.sql` — 创建 `usp_CheckTrayCodeExists` 和 `usp_CheckGradeSchemeExists` 两个 OUTPUT 布尔型存储过程,用于 R2-15 替代 `clsDB.IsExists`。需在 002 之后独立运行。
- `010_check_uniqueness_verification.sql` — 对 `009` 创建的两个存储过程做 6 个用例的事务回滚校验。需在 009 之后独立运行。

## 推荐部署流程

1. **本地 / 预发布环境演练**:
   ```
   运行 000 → 001 → 002 → 003(烟测) → 006 DryRun → 007 DryRun → 008
   确认 008 健康检查全部通过 + 006/007 DryRun 条数符合预期
   ```
2. **正式迁移**:
   ```
   运行 000 → 001 → 002 → 006 DryRun → 006 Execute → 007 DryRun → 007 Execute → 008
   ```
3. **应用切换**:
   - 在 `frmInit` 修改 OCV_DATA 连接配置(若用 xlsuper 走 005)
   - 应用启动后查看 MDIForm 状态栏 `Database connected`
   - 用查询页(frmInquery)抽查 1-2 个托盘数据,确认 DGVbar 显示 + 等级计算正确
4. **观察期**(建议 30 天):
   - 保留 `CWSQL` 旧库只读
   - 监控 `OCV_DATA.UploadOutbox Status=Pending` 是否持续下降
   - 监控 `OCV_DATA.MesUploadLog` 的 `IsSuccess=0` 比例
5. **最终退役**:
   - 30 天观察无异常后,人工 `DROP DATABASE CWSQL`
   - 移除 `CWSQL` 连接字符串
   - 移除项目内 `clsDB.vb` / `SqlServerHelper.vb` / `frmTest - 副本.vb` / `FrmEMSTask (3).vb`

## 脚本幂等性约定

- 所有 `001/002` 脚本:用 `IF NOT EXISTS`、`IF OBJECT_ID IS NULL`、`CREATE OR ALTER` 保证可重复执行
- `000` 脚本:检测 `DB_ID` 后才建库,重复运行会跳过
- `005/006/007/008` 脚本:有副作用,谨慎重复执行。`006/007` 推荐用 `@DryRun=1` 先验证条数

## 电芯条码结果覆盖部署（016）

- 已有数据库依次执行 `014_ng_retest_source_identity.sql`、`015_ng_retest_calibration_snapshot.sql`、`016_barcode_result_upsert.sql`，再部署应用。016 兼容 SQL Server 2008 R2，可重复运行；现代 001/002 与根目录完整安装脚本包含相同定义。
- `TestResult` 以去除首尾空格后的 `BatteryCode` 作为全库当前结果身份，跨任务、托盘、OCV 阶段以及 Debug/Manual/Formal 模式。同条码再次保存会移动并覆盖当前结果的任务/托盘/阶段/通道、规格、测量及判定字段，保留最新旧记录的 `TestResultId` 和 `CreatedAt`。最新记录按 `COALESCE(UpdatedAt, CreatedAt) DESC, TestResultId DESC` 确定。
- 安装/升级不清理历史结果。仅当某条码再次保存时，合并该条码所有旧重复行，并先把 `OCV_RETEST_DATA.SourceTestResultId` 引用重定向到保留行。未保存的条码历史重复行继续保留，因此条码索引不能是唯一索引。
- 输入条码先 trim，空值再使用同托盘/通道的有效 TrayCell 条码（同样 trim）；仍为空、归一化条码重复或通道重复会拒绝整批。目标任务/通道如果被本批以外的条码占用，同样拒绝；本批条码之间可交换通道。原 `UX_TestResult_Task_Channel` 替换为非唯一 `IX_TestResult_Task_Channel`，另建非唯一 `IX_TestResult_BatteryCode`。
- 普通保存、NG 覆盖和只记审计的复测保存通过同一事务级 `sp_getapplock` 串行化，15 秒获取超时会报错。任何错误回滚完整事务（包含调用者外层事务）。NG 覆盖仍验证来源行/条码/托盘/通道及 NG 状态，并要求来源是该条码最新当前行；审计模式仍只追加复测审计，不改原结果；若延迟提交的来源 ID 已被普通保存合并删除，则在锁内将引用解析到该条码当前保留行，保留调用者原始测量/上下文快照。来源属于其他条码或已无有效结果时拒绝写入。NG 来源及覆盖均使用结果条码 trim 后回退有效 TrayCell 条码的相同规则。复测快照及操作 ID 幂等行为保留。
- `017_verify_barcode_result_upsert_rollback.sql` 是发布前验证脚本，仅应在一次性验证库运行。覆盖重复/跨阶段保存、条码和通道归一化拒绝、旧重复按需合并、交换通道、历史来源引用重定向以及 NG 校准快照与覆盖；所有数据事务回滚。执行需允许建测试数据，建议 `sqlcmd -b` 使断言错误返回失败。
- 验证环境：独立命名的 SQL Server 2019 LocalDB 实例，数据库兼容级别 100。该验证不替代 SQL Server 2008 R2 引擎上的部署验收。

## 电芯 OCV0/1/2 汇总与 K 值（018）

- 已有数据库部署应用前运行 `018_cell_ocv_summary.sql`；该迁移兼容 SQL Server 2008 R2，可重复运行。现代新库由 `001_create_ocv_data_schema.sql` 和 `002_create_core_procedures.sql` 创建相同表和过程；SQL Server 2008 R2 新库使用根目录完整安装脚本。
- `dbo.CellOcvSummary` 按 trim 后的 `BatteryCode` 一行保存 OCV0/1/2 电压、最终内阻、阶段采集时间、K1/K2。单位分别为 V、mΩ、时间戳和 V/hour。缺阶段或时间差小于等于零时对应 K 为 NULL。
- `usp_SaveTestResults` 在现有条码锁和事务内更新明细与汇总；NG 复测只有 `@OverwriteOriginal=1` 覆盖当前阶段结果时同步汇总。普通复测审计不更改汇总。
- 升级不会从 `TestResult` 回填历史阶段：该表以条码为身份跨阶段覆盖，旧阶段值无法可靠恢复。汇总从升级后新保存的 OCV 阶段结果开始累计。
- 执行 `019_verify_cell_ocv_summary_rollback.sql` 前先部署 018（或现代 001/002），并仅在一次性验证库运行。用例验证 OCV0/1/2 累积、小时差 K1/K2、同阶段覆盖重算、非正时间差、查询接口；全部夹具事务回滚。
