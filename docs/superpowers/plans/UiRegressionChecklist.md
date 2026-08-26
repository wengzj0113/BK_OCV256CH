# OCV UI V2.0 回归基线清单

## 基线环境

- 项目目录：`D:\00 徐力电子\04 技术\XL-OCV-256CH`
- 基线日期：2026-08-13
- 目标 UI：`frmTest`
- 保护备份：`D:\00 徐力电子\04 技术\XL-OCV-256CH-backup-clean-20260813-090854`
- 编译工具：Visual Studio 2022 Build Tools，MSBuild 17.14.40。

## 现有代码入口

- 主窗体：`frmTest.vb`
- 主容器：`MDIForm.vb`
- 通道绘制：`FreshBat()`
- 布局方法：`ApplyFrmTestLayout()`
- 启动测试：`CmdStart_Click`
- 停止测试：`btnStop_Click`
- NG 复测：`btnNGTest_Click`
- 仪器调零：`btn_ZeroAdjustment_Click`
- 通道校准：`btn_ChCalibrate_Click`
- 托盘扫码：`btnScanTray_Click`

## 操作基线

| 操作 | 预期 | 实测 | 结果 |
| --- | --- | --- | --- |
| 程序启动 | 登录/主窗体正常出现 | 待在开发机记录 | 待验证 |
| 数据库连接 | 连接状态与当前版本一致 | 待在开发机记录 | 待验证 |
| 仪器连接 | 电压表、内阻仪状态正常 | 待在开发机记录 | 待验证 |
| 托盘扫码 | 托盘号和首尾电池位置正常更新 | 待在开发机记录 | 待验证 |
| 普通 OCV 测试 | 测试流程和结果与当前版本一致 | 待在开发机记录 | 待验证 |
| 停止测试 | 测试安全停止，状态正常 | 待在开发机记录 | 待验证 |
| NG 复测 | 仅复测 NG 通道，结果正常 | 待在开发机记录 | 待验证 |
| 仪器调零 | 调零流程正常 | 待在开发机记录 | 待验证 |
| 通道校准 | 校准流程和结果正常 | 待在开发机记录 | 待验证 |
| MES 上传 | 上传流程不受 UI 改造影响 | 待在开发机记录 | 待验证 |
| 1920×1080 | 布局完整、256 通道可见 | 待在开发机记录 | 待验证 |
| 较小窗口 | 无重叠，通道顺序不变 | 待在开发机记录 | 待验证 |

## 自动化/本机验证结果

| 项目 | 结果 | 证据 |
| --- | --- | --- |
| UI 改造版编译 | 通过 | `ui-build\\XL_OCV_256CH.exe`，MSBuild 退出码 0 |
| UI 改造版启动冒烟 | 通过 | 独立进程启动后保持响应，主窗口为 `FrmLoginNew`，随后正常关闭 |
| 独立构建版窗口自动化捕获 | 未完成 | Windows 自动化桥接未及时准备，未将宿主窗口截图误作目标 UI 证据 |
| 256 Tile 静态结构 | 通过 | `ChannelGridControl` 使用 16×16 循环并生成 256 个通道编号 |
| 通道索引映射 | 通过 | 沿用 `(column - 1) * 16 + row` 列优先映射 |
| 业务事件入口保留 | 通过 | 启动、停止、NG 复测、调零、校准、扫码 `Handles` 仍存在 |
| 核心业务文件未改动 | 通过 | 与日期备份 SHA-256 对比一致 |

## 现场验证状态

以下项目不能在当前无设备/数据库连接的环境中自动完成，必须由现场开发机执行并填写“实测”和“结果”：

- 数据库连接和写入。
- PLC、中位机、电压表和内阻仪连接。
- 托盘扫码。
- 普通 OCV 测试、停止测试和 NG 复测。
- 仪器调零、通道校准和 MES 上传。
- 1920×1080 与较小窗口的实际视觉检查。

## 当前阻塞说明

- 当前工作区仍有旧版 `bin\\Debug\\XL_OCV_256CH.exe` 实例运行，未强制关闭，以避免影响用户现有工作。
- 新版独立输出位于 `ui-build\\XL_OCV_256CH.exe`，已通过编译和进程级启动冒烟。
- 真实业务回归需要在具备有效 `Initsetting`、数据库、PLC、中位机、34465A/3562A 和 MES 环境的开发机或现场执行。

## 数据库诊断结果（2026-08-13）

