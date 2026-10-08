# Test Standard File Import and Export Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add import/save actions beside the voltage and resistance limits in `frmInit` and store portable UTF-8 `.set` standards under the application `SETS` directory.

**Architecture:** A pure profile/store module validates the four limits and reads/writes a versioned text format. `frmInit` configures standard file dialogs and moves values between the profile and the existing four text boxes; the existing OK/Cancel flow remains the only way to apply or discard imported settings. New controls are declared and laid out in `frmInit.Designer.vb`.

**Tech Stack:** VB.NET 4.8 WinForms, `System.IO`, invariant numeric formatting, PowerShell runtime verification, MSBuild.

---

## File Map

- Create `TestStandardProfile.vb`: four decimal limits, range validation, `.set` serialization/parsing, `SETS` path ownership, and atomic save.
- Modify `frmInit.Designer.vb`: add import/save buttons to `GroupBox6`, adjust its bounds and existing range-control positions only as needed, and register the buttons in the generated component declarations.
- Modify `frmInit.vb`: initialize the profile store, handle dialog interactions, parse/repopulate the four existing controls, and show actionable error messages.
- Modify `MP-60A-RV-64Mem.vbproj`: include `TestStandardProfile.vb` in the compile item group.
- Create `tools/Test-TestStandardProfileRuntime.ps1`: invoke the real compiled VB profile/store APIs and verify success and rejection cases using a temporary directory.
- Create `tools/Test-TestStandardFiles.ps1`: verify Designer ownership, project inclusion, button event wiring, and that the existing OK/Cancel commit boundary remains intact.

## Task 1: Define Runtime Tests for the Profile Store

**Files:**
- Create: `tools/Test-TestStandardProfileRuntime.ps1`

- [x] **Step 1: Write the failing runtime test**

The script accepts `-AssemblyPath`, loads `software.TestStandardProfile` and `software.TestStandardFileStore` by reflection, and throws if either type is absent. Create a temporary app directory and use the real API to assert:
  - `DirectoryPath` equals `<temp-root>\SETS` and is created by first save.
  - Saving profile values `3.25, 80.5, 2500, 4200` to `Cell-A` creates `Cell-A.set` with the format marker, version, four unit-specific keys, and invariant decimal points.
  - Reading that file returns the same four values.
  - Saving the same name with overwrite disabled fails; overwrite enabled replaces the file.
  - Negative values, inverted resistance/voltage ranges, missing keys, duplicate keys, unsupported versions, invalid numeric text, and a corrupt file are rejected.
  - The script removes only its unique temporary directory in `finally`.

- [x] **Step 2: Build the current project and run the test to confirm RED**

Run the repository's `MSBuild` executable against `MP-60A-RV-64Mem.vbproj` in Debug / Any CPU with output directed to `obj\test-standard-red\`. Then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-TestStandardProfileRuntime.ps1 -AssemblyPath obj\test-standard-red\XL_OCV_256CH.exe
```

Expected: the assembly loads, then the assertion fails because `software.TestStandardProfile` is not yet present.

## Task 2: Implement the Profile and File Store

**Files:**
- Create: `TestStandardProfile.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`
- Test: `tools/Test-TestStandardProfileRuntime.ps1`

- [x] **Step 1: Add `TestStandardProfile`**

Define public `Decimal` properties `ResistanceMin`, `ResistanceMax`, `VoltageMin`, and `VoltageMax`. Add `Validate()` that rejects values below zero and rejects either minimum greater than its corresponding maximum. Zero is valid.

- [x] **Step 2: Add `TestStandardFileStore`**

Constructor accepts an application root and exposes `DirectoryPath = Path.Combine(applicationRoot, "SETS")`. Implement:

```vb
Public Function Save(profile As TestStandardProfile,
                     standardName As String,
                     overwrite As Boolean) As String
Public Function Read(filePath As String) As TestStandardProfile
```

`Save` validates the profile and a leaf filename, adds `.set` when omitted, creates `SETS`, refuses an existing file when overwrite is false, serializes UTF-8 without BOM with invariant decimal values, writes a sibling temporary file, then atomically moves/replaces the target. `Read` accepts a selected file path, decodes UTF-8 strictly, requires the exact format marker/version and four unique required keys, parses invariant decimals, and validates before returning a profile. Do not return a partial profile or modify any form state.

- [x] **Step 3: Register the new file in the VB project**

Add `<Compile Include="TestStandardProfile.vb" />` once in the existing `<ItemGroup>` of source files.

- [x] **Step 4: Rebuild and run the runtime test to confirm GREEN**

Build into `obj\test-standard\`, then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-TestStandardProfileRuntime.ps1 -AssemblyPath obj\test-standard\XL_OCV_256CH.exe
```

