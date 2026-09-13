# 通道状态颜色设置 Implementation Plan

> For agentic workers: REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (- [ ]) syntax for tracking.

Goal: 在 frmInit 中提供可视化设计的五色配置页，并让主页面通道格及测试过程使用已保存的颜色。

Architecture: 主应用在 Initset 末尾追加五个 ARGB Int32 字段；frmInit 以暂存值编辑并沿用现有确定/取消保存流程。UIControls 提供独立的 ChannelStatusColors 模型和状态取色器，frmTest 将测量结果统一归一化为 PASS、TESTING、IDLE、RESISTANCE_NG、VOLTAGE_NG 后交给 channelGrid/ChannelTileControl 绘制。

Tech Stack: VB.NET, .NET Framework 4.8, WinForms Designer, MSBuild, PowerShell source-contract tests.

---

## 文件地图

- Create: tools/Test-ChannelColorSettings.ps1 — 无硬件条件下验证配置字段、Designer 控件、颜色映射和刷新接缝。
- Create: ChannelStatusColors.vb — UIControls 独立的状态枚举、调色板和前景色选择。
- Create: ChannelColorSettings.vb — 主应用默认值、ARGB 规范化和 Initset 转换。
- Modify: XL-OCV-UIControls.vbproj — 编译 ChannelStatusColors.vb。
- Modify: MP-60A-RV-64Mem.vbproj — 编译 ChannelColorSettings.vb。
- Modify: Module1.vb — 在 Initset 追加五个 ARGB 字段，并扩展 Initset.set 读写。
- Modify: frmInit.Designer.vb — 增加独立 TabPage、五行颜色设置控件、恢复默认按钮和 ColorDialog。
- Modify: frmInit.vb — 加载暂存颜色、颜色选择器按钮、预览更新、恢复默认和 TextToStruct 保存。
- Modify: ChannelGridControl.vb — 注入调色板并使用统一状态取色器。
- Modify: ChannelTileControl.vb — 使用同一状态取色器，删除重复硬编码映射。
- Modify: frmTest.vb — 应用配置调色板、细分 NG 状态、补齐测试开始/结束/异常刷新。

## Task 1: 建立红灯测试契约

Files:
- Create: tools/Test-ChannelColorSettings.ps1

- [ ] Step 1: 写出会失败的契约测试

测试必须检查以下当前尚不存在的内容，并在失败时报告具体缺口：

    $ErrorActionPreference = 'Stop'
    $root = Split-Path -Parent $PSScriptRoot

    function Assert-Contains([string]$path, [string]$pattern, [string]$message) {
        $text = Get-Content -LiteralPath (Join-Path $root $path) -Raw
        if ($text -notmatch [regex]::Escape($pattern)) {
            throw "$message ($path 缺少: $pattern)"
        }
    }

    Assert-Contains 'Module1.vb' 'ChannelPassColorArgb' 'Initset 未追加 PASS 颜色字段'
    Assert-Contains 'frmInit.Designer.vb' 'TabPageChannelColors' 'frmInit 未增加独立颜色页'
    Assert-Contains 'ChannelStatusColors.vb' 'RESISTANCE_NG' 'UIControls 未定义内阻 NG 状态'
    Assert-Contains 'ChannelStatusColors.vb' 'VoltageNgColor' 'UIControls 未定义电压 NG 颜色'
    Assert-Contains 'frmTest.vb' 'RESISTANCE_NG' '主页面未细分内阻 NG'
    Assert-Contains 'frmTest.vb' 'VOLTAGE_NG' '主页面未细分电压 NG'
    Assert-Contains 'ChannelGridControl.vb' 'ChannelStatusColors' '主通道格未接入调色板'
    Assert-Contains 'ChannelTileControl.vb' 'ChannelStatusColors' '通道控件未接入统一调色板'
    Assert-Contains 'frmInit.vb' 'btnResetChannelColors' 'frmInit 未接入恢复默认按钮'
    Write-Output 'Channel color contract: PASS'

- [ ] Step 2: 运行红灯测试并确认失败原因

运行：

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1

预期：失败，并指出 Module1.vb 尚不存在 ChannelPassColorArgb；若脚本因语法错误失败，先修正测试脚本再继续。

## Task 2: 实现 UIControls 调色板模型

Files:
- Modify: tools/Test-ChannelColorSettings.ps1
- Create: ChannelStatusColors.vb
- Modify: XL-OCV-UIControls.vbproj

- [ ] Step 1: 为调色板契约补充可验证规则

在测试脚本中加入：

    Assert-Contains 'ChannelStatusColors.vb' 'Public Enum ChannelStatusKind' '缺少通道状态枚举'
    Assert-Contains 'ChannelStatusColors.vb' 'Public Shared Function ForStatus' '缺少统一状态取色入口'
    Assert-Contains 'ChannelStatusColors.vb' 'Color.FromArgb(255' '缺少不透明颜色规范化'

