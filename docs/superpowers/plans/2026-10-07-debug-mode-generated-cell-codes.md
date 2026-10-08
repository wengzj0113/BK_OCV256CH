# No-Barcode Debug Mode Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Let `chk_Test` run real measurements without MES cell codes, save clearly classified debug tasks/results in OCV_DATA, show the latest debug result, preserve PLC handshakes, and suppress production MES uploads.

**Architecture:** Add a pure barcode policy for deterministic `TEST-yyyyMMdd-channel` identifiers. Snapshot the checkbox at a complete test run boundary and build a debug `TrayTestPlan` without MES cell lookup. Persist tasks as `TestType='Debug'` and results as `ResultSource='Debug'`; keep production and debug query paths separate and block both existing MES upload entry points in debug mode.

**Tech Stack:** VB.NET WinForms on .NET Framework 4.8, SQL Server stored procedures (SQL Server 2019 scripts and SQL Server 2008 R2 compatibility script), PowerShell contract/runtime tests, MSBuild.

---

## File Map

- Create `DebugBarcodePolicy.vb`: validate measurement channel numbers and generate the exact date/channel debug codes.
- Modify `MP-60A-RV-64Mem.vbproj`: include the new VB source file.
- Modify `frmTest.vb`: create and clear debug plans, hold the mode stable through manual tests and complete PLC tray cycles, classify database tasks/results, and suppress manual/automatic MES uploads.
- Modify `OcvDataService.vb`: pass debug query mode to OCV_DATA query procedures and filter date-based tray lists by task type.
- Modify `FrmInquery.vb` and `FrmInquery.Designer.vb`: add a default-off debug-result selector and refresh both the date tray list and result grid in the chosen mode.
- Modify `docs/sql/002_create_core_procedures.sql`: rank completed tasks independently by debug/production type, and ensure NG source lookup excludes debug tasks.
- Modify `OCV_DATA_SQLServer2008R2.sql`: keep the SQL Server 2008 R2 install script behavior aligned with the modern procedure source.
- Create `docs/sql/012_debug_mode_result_queries_sqlserver2008r2.sql`: update the two affected procedures on already-installed SQL Server 2008 R2 databases without rerunning the full install script.
- Modify `docs/sql/README.md`: document which procedure scripts to rerun for modern deployments and the new additive SQL Server 2008 R2 migration.
- Create `tools/Test-DebugModeContract.ps1`, `tools/Test-DebugBarcodePolicyRuntime.ps1`, `tools/Test-DebugModeRuntime.ps1`, and `tools/Test-DebugModeSqlContract.ps1`: cover mode wiring, code generation, query separation, and upload suppression.

## Task 1: Add and test debug barcode generation

**Files:**
- Create: `tools/Test-DebugModeContract.ps1`
- Create: `tools/Test-DebugBarcodePolicyRuntime.ps1`
- Create: `DebugBarcodePolicy.vb`
- Modify: `MP-60A-RV-64Mem.vbproj`

- [x] **Step 1: Add the failing barcode policy tests**

`DebugBarcodePolicy.vb` will expose `Public Shared Function CreateCodes(channels As IEnumerable(Of Integer), runDate As DateTime) As IDictionary(Of Integer, String)`. The runtime test will assert that channels 1, 16, and 256 map to `TEST-20261007-1`, `TEST-20261007-16`, and `TEST-20261007-256`; a repeated channel is emitted once; and channel 0 or 257 throws `ArgumentOutOfRangeException`.

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugBarcodePolicyRuntime.ps1`

Expected before implementation: FAIL because `software.DebugBarcodePolicy` is absent from the current application assembly.

- [x] **Step 2: Implement the minimal pure policy and add it to the project**

Use invariant date formatting and sorted distinct channels. Reject `Nothing` input and out-of-range channels. Include `DebugBarcodePolicy.vb` in the project compile list adjacent to `TrayTestPreparation.vb`.

- [x] **Step 3: Build and run the policy test**

Run: `msbuild MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /p:OutputPath=bin\build-verify-debug-mode\`

Then run: `$env:FRMTEST_EXE='bin\build-verify-debug-mode\XL_OCV_256CH.exe'; powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugBarcodePolicyRuntime.ps1`

Expected: build succeeds and the script prints `Debug barcode policy runtime: PASS`.

## Task 2: Integrate debug planning and mode lifetime into manual and PLC flows

**Files:**
- Modify: `frmTest.vb`
- Create: `tools/Test-DebugModeContract.ps1`

- [x] **Step 1: Add failing source contract assertions**

Assert the main form reads `chk_Test.Checked`, manual preparation does not call `GetTrayCellsByTrayCode` in debug mode, automatic preparation builds synthetic codes for all measurement channels, cancellation clears `activeTrayPlan` and `RecieveData(...).BatCode`, and the checkbox is disabled for a manual run or whole PLC cycle. Assert the automatic coordinator still calls the same `PrepareAutomaticTray`, `StartInitialPass`, `StartRetestPass`, and final cycle methods.

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugModeContract.ps1`

