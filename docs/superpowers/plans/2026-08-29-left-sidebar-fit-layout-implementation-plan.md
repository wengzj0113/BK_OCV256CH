# frmTest Left Sidebar Fit Layout Implementation Plan

> For agentic workers: REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** 让 frmTest 左侧栏在基准窗口内完整展示全部控件，消除异常底部空白，并将 frmInit 设定的唯一工作站以 2 倍尺寸置于最顶部。

**Architecture:** 保留现有业务控件和事件绑定，使用 ResponsiveTestLayout 统一管理左栏外层和区块内部布局。将已选 RadioButton 从旧 GroupBox2 中移出作为左栏首项；其余控件按真实高度垂直排列，事件日志在有剩余空间时吸收高度，空间不足时由 Panel1 整体滚动。

**Tech Stack:** VB.NET WinForms、TableLayoutPanel、FlowLayoutPanel、PowerShell UI runtime checks、Visual Studio MSBuild、.NET Framework 4.8、Debug x86。

---

## 文件映射

- Modify: ResponsiveTestLayout.vb — 左栏顺序、工作站脱离旧容器、真实内容高度、滚动和事件日志弹性高度。
- Modify: frmTest.vb — 在布局建立前完成工位配置、隐藏旧容器、应用当前工位按钮样式。
- Modify: UiTheme.vb — 增加当前工位大按钮的统一尺寸和样式。
- Modify: frmTest.Designer.vb — 将 Panel1.AutoScrollMinSize 从历史固定高度清零，避免旧 Designer 值制造虚假滚动空间。
- Modify: tools/Test-LeftSidebarLayout.ps1 — 增加新工作站顺序和旧容器移除的静态约束。
- Modify: tools/Test-LeftPanelBounds.ps1 — 按当前工位直接子控件和完整区块列表检查无重叠、宽度和内容边界。
- Modify: tools/Test-LeftSidebarRuntime.ps1 — 检查只显示一个工位、工位置顶、无 GroupBox2、所有区块和报警复位可达。
- Modify: tools/Test-ResponsiveTestLayout.ps1 — 将旧的“禁止设置 AutoScrollMinSize”断言改为检查 Designer 和运行时滚动边界均为动态内容。
- Create: docs/superpowers/plans/2026-08-29-left-sidebar-fit-layout-implementation-plan.md — 本实施计划。

### Task 1: 先更新左栏回归断言

**Files:**
- Modify: tools/Test-LeftSidebarLayout.ps1
- Modify: tools/Test-LeftPanelBounds.ps1
- Modify: tools/Test-LeftSidebarRuntime.ps1
- Modify: tools/Test-ResponsiveTestLayout.ps1

- [ ] Step 1: 更新静态布局断言

在 tools/Test-LeftSidebarLayout.ps1 保留现有控件存在性检查，并加入：

~~~powershell
if ($layout -notmatch 'TakeCurrentStationControl') { throw "Missing current-station extraction path." }
if ($layout -match 'AddIfPresent\(existingChildren, orderedChildren, "GroupBox2"\)') { throw "Legacy station container must not be added to the left content." }
if ($form -notmatch 'GroupBox2\.Visible\s*=\s*False') { throw "Legacy station container must be hidden." }
if ($form -notmatch 'Rbtn1\.Visible\s*=\s*False' -or $form -notmatch 'Rbtn2\.Visible\s*=\s*False' -or $form -notmatch 'Rbtn3\.Visible\s*=\s*False') { throw "Station controls must be hidden before selecting the configured station." }
~~~

- [ ] Step 2: 更新静态滚动断言

将 tools/Test-ResponsiveTestLayout.ps1 中禁止 leftPanel.AutoScrollMinSize 的旧检查替换为：

~~~powershell
if ($layoutSource -match 'leftPanel\.AutoScrollMinSize\s*=\s*New Size\(0,\s*1280\)') {
    throw 'Legacy fixed left scroll height must be removed.'
}
if ($layoutSource -notmatch 'StretchEventLogToViewport') { throw 'Flexible event-log height path is missing.' }
if ($layoutSource -notmatch 'CalculateLeftContentHeight') { throw 'Dynamic left content height calculation is missing.' }
~~~

- [ ] Step 3: 运行静态检查，确认新断言先失败

Run:

~~~powershell
& .\tools\Test-LeftSidebarLayout.ps1
& .\tools\Test-ResponsiveTestLayout.ps1
~~~

Expected: 至少一条检查因 TakeCurrentStationControl、旧容器移除或弹性日志高度尚未实现而失败；失败证明断言覆盖了本次改动目标。

