# FrmChannelCalibration Designer Refactor Implementation Plan

**Goal:** 将 FrmChannelCalibration 重构为标准 WinForms Designer 管理的窗体，同时保持现有校准业务行为不变。

**Architecture:** `FrmChannelCalibration.Designer.vb` 负责控件字段、窗体属性、布局、表格列和基础事件绑定；`FrmChannelCalibration.vb` 只保留校准服务、事件处理、数据加载和业务逻辑。两者使用 VB partial class 共享成员。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.8、现有 CalibrationService/ChannelCalibrationRunner。

---

### Task 1: 建立重构验收检查

- [ ] 检查 Designer 文件存在并声明核心控件。
- [ ] 检查代码文件不再动态创建核心控件。
- [ ] 检查工程编译通过且无警告。

### Task 2: 迁移控件和布局到 Designer

- [ ] 创建 `FrmChannelCalibration.Designer.vb`。
- [ ] 将窗体属性、顶部命令区、校准表格、底部状态区和表格列迁移到 `InitializeComponent()`。
- [ ] 保持现有控件名称，确保业务事件处理代码不需要改动。

### Task 3: 精简代码文件

- [ ] 将类改为 partial class。
- [ ] 构造函数调用 `InitializeComponent()`。
- [ ] 删除运行时控件创建、布局创建和重复事件绑定代码。
- [ ] 保留业务逻辑及加载事件。

### Task 4: 验证

- [ ] 运行 Designer 静态检查。
- [ ] 编译 `Battery Testing.sln` Debug|Any CPU。
- [ ] 确认 0 个错误、0 个警告。
