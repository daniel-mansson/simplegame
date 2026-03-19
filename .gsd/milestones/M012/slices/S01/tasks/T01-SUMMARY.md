---
id: T01
parent: S01
milestone: M012
provides:
  - PuzzleModel class — board + deck + N slots, pure C#, no Unity refs
  - SlotTapResult enum — Placed, Rejected, Empty
  - OnSlotChanged, OnPiecePlaced, OnRejected, OnCompleted events
  - TryPlace(slotIndex) → SlotTapResult
  - PlacedCount, TotalNonSeedCount, IsComplete, GetSlot(index), SlotCount
requires: []
affects: [S02, S03, S04]
key_files:
  - Assets/Scripts/Puzzle/PuzzleModel.cs
  - Assets/Scripts/Puzzle/SlotTapResult.cs
key_decisions:
  - "Slots are int?[] array; null means empty (deck exhausted)"
  - "Seeds placed via PuzzleBoard.Place in constructor; bypass adjacency rule"
  - "PlacedCount counts only non-seed pieces; seeds are scaffolding not progress"
  - "OnCompleted guarded by _completed flag — fires exactly once"
  - "Deck.Peek() + Deck.Advance() used for slot fill; reuses existing Deck class"
patterns_established:
  - "PuzzleModel: typed events fired synchronously inside TryPlace"
duration: 15min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T01: PuzzleModel core — board, deck, slots, events

**PuzzleModel implemented: pure C# state machine with N independently-tracked slots, shared Deck, typed events, board adjacency rule via PuzzleBoard.**

## What Happened

Created `SlotTapResult` enum (Placed/Rejected/Empty) and `PuzzleModel` class. The model owns a `PuzzleBoard` for placement rule enforcement, a `Deck` for ordered piece draw, and an `int?[]` slots array. Constructor pre-places seeds unconditionally via `PuzzleBoard.Place`, then fills slots left-to-right from the deck front. `TryPlace(slotIndex)` handles the three cases: out-of-range/null → Empty, `CanPlace` false → fire `OnRejected` + Rejected, else place on board + refill slot from deck top + fire events. `OnCompleted` guarded by a `_completed` bool.

## Deviations

None — implemented exactly as planned.

## Files Created/Modified
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — new, 160 lines
- `Assets/Scripts/Puzzle/SlotTapResult.cs` — new, 25 lines