### Task 2: 将当前工作站移到左栏第一项

**Files:**
- Modify: frmTest.vb: frmTest_Load, ApplyFrmTestDeferredVisualStyle, ConfigureStationControls
- Modify: ResponsiveTestLayout.vb: PrepareScrollableLeftPanel, BuildStructuredSectionLayouts

- [ ] Step 1: 在布局建立前读取并确定工作站

在 frmTest_Load 中将 ReadInitFile() 和 ConfigureStationControls() 放到第一次 ApplyFrmTestEssentialLayout() 之前；保留原有范围、探针和其它初始化顺序。目标顺序为：

~~~vb
InitializeChannelSelectionControls()
_isLoaded = True
ReadInitFile()
ConfigureStationControls()
ApplyFrmTestEssentialLayout()
~~~

删除原来布局调用之后的重复 ReadInitFile() 和 ConfigureStationControls() 调用，避免布局先按旧工位容器建立、再被二次移动。

- [ ] Step 2: 只保留配置工位可见

将 ConfigureStationControls 的可见性部分改为：

~~~vb
Rbtn1.Text = "OCV0"
Rbtn2.Text = "OCV1"
Rbtn3.Text = "OCV2"
Rbtn1.Visible = False
Rbtn2.Visible = False
Rbtn3.Visible = False
GroupBox2.Visible = False
GroupBox2.Enabled = False

Select Case Initsetting.stationSelect
    Case 1
        Rbtn2.Checked = True
        Rbtn2.Visible = True
    Case 2
        Rbtn3.Checked = True
        Rbtn3.Visible = True
    Case Else
        Rbtn1.Checked = True
        Rbtn1.Visible = True
End Select
~~~

保留 cmbShow、CmdBatInput 的隐藏逻辑、UpdateCurrentOcvSelection() 和现有禁用状态；不改动 Rbtn1/2/3.Checked 被测试流程读取的语义。

- [ ] Step 3: 从旧容器提取当前工位控件

在 ResponsiveTestLayout.vb 增加：

~~~vb
Private Shared Function TakeCurrentStationControl(ByVal leftPanel As Panel) As Control
    Dim stationGroup As Control = FindDirectChild(leftPanel, "GroupBox2")
    If stationGroup Is Nothing Then Return Nothing

    Dim selected As RadioButton = stationGroup.Controls.OfType(Of RadioButton)().FirstOrDefault(Function(button) button.Checked AndAlso button.Visible)
    If selected Is Nothing Then
        selected = stationGroup.Controls.OfType(Of RadioButton)().FirstOrDefault(Function(button) button.Checked)
    End If
    If selected Is Nothing Then Return Nothing

    stationGroup.Controls.Remove(selected)
    leftPanel.Controls.Remove(stationGroup)
    stationGroup.Visible = False
    selected.Visible = True
    Return selected
End Function
~~~

在 PrepareScrollableLeftPanel 中，创建 existingChildren 前调用 TakeCurrentStationControl(leftPanel)，并把返回值作为 orderedChildren 第一项；不要再把 GroupBox2 加入 orderedChildren。原有业务控件仍按 grpTestMode、GroupBox1、grpChannelSelection、GroupBox3、GroupBox5、GroupBox8、GroupBox7、Button2 的顺序添加。

- [ ] Step 4: 移除旧工位内部布局调用

从 BuildStructuredSectionLayouts 和 ConfigureGroupLayout 调用序列中移除 GroupBox2 与 BuildStationLayout；删除或不再调用 BuildStationLayout，避免重新创建 StationOptionsLayout。

### Task 3: 让左栏内容高度真实、完整且可滚动

**Files:**
- Modify: ResponsiveTestLayout.vb: ResizeLeftContent, SetLeftContentWidth, BuildEventLogLayout
- Modify: frmTest.Designer.vb: Panel1.AutoScrollMinSize

- [ ] Step 1: 清除旧的固定滚动高度和横向滚动

在 Designer 将：

~~~vb
Me.Panel1.AutoScrollMinSize = New System.Drawing.Size(0, 1280)
~~~

改为：

~~~vb
Me.Panel1.AutoScrollMinSize = New System.Drawing.Size(0, 0)
~~~

在 ResponsiveTestLayout.Apply 中保留 leftPanel.AutoScroll = True，设置 leftPanel.AutoScrollMinSize = New Size(0, 0)，并将 leftPanel.HorizontalScroll.Enabled = False；纵向滚动由运行时内容高度决定。

- [ ] Step 2: 增加事件日志基准高度和剩余空间吸收

在 ResponsiveTestLayout 中增加：