- [ ] Step 2: 运行测试确认仍为红灯

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1

预期：失败，指出 ChannelStatusColors.vb 不存在。

- [ ] Step 3: 编写最小调色板实现

在 UIControls 项目中实现以下公开接口：

    Imports System.Drawing

    Public Enum ChannelStatusKind
        Pass
        Testing
        Idle
        ResistanceNg
        VoltageNg
    End Enum

    Public NotInheritable Class ChannelStatusColors
        Public ReadOnly Property PassColor As Color
        Public ReadOnly Property TestingColor As Color
        Public ReadOnly Property IdleColor As Color
        Public ReadOnly Property ResistanceNgColor As Color
        Public ReadOnly Property VoltageNgColor As Color

        Public Sub New(passColor As Color, testingColor As Color, idleColor As Color,
                       resistanceNgColor As Color, voltageNgColor As Color)
            Me.PassColor = NormalizeOpaque(passColor)
            Me.TestingColor = NormalizeOpaque(testingColor)
            Me.IdleColor = NormalizeOpaque(idleColor)
            Me.ResistanceNgColor = NormalizeOpaque(resistanceNgColor)
            Me.VoltageNgColor = NormalizeOpaque(voltageNgColor)
        End Sub

        Public Shared Function ForStatus(status As ChannelStatusKind,
                                         colors As ChannelStatusColors,
                                         ByRef foreground As Color) As Color
            Dim background As Color
            Select Case status
                Case ChannelStatusKind.Pass : background = colors.PassColor
                Case ChannelStatusKind.Testing : background = colors.TestingColor
                Case ChannelStatusKind.ResistanceNg : background = colors.ResistanceNgColor
                Case ChannelStatusKind.VoltageNg : background = colors.VoltageNgColor
                Case Else : background = colors.IdleColor
            End Select
            foreground = If(GetLuma(background) < 145, Color.White, Color.Black)
            Return background
        End Function

        Public Shared Function NormalizeOpaque(value As Color) As Color
            Return Color.FromArgb(255, value.R, value.G, value.B)
        End Function

        Private Shared Function GetLuma(value As Color) As Integer
            Return CInt((299 * value.R + 587 * value.G + 114 * value.B) / 1000)
        End Function
    End Class

将文件加入 XL-OCV-UIControls.vbproj 的 Compile ItemGroup。

- [ ] Step 4: 运行契约测试和 UIControls 构建

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1
    msbuild .\XL-OCV-UIControls.vbproj /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal

预期：契约测试仍因主应用字段/Designer 未完成而失败；UIControls 构建退出码为 0。

## Task 3: 实现主应用颜色配置和二进制兼容

Files:
- Modify: tools/Test-ChannelColorSettings.ps1
- Create: ChannelColorSettings.vb
- Modify: MP-60A-RV-64Mem.vbproj
- Modify: Module1.vb

- [ ] Step 1: 增加配置读写契约

测试脚本加入：

    Assert-Contains 'Module1.vb' 'BW.Write(Initsetting.ChannelPassColorArgb)' '未写入 PASS 颜色'
    Assert-Contains 'Module1.vb' 'Initsetting.ChannelVoltageNgColorArgb = BR.ReadInt32()' '未读取电压 NG 颜色'
    Assert-Contains 'Module1.vb' 'DefaultVoltageNg' '旧配置缺少颜色默认值回退'

- [ ] Step 2: 运行红灯测试

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1

预期：失败，指出配置字段/读写代码尚未完成。

- [ ] Step 3: 追加 Initset 字段和集中默认值

