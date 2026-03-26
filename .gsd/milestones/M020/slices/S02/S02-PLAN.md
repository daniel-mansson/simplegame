# S02: Move Economy, Save, Progression, PlayFab, and MetaProgressionService

**Goal:** Create `Economy/`, `Save/`, `Progression/`, and `PlayFab/` feature folders; move MetaProgressionService to `Meta/`; move all remaining `Services/` files; remove the now-empty `Services/` folder and its `.meta`.

**Demo:** `Services/` directory gone; 5 target folders contain their respective files; tests pass.

## Must-Haves

- `Assets/Scripts/Game/Services/` directory does not exist
- `Assets/Scripts/Game/Services.meta` does not exist (git rm'd)
- `Economy/` (6 files), `Save/` (4 files), `Progression/` (4 files), `PlayFab/` (16 files) all exist
- `Meta/` has gained `MetaProgressionService.cs`
- 340 EditMode tests pass

## Tasks

- [ ] **T01: Move Economy (6), Save (4), MetaProgressionService to Meta/**
  Economy: ICoinsService, CoinsService, IGoldenPieceService, GoldenPieceService, IHeartService, HeartService.
  Save: IMetaSaveService, MetaSaveData, MetaSaveMerge, PlayerPrefsMetaSaveService.
  Meta: MetaProgressionService.cs (moved to existing Meta/ folder).

- [ ] **T02: Move Progression (4) and PlayFab (16), remove Services/ folder, run tests, commit**
  Progression: ProgressionService, GameService, GameSessionService, GameOutcome.
  PlayFab (14 from Services/): IPlayFabAuthService, PlayFabAuthService, ICloudSaveService, PlayFabCloudSaveService, IAnalyticsService, PlayFabAnalyticsService, IPlatformLinkService, PlayFabPlatformLinkService, IRemoteConfigService, PlayFabRemoteConfigService, GameRemoteConfig, ISingularService, SingularService, NullSingularService.
  PlayFab (2 from Popup/): IPlatformLinkView, PlatformLinkPresenter.
  Then: rmdir Services/ + git rm Services.meta. Run tests; commit.

## Files Likely Touched

- `Assets/Scripts/Game/Services/` — emptied and removed
- `Assets/Scripts/Game/Economy/`, `Save/`, `Progression/`, `PlayFab/` — created
- `Assets/Scripts/Game/Meta/` — gains MetaProgressionService.cs
