# M020: Feature-Cohesion Restructure

**Gathered:** 2026-03-26
**Status:** Ready for planning

## Project Description

A Unity mobile jigsaw puzzle game with ~130 C# source files in `Assets/Scripts/Game/`. The current structure organises by layer: a flat `Services/` folder (50 files: IAP, Ads, ATT, Economy, PlayFab, Save, Progression all mixed together) and a flat `Popup/` folder (30 files: all 9 popups and their view interfaces flat siblings). Feature code is scattered across folders — to understand the IAP feature you must look in `Services/`, `Popup/`, and the editor `CreateIAPAssets.cs`.

## Why This Milestone

Layer-based folders made sense when there were 5 services. At 50 files the navigation overhead is real: finding all IAP-related code requires knowing to look in 3 different directories. The goal is feature-cohesion: everything that belongs to one feature — its interface, service implementations, model types, popup presenter+view pair — lives in one folder. The assembly structure (`SimpleGame.Game.asmdef`) does not change. Namespaces do not change. Only folder organisation changes.

## User-Visible Outcome

### When this milestone is complete:

- Opening `Assets/Scripts/Game/IAP/` shows every IAP-related file: interface, real impl, mock, catalog, result types, popup pair
- Opening `Assets/Scripts/Game/Ads/` shows everything rewarded-ad related
- `Assets/Scripts/Game/Services/` no longer exists
- The flat `Assets/Scripts/Game/Popup/` folder contains only `UnityViewContainer.cs` (infrastructure) — all popup feature code lives in feature folders
- All 340 EditMode tests still pass
- No scene has a missing-script warning

### Entry point / environment

- Entry point: Unity Editor — file system navigation, script compilation
- Environment: local dev only — pure refactor, no runtime behaviour change
- Live dependencies: Unity scene files (GUID-sensitive), `.asset` files (GUID-sensitive)

## Completion Class

- Contract complete: all files in new locations, no files in old locations, tests pass
- Integration complete: Unity opens without missing-script warnings, all scenes load correctly
- Operational complete: n/a (no runtime behaviour change)

## Final Integrated Acceptance

- `rg -l "." Assets/Scripts/Game/Services/` returns no `.cs` files
- `find Assets/Scripts/Game/Popup -name "*.cs" | wc -l` returns 1 (UnityViewContainer only)
- 340 EditMode tests pass
- No missing-script errors in Boot, MainMenu, InGame, Settings scenes

## Risks and Unknowns

- **`m_EditorClassIdentifier` in scene/asset files** — these strings encode the namespace but NOT the folder. Since namespaces are unchanged, these strings are unaffected. GUID-based `m_Script` references are preserved by `git mv`. Zero scene risk.
- **`.meta` file orphans** — emptying a source folder leaves its `.meta` behind. Must `git rm` the `.meta` and `rmdir` the folder in the same commit (K008).
- **Editor scripts in `Assets/Editor/`** — `CreateIAPAssets.cs`, `SceneSetup.cs` etc. use `using SimpleGame.Game.Services` etc. Since namespaces are unchanged, these compile correctly after moves. No `using` changes needed anywhere.
- **`IAPMockConfig.asset` and `IAPProductCatalog.asset`** — loaded from `Resources/` by path string. Move of the `.cs` file has no effect on `Resources.Load<T>("IAPMockConfig")`. Safe.

## Proposed Folder Structure

```
Assets/Scripts/Game/
  IAP/          IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo,
                IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService,
                NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService,
                IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView

  Ads/          IAdService, AdResult, UnityAdService, NullAdService,
                IRewardedAdView, RewardedAdPresenter, RewardedAdView

  ATT/          IATTService, ATTAuthorizationStatus, UnityATTService, NullATTService,
                IConsentGateView, ConsentGatePresenter, ConsentGateView

  Economy/      ICoinsService, CoinsService,
                IGoldenPieceService, GoldenPieceService,
                IHeartService, HeartService

  Save/         IMetaSaveService, MetaSaveData, MetaSaveMerge, PlayerPrefsMetaSaveService

  PlayFab/      IPlayFabAuthService, PlayFabAuthService,
                ICloudSaveService, PlayFabCloudSaveService,
                IAnalyticsService, PlayFabAnalyticsService,
                IPlatformLinkService, PlayFabPlatformLinkService,
                IRemoteConfigService, PlayFabRemoteConfigService, GameRemoteConfig,
                ISingularService, SingularService, NullSingularService,
                IPlatformLinkView, PlatformLinkPresenter

  Meta/         EnvironmentData, RestorableObjectData, WorldData   (already here)
                MetaProgressionService                              (moved from Services/)
                IObjectRestoredView, ObjectRestoredPresenter, ObjectRestoredView  (moved from Popup/)

  Progression/  ProgressionService, GameService, GameSessionService, GameOutcome

  Shop/         IShopView, ShopPresenter, ShopView

  LevelFlow/    ILevelCompleteView, LevelCompletePresenter, LevelCompleteView,
                ILevelFailedView, LevelFailedPresenter, LevelFailedView, LevelFailedChoice

  ConfirmDialog/ IConfirmDialogView, ConfirmDialogPresenter, ConfirmDialogView

  Popup/        UnityViewContainer  (infrastructure only; all feature popups moved out)

  Boot/         (unchanged)
  InGame/       (unchanged)
  MainMenu/     (unchanged)
  Settings/     (unchanged)
  Puzzle/       (unchanged)
```

## Scope

### In Scope

- Move all `.cs` files from `Services/` and `Popup/` into feature folders using `git mv`
- Remove empty folders + orphaned `.meta` files (K008)
- Verify tests pass after each slice

### Out of Scope / Non-Goals

- Namespace changes
- Assembly restructuring
- Moving files in `Boot/`, `InGame/`, `MainMenu/`, `Settings/`, `Puzzle/`
- Any behavioural change
- Moving editor scripts in `Assets/Editor/`
- Moving test files in `Assets/Tests/`

## Technical Constraints

- Every file move must use `git mv` (D025, K008)
- Empty folder cleanup in same commit as the move that empties it (K008)
- Tests must pass after every slice
- No `using` statement changes (namespaces unchanged)
- No scene or asset file edits (GUIDs preserved, namespaces unchanged)
