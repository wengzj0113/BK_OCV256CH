# Manual barcode testing and barcode result updates implementation plan

> **For agentic workers:** Use subagent-driven-development for the independent database task and review. Preserve the existing dirty worktree; implementation branch is `codex/manual-barcode-save`. No commits of files containing pre-existing user edits.

**Goal:** Connect the independent manual testing form to tray barcodes, optional local persistence, explicit MES snapshots and global barcode result replacement.

**Architecture:** Keep the form as UI coordinator. Add a barcode plan plus immutable per-round snapshots and a local persistence service. Extend the existing MES uploader with an explicit request constructor. Change the central SQL result save procedure transactionally and ship a SQL Server 2008 R2 compatible upgrade.

**Tech Stack:** VB.NET/.NET Framework 4.8 WinForms, SQL Server TVPs/stored procedures, PowerShell, MSBuild x86.

## Task 1: Database result identity

- [x] Add `docs/sql/016_barcode_result_upsert.sql`; update `docs/sql/001_create_ocv_data_schema.sql`, `docs/sql/002_create_core_procedures.sql`, `OCV_DATA_SQLServer2008R2.sql`, and `docs/sql/README.md`. Preserve existing edits.
- [x] Normalize result barcodes before writing, reject empty or duplicate normalized barcodes and duplicate channels. Use transaction application lock with `@LockOwner='Transaction'`; roll back every change on failure.
- [x] For touched barcodes choose the newest existing result as canonical; redirect `OCV_RETEST_DATA.SourceTestResultId`, delete other rows for touched barcodes, update the canonical result in place, insert absent barcodes. Update latest task/tray/stage/channel/spec/result context and preserve result identity.
- [x] Replace task/channel unique index with a lookup index, add barcode lookup index. Reject target task/channel conflicts with an unrelated barcode. Permit incoming barcode/channel swaps.
- [x] Apply same lock and touched-barcode consolidation to NG retest when `OverwriteOriginal=1`, retaining stale-source checks and calibration snapshots. Leave audit-only retests unchanged.
- [x] Add transaction rollback SQL verification covering repeat saves, untouched/touched legacy duplicates, cross-task/stage updates, swapping channels, duplicates/empty input, and retest source remapping. Never execute upgrades against configured live databases automatically.

## Task 2: Barcode preparation and round snapshots

- [x] Add `ManualTestData.vb` and include it in the project adjacent to `FrmManualTest.vb`. Define `ManualTestBarcodePlan.Create(trayCode As String, table As DataTable, capturedAt As DateTime)` with read-only map and source flag. Generate `TEST-yyyyMMdd-<SHA256 tray prefix>-channel` for all channels only when no effective codes exist. Reject malformed/duplicate maps.
- [x] Define `ManualTestRoundSnapshot` and `ManualTestRunContext`. Clone selected `Bat_info` records and fix codes from the preparation map immediately after each measurement; retain full calibration/NG/time data and MES context.
- [x] Define a persistence service using `OcvDataService.CreateTestTask`, `SaveTestResults`, `CompleteTestTask` per completed round, with stable per-round task/idempotency keys. Use `Manual`/`ManualTest` for real data and `Debug`/`Debug` for generated data. Track partial local-save failures and never trigger MES on incomplete local persistence.
- [x] Add a focused runtime harness with empty/partial/invalid maps, stable tray-specific generated codes, sparse channel validation, immutable snapshots, and captured context isolation. First run must fail before implementation; rerun against the built assembly.

## Task 3: Explicit MES snapshot

- [x] Extend `ResultDataUploadAssembly.vb` with an explicit result snapshot/context constructor and outcome-returning upload method. Keep `StartTimer` and existing constructor compatible.
- [x] Payload uses only captured tray/channel/result/stage/process/operator/environment/time data. Use HTTP response plus `Return_Code='S'`; avoid using fixed-IP ping as an API gate. Queue current payload on network/HTTP/business failure; propagate queue failure to caller and distinguish uploaded from queued.
- [x] Never access/modify `frmTest` or global tray state from the explicit manual entry; generated runs do not create an uploader or outbox entry. Upload only the last completed real round after all selected rounds save locally, respecting existing MES enable/configuration settings.

## Task 4: Form and exports

- [x] Rename designer controls to `lblTrayCode`, `txtTrayCode`, `btnGetBarcode`; remove unused `Button1`; add compact status label and barcode result column. Fix the existing overlapping range/list order within the toolbar while keeping Designer ownership.
- [x] Implement guarded async tray lookup, scanner Enter handling, invalidation on tray edits, and validation of selected mapped channels before single/multi start. Query errors do not generate codes.
- [x] Capture run context before measurement, update table from round snapshot, prompt once after single/batch/partial failure. Declining performs no result/outbox writes. Save/upload is guarded so controls/close remain locked until it ends; report measuring, saving, and upload outcomes independently.
- [x] Keep multi-round Excel writing and automatic opening, including partial batches and file-open errors; these errors cannot bypass the database confirmation for completed measurements. Add barcode to worksheet, summary and CSV; format barcode cells as text to retain leading zeros and avoid formula interpretation.

## Task 5: Verification and review

- [x] Build using `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe MP-60A-RV-64Mem.vbproj /t:Build /p:Configuration=Debug /p:Platform=x86 /p:OutputPath=bin\build-verify-manual-barcode\` (expected: exit 0, zero compile errors).
- [x] Run the new runtime harness and relevant existing tray/calibration/NG tests against the verification assembly; expect all assertions to pass. Verify SQL script syntax with available SQL tooling; if no disposable SQL instance exists, report live DB concurrency/transaction validation as pending rather than claiming it passed.
- [x] Review staged/unstaged changes against pre-change snapshots, inspect API payload and simulation exclusion, and run `git diff --check`. Resolve spec and code quality review findings before delivery.
- [x] Deliver code changes, upgrade path and concise verification evidence, keeping the application/configured equipment and production databases untouched by test execution.
