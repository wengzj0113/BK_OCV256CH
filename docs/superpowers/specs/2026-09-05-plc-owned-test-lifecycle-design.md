# PLC-owned test lifecycle

## Scope

The PLC owns fixture preparation and all post-test process actions. The upstream PLC workflow must prepare and close the fixture before the operator is allowed to start the upper-computer test.

## Design

- `frmTest.StarTest` will not query PLC connection state or fixture state before the measurement.
- `frmTest.StarTest` will not close the fixture or wait for fixture motion before the measurement.
- `frmTest.StarTest` will not open the fixture after measurement or during normal test cleanup.
- The upper computer will retain its middle-machine communication check and formal measurement/result processing.
- The existing middle-machine completion wait remains the measurement completion boundary. No new PLC completion command will be invented because no such PLC interface exists in the current code; PLC-side sequencing remains responsible for receiving/acting on its own completion signal.
- Manual fixture controls and alarm safety handling remain unchanged unless they are part of the test-entry path.

## Failure and cleanup behavior

Measurement failures still return through the existing UI cleanup path, re-enable controls, clear the testing display state, and report the measurement/database error. They must not issue a PLC open/close command as a recovery action.

## Verification

- Add a regression check that the test-entry block contains no PLC preflight, close, wait, or post-test open call.
- Rebuild the x86 Debug solution.
- Run the existing channel-selection, frmTest initialization, and Designer serialization checks.