Expected: `Test standard profile runtime verification passed.`

## Task 3: Place the Actions in the WinForms Designer

**Files:**
- Modify: `frmInit.Designer.vb`
- Test: `tools/Test-TestStandardFiles.ps1`

- [x] **Step 1: Add the static UI regression test and confirm RED**

Assert that the Designer contains `btnImportTestStandard` and `btnSaveTestStandard` as `Button` fields and places both in `GroupBox6`; assert the Designer contains no runtime-only replacement control panel. Run the script before editing and confirm the missing-control assertion fails.

- [x] **Step 2: Add buttons in Designer code**

Declare, instantiate, set text to `导入标准` / `保存标准`, assign `Handles`-compatible names, add both to `GroupBox6.Controls`, and position them at the right of the two range rows. Resize `GroupBox6` within `TabPage2`; adjust the existing range-entry controls only enough to preserve their current labels, units, and spacing. Do not create or position controls in `frmInit.vb`.

- [x] **Step 3: Run the static UI regression test**

Run `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-TestStandardFiles.ps1`.

Expected: Designer controls and container membership are found; the test remains red only for not-yet-added click handlers if that assertion is already included.

## Task 4: Wire Import and Save to Existing Parameters

**Files:**
- Modify: `frmInit.vb`
- Test: `tools/Test-TestStandardFiles.ps1`
- Test: `tools/Test-TestStandardProfileRuntime.ps1`

- [x] **Step 1: Extend the static test for the existing commit boundary and confirm RED**

Assert the two buttons have `Handles` click handlers; import calls the profile reader and assigns the four controls only after successful read; save reads those same four controls; no handler calls `SaveInitFile` or writes `Initsetting`; `CmdOK_Click` retains the existing commit path; `Button1_Click` retains cancel/close behavior.

- [x] **Step 2: Add the form-level helper methods and event handlers**

Create a `TestStandardFileStore` using `Application.StartupPath`. Configure `OpenFileDialog` with `InitialDirectory = DirectoryPath`, `.set` filter, and existence checks. Configure `SaveFileDialog` with the same initial directory, `.set` extension, and overwrite prompt; reject a chosen output directory outside `SETS` so the file always lands in the requested location. Before opening either dialog, ensure `SETS` exists and report access errors.

The import handler reads into a local profile, then updates `txtRes1`, `txtRes2`, `txtVol1`, and `txtVol2` together using invariant display values. Catch expected I/O/format/range errors and leave all four text boxes unchanged. The save handler parses the four inputs using the current UI culture, validates them, uses the selected filename stem as the standard name, and calls the store. Display success/failure without committing unrelated initialization parameters.

- [x] **Step 3: Extend the static test and run it to GREEN**

Run `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-TestStandardFiles.ps1`.

Expected: `Test standard file UI verification passed.`

- [x] **Step 4: Rebuild and rerun both focused verification scripts**

Build into `obj\test-standard\`, then run the runtime and static scripts. Expected: both pass and the project build exits with code 0.

## Task 5: Full Verification and Review

**Files:**
- Review: all files listed above
- Test: `tools/Test-TestStandardProfileRuntime.ps1`
- Test: `tools/Test-TestStandardFiles.ps1`

- [x] **Step 1: Run all repository PowerShell verification scripts**

Run each `tools/Test-*.ps1` script using PowerShell with `-NoProfile -ExecutionPolicy Bypass`; first inspect each script's parameters to supply any required build output. Record any baseline failures separately from failures introduced by this change.

- [x] **Step 2: Build the application in Debug / Any CPU**

Run the full `MSBuild` command for `MP-60A-RV-64Mem.vbproj` and confirm exit code 0 with no VB or Designer errors.

- [x] **Step 3: Review the final diff against the design**

Confirm only the `.set` profile bounds are imported/exported, the root `Initset.set` binary layout is unchanged, new controls are Designer-authored, automatic/manual still consume the same shared configured ranges, and no existing working-tree changes were overwritten.

**Verification outcome:** The focused Debug / Any CPU build succeeded. `Test-TestStandardProfileRuntime.ps1` and `Test-TestStandardFiles.ps1` both passed. The repository-wide PowerShell sweep was attempted; 55 scripts passed, 19 reported failures, and 2 app-startup scripts were skipped. Failures include existing/out-of-scope assertions and legacy script encoding or architecture limitations, so the repository-wide suite is not clean. The build also reports existing dependency-version and x86/MSIL architecture warnings.

## Self-Review

- Every specified behavior maps to Tasks 1–5.
- New source API names used by runtime tests and handlers are defined in Task 2.
- UI writes cannot bypass the existing OK/Cancel boundary.
- No task requires PLC hardware, external database access, or new packages.
