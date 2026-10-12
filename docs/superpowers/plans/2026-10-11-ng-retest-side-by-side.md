# NG 复测页面并列对照实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use `subagent-driven-development` (recommended) or `executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 把 NG 复测页面改成历史数据与本次测量并列显示，支持无历史时只采集显示的调试模式，并确保覆盖写回扫码查询到的确切测试记录。

**Architecture:** `FrmNgRetest` 管理页面状态和交互；`NgRetestDataService` 区分无记录与查询错误并按来源 ID 保存；`RetestModels` 传递来源身份和一次采集的幂等 ID。数据库同时维护新装库脚本和既有库迁移脚本，正式复测记录写审计表，调试采集完全绕过数据库。

**Tech Stack:** VB.NET WinForms、ADO.NET、SQL Server（包括 SQL Server 2008 R2）、T-SQL。

---

## 文件与职责

- 修改 `FrmNgRetest.vb`：并列卡片、扫码查询状态、调试开关、复测采集、保存和失败重试交互。当前窗体通过代码动态创建控件，没有独立 Designer 文件。
- 修改 `RetestModels.vb`：来源 `TestResultId` 和保存请求幂等 ID。
- 修改 `NgRetestDataService.vb`：读取精确来源 ID；以无记录返回值表示查无历史，数据库异常继续抛出；正式来源才能调用保存。
- 修改 `OCV_DATA_SQLServer2008R2.sql`：同步新装 SQL Server 2008 R2 数据库的复测表和两个存储过程。
- 修改 `docs/sql/001_create_ocv_data_schema.sql`、`docs/sql/002_create_core_procedures.sql`：纳入现代 SQL Server 新装库的审计表、来源查询和保存过程；当前现代 schema 缺少 `OCV_RETEST_DATA`，当前 core procedures 也缺少 `usp_SaveNgRetest`。
- 新建 `docs/sql/014_ng_retest_source_identity.sql`：供已部署数据库升级；使用 SQL Server 2008 R2 可解析的语法，幂等补齐表/列/索引和两个过程。
- 修改 `docs/sql/README.md`：明确既有库执行 014 的时机，以及新装库通过 001/002 或根目录 2008 R2 安装脚本获得相同定义。
- 不改 `Retest3562Service.vb`、项目文件和其他测试/生产页面；本计划不新增或运行测试。

## Task 1：让数据库查询和保存锁定同一条来源记录

**Files:**
- Modify: `OCV_DATA_SQLServer2008R2.sql`
- Modify: `docs/sql/001_create_ocv_data_schema.sql`
- Modify: `docs/sql/002_create_core_procedures.sql`
- Create: `docs/sql/014_ng_retest_source_identity.sql`
- Modify: `docs/sql/README.md`

- [x] **Step 1：给复测审计表加入来源 ID 和操作幂等 ID**

新建表和根目录完整安装脚本的 `dbo.OCV_RETEST_DATA` 定义包含：

```sql
[SourceTestResultId] [bigint] NULL,
[RetestOperationId] [uniqueidentifier] NULL,
```

SQL 2008 R2 及现代 schema 均为 `RetestOperationId` 建过滤唯一索引，过滤条件为 `RetestOperationId IS NOT NULL`。既有表通过 014 以 `COL_LENGTH` 检查后再加可空列；旧审计行不回填。014 若发现审计表不存在，应创建包含现有列和这两列的完整表。

- [x] **Step 2：使 NG 来源查询返回来源主键**

根目录安装脚本、现代 `002` 和 014 的 `usp_GetNgRetestSourceByBatteryCode` 使用同一查询逻辑。不要从当前未暴露 `TestResultId` 的 `vw_CellTestSummary` 推导身份；直接连接 `TestResult`、`TestTask`、`Tray` 和 `TrayCell`，取 `TOP (1)` 最近正式 NG 行，并返回 `tr.TestResultId`、条码、托盘、通道、托盘位置、历史电压/内阻、判定和时间。条码映射沿用 `COALESCE(tr.BatteryCode, tc.BatteryCode)`；排除 `TestType = N'Debug'`；排序用 `tr.CreatedAt DESC, tr.TestResultId DESC`。托盘位置继续优先用 `TrayCell.RowNo/ColNo`，缺失时由通道号按 16 列计算。

- [x] **Step 3：让保存过程只覆盖查询所得来源并保证审计原子性**

`usp_SaveNgRetest` 接收 `@SourceTestResultId bigint` 和 `@RetestOperationId uniqueidentifier`。事务中先以 `UPDLOCK, HOLDLOCK` 查找同操作 ID；已存在则直接成功返回，防止保存响应丢失后重试产生第二条审计记录。覆盖时按来源 ID 更新，并同时校验条码、托盘号、通道号及原行仍为 NG；更新行数不是 1 时回滚并报错。未勾选覆盖时不更新 `TestResult`。两种正式路径都写一条包含 `SourceTestResultId` 与 `RetestOperationId` 的 `OCV_RETEST_DATA` 记录。SQL 2008 R2 过程使用 `TRY/CATCH`、`XACT_ABORT` 和 `RAISERROR`，不用 `THROW` 或 `CREATE OR ALTER`。

- [x] **Step 4：新增可重复运行的既有库迁移并记录部署步骤**

014 在 SQL 2008 R2 可执行：先创建缺失审计表、再补列和过滤唯一索引、最后用兼容的 create-stub/`ALTER PROCEDURE` 形式安装查询和保存过程。更新 README：SQL 2008 R2 既有库在部署应用前运行 014；现代库在 001/002 新装流程中获得相同对象，既有库也运行 014。根目录完整安装脚本保持独立可新装，不要求现场重跑完整建库脚本。