- 本机 `MSSQLSERVER` 服务状态为 Running。
- `127.0.0.1:1433` TCP 测试成功。
- 使用 `bin\\Debug\\Initset.set` 中的 SQL 登录配置连接 `OCV_DATA` 成功，能够执行 `SELECT DB_NAME()`。
- 使用同一配置连接旧库 `CWSQL` 失败，原因是登录所请求的数据库不可打开。
- `Initset.set` 中的 `chkDatabase` 当前为 False；这属于应用配置开关，不代表 SQL Server 服务不可用。
- 结论：当前数据库环境可用于 `OCV_DATA` 相关验证；旧 `CWSQL` 路径不可用，且不应作为新版主流程验收依据。

## Task 1 完成记录

- 已完成日期备份：是
- 已确认源码和项目文件在干净备份中：是
- 已完成 UI 改造后编译：是，使用 `MP-60A-RV-64Mem.sln`、`Debug|Any CPU`，独立输出到 `ui-build\`。
- 编译结果：成功生成 `ui-build\XL_OCV_256CH.exe`；存在项目原有 System.Net.Http/System.Runtime 等依赖版本冲突警告，无编译错误。
- 仍需现场/设备环境验证：数据库、设备连接、普通测试、停止、NG 复测、调零、校准和 MES 流程。

## Task 10：frmTest 左侧面板布局修复记录（2026-08-16）

发现的问题（frmTest.Designer.vb 左侧 Panel1 内）：

1. `GroupBox4`「托盘控制」自初始提交起就塌缩为 25×40，且位于 `GroupBox7`（事件日志）内部，被其遮挡，整个托盘控制区（托盘状态/开启托盘/闭合托盘按钮 + txtInfo 状态显示）从未可见。
2. `probeProgress`（探针寿命进度条）位于 y=628，与 `GroupBox8`「探针使用次数统计」底边（y=640）重叠，自动缩放后实际重叠约 16px。

修复（仅改 frmTest.Designer.vb，未动业务逻辑）：

- `GroupBox4`：位置/尺寸 `(25,647) 25×40` → `(2,664) 366×144`，恢复完整托盘控制区。
- `txtInfo`：加宽 263 → 352 匹配 366 宽面板。
- `probeProgress`：y=628 → y=646，落在 GroupBox8 下方独立间隙，不再重叠。
- `GroupBox7`：y=646 → y=814，为 GroupBox4 让位。
- `Panel1.AutoScrollMinSize`：910 → 1080，容纳重排后的内容高度。

回归验证：

| 项目 | 结果 | 证据 |
| --- | --- | --- |
| 独立构建 | 通过 | `ui-fix-build\XL_OCV_256CH.exe` 编译成功 |
| 左侧面板重叠检查 | 通过 | 新增 `tools/Test-LeftPanelBounds.ps1`：8 个 GroupBox 互不重叠，GroupBox4=427×204，probeProgress 位于 GroupBox8 下方，txtInfo 完整落在 GroupBox4 内 |
| 响应式布局 | 通过 | `Test-ResponsiveTestLayout.ps1` 全分辨率 PASS |
| 通道网格适配 | 通过 | `Test-ChannelGridRuntime.ps1` / `Test-ChannelGridFit.ps1` PASS |
| MDI 状态栏预留 | 通过 | `Test-MdiStatusLayout.ps1` PASS |

遗留观察（非布局范围）：`btnOpen`/`btnClose`/`btnTrayState` 在 frmTest 中无 Click 事件处理（仅测试期间切换 Enabled），托盘按钮目前为占位控件；如需启用请另行接线。

## Task 11：P0 结构性布局修复（2026-08-16，UI 分析报告）

针对 UI 分析报告的 P0 问题（顶部遮挡/底部裁切/行号丢失/左侧首屏），根因与修复：

### 1. MDI 客户区被菜单栏覆盖（根因）

- **根因**：`MDIForm.ApplyMdiClientLayout` 手动设置 `mdiClient.Dock=None` + `SetBounds`，而 Load 阶段 `mdiClient.Top` 尚未被布局到菜单下方（恒为 0），导致客户区顶到 y=0，菜单栏覆盖其顶部。
- **实测**（显示态 1920×1080 修复前）：`menu=[0..46] mdi=[0..1001]`，topOk=False。
- **修复**：`MDIForm.vb` 改为让 MdiClient 保持 `Dock=Fill`，交给 WinForms 原生 MDI 布局夹在菜单栏与底部状态栏之间。
- **实测**（修复后）：`menu=[0..46] mdi=[46..969] status=[969..1009]`，topOk/botOk=True。

### 2. 矩阵第 15/16 行被裁切

- **根因**：最大化 MDI 子窗体 frmTest 被留在比 mdiClient 更高的客户区高度（实测 1061 vs 919，AutoScale/MDI 时序所致），矩阵底部越过状态栏被裁。
- **修复**：`frmTest.vb` 新增 `FitToMdiClient()`（Resize 时强制执行 + Load 末尾 BeginInvoke 兜底），让 frmTest 客户区始终跟随 mdiClient。
- **实测**：frmTest.client=1916×919=mdiClient；最后一行 (15,15) 屏幕 y=[927..972]，状态栏顶 992，完整可见。

### 3. 矩阵行号不可见

- **根因**：`ChannelGridControl` 的 rowHeader TableLayoutPanel 未声明 ColumnStyles，单列按内容撑到 117px，而外层布局只给 4%（56px），行号右溢被矩阵主体覆盖、文字被裁成碎片。
- **修复**：行号/状态竖条改固定 44px 排水沟（`SizeType.Absolute, 44`，不属于 tile 行列），rowHeader 显式 `ColumnStyle(Percent,100)`。
- **实测**：行号列任意分辨率下 51px、标签 45px 完整可见，无溢出、无覆盖。

### 4. 左侧首屏不置顶

- **修复**：`ResponsiveTestLayout.Apply` 末尾 + `frmTest_Load` BeginInvoke 双保险，强制 `Panel1.AutoScrollPosition=(0,0)`，首屏从工位选择/托盘信息开始。

### 回归验证

| 项目 | 结果 |
| --- | --- |
| `Test-MdiGridPlacement.ps1`（新增，运行时） | PASS：menu/mdi/status 三段正确、frmTest 跟随 mdiClient、最后一行在状态栏上方 |
| `Test-MdiStatusLayout.ps1`（更新） | PASS：MdiClient 保持 Dock=Fill，不做手动 SetBounds |
| `Test-ChannelGridFit.ps1`（更新） | PASS：tile 行列禁止 Absolute，行号排水沟固定 44px |
| `Test-ResponsiveTestLayout.ps1` / `Test-ChannelGridRuntime.ps1` / `Test-LeftPanelBounds.ps1` | 全部 PASS |

> 注：`Test-ResponsiveTestLayout.ps1`、`Test-ChannelGridRuntime.ps1`、`Test-MdiGridPlacement.ps1` 支持 `$env:FRMTEST_EXE` 指定构建产物路径。
> 注：`Test-MdiGridPlacement.ps1` 需真实显示 MDI 窗体并等待 frmTest 完成 Load（socket/DB 超时约 13s）；若 frmTest_Load 弹出模态 MessageBox（如数据库连接失败），探针会阻塞，只适合人工/长时回归，不属于快速套件。

### 修复记录：首次打开右栏错位（重新最大化才恢复）

- **现象**：frmTest 首次打开时右侧统计区/矩阵布局错位，取消最大化再最大化后基本正常。
- **根因**：`ResponsiveTestLayout.Apply` 先设 `Dock=Fill` 再设 `Anchor=all`。WinForms 中 **Dock 与 Anchor 互斥**——设置 Anchor 会把 Dock 重置为 `None`。实测修复前 `statisticsControl/channelGrid/Panel1` 的 Dock 全部为 `None`。Dock=None 时控件只在首次布局按当时窗体尺寸计算，不保证随后续缩放重新填充单元格，需一次完整 re-maximize 触发重排才恢复。
- **修复**：移除 `ResponsiveTestLayout.Apply` 中三处 `Anchor = ...` 赋值，保留 `Dock=Fill`。实测修复后三控件 `Dock=Fill`，任意窗体尺寸下始终填充单元格。
- **验证**：`Test-ResponsiveTestLayout`（多分辨率）、`Test-ChannelGridRuntime`、`Test-LeftPanelBounds` 全部 PASS。

## Task 8 编译记录

执行命令：

```powershell
& "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" `
  "MP-60A-RV-64Mem.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" `
  /p:OutputPath="ui-build\" /p:BaseOutputPath="ui-build\" /m /v:minimal
```

结果：退出码 0，生成 `ui-build\XL_OCV_256CH.exe`。
