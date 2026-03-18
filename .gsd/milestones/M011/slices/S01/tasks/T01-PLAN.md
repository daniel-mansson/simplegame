# T01: Domain Types and Interfaces

**Slice:** S01
**Milestone:** M011

## Goal

Create the `SimpleGame.Puzzle` assembly with `noEngineReferences: true` and all puzzle domain interfaces and enums.

## Must-Haves

### Truths
- `Assets/Scripts/Puzzle/SimpleGame.Puzzle.asmdef` exists with `"noEngineReferences": true` and no references to `SimpleJigsaw` or `SimpleGame.Game`
- `IPuzzlePiece`, `IPuzzleBoard`, `IDeck`, `IPuzzleLevel`, `PlacementResult` all exist in namespace `SimpleGame.Puzzle`
- No `using UnityEngine` or `using UnityEditor` in any file

### Artifacts
- `Assets/Scripts/Puzzle/SimpleGame.Puzzle.asmdef` — `noEngineReferences: true`, no Unity refs
- `Assets/Scripts/Puzzle/IPuzzlePiece.cs` — `int Id`, `IReadOnlyList<int> NeighborIds`
- `Assets/Scripts/Puzzle/IPuzzleBoard.cs` — `bool CanPlace(int pieceId)`, `bool Place(int pieceId)`, `IReadOnlyCollection<int> PlacedIds`
- `Assets/Scripts/Puzzle/IDeck.cs` — `int? Peek()`, `bool Advance()`, `bool IsEmpty`, `int Count`, `int RemainingCount`
- `Assets/Scripts/Puzzle/IPuzzleLevel.cs` — `IReadOnlyList<IPuzzlePiece> Pieces`, `IReadOnlyList<int> SeedIds`, `IReadOnlyList<IDeck> Decks`, `int TotalPieceCount`
- `Assets/Scripts/Puzzle/PlacementResult.cs` — enum `Placed`, `Rejected`, `AlreadyPlaced`

### Key Links
- All types in `SimpleGame.Puzzle` namespace
- `IPuzzleLevel` references `IPuzzlePiece` and `IDeck`
- `IPuzzleBoard` is self-contained

## Steps
1. Create `Assets/Scripts/Puzzle/` directory
2. Write `SimpleGame.Puzzle.asmdef` with `noEngineReferences: true`, empty references array, `autoReferenced: false`
3. Write `PlacementResult.cs` enum
4. Write `IPuzzlePiece.cs`
5. Write `IPuzzleBoard.cs`
6. Write `IDeck.cs`
7. Write `IPuzzleLevel.cs`
8. Verify no Unity imports with `rg "using Unity" Assets/Scripts/Puzzle/`

## Context
- This is the contract layer — implementations come in T02
- `IDeck` needs `Count` and `RemainingCount` for the presenter to display progress (e.g. "3 pieces left")
- `noEngineReferences: true` in asmdef means Unity will refuse to compile the assembly if any `UnityEngine` or `UnityEditor` type is referenced — it is a hard enforced constraint, not just a convention
- Follow project asmdef pattern: `autoReferenced: false`, string name references
