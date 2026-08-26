# 控制板独占 COM12 通信实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 让上位机只通过 COM12 与控制板通信，由控制板自行控制 IO 板，上位机不再直接访问 IO 板。

**Architecture:** COM12 只由 `Com_SCM`/控制板客户端使用；启动检查只验证控制板串口；所有 IO 板函数改为旁路兼容入口，避免旧测试流程继续打开或读写 `Com_IOCard`。控制板协议仍由现有 `SerialMiddleMachineClient` 负责，非控制板数据不参与控制板响应判断。

**Tech Stack:** VB.NET WinForms、`System.IO.Ports.SerialPort`、现有控制板协议客户端、MSBuild Debug 编译。

---

### Task 1: 固定串口职责并停止 IO 板端口访问

**Files:**
- Modify: `MDIForm.Designer.vb`
- Modify: `MDIForm.vb`
- Modify: `Module1.vb`

- [ ] 保留 `Com_SCM` 作为 COM12 唯一通信对象，启动检测只调用 `CheckCom_SCM()`；不调用 `CheckCom_IOCard()`。
- [ ] 释放主窗体时只关闭控制板串口，避免 `Com_IOCard` 与 COM12 发生第二次打开。
- [ ] 保留旧 IO 字段和入口的编译兼容性，但不再执行 `Open`、`Close`、`Write`、`Read`。
- [ ] 将 IO 状态改为“由控制板控制/未启用”，不把 IO 板失败加入启动失败列表。

### Task 2: 旁路测试流程中的 IO 板调用

**Files:**
- Modify: `Module1.vb`
- Modify: `frmTest.vb`

- [ ] 将 `IOCardSetAcirOrOcv`、`IOCardSetOperation`、`IOCardSetCh`、`IOCard_ReadDoDi`、`IOCard_ReadDi` 改为记录旁路日志并立即返回，不访问串口。
- [ ] 删除测试流程中对 IO 板回复内容的读取和校验，保留控制板的模式/通道指令。
- [ ] 确保 OCV/ACIR 流程继续发送控制板对应指令，由控制板完成 IO 板动作。
- [ ] 保留函数签名，避免历史窗体和编译引用失效。

### Task 3: 增加回归检查并编译验证

**Files:**
- Create/Modify: `tools/verify-control-board-only.ps1`

- [ ] 检查启动路径不再调用 `CheckCom_IOCard()`。
- [ ] 检查生产代码不包含 `Com_IOCard.Open/Write/Read/Close` 调用。
- [ ] 检查控制板客户端仍使用 `Com_SCM` 和 `AA` 协议帧。
- [ ] 执行 `MSBuild Battery Testing.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'`，要求 0 error。

### Task 4: 运行验证

- [ ] 启动 Debug 构建，确认 COM12 只被控制板客户端使用。
- [ ] 确认底部状态显示控制板连接状态，IO 板不再显示为通信失败。
- [ ] 在无 IO 板直接响应的条件下验证启动、控制板寄存器读写和关闭流程不弹 IO 通信错误。
- [ ] 检查控制板日志，确认没有 IO 板串口读写记录。

