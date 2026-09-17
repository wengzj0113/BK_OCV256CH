# Authorization and Permissions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task with checkpoints.

**Goal:** Implement the approved C-scheme permission model so unauthenticated users may use business functions, administrator-only functions are protected at both UI and code-entry levels, user roles persist in `UserData.txt`, and login/logout transitions update the whole application immediately.

**Architecture:** Add a small authorization module with `UserRole`, `AppPermission`, session state, and centralized policy decisions. Add a user-account store that reads the current two-column file format and the new three-column role format. Keep existing global operator fields synchronized for compatibility, while making `MDIForm` consume the centralized role and policy for UI state and protected event entry points.

**Tech Stack:** VB.NET WinForms, .NET Framework, existing SHA256 password format, UTF-8 BOM text storage, PowerShell regression scripts, MSBuild AnyCPU build.

---

### Task 1: Add failing authorization contract tests

**Files:**
- Create: `tools/Test-AuthorizationContract.ps1`
- Test against: `Authorization.vb`, `UserAccountStore.vb`, `MDIForm.vb`, `FrmLoginNew.vb`, `frmUserManage.vb`

- [ ] **Step 1: Write the failing test**

Create a PowerShell test that requires the following source and runtime contracts:

```powershell
Assert-Contains 'Authorization.vb' 'Public Enum UserRole'
Assert-Contains 'Authorization.vb' 'Public Enum AppPermission'
Assert-Contains 'Authorization.vb' 'Public Shared Function CanAccess'
Assert-Contains 'UserAccountStore.vb' 'Role'
Assert-Contains 'FrmLoginNew.vb' 'account.Role'
Assert-Contains 'frmUserManage.vb' 'ComboBoxRole'
Assert-Contains 'MDIForm.vb' 'RequireAdministrator'
Assert-Contains 'MDIForm.vb' 'logoutMenu'
```

The runtime part loads the built executable under Windows PowerShell/.NET Framework, creates the authorization type through reflection, and verifies:

```text
Unauthenticated: business permission = true, administrator permission = false
Operator:        business permission = true, administrator permission = false
Administrator:   business permission = true, administrator permission = true
```

- [ ] **Step 2: Run it to verify it fails**

Run:

```powershell
& .\tools\Test-AuthorizationContract.ps1
```

Expected: FAIL because `Authorization.vb`, `UserAccountStore.vb`, the role field, and logout entry do not yet exist.

- [ ] **Step 3: Commit**

Do not commit the failing test separately; keep it in the working tree while implementing the first production task.

### Task 2: Implement the authorization session and policy module

**Files:**
- Create: `Authorization.vb`
- Modify: `Module1.vb: global session compatibility fields and startup initialization`

- [ ] **Step 1: Add the role and permission types**

Create these exact public types in the `software` namespace:

```vb
Public Enum UserRole
    Unauthenticated = 0
    Administrator = 1
    Operator = 2
End Enum

Public Enum AppPermission
    UseBusinessFunctions = 0
    InitialSettings = 1
    Debug = 2
    UserManagement = 3
End Enum
```

- [ ] **Step 2: Add centralized session transitions**

Implement `AuthorizationService` with:

```vb
Public Shared ReadOnly Property CurrentRole As UserRole
Public Shared ReadOnly Property CurrentUserName As String
Public Shared Function IsAuthenticated() As Boolean
Public Shared Function IsAdministrator() As Boolean
Public Shared Sub SetSession(userName As String, role As UserRole)
Public Shared Sub ResetSession()
Public Shared Function CanAccess(permission As AppPermission) As Boolean
Public Shared Function RequireAdministrator(owner As IWin32Window) As Boolean
```

`SetSession` must reject blank usernames for authenticated roles and must synchronize `operatorName`. `ResetSession` must set the role to `Unauthenticated`, clear `CurrentUserName`, clear `operatorName`, and clear `Initsetting.curOperator`. `CanAccess` must allow `UseBusinessFunctions` for all three roles and allow the other three permissions only for `Administrator`.

- [ ] **Step 3: Replace startup session initialization**

Call `AuthorizationService.ResetSession()` after configuration loading in `Module1.main()` and the legacy `MainwindowsShow()` path. Keep `MDIForm.operTypeNum` as a compatibility value, but derive it from the authorization role when the UI is refreshed.

- [ ] **Step 4: Run the contract test**

Run:

```powershell
& .\tools\Test-AuthorizationContract.ps1 -StaticOnly
```

Expected: the authorization source and policy assertions pass; UI and user-store assertions remain pending until their tasks are complete.

### Task 3: Add role-aware user account storage with backward compatibility

**Files:**
- Create: `UserAccountStore.vb`
- Modify: `FrmLoginNew.vb: account creation, loading, authentication, and role assignment`
- Modify: `frmUserManage.vb: user load/save/update/delete`

- [ ] **Step 1: Define the account record and parser**

Create `UserAccount` with `UserName`, `PasswordHash`, and `Role`. Parse both formats:

```text
用户名,密码哈希
用户名,密码哈希,角色
```

For two-column records, map the exact username `系统管理员` to `Administrator` and every other user to `Operator`. Unknown role text must map to `Operator`, not administrator.

- [ ] **Step 2: Preserve password compatibility**

Keep the existing SHA256 salt and hash format. Continue accepting legacy plaintext rows only for one successful login, then rewrite that record as a hashed row with the role column. New files must be created with:

```text
用户名,密码哈希,角色
系统管理员,<hash>,Administrator
操作员,<hash>,Operator
```