Expected before implementation: FAIL on missing debug plan/mode wiring.

- [x] **Step 2: Implement plan construction at test boundaries**

Add one `CreateDebugTrayPlan` helper in `frmTest.vb`. It will use the existing measurement channel list, call `DebugBarcodePolicy.CreateCodes`, create a `TrayTestPlan` with those channels as both measurement and occupied channels, and populate each corresponding `RecieveData(channel - 1).BatCode`. In `PrepareTrayForTest`, retain the current tray-code validation; branch to this helper only when the captured mode is debug. Clear any prior plan/codes before creating a plan. Keep the generated plan and codes unchanged across all automatic passes.

- [x] **Step 3: Lock and clear mode state safely**

Capture `chk_Test.Checked` when starting a manual run or PLC tray cycle. Disable the checkbox until that full operation has completed or failed. When the mode is unchecked, clear the debug plan/codes and require the normal real-code load path before formal startup. Do not change automatic PLC state transitions or handshake commands.

- [x] **Step 4: Run contract and runtime regression checks**

Run the new contract script and the existing barcode/planning checks:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugModeContract.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-FrmTestGetBarcode.ps1
$env:FRMTEST_EXE='bin\build-verify-debug-mode\XL_OCV_256CH.exe'; powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-TrayTestPreparationRuntime.ps1
```

Expected: all scripts pass; real MES preparation retains its current empty/duplicate/invalid-channel rejection behavior.

## Task 3: Mark OCV_DATA tasks/results and separate query paths

**Files:**
- Modify: `frmTest.vb`
- Modify: `OcvDataService.vb`
- Modify: `FrmInquery.vb`
- Modify: `FrmInquery.Designer.vb`
- Modify: `docs/sql/002_create_core_procedures.sql`
- Modify: `OCV_DATA_SQLServer2008R2.sql`
- Create: `docs/sql/012_debug_mode_result_queries_sqlserver2008r2.sql`
- Modify: `docs/sql/README.md`
- Create: `tools/Test-DebugModeSqlContract.ps1`

- [x] **Step 1: Add failing SQL and query contract checks**

Assert the latest-result procedure filters by an optional debug mode before ranking tasks, that formal queries exclude `TestType='Debug'`, and debug queries include only `TestType='Debug'`. Assert `usp_GetNgRetestSourceByBatteryCode` filters out debug tasks. Assert date tray-list queries accept the same debug selector. Assert the query page defaults to production and can select debug results.

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugModeSqlContract.ps1`

Expected before implementation: FAIL because query parameters and debug selectors are absent.

- [x] **Step 2: Mark test tasks and results at creation**

In `CreateOcvDataTestTask`, select `TestType='Debug'` when the captured debug mode is active; otherwise preserve `Formal`/`Retest`. Ensure the task code and idempotency key include the task type. In `SaveOcvDataTestResults`, set `ResultSource='Debug'` for debug runs and preserve current values for formal/retest runs.

- [x] **Step 3: Implement latest-result filtering and query UI**

Add an optional debug-mode parameter to `usp_GetTrayLatestStageResults`; filter task type before `ROW_NUMBER` ranking so debug tasks cannot mask production results. Extend `GetTrayLatestStageResults` and `GetTrayListByDate` to pass the selected mode. Add `chkDebugResults` to the query page, default unchecked; refresh the date tray list and grid on mode changes. The debug selector returns the newest completed debug task per real tray and OCV stage. Keep the current query schema and grading calculation intact.

