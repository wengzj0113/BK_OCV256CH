# frmTest Designer UI 固化实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (- [ ]) syntax for tracking.

**Goal:** 将当前成功运行后的 OCV 主界面及三个自定义 UserControl 固化为 Visual Studio WinForms Designer 可展开、可编辑的静态控件树。

**Architecture:** Designer 分部类负责所有稳定控件、父子关系、布局和静态视觉属性；非 Designer 部分只负责数据更新、事件、绘制和设备/业务状态。ChannelGridSurface 继续自绘 256 个动态通道单元，避免把动态矩阵扩展成 256 个独立控件。

**Tech Stack:** VB.NET、.NET Framework 4.8、WinForms、Visual Studio legacy .vbproj、现有 UiTheme 颜色和自定义绘制控件。

---

## 文件与职责地图

- Modify: frmTest.Designer.vb — 主窗体完整静态控件树、布局和默认视觉属性。
- Modify: frmTest.vb — 删除动态 UI 创建、重排和静态样式覆盖，保留业务和动态状态。
- Create: ChannelGridControl.Designer.vb — 标题、悬浮信息、坐标标签、网格承载布局。
- Modify: ChannelGridControl.vb — 构造函数改为 InitializeComponent，保留绘制和选择逻辑。
- Create: StatisticsControl.Designer.vb — 统计标题、数值、图例和表格布局。
- Modify: StatisticsControl.vb — 构造函数改为 InitializeComponent，保留统计更新。
- Create: BottomStatusControl.Designer.vb — 9 个状态标签及横向表格布局。
- Modify: BottomStatusControl.vb — 构造函数改为 InitializeComponent，保留状态更新。
- Modify: MDIForm.Designer.vb — 底部状态控件实例和菜单静态属性。
- Modify: MDIForm.vb — 删除底部状态控件、登录菜单和校准菜单的运行时创建及重复静态样式设置。
- Modify: MP-60A-RV-64Mem.vbproj — 注册新增 Designer 文件并设置 DependentUpon。
- Delete: ResponsiveTestLayout.vb — 主界面迁移完成且确认无其他调用者后删除。

## Task 1: 建立基线并锁定现有行为

**Files:**

- Read: frmTest.Designer.vb、frmTest.vb、ResponsiveTestLayout.vb、ChannelGridControl.vb、StatisticsControl.vb、BottomStatusControl.vb。
- Test: tools/Assert-UiDataNullSafety.ps1、tools/Assert-OcvRuntimeDatabase.ps1。

- [ ] **Step 1: 记录工程状态**

运行：

~~~powershell
git status --short
git log -1 --oneline
~~~

确认现有未跟踪构建目录不被后续提交带入。

- [ ] **Step 2: 编译基线项目**

~~~powershell
msbuild MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86
~~~

预期：出现 Build succeeded，无新增错误。

- [ ] **Step 3: 运行现有检查**

~~~powershell
powershell -ExecutionPolicy Bypass -File tools/Assert-UiDataNullSafety.ps1
powershell -ExecutionPolicy Bypass -File tools/Assert-OcvRuntimeDatabase.ps1
~~~

预期分别出现两个 PASS 信息。

- [ ] **Step 4: Commit**

本任务只读取和验证，不修改源文件，不创建提交。

## Task 2: 将 ChannelGridControl 固化为 Designer UserControl

**Files:**

- Create: ChannelGridControl.Designer.vb。
- Modify: ChannelGridControl.vb 中的字段、构造函数和静态创建辅助方法。
- Modify: MP-60A-RV-64Mem.vbproj 中 ChannelGridControl 编译项。

- [ ] **Step 1: 声明 Designer 控件字段**

将构造函数创建的 outerLayout、titleLayout、titleLabel、hoverDetailsLabel、cornerLabel、columnHeader、rowHeader、gridSurface，以及 16 个列坐标 Label 和 16 个行坐标 Label 拆为 Designer 字段。非 Designer 逻辑继续访问 hoverDetailsLabel 和 gridSurface。

- [ ] **Step 2: 迁移 InitializeComponent 布局**

保留现有两列三行结构、列宽 44px/Percent、行高 34px/6%/89%、标题跨两列、16×16 坐标标签和 gridSurface Dock=Fill。静态颜色使用当前白色、主色、正文色和辅助文本色；字体、Padding、Margin 和 BorderStyle 与当前运行结果一致。

