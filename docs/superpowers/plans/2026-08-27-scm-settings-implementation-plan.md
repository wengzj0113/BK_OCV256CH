# SCM 控制板时间参数设置 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 `frmInit` 的静态 `TabScmSet` 中实现 SCM 五个毫秒时间参数的读取、逐项验证写入、成功后本机保存以及旧配置兼容。

**Architecture:** `SerialMiddleMachineClient` 负责协议寄存器、状态判断、连续读写和失败上下文；`frmInit` 负责静态控件事件、输入校验、界面状态和成功后更新 `Initsetting`；`Module1.Initset` 负责追加字段并兼容旧版二进制配置。失败不回滚 SCM 已成功写入的前置项，也不更新本机配置。

**Tech Stack:** VB.NET WinForms、`SerialPort`、AA/Modbus CRC16 协议、现有 PowerShell 静态验证脚本、MSBuild/Visual Studio 工程。

---

### Task 1: 建立 SCM 设置的失败测试

**Files:**
- Create: `tools/verify-scm-settings.ps1`
- Reference: `SerialMiddleMachineClient.vb`, `frmInit.vb`, `frmInit.Designer.vb`, `Module1.vb`

- [ ] **Step 1: 写入静态行为断言脚本**

脚本读取四个生产文件，并断言以下行为：五个寄存器和范围存在；`ReadScmTimingSettings` 读取两个区间；`WriteScmTimingSettings` 检查状态并按五项顺序调用验证写入；Designer 声明五个输入框、两个按钮、状态标签并挂入 `TabScmSet`；`Initset`、保存和读取路径包含五个字段和旧文件默认值。

```powershell
$ErrorActionPreference = 'Stop'

function Assert-Contains([string]$text, [string]$pattern, [string]$message) {
    if ($text -notmatch $pattern) { throw $message }
}

$client = Get-Content -LiteralPath 'SerialMiddleMachineClient.vb' -Raw
$form = Get-Content -LiteralPath 'frmInit.vb' -Raw
$designer = Get-Content -LiteralPath 'frmInit.Designer.vb' -Raw
$module = Get-Content -LiteralPath 'Module1.vb' -Raw

@(
    @{ p = 'RegRelaySettleMs\s+As\s+UShort\s*=\s*&H33'; m = 'missing relay register' },
    @{ p = 'RegChannelSettleMs\s+As\s+UShort\s*=\s*&H34'; m = 'missing channel register' },
    @{ p = 'RegAutoStepMs\s+As\s+UShort\s*=\s*&H36'; m = 'missing instrument register' },
    @{ p = 'RegTriggerPulseMs\s+As\s+UShort\s*=\s*&H37'; m = 'missing trigger register' },
    @{ p = 'RegPhaseGuardMs\s+As\s+UShort\s*=\s*&H6C'; m = 'missing phase guard register' },
    @{ p = 'ReadScmTimingSettings'; m = 'missing SCM read method' },
    @{ p = 'WriteScmTimingSettings'; m = 'missing SCM write method' },
    @{ p = 'WriteRegisterAndVerifyStatus\(RegRelaySettleMs'; m = 'relay is not verified' },
    @{ p = 'WriteRegisterAndVerifyStatus\(RegPhaseGuardMs'; m = 'phase guard is not verified' }
) | ForEach-Object { Assert-Contains $client $($_.p) $_.m }

Assert-Contains $form 'Handles\s+btnScmRead\.Click' 'read event is not wired'
Assert-Contains $form 'Handles\s+btnScmApply\.Click' 'apply event is not wired'
Assert-Contains $designer 'TabControl1\.Controls\.Add\(Me\.TabScmSet\)' 'SCM tab is not in TabControl'
@(
    @{ p = 'btnScmRead'; m = 'missing read button' },
    @{ p = 'btnScmApply'; m = 'missing apply button' },
    @{ p = 'lblScmStatus'; m = 'missing status label' },
    @{ p = 'TabScmSet\.Controls\.Add\(Me\.txtScmRelaySettleMs\)'; m = 'relay textbox is not static Designer control' }
) | ForEach-Object { Assert-Contains $designer $($_.p) $_.m }
Assert-Contains $module 'SCMComRate' 'unexpected settings source'
Assert-Contains $module 'ScmRelaySettleMs' 'SCM fields are not declared'
Assert-Contains $module 'SaveInitFile\(\)[\s\S]*ScmRelaySettleMs' 'SCM fields are not persisted'
Assert-Contains $module 'ReadInitFile\(\)[\s\S]*ScmRelaySettleMs' 'SCM fields are not loaded'
Write-Host 'SCM settings static verification passed.'
```

