# 日志查询页面布局调整实施计划

**目标：** 日志查询窗体启动时最大化，并将日志文件列表与日志内容区域按 1:3 比例布局。

**方案：** 保留现有 `SplitContainer`。窗体加载时设置最大化；窗体尺寸变化时按当前可用宽度的 25% 设置 `SplitterDistance`，右侧自然占 75%，同时保留用户拖动分隔条的能力。

**验证：** 使用现有日志查询静态检查，并执行 Debug 编译，要求 0 个错误、0 个警告。

## 修改步骤

- [ ] 在 `FrmLogQuery.vb` 的窗体加载流程中设置 `WindowState = Maximized`。
- [ ] 在 `FrmLogQuery.vb` 增加尺寸变化处理，将分隔条限制为客户区宽度的 25%。
- [ ] 在 `FrmLogQuery.Designer.vb` 保留 SplitContainer 的停靠和最小宽度配置。
- [ ] 运行 `tools\verify-log-query.ps1`。
- [ ] 使用 MSBuild 编译解决方案并确认无警告、无错误。
