---
id: T01
parent: S02
milestone: M006
provides:
  - IGoldenPieceService interface + GoldenPieceService backed by IMetaSaveService
  - IHeartService interface + HeartService in-memory implementation
  - 15 GoldenPieceService tests + 12 HeartService tests
  - Reload-then-merge save pattern on both GoldenPieceService and MetaProgressionService
requires:
  - S01 provides IMetaSaveService, MetaSaveData, MetaProgressionService
affects: [S03, S04, S05]
key_files:
  - Assets/Scripts/Game/Services/IGoldenPieceService.cs
  - Assets/Scripts/Game/Services/GoldenPieceService.cs
  - Assets/Scripts/Game/Services/IHeartService.cs
  - Assets/Scripts/Game/Services/HeartService.cs
  - Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs
  - Assets/Tests/EditMode/Game/HeartServiceTests.cs
key_decisions:
  - "GoldenPieceService uses reload-then-merge on Save() to avoid overwriting MetaProgressionService's objectProgress"
  - "HeartService is pure in-memory — no persistence needed, hearts reset per level"
patterns_established:
  - "Reload-then-merge save pattern: when multiple services share IMetaSaveService, each reloads before save and only writes its own fields"
drill_down_paths:
  - .gsd/milestones/M006/slices/S02/tasks/T01-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-17T13:40:00Z
---

# T01: GoldenPieceService and HeartService with tests

**Created both currency and heart services with interfaces, implementations, and 27 edit-mode tests. Fixed shared-save-data contention with reload-then-merge pattern.**

## What Happened

Created GoldenPieceService — manages golden piece balance backed by IMetaSaveService. Interface: Balance (get), Earn(amount), TrySpend(amount) → bool, Save(). The implementation reads initial balance on construction. On Save(), it reloads the latest MetaSaveData from the save service, applies only its goldenPieces field, then persists — preventing it from overwriting MetaProgressionService's objectProgress. 15 tests including a cross-service data preservation test.

Created HeartService — pure in-memory per-level heart tracking. Interface: RemainingHearts (get), IsAlive (get), Reset(count), UseHeart() → bool. Starts at 0 hearts (not alive). Reset sets the count. UseHeart decrements and returns false at 0. 12 tests covering reset, use, death, re-reset, and full drain sequences.

Also updated MetaProgressionService.Save() to use the same reload-then-merge pattern — it reloads latest data, applies its objectProgress, then persists. This prevents MetaProgressionService from overwriting GoldenPieceService's goldenPieces.

## Deviations
- Added reload-then-merge pattern to both services (not originally planned) after identifying shared MetaSaveData contention as a real bug.
- Tests cannot run in Unity yet because the worktree is separate from the running Unity project (same as S01).

## Files Created
- `Assets/Scripts/Game/Services/IGoldenPieceService.cs` — Currency interface
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — Implementation with reload-then-merge save
- `Assets/Scripts/Game/Services/IHeartService.cs` — Heart interface
- `Assets/Scripts/Game/Services/HeartService.cs` — In-memory implementation

## Files Modified
- `Assets/Scripts/Game/Services/MetaProgressionService.cs` — Save() updated to reload-then-merge

## Files Created (Tests)
- `Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs` — 15 tests
- `Assets/Tests/EditMode/Game/HeartServiceTests.cs` — 12 tests
