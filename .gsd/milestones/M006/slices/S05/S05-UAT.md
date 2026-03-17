# S05: Main screen with meta world — UAT

**Milestone:** M006
**Written:** 2026-03-17

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: All behavior proven by edit-mode tests. Runtime integration in S06.

## Preconditions

- Unity compiles without errors

## Smoke Test

All DemoWiringTests + SceneControllerTests + ObjectRestoredPresenterTests pass.

## Test Cases

### 1. Object tapping spends golden pieces

1. Run DemoWiringTests — MainMenuPresenter_TapObject_SpendsGoldenPieces
2. **Expected:** Balance decreases by costPerStep, object progress increments

### 2. Object completion triggers popup

1. Run DemoWiringTests — MainMenuPresenter_TapObject_CompletesObject_ResolvesObjectRestored
2. **Expected:** ObjectRestored action resolves with object name

### 3. Insufficient balance prevents restoration

1. Run DemoWiringTests — MainMenuPresenter_TapObject_InsufficientBalance_NoChange
2. **Expected:** No progress change, no balance change

## Failure Signals

- Compile errors from IMainMenuView interface change
- Test failures in DemoWiringTests or SceneControllerTests
- Missing ObjectDisplayData or ObjectRestored types

## Requirements Proved By This UAT

- R045 — Main screen with environment, objects, balance, play button
- R048 — Golden pieces spent on object restoration
- R055 — Object restored celebration popup

## Not Proven By This UAT

- Blocked object display (tested via data, not visual)
- Runtime play-mode navigation (S06)
- Environment switching/progression (S06)

## Notes for Tester

DemoWiringTests now uses ScriptableObject test data — TearDown destroys instances properly.
