---
id: T02
parent: S01
milestone: M011
provides:
  - PuzzlePiece concrete implementation
  - PuzzleBoard with HashSet-backed O(1) neighbor lookup
  - Deck with index-cursor ordered sequence
  - PuzzleLevel immutable data holder
  - PuzzleSession: seed pre-placement, TryPlace, OnPlacementResolved, IsComplete, CurrentDeckPiece
requires: []
affects: [T03, S02, S03]
key_files:
  - Assets/Scripts/Puzzle/PuzzlePiece.cs
  - Assets/Scripts/Puzzle/PuzzleBoard.cs
  - Assets/Scripts/Puzzle/Deck.cs
  - Assets/Scripts/Puzzle/PuzzleLevel.cs
  - Assets/Scripts/Puzzle/PuzzleSession.cs
key_decisions:
  - "PuzzleBoard uses HashSet<int> for O(1) neighbor lookup — CanPlace is the hot path"
  - "Seeds bypass CanPlace — placed unconditionally in PuzzleSession constructor"
  - "TryPlace advances any deck whose front matches the placed piece ID (supports multi-deck)"
  - "OnPlacementResolved fires after every TryPlace including Rejected and AlreadyPlaced"
  - "PuzzleSession owns the PuzzleBoard instance — board not exposed publicly"
patterns_established:
  - "PuzzleSession as coordinator: owns board, delegates to board/decks, fires event"
drill_down_paths:
  - .gsd/milestones/M011/slices/S01/tasks/T02-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-18T22:05:00Z
---

# T02: Concrete Implementations and PuzzleSession

**HashSet-backed PuzzleBoard, index-cursor Deck, PuzzleSession with seed pre-placement and placement event**

## What Happened

Implemented all concrete types. `PuzzleBoard` builds an internal `Dictionary<int, IReadOnlyList<int>>` neighbor map from the piece list at construction, then uses a `HashSet<int>` for placed IDs — `CanPlace` is O(neighbors) with O(1) per lookup. `Deck` uses an int index cursor over a readonly list. `PuzzleSession` pre-places seeds via unconditional `Place()` calls, then `TryPlace` checks `PlacedIds.Contains` first (AlreadyPlaced), then `CanPlace` (Rejected), then places and advances matching deck fronts.

## Deviations
None — implemented exactly as planned.

## Files Created/Modified
- `Assets/Scripts/Puzzle/PuzzlePiece.cs` — sealed, immutable
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` — HashSet + neighbor Dictionary
- `Assets/Scripts/Puzzle/Deck.cs` — index cursor over IReadOnlyList<int>
- `Assets/Scripts/Puzzle/PuzzleLevel.cs` — plain data holder
- `Assets/Scripts/Puzzle/PuzzleSession.cs` — orchestrator with event