- [ ] **Step 3: 收敛构造函数**

构造函数目标形态：

~~~vb
Public Sub New()
    InitializeComponent()
    AddHandler gridSurface.Paint, AddressOf HandleGridPaint
    AddHandler gridSurface.MouseDown, AddressOf HandleGridMouseDown
    AddHandler gridSurface.MouseMove, AddressOf HandleGridMouseMove
    AddHandler gridSurface.MouseUp, AddressOf HandleGridMouseUp
    AddHandler gridSurface.MouseLeave, AddressOf HandleGridMouseLeave
End Sub
~~~

删除静态控件创建代码，保留绘制、选择、悬停和公开 API。

- [ ] **Step 4: 注册、编译**

在项目文件中加入：

~~~xml
<Compile Include="ChannelGridControl.Designer.vb">
  <DependentUpon>ChannelGridControl.vb</DependentUpon>
</Compile>
~~~

运行 Debug x86 msbuild，预期构建成功且公开属性、事件签名不变。

- [ ] **Step 5: Commit**

~~~powershell
git add ChannelGridControl.vb ChannelGridControl.Designer.vb MP-60A-RV-64Mem.vbproj
git commit -m "refactor: move channel grid layout to designer"
~~~

## Task 3: 将 StatisticsControl 固化为 Designer UserControl

**Files:**

- Create: StatisticsControl.Designer.vb。
- Modify: StatisticsControl.vb 中的字段、构造函数和静态创建辅助方法。
- Modify: MP-60A-RV-64Mem.vbproj 中 StatisticsControl 编译项。

- [ ] **Step 1: 声明稳定控件**

在 Designer 中声明主布局、5 个数值 Label、5 个标题 Label、图例布局、4 个图例 FlowLayoutPanel 及其色块/文字 Label。保留 totalValue、passValue、ngValue、testingValue、idleValue 字段名。

- [ ] **Step 2: 迁移布局和颜色**

保留现有 6 列 2 行表格、列宽 14/17/17/17/17/18%、行高 34%/66%、数值颜色、标题粗体和 2×2 图例结构。

- [ ] **Step 3: 收敛构造函数和数据更新**

构造函数改为：

~~~vb
Public Sub New()
    InitializeComponent()
End Sub
~~~

UpdateStatistics 只更新 5 个数值 Label 的 Text，不重新创建控件或布局；保留 FormatMetric。

- [ ] **Step 4: 编译并提交**

运行 Debug x86 msbuild，预期构建成功后：

~~~powershell
git add StatisticsControl.vb StatisticsControl.Designer.vb MP-60A-RV-64Mem.vbproj
git commit -m "refactor: move statistics layout to designer"
~~~

## Task 4: 将 BottomStatusControl 和 MDI 宿主固化

**Files:**

- Create: BottomStatusControl.Designer.vb。
- Modify: BottomStatusControl.vb。
- Modify: MDIForm.Designer.vb、MDIForm.vb 中底部状态控件及菜单初始化相关代码。
- Modify: MP-60A-RV-64Mem.vbproj。

- [ ] **Step 1: 迁移 9 个状态 Label**

在 Designer 中创建 stationLabel、controlBoardLabel、plcLabel、voltageLabel、resistanceLabel、databaseLabel、mesLabel、userLabel、timeLabel 及 9 列横向表格。保留底色 #061D3B、默认白色文字、UI 字体和时间右对齐。

- [ ] **Step 2: 收敛 BottomStatusControl 构造函数**

构造函数只调用 InitializeComponent；UpdateStatus 保留文本格式化和连接状态颜色更新，不再 New TableLayoutPanel 或 Controls.Add。

- [ ] **Step 3: 将实例放入 MDIForm.Designer**

把 bottomStatusControl 字段声明移入 Designer，在 InitializeComponent 中实例化、设置 Dock=Bottom、默认可见和尺寸并加入 MDI Form。EnsureBottomStatusControl 改为只调用 UpdateBottomStatus，不再创建实例或 Controls.Add。

- [ ] **Step 4: 收缩 MDI 静态样式代码**

