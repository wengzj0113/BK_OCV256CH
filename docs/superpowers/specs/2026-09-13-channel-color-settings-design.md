# 通道状态颜色设置设计

## 1. 目标

在 frmInit 中增加一个独立的“通道颜色”设置页，使用 WinForms 可视化设计器生成布局，允许用户通过颜色选择器配置以下五种通道状态颜色：

| 状态 | 默认颜色 | 用途 |
| --- | --- | --- |
| PASS | #18A968 | 测试合格通道 |
| 测试中 | #1769E8 | 已进入本轮测试、尚未完成的通道 |
| 空闲 | #657184 | 未测试或无有效结果的通道 |
| 内阻 NG | #EF3D43 | 内阻超出合格范围 |
| 电压 NG | #F2763D | 电压超出合格范围 |

用户可以选择颜色、查看预览、恢复默认，并通过现有 frmInit 的“确定”流程保存到现有的 Initset.set（项目中的实际文件名为 Initset.set）中。

主页面可见的 channelGrid 以及测试进行中的通道刷新必须使用这些配置颜色。颜色设置不能破坏现有通道选择、悬停详情、统计、测试结果保存和设备通讯流程。

## 2. 非目标

- 不改变 PLC 所有的测试生命周期逻辑；PLC 仍只负责通知测试开始和接收测试完成。
- 不改变测量值、合格范围、数据库字段或测试结果的存储格式。
- 不把颜色设置页并入现有参数页、通讯页或其他共享 Page；它必须是 frmInit 中独立的 TabPage。
- 不用运行时动态创建颜色设置页的可视控件；布局控件和层级必须存在于 frmInit.Designer.vb，可在 Visual Studio 设计器中展开和修改。

## 3. 方案与模块边界

采用“配置数据与控件渲染解耦”的方案：

1. 主应用在 Initset 中保存五个颜色的 ARGB Int32 值。
2. frmInit 只负责把 Initset 映射到编辑控件，并将编辑结果写回暂存结构 TempInitsetting。
3. 主应用将 Initset 的 ARGB 值转换为 UIControls 项目可使用的颜色调色板对象。
4. ChannelGridControl 和 ChannelTileControl 使用同一个状态颜色辅助类，不再各自维护一份硬编码的状态映射。
5. frmTest.BuildChannelDisplayData 负责把测量结果归一化为明确的状态类型；控件只负责根据状态类型绘制颜色。

这样可以避免 UIControls 项目反向引用主应用的 Initset，也避免配置文件格式、窗体事件和绘制逻辑互相耦合。

## 4. 配置数据模型

在 Initset 结构末尾追加五个字段，字段顺序固定：

    Dim ChannelPassColorArgb As Integer
    Dim ChannelTestingColorArgb As Integer
    Dim ChannelIdleColorArgb As Integer
    Dim ChannelResistanceNgColorArgb As Integer
    Dim ChannelVoltageNgColorArgb As Integer

颜色使用 Color.ToArgb() 保存，读取时使用 Color.FromArgb(value)。颜色必须是不透明色；颜色选择器返回的 Alpha 若不是 255，保存前统一规范化为 Alpha=255，避免半透明背景导致不同控件显示不一致。

建议在主应用中增加集中定义默认值、读取值规范化和 Initset 转换的辅助类，例如 ChannelColorSettings：

- DefaultPass：#18A968
- DefaultTesting：#1769E8
- DefaultIdle：#657184
- DefaultResistanceNg：#EF3D43
- DefaultVoltageNg：#F2763D
- 能够从 Initset 读取调色板，也能够将调色板写回 Initset。
- ARGB 为无效/透明值时，对应项回退到默认颜色；不能因为单个颜色损坏而阻止整个配置文件读取。

五个颜色的业务名称和顺序必须保持稳定，后续不能仅按 UI 控件顺序猜测字段含义。

## 5. frmInit 独立颜色设置页

在 frmInit.Designer.vb 中增加独立的 TabPage，建议命名为 TabPageChannelColors，标题为“通道颜色”。该页不复用现有 Page 的动态布局，也不在 frmInit.vb 中通过 New Label、New Panel 或 Controls.Add 动态拼接。

页面采用已确认的列表式方案 A：

- 顶部说明文字：说明颜色会应用到主页面通道格及测试过程。
- 五行颜色设置，每行包含：状态名称、颜色预览 Panel、颜色值文本、选择按钮。
- 五个预览 Panel 使用固定大小，边框清晰，文本值以 #RRGGBB 显示。
- 底部提供“恢复默认”按钮。
- 使用一个设计器托管的 ColorDialog 作为颜色选择器；五个选择按钮分别打开它并将结果写入对应暂存颜色。
- 复用 frmInit 现有底部“确定”和“取消”按钮：点击“确定”时连同其他配置一次性保存，点击“取消”丢弃所有尚未确认的颜色修改。

