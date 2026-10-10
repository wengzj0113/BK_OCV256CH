# LES30 NTC Temperature Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Adapt the existing temperature acquisition path to the LES30 NTC RS485 module while reading and displaying only the two wired temperature channels.

**Architecture:** Keep Modbus RTU function 04, slave address 1, 9600/8N1, first register 0, and a two-register read. Correct the LES30 signed-tenths decoder and sensor-fault marker in the client, align the form with two active channels, retain the four-column local CSV shape with blank values in columns 3 and 4, and ensure the device is configured for Celsius.

**Tech Stack:** VB.NET WinForms, NModbus RTU, existing PowerShell source/runtime verification scripts, attached LES30-NTC-RS485 V1.0.2 manual.

---

## Scope and decisions

- This implementation covers the current two-sensor installation. Registers 0 and 1 are read; the module's additional channels and 32-bit float map are out of scope.
- Keep fixed factory communication settings: Modbus station 1, 9600 baud, no parity, 8 data bits, one stop bit. Do not expand the settings-file format or enable the baud selector in this change.
- Treat Celsius as the supported device unit. Verify the LES30 manual's unit register encoding before implementation. Read the unit setting using its documented function/register and reject Fahrenheit with a clear message; do not silently convert or rewrite the module setting.
- A successful Modbus response with register `0x7FFF` is a successful link and a sensor fault for that channel. A timeout, bad Modbus response, or serial-port error is a communication fault for both active channels.
- Preserve local result CSV field count/order: temperature columns 1 and 2 contain active readings; columns 3 and 4 are empty strings.
- Do not change JIG/IO polling, alarm policy outside the active temperature channels, or unrelated modified files.

## Files in scope

- Modify `NtcTemperatureModuleClient.vb`: documentation, register count/read error, LES30 decoder and Celsius-unit validation.
- Modify `frmTest.vb`: accept exactly two readings, update only the two active displays, distinguish per-sensor fault from communication fault, and supply empty strings for inactive CSV temperature fields.
- Modify `frmInit.vb`: make the connection-test result accurately report Modbus link status and channel sensor status if its existing handler currently conflates them. Keep the current fixed communication defaults.
- Create `tools/Test-NtcTemperatureProtocol.ps1`: focused regression checks for the decoder and two-channel contract, following existing reflection/runtime script conventions.
- Do not modify `Module1.vb` unless inspection during implementation finds that persistence currently changes the required fixed temperature defaults; if needed, make only the smallest backward-compatible correction and record it in the review.

## Implementation tasks

### Task 1: Add protocol regression checks

**Files:**
- Create: `tools/Test-NtcTemperatureProtocol.ps1`
- Read: `NtcTemperatureModuleClient.vb`, `MP-60A-RV-64Mem.vbproj`

- [ ] **Step 1: Confirm the project assembly and source inclusion convention**

Read the project file to establish how `NtcTemperatureModuleClient.vb` is compiled and which already-built assembly can be loaded by the PowerShell scripts. Do not edit generated build directories.

- [ ] **Step 2: Add decoder checks against the public shared decoder**

Load the built application assembly (`bin\Debug\XL_OCV_256CH.exe` for the Debug|AnyCPU configuration) and invoke `NtcTemperatureModuleClient.DecodeRegister` by reflection, as existing runtime scripts do. Assert:

```text
0x00FA -> IsValid=True, TemperatureCelsius=25.0
0xFFFF -> IsValid=True, TemperatureCelsius=-0.1
0xFFF6 -> IsValid=True, TemperatureCelsius=-1.0
0x7FFF -> IsValid=False
0x8000 -> IsValid=True, TemperatureCelsius=-3276.8
```

The final boundary assertion verifies signed Int16 semantics, not a plausible physical sensor reading. Physical-range rejection is not part of this task.

- [ ] **Step 3: Add source-contract assertions for the active protocol**

Assert the client uses function 04, start register 0, count 2, and factory serial settings 9600/None/8/One. Assert that the form accepts two readings, has no four-reading requirement in the temperature update path, and emits two blank inactive temperature fields for local result rows. Keep assertions focused on behavior and avoid pinning unrelated source formatting.

