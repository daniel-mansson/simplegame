---
id: T01
parent: S02
milestone: M012
provides:
  - InGamePresenter rewritten to accept PuzzleModel (not IPuzzleLevel)
  - Subscribes to OnSlotChanged/OnPiecePlaced/OnRejected/OnCompleted in Initialize
  - Unsubscribes in Dispose — no memory leaks
  - HandleTapPiece bridges OnTapPiece(pieceId) to TryPlace(slotIndex) via slot scan
  - HandleSlotChanged pushes full slot window as RefreshTray (temp bridge until S03)
  - UIFactory.CreateInGamePresenter updated to accept PuzzleModel
  - InGameSceneController.BuildPuzzleModel helper bridges IPuzzleLevel → PuzzleModel
requires:
  - task: S01/T01
    provides: PuzzleModel API
affects: [S03, S04]
key_files:
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
key_decisions:
  - "HandleTapPiece scans _model.GetSlot(i) to find which slot holds the tapped piece; taps on non-slot pieces are ignored"
  - "HandleSlotChanged rebuilds full slot array and calls RefreshTray — temp bridge until S03 slot-indexed API"
  - "BuildPuzzleModel in scene controller bridges IPuzzleLevel→PuzzleModel with slotCount:3 until S03 wires PuzzleModelConfig"
  - "PushTrayWindow() completely removed — no deck-cursor walking"
patterns_established:
  - "Presenter subscribes to PuzzleModel events in Initialize, unsubscribes in Dispose"
duration: 30min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T01: Rewrite InGamePresenter

**InGamePresenter rewritten to subscribe to PuzzleModel events; tray-window lookahead logic eliminated.**

## What Happened

Rewrote `InGamePresenter` to accept `PuzzleModel` instead of `IPuzzleLevel`. All four model events are subscribed in `Initialize()` and unsubscribed in `Dispose()`. The old `PushTrayWindow()` / deck-cursor-walking logic is gone. `HandleTapPiece` bridges the old view API: scans slots for the tapped piece ID and calls `model.TryPlace(slotIndex)` — taps on pieces not in any slot are silently ignored. `HandleSlotChanged` rebuilds the full slot window and calls `RefreshTray` as a temporary bridge until S03 updates the view interface.

Also added `BuildPuzzleModel()` static helper to `InGameSceneController` — extracts the first deck from `IPuzzleLevel` and constructs a `PuzzleModel` with slotCount:3. Temporary bridge removed in S03.

## Deviations

`HandleSlotChanged` builds a full slot window rather than calling `RefreshTray` with just the one changed slot — this matches the existing `RefreshTray(int?[])` API and avoids touching the view interface in this slice.

## Files Modified
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — rewritten, ~140 lines
- `Assets/Scripts/Game/Boot/UIFactory.cs` — CreateInGamePresenter signature updated
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — BuildPuzzleModel helper added, RunAsync updated
