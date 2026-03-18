# T02: Adapter EditMode Test

**Slice:** S02
**Milestone:** M011

## Goal

Prove `JigsawLevelFactory.Build` produces a correct `IPuzzleLevel` from a real `BoardFactory` output.

## Must-Haves

### Truths
- All existing EditMode tests still pass (193+)
- New test: 2×2 grid produces `IPuzzleLevel` with 4 pieces
- New test: neighbor topology matches jigsaw adjacency (corner piece has 2 neighbors, edge piece has 3)
- New test: seed IDs appear in `level.SeedIds`
- New test: deck order is preserved from input `deckOrders`
- New test: `PuzzleSession` constructed from the level completes successfully when all pieces placed in valid order

### Artifacts
- `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` — add `SimpleGame.Puzzle` to references
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs` — 5+ test cases

### Key Links
- Tests reference `JigsawLevelFactory` (in `SimpleGame.Game.Puzzle`) and `PuzzleSession` (in `SimpleGame.Puzzle`)
- Tests use real `GridLayoutConfig` created via `ScriptableObject.CreateInstance<GridLayoutConfig>()`

## Steps
1. Add `"SimpleGame.Puzzle"` to `SimpleGame.Tests.Game.asmdef` references
2. Write `JigsawAdapterTests.cs` with a helper that creates a 2×2 `GridLayoutConfig` ScriptableObject and calls `JigsawLevelFactory.Build`
3. Write all test cases
4. Run tests via stdin pipe: `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin`
5. Fix failures until all pass

## Context
- A 2×2 grid has IDs 0,1,2,3 — piece 0 (top-left) neighbors 1 and 2; piece 3 (bottom-right) neighbors 1 and 2
- For the default deck test: pass `deckOrders: null` → factory should auto-generate a deck of all non-seed pieces
- `ScriptableObject.CreateInstance<GridLayoutConfig>()` creates an in-memory SO without needing an asset file — valid in EditMode tests
- `BoardFactory.Generate` needs no `BoardShapeConfig` for a simple rect grid — pass `null` or use the two-param overload