将 MDI 背景色、菜单颜色、字体、Padding、高度、ToolStrip/StatusStrip 可见性移入 Designer。把登录菜单项和校准菜单项按当前最终顺序直接放入 Designer，并把它们的 Name、ToolTipText 和 Click 处理绑定固化；删除 EnsureLoginMenu、EnsureCalibrationMenu 的启动调用和菜单插入逻辑。保留 MenuStrip.Renderer = New HeaderMenuRenderer()，因为自定义绘制对象不适合由旧版 WinForms Designer 序列化。保留权限、数据库占位状态和状态更新逻辑。

- [ ] **Step 5: 编译并提交**

运行 Debug x86 msbuild，预期构建成功后：

~~~powershell
git add BottomStatusControl.vb BottomStatusControl.Designer.vb MDIForm.vb MDIForm.Designer.vb MP-60A-RV-64Mem.vbproj
git commit -m "refactor: move bottom status ui to designer"
~~~

## Task 5: 将 frmTest 主界面完整迁移到 Designer

**Files:**

- Modify: frmTest.Designer.vb。
- Modify: frmTest.vb 中字段、Load、动态布局、样式和 Resize 相关方法。

- [ ] **Step 1: 增加测试模式和当前工位显示控件**

在 Designer 中声明 grpTestMode、chkAutoTest、chkManualTest 和 currentStationButton。currentStationButton 作为静态当前工位显示控件，避免继续从 GroupBox2 运行时移出 Rbtn1/Rbtn2/Rbtn3；旧 GroupBox2 和三枚工位按钮作为兼容控件保留并默认隐藏。

- [ ] **Step 2: 创建根布局**

将 Panel1 放入两列根 TableLayoutPanel 第 0 列，新增 RightLayout，第 0 行放 statisticsControl，第 1 行放 channelGrid。根布局左列固定 350px、右列 Percent 100%；右侧行高 14%/86%；Panel1 保持 AutoScroll=True、水平滚动关闭、垂直滚动可用。

- [ ] **Step 3: 固化左侧分组和内部表格**

按运行顺序将 currentStationButton、grpTestMode、GroupBox1、grpChannelSelection、GroupBox3、GroupBox5、GroupBox8、GroupBox7 和 Button2 放入单列 LeftControlContent。把 ResponsiveTestLayout 中的稳定 TableLayoutPanel/FlowLayoutPanel 结构迁移到 Designer，并保留 ResetAlarmLayout 居中包装。

- [ ] **Step 4: 写入静态样式**

将页面 #F3F6FA、卡片白色、边框 #E7EBF0、主色 #1769E8、正文 #243142、辅助文本 #637084、Microsoft YaHei UI 字体层级、按钮 FlatStyle、边框、Padding、Margin 和固定按钮高度写入 Designer。Load/Paint 后不得重新套用整套 UiTheme 样式。

- [ ] **Step 5: 保留事件、删除动态创建**

测试模式控件在 Designer 中绑定现有 TestModeCheckedChanged，或继续使用 VB Handles。删除 EnsureTestModeControls 的创建逻辑；保留 InitializeChannelSelectionControls、按钮事件、通道/统计更新和业务方法。

- [ ] **Step 6: 改造工位配置**

ConfigureStationControls 只设置 currentStationButton.Text、Visible 和必要的 Rbtn1/Rbtn2/Rbtn3.Checked，不得移除、重父化或移动控件；工位仍由 Initsetting.stationSelect 决定。

- [ ] **Step 7: 收缩动态样式**

删除 ApplyFrmTestEssentialLayout 和 ApplyFrmTestDeferredVisualStyle 的静态布局/样式职责。SetSelectionModeButton 只更新选中态：

~~~vb
button.BackColor = If(button Is activeButton, UiTheme.Primary, UiTheme.CardBackground)
button.ForeColor = If(button Is activeButton, Color.White, UiTheme.TextPrimary)
button.FlatAppearance.BorderColor = If(button Is activeButton, UiTheme.Primary, UiTheme.CardBorder)
~~~

保留 UpdateTestModeVisuals、报警颜色、运行/报警可见性和启用状态。

- [ ] **Step 8: 移除 Resize 重排**

删除 responsiveLayout 字段和 frmTest_Resize 对 RefreshLeftContent 的调用；保留 ResetLeftPanelScroll 作为启动后滚动位置初始化。运行：

~~~powershell
rg -n "ResponsiveTestLayout|ApplyFrmTestEssentialLayout|ApplyFrmTestDeferredVisualStyle|EnsureTestModeControls" frmTest.vb
~~~

