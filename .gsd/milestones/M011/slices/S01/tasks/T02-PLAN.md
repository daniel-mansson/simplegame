# T02: Concrete Implementations and PuzzleSession

**Slice:** S01
**Milestone:** M011

## Goal

Implement all concrete domain types: `PuzzlePiece`, `PuzzleBoard` (HashSet-backed), `Deck`, `PuzzleLevel`, and `PuzzleSession` with full placement rule, seed pre-placement, and event.

## Must-Haves

### Truths
- `PuzzleBoard.CanPlace(id)` returns true iff: piece is a seed OR at least one of its `NeighborIds` is in `PlacedIds`
- `PuzzleBoard.Place(id)` adds piece to `PlacedIds` and returns true; returns false if already placed
- Seeds are added to `PlacedIds` during `PuzzleSession` construction (before first `TryPlace`)
- `PuzzleSession.TryPlace(id)` returns `PlacementResult.Placed` on success, `Rejected` on rule failure, `AlreadyPlaced` if already on board
- `PuzzleSession.TryPlace(id)` fires `OnPlacementResolved(id, result)` after every call
- `Deck.Advance()` moves to next piece; returns false when exhausted
- `PuzzleSession.IsComplete` is true when `PlacedIds.Count == level.TotalPieceCount`
- `PuzzleSession.CurrentDeckPiece(slotIndex)` returns the piece ID at the front of that slot's deck, or null if empty

### Artifacts
- `Assets/Scripts/Puzzle/PuzzlePiece.cs` — implements `IPuzzlePiece`, constructor takes `int id, IReadOnlyList<int> neighborIds`
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` — implements `IPuzzleBoard`, uses `HashSet<int>` for O(1) neighbor lookup
- `Assets/Scripts/Puzzle/Deck.cs` — implements `IDeck`, backed by `IReadOnlyList<int>` with index cursor
- `Assets/Scripts/Puzzle/PuzzleLevel.cs` — implements `IPuzzleLevel`, plain data holder
- `Assets/Scripts/Puzzle/PuzzleSession.cs` — orchestrates board + decks, pre-places seeds on construction

### Key Links
- `PuzzleSession` → `IPuzzleBoard` via `PuzzleBoard` constructor injection
- `PuzzleSession` → `IPuzzleLevel` via constructor parameter
- `PuzzleBoard.CanPlace` checks `_placedIds.Contains(neighborId)` for each neighbor — O(neighbors) per check, O(1) per neighbor lookup

## Steps
1. Write `PuzzlePiece.cs`
2. Write `PuzzleBoard.cs` — `HashSet<int> _placedIds`; `CanPlace` checks seeds OR neighbor presence; `Place` returns false if already present
3. Write `Deck.cs` — `IReadOnlyList<int> _pieceIds`, `int _index`; `Peek()` returns `_pieceIds[_index]` or null; `Advance()` increments and returns whether more remain
4. Write `PuzzleLevel.cs` — immutable data holder
5. Write `PuzzleSession.cs` — constructor pre-places seeds; `TryPlace` delegates to board + fires event; `CurrentDeckPiece` delegates to deck
6. Verify `rg "using Unity" Assets/Scripts/Puzzle/` returns nothing

## Context
- `PuzzleBoard` is the hot path — `CanPlace` is called on every tap. HashSet gives O(1) lookup. Piece neighbor counts are small (2–6) so the outer loop is bounded.
- `PuzzleSession` owns the board instance; the board is not exposed publicly (game code calls session methods only)
- The `OnPlacementResolved` event fires even for `Rejected` and `AlreadyPlaced` — the presenter needs to react to all outcomes
- `PuzzleLevel` stores seeds as a list; `PuzzleSession` iterates over them at construction time to pre-place them via `PuzzleBoard.Place`
- Seeds bypass `CanPlace` — they are placed unconditionally at session start
