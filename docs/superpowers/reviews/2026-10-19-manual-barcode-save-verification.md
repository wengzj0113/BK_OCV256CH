# 手动测试条码与按条码保存验证记录

日期：2026-10-19

分支：`codex/manual-barcode-save`。实现保留在工作区，涉及文件原有的未提交修改已保留；未提交或部署应用代码，未连接应用配置的现场设备、MES 或数据库。

## 实现

- `ManualTestData.vb`：托盘条码准备、带托盘身份的稳定模拟码、固定测试上下文、独立轮次测量快照和本地保存服务。
- `ManualTestExecution.vb`：取码/回车扫码、运行保护、单次与多次测量、部分失败、一次保存确认、按配置上传最后完成的真实条码轮次。
- `FrmManualTest.Designer.vb`：方案 A 紧凑工具栏、条码来源状态、结果条码列；保留设计器编辑能力。
- `ManualTestWorkbookWriter.vb`：每轮和汇总增加电芯条码，写入前设置文本格式。
- `ResultDataUploadAssembly.vb`：显式测量快照、真实 NG 状态、冻结的操作员/工艺/时间、HTTP 结果处理和待传队列状态；兼容原有后台入口并拦截其异步异常。
- `docs/sql/016_barcode_result_upsert.sql`：全局按归一化电芯条码更新当前结果、按需合并旧重复、保留结果编号、维护复测来源引用、事务锁与错误回滚。现代/旧版完整安装脚本和 015 已对齐。

## 已通过

1. x86 Debug 项目构建，退出码 0：
   ```powershell
   & 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /p:OutputPath=bin\build-verify-manual-barcode\ /verbosity:minimal /nologo
   ```
2. `tools/Test-ManualBarcodeRuntime.ps1`：58 个断言通过。覆盖条码空/部分/重复/非法映射、模拟码稳定性与托盘区分、轮次快照隔离、校准与判定、稀疏 MES 载荷、模拟码上传拦截、上传/队列双失败边界，以及默认/最小窗口下工具栏可见范围。
3. 隐藏窗口渲染已检查；预览为 `bin/build-verify-manual-barcode/manual-form-preview.png`，没有连接设备或启动主程序测试流程。
4. `tools/Test-ManualResultPersistenceRuntime.ps1`：实际 x86 应用程序集在专用 LocalDB 完成两轮真实条码保存，生成两个 Completed Manual 任务、只保留一条最新 ManualTest 结果，最终电压、原始内阻、校准和最终内阻正确。模拟结果保存为 Debug，未注册模拟 TrayCell。
5. `docs/sql/017_verify_barcode_result_upsert_rollback.sql`：重复保存、跨任务/阶段/托盘/来源更新、条码和通道交换、按需合并/未触及旧重复保留、复测关联重定向、延迟审计、历史 NULL/空白条码回退，以及非法输入和晚期触发器错误的整事务回滚均通过。
6. SQL 测试还覆盖现代与旧版脚本全新安装、015→016→016 重复部署、升级不删除已有历史、普通/覆盖复测/审计保存共用锁的并发超时与无遗留事务。
7. `Test-DebugBarcodePolicyRuntime.ps1` 与 `Test-TrayChannelMappingRuntime.ps1` 对本次构建通过。
8. 规格审核及代码质量审核通过；审查发现的复测悬空引用、历史空条码回退、部分失败状态丢失和后台 Timer 异常传播均已修复并回归验证。
9. 修改文件 `git diff --check` 通过。SQL 定义在 002、016、完整旧版安装脚本之间一致，015 复测定义也已同步。
10. `tools/Test-ManualWorkbookRuntime.ps1` 使用实际 x86 writer 连续写入三轮并重新打开工作簿，四个工作表的条码、前导零、长数字及公式样式文本均原样保留，汇总轮数/最小值/最大值/偏离度正确。该检查发现并修复了原有首轮释放汇总工作表 COM 对象、导致第二轮无法写入的问题；修复后 writer 和重新打开的 Excel 均正常清理。

运行检查使用 32 位 Windows PowerShell，以匹配 x86 .NET Framework 应用。数据库测试只使用专用命名 LocalDB `OcvBarcodeVerify20261019`，已清除测试数据、删除数据库并移除该测试实例；未使用默认 LocalDB 或现场连接配置。

## 部署与现场检查

- 已有 OCV_DATA 需要按 `docs/sql/README.md` 执行前置 014、015 及本次 016，再使用新程序；本次没有对现场库执行升级。
- SQL 验证引擎为 SQL Server 2019 LocalDB、兼容级别 100；SQL Server 2008 R2 实际引擎仍需部署验收。
- 真实仪表测量、扫码枪回车输入及客户 MES 联调需要现场环境；本次 MES 使用固定快照和模拟 HTTP 响应验证，没有发送现场测量数据。
