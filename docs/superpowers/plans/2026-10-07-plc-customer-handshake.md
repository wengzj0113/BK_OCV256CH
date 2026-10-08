# PLC 客户握手流程重整实施计划

> **For agentic workers:** 按顺序执行；每个行为先写失败检查并观察到预期失败，再改生产代码。不要覆盖工作区中与本计划无关的改动。

**目标：** 按客户确认的 R150/R160、R151/R161、R152/R162 三段握手运行自动测试，并仅向顺序 NG DT 地址写入档位 2。

**架构：** 保留 `PlcAutomaticTestCoordinator` 作为唯一流程状态机和现有 PLC 串口请求锁。`PlcAutomationProtocol` 负责帧和 NG 逻辑通道到顺序 DT 地址映射；`PlcAutomationClient` 提供 R152/R162 操作；`frmTest` 负责扫码失败时的手工输入 UI。

**技术栈：** VB.NET WinForms、MEWTOCOL ASCII、PowerShell 源码/运行时回归脚本、现有 .NET Framework x86 构建。

---

## 文件职责

- 修改 `PlcAutomationProtocol.vb`：R152/R162 帧常量；逻辑 NG 序号 n 映射到 DT(1000+n)；保留参数边界检查。
- 修改 `PlcAutomationClient.vb`：接口增加 `ReadR152`、`WriteR162` 并复用已有同步串口读写和 ACK 校验。
- 修改 `PlcAutomaticTestState.vb`：表达等待 R152、NG 写入/R162 完成及测试完成/R161 的分阶段状态。
- 修改 `PlcAutomaticTestCoordinator.vb`：扫码回退、测试后 R161、R152 门控、只写 NG=2、R162 后才开放后续 R151。
- 修改 `tools/Test-PlcV4Protocol.ps1`、`tools/Test-PlcV4StateMachine.ps1`、`tools/Test-PlcNgHandoffRuntime.ps1`：更新过时契约并覆盖新握手时序。
- 修改 `docs/XL-OCV-256CH-系统软件使用手册.html`、`docs/superpowers/specs/2026-10-01-plc-ng-dt-mapping-design.md`：以客户流程替代 R170、DT 清零、物理矩阵转置等旧说明。

## Task 1：先锁定协议与顺序 DT 映射

**先改测试：** 在 tools/Test-PlcV4Protocol.ps1 断言含 R152 读取帧 `%01#RCSR0152**`、R162 写入帧 `%01#WCSR01621**`；断言协议暴露 NG 专用档位 2 组帧/写入 API，不暴露可写 OK=0 的通用 DT API；断言映射表达式为 1000 + ngChannelNumber，通道 1、2、256 分别是 DT1001、DT1002、DT1256。

- [x] 运行 `powershell -ExecutionPolicy Bypass -File tools/Test-PlcV4Protocol.ps1`，确认因新帧/新映射断言失败。
- [x] 在 PlcAutomationProtocol.vb 增加帧常量并将组帧器收紧为 BuildNgSelectionFrame(ngChannelNumber)，地址计算为 1000 + ngChannelNumber，固定使用 grade=2，仅验证 NG 序号 1–256。
- [x] 重跑协议脚本，确认新断言通过；保留串口格式、BCC/ACK 和 CR 处理断言。

## Task 2：扩展串口客户端接口

**先改测试：** 更新协议脚本断言客户端实现 ReadR152、WriteR162 和 WriteNgSelection(ngChannelNumber)，写 R162 要求 WC ACK；确认所有 IPlcAutomationClient 的运行时假对象都实现新接口。

- [x] 运行协议脚本确认新接口断言失败。
- [x] 在 IPlcAutomationClient 和 PlcAutomationClient 添加 R152 读取、R162 写入和 NG 专用 DT 写入，复用 SendAndReceive、ParseSignal、WriteAndRequireAck，不新增串口所有者或并行请求路径。
- [x] 重跑协议检查，并用帧构造检查确认读写地址及值均准确。

## Task 3：覆盖托盘码失败的手动回退

**先改测试：** 在 `Test-PlcNgHandoffRuntime.ps1` 的假扫码器增加空码、错误长度和抛异常场景；断言触发一次手工回调，输入有效码才写 R160，输入取消/无效时不写 R160、不启动测试。

