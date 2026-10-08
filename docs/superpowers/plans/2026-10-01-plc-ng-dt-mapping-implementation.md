# PLC NG 挑选点位映射实施方案

> 本历史方案的 R170、全 DT 清零和物理矩阵映射约定已被 [客户握手流程设计](../specs/2026-10-01-plc-ng-dt-mapping-design.md) 取代，不应用于当前 PLC 自动流程。

**目标：** 确认并固定“最终 NG 标准编号 → PLC DT 点位”的单一转换边界，使 PLC 挑选位置与托盘标准编号一致。

**依据：** [技术优化方案](../specs/2026-10-01-plc-ng-dt-mapping-design.md)。当前 `PlcAutomationProtocol.BuildDtGradeFrame` 已有正确换算，实施以最小改动为原则。

**开始条件：** 用户确认本方案及三张图的托盘朝向一致。确认前不修改生产代码、不编写或运行测试。

## 阶段 1：锁定编号契约

涉及 `TrayChannelMapping.vb`、`frmTest.vb`、`PlcAutomaticTestState.vb`、`PlcAutomaticTestCoordinator.vb`、`PlcAutomationClient.vb`、`PlcAutomationProtocol.vb`。

1. 核对 `BuildAutomaticPassResult` 的 `ChannelNumber`、`CurrentNgChannels`、`WriteChannelGrade` 入参均为标准编号；核对设备号只在采集执行和结果归位环节使用。
2. 若发现某条生产路径把设备号送入 PLC，先在该路径边界转换为标准编号；不在 PLC 协调器里猜测输入编号类型。
3. 在已有 PLC 写入接口和换算处标清“标准编号”语义；仅在现有命名或注释容易误导时改动，不添加重复映射表。

**阶段结果：** 所有 PLC 写入调用都以标准编号为输入，标准号到 DT 地址仅由 `BuildDtGradeFrame` 换算一次。

## 阶段 2：固定点位和最终 NG 行为

拟检查或调整 `tools/Test-PlcTrayMappingRuntime.ps1`、`tools/Test-TrayChannelMappingRuntime.ps1`、`tools/Test-PlcNgHandoffRuntime.ps1`。用户确认后才编写和运行这些检查。

1. 在 DT 映射检查中锁定四角、标准 17 和全部 256 个唯一地址，并将现有“physical channels”描述改为“standard tray channels”。
2. 加入跨编号例子：设备 1 → 标准 16 → DT1241，设备 241 → 标准 1 → DT1001，防止未来误把设备号传给 PLC。
3. 用 PLC 假对象检查最终 NG 集合：初测 NG 保持 NG、初测 NG 复测为 OK、无 NG、多个角点 NG。断言清零 256 个点后，仅最终 NG 标准编号被写为 2，R161 在写入确认后发送。
4. 保留并检查 R170 等待和部分写入失败后的重试行为。若检查发现具体缺陷，仅修复相应边界，不改 PLC 握手流程。

**阶段结果：** 软件检查可以识别上下或左右颠倒、重复映射、设备号误入 PLC、复测后残留 NG 标记。

## 阶段 3：构建和现场验收

1. 构建现有 VB.NET x86 项目，并运行阶段 2 的定向检查以及现有 PLC 周期检查；记录每项结果。构建与自动检查均在用户确认后执行。
2. 在实机上用单点可控 NG 依次核对标准 1、16、241、256 对应 DT1001、DT1241、DT1016、DT1256，以及 PLC 实际挑选位置。每次只让一个点为 NG，避免辨认混淆。
3. 再核对一个中间点和一个“初测 NG、复测 OK”点。确认后记录设备通道、标准编号、DT 地址和 PLC 实际位置四列结果；如现场朝向不符，暂停上线并重新确定物理坐标关系。

**完成判定：** 软件检查通过，现场点位与标准编号一致，PLC 仅挑选最终 NG；未具备现场条件时明确记录“软件检查完成、实机验收待办”，不声称设备验证通过。

## 预期文件改动

- 必要时：`PlcAutomationProtocol.vb`、`PlcAutomationClient.vb`、`PlcAutomaticTestState.vb`、`PlcAutomaticTestCoordinator.vb` 中与标准编号语义直接相关的注释或参数名。
- 检查文件：`tools/Test-PlcTrayMappingRuntime.ps1`、`tools/Test-TrayChannelMappingRuntime.ps1`、`tools/Test-PlcNgHandoffRuntime.ps1`。
- 仅当检查证明现有换算有误时，才修改生产换算代码；不新增映射配置、数据库表或 UI 功能。
