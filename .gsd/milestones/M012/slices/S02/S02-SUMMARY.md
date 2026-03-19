---
id: S02
milestone: M012
provides:
  - InGamePresenter rewritten — accepts PuzzleModel, subscribes to events, no tray-window logic
  - UIFactory.CreateInGamePresenter updated to accept PuzzleModel
  - InGameSceneController.BuildPuzzleModel bridge (IPuzzleLevel→PuzzleModel, slotCount:3)
  - InGamePresenterTests rewritten for PuzzleModel (slotCount=1)
  - InGameSceneControllerTests updated with valid wrong-tap piece IDs
  - All 241 EditMode tests passing
requires:
  - slice: S01
    provides: PuzzleModel class and events
affects: [S03, S04]
key_files:
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
key_decisions:
  - "Presenter bridges old OnTapPiece(pieceId) API by scanning slots for the piece ID"
  - "HandleSlotChanged builds full slot window and calls RefreshTray — bridge removed in S03"
  - "BuildPuzzleModel in scene controller: temp bridge, removed in S03"
  - "Ignored taps (piece not in any slot) cost no heart — tests updated to reflect this"
patterns_established:
  - "PuzzleModel event subscription/unsubscription pattern in presenter Initialize/Dispose"
drill_down_paths:
  - .gsd/milestones/M012/slices/S02/tasks/T01-SUMMARY.md
  - .gsd/milestones/M012/slices/S02/tasks/T02-SUMMARY.md
duration: 55min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S02: InGamePresenter rewrite

**InGamePresenter fully event-driven via PuzzleModel; all 241 EditMode tests passing.**

## What Was Built

Rewrote `InGamePresenter` to accept `PuzzleModel` in the constructor and subscribe to all four model events. The old `PuzzleSession` + `PushTrayWindow()` tray-window lookahead is gone. Temporary bridges keep the existing view interface working: `HandleTapPiece` scans slots for the tapped piece ID; `HandleSlotChanged` rebuilds the full slot array for `RefreshTray`. Both bridges are removed in S03.

Updated `UIFactory.CreateInGamePresenter` signature. Added `BuildPuzzleModel` helper to `InGameSceneController` to convert `IPuzzleLevel → PuzzleModel` (temp, removed in S03).

Rewrote `InGamePresenterTests` to use `PuzzleModel` directly. Updated `InGameSceneControllerTests` with valid wrong-tap scenarios (piece 2 in slot 1 is unplaceable until piece 1 placed).

## Files Modified
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — rewritten
- `Assets/Scripts/Game/Boot/UIFactory.cs` — CreateInGamePresenter signature
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — BuildPuzzleModel bridge added
- `Assets/Tests/EditMode/Game/InGameTests.cs` — presenter and controller tests updated
