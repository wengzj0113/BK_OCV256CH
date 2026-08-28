# frmTest 左侧控制栏布局 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 `frmTest` 左侧栏收紧为稳定的垂直布局，显示 OCV0/OCV1/OCV2 和自动/手动模式，并验证启动时左栏回到顶部。

**Architecture:** 保留 `frmTest` 现有业务事件和控件名称，以 `ResponsiveTestLayout` 负责左栏容器重排，以 `frmTest` 负责模式/工位初始化和一次性滚动复位。Designer 只保存默认控件属性，运行时统一样式由 `UiTheme` 应用。

**Tech Stack:** VB.NET WinForms、.NET Framework 4.x、MSBuild Debug x86、PowerShell 静态 UI 检查。

---

### Task 1: 建立 UI 验证基线

**Files:**
- Test: `tools/Test-LeftSidebarLayout.ps1`
- Inspect: `frmTest.Designer.vb`, `frmTest.vb`, `ResponsiveTestLayout.vb`

- [ ] **Step 1: 写静态验证脚本**

脚本读取三个目标文件，验证模式控件名、OCV 文本、四个通道按钮和一次性滚动复位标识存在；失败时返回非零退出码。

```powershell
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$designer = Get-Content (Join-Path $root "frmTest.Designer.vb") -Raw
$form = Get-Content (Join-Path $root "frmTest.vb") -Raw
$layout = Get-Content (Join-Path $root "ResponsiveTestLayout.vb") -Raw

foreach ($name in @("chkAutoTest", "chkManualTest", "OCV0", "OCV1", "OCV2")) {
    if ($designer -notmatch [regex]::Escape($name)) { throw "Missing UI token: $name" }
}
foreach ($name in @("btnChannelSelectAll", "btnChannelClear", "btnChannelRectangle", "btnChannelSelectSingle")) {
    if ($designer -notmatch [regex]::Escape($name)) { throw "Missing channel button: $name" }
}
if ($form -notmatch "AutoScrollPosition\s*=\s*New Point\(0, 0\)") { throw "Missing startup scroll reset" }
if ($layout -notmatch "TableLayoutPanel|FlowLayoutPanel") { throw "Missing managed left layout container" }
Write-Output "Left sidebar static checks passed."
```

- [ ] **Step 2: 运行基线脚本并确认当前失败项**

运行：`powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-LeftSidebarLayout.ps1`

期望：在实现前因为 `chkAutoTest`/`chkManualTest` 尚不存在而失败；保存失败输出作为改动范围依据。

### Task 2: 增加模式和三工位控件

**Files:**
- Modify: `frmTest.Designer.vb` — 声明 `chkAutoTest`、`chkManualTest`，显示 OCV0/OCV1/OCV2。
- Modify: `frmTest.vb` — 初始化模式、互斥处理和 `Initsetting.stationSelect` 映射。

- [ ] **Step 1: 在 Designer 中添加两个 CheckBox 并保留现有事件控件**

控件默认属性使用以下值：`Text` 分别为“自动”“手动”，`AutoSize=True`，自动默认 `Checked=True`，两者放入左栏最顶部的测试控制容器；不要删除现有按钮和工作站控件。

- [ ] **Step 2: 将工作站文本和默认项改为 OCV0/OCV1/OCV2**

把 `Rbtn1.Text/Rbtn2.Text/Rbtn3.Text` 设为 `OCV0/OCV1/OCV2`，三项同一行并保持互斥；Load 中使用 `Select Case Initsetting.stationSelect` 选择对应项，非法值回退到 OCV0。

- [ ] **Step 3: 添加模式互斥处理**

在 `frmTest.vb` 增加 `SetTestMode` 和两个 `CheckedChanged` 处理器：用户勾选自动时清除手动，用户勾选手动时清除自动；如果两者都被清除，恢复自动。模式处理不得直接启动测试，也不得修改 `autoStartFlag` 的硬件解析。

- [ ] **Step 4: 运行静态脚本**

运行：`powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-LeftSidebarLayout.ps1`

期望：`Left sidebar static checks passed.`

### Task 3: 把左栏改为紧凑、可滚动的统一布局

**Files:**
- Modify: `ResponsiveTestLayout.vb` — 使用统一垂直布局容器管理现有卡片。
- Modify: `UiTheme.vb` — 添加统一间距、按钮高度和选项控件样式。
- Modify: `frmTest.Designer.vb` — 让通道选择四按钮同一行，收紧卡片高度。

- [ ] **Step 1: 设置左栏布局参数**

使用 5~6px 统一组间距、6~8px 卡片内边距；左栏内容宽度跟随可用区域，内容高度由实际控件计算，保留 `AutoScroll=True`。

- [ ] **Step 2: 调整通道选择一行布局**

四个按钮使用同一行的等宽列，按钮顺序固定为“全选、清除、框选、单选”；在 330px 左栏宽度下不得换行。

- [ ] **Step 3: 收紧参考图中的内容块**

控制板按钮使用 3 列网格；合格范围使用 `Min/Max` 两列输入；报警温度使用 2×2 网格；探针和日志保留可读最小高度；报警复位紧接日志之后。

- [ ] **Step 4: 运行编译**

运行：`& 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe' 'MP-60A-RV-64Mem.vbproj' /t:Build /p:Configuration=Debug /p:Platform=x86`

期望：`Build succeeded`，不得出现新增编译错误。

### Task 4: 确保首次启动滚动到顶部

**Files:**
- Modify: `frmTest.vb` — 在 Load 的布局稳定后调用一次滚动复位。
- Modify: `ResponsiveTestLayout.vb` — 普通 Resize 不强制改变滚动位置。

- [ ] **Step 1: 抽出一次性滚动复位方法**

添加 `ResetLeftPanelScroll()`，在 `Panel1` 存在且已创建句柄时设置 `Panel1.AutoScrollPosition = New Point(0, 0)`；在 `frmTest_Load` 的 `BeginInvoke(FitToMdiClient)` 之后调用一次。

- [ ] **Step 2: 移除重排路径中的强制复位**

`ResponsiveTestLayout.Apply` 只负责布局和尺寸计算，不在每次 Resize 结束时设置 `AutoScrollPosition`，防止用户操作期间跳回顶部。

- [ ] **Step 3: 运行静态验证和编译**

运行静态脚本和 Task 3 的 MSBuild 命令；期望脚本通过且 MSBuild 无错误。

### Task 5: 回归验证并交付

**Files:**
- Test: `tools/Test-LeftSidebarLayout.ps1`
- Existing: `tools/Test-ChannelGridFit.ps1`, `tools/Assert-UiDataNullSafety.ps1`

- [ ] **Step 1: 执行 UI 静态检查**

运行：`powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-LeftSidebarLayout.ps1`

期望：`Left sidebar static checks passed.`

- [ ] **Step 2: 执行已有 UI/数据安全检查**

运行：`powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-ChannelGridFit.ps1` 和 `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Assert-UiDataNullSafety.ps1`

期望：两个脚本均通过，且无因本次 UI 改动新增的失败。

- [ ] **Step 3: 复查工作区差异**

运行：`git diff --check; git status --short`

期望：无空白错误；只确认本次 UI 文件和新增验证脚本的差异，保留用户已有未提交业务改动。