- [ ] **Step 3: Return the authenticated account**

Change authentication to return `UserAccount` instead of only `Boolean`. `FrmLoginNew` must call `AuthorizationService.SetSession(account.UserName, account.Role)` after successful authentication and set `OperTypeNum` from `account.Role`; it must not compare the selected username to determine permissions.

- [ ] **Step 4: Make user management role-aware**

Add a `ComboBoxRole` and label to `frmUserManage.Designer.vb`, with `Operator` and `Administrator` choices. Load the stored role for the selected user. New and updated rows must write the selected role. Keep the built-in `系统管理员` account undeletable. The form remains administrator-only through the MDI guard and its own load-time guard.

- [ ] **Step 5: Run storage tests**

Extend `Test-AuthorizationContract.ps1` with temporary-file parser checks for two-column and three-column records, including an unknown role mapping to `Operator`.

Expected: old records load without data loss and new records retain the role column.

### Task 4: Apply policy to MDI UI and add logout

**Files:**
- Modify: `MDIForm.Designer.vb: add logoutMenu`
- Modify: `MDIForm.vb: login, logout, permission refresh, status text`

- [ ] **Step 1: Add the designer-owned logout menu item**

Add `logoutMenu As ToolStripMenuItem` in the designer, place it beside `loginMenu`, set text to `退出登录`, and initialize it disabled. Do not dynamically create this control in `MDIForm.vb`.

- [ ] **Step 2: Centralize MDI UI refresh**

Replace the current hard-coded `operTypeNum = 1` check with calls to `AuthorizationService.CanAccess`. The refresh method must set:

```vb
init.Enabled = CanAccess(InitialSettings)
btnInit.Enabled = CanAccess(InitialSettings)
btnDebug.Enabled = CanAccess(Debug)
ToolDebug.Enabled = CanAccess(Debug)
userMenage.Enabled = CanAccess(UserManagement)
logoutMenu.Enabled = AuthorizationService.IsAuthenticated()
loginMenu.Text = If(AuthorizationService.IsAuthenticated(), "切换用户", "登录")
operTypeNum = CInt(AuthorizationService.CurrentRole)
```

Business entries stay enabled for all roles under C-scheme semantics.

- [ ] **Step 3: Implement logout**

Add `logoutMenu_Click` that calls `AuthorizationService.ResetSession()`, refreshes the permission state and bottom status, and leaves testing/querying/MES entries enabled.

- [ ] **Step 4: Keep status display truthful**

`UpdateBottomStatus` must use `AuthorizationService.CurrentUserName`, with the exact fallback `未登录`. After login and logout, the status bar must refresh immediately.

### Task 5: Protect administrator event and form entry points

**Files:**
- Modify: `MDIForm.vb: init_Click, btnInit_Click, btnDebug_Click, ToolDebug_Click, userMenage_Click`
- Modify: `frmInit.vb: Load event`
- Modify: `frmDebug.vb: Load event`
- Modify: `frmUserManage.vb: Load event`

- [ ] **Step 1: Guard MDI handlers**

At the first executable line of each administrator handler, call:

```vb
If Not AuthorizationService.RequireAdministrator(Me) Then Return
```

The guard must show one clear message and never open the protected form for a non-admin role.

- [ ] **Step 2: Guard protected forms**

Each protected form's load path must close immediately when the current role is not administrator. This covers direct construction that bypasses the normal MDI menu handler.

- [ ] **Step 3: Add bypass tests**

Extend the runtime test to instantiate `MDIForm`, set role to `Unauthenticated`, `Operator`, and `Administrator`, invoke the private permission refresh through reflection, and assert the five administrator controls' `Enabled` values. Invoke each protected handler through reflection for a non-admin instance and assert no protected form is shown.

### Task 6: Verification and integration

**Files:**
- Modify: `tools/Test-AuthorizationContract.ps1` to include the final role, storage, UI, and bypass assertions
- Verify: existing UI, startup, color, and build checks

- [ ] **Step 1: Run the complete authorization test**

Run under Windows PowerShell/.NET Framework:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\Test-AuthorizationContract.ps1 -AssemblyPath .\bin\build-verify-authorization\XL_OCV_256CH.exe
```

Expected: `Authorization contract: PASS`.

- [ ] **Step 2: Build an isolated AnyCPU output**

Run:

```powershell
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe'
& $msbuild .\MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:OutputPath=bin\build-verify-authorization\ /p:IntermediateOutputPath=obj\build-verify-authorization-main\ /p:BuildProjectReferences=true /v:minimal
```

Expected: exit code 0. Existing assembly-version and x86/MSIL warnings may remain; no new compile errors are acceptable.

- [ ] **Step 3: Run existing regression checks**

Run the current-user, channel-color, designer, and startup checks. Record any pre-existing static-script mismatch separately from authorization results.

- [ ] **Step 4: Inspect the final diff**

Run `git diff --check`, inspect staged files, and confirm unrelated user changes such as existing `MDIForm.vb` formatting and `frmTest.Designer.vb` modifications are not overwritten.

- [ ] **Step 5: Commit the implementation**

Use separate commits for the authorization implementation and its verification updates:

```powershell
git add Authorization.vb UserAccountStore.vb FrmLoginNew.vb frmUserManage.vb frmUserManage.Designer.vb MDIForm.vb MDIForm.Designer.vb frmInit.vb frmDebug.vb
git commit -m "feat: enforce role-based application permissions"
git add tools/Test-AuthorizationContract.ps1
git commit -m "test: add authorization permission matrix checks"
```