在 Module1.vb 的 Initset 末尾按固定顺序追加五个 Integer 字段。ChannelColorSettings.vb 提供以下接口和默认颜色：

    Imports System.Drawing

    Public NotInheritable Class ChannelColorSettings
        Public Shared ReadOnly DefaultPass As Color = Color.FromArgb(255, 24, 169, 104)
        Public Shared ReadOnly DefaultTesting As Color = Color.FromArgb(255, 23, 105, 232)
        Public Shared ReadOnly DefaultIdle As Color = Color.FromArgb(255, 101, 113, 132)
        Public Shared ReadOnly DefaultResistanceNg As Color = Color.FromArgb(255, 239, 61, 67)
        Public Shared ReadOnly DefaultVoltageNg As Color = Color.FromArgb(255, 242, 118, 61)

        Public Shared Function FromInitset(value As Initset) As ChannelStatusColors
            Return New ChannelStatusColors(
                SafeColor(value.ChannelPassColorArgb, DefaultPass),
                SafeColor(value.ChannelTestingColorArgb, DefaultTesting),
                SafeColor(value.ChannelIdleColorArgb, DefaultIdle),
                SafeColor(value.ChannelResistanceNgColorArgb, DefaultResistanceNg),
                SafeColor(value.ChannelVoltageNgColorArgb, DefaultVoltageNg))
        End Function

        Public Shared Sub ApplyToInitset(ByRef target As Initset, colors As ChannelStatusColors)
            target.ChannelPassColorArgb = colors.PassColor.ToArgb()
            target.ChannelTestingColorArgb = colors.TestingColor.ToArgb()
            target.ChannelIdleColorArgb = colors.IdleColor.ToArgb()
            target.ChannelResistanceNgColorArgb = colors.ResistanceNgColor.ToArgb()
            target.ChannelVoltageNgColorArgb = colors.VoltageNgColor.ToArgb()
        End Sub

        Private Shared Function SafeColor(argb As Integer, fallback As Color) As Color
            Try
                Dim value As Color = ChannelStatusColors.NormalizeOpaque(Color.FromArgb(argb))
                If argb = 0 OrElse value.A = 0 Then Return fallback
                Return value
            Catch
                Return fallback
            End Try
        End Function
    End Class

- [ ] Step 4: 扩展 SaveInitFile/ReadInitFile

在现有 ScmPhaseGuardMs 写入之后追加五个 Int32；读取时每项以 FS.Position < FS.Length 判断，旧文件或缺失项分别回退默认 ARGB。保持 Initset.set 文件名、旧字段顺序和临时文件替换策略不变。

- [ ] Step 5: 运行配置契约和主项目编译

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1
    msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /v:minimal

预期：配置相关契约通过；主项目如因 Designer 尚未完成导致契约仍有剩余失败，编译错误必须为既有/未完成接缝而不是语法错误。

## Task 4: 在 frmInit.Designer.vb 增加独立颜色页

Files:
- Modify: tools/Test-ChannelColorSettings.ps1
- Modify: frmInit.Designer.vb
- Modify: frmInit.vb

- [ ] Step 1: 增加 Designer 静态结构测试

加入以下检查：

    Assert-Contains 'frmInit.Designer.vb' 'Me.TabPageChannelColors = New System.Windows.Forms.TabPage()' '颜色页未在 Designer 初始化'
    Assert-Contains 'frmInit.Designer.vb' 'Me.colorDialogChannel = New System.Windows.Forms.ColorDialog()' 'ColorDialog 未托管到 Designer'
    Assert-Contains 'frmInit.Designer.vb' 'Me.btnResetChannelColors' 'Designer 未声明恢复默认按钮'
    Assert-Contains 'frmInit.Designer.vb' 'Me.TabControl1.Controls.Add(Me.TabPageChannelColors)' '颜色页未加入 TabControl'

- [ ] Step 2: 运行红灯测试

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1

预期：失败，指出颜色页不在 Designer。

- [ ] Step 3: 只在 Designer 中声明和布局控件

在 InitializeComponent 中创建独立 TabPage、GroupBox、说明 Label、五行 Label/Panel/只读文本框/Button 和恢复默认 Button；所有新增字段放入 Designer 类字段区，设置 Name、Text、Location、Size、TabIndex、Font、BorderStyle 和 Suspend/ResumeLayout。将 TabPage 加入现有 TabControl，并在窗体 Dispose 中释放 ColorDialog。

不得在 frmInit.vb 的构造函数或 Load 中创建上述可视控件、改变布局或调用动态 Controls.Add。

- [ ] Step 4: 实现 frmInit 行为代码

在 frmInit.vb 中使用五个暂存 Color 字段；实现 LoadChannelColors、RefreshChannelColorPreview、五个选择按钮和 btnResetChannelColors_Click。按钮只改暂存值；CmdOK 的 TextToStruct 成功路径用 ChannelColorSettings.ApplyToInitset(ByRef TempInitsetting, palette) 写入暂存结构；取消不写回。

- [ ] Step 5: 运行 Designer 契约

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1
    msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /v:minimal

预期：Designer 契约通过；主项目退出码为 0。

## Task 5: 统一控件取色并接入 frmTest 状态模型

Files:
- Modify: tools/Test-ChannelColorSettings.ps1
- Modify: ChannelGridControl.vb
- Modify: ChannelTileControl.vb
- Modify: frmTest.vb

- [ ] Step 1: 增加状态和刷新契约

加入：

    Assert-Contains 'ChannelGridControl.vb' 'Public Sub ApplyStatusColors' 'channelGrid 缺少调色板入口'
    Assert-Contains 'ChannelTileControl.vb' 'ChannelStatusColors.ForStatus' 'ChannelTileControl 未使用统一取色'
    Assert-Contains 'frmTest.vb' 'ChannelColorSettings.FromInitset' 'frmTest 未从配置建立调色板'
    Assert-Contains 'frmTest.vb' 'testingChannelNumbers.Contains(channelNo)' '测试中状态未保留'