~~~vb
Private Const EventLogMinimumHeight As Integer = 82

Private Sub ResetEventLogHeight()
    If leftContentPanel Is Nothing Then Return
    Dim eventLog As Control = FindDirectChild(leftContentPanel, "GroupBox7")
    If eventLog IsNot Nothing Then eventLog.Height = EventLogMinimumHeight
End Sub

Private Sub StretchEventLogToViewport(ByVal leftPanel As Panel)
    If leftContentPanel Is Nothing OrElse leftPanel Is Nothing Then Return
    Dim eventLog As Control = FindDirectChild(leftContentPanel, "GroupBox7")
    If eventLog Is Nothing Then Return

    Dim extraHeight As Integer = leftPanel.ClientSize.Height - CalculateLeftContentHeight()
    If extraHeight > 0 Then eventLog.Height += extraHeight
End Sub
~~~

将 BuildEventLogLayout 的最小高度和初始高度统一改为 EventLogMinimumHeight。

- [ ] Step 3: 替换 ResizeLeftContent 的计算流程

将 ResizeLeftContent 改为：

~~~vb
Private Sub ResizeLeftContent(ByVal leftPanel As Panel)
    If leftContentPanel Is Nothing OrElse leftPanel Is Nothing Then Return

    ResetEventLogHeight()
    SetLeftContentWidth(leftPanel.ClientSize.Width)
    Dim contentHeight As Integer = CalculateLeftContentHeight()

    If contentHeight < leftPanel.ClientSize.Height Then
        StretchEventLogToViewport(leftPanel)
        contentHeight = CalculateLeftContentHeight()
    End If

    If contentHeight > leftPanel.ClientSize.Height Then
        ResetEventLogHeight()
        SetLeftContentWidth(Math.Max(0, leftPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth))
        contentHeight = CalculateLeftContentHeight()
    End If

    If contentHeight < leftPanel.ClientSize.Height Then
        StretchEventLogToViewport(leftPanel)
        contentHeight = CalculateLeftContentHeight()
    End If

    leftContentPanel.Height = Math.Max(contentHeight, leftPanel.ClientSize.Height)
End Sub
~~~

这样基准窗口中剩余高度由事件日志吸收，报警复位停在左栏底部；窗口变小时则保留日志最小高度并让整个内容面板进入纵向滚动。

- [ ] Step 4: 保证每次重排都先布局再测量

在 RefreshLeftContent 保留以下顺序，并在 ResizeLeftContent 完成后再调用 leftPanel.PerformLayout()：

~~~vb
leftContentPanel.PerformLayout()
ResizeLeftContent(leftPanel)
leftPanel.PerformLayout()
~~~

### Task 4: 统一当前工位按钮的放大样式

**Files:**
- Modify: UiTheme.vb
- Modify: frmTest.vb: ApplyFrmTestDeferredVisualStyle, ConfigureCompactLeftControls

- [ ] Step 1: 增加工位按钮样式

在 UiTheme.vb 增加：

~~~vb
Public Const LeftStationButtonHeight As Integer = 56

Public Shared Sub ApplyStationButtonStyle(ByVal radioButton As RadioButton)
    If radioButton Is Nothing Then Return

    radioButton.AutoSize = False
    radioButton.Height = LeftStationButtonHeight
    radioButton.Font = UiFontTitle
    radioButton.TextAlign = ContentAlignment.MiddleCenter
    radioButton.Appearance = Appearance.Button
    radioButton.FlatStyle = FlatStyle.Flat
    radioButton.UseVisualStyleBackColor = False
    radioButton.FlatAppearance.BorderSize = 1
    ApplyToggleStateStyle(radioButton, True)
End Sub
~~~

- [ ] Step 2: 只对当前可见工位应用放大样式

在 ApplyFrmTestDeferredVisualStyle 中保留三个工位的基础样式调用，然后加入：

~~~vb
If Rbtn1.Visible Then
    UiTheme.ApplyStationButtonStyle(Rbtn1)
ElseIf Rbtn2.Visible Then
    UiTheme.ApplyStationButtonStyle(Rbtn2)
ElseIf Rbtn3.Visible Then
    UiTheme.ApplyStationButtonStyle(Rbtn3)
End If
~~~

从 ConfigureCompactLeftControls 删除对旧报警复位尺寸的唯一配置，改为只保留必要的非布局保护；报警复位尺寸由其现有布局项和 UiTheme.LeftButtonHeight 统一管理。

### Task 5: 更新运行时布局验证

**Files:**
- Modify: tools/Test-LeftPanelBounds.ps1
- Modify: tools/Test-LeftSidebarRuntime.ps1

