# T02: SolvableShuffleTests

**Slice:** S01
**Milestone:** M013

## Goal

Write NUnit unit tests for `SolvableShuffle.Shuffle()` covering solvability guarantee, backtracking, anti-trivialisation, and slot-window semantics.

## Must-Haves

### Truths
- All tests pass in Unity EditMode test runner
- Tests cover: linear chain (slotCount=1), fully connected (any order valid), backtracking fires on constrained topology, anti-trivialisation (result is not always ascending on all-valid graph), slot-window (slotCount=2 allows non-placeable piece at i if placeable at i+1)
- Tests use only `IPuzzlePiece`, `PuzzlePiece`, and `SolvableShuffle` — no Unity types

### Artifacts
- `Assets/Tests/EditMode/Puzzle/SolvableShuffleTests.cs` — min 80 lines, namespace `SimpleGame.Tests.Puzzle`

### Key Links
- References `SimpleGame.Puzzle` assembly (already in `SimpleGame.Tests.Puzzle.asmdef`)
- Uses same `PuzzlePiece` test helper pattern as `PuzzleBoardTests` and `PuzzleModelTests`

## Steps

1. Create `Assets/Tests/EditMode/Puzzle/SolvableShuffleTests.cs`
2. Test: `LinearChain_SlotCount1_ReturnsPiecesInChainOrder` — topology `0→1→2→3→4`, seed=0 is pre-placed, slotCount=1; assert result = `[1,2,3,4]`
3. Test: `FullyStar_AllConnectedToSeed_AnyOrderValid` — all pieces connect to seed=0; assert result contains exactly all non-seed IDs (any order)
4. Test: `ResultContainsAllNonSeedPieces` — parametric over slotCount 1..3; assert result is a permutation of non-seed IDs
5. Test: `SlotWindow_NonPlaceablePieceAllowedIfValidWithinWindow` — topology: seed=0, piece 1 connects to 0, piece 2 connects only to 1; slotCount=2; assert piece 1 appears before piece 2 in result
6. Test: `AntiTrivialisation_NotAlwaysAscending` — run Shuffle 20 times with different RNG seeds on a fully-connected graph; assert at least one result is not in ascending order
7. Test: `AtEveryPosition_AtLeastOneInWindowIsPlaceable` — for any returned deck, simulate placement left-to-right and assert window invariant holds at every position
8. Verify tests compile and are recognized by Unity test runner (check for `[TestFixture]` and `[Test]` attributes)

## Context

- Assembly reference is already correct: `SimpleGame.Tests.Puzzle` references `SimpleGame.Puzzle`
- Follow the same style as `PuzzleBoardTests`: static builder helpers, no `[SetUp]`, inline topology construction
- The `AtEveryPosition` test is the strongest correctness proof — it directly verifies the invariant the algorithm claims to guarantee
