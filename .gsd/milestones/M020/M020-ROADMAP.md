# M020: Feature-Cohesion Restructure

**Vision:** Reorganise `Assets/Scripts/Game/` from layer-based folders (`Services/`, `Popup/`) to feature-cohesive folders (`IAP/`, `Ads/`, `ATT/`, `Economy/`, `Save/`, `PlayFab/`, `Meta/`, `Progression/`, `Shop/`, `LevelFlow/`, `ConfirmDialog/`). Namespaces and assemblies unchanged. Pure folder reorganisation.

## Success Criteria

- `Assets/Scripts/Game/Services/` does not exist
- `Assets/Scripts/Game/Popup/` contains only `UnityViewContainer.cs`
- Every feature folder contains all related files (service + popup pair where applicable)
- All 347 EditMode tests pass
- No missing-script warnings in any scene

## Key Risks / Unknowns

- `.meta` orphans when folders are emptied — must clean up in same commit (K008)
- `git mv` required for all moves to preserve GUIDs — raw filesystem copy silently breaks scene wiring (D025)

## Proof Strategy

- GUID safety → verified by checking no missing-script warnings after each slice; `git mv` is the mechanism
- Test regression → run EditMode suite after every slice commit

## Verification Classes

- Contract verification: `rg -l "." Assets/Scripts/Game/Services/` returns nothing; file counts per target folder match manifest
- Integration verification: Unity compiles without errors; no missing-script warnings in scenes
- Operational verification: n/a
- UAT / human verification: open Unity Editor, check Console for missing-script errors; navigate file tree to spot-check folder structure

## Milestone Definition of Done

This milestone is complete only when all are true:

- All four slices complete
- `Assets/Scripts/Game/Services/` directory does not exist
- `Assets/Scripts/Game/Popup/` contains exactly 1 `.cs` file (`UnityViewContainer.cs`)
- 347 EditMode tests pass
- `git status` shows no untracked `.meta` files from emptied folders

## Requirement Coverage

- Covers: none (structural refactor, no capability change)
- Partially covers: none
- Leaves for later: none
- Orphan risks: none

## Slices

- [x] **S01: Move IAP, Ads, and ATT feature groups** `risk:medium` `depends:[]`
  > After this: `Assets/Scripts/Game/IAP/` (15 files), `Ads/` (7 files), `ATT/` (7 files) exist; those files no longer in `Services/` or `Popup/`; tests pass. Services/ and Popup/ cleanup completed ahead of schedule.

- [x] **S02: Move Economy, Save, Progression, and PlayFab feature groups** `risk:medium` `depends:[S01]`
  > After this: `Economy/` (6 files), `Save/` (4 files), `Progression/` (4 files), `PlayFab/` (17 files) exist; tests pass (347). Services/ fully removed (was already gone from S01). IPlayFabCatalogService.cs swept in as extra file. S03 work partially committed on branch already.

- [x] **S03: Move remaining Popup feature files into feature folders** `risk:low` `depends:[S02]`
  > After this: `Meta/` gains `MetaProgressionService` + ObjectRestored popup trio; `Shop/` (3 files), `LevelFlow/` (7 files), `ConfirmDialog/` (3 files) created; tests pass. Note: All S03 moves are already committed on the branch ahead of schedule (confirmed by filesystem state after S02). S03 is a verification-only pass — confirm target folders match the boundary map, then close the slice.

- [x] **S04: Final verification — compile, tests, orphan cleanup** `risk:low` `depends:[S03]`
  > After this: zero orphaned `.meta` files, 347 tests confirmed passing, no missing-script warnings, all target folder manifests verified against expected file counts.

## Boundary Map

### S01 outputs

Produces:
- `Assets/Scripts/Game/IAP/` — 15 files (IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo, IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService, NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService, IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView)
- `Assets/Scripts/Game/Ads/` — 7 files (IAdService, AdResult, UnityAdService, NullAdService, IRewardedAdView, RewardedAdPresenter, RewardedAdView)
- `Assets/Scripts/Game/ATT/` — 7 files (IATTService, ATTAuthorizationStatus, UnityATTService, NullATTService, IConsentGateView, ConsentGatePresenter, ConsentGateView)
- Corresponding `.meta` files preserved by `git mv`
- `Services/` directory removed (emptied by these moves plus pre-existing work) — ahead of schedule
- `Popup/` contains only `UnityViewContainer.cs` — ahead of schedule

### S02 outputs

Produces:
- `Assets/Scripts/Game/Economy/` — 6 files (ICoinsService, CoinsService, IGoldenPieceService, GoldenPieceService, IHeartService, HeartService)
- `Assets/Scripts/Game/Save/` — 4 files (IMetaSaveService, MetaSaveData, MetaSaveMerge, PlayerPrefsMetaSaveService)
- `Assets/Scripts/Game/Progression/` — 4 files (ProgressionService, GameService, GameSessionService, GameOutcome)
- `Assets/Scripts/Game/PlayFab/` — 15 files (IPlayFabAuthService, PlayFabAuthService, ICloudSaveService, PlayFabCloudSaveService, IAnalyticsService, PlayFabAnalyticsService, IPlatformLinkService, PlayFabPlatformLinkService, IRemoteConfigService, PlayFabRemoteConfigService, GameRemoteConfig, ISingularService, SingularService, NullSingularService, IPlatformLinkView, PlatformLinkPresenter)
- Note: `Assets/Scripts/Game/Services/` is already gone — no removal step required; verify absence only

### S03 outputs

Produces:
- `Assets/Scripts/Game/Meta/` gains: MetaProgressionService, IObjectRestoredView, ObjectRestoredPresenter, ObjectRestoredView
- `Assets/Scripts/Game/Shop/` — 3 files (IShopView, ShopPresenter, ShopView)
- `Assets/Scripts/Game/LevelFlow/` — 7 files (ILevelCompleteView, LevelCompletePresenter, LevelCompleteView, ILevelFailedView, LevelFailedPresenter, LevelFailedView, LevelFailedChoice)
- `Assets/Scripts/Game/ConfirmDialog/` — 3 files (IConfirmDialogView, ConfirmDialogPresenter, ConfirmDialogView)
- Note: `Assets/Scripts/Game/Popup/` already contains only `UnityViewContainer.cs` — verify this state is preserved; no Popup cleanup moves needed

### S04 outputs

Produces:
- Confirmed clean state: zero orphaned `.meta` files, 347 tests passing, no missing-script warnings
