# frmTest 全界面 Designer 化设计

## 1. 目标

将程序运行成功后展示的主界面固化为 Visual Studio WinForms Designer 可见、可编辑的控件树。静态 UI 包括控件层级、布局容器、尺寸、Dock/Anchor、字体、颜色、边框、Padding、Margin、默认可见状态和默认启用状态。

运行时只负责业务数据、事件绑定、设备状态、统计值、通道状态和报警状态，不再负责重建静态界面或在首屏后重新套用整套视觉样式。

## 2. 范围与非目标

### 范围

- `frmTest` 主测试页面的完整静态布局。
- `ChannelGridControl`、`StatisticsControl`、`BottomStatusControl` 内部稳定可视控件的 Designer 化。
- `MDIForm` 中底部状态控件实例和静态菜单外观的 Designer 化。
- 删除主界面对运行时布局重建的依赖。
- 保留现有测试、设备、数据库、MES 和通道数据逻辑。

### 非目标

- 不将 256 个通道单元改成 256 个独立 WinForms 控件。
- 不改变通道选择、统计计算、报警处理、设备连接或测试流程。
- 不改变现有 UI 的视觉设计，只把当前成功运行后的视觉结果前置到 Designer。

## 3. 当前实现与问题

`frmTest.Designer.vb` 已包含大部分传统控件，但主界面的最终控件树并不由 Designer 完成：

- `ResponsiveTestLayout` 运行时创建根部左右布局、左栏内容表格、分组内部表格和部分 FlowLayoutPanel，并重排控件父子关系。
- `EnsureTestModeControls` 在运行时创建“测试控制”GroupBox 及自动/手动 CheckBox。
- `ApplyFrmTestDeferredVisualStyle` 在首次 Paint 后重新设置大量颜色、字体和控件样式。
- `ChannelGridControl`、`StatisticsControl` 和 `BottomStatusControl` 在构造函数中创建内部可视控件。
- `MDIForm` 在运行时创建并加入 `BottomStatusControl`，同时在运行时设置菜单外观。

因此，打开 Designer 时看到的是不完整的旧控件结构，不能直接修改程序成功运行后的最终界面。

## 4. 目标控件结构

### 4.1 frmTest

`frmTest.Designer.vb` 负责创建以下静态树：

```text
frmTest
└── RootLayout (TableLayoutPanel, Fill)
    ├── Panel1 / LeftScrollPanel (左侧控制区，可滚动)
    │   └── LeftControlContent (TableLayoutPanel, 单列)
    │       ├── 当前工位 RadioButton
    │       ├── grpTestMode
    │       │   ├── chkAutoTest
    │       │   └── chkManualTest
    │       ├── GroupBox1（托盘与测试控制）
    │       ├── grpChannelSelection
    │       ├── GroupBox3（合格范围）
    │       ├── GroupBox5（报警与温度）
    │       ├── GroupBox8（探针使用次数）
    │       ├── GroupBox7（事件日志）
    │       └── ResetAlarmLayout / Button2
    └── RightLayout (TableLayoutPanel, Fill)
        ├── statisticsControl
        └── channelGrid
```

左侧分组内部的字段布局使用 Designer 中的 `TableLayoutPanel` 固化，分组顺序和当前成功运行后的显示顺序保持一致。左侧窄屏滚动由 `Panel1.AutoScroll` 和 Designer 中的 Dock/Anchor/AutoSize 组合承担，不再由 `ResponsiveTestLayout` 在 Resize 时重排。

### 4.2 ChannelGridControl

新增 `ChannelGridControl.Designer.vb`，静态控件树为：

```text
ChannelGridControl
└── outerLayout
    ├── titleLayout
    │   ├── titleLabel
    │   └── hoverDetailsLabel
    ├── cornerLabel
    ├── columnHeader（16 个坐标 Label）
    ├── rowHeader（16 个坐标 Label）
    └── gridSurface
```

`gridSurface` 继续使用自定义绘制处理 256 个通道的颜色、选中状态、鼠标悬停和矩形选择。它是动态数据绘制面，不作为 256 个独立 Designer 控件展开。

### 4.3 StatisticsControl

新增 `StatisticsControl.Designer.vb`，将统计标题、总数/PASS/NG/测试中/空闲的标题与数值 Label、图例表格、图例色块和文字全部放入 Designer。

`UpdateStatistics` 只更新数值文本及必要的动态统计内容，不重建布局。

### 4.4 BottomStatusControl

新增 `BottomStatusControl.Designer.vb`，将 9 个状态 Label 及其横向 `TableLayoutPanel` 放入 Designer。`UpdateStatus` 只更新状态文本、时间和连接状态颜色。

`MDIForm.Designer.vb` 增加一个 `BottomStatusControl` 实例并设置 `Dock = Bottom`。`MDIForm` 的菜单背景、前景色、字体、间距和高度等静态属性也进入 Designer；MDI 客户区本身仍由 WinForms 框架创建。

## 5. 代码职责调整

### Designer 文件

负责：

- 所有稳定控件的实例化和 `Controls.Add`；
- 控件父子关系、顺序和布局表格；
- 颜色、字体、尺寸、边框、Padding、Margin；
- 默认 `Visible`、`Enabled`、`Checked` 和 `ReadOnly` 状态；
- `InitializeComponent` 内需要由 Designer 持久化的事件绑定和组件容器初始化。

