# S02: Jigsaw Adapter

**Goal:** Create `JigsawLevelFactory` in `SimpleGame.Game` that converts a `SimpleJigsaw.PuzzleBoard` into an `IPuzzleLevel`. No `SimpleJigsaw.*` type is visible outside this factory.

**Demo:** `JigsawLevelFactory.Build(GridLayoutConfig, seed)` returns a valid `IPuzzleLevel` — verified by EditMode test using real `BoardFactory` output; no `SimpleJigsaw.*` type visible outside the factory.

## Must-Haves

- `SimpleGame.Game` asmdef gains a reference to `SimpleGame.Puzzle` (not `SimpleJigsaw` — it already has it transitively via the package)
- `JigsawLevelFactory` in `Assets/Scripts/Game/Puzzle/` folder, namespace `SimpleGame.Game.Puzzle`
- `JigsawLevelFactory.Build(GridLayoutConfig config, int seed, int[] seedPieceIds, int[][] deckOrders)` returns `IPuzzleLevel`
- Method also returns the raw `SimpleJigsaw.PuzzleBoard` for rendering (via out param or a result struct) — S04 needs it for `PieceObjectFactory.CreateAll`
- No `SimpleJigsaw.*` type in any `using` statement outside `JigsawLevelFactory.cs`
- EditMode test: build a 2×2 grid, confirm `IPuzzleLevel.Pieces.Count == 4`, confirm neighbor IDs match jigsaw topology, confirm deck order is preserved

## Tasks

- [ ] **T01: JigsawLevelFactory**
  Add `SimpleGame.Puzzle` reference to `SimpleGame.Game.asmdef`. Create `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` with `Build(...)` method that maps `PieceDescriptor` → `PuzzlePiece`, constructs `PuzzleLevel` with provided seeds and deck orders, and returns both the level and the raw `PuzzleBoard` for rendering.

- [ ] **T02: Adapter EditMode Test**
  Add `SimpleGame.Puzzle` to `SimpleGame.Tests.Game.asmdef`. Write `JigsawAdapterTests.cs` in `Assets/Tests/EditMode/Game/` that calls `JigsawLevelFactory.Build` with a real 2×2 `GridLayoutConfig`, confirms piece count, neighbor topology, seed placement, and deck order. Run and verify all tests pass.

## Files Likely Touched

- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — add `SimpleGame.Puzzle` reference
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` (new)
- `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` — add `SimpleGame.Puzzle` reference
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs` (new)
