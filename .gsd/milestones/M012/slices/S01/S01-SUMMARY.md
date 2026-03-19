---
id: S01
milestone: M012
provides:
  - PuzzleModel — pure C# state machine, board + deck + N slots, typed events
  - SlotTapResult enum — Placed, Rejected, Empty
  - PuzzleModelConfig ScriptableObject (SimpleGame.Game) with SlotCount field
  - 22 PuzzleModel EditMode tests — full domain contract verified
requires: []
affects: [S02, S03, S04]
key_files:
  - Assets/Scripts/Puzzle/PuzzleModel.cs
  - Assets/Scripts/Puzzle/SlotTapResult.cs
  - Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs
  - Assets/Tests/EditMode/Puzzle/PuzzleModelTests.cs
key_decisions:
  - "PuzzleModel uses PuzzleBoard + Deck internally (reuses existing classes)"
  - "Slots are int?[] — null means empty; filled left-to-right from deck at construction"
  - "PlacedCount counts non-seed pieces only; seeds are pre-placed scaffolding"
  - "OnCompleted guarded by _completed flag — fires exactly once"
  - "PuzzleModelConfig in SimpleGame.Game (needs Unity); PuzzleModel in SimpleGame.Puzzle (pure C#)"
patterns_established:
  - "PuzzleModel: all events fired synchronously inside TryPlace"
  - "BuildDefault(slotCount) helper pattern for test fixtures"
drill_down_paths:
  - .gsd/milestones/M012/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M012/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M012/slices/S01/tasks/T03-SUMMARY.md
duration: 40min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S01: PuzzleModel — board, deck, N slots

**PuzzleModel implemented with full slot/deck/board mechanics and typed events; 241/241 EditMode tests passing.**

## What Was Built

Three artefacts:

1. **`PuzzleModel`** (`SimpleGame.Puzzle`, no engine refs) — takes a piece list, seed IDs, ordered deck, and slot count. Pre-places seeds on a `PuzzleBoard`. Fills slots left-to-right from the deck front. `TryPlace(slotIndex)` returns `SlotTapResult`. On Placed: piece goes to board, slot refills from deck top, events fire. On Rejected: slot unchanged, `OnRejected` fires. On Empty: no-op. `IsComplete` true when all non-seed pieces placed; `OnCompleted` fires once.

2. **`SlotTapResult`** — enum replacing the old `PlacementResult`-based presenter logic.

3. **`PuzzleModelConfig`** (`SimpleGame.Game`) — ScriptableObject with `SlotCount` (default 3, clamped to ≥ 1).

All 22 new model tests pass. Existing 219 tests unaffected.

## Deviations

None from plan.

## Files Created
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — 160 lines
- `Assets/Scripts/Puzzle/SlotTapResult.cs` — 25 lines
- `Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs` — 28 lines
- `Assets/Tests/EditMode/Puzzle/PuzzleModelTests.cs` — 240 lines, 22 tests
