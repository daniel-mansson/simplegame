# S01: PuzzleModel — board, deck, N slots

**Goal:** Implement `PuzzleModel` — a pure C# state machine with no Unity dependencies that owns board state, a shared ordered deck, and N independently-tracked slots. Also create `PuzzleModelConfig` ScriptableObject. Proves contract via EditMode tests.

**Demo:** EditMode tests pass: slot refills on correct placement, heart event on wrong tap, win when all placed, deck exhaustion handled correctly.

## Must-Haves

- `PuzzleModel` lives in `SimpleGame.Puzzle` (pure C#, no engine refs)
- Constructor accepts: piece list, seed IDs, ordered deck (IReadOnlyList<int>), slot count
- Seeds are pre-placed on the board at construction
- `TryPlace(int slotIndex)` returns `SlotTapResult.Placed`, `Rejected`, or `Empty`
- On `Placed`: piece moves to board, slot draws from deck top, `OnSlotChanged` + `OnPiecePlaced` fire
- On `Rejected`: no state change, `OnRejected(slotIndex, pieceId)` fires
- On `Empty` (slot has no piece): no state change, no event
- When all non-seed pieces are placed: `OnCompleted` fires
- `PuzzleModelConfig` ScriptableObject with `SlotCount` (default 3) in `SimpleGame.Game`
- `PuzzleDomainTests` (new, minimal) green for the model contract

## Tasks

- [x] **T01: PuzzleModel core — board, deck, slots, events**
  Implement the full PuzzleModel class with all events, TryPlace logic, slot tracking, deck drain. Also SlotTapResult enum.

- [x] **T02: PuzzleModelConfig ScriptableObject**
  Create the ScriptableObject config class in SimpleGame.Game assembly. Separate task because it requires Unity types and a different assembly.

- [x] **T03: PuzzleDomainTests for PuzzleModel**
  Write EditMode tests covering: seed pre-placement, correct tap places + refills, wrong tap fires Rejected, deck exhaustion, win condition, multi-slot independence, events fire correctly.

## Files Likely Touched

- `Assets/Scripts/Puzzle/PuzzleModel.cs` — new
- `Assets/Scripts/Puzzle/SlotTapResult.cs` — new enum
- `Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs` — new ScriptableObject
- `Assets/Tests/EditMode/Puzzle/PuzzleModelTests.cs` — new (alongside existing PuzzleDomainTests.cs for now)
