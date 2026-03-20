# S02: Wire into JigsawLevelFactory

**Goal:** Replace `Shuffle()` in `JigsawLevelFactory.Build()` with `SolvableShuffle.Shuffle()`, and drop the `BuildSolvable` retry cap from 100 to 10.

**Demo:** All existing `JigsawAdapterTests` pass; `BuildSolvable` max attempts is 10; private `Shuffle()` is removed from `JigsawLevelFactory`.

## Must-Haves

- `JigsawLevelFactory.Build()` calls `SolvableShuffle.Shuffle(seeds, pieces, slotCount, rng)` instead of `Shuffle(deckOrder, rng)`
- `Build()` signature gains `slotCount` parameter (needed by `SolvableShuffle`)
- `BuildSolvable` default `maxAttempts` is 10
- Private `Shuffle<T>()` method removed from `JigsawLevelFactory`
- All existing `JigsawAdapterTests` pass (update call sites if `Build()` signature changed)
- `IsSolvable` is unchanged

## Tasks

- [ ] **T01: Update JigsawLevelFactory**
  Add `slotCount` to `Build()`, call `SolvableShuffle.Shuffle()`, remove private `Shuffle<T>()`, drop retry cap to 10.

- [ ] **T02: Update call sites and verify tests**
  Update `JigsawAdapterTests` and any other callers of `Build()` for the new signature; confirm all tests pass.

## Files Likely Touched

- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs`
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs`
- Any other file calling `JigsawLevelFactory.Build()`
