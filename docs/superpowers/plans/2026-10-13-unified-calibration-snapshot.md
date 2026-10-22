# Unified Calibration Snapshot Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Ensure each production measurement uses exactly one validated, channel-correct calibration compensation and stores raw, compensation, and final resistance consistently.

**Architecture:** Load a 256-channel immutable calibration snapshot before production testing is enabled. Active database values take precedence over the local CSV cache, while a valid cache supports database outages. A shared result-application step receives raw milliohms, the physical instrument channel, and the operation's snapshot; formal, manual, tray retest, and barcode NG retest use it. Persist the exact raw/compensation/final triple without reconstructing raw values from final values.

**Tech Stack:** VB.NET WinForms, SQL Server stored procedures and additive SQL migrations, existing CalibrationService, FormalTestRunner, TwoPassResultMapper, and TrayChannelMapping.

---

## File map

- Modify CalibrationService.vb and add a focused calibration snapshot model/provider file. Own loading, validation, precedence, atomic local cache writes, snapshot publication, and current snapshot access.
- Modify frmTest.vb and FormalTestRunner.vb. Load calibration before enabling tests, remove startup reset/shutdown overwrite, capture one snapshot per run, and persist the exact measured result triple.
- Modify TwoPassResultMapper.vb and Module1.vb. Apply compensation once using the physical device channel, preserve raw and final values separately, and keep legacy result fields compatible.
- Modify FrmChannelCalibration.vb and FrmChannelCalibration.Designer.vb. Read and publish snapshots through the provider and make designer column names match the names used by event handlers.
- Modify RetestModels.vb, Retest3562Service.vb, FrmNgRetest.vb, and NgRetestDataService.vb. Treat instrument output as raw, map the source tray channel to its physical device channel, apply the snapshot, and save the corrected triple.
- Update docs/sql/001_create_ocv_data_schema.sql, docs/sql/002_create_core_procedures.sql, the SQL Server 2008 R2 baseline script, and add an idempotent numbered migration for existing databases. Store the raw, compensation, and final values in NG retest history and update all three values when overwriting an original result.
- Update docs/sql/README.md with the migration order and deployment note.

## Task 1: Add the immutable calibration snapshot

- [ ] Define a snapshot with exactly 256 physical-device-channel entries, values in mΩ, a source per entry, load time, and a stable fingerprint.
- [ ] Resolve each channel from active OCV_DATA calibration first, then a valid CSV entry; represent missing calibration explicitly as an uncalibrated zero rather than a load failure.
- [ ] If the database is unavailable, accept a valid 256-entry CSV cache and mark the snapshot as cache-backed. If neither source is valid, report a load error instead of silently publishing an all-zero snapshot.
- [ ] Keep the existing CSV line format for compatibility. Write it to a temporary file and replace the old file only after all values pass validation.
- [ ] Publish a new in-memory snapshot only after loading or persistence succeeds. Do not allow callers to mutate snapshot entries.

## Task 2: Load calibration before production tests

- [ ] In frmTest_Load, replace CalibrateOperate, the following ReDim, and the zeroing loop with provider initialization after initial settings are loaded.
- [ ] Keep test-start controls disabled until a valid snapshot is available. Show whether values came from OCV_DATA or the local cache and whether channels remain uncalibrated.
- [ ] Remove SaveCalibrateArrayToFile() from frmTest_FormClosed; calibration persistence belongs to the calibration service save operation, not application shutdown.
- [ ] Ensure an operation captures CurrentSnapshot once and passes that same reference to its measurement mapper and result persistence path.

## Task 3: Unify raw and compensated results for tray measurements

- [ ] Change TwoPassResultMapper to accept a calibration snapshot instead of IList(Of Double).
- [ ] For each physical channel, set raw resistance from the instrument's mΩ value, compensation from the snapshot at deviceChannel - 1, and final resistance to raw + compensation exactly once.
- [ ] Continue mapping the result to the tray's standard channel with TrayChannelMapping.ToStandardChannel.
- [ ] Keep RecieveData(...).Resistance as the corrected final value for existing grading and UI behavior; preserve raw and compensation alongside it for storage.
- [ ] Route formal, automatic, manual, and tray retest measurement entry points through the same snapshot-aware runner.
- [ ] Update any still-reachable legacy serial result parser to produce the same raw/compensation/final values or remove its duplicate compensation path.

## Task 4: Make calibration-page operations publish valid snapshots

- [ ] Set the designer column Name values to Selected, ChannelNo, CurrentValue, Measured, Standard, Compensation, CalibratedAt, and Status, matching the semantic names used in FrmChannelCalibration.vb.
- [ ] Render the current provider snapshot in LoadGrid; do not reload a separate mutable global array.
- [ ] On save or clear, validate all 256 values, persist to OCV_DATA, update the local cache atomically, and only then publish the new snapshot.
- [ ] On a failed database save, retain the previous snapshot and clearly report failure. Do not leave the running test provider with unsaved page values.
- [ ] Block calibration acquisition while a production measurement is active, so a run cannot observe a mid-operation calibration change.

## Task 5: Include barcode NG retest

- [ ] Keep Retest3562Service.Measure() responsible for returning raw resistance in mΩ; it must not apply channel compensation internally because it has no source-channel context.
- [ ] In FrmNgRetest, after looking up the original NG result, map Source.ChannelNo from tray/standard channel to physical device channel with TrayChannelMapping.ToDeviceChannel.
- [ ] Capture one current snapshot when the retest starts; compute FinalResistance = RawResistance + Compensation and classify OK/NG using the final resistance.
- [ ] Display final resistance and expose raw and compensation values in result details and logs.
- [ ] Pass raw, compensation, and final values to NgRetestDataService and the stored procedure. When overwriting the source result, update all three TestResult columns together.

## Task 6: Persist exact result triples

- [ ] In SaveOcvDataTestResults, read raw and compensation from the result produced by the same run snapshot. Remove finalResistance - calibrationValue reconstruction.
- [ ] Preserve the existing TestResult fields RawResistanceMilliohm, CalibrationValueMilliohm, and FinalResistanceMilliohm and pass the unrounded calculation values to them.
- [ ] Add RetestRawResistanceMilliohm and CalibrationValueMilliohm columns to OCV_RETEST_DATA; keep RetestResistance as the final corrected value for existing queries and reports.
- [ ] Extend usp_SaveNgRetest parameters and its transaction so retest history and optional overwrite update the same raw/compensation/final triple atomically.
- [ ] Add an idempotent additive migration for existing OCV_DATA databases; update clean-install and SQL Server 2008 R2 scripts to the same shape.

## Task 7: Source review and acceptance checks

- [ ] Trace every production result-producing entry point and confirm it passes one snapshot to the shared result-application step.
- [ ] Check the channel mapping for both a direct physical-channel measurement and a barcode retest whose stored channel is a tray/standard channel.
- [ ] Check the invariant FinalResistanceMilliohm = RawResistanceMilliohm + CalibrationValueMilliohm for formal and NG retest persistence, including values near the existing display saturation boundary.
- [ ] Confirm application startup no longer zeros loaded calibration and application shutdown cannot rewrite calibration values.
- [ ] Confirm invalid/missing calibration sources produce an explicit startup/test block and valid CSV fallback produces a visible cache-source status.
- [ ] Do not add or execute automated tests unless the user separately requests test or verification work; report any remaining compile or hardware validation as outstanding.