所有新增可视控件、名称、尺寸、位置、字体、颜色、Tab 顺序和事件绑定所需的字段都写入 frmInit.Designer.vb。frmInit.vb 只保留加载值、响应按钮和保存前校验等行为代码。

建议新增控件命名如下，便于设计器树和代码维护：

    TabPageChannelColors
    GroupBoxChannelColors
    lblChannelColorHint
    lblPassColor / pnlPassColor / txtPassColor / btnPassColor
    lblTestingColor / pnlTestingColor / txtTestingColor / btnTestingColor
    lblIdleColor / pnlIdleColor / txtIdleColor / btnIdleColor
    lblResistanceNgColor / pnlResistanceNgColor / txtResistanceNgColor / btnResistanceNgColor
    lblVoltageNgColor / pnlVoltageNgColor / txtVoltageNgColor / btnVoltageNgColor
    btnResetChannelColors
    colorDialogChannel

颜色值文本只用于展示，不作为配置文件的真实数据源；真实值始终来自暂存的 Color/ARGB 值。

## 6. frmInit 编辑与保存流程

### 6.1 打开页面

frmInit_Load 在完成 Initsetting 读取和其他页面初始化后，调用颜色初始化方法：

1. 从 Initsetting 规范化五个颜色。
2. 复制到颜色页的暂存变量；不直接修改全局 Initsetting。
3. 更新五个预览 Panel 的 BackColor 和 #RRGGBB 文本。

旧配置没有新增颜色字段时，初始化为默认颜色，并允许用户直接保存生成新字段。

### 6.2 选择颜色

每个选择按钮打开 ColorDialog，初始颜色为该行暂存颜色。用户点击“取消”时不改变任何内容；点击“确定”时只更新对应暂存颜色和该行预览，不立即写文件、不立即改变其他测试配置。

### 6.3 恢复默认

点击“恢复默认”时，五项暂存颜色全部恢复为约定默认值并立即刷新预览。该操作仍需点击窗体的“确定”才写入 Initset.set；点击“取消”可整体撤销。

### 6.4 确定/取消

在现有 TextToStruct() 的成功路径中，将五项暂存颜色写入 TempInitsetting，并沿用当前 CmdOK_Click 的保存和退出行为。不能新增一套绕过现有校验的独立保存流程。

Button1_Click 关闭窗体时不把颜色暂存值写回全局配置。SCM 时间参数的现有即时应用行为保持不变，不与颜色页的暂存语义混用。

## 7. Initset.set 二进制兼容

当前配置文件是按字段顺序写入的二进制文件，新增五个 Int32 必须追加在现有最后一个字段之后，不能插入旧字段中间：

    现有字段 ...
    ScmPhaseGuardMs
    ChannelPassColorArgb
    ChannelTestingColorArgb
    ChannelIdleColorArgb
    ChannelResistanceNgColorArgb
    ChannelVoltageNgColorArgb

SaveInitFile() 按上述顺序写入，继续使用现有临时文件和替换策略。

ReadInitFile() 对每一个追加字段进行 EOF 判断：

- 老版本文件读不到颜色字段：五项全部使用默认值。
- 文件只写入了部分颜色字段：已读到的有效项保留，其余项使用默认值。
- 单项值无效、透明或读取异常：该项回退默认值，不影响其他设备配置。
- 保存后再次读取，五项颜色必须逐项保持一致。

除新增字段外，不修改既有字段顺序，也不重命名 Initset.set，以保证现有安装和旧配置可以升级。

## 8. 通道状态模型与颜色映射

在 UIControls 项目中增加独立的状态颜色模型，例如 ChannelStatusColors，至少提供：

- PassColor
- TestingColor
- IdleColor
- ResistanceNgColor
- VoltageNgColor

同时增加统一的状态枚举或规范化方法，建议使用以下状态值：

    PASS
    TESTING
    IDLE
    RESISTANCE_NG
    VOLTAGE_NG

ChannelGridControl 和 ChannelTileControl 均通过该模型取得背景色和适合的前景色。背景色使用用户配置颜色；前景色由统一辅助方法根据背景亮度选择黑/白，或使用为该状态定义的可读前景色，确保自定义深色和浅色都能辨识。选中边框、框选覆盖层和悬停提示的交互样式仍使用现有主题色，不被五项状态颜色覆盖。

没有结果的通道、旧代码中的 WAITING、未知状态都归入 IDLE，不再保留一个没有配置来源的第六种通道背景色。

