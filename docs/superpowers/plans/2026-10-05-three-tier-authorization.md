# 三级权限实施计划

> **For agentic workers:** Implement each task in order in this checkout and preserve unrelated working-tree changes. Use test-first changes, then run the listed Windows PowerShell/.NET Framework checks.

**Goal:** Implement and verify operator, engineer, and administrator permissions from `docs/superpowers/specs/2026-10-05-three-tier-authorization-design.md`.

**Architecture:** Extend `UserRole` and `AppPermission` in `Authorization.vb`; keep role parsing and serialization in `UserAccountStore`; have `MDIForm` set menu availability from the central policy; check permissions again in forms and write handlers. Engineers can use the process-parameter page without writing communications, database, user, SCM, color, or instrument settings.

**Tech Stack:** VB.NET WinForms, .NET Framework, Windows PowerShell, MSBuild.

---

### Task 1: Extend the authorization contract tests

**Files:**
- Modify: `tools/Test-AuthorizationContract.ps1`

- [ ] Assert that `UserRole` defines `Engineer` and that `AppPermission` defines standard management, channel calibration, device diagnostics, process parameters, system settings, user management, and log export.
- [ ] Reflect the compiled policy for unauthenticated, operator, engineer, and administrator sessions. Verify the permissions from the approved matrix, including engineer denial for system settings/user management and operator denial for all engineering write functions.
- [ ] Parse `Engineer` in a three-column account row and assert malformed or unknown role values remain `Operator`.
- [ ] Verify user-role selector includes engineer and the MDI permission refresh enables the engineer functions only for engineer and administrator.
- [ ] Run `./tools/Test-AuthorizationContract.ps1 -StaticOnly` before production changes and confirm it fails on the missing engineer contract.

### Task 2: Add engineer role storage and role assignment

**Files:**
- Modify: `Authorization.vb`
- Modify: `UserAccountStore.vb`
- Modify: `frmUserManage.vb`
- Modify: `frmUserManage.Designer.vb`
- Test: `tools/Test-AuthorizationContract.ps1`

- [ ] Add `Engineer = 3` and the permission values named in the approved spec; make `CanAccess` deny unknown values and grant all permissions to administrators.
- [ ] Allow `SetSession` to accept engineer accounts, keep unauthenticated sessions reset, and preserve operator-name synchronization.
- [ ] Parse `Engineer` case-insensitively from three-column account rows; retain legacy two-column mapping and default unknown role text to operator.
- [ ] Add “工程师” to the role selector, map all three selected roles in both directions, and retain admin-only form/event checks.
- [ ] Run the authorization static contract and plaintext account static regression scripts.

### Task 3: Apply role policy to application entry points

**Files:**
- Modify: `MDIForm.vb`
- Modify: `MDIForm.Designer.vb` only if a permission label or menu item must change
- Modify: `frmDebug.vb`
- Modify: `FrmChannelCalibration.vb`
- Modify: `FrmLogQuery.vb`
- Test: `tools/Test-AuthorizationContract.ps1`

- [ ] Keep operator business menus available to all authenticated roles; enable debug, calibration, and process settings only for engineer/admin; enable initial system settings and user management only for admin.
- [ ] Recheck permissions in each MDI click handler, each protected form load, and each calibration write/action handler.
- [ ] Disable log export for operators and check permission again in the export method while preserving log viewing.
- [ ] When permissions refresh after logout or role switch, close any open engineer-only diagnostic or calibration window that the new role cannot access.
- [ ] Extend reflection checks to verify the central policy and MDI menu states for the four session states.

### Task 4: Restrict initial settings to the engineer process page

**Files:**
- Modify: `frmInit.vb`
- Test: `tools/Test-AuthorizationContract.ps1`

- [ ] Permit the form to open for engineers only when they can manage process parameters; when an engineer opens it, expose only `TabPage2` (“参数设置”). Admins continue to see all pages.
- [ ] Guard test-standard import and save handlers with `ManageTestStandards`.
- [ ] Add an engineer-only save path that validates and persists only test thresholds, measurement correction coefficients, barcode length, probe/process thresholds, channel delay, calibration resistance, temperature threshold, and station selection. Start from the current `Initsetting` so communication, database, user password, MES credentials, SCM timing, color, and instrument settings stay unchanged.
- [ ] Audit engineer saves as “保存工艺参数”; retain the existing administrator `TextToStruct` and full initial-settings save path.
- [ ] Add static and runtime checks that an engineer cannot enter system settings or change protected fields through the process save path.

### Task 5: Build and run regression checks

**Files:**
- No further source files unless a failure identifies a defect in Tasks 1–4.

- [ ] Run `./tools/Test-AuthorizationContract.ps1 -StaticOnly`.
- [ ] Run `./tools/Test-PlaintextUserAccounts.ps1 -StaticOnly`.
- [ ] Run the full authorization contract against the newly built AnyCPU executable and the full plaintext account contract against a temporary assembly copy.
- [ ] Build `MP-60A-RV-64Mem.vbproj` for AnyCPU using the available Visual Studio/MSBuild installation.
- [ ] Confirm `git diff --check` passes and confirm unrelated existing modifications remain untouched.