- [ ] Step 1: 检查当前工位是内容面板第一项

在运行时脚本中使用：

~~~powershell
$stationButtons = @('Rbtn1','Rbtn2','Rbtn3') | ForEach-Object { $content.Controls[$_] } | Where-Object { $null -ne $_ -and $_.Visible }
if ($stationButtons.Count -ne 1) { throw "Expected exactly one visible station button, actual=$($stationButtons.Count)." }
$station = $stationButtons[0]
if ($station.Parent -ne $content) { throw "Current station is not a direct child of LeftControlContent." }
if ($content.Controls.GetChildIndex($station) -ne 0) { throw "Current station is not the first left-sidebar item." }
if ($station.Height -lt 50) { throw "Current station button was not enlarged: height=$($station.Height)." }
if ($null -ne $content.Controls['GroupBox2']) { throw 'Legacy station container must not remain in LeftControlContent.' }
~~~

- [ ] Step 2: 检查所有区块和报警复位完整可达

保留原有无重叠和宽度检查，并将区块列表改为：

~~~powershell
$names = @('Rbtn1','grpTestMode','GroupBox1','grpChannelSelection','GroupBox3','GroupBox5','GroupBox8','GroupBox7','ResetAlarmLayout')
~~~

对 Rbtn1/2/3 采用“可见者必须存在”的判断；对 GroupBox2 采用“不在内容面板”的判断。增加：

~~~powershell
$reset = $content.Controls['ResetAlarmLayout']
if ($null -eq $reset) { throw 'Alarm reset layout is missing.' }
if ($reset.Bottom -gt $content.Height) { throw "Alarm reset is outside content: bottom=$($reset.Bottom), content=$($content.Height)." }
if ($leftPanel.HorizontalScroll.Visible) { throw 'The left sidebar must not require horizontal scrolling.' }
~~~

- [ ] Step 3: 覆盖三种工作站配置和小窗口滚动

在 Test-LeftSidebarRuntime.ps1 对 stationSelect = 0/1/2 分别运行实例化检查；每次只允许一个可见工位，并检查其文本为 OCV0、OCV1、OCV2。对 1280x920 检查报警复位在可视内容内；对 800x600 检查内容高度大于客户区且纵向滚动可用，所有区块仍存在。

### Task 6: 编译、回归和交付

**Files:**
- Verify: MP-60A-RV-64Mem.vbproj
- Verify: all left-sidebar scripts and existing responsive UI scripts

- [ ] Step 1: 运行静态检查

Run:

~~~powershell
& .\tools\Test-LeftSidebarLayout.ps1
& .\tools\Test-ResponsiveTestLayout.ps1
& .\tools\Test-ResponsiveContentBounds.ps1
~~~

Expected: all report PASS.

- [ ] Step 2: 使用隔离输出执行 Debug x86 构建

Run:

~~~powershell
& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'MP-60A-RV-64Mem.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86 /p:OutDir=obj\x86\VerifyLeftSidebar\
~~~

Expected: exit code 0 and no new compiler errors.

- [ ] Step 3: 运行程序集级 UI 验证

Run with the isolated executable path:

~~~powershell
$env:FRMTEST_EXE = (Resolve-Path '.\obj\x86\VerifyLeftSidebar\XL_OCV_256CH.exe').Path
& .\tools\Test-LeftPanelBounds.ps1
& .\tools\Test-LeftSidebarRuntime.ps1
& .\tools\Test-ResponsiveTestLayout.ps1
~~~

Expected: 工位置顶、旧容器消失、所有区块无重叠、报警复位可达、无横向滚动、缩小窗口时整体滚动。

- [ ] Step 4: 运行相关既有回归

Run:

~~~powershell
& .\tools\Test-MdiInitialLayout.ps1
& .\tools\Test-MdiInitialChildSizing.ps1
& .\tools\Test-LeftSidebarLayout.ps1
~~~

Expected: all report PASS and no test/business event binding changes are detected.

- [ ] Step 5: 检查差异并提交实现

Run:

~~~powershell
git diff --check
git status --short
git diff --stat
git add -- ResponsiveTestLayout.vb frmTest.vb frmTest.Designer.vb UiTheme.vb tools/Test-LeftSidebarLayout.ps1 tools/Test-LeftPanelBounds.ps1 tools/Test-LeftSidebarRuntime.ps1 tools/Test-ResponsiveTestLayout.ps1
git commit -m "fix: fit complete frmTest left sidebar layout"
~~~

提交前确认没有把 .superpowers/、构建输出、用户临时文件或其它无关改动加入提交。

