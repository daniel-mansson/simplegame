---
estimated_steps: 6
estimated_files: 6
---

# T01: GoldenPieceService and HeartService with tests

**Slice:** S02 — Currency and heart services
**Milestone:** M006

## Description

Create both currency and heart services with interfaces, implementations, and edit-mode tests. GoldenPieceService manages golden piece balance backed by IMetaSaveService. HeartService is pure in-memory per-level heart tracking.

## Steps

1. Create IGoldenPieceService interface — Balance, Earn, TrySpend, Save
2. Create GoldenPieceService — reads from IMetaSaveService on construction, reload-then-merge on Save to preserve objectProgress from MetaProgressionService
3. Create IHeartService interface — RemainingHearts, IsAlive, Reset, UseHeart
4. Create HeartService — pure in-memory, starts at 0, Reset sets count, UseHeart decrements
5. Write GoldenPieceServiceTests — 15 tests covering earn, spend, edge cases, persistence, cross-service data preservation
6. Write HeartServiceTests — 12 tests covering reset, use, death, re-reset, full sequences
7. Fix MetaProgressionService.Save() to also reload-then-merge (preserve goldenPieces)

## Must-Haves

- [x] IGoldenPieceService interface with Balance, Earn, TrySpend, Save
- [x] GoldenPieceService backed by IMetaSaveService with reload-then-merge save
- [x] IHeartService interface with RemainingHearts, IsAlive, Reset, UseHeart
- [x] HeartService pure in-memory implementation
- [x] GoldenPieceServiceTests — 15 tests
- [x] HeartServiceTests — 12 tests
- [x] MetaProgressionService.Save() updated to reload-then-merge

## Verification

- Code follows established patterns (interface-backed, plain C#, no static state)
- Tests use MockMetaSaveService pattern from S01
- Cross-service save test proves GoldenPieceService.Save() preserves objectProgress

## Inputs

- `Assets/Scripts/Game/Services/IMetaSaveService.cs` — persistence interface from S01
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — shared save data with goldenPieces field
- `Assets/Tests/EditMode/Game/MetaProgressionServiceTests.cs` — MockMetaSaveService pattern

## Expected Output

- `Assets/Scripts/Game/Services/IGoldenPieceService.cs` — interface
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — implementation
- `Assets/Scripts/Game/Services/IHeartService.cs` — interface
- `Assets/Scripts/Game/Services/HeartService.cs` — implementation
- `Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs` — 15 tests
- `Assets/Tests/EditMode/Game/HeartServiceTests.cs` — 12 tests
- `Assets/Scripts/Game/Services/MetaProgressionService.cs` — Save() updated