预期：不再出现主界面初始化调用。

- [ ] **Step 9: 编译并提交**

运行 Debug x86 msbuild，预期构建成功后：

~~~powershell
git add frmTest.vb frmTest.Designer.vb
git commit -m "refactor: move frmTest layout and styling to designer"
~~~

## Task 6: 删除废弃布局类

**Files:**

- Delete: ResponsiveTestLayout.vb。
- Modify: MP-60A-RV-64Mem.vbproj。

- [ ] **Step 1: 确认无生产调用者**

~~~powershell
rg -n "ResponsiveTestLayout|RefreshLeftContent|ApplyDeferredStructuredLayouts|BuildStructuredSectionLayouts" --glob '*.vb' --glob '*.vbproj' .
~~~

预期：除待删除文件自身外没有主界面调用者。

- [ ] **Step 2: 移除项目项并删除文件**

从项目文件删除 Compile Include="ResponsiveTestLayout.vb"，再删除已确认无调用者的 ResponsiveTestLayout.vb。

- [ ] **Step 3: 编译并提交**

~~~powershell
msbuild MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86
git add MP-60A-RV-64Mem.vbproj
git rm ResponsiveTestLayout.vb
git commit -m "refactor: remove runtime test layout builder"
~~~

预期：构建成功。

## Task 7: 设计器、视觉和行为回归

**Files:**

- Verify: 5 个 Designer 文件和对应的 5 个 UserControl/Form。
- Verify: frmTest.vb、MDIForm.vb 及三个自定义控件 .vb。

- [ ] **Step 1: 执行结构检查**

~~~powershell
rg -n "New (TableLayoutPanel|GroupBox|CheckBox|Panel|BottomStatusControl|ToolStripMenuItem)|Controls\\.Add|Controls\\.Remove|ResponsiveTestLayout|ApplyFrmTestDeferredVisualStyle|ApplyFrmTestEssentialLayout|EnsureLoginMenu|EnsureCalibrationMenu" frmTest.vb MDIForm.vb ChannelGridControl.vb StatisticsControl.vb BottomStatusControl.vb
~~~

预期：静态控件创建和静态 Controls.Add 只存在于 Designer；运行时代码只保留动态绘制承载、状态更新和业务窗口创建。

- [ ] **Step 2: 编译和安全检查**

~~~powershell
msbuild MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86
powershell -ExecutionPolicy Bypass -File tools/Assert-UiDataNullSafety.ps1
powershell -ExecutionPolicy Bypass -File tools/Assert-OcvRuntimeDatabase.ps1
~~~

预期：全部成功。

- [ ] **Step 3: 打开所有 Designer**

在 Visual Studio 依次打开 frmTest、ChannelGridControl、StatisticsControl、BottomStatusControl 和 MDIForm 的设计器。确认控件树可展开，属性窗口可以修改静态布局/颜色，关闭并重新打开后修改仍由 Designer 保留。

- [ ] **Step 4: 视觉回归**

在默认 1464×902 窗口检查左栏顺序和间距、统计区、16×16 通道区、按钮颜色、字体、底部状态栏和报警复位按钮；再检查窄屏左侧滚动、右侧矩阵可见性和底部状态栏不遮挡。

- [ ] **Step 5: 行为回归**

验证自动/手动模式、托盘输入、开始/停止/NG 测试、开关托盘、报警复位、单通道/全选/清除/矩形选择、统计刷新、报警颜色、事件日志、工位配置、设备连接状态、数据库/MES 状态和用户时间显示。

- [ ] **Step 6: 最终状态检查**

~~~powershell
git diff --check
git status --short
~~~

预期：无空白错误；本次实现以外的未跟踪构建目录保留但不纳入提交。

## Self-review

- Spec coverage: 目标、控件分层、静态/动态职责、样式策略、MDI 状态栏、通道自绘、兼容性和验证标准均有对应任务。
- Placeholder scan: 没有 TODO、TBD 或未定义的待办占位符；每个代码步骤都有文件、目标符号、命令和预期结果。
- Type consistency: 公开方法和事件签名保持现有定义；currentStationButton 在 Task 5.1 定义并在 Task 5.6 使用。
- Scope check: 计划只处理主界面 Designer 化，不修改测试、设备、数据库和 MES 子系统。