- [x] 对照原 BeginTrayCodeRead 确认扫码异常与无效码直接进入 Fault，手工回调虽存在但未被调用。
- [x] 在 `BeginTrayCodeRead` 对扫码异常或 `ValidateTrayCode=False` 调用 `host.RequestManualTrayCode`；确认回调接受与取消均保持协调器状态线程安全，接受码仍经过同一校验和 `AcceptTrayCode` 路径。
- [x] 重新运行运行时脚本，确认正常扫码、手工成功、取消、错误码均符合断言。

## Task 4：按客户时序重整协调器

**先改测试：** 将 `HandoffPlc` 替换为可控 R150/R151/R152 和记录 R160/R161/R162/DT 操作顺序的假对象。断言：R160 在有效托盘码之后；R151 高触发一次测试；测试/复测与结果发布完成后先发 R161；未到 R152 不写 DT/R162；R152=1 后只为最终 NG 写 grade 2；全部 NG ACK 前无 R162；R162 前下一次 R151 不启动测试，R162 ACK 后才开放。

- [x] 基于初始代码确认旧流程等待 R170、在 R161 前写 DT；为新握手添加 HandoffPlc 顺序断言并运行通过。
- [x] 更新状态枚举和 `AutomaticTestCycle` 阶段标志，避免 R161 重试重复测量/保存，或 R162 重试重复写入已确认之外的结果。
- [x] 调整轮询状态：R150 扫码→校验/手工回退→R160；等待 R150 复位并接收 R151；完整测试/复测后发布结果并发送 R161；需分选时等待 R152；按排序序号写最终 NG=2；写入成功后发送 R162；确认后才允许后续 R151。
- [x] 非分选阶段继续跳过 R152/DT/R162，按现有阶段配置发送 R161。
- [x] DT 单项失败时保留待发送 NG 集合并重试；确认 R162 未成功前不进入允许后续 R151 的状态。零 NG 时不写任何 DT，仍在 R152=1 后发送 R162。
- [x] 运行手握运行时脚本与 `tools/Test-PlcV4StateMachine.ps1`，修复所有不匹配项。

## Task 5：更新过时协议文档及相关回归

- [x] 搜索手册、设计规格和 PLC 脚本中的 `R170`、`R161` 分选交接、DT 全区清零、矩阵转置映射，并仅更新属于 PLC 自动分选契约的内容。
- [x] 保留设备通道与软件托盘标准号在测量数据归位中的映射；仅 PLC 挑选 DT 使用客户规定的顺序 NG 序号，明确两者边界。
- [x] 运行 `tools/Test-PlcV4Protocol.ps1`、`tools/Test-PlcV4StateMachine.ps1`、`tools/Test-PlcNgHandoffRuntime.ps1`、`tools/Test-PlcCycleRecoveryRuntime.ps1`、`tools/Test-PlcTrayMappingRuntime.ps1`，逐项记录结果。
- [x] 构建 `MP-60A-RV-64Mem.vbproj` 的 x86 Debug 配置，确认接口实现、WinForms 回调与 VB.NET 编译通过。
- [x] 最后搜索生产源码和用户手册，确认流程代码不再读取 R170、不再以 DT=0 清零、不再在 R152 之前写 NG、R162 ACK 前不开放后续 R151。

## 现场验收限制

当前没有 PLC 串口抓包或实机可供本地验证。R152/R162 报文帧格式需沿用现场已有 R151/R161 合同，并在接机时核对；PLC 是否在下一托盘前清除 DT 旧值也需现场确认。软件只按客户要求写 NG=2，不发送 OK=0。

## 执行结果

- `MSBuild.exe MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /m:1 /v:minimal`：构建成功。输出包含既有 System.Net.Http、System.Runtime、System.Reflection 等程序集版本冲突警告，没有编译错误。
- `tools/Test-PlcV4Protocol.ps1`：通过。
- `tools/Test-PlcV4StateMachine.ps1`：通过。
- `tools/Test-PlcNgHandoffRuntime.ps1`（32 位 PowerShell）：通过，含失败重试、无 NG、无分选和手工托盘码情景。
- `tools/Test-PlcCycleRecoveryRuntime.ps1`（32 位 PowerShell）：通过。
- `tools/Test-PlcTrayMappingRuntime.ps1`（32 位 PowerShell）：通过，验证 256 个 NG 序号映射到 256 个连续 DT 地址，同时保留测量用的托盘/设备通道转换。
- 实机 PLC 与旧 DT 标记清除行为尚未验证。
