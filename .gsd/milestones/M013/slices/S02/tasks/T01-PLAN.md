# T01: Update JigsawLevelFactory

**Slice:** S02
**Milestone:** M013

## Goal

Wire `SolvableShuffle.Shuffle()` into `JigsawLevelFactory.Build()`, add `slotCount` parameter, remove private `Shuffle<T>()`, and drop `BuildSolvable` retry cap to 10.

## Must-Haves

### Truths
- `JigsawLevelFactory.Build()` no longer contains a `Shuffle()` call — uses `SolvableShuffle.Shuffle()` instead
- `Build()` has a `slotCount` parameter (int, with a sensible default e.g. `1`)
- `BuildSolvable` `maxAttempts` default is `10`
- Private `Shuffle<T>` method is removed
- `IsSolvable` method is unchanged

### Artifacts
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` — modified, no `Shuffle<T>` private method, calls `SolvableShuffle.Shuffle()`

### Key Links
- `JigsawLevelFactory` is in `SimpleGame.Game` assembly which references `SimpleGame.Puzzle` — `SolvableShuffle` is accessible

## Steps

1. Open `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs`
2. Add `slotCount` parameter to `Build()` (after `seedPieceIds`, default value `1`)
3. Replace `Shuffle(deckOrder, rng)` call with `SolvableShuffle.Shuffle(seeds, pieces, slotCount, rng)` — note: `SolvableShuffle` returns the deck directly (no separate `deckOrder` list needed)
4. Remove the private `Shuffle<T>(List<T>, System.Random)` method
5. In `BuildSolvable`, change `int maxAttempts = 100` → `int maxAttempts = 10`
6. Update `BuildSolvable`'s call to `Build()` to pass `slotCount` through
7. Run LSP diagnostics to confirm no compile errors

## Context

- `SolvableShuffle.Shuffle()` takes `(IReadOnlyList<int> seedIds, IReadOnlyList<IPuzzlePiece> pieces, int slotCount, System.Random rng)` and returns `List<int>` — this replaces both the `deckOrder` building loop and the `Shuffle()` call
- The `deckOrder` list construction (filtering seeds from pieces) should be removed since `SolvableShuffle` handles that internally
