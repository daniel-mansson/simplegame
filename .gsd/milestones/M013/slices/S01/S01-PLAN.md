# S01: SolvableShuffle Algorithm

**Goal:** Implement a standalone, Unity-free `SolvableShuffle` class in `SimpleGame.Puzzle` that produces deck orderings guaranteed solvable by construction, with backtracking and anti-trivialisation.

**Demo:** NUnit tests in `SimpleGame.Tests.Puzzle` pass: solvability guarantee, backtracking on constrained topologies, anti-trivialisation check.

## Must-Haves

- `SolvableShuffle` lives in `Assets/Scripts/Puzzle/SolvableShuffle.cs`, namespace `SimpleGame.Puzzle`
- No Unity references (`noEngineReferences: true` assembly)
- Static `Shuffle(IReadOnlyList<int> seedIds, IReadOnlyList<IPuzzlePiece> pieces, int slotCount, System.Random rng) → List<int>` method
- At every deck position `i`, at least one piece in positions `[i .. i + slotCount - 1]` has a placed neighbour given the pieces placed so far
- Backtracking: limited (cap ~50 attempts per position before giving up and returning best-effort)
- Anti-trivialisation: algorithm does not always pick the piece with the most placed neighbours — enforces ordering variance (e.g. shuffle candidates before evaluating, or prefer less-connected candidates with some probability)
- `SolvableShuffleTests.cs` in `Assets/Tests/EditMode/Puzzle/` covers:
  - Linear chain: `0→1→2→3→4`, seed=0, slotCount=1 → output is `[1,2,3,4]` in order (only valid ordering)
  - Fully connected: seed=0, all others connected to seed → any order is valid → result contains all non-seed IDs
  - Anti-trivialisation: on a graph where all pieces are always valid, result is not always sorted ascending
  - Slot window: slotCount=2, topology where piece 2 is unreachable until piece 1 is placed → piece 1 appears in window before piece 2

## Tasks

- [ ] **T01: SolvableShuffle implementation**
  Core algorithm: incremental deck building with neighbor-tracking, limited backtracking, anti-trivialisation guard.

- [ ] **T02: SolvableShuffleTests**
  Unit tests covering solvability guarantee, backtracking, anti-trivialisation, and slot-window semantics.

## Files Likely Touched

- `Assets/Scripts/Puzzle/SolvableShuffle.cs` (new)
- `Assets/Tests/EditMode/Puzzle/SolvableShuffleTests.cs` (new)