### 运行时代码文件

保留：

- `frmTest_Load` 中的设置读取、设备/数据初始化和事件处理；
- `ConfigureStationControls` 对当前工位的配置相关显示和选中状态调整；
- 测试模式互斥逻辑；
- 通道选择、统计刷新、报警状态、设备状态和按钮事件；
- `ChannelGridSurface` 的动态绘制与鼠标交互；
- `BottomStatusControl.UpdateStatus` 和 `StatisticsControl.UpdateStatistics` 的数据更新。

移除或停用：

- `EnsureTestModeControls` 的运行时控件创建；
- `ResponsiveTestLayout.Apply`、`ApplyDeferredStructuredLayouts` 及其主窗体调用；
- `ApplyFrmTestEssentialLayout` 和 `ApplyFrmTestDeferredVisualStyle` 中的静态布局/样式工作；
- `MDIForm.EnsureBottomStatusControl` 中的实例创建与 `Controls.Add`；
- 会覆盖 Designer 静态属性的整套 `UiTheme.Apply...` 启动式调用。

`ResponsiveTestLayout.vb` 当前仅服务于这套主界面运行时布局。迁移完成并确认无其他调用者后，将其从项目文件移除并删除；最终实现以项目中不存在主界面运行时重排调用为验收条件。

## 6. 样式策略

当前成功运行后的静态视觉值直接写入对应 Designer 属性，包括：

- 页面背景 `#F3F6FA`；
- 卡片背景 `White`；
- 卡片边框 `#E7EBF0`；
- 顶部/底部状态栏背景 `#061D3B`；
- 主按钮 `#1769E8`；
- 成功、警告、危险和文本颜色；
- Microsoft YaHei UI 字体层级、字体大小和粗体设置。

`UiTheme` 保留公共颜色定义，供运行时的报警、连接、选中、通道状态和统计状态使用，但不再在窗体加载或首次 Paint 后批量覆盖静态 Designer 属性。动态状态代码只修改状态所需的文本、可见性、启用性和颜色。

## 7. 初始化与数据流

```text
构造窗体
  → InitializeComponent 创建完整静态控件树
  → Load 读取配置、绑定事件、设置初始数据状态
  → UpdateStatistics / UpdateAll / UpdateStatus 更新动态数据
  → 用户操作触发既有业务事件和状态刷新
```

初始化过程中不应再出现静态控件的运行时创建、移除、重父化或整套样式重套。配置驱动的工位可见性和运行时状态仍允许在 Load 或业务事件中改变，因为这些值取决于设备和系统配置。

## 8. 兼容性与风险控制

- 保留现有控件字段名称，避免破坏 `frmTest.vb` 中的事件处理和业务引用。
- 将原构造函数中的私有只读可视字段调整为 Designer 可生成的字段，并在非 Designer 部分继续使用相同字段名。
- 自定义绘制和自定义控件的公开方法、属性、事件签名保持不变。
- 先迁移静态布局，再删除运行时布局调用；每一步都保持可编译，便于定位 Designer 初始化冲突。
- 不直接依赖 Designer 生成的默认字体缩放覆盖当前视觉结果；明确设置 `AutoScaleMode`、字体和尺寸。
- 在项目文件中将新增 Designer 文件与对应 UserControl 通过 `DependentUpon` 关联，确保 Visual Studio 正确显示嵌套文件。

## 9. 验证方案

### 编译与设计器

- 使用现有 VB.NET 项目配置编译，确认新增分部类、控件字段和事件绑定无冲突。
- 在 Visual Studio 中打开 `frmTest`、`ChannelGridControl`、`StatisticsControl`、`BottomStatusControl` 和 `MDIForm` 设计器。
- 确认静态控件树可以展开，修改布局或颜色后重新打开设计器仍保持修改结果。

### 视觉回归

- 使用当前成功运行的主界面作为基准，至少检查 1464×902 默认尺寸。
- 检查左栏顺序、卡片尺寸、按钮层级、统计区、通道区和底部状态栏。
- 检查窗口缩放、窄屏左侧滚动、右侧通道区填充和底部状态栏不遮挡。

### 行为回归

- 自动/手动模式互斥和默认选中状态；
- 托盘输入、开始、停止、NG 测试、开关托盘和报警复位；
- 单通道、全选、清除和矩形选择；
- 统计值和通道状态刷新；
- 报警/运行标签、烟雾状态、温度状态和事件日志；
- 工位配置、设备连接状态、数据库/MES 状态和用户时间显示。

### 结构验收

- 主界面初始化后不再调用 `ResponsiveTestLayout` 重建静态布局。
- 静态 UI 初始化后的 `Controls.Add/Remove` 仅存在于各自 Designer 或必要的动态绘制承载逻辑中。
- 修改 Designer 中的静态颜色、字体或布局后，启动程序不会被静态样式方法恢复覆盖。

## 10. 完成标准

当 Visual Studio Designer 能展示并编辑整个稳定 UI 控件树，程序运行后的外观与当前成功版本一致，窗口缩放和既有业务行为通过回归验证，且主界面不再运行时重建静态控件时，本设计视为实施完成。
