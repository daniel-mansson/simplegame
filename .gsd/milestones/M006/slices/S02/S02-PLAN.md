# S02: Currency and heart services

**Goal:** GoldenPieceService and HeartService exist as interface-backed plain C# services with full edit-mode test coverage.
**Demo:** Tests prove golden piece earn/spend/balance/persist and heart decrement/reset/death semantics.

## Must-Haves

- IGoldenPieceService interface + GoldenPieceService implementation backed by IMetaSaveService
- IHeartService interface + HeartService in-memory implementation
- Edit-mode tests for both services

## Verification

- All new tests pass in `MetaProgressionServiceTests.cs` (existing 18) + new `GoldenPieceServiceTests.cs` + `HeartServiceTests.cs`
- No compile errors across both assemblies

## Tasks

- [x] **T01: GoldenPieceService and HeartService with tests** `est:20m`
  - Why: S02 is small — both services are simple enough for a single task. GoldenPieceService wraps IMetaSaveService for golden piece balance (earn/spend/persist). HeartService is pure in-memory per-level heart tracking.
  - Files: `Assets/Scripts/Game/Services/IGoldenPieceService.cs`, `Assets/Scripts/Game/Services/GoldenPieceService.cs`, `Assets/Scripts/Game/Services/IHeartService.cs`, `Assets/Scripts/Game/Services/HeartService.cs`, `Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs`, `Assets/Tests/EditMode/Game/HeartServiceTests.cs`
  - Do:
    1. Create `IGoldenPieceService` — Balance (get), Earn(amount), TrySpend(amount) → bool, Save()
    2. Create `GoldenPieceService` — takes IMetaSaveService in constructor, reads/writes goldenPieces field on MetaSaveData
    3. Create `IHeartService` — RemainingHearts (get), Reset(count), UseHeart() → bool, IsAlive (get)
    4. Create `HeartService` — pure in-memory, starts at 0, Reset sets count, UseHeart decrements (returns false at 0)
    5. Write `GoldenPieceServiceTests` — earn, spend, insufficient balance, persist round-trip, initial zero
    6. Write `HeartServiceTests` — reset, use, death, use-when-dead, re-reset
  - Verify: All tests compile and pass
  - Done when: Both services have interfaces, implementations, and passing edit-mode tests

## Files Likely Touched

- `Assets/Scripts/Game/Services/IGoldenPieceService.cs`
- `Assets/Scripts/Game/Services/GoldenPieceService.cs`
- `Assets/Scripts/Game/Services/IHeartService.cs`
- `Assets/Scripts/Game/Services/HeartService.cs`
- `Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs`
- `Assets/Tests/EditMode/Game/HeartServiceTests.cs`
