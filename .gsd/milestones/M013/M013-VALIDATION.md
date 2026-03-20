---
verdict: pass
remediation_round: 0
---

# Milestone Validation: M013

## Success Criteria Checklist

- [x] **`SolvableShuffle` produces a deck where at least one piece in every `slotCount`-wide window is placeable** — `AssertWindowInvariant` simulates the slot-based game and verifies no deadlock across linear chain, diamond, star, and grid topologies at slotCount 1–3. `GridGraph_9x9_SlotCount3_AlwaysSolvable_10Seeds` and `GridGraph_4x4_SlotCount3_AlwaysSolvable_20Seeds` confirm this at scale. `JigsawAdapterTests.BuildSolvable_9x9_SlotCount3_AlwaysSolvableInOneAttempt` confirms IsSolvable passes on the first try (maxAttempts=1) across 10 seeds.

- [x] **Algorithm never degenerates into all-valid windows (anti-trivialisation)** — `AntiTrivialisation_NotAllWindowsAllValid_DiamondSlotCount2` asserts that paired (invalid=3, valid=1or2) windows are produced on at least one seed in 100 trials. `AntiTrivialisation_StarGraph_NotAlwaysAscending` confirms ordering variance across 30 seeds. Implementation uses Fisher-Yates initial shuffle + paired (invalid, valid) emission controlled by `pairsThisWindow` counter.

- [x] **`JigsawLevelFactory.Build()` uses `SolvableShuffle` instead of Fisher-Yates** — `Build()` calls `SolvableShuffle.Shuffle(seeds, pieces, slotCount, rng)` directly. No private `Shuffle()` method exists in `JigsawLevelFactory.cs` (grep returned no matches). `BuildSolvable()` calls `Build()` which delegates to `SolvableShuffle`.

- [x] **`BuildSolvable` retry cap is 10** — `BuildSolvable()` signature: `int maxAttempts = 10`. `InGameSceneController` calls `BuildSolvable(_runtimeGridConfig, slotCount, initialSeed)` with no explicit `maxAttempts` override, so the default 10 applies. (Note: a stale comment on line 266 still reads "retries up to 100 seeds" — this is a cosmetic issue only; the functional default is confirmed as 10.)

- [x] **All existing puzzle and adapter tests pass** — S01 summary reports 261/261 tests pass. S02 summary reports 261/261 tests pass. The final fix commit (`8c4ea27`) records 264/264 tests passing (three additional test cases were added to `JigsawAdapterTests` in S02: `BuildSolvable_9x9_SlotCount3_AlwaysSolvableInOneAttempt`, `BuildSolvable_2x2_ReturnsSolvableResult`, `BuildSolvable_2x2_PuzzleIsCompletableWithReturnedDeck`). All previously existing `PuzzleModelTests`, `PuzzleBoardTests`, and `JigsawAdapterTests` are included in that count.

## Slice Delivery Audit

| Slice | Claimed | Delivered | Status |
|-------|---------|-----------|--------|
| S01 | `SolvableShuffle` static class in `SimpleGame.Puzzle`, no Unity deps; `Shuffle(seedIds, pieces, slotCount, rng) → List<int>`; window invariant; anti-trivialisation; backtracking (50 steps); `SolvableShuffleTests` with 20+ tests | `Assets/Scripts/Puzzle/SolvableShuffle.cs` exists, ~300 lines, `namespace SimpleGame.Puzzle`, zero Unity imports, `noEngineReferences: true` in asmdef confirmed. `Shuffle()` signature matches exactly. `MaxBacktrackSteps = 50`. `SolvableShuffleTests.cs` exists with 22 test methods (29 test cases after parameterised expansion). All correctness properties covered. | **pass** |
| S02 | `JigsawLevelFactory.Build()` uses `SolvableShuffle`; `slotCount` added as required param; `BuildSolvable` default `maxAttempts = 10`; `InGameSceneController` no longer overrides `maxAttempts`; private `Shuffle()` removed; `JigsawAdapterTests` updated; 261 tests pass | All confirmed. `Build()` calls `SolvableShuffle.Shuffle()` directly. `slotCount` is required (no default). `maxAttempts = 10`. `InGameSceneController` omits `maxAttempts` argument. No private `Shuffle()` in factory. `JigsawAdapterTests` updated with `slotCount: 1` at all call sites. Final test count is 264 (3 new adapter tests added). | **pass** |

## Cross-Slice Integration

**S01 → S02 boundary** — S01 produces `SolvableShuffle.Shuffle(IReadOnlyList<int>, IReadOnlyList<IPuzzlePiece>, int, System.Random) → List<int>` in `SimpleGame.Puzzle`. S02 consumes this via `SolvableShuffle.Shuffle(seeds, pieces, slotCount, rng)` in `JigsawLevelFactory.Build()`. Signature matches exactly. No mismatches.

**Minor cosmetic issue (non-blocking):** `InGameSceneController` line 266 still contains the stale comment "BuildSolvable runs a greedy solver and retries up to 100 seeds to guarantee solvability." This comment predates M013. The code behavior is correct (10 retries by default), so this is documentation debt only.

## Requirement Coverage

M013 does not introduce new requirements in `REQUIREMENTS.md`. The milestone addresses the solvable deck generation capability referenced in the project description and M013-CONTEXT. All active requirements from prior milestones (R001–R097 scoped to earlier work) are unaffected — the changes are confined to:
- `Assets/Scripts/Puzzle/SolvableShuffle.cs` (new file, no Unity deps, consistent with R091)
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` (internal algorithm change, adapter boundary preserved per R095)
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` (one-line change removing explicit `maxAttempts` override)

No previously-validated requirements are touched in a way that could introduce regressions.

## Verdict Rationale

All five success criteria are met by direct code evidence:
1. Window invariant confirmed by unit tests on multiple topologies and integration test at 9×9 scale.
2. Anti-trivialisation confirmed by dedicated tests showing paired invalid/valid emission and non-ascending ordering.
3. `JigsawLevelFactory.Build()` exclusively uses `SolvableShuffle` — no private shuffle remains.
4. `BuildSolvable` default is 10; no caller overrides it.
5. 264 tests pass (more than the 261 cited in summaries — three additional adapter tests were added).

Both slice summaries are substantiated by files on disk. Cross-slice boundary is correctly implemented. The one cosmetic finding (stale comment) does not affect correctness or behavior.

## Remediation Plan

None required.
