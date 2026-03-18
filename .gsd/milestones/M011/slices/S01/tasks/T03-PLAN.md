# T03: EditMode Tests

**Slice:** S01
**Milestone:** M011

## Goal

Create `SimpleGame.Tests.Puzzle` asmdef and comprehensive EditMode tests proving all placement rules, deck draw, win detection, and event firing.

## Must-Haves

### Truths
- All tests pass when run via `mcporter call unityMCP.run_tests` (stdin pipe mode per K006)
- Test count: minimum 12 distinct test cases
- Zero Unity API calls in test code (no `GameObject`, `MonoBehaviour`, etc.)

### Artifacts
- `Assets/Tests/EditMode/Puzzle/SimpleGame.Tests.Puzzle.asmdef` — references `SimpleGame.Puzzle`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`; `noEngineReferences: false` (needs test runner)
- `Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs` — all test cases in `SimpleGame.Tests.Puzzle` namespace

### Test cases required
- `SeedPiecesArePlacedOnConstruction` — seeds in PlacedIds after session created
- `NonSeedWithNoNeighborOnBoardIsRejected` — piece with no placed neighbors returns Rejected
- `NonSeedWithPlacedNeighborIsAccepted` — piece whose neighbor is seed returns Placed
- `PlacingPieceAdvancesDeck` — after Placed result, `CurrentDeckPiece` returns next piece
- `AlreadyPlacedPieceReturnsAlreadyPlaced` — placing same piece twice returns AlreadyPlaced
- `OnPlacementResolvedFiresOnCorrectPlacement` — event fires with Placed result
- `OnPlacementResolvedFiresOnRejection` — event fires with Rejected result
- `OnPlacementResolvedFiresOnAlreadyPlaced` — event fires with AlreadyPlaced result
- `IsCompleteWhenAllPiecesPlaced` — IsComplete true after all non-seed pieces placed
- `IsNotCompleteWithRemainingPieces` — IsComplete false when pieces remain
- `DeckIsEmptyAfterAllPiecesAdvanced` — IDeck.IsEmpty true after advancing past all pieces
- `MultiPieceChainPlacement` — place A (seed), B (neighbor of A), C (neighbor of B) in sequence, all Placed

### Key Links
- Tests reference `SimpleGame.Puzzle` types only — no `SimpleGame.Game` or `SimpleGame.Core`

## Steps
1. Create `Assets/Tests/EditMode/Puzzle/` directory
2. Write `SimpleGame.Tests.Puzzle.asmdef`
3. Write `PuzzleDomainTests.cs` with all 12 test cases using builder helpers for constructing test levels
4. Run tests via stdin pipe: `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin` then poll `get_test_job`
5. Fix any failures until all 12 pass

## Context
- Use `PuzzleLevel` constructor directly in tests — no factory needed yet
- Build test levels with explicit piece/neighbor lists: e.g. piece 0 (seed, neighbors:[1,2]), piece 1 (neighbors:[0,3]), piece 2 (neighbors:[0]), piece 3 (neighbors:[1])
- Deck ordering is just an `int[]` — pass directly to `Deck` constructor
- Per K006: `mcporter call unityMCP.run_tests` crashes on Windows; use stdin pipe mode exclusively
