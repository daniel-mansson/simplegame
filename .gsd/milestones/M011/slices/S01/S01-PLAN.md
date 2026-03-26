# S01: Pure Puzzle Domain Model

**Goal:** Create the `SimpleGame.Puzzle` assembly — a pure C# library with `noEngineReferences: true` that defines all puzzle domain types and rules.

**Demo:** EditMode tests prove seed placement, neighbor validation, deck draw, and win detection — all in plain C# with zero Unity references. `asmdef` compiles with `noEngineReferences: true`.

## Must-Haves

- `SimpleGame.Puzzle` asmdef exists with `noEngineReferences: true` and zero `UnityEngine`/`UnityEditor` imports
- `IPuzzlePiece` — `int Id`, `IReadOnlyList<int> NeighborIds`
- `IPuzzleBoard` — `CanPlace(pieceId)`, `Place(pieceId)`, `PlacedIds`
- `IDeck` — `Peek()`, `Advance()`, `IsEmpty`
- `IPuzzleLevel` — `Pieces`, `SeedIds`, `Decks`, `TotalPieceCount`
- `PlacementResult` enum — `Placed`, `Rejected`, `AlreadyPlaced`
- `PuzzleSession` — `TryPlace(pieceId)`, `IsComplete`, `CurrentDeckPiece(slotIndex)`, `event OnPlacementResolved`
- Placement rule: a non-seed piece is placeable iff at least one neighbor is already in `PlacedIds`
- Seeds are placed on `PuzzleSession` construction
- EditMode tests cover: seed pre-placement, correct neighbor accepted, wrong neighbor rejected, already-placed returns `AlreadyPlaced`, deck advances on correct placement, `IsComplete` when all non-seed pieces placed, `OnPlacementResolved` fires with correct result

## Tasks

- [x] **T01: Domain types and interfaces**
  Create `Assets/Scripts/Puzzle/` folder, `SimpleGame.Puzzle` asmdef with `noEngineReferences: true`, and all interfaces + enums: `IPuzzlePiece`, `IPuzzleBoard`, `IDeck`, `IPuzzleLevel`, `PlacementResult`.

- [x] **T02: Concrete implementations and PuzzleSession**
  Implement `PuzzlePiece`, `PuzzleBoard` (HashSet-backed), `Deck`, `PuzzleLevel`, and `PuzzleSession` with full placement logic, seed pre-placement, and `OnPlacementResolved` event.

- [x] **T03: EditMode tests**
  Create `Assets/Tests/EditMode/Puzzle/` with `SimpleGame.Tests.Puzzle` asmdef and comprehensive tests covering all placement rules, deck draw, win detection, and event firing.

## Files Likely Touched

- `Assets/Scripts/Puzzle/SimpleGame.Puzzle.asmdef` (new)
- `Assets/Scripts/Puzzle/IPuzzlePiece.cs` (new)
- `Assets/Scripts/Puzzle/IPuzzleBoard.cs` (new)
- `Assets/Scripts/Puzzle/IDeck.cs` (new)
- `Assets/Scripts/Puzzle/IPuzzleLevel.cs` (new)
- `Assets/Scripts/Puzzle/PlacementResult.cs` (new)
- `Assets/Scripts/Puzzle/PuzzlePiece.cs` (new)
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` (new)
- `Assets/Scripts/Puzzle/Deck.cs` (new)
- `Assets/Scripts/Puzzle/PuzzleLevel.cs` (new)
- `Assets/Scripts/Puzzle/PuzzleSession.cs` (new)
- `Assets/Tests/EditMode/Puzzle/SimpleGame.Tests.Puzzle.asmdef` (new)
- `Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs` (new)
