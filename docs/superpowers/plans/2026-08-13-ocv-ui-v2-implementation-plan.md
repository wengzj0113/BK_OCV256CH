# OCV 上位机 UI V2.0 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task with verification checkpoints.

**Goal:** 在不修改 OCV 测试、256 通道逻辑、数据库、设备通讯和测试流程的前提下，完成 `frmTest` 首页的现代化 UI 重构。

**Architecture:** 采用渐进式 WinForms 改造。第一阶段保留 `frmTest` 现有业务事件和 `FreshBat()` 数据来源，在其上增加统一主题、统计展示、通道 Tile 和状态栏控件；通道显示通过 UI 映射读取现有状态，逐步从整图刷新转为局部更新。业务层、通讯层和数据库层不改动。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.x、现有 `MP-60A-RV-64Mem.vbproj`、System.Drawing、System.Windows.Forms。

---

## 文件地图

| 文件 | 责任 |
| --- | --- |
| `frmTest.vb` | 保留测试流程，接入主题、统计、通道矩阵和布局更新 |
| `frmTest.Designer.vb` | 调整现有控件布局和新增 UI 控件声明 |
| `MDIForm.vb` | 保留菜单事件，接入顶部导航和底部状态展示所需的现有状态 |
| `MDIForm.Designer.vb` | 调整主窗体导航和状态栏外观 |
| `UiTheme.vb` | 集中管理颜色、字体、间距和控件样式 |
| `ChannelTileControl.vb` | 单个通道 Tile 的显示、状态颜色、Tooltip 和点击事件 |
| `ChannelGridControl.vb` | 动态生成 16×16 Tile、坐标头、按通道局部更新 |
| `StatisticsControl.vb` | 总数、PASS、NG、测试中、空闲统计卡片 |
| `BottomStatusControl.vb` | 工位、设备、数据库、MES、用户和时间状态展示 |
| `UiRegressionChecklist.md` | 记录手工回归步骤、预期结果和实测结果 |

现有 `.vb` 文件命名风格优先使用项目当前风格；新增文件必须加入 `MP-60A-RV-64Mem.vbproj` 的 `<Compile Include="..." />`。

## Task 1: 建立可回滚基线和编译入口

**Files:**
- Create: `docs/superpowers/plans/UiRegressionChecklist.md`
- Read-only check: `MP-60A-RV-64Mem.vbproj`, `frmTest.vb`, `frmTest.Designer.vb`, `MDIForm.vb`, `MDIForm.Designer.vb`
- Backup target: 工作区外的日期目录，例如 `D:\00 徐力电子\04 技术\XL-OCV-256CH-backup-20260813`

- [ ] **Step 1: 建立项目备份**

  先确认源目录为 `D:\00 徐力电子\04 技术\XL-OCV-256CH`，再将整个项目复制到带日期的兄弟目录。禁止覆盖现有备份目录；若目标已存在，使用新的时间后缀。

- [ ] **Step 2: 记录当前 UI 和核心流程基线**

  在 `UiRegressionChecklist.md` 中记录：程序启动、数据库连接、仪器连接、扫码、普通测试、停止测试、NG 复测、调零、校准、MES 上传、最大化和缩放行为。每项包含“操作、预期、实测、结果”四列。

- [ ] **Step 3: 确认可用编译工具**

  查找 Visual Studio/MSBuild 或项目既有编译入口；如果当前环境没有 VB.NET Framework 编译器，不伪造通过结果，改为记录“需要在开发机编译”的验证项，并继续做静态检查。

- [ ] **Step 4: 记录基线完成**

  在清单中记录当前项目是否能编译、首页入口、当前窗口尺寸和 `FreshBat()` 的调用点。

## Task 2: 增加统一 UI 主题