- [ ] **Step 4: Run the new regression script against the current build**

Run from the repository root: `powershell.exe -ExecutionPolicy Bypass -File .\tools\Test-NtcTemperatureProtocol.ps1 -AssemblyPath (Resolve-Path .\bin\Debug\XL_OCV_256CH.exe).Path`

Expected before implementation: decoder checks fail for `0xFFFF`, `0x7FFF`, and negative values; protocol-contract checks identify the existing four-reading display requirement. The script must fail with the specific assertion names, not an unhandled reflection exception.

### Task 2: Correct the LES30 protocol client

**Files:**
- Modify: `NtcTemperatureModuleClient.vb`

- [ ] **Step 1: Correct stale client documentation and response validation**

Change the class summary to say it reads the first two wired channels. Keep `TemperatureRegisterCount = 2`. Change the incorrect response-length message from “不是4” to “不是2” (or equivalent wording that does not imply four channels).

- [ ] **Step 2: Implement signed 16-bit tenths decoding**

Check `registerValue = &H7FFFUS` first and return `IsValid = False`. For every other register, reinterpret the 16-bit bit pattern as signed `Int16`, divide by `10D`, and return the result in `TemperatureCelsius`. Do not special-case `0xFFFF`; it must decode to -0.1℃.

- [ ] **Step 3: Validate the module unit is Celsius**

Use the exact LES30 manual mapping for holding register `0x0085` and function 03. Confirm the documented C/F enum values from the manual before coding. At client initialization or before the first temperature read, read that register and cache the result for the client lifetime. Continue only for Celsius. For Fahrenheit or an unknown enum, throw an `InvalidOperationException` with a clear Chinese message that instructs the operator to set the module to Celsius. Do not write configuration registers.

- [ ] **Step 4: Run protocol regression checks**

Run from the repository root: `powershell.exe -ExecutionPolicy Bypass -File .\tools\Test-NtcTemperatureProtocol.ps1 -AssemblyPath (Resolve-Path .\bin\Debug\XL_OCV_256CH.exe).Path`

Expected: all decoding boundary cases pass, and the protocol source contract confirms two registers/function 04 with fixed factory parameters.

### Task 3: Align polling and visible channels with two readings

**Files:**
- Modify: `frmTest.vb`
- Read: `frmTest.Designer.vb`

- [ ] **Step 1: Change the temperature update contract to exactly two readings**

In `UpdateTemperatureReadings`, require a non-null array of length 2. Treat an unexpected length as a protocol/data contract failure and route it through the existing communication-error path. Update only `t1` and `t2`; remove the four-label loop from this path. Keep `t3` and `t4` hidden and untouched.

- [ ] **Step 2: Scope communication failure to the active channels**

In `UpdateTemperatureCommunicationError`, set only `t1` and `t2` to “通信故障”. Do not update hidden channel labels. Ensure sensor marker `0x7FFF` renders “探头故障” only for its channel and does not turn the other channel into a communication failure.

- [ ] **Step 3: Keep the existing 5-second acquisition lifecycle**

Do not change timer interval, interlocked overlap guard, alarm suppression, client disposal/retry, or shutdown disposal. Verify these remain in place after editing so the two-channel fix does not alter acquisition cadence or port lifecycle.

- [ ] **Step 4: Run focused regression checks**

Run from the repository root: `powershell.exe -ExecutionPolicy Bypass -File .\tools\Test-NtcTemperatureProtocol.ps1 -AssemblyPath (Resolve-Path .\bin\Debug\XL_OCV_256CH.exe).Path`

Expected: the active-channel contract passes; inspection confirms `temperatureTimer` remains 5000 ms and its client is disposed on read failure and shutdown.

### Task 4: Preserve the local CSV contract without stale hidden values

**Files:**
- Modify: `frmTest.vb`

- [ ] **Step 1: Clear inactive temperature values at the serialization boundary**

Change the `FormatLocalResultRows` call in `SaveTestDataLocalFile` to pass the current `t1` and `t2` values followed by two empty strings. Keep the formatter's four-temperature argument validation and preserve CSV column ordering and count.

- [ ] **Step 2: Verify row serialization contract**

