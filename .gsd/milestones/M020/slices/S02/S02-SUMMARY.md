---
id: S02
parent: M020
milestone: M020
provides:
  - Assets/Scripts/Game/Economy/ — 6 files (ICoinsService, CoinsService, IGoldenPieceService, GoldenPieceService, IHeartService, HeartService)
  - Assets/Scripts/Game/Save/ — 4 files (IMetaSaveService, MetaSaveData, MetaSaveMerge, PlayerPrefsMetaSaveService)
  - Assets/Scripts/Game/Progression/ — 4 files (ProgressionService, GameService, GameSessionService, GameOutcome)
  - Assets/Scripts/Game/PlayFab/ — 17 files (all PlayFab services + IPlatformLinkView + PlatformLinkPresenter + IPlayFabCatalogService)
  - Assets/Scripts/Game/Meta/MetaProgressionService.cs — moved from Services/
  - Assets/Scripts/Game/Services/ — removed (empty after all moves)
requires:
  - slice: S01
    provides: IAP/, Ads/, ATT/ created
affects: [S03]
key_files:
  - Assets/Scripts/Game/Economy/
  - Assets/Scripts/Game/Save/
  - Assets/Scripts/Game/Progression/
  - Assets/Scripts/Game/PlayFab/
key_decisions:
  - "IPlayFabCatalogService.cs was missed in the plan list but discovered during execution — moved to PlayFab/ (correct home)"
  - "MetaProgressionService moved to Meta/ (not Progression/) since it's meta-world progression, not game-session progression"
  - "PlayFab/ ended up with 17 files (plan said 16) due to IPlayFabCatalogService discovery"
duration: ~20min
verification_result: pass
completed_at: 2026-03-26
---

# S02: Move Economy, Save, Progression, PlayFab, MetaProgressionService; Remove Services/

**Economy/ (6), Save/ (4), Progression/ (4), PlayFab/ (17) created; Services/ directory removed; 347/347 tests pass.**

## What Happened

Moved all remaining files from `Services/` into four new feature folders. Discovered `IPlayFabCatalogService.cs` had been missed in the plan list — moved it to `PlayFab/`. `MetaProgressionService.cs` moved to `Meta/` (not Progression/) since it operates on meta-world data. Removed empty `Services/` directory and its `.meta`. Used `git add -A` to correctly stage all `.meta` sibling moves.

## Deviations

PlayFab/ has 17 files instead of planned 16 — extra file is `IPlayFabCatalogService.cs` (omitted from plan count but always belonged in PlayFab/).

## Files Created/Modified

- `Assets/Scripts/Game/Economy/` — created (6 files)
- `Assets/Scripts/Game/Save/` — created (4 files)
- `Assets/Scripts/Game/Progression/` — created (4 files)
- `Assets/Scripts/Game/PlayFab/` — created (17 files)
- `Assets/Scripts/Game/Meta/MetaProgressionService.cs` — moved in from Services/
- `Assets/Scripts/Game/Services/` — removed