- [x] **Step 4: Exclude debug NG records from production retest lookup**

Update `usp_GetNgRetestSourceByBatteryCode` to require a non-debug task. Add the procedure to the modern `002_create_core_procedures.sql` source because the application calls it, and keep the SQL Server 2008 R2 consolidated install script synchronized.

- [x] **Step 5: Add a SQL Server 2008 R2 additive migration**

Create `012_debug_mode_result_queries_sqlserver2008r2.sql` using SQL Server 2008 R2 compatible `DROP PROCEDURE IF EXISTS` guards expressed as `IF OBJECT_ID(...) IS NOT NULL DROP PROCEDURE`, `GO`, and `CREATE PROCEDURE` batches. Include only the two affected query procedures; do not alter tables or data. Update `docs/sql/README.md` with the required rerun/migration sequence.

- [x] **Step 6: Run SQL and query tests**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugModeSqlContract.ps1`

Run the existing database verification script only if a configured OCV_DATA test instance is available; otherwise report that live SQL execution was not available and rely on the script contract plus syntax inspection. Expected: formal latest-result and NG retest queries cannot select debug tasks; debug query returns only debug tasks.

## Task 4: Suppress both production MES upload paths in debug mode

**Files:**
- Modify: `frmTest.vb`
- Modify: `tools/Test-DebugModeContract.ps1`

- [x] **Step 1: Add failing upload contract checks**

Assert the manual upload call guarded by `FrmMesTask.chkResultDataUploadAssembly.Checked` and the PLC final-cycle `ResultDataUploadAssembly.StartTimer` call both require `Not debugMode`; assert OCV_DATA save and PLC finalization remain reachable while debug mode is active.

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugModeContract.ps1`

Expected before implementation: FAIL because both current upload paths are unconditionally eligible.

- [x] **Step 2: Guard manual and automatic upload starts**

Keep both upload configuration checks, but add the captured mode guard to each. Do not enqueue or start production MES work for a debug task. Do not stop/cancel a separate previously started formal upload. Keep PLC handshake logic untouched.

- [x] **Step 3: Run upload and PLC regressions**

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-DebugModeContract.ps1
$env:FRMTEST_EXE='bin\build-verify-debug-mode\XL_OCV_256CH.exe'; powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-PlcCycleRecoveryRuntime.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Test-MesUploadSnapshotRuntime.ps1
```

Expected: debug upload guards pass, PLC cycle callbacks remain unchanged, and existing formal MES upload behavior passes its regression checks.

## Task 5: Build and verify the integrated feature

**Files:**
- Modify as above only.

- [x] **Step 1: Build the x86 Debug application**

Run: `msbuild MP-60A-RV-64Mem.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform=x86 /p:OutputPath=bin\build-verify-debug-mode\`

Expected: exit code 0 and `bin\build-verify-debug-mode\XL_OCV_256CH.exe` exists.

- [x] **Step 2: Run all feature and focused regression scripts**

Run the four new debug scripts plus `Test-FrmTestGetBarcode.ps1`, `Test-TrayTestPreparationRuntime.ps1`, `Test-PlcCycleRecoveryRuntime.ps1`, `Test-PlcNgHandoffRuntime.ps1`, `Test-AutomaticOccupiedResultRuntime.ps1`, and `Test-MesUploadSnapshotRuntime.ps1`, setting `FRMTEST_EXE` to the new build for runtime scripts.

Expected: all feature-focused scripts and the x86 build pass. The unrelated `Test-FrmTestChannelSelectionRuntime.ps1` also fails against the pre-existing baseline binary because the left count label is not renamed. Live SQL execution was attempted but the local ODBC TLS handshake failed, so deployment validation remains unverified.

## Self-review and coverage

- Mode checkbox, default-off behavior, test-run locking, test plan cleanup: Tasks 1–2.
- Manual and automatic generated barcodes with repeat-safe task persistence: Tasks 1–3.
- OCV_DATA debug task/result marking, current result query, production isolation, NG retest isolation: Task 3.
- MES upload disabled for both manual and PLC automatic paths while preserving PLC handshake: Task 4.
- SQL Server 2019 deployment source and existing SQL Server 2008 R2 deployment path: Task 3.
- Build and focused regression verification: Task 5.

