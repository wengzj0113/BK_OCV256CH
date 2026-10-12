# NG 复测窗体 Designer 化设计

## 目标

让 `FrmNgRetest` 能在 Visual Studio Windows Forms Designer 中可视化调整，同时保留当前界面外观和运行行为。

## 当前情况

`FrmNgRetest.vb` 目前继承 `Form`，并在构造函数调用 `InitializeControls`，由运行时代码创建所有控件、容器和左右结果卡片。项目没有 `FrmNgRetest.Designer.vb` 或窗体资源文件，因此 Visual Studio 无法将该窗体作为标准 WinForms Designer 表单编辑。

## 设计

- 将 `FrmNgRetest` 拆为标准 VB WinForms partial class。新建 `FrmNgRetest.Designer.vb`，其中实现 `InitializeComponent`、组件容器清理和所有可视控件字段。
- 把当前窗体大小、最小尺寸、条码查询行、仪表与查询状态行、左右并排的“历史 NG / 本次复测”卡片，以及底部复测操作栏迁入 Designer。控件初始文字、名称、尺寸、间距和颜色保持当前行为。
- 在主 `FrmNgRetest.vb` 中保留仪表及数据服务、查询状态、异步查询/采集/保存、动态结果文字和颜色更新、窗体关闭时释放仪表等逻辑。构造函数调用 `InitializeComponent`；事件改为标准 `Handles` 绑定，避免 Designer 和运行时代码重复绑定。
- 新建 `FrmNgRetest.resx`，并在旧式 `.vbproj` 登记 Designer 编译项及其 `DependentUpon`、资源嵌入项及其 `DependentUpon`，使窗体以标准层级出现在解决方案资源管理器中。

## 验收标准

1. 在 Visual Studio 中对 `FrmNgRetest.vb` 执行“查看设计器”能打开窗体，不报告无法加载设计器的异常。
2. 查询区、仪表状态、历史 NG 与本次复测左右卡片、复测操作区中的所有控件都能在设计器中选中和调整。
3. 窗体初始布局及运行行为与改动前一致；数据库、仪表和复测保存流程不在本次范围内改变。
4. 项目 Debug x86 配置可编译。

## 验证范围

进行项目编译和代码/项目文件静态检查。不添加或运行测试套件；本次只重组 WinForms 窗体的 Designer 与事件绑定。
