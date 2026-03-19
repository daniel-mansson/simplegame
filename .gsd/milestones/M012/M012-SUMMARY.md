---
id: M012
provides:
  - PuzzleModel — pure C# state machine, board + shared deck + N independently-tracked slots
  - SlotTapResult enum (Placed/Rejected/Empty)
  - PuzzleModelConfig ScriptableObject with configurable SlotCount
  - InGamePresenter rewritten to subscribe to PuzzleModel events reactively
  - IInGameView slot-indexed API (RefreshSlot replaces RefreshTray)
  - InGameView, InGameSceneController, UIFactory all updated
  - JigsawLevelFactory updated: flat PieceList/SeedIds/DeckOrder fields
  - PuzzleSession, IPuzzleLevel, PuzzleLevel, PlacementResult deleted
  - 232 EditMode tests — all passing
requires: []
key_files:
  - Assets/Scripts/Puzzle/PuzzleModel.cs
  - Assets/Scripts/Puzzle/SlotTapResult.cs
  - Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/InGame/IInGameView.cs
  - Assets/Scripts/Game/InGame/InGameView.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs
  - Assets/Tests/EditMode/Puzzle/PuzzleModelTests.cs
  - Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs
key_decisions:
  - "PuzzleModel uses PuzzleBoard + Deck internally — reuses existing correct classes"
  - "Slots are first-class int?[] state — no more view sync bugs from inferred window"
  - "Model fires typed events synchronously in TryPlace — presenter subscribes reactively"
  - "Wrong tap: slot piece unchanged, heart deducted — correct mechanic"
  - "SlotCount from PuzzleModelConfig SO — configurable per scene"
  - "JigsawLevelFactory.Build drops deckOrders param; exposes flat fields for PuzzleModel"
  - "SetModelFactory test seam replaces SetLevelFactory — no IPuzzleLevel in test paths"
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# M012: Stable Core Game — PuzzleModel Refactor

**PuzzleSession replaced by event-driven PuzzleModel with N explicitly-tracked slots; 232/232 tests passing; view sync bugs eliminated by design.**

## What Was Built

Four slices:

**S01 — PuzzleModel:** New pure C# `PuzzleModel` class with board (via `PuzzleBoard`), single shared `Deck`, and N independent `int?[]` slots. `TryPlace(slotIndex)` returns `SlotTapResult`. On correct placement: piece goes to board, slot refills from deck top, events fire. On rejection: slot unchanged, `OnRejected` fires. `PuzzleModelConfig` ScriptableObject with default `SlotCount = 3`. 22 domain tests.

**S02 — Presenter rewrite:** `InGamePresenter` rewritten to accept `PuzzleModel`, subscribe to all four events in `Initialize`, unsubscribe in `Dispose`. `PushTrayWindow()` and all deck-cursor walking eliminated. `UIFactory.CreateInGamePresenter` updated. Presenter and scene controller tests updated.

**S03 — View wiring:** `IInGameView.RefreshTray(int?[])` replaced with `RefreshSlot(int, int?)`. `InGameView` reimplemented with per-slot tracking. `InGameSceneController` reads `PuzzleModelConfig` for slot count. `JigsawLevelFactory.Build` updated: `JigsawBuildResult` exposes `PieceList`, `SeedIds`, `DeckOrder` — scene controller constructs `PuzzleModel` directly from these. `JigsawAdapterTests` updated.

**S04 — Cleanup:** `PuzzleSession`, `IPuzzleLevel`, `PuzzleLevel`, `PlacementResult` deleted. `JigsawBuildResult.Level` legacy accessor removed. `PuzzleDomainTests` rewritten as `PuzzleBoardTests`. `SetLevelFactory` → `SetModelFactory`. `TestLevelBuilder` removed. Final: 232/232.

## Requirements Satisfied
- R101 PuzzleModel as ID-only state machine ✓
- R102 Configurable slot count via ScriptableObject ✓
- R103 Slots refill independently from shared deck top ✓
- R104 Wrong tap costs heart, slot unchanged ✓
- R105 Model fires typed events, presenter reacts ✓
- R106 PuzzleSession deleted, replaced by PuzzleModel ✓
- R107 View receives slot-indexed updates ✓
- R108 Domain tests for PuzzleModel contract ✓
- R109 JigsawLevelFactory feeds PuzzleModel cleanly ✓