- [ ] Step 2: 运行红灯测试

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1

预期：失败，指出控件和 frmTest 尚未接入。

- [ ] Step 3: 给 ChannelGridControl 和 ChannelTileControl 注入调色板

两控件都保留默认调色板，新增 ApplyStatusColors(colors As ChannelStatusColors) 保存引用并 Invalidate；绘制时调用 ChannelStatusColors.ForStatus。删除两处私有重复的硬编码 GetStatusColors。选中边框、框选覆盖层和悬停文字保持原有 UiTheme 交互样式。

- [ ] Step 4: 细分 frmTest 的 NG 状态

在 frmTest 中增加统一方法：

    Private Function GetChannelStatus(channelNo As Integer) As ChannelStatusKind
        If testingChannelNumbers.Contains(channelNo) Then Return ChannelStatusKind.Testing
        Dim item As Bat_info = RecieveData(channelNo - 1)
        Dim resultText As String = If(item.Result, String.Empty).Trim()
        If resultText.Equals("OK", StringComparison.OrdinalIgnoreCase) OrElse
           resultText.Equals("PASS", StringComparison.OrdinalIgnoreCase) Then
            Return ChannelStatusKind.Pass
        End If
        If resultText.Equals("NG", StringComparison.OrdinalIgnoreCase) Then
            Dim voltageNg As Boolean = item.Voltage * 1000.0F > Initsetting.upVoltage OrElse
                                       item.Voltage * 1000.0F < Initsetting.downVoltage
            Dim resistanceNg As Boolean = item.Resistance > Initsetting.upResistance OrElse
                                          item.Resistance < Initsetting.downResistance
            If resistanceNg Then Return ChannelStatusKind.ResistanceNg
            If voltageNg Then Return ChannelStatusKind.VoltageNg
        End If
        Return ChannelStatusKind.Idle
    End Function

BuildChannelDisplayData 将该状态序列化为 PASS、TESTING、IDLE、RESISTANCE_NG、VOLTAGE_NG；两项同时 NG 时按内阻优先。原始 Bat_info、Result、测量值和统计逻辑不改。

- [ ] Step 5: 接入刷新时机

frmTest_Load 在 ReadInitFile() 后调用 channelGrid.ApplyStatusColors(ChannelColorSettings.FromInitset(Initsetting))；增加公开刷新入口供 frmInit 保存后调用；在 MarkSelectedChannelsTesting、结果回填后、异常/清理路径调用现有 RefreshChannelOverview()，保证测试中蓝色不会残留。

- [ ] Step 6: 运行状态契约和构建

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1
    msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /v:minimal

预期：契约测试输出 Channel color contract: PASS，构建退出码为 0。

## Task 6: 完整验证与回归

Files:
- Modify: tools/Test-ChannelColorSettings.ps1 only if a verified test gap is found.

- [ ] Step 1: 运行完整颜色契约

    pwsh -NoProfile -File .\tools\Test-ChannelColorSettings.ps1

预期：输出 Channel color contract: PASS 且退出码为 0。

- [ ] Step 2: 构建两个项目的 Any CPU 和 x86 配置

    msbuild .\XL-OCV-UIControls.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /v:minimal
    msbuild .\MP-60A-RV-64Mem.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform=x86 /v:minimal

预期：两个命令退出码均为 0，0 error。

- [ ] Step 3: 运行现有相关回归脚本

    pwsh -NoProfile -File .\tools\Test-ChannelGridRuntime.ps1
    pwsh -NoProfile -File .\tools\Test-ChannelGridVirtualization.ps1
    pwsh -NoProfile -File .\tools\Assert-WinFormsDesignerSerialization.ps1
    pwsh -NoProfile -File .\tools\Test-MainTestSelectionSnapshot.ps1
    pwsh -NoProfile -File .\tools\Test-SingleGroupMeasurementMode.ps1

预期：各脚本退出码为 0；硬件不可用只能影响硬件运行验证，不能影响静态配置/设计器/状态映射测试。

- [ ] Step 4: 检查最终差异和无关文件

    git diff --check
    git status --short
    git diff --stat

确认只提交本功能相关的源码、项目文件、测试脚本和计划文档；不删除或覆盖现有未跟踪的回滚/构建验证目录。

- [ ] Step 5: 提交实现

    git add -- Module1.vb frmInit.vb frmInit.Designer.vb frmTest.vb ChannelGridControl.vb ChannelTileControl.vb ChannelStatusColors.vb ChannelColorSettings.vb XL-OCV-UIControls.vbproj MP-60A-RV-64Mem.vbproj tools/Test-ChannelColorSettings.ps1
    git commit -m "feat: add configurable channel status colors"
