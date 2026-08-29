# 快速启动 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让 `frmTest` 首屏先完成显示并保持响应，设备、数据库、Socket 和定时器在首屏之后后台启动。

**Architecture:** 启动入口只初始化状态和显示 MDI；`frmTest` 首屏使用一次性批量布局，首次 Paint 后通过一次性调度启动后台设备检查及非关键工作。设备检查结果只更新状态栏，不在 UI 回调中重连或弹窗。

**Tech Stack:** VB.NET 4.8, WinForms MDI, ThreadPool/Task, System.Timers.Timer, 现有设备通信与 OcvDataService。

---

### Task 1: 建立启动阶段计时与后台服务入口

**Files:**
- Create: `StartupTiming.vb`
- Create: `StartupDeviceService.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Modify: `Module1.vb:676-818`

- [ ] **Step 1: 添加可关闭的启动计时记录器**

记录 `main`、MDI 显示、frmTest Load、首屏 Paint、后台服务开始及各设备结束时间，写入现有启动日志；日志失败不得影响启动。

- [ ] **Step 2: 添加后台设备服务**

提供 `StartAsync(form)`、取消和一次性运行保护。服务在后台执行各设备检查，结果通过 `BeginInvoke` 传递给状态更新回调；服务不执行 UI 控件属性写入。

- [ ] **Step 3: 将启动入口改为只显示 UI**

`MainwindowsShowAsync()` 只读初始化配置和设置“检测将在首屏后开始”状态，不排队设备检查；保留独立的后台服务入口供首屏后和手动重试调用。

- [ ] **Step 4: 运行静态检查并提交**

```powershell
git add StartupTiming.vb StartupDeviceService.vb Module1.vb MP-60A-RV-64Mem.vbproj
git commit -m "perf: move startup device checks behind first screen"
```

### Task 2: 把设备结果回调变成纯 UI 更新

**Files:**
- Modify: `Module1.vb:741-818`
- Modify: `MDIForm.vb:353-356`

- [ ] **Step 1: 删除 UI 回调中的设备连接和弹窗**

`ApplyCheckResultsToUi()` 只根据结果设置状态文字、颜色和底部状态栏；删除 `ConnectFor34461()`、`ConnectFor3561()`、`MessageBox.Show()` 和 `MsgBox()`。

- [ ] **Step 2: 增加后台重试入口**

手动重试调用后台服务，不直接从按钮事件调用设备连接函数；关闭窗体或已有检查时忽略新请求。

- [ ] **Step 3: 限制启动检查耗时**

34461 启动检查不发送 `*RST`，只进行连接/身份查询；每项设备检查使用最多 3 秒的超时，保留测试阶段所需的正式连接和复位逻辑。

- [ ] **Step 4: 添加无弹窗回归检查并提交**

```powershell
.\tools\Test-StartupDeviceDeferral.ps1
git add Module1.vb MDIForm.vb
git commit -m "perf: keep startup device results nonblocking"
```

### Task 3: 将 frmTest 首屏和延后工作分离

**Files:**
- Modify: `frmTest.vb:71-147`
- Modify: `frmTest.vb:2339-2430`
- Modify: `MDIForm.vb:362-414`

- [ ] **Step 1: 保留 Load 中的首屏必要初始化**

保留控件创建、通道初始数据和基础布局；删除 Load 中的 `timer3.Start()`、`Timer4.Start()`、同步 `GetCN()` 及设备相关调用。

- [ ] **Step 2: 在首次 Paint 后只调度一次延后工作**

增加一次性 `StartPostFirstPaintWork()`，通过 `BeginInvoke` 启动完整样式、定时器、GetCN、数据库检查和后台设备服务；重复 Paint 不得重复启动。

- [ ] **Step 3: 记录首屏阶段时间点**

在 MDI Shown、frmTest Load、基础布局、首次 Paint 和后台启动处写入 `StartupTiming`。

- [ ] **Step 4: 验证启动路径并提交**

```powershell
.\tools\Test-FrmTestStartupIsolation.ps1
.\tools\Test-StartupRuntimeTiming.ps1
git add frmTest.vb MDIForm.vb
git commit -m "perf: defer noncritical frmTest startup work"
```

### Task 4: 减少首屏布局和 MDI 重排

**Files:**
- Modify: `ResponsiveTestLayout.vb:25-125`
- Modify: `frmTest.vb:2339-2430`
- Modify: `MDIForm.vb:253-414`

- [ ] **Step 1: 将 ResponsiveTestLayout 首次构建包在完整 SuspendLayout 中**

控件重设父容器、Dock、宽度和样式时禁止中间自动布局，恢复时使用 `ResumeLayout(False)`，末尾只执行一次必要布局。

- [ ] **Step 2: 只在实际尺寸变化时刷新左侧内容**

避免样式应用过程调用多次 `RefreshLeftContent()`；宽度未变化时不重新设置子控件宽度、不调用 `PerformLayout()`。

- [ ] **Step 3: 移除重复最大化与强制客户区重算**

首次显示后不再次设置当前最大化状态；MdiClient 使用 `Dock = Fill`，状态栏使用底部 Dock，保留 75ms Resize 防抖。

- [ ] **Step 4: 执行布局回归测试并提交**

```powershell
.\tools\Test-MdiGridPlacement.ps1
.\tools\Test-ResponsiveTestLayout.ps1
.\tools\Test-StartupResizePerformance.ps1
git add ResponsiveTestLayout.vb frmTest.vb MDIForm.vb
git commit -m "perf: reduce initial WinForms layout passes"
```

### Task 5: 构建和真实启动验收

**Files:**
- Modify only if verification finds a defect in the above files.
- Test: existing `tools/Test-*.ps1` startup, layout, reset and channel-grid checks.

- [ ] **Step 1: 编译 Debug 配置**

```powershell
msbuild .\MP-60A-RV-64Mem.sln /t:Build /p:Configuration=Debug /m
```

Expected: exit code 0.

- [ ] **Step 2: 运行静态回归集合**

运行启动隔离、MDI 布局、响应式布局、通道网格和关闭生命周期测试，所有测试通过。

- [ ] **Step 3: 运行真实程序启动探针**

确认主窗口先出现且保持响应；设备不可达时首屏不弹窗，状态栏在后台更新；启动日志能区分首屏时间和设备时间。

- [ ] **Step 4: 检查工作区差异并提交最终修改**

```powershell
git diff --check
git status --short
git add Module1.vb MDIForm.vb frmTest.vb ResponsiveTestLayout.vb StartupTiming.vb StartupDeviceService.vb MP-60A-RV-64Mem.vbproj
git commit -m "perf: complete fast startup implementation"
```
