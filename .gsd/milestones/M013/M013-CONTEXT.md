# M013: Solvable Deck Shuffle

**Gathered:** 2026-03-18
**Status:** Ready for planning

## Project Description

A Unity jigsaw puzzle game. Deck generation currently uses Fisher-Yates shuffle then validates solvability post-hoc via a greedy solver, retrying up to 100 times with different seeds. The shuffle has no awareness of puzzle topology.

## Why This Milestone

Post-hoc retry is wasteful and fragile for constrained topologies. A shuffle that guarantees solvability by construction is more reliable and sets up better difficulty tuning later.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Play a puzzle that was generated with a topology-aware shuffle (no behavioral change visible, but the underlying deck order is guaranteed solvable by construction)

### Entry point / environment

- Entry point: `JigsawLevelFactory.Build()` / `BuildSolvable()`
- Environment: Unity EditMode tests + runtime game
- Live dependencies involved: none (pure domain logic)

## Completion Class

- Contract complete means: unit tests in `SimpleGame.Tests.Puzzle` and `SimpleGame.Tests.Game` pass
- Integration complete means: `JigsawLevelFactory.Build()` uses `SolvableShuffle`; `BuildSolvable` retry cap = 10
- Operational complete means: none

## Final Integrated Acceptance

- All existing `JigsawAdapterTests` and `PuzzleModelTests` pass unchanged
- New `SolvableShuffleTests` cover: solvability guarantee, backtracking, anti-trivialisation, slot-count sensitivity

## Risks and Unknowns

- Anti-trivialisation: ensuring the algorithm doesn't produce all-valid windows without also creating deadlocks — needs careful balance
- Backtracking depth: unlimited backtracking could be expensive on large grids; cap must be chosen carefully

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` — current shuffle + IsSolvable + retry loop
- `Assets/Scripts/Puzzle/` — domain assembly (`SimpleGame.Puzzle`), `noEngineReferences: true`
- `Assets/Tests/EditMode/Puzzle/` — `SimpleGame.Tests.Puzzle` assembly
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs` — existing adapter tests to preserve

## Implementation Decisions

- `SolvableShuffle` lives in `SimpleGame.Puzzle` assembly — pure C#, no Unity deps
- `IsSolvable` in `JigsawLevelFactory` stays exactly as-is
- `BuildSolvable` retry cap changes from 100 → 10
- Algorithm inputs: seed piece IDs, all available (non-seed) pieces with their neighbor sets, slot count
- Algorithm guarantees: at every position in the deck, at least one piece within the next `slotCount` positions is placeable given what's been placed so far
- Anti-trivialisation: the algorithm must not degenerate into always picking the easiest (most-connected) piece — enforce some ordering variance

## Scope

### In Scope

- `SolvableShuffle` standalone class in `SimpleGame.Puzzle`
- Unit tests for `SolvableShuffle` in `SimpleGame.Tests.Puzzle`
- Wire `SolvableShuffle` into `JigsawLevelFactory.Build()`
- Drop retry cap from 100 → 10

### Out of Scope / Non-Goals

- Difficulty tuning (hard vs easy decks)
- Changes to `IsSolvable` logic
- Any UI or rendering changes

## Open Questions

- None — algorithm approach confirmed: DP/incremental, piece-by-piece, limited backtracking, anti-trivialisation guard