## Task 2：在应用服务中保留来源身份并明确“查无记录”

**Files:**
- Modify: `RetestModels.vb`
- Modify: `NgRetestDataService.vb`

- [x] **Step 1：在数据模型中传递精确来源和重试身份**

在 `NgRetestSource` 增加 `SourceTestResultId As Long`；在 `NgRetestSaveRequest` 增加 `RetestOperationId As Guid`。保存请求在一次采集后创建一次，保存失败重试时复用同一个对象和操作 ID。

- [x] **Step 2：让查询把“没有行”与 SQL 错误分开**

`LoadByBatteryCode` 在 reader 无行时返回 `Nothing`，有行时映射 `TestResultId` 到来源模型。连接、超时、权限或 SQL 错误继续抛出原异常；不要把异常转换成 `Nothing`。保留当前 NG 判定校验和位置回退算法。

- [x] **Step 3：把来源 ID 和幂等 ID 作为保存参数传给过程**

`SaveRetest` 对正式来源仍调用 `dbo.usp_SaveNgRetest`，新增两个参数：

```vb
command.Parameters.Add("@SourceTestResultId", SqlDbType.BigInt).Value = request.Source.SourceTestResultId
command.Parameters.Add("@RetestOperationId", SqlDbType.UniqueIdentifier).Value = request.RetestOperationId
```

无历史调试路径不得调用 `SaveRetest`；服务收到 `Source Is Nothing` 时立即拒绝，避免绕过窗体约束写库。

## Task 3：实现并列页面和无历史调试状态

**Files:**
- Modify: `FrmNgRetest.vb`

- [x] **Step 1：用动态布局搭建左右数据卡片**

保留顶部条码、查询按钮和 3562 状态；在主体用 `TableLayoutPanel` 两列各占 50%，左卡片显示历史 NG 判定、电压、内阻、测试时间，右卡片显示本次判定、电压、内阻、采集时间。下方放“调试模式（无历史时仅采集显示）”“覆盖历史”、复测按钮和仅在保存失败时可用的“重试保存”按钮。设置能容纳两列的初始尺寸和最小尺寸；页面缩放时卡片随窗体扩展。初始化时覆盖和调试均不勾选。

- [x] **Step 2：按条码重置界面状态并区分查询结果状态**

新增窗体级查询状态 `NotQueried / Found / NotFound / Failed`。每次 Enter 或点击查询开始时，清空上一条的来源、测量值、待保存请求；取消覆盖与调试勾选；清空左右卡片；暂时禁用复测、调试和重试。

查询返回来源时设 `Found` 并显示条码、托盘/通道/位置及历史指标。返回 `Nothing` 时设 `NotFound`，左卡片显示无 NG 历史，允许勾选调试但仍禁用复测。数据库异常设 `Failed`、显示查询错误并禁用调试和复测。仅在 `NotFound` 状态勾选调试后启用复测。

- [x] **Step 3：实现正式复测、自动覆盖和调试只显示**

点击复测后采集一次并先显示右侧结果。若 `Found`，建立一个含 `Guid.NewGuid()` 的 `NgRetestSaveRequest` 并自动调用保存，不再弹覆盖确认；覆盖复选框默认关闭，关闭时仅写审计，打开时更新确切来源。若 `NotFound And chkDebugMode.Checked`，只显示结果与“调试采集完成 · 仅显示未入库”，不调用任何 SQL 服务，也不写 `BusinessLogService` 数据库日志。其他状态不允许开始测量。保持测量异常时不保存，并在右卡片显示错误。

- [x] **Step 4：实现保存失败后的同次结果重试**

正式保存失败时保留右侧测量值和原 `NgRetestSaveRequest`，启用“重试保存”，不再次读仪表。重试复用 `RetestOperationId`；成功后禁用重试并显示“已记录复测/已覆盖历史”。保存失败提示明确表示结果未确认保存。调试采集永远不显示保存重试入口。

- [x] **Step 5：保持忙碌状态和操作日志与新状态一致**

查询/采集/保存期间禁止修改条码、重复查询、切换模式和二次采集。恢复控件时根据 `Found`、`NotFound`、`Failed`、仪表连接状态和保存状态分别计算按钮可用性。仅正式查询/保存写现有业务操作日志；调试测量不落任何数据库表。

## Task 4：对照设计说明收尾

**Files:**
- Review: `docs/superpowers/specs/2026-10-11-ng-retest-side-by-side-design.md`
- Review: `docs/sql/README.md`

- [x] **Step 1：逐项核对实现范围与已确认规则**

检查页面初始化和新条码是否默认不勾覆盖；Enter 是否自动查；左右区域是否分别显示历史和复测电压/内阻；覆盖是否无二次确认且按来源 ID 更新；无历史调试是否只采集显示；SQL 异常是否不开放调试；非覆盖正式结果是否仅新增审计行；两个安装路径是否都有必需 SQL 对象。若代码或 SQL 部署说明与设计说明不一致，先修正实现/部署文档，不扩展到正式测试、PLC、仪表判定或调试数据持久化。

## 设计覆盖自检

- 并列历史/复测指标、托盘位置和扫码自动查：Task 3。
- 覆盖默认不勾、自动保存、确切来源 ID、失败回滚：Tasks 1–3。
- 无历史的调试采集只显示不入库；数据库异常不放行：Tasks 2–3。
- 新旧数据库部署脚本保持一致：Task 1。
- 未改仪表测量协议、判定规则、PLC 或正式测试流程：文件范围中明确排除。
