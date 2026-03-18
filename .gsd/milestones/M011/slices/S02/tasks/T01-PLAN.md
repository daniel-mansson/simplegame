# T01: JigsawLevelFactory

**Slice:** S02
**Milestone:** M011

## Goal

Create `JigsawLevelFactory` — the sole bridge between `SimpleJigsaw` and the puzzle domain model. Returns `IPuzzleLevel` + raw `PuzzleBoard` for rendering.

## Must-Haves

### Truths
- `SimpleGame.Game.asmdef` references `SimpleGame.Puzzle`
- `JigsawLevelFactory.Build(...)` returns an `IPuzzleLevel` with correct piece count, neighbor IDs, seeds, and deck order
- The raw `SimpleJigsaw.PuzzleBoard` is also returned (for `PieceObjectFactory.CreateAll` in S04)
- No file outside `JigsawLevelFactory.cs` imports any `SimpleJigsaw.*` type

### Artifacts
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` — static class, `Build` method, namespace `SimpleGame.Game.Puzzle`
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — updated with `SimpleGame.Puzzle` in references

### Key Links
- `JigsawLevelFactory` → `SimpleJigsaw.BoardFactory.Generate` to get `PuzzleBoard`
- `JigsawLevelFactory` → `SimpleGame.Puzzle.PuzzlePiece` to construct domain pieces
- `JigsawLevelFactory` → `SimpleGame.Puzzle.PuzzleLevel` to construct level
- `JigsawLevelFactory` → `SimpleGame.Puzzle.Deck` to construct decks from `deckOrders`

## Steps
1. Add `"SimpleGame.Puzzle"` to `SimpleGame.Game.asmdef` references array
2. Create `Assets/Scripts/Game/Puzzle/` directory
3. Write `JigsawLevelFactory.cs`:
   - Call `BoardFactory.Generate(config, seed)` to get `SimpleJigsaw.PuzzleBoard`
   - Map each `PieceDescriptor` → `PuzzlePiece(id, neighborIds)` extracting neighbor IDs from `piece.Neighbors`
   - Construct `PuzzleLevel(pieces, seedPieceIds, decks)` where decks come from `deckOrders`
   - Return a `JigsawBuildResult` struct with both `IPuzzleLevel` and `SimpleJigsaw.PuzzleBoard`
4. Verify: `rg "using SimpleJigsaw" Assets/Scripts/Game/ --glob "!*JigsawLevelFactory*"` returns nothing

## Context
- `PieceDescriptor.Neighbors` is `List<(int NeighborId, int SharedEdgeIndex)>` — extract `.NeighborId` only
- `deckOrders` is `int[][]` — one array per slot; if null or empty, default to a single deck with all non-seed pieces in ID order
- `JigsawBuildResult` is a plain struct (no Unity deps) — holds `IPuzzleLevel Level` and `SimpleJigsaw.PuzzleBoard RawBoard`
- `JigsawBuildResult` struct lives in `JigsawLevelFactory.cs` — it references `SimpleJigsaw.PuzzleBoard` so it must stay in the same file/namespace (not in `SimpleGame.Puzzle`)
