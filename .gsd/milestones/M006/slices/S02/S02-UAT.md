# S02: Currency and heart services — UAT

**Milestone:** M006
**Written:** 2026-03-17

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S02 produces pure service code with no UI. All behavior is proven by edit-mode tests. No runtime or visual verification needed.

## Preconditions

- Worktree branch merged into Unity project or Unity pointed at worktree
- Unity compiles without errors

## Smoke Test

All 27 new tests (15 GoldenPieceService + 12 HeartService) pass in edit-mode test runner.

## Test Cases

### 1. GoldenPieceService earn and spend

1. Run GoldenPieceServiceTests
2. **Expected:** 15/15 pass — earn increases balance, spend deducts, insufficient balance rejected, persistence round-trips

### 2. HeartService lifecycle

1. Run HeartServiceTests
2. **Expected:** 12/12 pass — reset sets count, use decrements, 0 = not alive, use-when-dead returns false, re-reset works

### 3. Cross-service save safety

1. Run `Save_PreservesObjectProgressFromOtherService` test
2. **Expected:** GoldenPieceService.Save() preserves MetaProgressionService's objectProgress in shared save data

## Edge Cases

### Zero/negative amounts

1. Call Earn(0), Earn(-1), TrySpend(0)
2. **Expected:** No balance change, warning logs

### Hearts never reset

1. Create HeartService, call UseHeart() without Reset()
2. **Expected:** Returns false, remains not alive

## Failure Signals

- Compile errors in SimpleGame.Game or SimpleGame.Tests.Game assemblies
- Any test failure in GoldenPieceServiceTests or HeartServiceTests
- MetaProgressionServiceTests regression (existing 18 tests)

## Requirements Proved By This UAT

- R048 — Golden pieces earned and spent (service layer proven, UI integration in S04/S05)
- R057 — Heart system 3 per level (service proven, integration in S03)

## Not Proven By This UAT

- UI integration (no screens or popups yet)
- Runtime persistence in play mode (service is correct, wiring happens in S06)
- Actual heart count of 3 (service supports any count, caller passes 3)

## Notes for Tester

Tests use MockMetaSaveService (in-memory) — no PlayerPrefs side effects. The reload-then-merge save pattern is new and critical for correctness when multiple services share IMetaSaveService.
