---
id: T03
parent: S01
milestone: M011
provides:
  - SimpleGame.Tests.Puzzle asmdef
  - 16 EditMode tests covering all domain model behaviors
  - Test topology: 4-piece linear chain with one seed
requires: []
affects: [S02, S03]
key_files:
  - Assets/Tests/EditMode/Puzzle/SimpleGame.Tests.Puzzle.asmdef
  - Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs
key_decisions:
  - "Test topology: piece 0 (seed)→1→2 and 1→3; proves seed anchoring, chain placement, isolation rejection"
  - "RejectingPieceDoesNotAdvanceDeck tests deck invariant explicitly"
patterns_established:
  - "Puzzle tests reference only SimpleGame.Puzzle — no SimpleGame.Game or SimpleGame.Core"
  - "Test levels built inline with PuzzlePiece/PuzzleLevel/Deck constructors — no factory needed"
drill_down_paths:
  - .gsd/milestones/M011/slices/S01/tasks/T03-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-18T22:10:00Z
---

# T03: EditMode Tests

**16 EditMode tests for pure puzzle domain model — 193/193 total suite passing**

## What Happened

Created `Assets/Tests/EditMode/Puzzle/` with `SimpleGame.Tests.Puzzle.asmdef` (references `SimpleGame.Puzzle` + test runners) and `PuzzleDomainTests.cs` with 16 test cases covering: seed pre-placement (2), placement rule acceptance/rejection (4), already-placed guard (2), deck advance on correct/reject (3), win detection (2), event firing (3), chain placement (1). All 193 EditMode tests pass.

## Deviations
Added 4 extra test cases beyond the 12 required minimum: `NonSeedPiecesAreNotPlacedOnConstruction`, `PlacingPieceAddsToBoardPlacedIds`, `AlreadyPlacedPieceDoesNotMutateBoardFurther`, `PieceNotNeighboringAnyPlacedPieceIsRejected`.

## Files Created/Modified
- `Assets/Tests/EditMode/Puzzle/SimpleGame.Tests.Puzzle.asmdef` — pure puzzle test assembly
- `Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs` — 16 test cases