- [ ] **Step 2: 运行测试确认当前实现失败**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify-scm-settings.ps1`

Expected: FAIL because `SerialMiddleMachineClient.vb` does not yet contain the SCM timing methods and the Designer tab is still empty.

### Task 2: 增加协议模型、寄存器读写和错误上下文

**Files:**
- Modify: `SerialMiddleMachineClient.vb:23-70,286-390`

- [ ] **Step 1: 增加参数模型和明确范围**

在 `SerialMiddleMachineClient` 前增加公开 `ScmTimingSettings` 类型，属性使用 `Integer`，提供默认值构造方法和 `Validate()`；范围必须精确为 5-5000、0-5000、10-10000、1-1000、100-10000。增加 `ScmTimingWriteException`，携带 `ParameterName`、`RegisterAddress`、`Value` 和 `CompletedCount`。

- [ ] **Step 2: 增加五个寄存器常量**

在现有寄存器常量区域增加：

```vb
Private Const RegRelaySettleMs As UShort = &H33
Private Const RegChannelSettleMs As UShort = &H34
Private Const RegAutoStepMs As UShort = &H36
Private Const RegTriggerPulseMs As UShort = &H37
Private Const RegPhaseGuardMs As UShort = &H6C
```

同时保留已有状态寄存器常量，使配置状态读取使用 `RegBasicStatusStart` (`0x0040`)。

- [ ] **Step 3: 增加 SCM 参数读取方法**

实现 `Public Function ReadScmTimingSettings() As ScmTimingSettings`：调用 `ReadRegisters(RegRelaySettleMs, 5)` 取得 `0x0033~0x0037`，调用 `ReadRegisters(RegPhaseGuardMs, 1)` 取得 `0x006C`，按属性顺序转换并返回。读取不写入本机配置。

- [ ] **Step 4: 增加状态名称和可配置状态判断**

允许状态只包括 `0`、`6`、`100`。对 `1/2/3/4/7` 以及其他值抛出包含十六进制状态值的异常，并提示先停止测试。

- [ ] **Step 5: 增加逐项写入方法**

实现 `Public Sub WriteScmTimingSettings(settings As ScmTimingSettings)`：先调用 `Validate()`，再读取 `0x0040`，然后严格按 `0x0033`、`0x0034`、`0x0036`、`0x0037`、`0x006C` 顺序调用现有 `WriteRegisterAndVerifyStatus`。每项失败时抛出 `ScmTimingWriteException` 并停止后续写入，不回滚；连续写入之间等待 100ms。现有重试、写回显和 `0x005B/0x005C` 验证逻辑继续复用。

### Task 3: 先写失败测试，再实现配置持久化

**Files:**
- Modify: `Module1.vb:130-155,1070-1145,1160-1260`
- Modify: `tools/verify-scm-settings.ps1`

- [ ] **Step 1: 扩展静态测试覆盖字段顺序和默认值**

在测试脚本中断言 `Initset` 有五个 `Integer` 字段，`SaveInitFile` 按五字段顺序追加写入，`ReadInitFile` 在字段不存在时分别得到 5、50、10、1、100 默认值。

- [ ] **Step 2: 运行测试确认新增持久化断言失败**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify-scm-settings.ps1`

Expected: FAIL because `Initset` and binary save/load do not yet have the five timing fields.

- [ ] **Step 3: 在 `Initset` 结构体追加字段**

追加以下字段，放在现有配置字段尾部，避免改变旧字段顺序：

```vb
Dim ScmRelaySettleMs As Integer
Dim ScmChannelSettleMs As Integer
Dim ScmAutoStepMs As Integer
Dim ScmTriggerPulseMs As Integer
Dim ScmPhaseGuardMs As Integer
```

- [ ] **Step 4: 追加保存字段并兼容读取旧文件**

在 `SaveInitFile()` 末尾按上述顺序写入五个整数。在 `ReadInitFile()` 读取现有 `Retest3562Rate` 后，逐字段判断 `FS.Position < FS.Length`；缺失时使用协议默认值。读取到越界值时也回退到对应默认值，不让无效值进入界面。

