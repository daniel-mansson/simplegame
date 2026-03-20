# M013: Solvable Deck Shuffle

**Vision:** Replace the Fisher-Yates shuffle in deck generation with a topology-aware algorithm that guarantees solvability by construction, while keeping the existing `IsSolvable` post-hoc check as a safety net (retry cap 10).

## Success Criteria

- `SolvableShuffle` produces a deck ordering where at least one piece in every `slotCount`-wide window is placeable at that point in the sequence
- Algorithm never degenerates into all-valid windows (anti-trivialisation)
- `JigsawLevelFactory.Build()` uses `SolvableShuffle` instead of Fisher-Yates
- `BuildSolvable` retry cap is 10
- All existing puzzle and adapter tests pass

## Key Risks / Unknowns

- Anti-trivialisation guard conflicting with solvability guarantee — need to find the right balance
- Backtracking depth cap: too low → falls back too often; too high → slow on large grids

## Proof Strategy

- Anti-trivialisation risk → retire in S01 by unit-testing that some windows contain non-placeable pieces on simple constrained topologies
- Backtracking cap → retire in S01 by confirming algorithm completes in < 1ms for 4×4 grids

## Verification Classes

- Contract verification: NUnit EditMode tests in `SimpleGame.Tests.Puzzle` and `SimpleGame.Tests.Game`
- Integration verification: `JigsawLevelFactory.Build()` produces a result that `IsSolvable` confirms in one pass
- Operational verification: none
- UAT / human verification: none

## Milestone Definition of Done

- `SolvableShuffle` class exists in `Assets/Scripts/Puzzle/` with no Unity deps
- `SolvableShuffleTests.cs` exists in `Assets/Tests/EditMode/Puzzle/` and all tests pass
- `JigsawLevelFactory.Build()` calls `SolvableShuffle` instead of `Shuffle()`
- `BuildSolvable` `maxAttempts` default is 10
- All existing `PuzzleModelTests`, `PuzzleBoardTests`, and `JigsawAdapterTests` still pass

## Requirement Coverage

- Covers: solvable deck generation by construction
- Leaves for later: difficulty tuning, variable window sizes

## Slices

- [ ] **S01: SolvableShuffle algorithm** `risk:medium` `depends:[]`
  > After this: unit tests demonstrate the algorithm produces solvable orderings, fires backtracking on constrained topologies, and never produces all-valid windows.

- [ ] **S02: Wire into JigsawLevelFactory** `risk:low` `depends:[S01]`
  > After this: `JigsawLevelFactory.Build()` uses `SolvableShuffle`; retry cap is 10; all existing adapter tests pass.

## Boundary Map

### S01 → S02

Produces:
- `SolvableShuffle` class in `SimpleGame.Puzzle` namespace
- Static method: `Shuffle(IReadOnlyList<int> seedIds, IReadOnlyList<IPuzzlePiece> pieces, int slotCount, System.Random rng) → List<int>`
- Takes seed IDs, all pieces (with neighbor sets), slot count, and an RNG instance
- Returns an ordered list of non-seed piece IDs guaranteed to have at least one placeable piece per slot window throughout the sequence

Consumes:
- nothing (first slice)

### S02 → done

Produces:
- `JigsawLevelFactory.Build()` wired to `SolvableShuffle`
- `BuildSolvable` retry cap = 10
- Private `Shuffle()` method removed from `JigsawLevelFactory`

Consumes from S01:
- `SolvableShuffle.Shuffle()` static method
