---
id: S02
milestone: M006
provides:
  - IGoldenPieceService + GoldenPieceService — golden piece currency backed by IMetaSaveService
  - IHeartService + HeartService — in-memory per-level heart tracking
  - Reload-then-merge save pattern preventing shared MetaSaveData contention
  - 27 edit-mode tests (15 GoldenPieceService + 12 HeartService)
key_files:
  - Assets/Scripts/Game/Services/IGoldenPieceService.cs
  - Assets/Scripts/Game/Services/GoldenPieceService.cs
  - Assets/Scripts/Game/Services/IHeartService.cs
  - Assets/Scripts/Game/Services/HeartService.cs
  - Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs
  - Assets/Tests/EditMode/Game/HeartServiceTests.cs
key_decisions:
  - "Reload-then-merge save: services sharing IMetaSaveService reload latest data before persisting their own fields"
  - "HeartService is stateless across levels — no persistence, caller resets per level"
patterns_established:
  - "Reload-then-merge save pattern for multi-service shared persistence"
drill_down_paths:
  - .gsd/milestones/M006/slices/S02/tasks/T01-SUMMARY.md
verification_result: pass
completed_at: 2026-03-17T13:40:00Z
---

# S02: Currency and heart services

**Built golden piece currency service (earn/spend/persist) and per-level heart service (reset/use/death) with 27 edit-mode tests and reload-then-merge save pattern**

## What Happened

Created two interface-backed services:

**GoldenPieceService** — manages golden puzzle piece currency. Backed by IMetaSaveService for persistence. Balance property, Earn(amount), TrySpend(amount) → bool, Save(). Uses reload-then-merge on Save() to prevent overwriting MetaProgressionService's objectProgress when sharing the same PlayerPrefs save key. 15 tests including cross-service data preservation.

**HeartService** — pure in-memory per-level heart tracking. RemainingHearts, IsAlive, Reset(count), UseHeart() → bool. Starts at 0 hearts (not alive). Reset at level start, UseHeart on incorrect placement, fail at 0. 12 tests covering all states.

Also retrofitted MetaProgressionService.Save() with the same reload-then-merge pattern to prevent it from overwriting golden piece balance.

## Tasks Completed
- T01: GoldenPieceService and HeartService with tests
