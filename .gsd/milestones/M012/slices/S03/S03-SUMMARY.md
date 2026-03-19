---
id: S03
milestone: M012
provides:
  - IInGameView: RefreshTray removed, RefreshSlot(int slotIndex, int? pieceId) added
  - InGameView: slot-indexed callbacks, per-slot tracking via int?[] array
  - InGamePresenter: HandleSlotChanged calls RefreshSlot; PushAllSlots calls RefreshSlot per slot
  - InGameSceneController: reads PuzzleModelConfig for slot count; constructs PuzzleModel directly
  - InGameSceneController: LevelToPuzzleModel helper (replaces BuildPuzzleModel temp bridge)
  - JigsawLevelFactory: flat PieceList/SeedIds/DeckOrder exposed in JigsawBuildResult
  - JigsawAdapterTests: updated to use PieceList/DeckOrder fields and PuzzleModel
  - MockInGameView: implements RefreshSlot
  - All 241 EditMode tests passing
requires:
  - slice: S02
    provides: InGamePresenter with PuzzleModel event subscriptions
affects: [S04]
key_files:
  - Assets/Scripts/Game/InGame/IInGameView.cs
  - Assets/Scripts/Game/InGame/InGameView.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
  - Assets/Tests/EditMode/Game/JigsawAdapterTests.cs
key_decisions:
  - "IInGameView.RefreshTray replaced with RefreshSlot — breaking change, view is now slot-indexed"
  - "JigsawBuildResult keeps Level as legacy accessor for backward compat; callers prefer PieceList/SeedIds/DeckOrder"
  - "JigsawLevelFactory.Build drops deckOrders parameter — single shared deck only"
  - "InGameSceneController.LevelToPuzzleModel kept for legacy test seam (SetLevelFactory); not a public API"
  - "PuzzleModelConfig slot count wired: defaults to 3 if no asset assigned"
patterns_established:
  - "JigsawBuildResult exposes flat fields (PieceList, SeedIds, DeckOrder) for direct PuzzleModel construction"
drill_down_paths:
  - .gsd/milestones/M012/slices/S03/tasks/T01-SUMMARY.md
  - .gsd/milestones/M012/slices/S03/tasks/T02-SUMMARY.md
duration: 45min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S03: View + SceneController wiring

**Slot-indexed view API wired end-to-end; JigsawLevelFactory updated; PuzzleModelConfig connected. 241/241 tests passing.**

## What Was Built

**T01:** `IInGameView` updated — `RefreshTray(int?[])` replaced with `RefreshSlot(int slotIndex, int? pieceId)`. `InGameView` reimplemented with per-slot tracking dictionary. `InGamePresenter.HandleSlotChanged` now calls `RefreshSlot` directly (one slot, correct index). `PushAllSlots` iterates slots. `HandleCompleted` clears all slots individually.

**T02:** `InGameSceneController` gets `[SerializeField] PuzzleModelConfig _puzzleModelConfig`. `RunAsync` reads `slotCount` from config (default 3). Jigsaw path now captures flat fields from `JigsawBuildResult` and constructs `PuzzleModel` directly in lambda — no `IPuzzleLevel` involved. `LevelToPuzzleModel` (renamed from temp `BuildPuzzleModel`) kept for the `SetLevelFactory` test seam. `JigsawLevelFactory.Build` updated: `deckOrders` param removed; `JigsawBuildResult` exposes `PieceList`, `SeedIds`, `DeckOrder` directly. Legacy `Level` accessor kept. `JigsawAdapterTests` updated to test new fields and use `PuzzleModel` instead of `PuzzleSession`.

## Files Modified
- `Assets/Scripts/Game/InGame/IInGameView.cs`
- `Assets/Scripts/Game/InGame/InGameView.cs`
- `Assets/Scripts/Game/InGame/InGamePresenter.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs` (MockInGameView)
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs`
