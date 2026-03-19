---
id: S04
milestone: M012
provides:
  - PuzzleSession, IPuzzleLevel, PuzzleLevel, PlacementResult deleted
  - JigsawBuildResult.Level legacy accessor removed
  - PuzzleDomainTests rewritten as PuzzleBoardTests (8 tests for PuzzleBoard internals)
  - InGameSceneController.SetModelFactory replaces SetLevelFactory
  - BuildStubModel replaces BuildStubLevel (returns PuzzleModel directly)
  - TestLevelBuilder.LinearChain removed from tests
  - JigsawAdapterTests updated (deckOrders test replaced with DeckOrder ordering test)
  - 232/232 EditMode tests passing, zero regressions
requires:
  - slice: S03
    provides: Full wired stack
affects: []
key_files:
  - Assets/Scripts/Puzzle/PuzzleSession.cs (deleted)
  - Assets/Scripts/Puzzle/IPuzzleLevel.cs (deleted)
  - Assets/Scripts/Puzzle/PuzzleLevel.cs (deleted)
  - Assets/Scripts/Puzzle/PlacementResult.cs (deleted)
  - Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs (rewritten as PuzzleBoardTests)
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
  - Assets/Tests/EditMode/Game/JigsawAdapterTests.cs
key_decisions:
  - "SetModelFactory(Func<PuzzleModel>) replaces SetLevelFactory(Func<IPuzzleLevel>) — cleaner test seam"
  - "PuzzleDomainTests repurposed as PuzzleBoardTests — tests PuzzleBoard directly (still the core engine)"
  - "IDeck and Deck kept — still used internally by PuzzleModel"
patterns_established: []
drill_down_paths: []
duration: 30min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S04: Tests & cleanup

**PuzzleSession and related types deleted; all domain tests rewritten; 232/232 EditMode tests passing.**

## What Was Built

Deleted four files from `SimpleGame.Puzzle`: `PuzzleSession.cs`, `IPuzzleLevel.cs`, `PuzzleLevel.cs`, `PlacementResult.cs`. Removed `JigsawBuildResult.Level` legacy accessor. Rewrote `PuzzleDomainTests.cs` as `PuzzleBoardTests` testing the `PuzzleBoard` class directly (still the internal placement engine in `PuzzleModel`). Replaced `SetLevelFactory`/`BuildStubLevel` in `InGameSceneController` with `SetModelFactory`/`BuildStubModel`. Removed `TestLevelBuilder.LinearChain` from tests. Updated `JigsawAdapterTests` to drop `deckOrders` test. Final test count: 232/232 green.

## Deviations

`PuzzleDomainTests` kept as filename but content repurposed — renamed class to `PuzzleBoardTests` inside the file for clarity.

## Files Deleted
- `Assets/Scripts/Puzzle/PuzzleSession.cs`
- `Assets/Scripts/Puzzle/IPuzzleLevel.cs`
- `Assets/Scripts/Puzzle/PuzzleLevel.cs`
- `Assets/Scripts/Puzzle/PlacementResult.cs`

## Files Modified
- `Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs` — rewritten as PuzzleBoardTests
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — SetModelFactory, BuildStubModel
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` — Level accessor removed
- `Assets/Tests/EditMode/Game/InGameTests.cs` — TestLevelBuilder removed, SetStubLevel updated
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs` — updated tests
