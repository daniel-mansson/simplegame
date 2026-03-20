---
id: S02
milestone: M013
provides:
  - JigsawLevelFactory.Build() uses SolvableShuffle (slotCount-aware, no Shuffle() fallback)
  - Build() signature: (config, slotCount, seed, seedPieceIds) — slotCount is new required param
  - BuildSolvable() retry cap changed from 100 → 10
  - InGameSceneController no longer overrides maxAttempts (relies on default 10)
  - JigsawAdapterTests updated for new Build() signature — all 261 tests pass
key_files:
  - Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/JigsawAdapterTests.cs
key_decisions:
  - "Build_PuzzleModelCanCompleteLevelFromDefaultDeck test simplified to greedy placement — SolvableShuffle deck order is non-deterministic"
drill_down_paths:
  - .gsd/milestones/M013/slices/S02/tasks/T01-PLAN.md
  - .gsd/milestones/M013/slices/S02/tasks/T02-PLAN.md
duration: 15min
verification_result: pass
completed_at: 2026-03-18T11:00:00Z
---

# S02: Wire into JigsawLevelFactory

**JigsawLevelFactory.Build() wired to SolvableShuffle; retry cap 10; all 261 tests pass**

## What Happened

Replaced the private `Shuffle<T>` + deck-building loop in `JigsawLevelFactory.Build()` with a direct call to `SolvableShuffle.Shuffle()`. Added `slotCount` as a required parameter to `Build()` (after `config`). Updated `BuildSolvable()` default from 100 → 10 retries. Updated `InGameSceneController` to drop explicit `maxAttempts: 1000` override.

Updated `JigsawAdapterTests` to pass `slotCount: 1` at each `Build()` call site. Simplified `Build_PuzzleModelCanCompleteLevelFromDefaultDeck` from step-by-step manual placement to greedy placement since deck order is now topology-driven and not predictably sorted.

## Deviations

None significant.

## Files Created/Modified

- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` — modified
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — minor update
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs` — updated call sites
