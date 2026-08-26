# Full-Channel Test Timeout and Speed Design

## Goal

Allow the 256-channel formal test to wait up to 300 seconds and configure the middle machine for the fastest channel-switching mode.

## Design

- Change `FormalTestRunner`'s default timeout from 30 seconds to 300 seconds.
- Change middle-machine plan configuration calls from `stableSpeed=True` to `False`, which writes `0x0038=0`.
- Keep existing register read-back verification; it will validate the selected speed value.
- Do not change channel grouping, measurement items, trigger sequencing, or cleanup behavior.

## Verification

- Add a source-level regression check because this legacy VB project has no automated unit-test project.
- Build the solution after the change.
- Confirm the generated/logged configuration contains `0x0038=0` and the default formal-test timeout is `300000` ms.
