# 启动设备检查失败提示 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在启动设备检查完成后，向用户一次性显示失败设备和具体原因，同时保持现有后台检查和状态栏更新行为。

**Architecture:** `StartupDeviceCheckResult` 保存每个设备的状态和失败原因；底层检查函数记录异常或协议失败原因；UI 回调在 UI 线程先更新连接状态，再根据结果生成一次汇总提示。提示逻辑放在启动检查回调边界，避免后台线程直接弹窗和多次弹窗。

**Tech Stack:** VB.NET、WinForms、.NET Framework 4.8、PowerShell 静态回归脚本、MSBuild x86。

---

### Task 1: 为失败原因和汇总提示建立回归检查

**Files:**
- Modify: `tools/verify-control-board-status.ps1`

- [ ] **Step 1: 添加失败原因字段和汇总提示的静态断言**

断言 `StartupDeviceCheckResult` 包含失败原因集合；断言启动回调调用提示方法，且提示方法使用单次 `MessageBox.Show`，并包含“控制模块”和失败原因。

- [ ] **Step 2: 运行回归脚本确认当前实现失败**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\verify-control-board-status.ps1`

Expected: FAIL，提示尚未存在失败原因集合或启动失败汇总提示。

### Task 2: 让启动检查保留每个设备的失败原因

**Files:**
- Modify: `StartupDeviceService.vb`
- Modify: `Module1.vb`

- [ ] **Step 1: 在 `StartupDeviceCheckResult` 增加按设备保存原因的字典**

使用 `Dictionary(Of String, String)`，键采用 `SCM`、`34461`、`3561`、`PLC`、`MES`，并保留现有 `Errors` 兼容调用。

- [ ] **Step 2: 让 `RunCheck` 在返回失败码时记录通用原因**

异常记录异常消息；当操作返回非成功码但未抛异常时，记录“检查返回失败码”。已有 `CheckCom_SCM` 抛出的协议握手错误继续保留为具体原因。

- [ ] **Step 3: 让 `CheckCom_SCM` 把串口配置、打开失败和协议握手失败写入启动可见日志**

不改变返回码和通信协议；失败时记录端口名、波特率以及异常消息，仍关闭已打开的串口。

### Task 3: 在 UI 线程显示一次性失败汇总

**Files:**
- Modify: `Module1.vb`

- [ ] **Step 1: 扩展完成回调传递完整 `StartupDeviceCheckResult`**

保留现有五个状态码更新逻辑，同时把完整结果交给提示构造方法。

- [ ] **Step 2: 添加启动失败汇总文本构造方法**

按固定顺序输出控制模块、34461、3561、PLC、MES；只输出失败项；优先使用具体原因，没有原因时输出“检查返回失败”。

- [ ] **Step 3: 添加一次性 UI 提示保护**

在 UI 回调线程使用一个启动检查提示标志；有失败项时只调用一次 `MessageBox.Show`，全部成功时不调用。弹窗标题为“设备连接检查”，图标为警告，正文包含处理建议。

### Task 4: 验证

**Files:**
- Test: `tools/verify-control-board-status.ps1`
- Test: `tools/verify-status-regression.ps1`
- Test: `tools/Assert-WinFormsDesignerSerialization.ps1`

- [ ] **Step 1: 运行状态和提示静态回归检查**

Expected: 所有检查输出 `PASS`。

- [ ] **Step 2: 使用 x86 MSBuild 重建主项目**

Expected: 构建退出码为 0，无编译错误。

- [ ] **Step 3: 启动程序并检查日志**

Expected: 启动日志包含一次设备检查回调；控制模块失败时 `MiddleMachine_yyyyMMdd.log` 包含 COM14 配置、发送帧和超时原因，UI 显示一次汇总提示。

- [ ] **Step 4: 检查工作区差异和换行格式**

Run: `git diff --check`

Expected: 无空白错误；不修改与本任务无关的已有用户改动。
