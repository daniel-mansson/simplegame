# T02: Update Call Sites and Verify Tests

**Slice:** S02
**Milestone:** M013

## Goal

Update all callers of `JigsawLevelFactory.Build()` for the new `slotCount` parameter, confirm all existing tests pass.

## Must-Haves

### Truths
- All `JigsawAdapterTests` pass
- All `PuzzleModelTests` and `PuzzleBoardTests` pass (they don't touch factory, but confirm no regressions)
- No compile errors anywhere in the solution

### Artifacts
- `Assets/Tests/EditMode/Game/JigsawAdapterTests.cs` — updated call sites for new `Build()` signature

### Key Links
- `JigsawAdapterTests` calls `JigsawLevelFactory.Build()` directly — needs `slotCount` argument

## Steps

1. `rg "JigsawLevelFactory.Build(" Assets/` — find all call sites
2. Update each call to include `slotCount` (use `1` for tests that don't care about slot count, or match the test's intent)
3. Check `BuildSolvable` call sites — it already passes `slotCount` through; confirm no changes needed there
4. Run LSP diagnostics on modified files
5. Trigger Unity compilation (via MCP or note for manual trigger)
6. Run tests via stdin pipe workaround (K006): `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin`
7. Poll `get_test_job` until complete; assert all pass

## Context

- K006: `run_tests` on Windows must use stdin pipe mode — see KNOWLEDGE.md
- If `Build()` default `slotCount = 1` is used, existing test calls without the arg still compile (default parameter)
- `BuildSolvable_2x2_PuzzleIsCompletableWithReturnedDeck` uses `slotCount: 3` — this call goes through `BuildSolvable` which already takes slotCount, so no change needed there
