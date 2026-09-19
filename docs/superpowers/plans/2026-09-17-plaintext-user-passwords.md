# 明文用户密码实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将应用内账户文件改为保存和比较明文密码，并重置当前 Debug/AnyCPU 输出中的系统管理员与操作员账户。

**Architecture:** 保留现有 `UserAccountStore` 文件账户模型和角色字段，只把账户记录的第二列从哈希语义改为明文密码语义。登录、添加、修改和初始化统一使用直接字符串比较；运行时账户文件仍位于可执行文件目录，不把账户文件纳入 Git。

**Tech Stack:** VB.NET 4.8 WinForms、PowerShell contract tests、MSBuild/Visual Studio 2022。

---

### Task 1: 添加明文账户回归测试

**Files:**
- Create: `tools/Test-PlaintextUserAccounts.ps1`

- [ ] **Step 1: Write the failing test**

创建 PowerShell 测试，读取 `UserAccountStore.vb` 和 `FrmLoginNew.vb`，断言账户模型使用 `Password` 字段、账户存储不再调用 `HashPassword`、登录直接比较密码；在非 `-StaticOnly` 模式下，加载构建后的程序集，使用临时目录验证 `ParseLine`、`Authenticate` 和 `AddOrUpdate` 的明文行为。

- [ ] **Step 2: Run the test to verify it fails**

运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-PlaintextUserAccounts.ps1 -StaticOnly
```

预期：失败，因为当前源码仍包含 `PasswordHash`、`HashPassword` 和哈希比较逻辑。

### Task 2: 改造账户存储和登录

**Files:**
- Modify: `UserAccountStore.vb:6-113`
- Modify: `FrmLoginNew.vb:1-23`

- [ ] **Step 1: Implement direct plaintext storage**

在 `UserAccountStore` 中将账户属性改为 `Password`；初始化、解析、认证、保存和新增/更新全部直接使用密码字符串。删除旧的哈希匹配、明文登录后自动哈希升级逻辑。

- [ ] **Step 2: Remove obsolete hashing helper**

从 `FrmLoginNew.vb` 删除 `System.Security.Cryptography`、`System.Text` 引用和 `HashPassword` 函数，保留登录窗体的账户加载与认证调用。

- [ ] **Step 3: Run the static regression test**

运行同一个 `-StaticOnly` 命令，预期输出 `Plaintext account static contract: PASS`。

- [ ] **Step 4: Run the compiled behavioral regression test**

先编译 `Debug|x86`，再运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-PlaintextUserAccounts.ps1 -AssemblyPath .\bin\Debug\XL_OCV_256CH.exe
```

预期输出 `Plaintext account contract: PASS`，且临时账户文件中第二列不是 64 位哈希。

### Task 3: 写入当前构建输出的账户文件

**Files:**
- Modify: ignored runtime data files `bin\Debug\UserData.txt` and `bin\AnyCPU\UserData.txt`

- [ ] **Step 1: Reset the two requested accounts**

在两个输出目录分别写入系统管理员和操作员账户，角色分别为 `Administrator` 和 `Operator`，密码使用用户在当前会话确认的两组值。密码只写入被 Git 忽略的运行时数据文件，不写入源码、文档、提交信息或测试输出。

- [ ] **Step 2: Verify file format without printing passwords**

读取两个文件，仅检查用户名、角色、密码列长度和明文匹配结果；不得输出密码列内容。

### Task 4: 全量验证

**Files:**
- No additional source files.

- [ ] **Step 1: Build both configurations**

运行 `Debug|x86` 和 `Debug|Any CPU` 的 MSBuild，确认两个项目均成功生成。

- [ ] **Step 2: Run authorization and plaintext contracts**

运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-AuthorizationContract.ps1 -StaticOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-PlaintextUserAccounts.ps1 -AssemblyPath .\bin\AnyCPU\XL_OCV_256CH.exe
```

- [ ] **Step 3: Review the diff and preserve unrelated work**

确认只提交账户存储、登录窗体、回归测试和计划/设计文档；不暂存现有 `MDIForm.vb` 修改、`.rollback-backup-20260828`、`.superpowers` 或其他构建验证目录。

- [ ] **Step 4: Commit implementation files**

提交源代码和测试脚本，运行时 `UserData.txt` 保持本地未跟踪/被忽略状态。