主页面当前实际显示的是 channelGrid；隐藏的旧 Pic_Bat 不是当前用户可见渲染路径。若保留其绘制代码，必须确保它不会覆盖 channelGrid，并在静态检查中确认当前主页面没有重新显示它。若 ChannelTileControl 仍被其他页面使用，也必须使用同一状态颜色模型，避免同一个状态在不同控件上显示不同颜色。

## 9. 结果判定与优先级

frmTest.BuildChannelDisplayData 不再把所有 Result = “NG”直接映射为同一个 NG 颜色，而是根据通道的电压、内阻和当前合格范围计算 NG 原因：

1. testingChannelNumbers 包含该通道时，状态为 TESTING，优先于旧结果文本，保证测试过程中立即显示蓝色。
2. 结果为 OK/PASS 时，状态为 PASS。
3. 结果为 NG 时：
   - 只有内阻超范围：RESISTANCE_NG。
   - 只有电压超范围：VOLTAGE_NG。
   - 两者都超范围：显示 RESISTANCE_NG，即内阻 NG 红色优先。
4. 没有有效结果时，状态为 IDLE。

两项同时 NG 时仅决定通道格的主显示颜色；原始电压、内阻、Result、数据库记录和日志不能被覆盖或丢失。若现有结果模型不足以在刷新时可靠区分原因，应在结果生成时增加内部 NG 原因信息或统一判定函数，但不能通过修改显示文本来替代原始结果。

统计逻辑仍按 PASS/NG/TESTING/空闲统计，不因颜色细分而改变统计数量。悬停详情继续显示真实测量值和结果，必要时可追加“内阻 NG/电压 NG”原因文字。

## 10. 颜色应用时机

frmTest 在加载配置和初始化 channelGrid 时应用当前调色板，并在每次 RefreshChannelOverview() 前确保控件使用当前调色板。

以下节点必须触发通道颜色刷新：

- 主页面首次初始化：所有未测试通道显示空闲色。
- 测试开始并调用 MarkSelectedChannelsTesting() 后：选中测试通道显示测试中色。
- 每个通道结果回填后：立即根据 PASS、内阻 NG 或电压 NG 更新颜色。
- 测试异常、取消或清理测试状态时：清除测试中标记并刷新，避免通道永久保持蓝色。
- 从 frmInit 保存颜色后，若主窗体仍在显示，应通过明确的公开刷新入口重新应用调色板；否则至少保证下一次主页面刷新/重新打开时生效。不得依赖窗体重启才能修正已经打开页面的颜色。

调色板应用必须只触发重绘和数据刷新，不能重新打开串口、重新启动仪表或改变 PLC/SCM 测试时序。

## 11. 验证范围

实施后至少完成以下验证：

### 11.1 设计器验证

- frmInit 可以在 Visual Studio 设计器打开。
- 新的“通道颜色”页独立存在，五行控件和恢复默认按钮都能在设计器控件树中看到。
- frmInit.Designer.vb 包含页面布局，不依赖运行时动态创建这些可视控件。
- 现有 frmInit 其他页面和打开/取消/确定行为不回归。

### 11.2 配置文件验证

- 用旧版无颜色字段的 Initset.set 启动，五项显示默认颜色，其他设备配置不变。
- 五项分别设置为不同颜色，点击确定保存，重新打开后逐项一致。
- 点击恢复默认但取消，原颜色保持不变。
- 写入透明或非法 ARGB 时自动回退默认值且程序可启动。

### 11.3 状态映射验证

- PASS 使用绿色。
- 测试中使用蓝色，并在测试完成/异常后正确退出测试中状态。
- 空闲和未知/等待状态使用铁灰色。
- 仅内阻 NG 使用红色。
- 仅电压 NG 使用橘红色。
- 电压和内阻同时 NG 时使用内阻 NG 红色，同时保留两项测量值和 NG 结果。
- 主页面的选择、框选、悬停详情、统计数量不受影响。

### 11.4 构建和回归

- 按解决方案现有目标平台分别构建 XL-OCV-UIControls 和 MP-60A-RV-64Mem。
- 运行现有回归测试/脚本，尤其是 3562 复测、缓存读取、主页面测试和手动测试路径。
- 在无外部设备的环境下至少验证设计器加载、配置读写和状态颜色映射，不把硬件不可用误判为颜色功能失败。

## 12. 完成判定

只有同时满足以下条件才算完成：

1. 五项颜色可以在独立的 frmInit 设计器页面配置、预览、恢复默认和保存。
2. 旧版 Initset.set 可兼容读取，新颜色可稳定 round-trip 保存。
3. 主页面可见 channelGrid 和测试过程使用配置颜色，五种状态和内阻优先规则生效。
4. 设计器、构建、配置兼容和现有测试路径均通过验证。