**Files:**
- Create: `UiTheme.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: 新增主题常量模块**

  创建 `Public NotInheritable Class UiTheme`，集中定义以下共享值：

  ```vb
  Public Shared ReadOnly PageBackground As Color = ColorTranslator.FromHtml("#F5F7FA")
  Public Shared ReadOnly CardBackground As Color = Color.White
  Public Shared ReadOnly CardBorder As Color = ColorTranslator.FromHtml("#E5E7EB")
  Public Shared ReadOnly Primary As Color = ColorTranslator.FromHtml("#1677FF")
  Public Shared ReadOnly HeaderBackground As Color = ColorTranslator.FromHtml("#0B2345")
  Public Shared ReadOnly Success As Color = ColorTranslator.FromHtml("#22C55E")
  Public Shared ReadOnly Warning As Color = ColorTranslator.FromHtml("#F59E0B")
  Public Shared ReadOnly Danger As Color = ColorTranslator.FromHtml("#EF4444")
  Public Shared ReadOnly TextPrimary As Color = ColorTranslator.FromHtml("#1F2937")
  Public Shared ReadOnly TextSecondary As Color = ColorTranslator.FromHtml("#6B7280")
  Public Shared ReadOnly UiFont As New Font("Microsoft YaHei UI", 9.0F)
  Public Shared ReadOnly UiFontBold As New Font("Microsoft YaHei UI", 9.0F, FontStyle.Bold)
  End Class
  ```

- [ ] **Step 2: 增加控件样式辅助方法**

  在同一文件中增加 `ApplyCardStyle(Control)`、`ApplyTextBoxStyle(TextBox)` 和 `ApplyButtonStyle(Button, ButtonRole)`；方法只设置外观，不绑定事件、不访问测试数据。

- [ ] **Step 3: 将新文件加入项目**

  在 `MP-60A-RV-64Mem.vbproj` 的 Compile 项中加入 `<Compile Include="UiTheme.vb" />`，保持现有 XML 结构和编码。

- [ ] **Step 4: 静态验证主题使用范围**

  搜索新增代码中的 `Color.FromArgb`、直接设置字体和重复颜色字面量，确保主题新增代码只通过 `UiTheme` 获取颜色和字体。

## Task 3: 重做 `frmTest` 主体布局并保留业务控件

**Files:**
- Modify: `frmTest.vb`
- Modify: `frmTest.Designer.vb`

- [ ] **Step 1: 建立稳定的布局常量**

  在 `frmTest.vb` 增加私有常量：`HeaderHeight = 58`、`BottomStatusHeight = 36`、`LeftPanelWidth = 370`、`OuterMargin = 12`、`CardGap = 10`。这些常量只服务布局，不参与测试判断。

- [ ] **Step 2: 改造 `ApplyFrmTestLayout()`**

  保留现有 `Panel1`、`Pic_Bat` 和 `FreshBat()` 调用关系，但将布局改成：页面背景 `#F5F7FA`，左侧固定约 370px，右侧占用剩余空间，统一外边距和间距。窗口尺寸小于左侧区域加最小通道区宽度时，保留右侧滚动或缩放能力，不能让 `Pic_Bat` 负宽度。

- [ ] **Step 3: 保留并重新排列现有控件**

  不删除 `GroupBox1`~`GroupBox8`、`Panel1`、`Pic_Bat` 或业务按钮；仅调整其 Parent、Bounds、BackColor、Font、Padding、Margin 和 BorderStyle，使其呈现白色 Card。现有 `Handles ...Click` 事件必须保持原名和原绑定。

- [ ] **Step 4: 应用按钮层级**

  将启动、停止、NG 复测、调零和报警复位分别调用主题样式辅助方法。只改变背景、前景、边框和尺寸，不改变 `Enabled` 状态赋值逻辑。

- [ ] **Step 5: 验证布局事件**

  手工或静态确认 `Load`、`Resize`、启动测试、停止测试、NG 复测、调零、校准、扫码事件仍存在；`ApplyFrmTestLayout()` 不调用测试服务和数据库。

## Task 4: 新增顶部通道统计控件