### Task 4: 先写失败测试，再实现静态 Designer 界面和窗体事件

**Files:**
- Modify: `frmInit.Designer.vb:130-170,1300-1480`
- Modify: `frmInit.vb:1-320`
- Modify: `tools/verify-scm-settings.ps1`

- [ ] **Step 1: 扩展静态测试检查控件完整性**

断言五个输入框、五个单位标签、两个按钮和状态标签均在 Designer 中声明，并且每个输入框通过 Designer 的 `TabScmSet.Controls.Add` 加入卡片；断言 `frmInit.vb` 不包含 `New TextBox`、`New Label`、`Controls.Add` 等运行时创建代码。

- [ ] **Step 2: 运行测试确认 Designer/UI 断言失败**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify-scm-settings.ps1`

Expected: FAIL because `TabScmSet` currently has no child controls and no event handlers exist.

- [ ] **Step 3: 在 Designer 中静态加入控件**

增加五个 `TextBox`：`txtScmRelaySettleMs`、`txtScmChannelSettleMs`、`txtScmAutoStepMs`、`txtScmTriggerPulseMs`、`txtScmPhaseGuardMs`；增加对应标签和 `ms` 单位标签；增加 `btnScmRead`、`btnScmApply`、`lblScmStatus`。设置明确的 Location、Size、Name、TabIndex、Text，并将全部控件通过 Designer 代码加入 `TabScmSet`。

- [ ] **Step 4: 初始化本机值和默认值**

在 `frmInit_Load` 中调用一个窗体私有的 `NormalizeScmTimingSettings()`，对 `Initsetting` 中缺失或越界值使用默认值；`structToText()` 将五个本机值显示到输入框。将 `TempInitsetting = Initsetting` 作为 `TextToStruct()` 的起始状态，确保点击原有“确定”不会把未应用的 SCM 输入覆盖成零值。

- [ ] **Step 5: 实现读取按钮事件**

`btnScmRead_Click` 创建现有 `SerialMiddleMachineClient(frmMDI.Com_SCM)`，调用 `ReadScmTimingSettings()`，成功后仅更新五个输入框和状态标签；失败时保留输入框值并显示异常原因。操作期间禁用两个 SCM 按钮，使用 `Try...Finally` 恢复。

- [ ] **Step 6: 实现应用并保存按钮事件**

先把五个输入框解析为整数并调用同一模型的 `Validate()`；验证失败时不打开串口、不发送写帧。验证通过后调用 `WriteScmTimingSettings()`；成功后把模型值写入 `Initsetting` 和 `TempInitsetting`，调用 `SaveInitFile()`，再显示成功信息。捕获 `ScmTimingWriteException` 时显示已完成数量、失败参数、寄存器地址、目标值和原因，不更新 `Initsetting`。不捕获后继续写入，也不执行回滚。

### Task 5: 运行红绿测试、构建和协议静态验证

**Files:**
- Modify: `tools/verify-scm-settings.ps1` only if an observed failure identifies a real requirement gap

- [ ] **Step 1: 运行静态验证并确认通过**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify-scm-settings.ps1`

Expected: `SCM settings static verification passed.` and exit code 0.

- [ ] **Step 2: 编译解决方案**

Run: `msbuild MP-60A-RV-64Mem.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`

Expected: exit code 0，且无新增 VB 编译错误。

- [ ] **Step 3: 检查协议帧和配置范围**

用现有 `SerialMiddleMachineClient` 的帧构造与 CRC 逻辑检查五个地址的写帧仍为 AA/06、大端寄存器和值、CRC 低字节在前；确认 `0x0033~0x0037` 连续读取不会包含错误地址，`0x006C` 单独读取。

- [ ] **Step 4: 做硬件联调检查清单**

在 IDLE 读取并核对五值；在 IDLE/DONE/ERROR 应用边界值；在运行等待和 PAUSE 应用确认拒绝；实测中途失败确认停止后续写入且本机配置不变；重启软件确认成功应用值仍显示。若无硬件连接，只报告构建和静态验证结果，不宣称硬件联调通过。

- [ ] **Step 5: 复查差异并请求代码审查**

确认只包含本功能相关的生产文件、验证脚本和文档；保留现有用户未提交改动。对完成的功能变更执行代码审查，修复 Critical/Important 问题后再交付。
