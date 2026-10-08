# 初始化设置热应用实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** 初始化参数在安全空闲点保存后，通过刷新当前窗体和重建相关设备连接生效；仅保留确实无法热重载时的重启兜底。

**Architecture:** 用独立纯策略类比较 `Initset` 快照并描述变化。设置窗体调用 MDI 保存/应用协调器；测试运行期间先持久化新快照并在安全周期边界排队提交，PLC 自动控制器支持周期边界暂停。MDI 后台重检设备/数据库，测试窗体负责释放并重建它持有的运行时客户端与页面状态。设备检查结果明确区分已启动、成功和失败。

**Tech Stack:** VB.NET 4.8 WinForms、Windows PowerShell runtime verification scripts、MSBuild.

---

### Task 1: 参数变化分类和回归验证

**Files:**
- Create: `InitSettingsApplyPolicy.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Create: `tools/Test-InitSettingsApplyPolicy.ps1`

- [x] **Step 1: 写分类器运行时验证脚本**
- [x] **Step 2: 执行脚本确认当前程序集缺少分类器而失败**
- [x] **Step 3: 实现配置变化枚举和比较策略**
- [x] **Step 4: 将新 VB 文件加入项目并重新运行分类器脚本**

### Task 2: 安全门、待应用队列与保存后应用流程

**Files:**
- Modify: `frmInit.vb`
- Modify: `MDIForm.vb`
- Modify: `frmTest.vb`
- Create: `tools/Test-InitSettingsHotApplyRuntime.ps1`

- [x] **Step 1: 写保存入口运行时验证，覆盖忙碌拒绝、空闲保存与不退出进程**
- [x] **Step 2: 运行验证确认它暴露当前热应用缺口**
- [x] **Step 3: 增加测试窗体配置安全状态判断与 PLC 周期边界暂停**
- [x] **Step 4: 增加测试窗体释放和刷新运行时配置的入口**
- [x] **Step 5: 增加按 Initset 快照写入配置文件的保存重载**
- [x] **Step 6: 将通用重启确认改为保存、排队或立即应用流程**
- [x] **Step 7: 运行热应用入口验证**

### Task 3: 设备和数据库重检结果

**Files:**
- Modify: `StartupDeviceService.vb`
- Modify: `MDIForm.vb`
- Modify: `tools/Test-InitSettingsHotApplyRuntime.ps1`

- [x] **Step 1: 为重复/并发检查提供明确启动结果**
- [x] **Step 2: MDI 热应用时刷新设备状态并重跑数据库健康检查**
- [x] **Step 3: 后台检查失败时显示明确错误，不误报应用成功**
- [x] **Step 4: 运行相关 runtime verification scripts**

### Task 4: 完整验证与文档复核

**Files:**
- Review: `docs/superpowers/specs/2026-10-05-initial-settings-hot-apply-design.md`
- Review: `docs/superpowers/plans/2026-10-05-initial-settings-hot-apply.md`
- Verify: `MP-60A-RV-64Mem.vbproj` 与相关 PowerShell runtime scripts

- [x] **Step 1: 运行应用项目构建**
- [x] **Step 2: 运行新增分类与热应用验证脚本**
- [x] **Step 3: 运行现有设备/设置相关验证脚本**
- [x] **Step 4: 对照 V2.0 验收标准检查实现并复核工作区差异**



## 执行结果

- V2.0 实施完成，工程构建与策略分类、热应用安全门、PLC 周期边界暂停/恢复、设备失败弹窗检查均通过。
- `Test-FrmInitChannelColorsRuntime.ps1` 当前失败于 `frmInit.TopMost` 断言；该属性不属于本次热应用实现范围，需作为单独 UI 遗留项处理。
- 复核补齐工程师工艺参数保存分支、复测 3562 失败反馈，以及 PLC/扫码客户端重建失败提示；工程师参数现在也通过共享协调器在安全边界应用。
- 构建及检查使用临时输出目录；工作区原有未提交改动均予保留。
