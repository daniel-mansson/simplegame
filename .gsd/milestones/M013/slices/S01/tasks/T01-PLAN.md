# T01: SolvableShuffle Implementation

**Slice:** S01
**Milestone:** M013

## Goal

Implement `SolvableShuffle.Shuffle()` — a static method that builds a deck ordering incrementally, guaranteeing at least one placeable piece per slot window throughout the sequence, with limited backtracking and anti-trivialisation.

## Must-Haves

### Truths
- `SolvableShuffle.Shuffle()` returns a list containing exactly all non-seed piece IDs (same set as Fisher-Yates would produce)
- At every index `i` in the returned list, at least one piece in `result[i .. i + slotCount - 1]` has a placed neighbour given `placed = seeds ∪ result[0..i-1]`
- When no valid placement exists within `slotCount` lookahead, backtracking occurs (up to 50 steps back) before giving up
- The algorithm does not always return pieces in the order of "most connected first" — ordering has genuine variance across seeds

### Artifacts
- `Assets/Scripts/Puzzle/SolvableShuffle.cs` — min 60 lines, no Unity using directives, namespace `SimpleGame.Puzzle`

### Key Links
- `SolvableShuffle` uses `IPuzzlePiece.NeighborIds` for connectivity — same interface already used by `PuzzleBoard`
- No dependency on `PuzzleBoard` or `PuzzleModel` — self-contained neighbor tracking

## Steps

1. Create `Assets/Scripts/Puzzle/SolvableShuffle.cs` with namespace `SimpleGame.Puzzle`
2. Define `public static class SolvableShuffle` with XML doc
3. Implement `public static List<int> Shuffle(IReadOnlyList<int> seedIds, IReadOnlyList<IPuzzlePiece> pieces, int slotCount, System.Random rng)`
4. Build neighbor map: `Dictionary<int, List<int>>` from pieces
5. Initialize `placed` HashSet from seedIds; build `remaining` list of non-seed IDs
6. Shuffle `remaining` with Fisher-Yates as the initial candidate order (this is the anti-trivialisation base — we start random, then constrain)
7. Main loop: for each position `i` in `result`:
   - From `remaining`, find all candidates that are placeable now (have a placed neighbour) — call this `valid`
   - Also track candidates that are NOT placeable — call this `invalid`
   - **Solvability window check:** if `valid` is empty, backtrack (up to 50 steps): undo last placement, try a different candidate at that position
   - **Anti-trivialisation:** if `valid.Count > 1` AND `invalid.Count > 0` AND `slotCount > 1`, allow placing one `invalid` candidate first (a piece that isn't yet placeable) as long as a valid piece still exists within the next `slotCount - 1` positions in `remaining` — this creates non-trivial windows
   - Pick a candidate (random from valid, or the anti-trivialisation invalid pick), add to result, add to `placed`, remove from `remaining`
8. If backtracking exhausts all options at a position, append remaining in current order (best-effort)
9. Return result

## Context

- `IPuzzlePiece` is in `SimpleGame.Puzzle` — already accessible
- The algorithm must work without Unity (assembly has `noEngineReferences: true`)
- Anti-trivialisation: the key insight is that with `slotCount > 1`, you can place an "not-yet-valid" piece at position `i` as long as a valid piece exists at position `i+1` (within the window). This makes windows non-trivially-all-valid without breaking solvability.
- Keep backtrack state minimal: a stack of `(position, candidateIndex)` is sufficient
