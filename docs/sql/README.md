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
2. `001_create_ocv_data_schema.sql` — 创建所有表、索引、约束、视图、用户定义表类型(17 张表 + 1 视图 + 2 UDDT)、种子数据(OcvStage / GradeSelectableField)。幂等,缺失对象才创建。
3. `002_create_core_procedures.sql` — 创建所有存储过程(`usp_RegisterTray` / `usp_CreateTestTask` / `usp_SaveTestResults` / `usp_CompleteTestTask` / `usp_GetActiveCalibrationValues` / `usp_SaveChannelCalibration` / `usp_EnqueueUpload` / `usp_GetPendingUploads` / `usp_MarkUploadSent` / `usp_MarkUploadFailed` / `usp_SaveMesUploadLog` / `usp_GetTrayListByDate` / `usp_GetTrayLatestStageResults` / `usp_GetNgRetestSourceByBatteryCode` / `usp_CheckDatabaseHealth` / `usp_GetSchemaVersion` 等)。幂等,使用 `CREATE OR ALTER`。

## 业务数据迁移(R2-15,旧 CWSQL → 新 OCV_DATA)

> 重要:此步骤会从旧库 `CWSQL` 读取历史数据,需要 `CWSQL` 数据库仍可访问,且 `CWSQL` 与 `OCV_DATA` 在同一实例。如果跨实例,需要先用 SSMS 的"导出数据"或 Linked Server 桥接。

4. `006_migrate_unsent_data_to_upload_outbox.sql` — 把 `CWSQL.UnsentData (Is_Sent=0)` 和 `CWSQL.PendingUploads` 报迁到 `OCV_DATA.UploadOutbox`。存储过程 `usp_MigrateLegacyUnsentDataToUploadOutbox` 支持 `@DryRun=1` 演练模式,先 DryRun 确认条数,再改为 0 正式执行。
5. `007_migrate_grade_rules.sql` — 把 `CWSQL.CW_tblCondition` + `CWSQL.CW_tblSelFld` 报迁到 `OCV_DATA.GradeScheme` + `GradeRule` + `GradeSelectableField`。存储过程 `usp_MigrateLegacyGradeRules` 支持 `@DryRun=1` 演练模式,字段名从旧全限定列名(`CW_tblData_Info.Fld_VolA`)反向映射为新字段名(`VoltageVolt`)。

## 迁移后验证

6. `008_post_migration_health_check.sql` — 8 项检查:SchemaVersion 完整性、必备存储过程、关键表行数、OcvStage 种子数据、迁移标识、索引、视图、`usp_CheckDatabaseHealth` 综合检查。**所有检查通过后才能切换应用连接**。

## SQL Server 2008 R2 完整导出脚本

- `011_seed_ocv_data_sqlserver2008r2_baseline.sql` — 在 `OCV_DATA_SQLServer2008R2.sql` 成功创建表和存储过程后运行。该兼容脚本补齐导出文件不包含的 OCV0/OCV1/OCV2 阶段、等级字段及 001/002 版本记录，并在末尾执行数据库健康检查。可重复运行。
- `012_debug_mode_result_queries_sqlserver2008r2.sql` — 在已部署的 SQL Server 2008 R2 `OCV_DATA` 上安装调试/生产结果隔离查询和 NG 来源过滤。该脚本只重建两个存储过程，不改表和业务数据；将调试模式应用版本部署到已有 2008 R2 数据库前必须运行。全新部署仍使用根目录完整安装脚本，该脚本也包含相同过程定义。

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
