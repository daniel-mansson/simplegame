# Project

## What This Is

A Unity 6 (6000.3.10f1) mobile puzzle game called **Puzzle Tap** — a cozy, feel-good jigsaw puzzle game where players tap to place pieces, earn golden puzzle pieces, and restore a charming illustrated world. Built on a clean MVP-based UI architecture with screen management, popups, transitions, and domain services.

## Core Value

A complete game flow skeleton — main screen with meta world, stub gameplay with hearts, level progression, golden piece economy, object restoration, environment unlocking, and ad/IAP stub popups — all wired end-to-end so future milestones can focus on real gameplay, art, and monetization one at a time.

## Current State

**M007 S03 complete — M007 mechanically complete, human UAT pending.** All 3 remaining `FindFirstObjectByType` calls replaced with `FindSceneController<T>(current.Value.ToString())` private static helper using scene root convention (`SceneManager.GetSceneByName()` + `scene.IsValid()` + `GetRootGameObjects()` + `GetComponent<T>()`). Zero `FindObject*` variants anywhere in Assets/. 169/169 EditMode tests pass. Human UAT (R077: full play-through MainMenu→InGame→Win→MainMenu, InGame→Lose→Retry→Win) is the only remaining gate before M007 closes.

**M001–M006 complete.** MVP pattern, screen management, popup system, transitions, input blocking, assembly separation, SceneController architecture, boot-from-any-scene, full game loop, LitMotion prefab transitions, meta world with restoration + economy — all proven and tested.

**M007 complete (pending UAT).** All popup views are prefabs under `UnityViewContainer` in Boot scene. `IViewResolver` in Core with `Get<T>()`, implemented by container. Scene controllers receive `IViewResolver` via `Initialize()`. `GameBootstrapper` has `[SerializeField]` refs for all boot infrastructure. Scene controllers found via scene root convention. Zero `FindObject*` in production code. 169 edit-mode tests pass.

**Test count:** 169 edit-mode tests passing across Core and Game assemblies.

## Architecture / Key Patterns

- **MVP pattern**: Views (MonoBehaviour/uGUI) expose interfaces → Presenters (plain C#) consume view interfaces and domain services → Models/Services encapsulate domain logic
- **Assembly separation**: `SimpleGame.Core` (game-agnostic framework) and `SimpleGame.Game` (game-specific code) are separate asmdefs; Core has no game-specific references
- **Generic managers**: `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` where `T : struct, System.Enum`
- **Feature cohesion**: Game code grouped by feature (`Game/MainMenu/`, `Game/Settings/`, `Game/Popup/`, `Game/InGame/`, `Game/Boot/`, `Game/Services/`, `Game/Meta/`)
- **View independence**: Views have no references to presenters, models, or services
- **Explicit DI**: Constructor or Init method injection only. No DI framework, no static state, no singletons.
- **Central UI Factory**: `UIFactory` in `Game/Boot/` constructs all game presenters
- **SceneController pattern**: Per-scene MonoBehaviour with `RunAsync()` — loops internally, returns `ScreenId` for navigation. GameBootstrapper drives the navigation loop.
- **Hybrid scene management**: Persistent Boot scene with additive scene loading for screen scenes (MainMenu, Settings, InGame)
- **UniTask**: All async operations use UniTask
- **Popup pre-instantiation**: `UnityViewContainer` shows/hides pre-instantiated popup GameObjects in Boot scene via `SetActive`; exposes `IViewResolver.Get<T>()` for view resolution without scene scanning
- **Presenter results**: Presenters expose awaitable result methods (`WaitForAction`, `WaitForBack`, `WaitForConfirmation`, `WaitForContinue`, `WaitForChoice`) — no outbound callbacks
- **Boot injection**: `BootInjector` `[RuntimeInitializeOnLoadMethod]` loads Boot additively if not present (play-from-any-scene)
- **Service-mediated context**: `GameSessionService` passes context between scenes; controllers read what they need
- **Prefab transitions**: `UnityTransitionPlayer` uses LitMotion `BindToAlpha`/`ToUniTask()` on a self-contained prefab
- **ScriptableObject data**: WorldData/EnvironmentData/RestorableObjectData define meta-world structure
- **Interface-backed persistence**: IMetaSaveService with PlayerPrefs JSON implementation; reload-then-merge pattern for multi-service shared persistence
- **Golden piece economy**: GoldenPieceService (earn/spend/persist) + HeartService (per-level, 3 hearts)
- **6 popup types**: ConfirmDialog, LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored — all in UnityViewContainer
- **IViewResolver**: `IViewResolver` interface in Core (`T Get<T>() where T : class`) implemented by `UnityViewContainer` via `GetComponentInChildren<T>(true)`
- **Auto-resolving presenters**: InGamePresenter auto-resolves Win/Lose based on game state
- **Presenter→view data transfer**: ObjectDisplayData struct decouples presenter from view
- **Testing**: `SimpleGame.Tests.Core` + `SimpleGame.Tests.Game`; 164+ tests passing

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — screen management, popups, transitions, MVP pattern, demo screens
- [x] M002: Assembly Restructure — Core/Game separation, generic managers, feature-cohesive folders
- [x] M003: SceneController Architecture — async control flow, linear scene controllers, boot-from-any-scene
- [x] M004: Game Loop — meta-progression, context passing, InGame scene, win/lose flow, full menu→play→outcome→menu loop
- [x] M005: Prefab-Based Transitions — LitMotion tweening, prefab-driven transition visuals, extensible for future complex transitions
- [x] M006: Puzzle Tap Game Skeleton — all screens, popups, domain services, meta world data model, persistence, ad/IAP stubs, full game flow
- [ ] M007: Prefab-Based View Management — refactor popup wiring to prefab-based IViewResolver, eliminate FindFirstObjectByType