Run the focused regression script and verify it asserts the fourth and fifth temperature CSV fields (temperature fields 3 and 4) are empty, while the first two match the active display values. Existing consumers that expect four temperature columns must still parse each row.

### Task 5: Make the connection test report link and sensor state accurately

**Files:**
- Modify: `frmInit.vb` only if required by inspection
- Modify: `NtcTemperatureModuleClient.vb` only if a small result distinction is needed

- [ ] **Step 1: Trace the existing temperature “测试连接” handler**

Confirm it creates the same client with the selected COM port and calls the same two-register read. Preserve the existing behavior that no COM port selection means no test attempt.

- [ ] **Step 2: Separate link success from sensor faults in the result message**

If Modbus returns both registers, report communication success. Include channel-specific “探头故障” state for any `0x7FFF` result. Represent the unit mismatch with a dedicated `NtcTemperatureUnitException` (derived from `InvalidOperationException`) so the handler can report that the module responded but must be set to Celsius; do not report this as a cable/port failure. For serial/Modbus exceptions, report communication failure with the existing user-facing error mechanism.

- [ ] **Step 3: Confirm persisted settings remain fixed and stable**

Inspect save/load code and verify temperature COM port continues to persist and reload, while baud remains 9600. Do not add fields or alter the binary settings layout. Confirm the temperature client and test button use that same selected COM port and fixed 9600/8N1 parameters.

### Task 6: Build and validate the integrated change

**Files:**
- Read-only validation of changed files and the application project

- [ ] **Step 1: Build the application project**

Run the repository's established build command for `MP-60A-RV-64Mem.vbproj` using the project's configured target framework and platform.

Expected: build succeeds without new warnings attributable to the temperature changes.

- [ ] **Step 2: Run focused protocol regression checks**

Run from the repository root: `powershell.exe -ExecutionPolicy Bypass -File .\tools\Test-NtcTemperatureProtocol.ps1 -AssemblyPath (Resolve-Path .\bin\Debug\XL_OCV_256CH.exe).Path`

Expected: decoder, two-channel, CSV, and fixed-parameter assertions pass.

- [ ] **Step 3: Run relevant existing verification scripts**

Run only the existing checks that cover the touched paths, including `tools/Test-InitSettingsApplyPolicy.ps1` if the assembly or temperature connection settings policy was affected. Do not run the full unrelated suite as part of this scoped change.

- [ ] **Step 4: Perform hardware acceptance with the LES30 module**

With module station 1, 9600/8N1, and Celsius selected, verify:

1. Connection test reads both registers and reports link success.
2. `25.0℃`, `-0.1℃`, and another negative temperature display correctly.
3. Disconnecting one sensor makes only its channel show “探头故障”; the other remains live.
4. Disconnecting the RS485 link marks both active channels “通信故障”.
5. Setting the module to Fahrenheit produces the explicit unit warning and no misleading Celsius display.
6. The main window refreshes every five seconds, channels 3 and 4 remain hidden, and local CSV rows keep four temperature columns with the last two blank.
7. Close and reopen the settings dialog; the selected COM port remains persisted.

Record hardware model suffix, module station/unit/serial settings, observed register values, and pass/fail for each item. Software-only build and scripts do not count as hardware communication verification.

## Change safety

The working tree already contains unrelated modifications, including `frmTest.vb`, plus untracked build/rollback directories. Before implementation, inspect the existing diff for every in-scope file and preserve unrelated edits. Do not reset, checkout, clean, stage, or commit unrelated user changes. Limit implementation edits to the files listed above and review the final diff for accidental changes.

## Completion criteria

- The documented LES30 register examples decode exactly, including `0xFFFF = -0.1℃` and `0x7FFF = sensor fault`.
- Main acquisition and connection test both use function 04/registers 0–1 and the same persisted COM port with 9600/8N1/station 1.
- The form updates only the two active channels and differentiates sensor fault from communication fault.
- Temperature CSV columns 3 and 4 are empty while the established four-column shape remains intact.
- Celsius configuration is validated and Fahrenheit cannot silently appear as Celsius.
- Build and focused checks pass; hardware results are reported separately and only after running the acceptance checklist on the real module.