**Files:**
- Create: `StatisticsControl.vb`
- Modify: `frmTest.vb`
- Modify: `frmTest.Designer.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: 定义统计数据结构**

  在 `StatisticsControl.vb` 定义 `Public Structure ChannelStatistics`，包含 `Total`、`PassCount`、`NgCount`、`TestingCount`、`IdleCount` 五个整数，并提供只读百分比计算属性。

- [ ] **Step 2: 创建统计卡片 UI**

  创建 `StatisticsControl`，包含总数卡和四个状态卡；控件只接受 `ChannelStatistics`，不直接读取 `RecieveData`、数据库或通讯对象。

- [ ] **Step 3: 在 `frmTest` 增加统计容器**

  在 Designer 中声明并初始化 `StatisticsControl`，放置于右侧通道矩阵上方，保持 `Pic_Bat`/新矩阵区域的剩余高度计算。

- [ ] **Step 4: 增加 UI 层统计映射**

  在 `frmTest.vb` 增加 `BuildChannelStatistics()`，仅遍历现有通道数据和已存在的状态判断结果，返回五项统计；不得修改通道状态，不得触发测试。

- [ ] **Step 5: 接入已有刷新点**

  在窗体加载、测试结果完成和现有通道刷新点调用统计控件更新；避免在高频循环中重复重建整个窗体。

## Task 5: 新增 16×16 通道 Tile 和矩阵控件

**Files:**
- Create: `ChannelTileControl.vb`
- Create: `ChannelGridControl.vb`
- Modify: `frmTest.vb`
- Modify: `frmTest.Designer.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: 定义纯 UI 通道显示数据**

  在 `ChannelTileControl.vb` 定义 `ChannelDisplayData`，包含 `ChannelNo`、`Row`、`Column`、`Barcode`、`Ocv1`、`Ir1`、`StatusText` 和 `ResultText`。它只表示显示数据，不替代现有业务数据结构。

- [ ] **Step 2: 实现 Tile 外观和状态颜色**

  `ChannelTileControl` 提供 `UpdateDisplay(ChannelDisplayData)`，根据 PASS、NG、Testing、Waiting、Idle、Disabled、Error 设置整块 Tile 的背景和文字颜色；默认只显示 `row-column` 或现有通道编号。

- [ ] **Step 3: 实现 Tooltip 和点击事件**

  Tooltip 显示通道、条码、OCV1、IR1、状态和结果；点击只触发 `ChannelSelected` 事件，把显示数据交回 `frmTest`，不执行测试、复测或数据库操作。

- [ ] **Step 4: 实现 `ChannelGridControl` 动态生成**

  使用 16 行、16 列循环生成 256 个 Tile，并显示顶部列号和左侧行号。通道映射必须显式使用当前项目的列/行顺序，并以现有 `FreshBat()` 的 `TempNum = intRow * I + J` 为基准核对，不得凭感觉改成另一种顺序。

- [ ] **Step 5: 实现局部更新接口**

  提供 `UpdateChannel(channelNo, data)` 和 `UpdateAll(data)`；高频测试更新调用前者，只有初始化或全量状态切换才调用后者。

- [ ] **Step 6: 在 `frmTest` 增加状态映射**

  增加 `BuildChannelDisplayData(channelNo)`，从现有 `RecieveData` 和已有结果字段生成 UI 数据；保留 `FreshBat()` 业务判断和报警副作用，先以并行显示方式接入新矩阵，再经过回归后逐步减少旧图像绘制依赖。

- [ ] **Step 7: 建立回归检查点**

  验证初始化时正好生成 256 个 Tile；检查 `1-1`、`1-16`、`16-1`、`16-16` 与现有数据索引一致；测试过程中随机改变一个通道时只更新对应 Tile。

## Task 6: 事件日志、探针寿命和底部状态栏

**Files:**
- Create: `BottomStatusControl.vb`
- Modify: `frmTest.vb`
- Modify: `frmTest.Designer.vb`
- Modify: `MDIForm.vb`
- Modify: `MDIForm.Designer.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: 将日志区域改为事件日志视觉**

  保留现有 `txtAlarm` 和 `AppendAlarmText()` 数据路径，调整标题、字体、颜色和滚动策略，使日志呈现时间轴样式。不得删除现有报警文件写入逻辑。

- [ ] **Step 2: 增加探针进度条**

  在探针统计 Card 增加 ProgressBar 或等效绘制控件，使用现有 `txtTotle`、`txtUsed`、`txtRemain` 数值计算使用率并按 80%/95% 阈值切换颜色；不修改计数来源。

- [ ] **Step 3: 实现底部状态控件**

  `BottomStatusControl` 提供 `UpdateStatus(station, plc, voltageMeter, resistanceMeter, database, mes, user, timestamp)`，统一显示中文状态。它不主动连接设备、不查询数据库、不创建计时器以外的业务任务。

- [ ] **Step 4: 接入 `MDIForm` 当前状态**

  从 `MDIForm` 现有状态控件和现有连接判断处更新底部状态控件；保留菜单点击和主窗体生命周期事件。

- [ ] **Step 5: 验证状态显示**

  分别检查已连接、未连接、测试中、报警和用户/时间变化时只更新显示文本，不影响现有连接和测试行为。

## Task 7: 线程边界与局部刷新收口

**Files:**
- Modify: `frmTest.vb`
- Modify: `ChannelGridControl.vb`
- Modify: `StatisticsControl.vb`
- Create: `UiUpdateService.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: 定义 UI 更新入口**

  创建 `UiUpdateService`，提供 `UpdateChannel(channelNo, data)` 和 `UpdateStatistics(statistics)`；如果调用线程不是 UI 线程，使用窗体 `BeginInvoke` 调度，禁止后台线程直接改控件。

