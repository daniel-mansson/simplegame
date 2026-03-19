# T01: PuzzleModel core — board, deck, slots, events

**Slice:** S01
**Milestone:** M012

## Goal

Implement the complete `PuzzleModel` class in `SimpleGame.Puzzle` with full slot tracking, shared deck, board adjacency enforcement, and typed events.

## Must-Haves

### Truths
- `PuzzleModel` compiles with no Unity references (SimpleGame.Puzzle has noEngineReferences:true)
- Seeds are on the board after construction — no TryPlace needed
- `TryPlace(0)` on a slot holding a placeable piece returns `SlotTapResult.Placed`
- `TryPlace(0)` on a slot holding an unplaceable piece returns `SlotTapResult.Rejected`
- `TryPlace(0)` on an empty slot returns `SlotTapResult.Empty`
- After `Placed`: the slot now holds the next deck piece (or null if deck exhausted)
- After `Rejected`: slot content is unchanged
- `OnSlotChanged(slotIndex, pieceId?)` fires after every Placed result (with new slot content)
- `OnPiecePlaced(pieceId)` fires after every Placed result
- `OnRejected(slotIndex, pieceId)` fires after every Rejected result
- `OnCompleted()` fires when all non-seed pieces are on the board
- `IsComplete` is true when all non-seed pieces are placed
- `PlacedCount` reflects only non-seed placed pieces (seeds don't count toward progress)
- `TotalNonSeedCount` is the total non-seed piece count

### Artifacts
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — full implementation, min 80 lines, no stubs
- `Assets/Scripts/Puzzle/SlotTapResult.cs` — enum: Placed, Rejected, Empty

### Key Links
- `PuzzleModel` uses `PuzzleBoard` internally for placement rule enforcement
- `PuzzleModel` uses `Deck` internally for deck management (or equivalent inline logic)

## Steps
1. Create `SlotTapResult.cs` enum in `SimpleGame.Puzzle` namespace
2. Create `PuzzleModel.cs` — fields: `_board`, `_deck`, `_slots` array, seed count
3. Constructor: validate args, build `PuzzleBoard` from piece list, build `Deck` from deckOrder, initialize slots array (fill first N from deck), pre-place seeds
4. Implement `TryPlace(int slotIndex)`:
   - Out of range → Empty
   - Slot null → Empty
   - `_board.CanPlace(pieceId)` false → fire OnRejected, return Rejected
   - Place on board, draw next from deck into slot, fire OnSlotChanged + OnPiecePlaced, check IsComplete → fire OnCompleted if so, return Placed
5. Implement public properties: `SlotCount`, `IsComplete`, `PlacedCount`, `TotalNonSeedCount`, `GetSlot(int index)`
6. Add XML doc comments on all public members

## Context
- `PuzzleBoard` is in `Assets/Scripts/Puzzle/PuzzleBoard.cs` — reuse it directly
- `Deck` is in `Assets/Scripts/Puzzle/Deck.cs` — reuse it directly
- `PuzzleBoard.CanPlace(pieceId)` checks neighbour presence; `PuzzleBoard.Place(pieceId)` adds to board
- Seeds bypass the neighbour rule — place them directly via `PuzzleBoard.Place` in the constructor
- `PlacedCount` should NOT count seeds (seeds are pre-placed scaffolding, not player progress)
- The `_slots` array is `int?[]` of length `slotCount` — null means empty
- On construction, fill slots left-to-right from deck front (deck.Advance() after each)
- `OnCompleted` fires exactly once — guard with `_completed` flag
