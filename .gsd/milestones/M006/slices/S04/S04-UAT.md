# S04: LevelComplete, LevelFailed, and ad/IAP stub popups — UAT

**Milestone:** M006
**Written:** 2026-03-17

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: All popup behavior proven by edit-mode tests. No runtime required.

## Preconditions

- Unity compiles without errors

## Smoke Test

All 20 popup tests + 4 controller tests pass in edit-mode test runner.

## Test Cases

### 1. LevelComplete shows golden pieces

1. Run LevelCompletePresenterTests — Initialize_SetsScoreLevelAndGoldenPieces
2. **Expected:** Score, level, and "+5 Golden Pieces" text set correctly

### 2. LevelFailed WatchAd option

1. Run LevelFailedPresenterTests — WaitForChoice_WatchAdClicked_ReturnsWatchAd
2. **Expected:** WatchAd choice resolves correctly

### 3. RewardedAd stub flow

1. Run RewardedAdPresenterTests — WaitForResult_WatchClicked_ReturnsTrue
2. **Expected:** Watching returns true, skipping returns false

### 4. IAPPurchase stub flow

1. Run IAPPurchasePresenterTests — WaitForResult_PurchaseClicked_ReturnsTrue
2. **Expected:** Purchase returns true, cancel returns false

## Failure Signals

- Compile errors from WinDialog/LoseDialog reference (should all be renamed)
- Test failures in PopupTests or InGameTests

## Requirements Proved By This UAT

- R051 — LevelComplete popup with golden pieces earned
- R052 — LevelFailed popup with retry/watch-ad/quit
- R053 — Rewarded ad stub popup (watch/skip)
- R054 — IAP purchase stub popup (purchase/cancel)

## Not Proven By This UAT

- Runtime popup display in play mode (S06)
- SceneSetup.cs UI creation for new popups (S06)
- ObjectRestored popup (S05)

## Notes for Tester

Old WinDialog/LoseDialog files deleted. Boot.unity scene will need SceneSetup regeneration in S06.
