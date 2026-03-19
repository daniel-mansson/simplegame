---
id: T02
parent: S02
milestone: M012
provides:
  - InGamePresenterTests rewritten to use PuzzleModel (slotCount=1 for determinism)
  - InGameSceneControllerTests updated: TapWrong uses piece 2 (slot 1, unplaceable until piece 1 placed)
  - Tests for WatchAd and ResetsProgress use explicit piece IDs for wrong taps
  - All 241 EditMode tests passing
requires:
  - task: T01
    provides: InGamePresenter with PuzzleModel
affects: [S04]
key_files:
  - Assets/Tests/EditMode/Game/InGameTests.cs
key_decisions:
  - "InGamePresenterTests use slotCount=1 PuzzleModel so tap always targets slot 0"
  - "InGameSceneControllerTests TapWrong uses wrongPieceId=2 (in slot 1, unplaceable until piece 1 placed)"
  - "WatchAd and ResetsProgress tests use 5- and 8-piece levels with explicit wrong-piece IDs"
  - "Tapping a piece not in any slot is ignored (no heart cost) — tests updated to reflect this"
patterns_established:
  - "LinearChainModel(totalPieces, slotCount) helper for presenter tests"
duration: 25min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T02: Update InGamePresenter tests

**InGamePresenterTests rewritten for PuzzleModel; InGameSceneControllerTests updated with correct wrong-tap piece IDs. 241/241 passing.**

## What Happened

`InGamePresenterTests` completely rewritten: replaced `IPuzzleLevel` + `PuzzleSession` with `PuzzleModel`. Added `LinearChainModel(totalPieces, slotCount)` helper. Used `slotCount=1` for most tests to make tap-to-slot mapping deterministic (tap piece N → always slot 0 since it refills sequentially). For rejection tests used `slotCount=2` so slot 1 holds piece 2 (unplaceable until piece 1 placed).

`InGameSceneControllerTests` updated: `TapWrong` changed to tap `wrongPieceId=2` (in slot 1 on a 3-slot level, requires piece 1 placed first). Tests that tap wrong pieces after partial progress updated to use explicit piece IDs that are guaranteed unplaceable at that moment. `LoseWatchAdThenContinue` bumped to 8-piece level to ensure wrong pieces are available after placing 3 correct ones.

## Deviations

The `MixedActions_WinBeforeDeath` test changed: the old test tapped piece 4 (non-existent in a 3-piece level) as wrong; new test taps piece 99 (not in any slot → ignored, no heart cost). The test still verifies win with hearts intact — the assertion changed to expect 3 remaining hearts (old: 2), reflecting that an ignored tap doesn't cost a heart.

## Files Modified
- `Assets/Tests/EditMode/Game/InGameTests.cs` — presenter tests rewritten, controller tests updated