- [ ] **Step 2: 替换高频整页刷新调用**

  搜索 `frmMDI.Refresh()`、`Pic_Bat.Refresh()`、`Form.Refresh()` 和测试循环中的整页刷新；只替换 UI 改造新增或明确属于通道高频更新的调用，业务流程中的必要状态更新保持原行为并记录原因。

- [ ] **Step 3: 保留旧绘制作为安全回退**

  在新 Tile 矩阵通过回归前，不删除 `FreshBat()`；增加明确的 UI 模式开关或安全路径，使出现异常时可以回到现有 `Pic_Bat` 显示而不影响测试流程。

- [ ] **Step 4: 做静态安全检查**

  确认 `UiUpdateService` 不引用数据库、通讯、MES 或校准类；确认新控件没有调用 `btnStart_Click`、`btnStop_Click` 等业务事件。

## Task 8: 编译、运行和业务回归

**Files:**
- Modify: `docs/superpowers/plans/UiRegressionChecklist.md`
- Review: `frmTest.vb`, `frmTest.Designer.vb`, `MDIForm.vb`, `MDIForm.Designer.vb`, `UiTheme.vb`, `ChannelGridControl.vb`, `ChannelTileControl.vb`, `StatisticsControl.vb`, `BottomStatusControl.vb`, `UiUpdateService.vb`

- [ ] **Step 1: 执行静态文件检查**

  确认所有新增 `.vb` 文件已加入 `MP-60A-RV-64Mem.vbproj`，所有窗体 Designer 声明与初始化配对，所有 `Handles` 事件仍对应现有控件。

- [ ] **Step 2: 执行项目编译**

  使用开发机可用的 Visual Studio/MSBuild 编译 `MP-60A-RV-64Mem.vbproj` 或其对应解决方案；预期为 0 个编译错误。若当前环境缺少完整 Framework 工具链，记录实际阻塞信息，不将静态检查结果写成编译通过。

- [ ] **Step 3: 执行 UI 回归**

  按 `UiRegressionChecklist.md` 逐项执行，至少覆盖 1920×1080 和一个较小窗口尺寸。记录通道数量、坐标、颜色、统计占比、按钮可用性、日志和底部状态。

- [ ] **Step 4: 执行业务回归**

  在可用设备/模拟环境中执行普通测试、停止、NG 复测、调零、校准、数据库写入和 MES 上传；将结果与 Task 1 基线逐项比较。

- [ ] **Step 5: 检查性能和闪烁**

  测试连续通道更新时，确认没有整页明显闪烁、CPU 异常增长、输入卡顿或通道矩阵错位；如失败，优先回退局部刷新接入，不改业务流程。

- [ ] **Step 6: 形成交付记录**

  在回归清单中记录编译命令、运行环境、实测结果、已知限制和备份目录，只有所有核心流程通过后才标记 UI 阶段完成。

## 计划自检

- UI 主题、主框架、左侧 Card、统计区、16×16 Tile、Tooltip、日志、探针进度、底部状态和局部刷新均有对应任务。
- 现有测试事件、`FreshBat()`、通讯、数据库、MES、校准和 NG 判定均列为保护边界。
- 所有新增文件都列出责任，并在任务中列出加入项目文件的动作。
- 未把缺少 MSBuild 时的静态检查误写成编译通过。
- 未使用 TBD、TODO 或“稍后补充”等占位描述。
