# Requirements

This file is the explicit capability and coverage contract for the project.

Use it to track what is actively in scope, what has been validated by completed work, what is intentionally deferred, and what is explicitly out of scope.

## Active

### R001 — MVP pattern with strict separation
- Class: core-capability
- Status: active
- Description: Views are MonoBehaviours exposing interfaces. Presenters are plain C# classes. Models include domain services. No layer references a layer it shouldn't.
- Why it matters: Foundation pattern for the entire project — everything builds on this separation being correct.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04, M001/S05, M004/S01-S04
- Validation: M001 — 3 view interfaces + 3 presenters + 3 view MonoBehaviours; M004 — 6 additional view interfaces + 6 presenters + 6 views; 98/98 tests; static guard clean
- Notes: Presenters must not be MonoBehaviours. Views must not know about presenters or models.

### R002 — View independence (no backward refs to systems/services)
- Class: core-capability
- Status: active
- Description: Views function entirely on their own with no references to presenters, models, services, or any system outside their own interface.
- Why it matters: Enables future view preview tool and guarantees testability.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05, M004/S02-S04
- Validation: M001 — 3 MockXxxViewHasNoPresenterReference reflection tests pass; M004 — 6 new views follow same pattern; view MonoBehaviours grep clean
- Notes: This constraint is stricter than typical MVP — views don't even have a SetPresenter method.

### R004 — Central UI factory for presenter construction
- Class: core-capability
- Status: active
- Description: One UIFactory class constructs all presenters. It receives all dependencies at its own construction and passes the correct ones to each presenter.
- Why it matters: Single wiring point — makes dependency flow explicit and traceable.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05, M004/S02-S04
- Validation: M001 — UIFactory 3 Create methods; M004 — UIFactory extended with CreateInGamePresenter, CreateWinDialogPresenter, CreateLoseDialogPresenter; 98/98 tests pass
- Notes: M004 will extend UIFactory with new Create methods for InGame and popup presenters.

### R007 — Model layer with domain services/systems
- Class: core-capability
- Status: active
- Description: The model layer includes domain services/systems that encapsulate business logic. Presenters interact with these services.
- Why it matters: Keeps presenters thin and domain logic reusable.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05, M004/S01
- Validation: M001 — GameService constructed in GameBootstrapper; M004 — GameSessionService + ProgressionService as real domain services; 12 service tests pass
- Notes: M004 adds GameSessionService and ProgressionService as real domain services.

### R008 — Boot scene → main scene initialization flow
- Class: launchability
- Status: active
- Description: A dedicated boot scene handles initialization, then transitions to the first real screen.
- Why it matters: Clean separation of bootstrap vs runtime.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: M001/S02, M004/S05
- Validation: M001 — Boot.unity at index 0; GameBootstrapper wires full chain; M004 — GameBootstrapper handles MainMenu + Settings + InGame

### R009 — Hybrid scene management (persistent + additive scenes)
- Class: core-capability
- Status: active
- Description: One persistent scene stays loaded. Screen scenes are loaded additively.
- Why it matters: Persistent scene holds shared UI. Additive loading gives screen isolation.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S05, M004/S03
- Validation: M001 — Boot persistent; MainMenu/Settings additive; M004 — InGame additive; 4 scenes total

### R010 — Screen navigation between full screens
- Class: primary-user-loop
- Status: active
- Description: Navigate between full-screen views via a screen manager.
- Why it matters: Core user loop — moving between screens is what the app does.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S04, M001/S05, M004/S02, M004/S03
- Validation: M001 — ShowScreenAsync + GoBack + presenter lifecycle; M004 — InGame navigation + full loop; 98/98 tests

### R014 — UniTask async/await for async operations
- Class: constraint
- Status: active
- Description: All asynchronous operations use UniTask with proper CancellationToken support.
- Why it matters: Zero-allocation async, clean cancellation, native Unity integration.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04, M004/S03-S04
- Validation: M001-M003 — UniTask in all async paths; 58/58 tests pass

### R016 — Demo screens proving end-to-end dependency flow
- Class: launchability
- Status: active
- Description: Example screens with working navigation, popups, and transitions proving the architecture works.
- Why it matters: The demo is the proof.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: M004/S05
- Validation: M001 — 3 scenes; full chain; M004 — 4 scenes with working game loop; 98/98 tests
- Notes: M004 upgrades the demo from navigation showcase to working game loop.




### R069 — Popup views are individual prefabs
- Class: core-capability
- Status: validated
- Description: Each popup view (ConfirmDialog, LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored) exists as its own prefab rather than being placed directly in a scene.
- Why it matters: Prefab-based organization is the standard Unity pattern — enables reuse, clean hierarchy, and future instantiation-based management.
- Source: user
- Primary owning slice: M007/S01
- Supporting slices: none
- Validation: M007/S01 — All 6 popup views held as inactive children under UnityViewContainer in Boot scene. Container resolves all 6 interfaces via GetComponentInChildren<T>(true). Proven by 5 ViewContainerTests.
- Notes: Popup prefabs held as inactive children under the container in Boot scene.

### R070 — Generic view resolver interface (IViewResolver) in Core
- Class: core-capability
- Status: validated
- Description: A Core interface `IViewResolver` with `T Get<T>() where T : class` enables generic resolution of view interfaces without scene scanning.
- Why it matters: Decouples view resolution from Unity scene queries. Keeps Core game-agnostic.
- Source: user
- Primary owning slice: M007/S01
- Supporting slices: M007/S02
- Validation: M007/S01 — IViewResolver interface in Assets/Scripts/Core/PopupManagement/IViewResolver.cs. Proven by 3 ViewContainerGetTests + 2 MockViewResolverTests.
- Notes: Separate from IPopupContainer — single responsibility.

### R071 — Popup container renamed and implements IViewResolver
- Class: core-capability
- Status: validated
- Description: UnityPopupContainer is renamed to reflect its expanded role and implements both IPopupContainer<PopupId> and IViewResolver.
- Why it matters: Single component handles both popup show/hide and view interface resolution.
- Source: user
- Primary owning slice: M007/S01
- Supporting slices: M007/S02
- Validation: M007/S01 — UnityViewContainer implements IPopupContainer<PopupId> + IViewResolver. rg "UnityPopupContainer" Assets/ → exit 1 (zero matches). GUID preserved via git mv.
- Notes: Renamed to UnityViewContainer per D042.

### R072 — Scene controllers get popup views via IViewResolver
- Class: core-capability
- Status: validated
- Description: Scene controllers receive IViewResolver in their Initialize() method and use Get<T>() to resolve popup view interfaces instead of FindFirstObjectByType.
- Why it matters: Eliminates implicit scene scanning. Makes view dependencies explicit.
- Source: user
- Primary owning slice: M007/S02
- Supporting slices: none
- Validation: Zero FindFirstObjectByType in InGameSceneController.cs and MainMenuSceneController.cs (grep exit 1). IViewResolver field and parameter present in both. 4 LogError signals intact. All 8 test call sites compile with null IViewResolver (SetViewsForTesting overrides take precedence). Verified M007/S02.
- Notes: SetViewsForTesting test seam must still work for test doubles.

### R073 — GameBootstrapper uses SerializeField refs for boot infrastructure
- Class: core-capability
- Status: validated
- Description: GameBootstrapper has [SerializeField] references to UnityInputBlocker, UnityTransitionPlayer, and the popup container — no FindFirstObjectByType for boot infrastructure.
- Why it matters: Explicit wiring, no scene scanning at boot time.
- Source: user
- Primary owning slice: M007/S02
- Supporting slices: none
- Validation: 4 [SerializeField] fields in GameBootstrapper (_worldData + 3 new). rg "FindFirstObjectByType<Unity" Assets/Scripts/Game/Boot/GameBootstrapper.cs → exit 1. Exactly 3 FindFirstObjectByType remaining (scene controller lookups only). 3 WireSerializedField calls in SceneSetup.cs. Verified M007/S02.
- Notes: These are all in the Boot scene, so SerializeField refs are straightforward.

### R077 — Functionally identical behavior end-to-end
- Class: constraint
- Status: active
- Description: The game flow, popup behavior, transitions, and all user-visible interactions remain identical after the refactor.
- Why it matters: This is a structural refactor, not a feature change.
- Source: user
- Primary owning slice: M007/S03
- Supporting slices: M007/S01, M007/S02
- Validation: unmapped — Human UAT required: MainMenu → InGame → Win → MainMenu and InGame → Lose → Retry → Win play-through. Not yet performed.
- Notes: Verified by human UAT play-through. Milestone not fully closed until UAT passes.

## Deferred

### R078 — Popup instantiation from prefabs
- Class: core-capability
- Status: deferred
- Description: Container instantiates popup views from prefab references on demand instead of holding pre-instantiated inactive children.
- Why it matters: Reduces memory footprint and enables dynamic popup loading.
- Source: user (mentioned as future possibility)
- Primary owning slice: none
- Validation: unmapped
- Notes: M007 uses inactive children as stepping stone. Instantiation is a future improvement.

### R060 — Real puzzle board with piece placement
- Class: core-capability
- Status: deferred
- Description: Real puzzle board with piece placement, tray, neighbor validation, camera auto-adjust.
- Why it matters: The actual core gameplay mechanic.
- Source: user
- Primary owning slice: none
- Validation: unmapped
- Notes: M006 provides stub gameplay only. Real mechanics are a future milestone.

### R061 — Real art/illustrations
- Class: quality-attribute
- Status: deferred
- Description: Real art/illustrations for environments, objects, and puzzle images.
- Why it matters: Visual identity of the game.
- Source: user
- Primary owning slice: none
- Validation: unmapped

### R062 — Real ad SDK integration
- Class: integration
- Status: deferred
- Description: Real ad SDK integration (rewarded + interstitial).
- Why it matters: Primary monetization channel.
- Source: user
- Primary owning slice: none
- Validation: unmapped
- Notes: M006 provides UI stubs and integration points.

### R063 — Real IAP integration
- Class: integration
- Status: deferred
- Description: Real IAP integration (hearts, ad removal, powerups, bundles).
- Why it matters: Secondary monetization channel.
- Source: user
- Primary owning slice: none
- Validation: unmapped
- Notes: M006 provides UI stubs and integration points.

### R064 — Powerups and special mechanics
- Class: differentiator
- Status: deferred
- Description: In-level abilities that assist with piece placement or reveal valid pieces.
- Why it matters: Adds depth and monetization options to gameplay.
- Source: GDD
- Primary owning slice: none
- Validation: unmapped

### R065 — Daily challenge mode
- Class: differentiator
- Status: deferred
- Description: Standalone mode with unique rules/constraints, available as premium feature.
- Why it matters: Retention mechanic and premium content.
- Source: GDD
- Primary owning slice: none
- Validation: unmapped

### R066 — New piece types (hex, triangular)
- Class: differentiator
- Status: deferred
- Description: Hexagonal, triangular, and other non-standard puzzle piece shapes.
- Why it matters: Content variety for long-term engagement.
- Source: GDD
- Primary owning slice: none
- Validation: unmapped

### R067 — LiveOps and seasonal events
- Class: differentiator
- Status: deferred
- Description: Seasonal content, limited-time challenges, themed rewards.
- Why it matters: Live service engagement loop.
- Source: GDD
- Primary owning slice: none
- Validation: unmapped

### R068 — Object restoration animations
- Class: quality-attribute
- Status: deferred
- Description: Looping animations when a meta-world object is fully restored.
- Why it matters: Brings the meta world to life — reward for completion.
- Source: GDD
- Primary owning slice: none
- Validation: unmapped

## Validated

### R074 — Scene controllers resolved via scene root convention
- Class: core-capability
- Status: validated
- Description: After additive scene load, the GameBootstrapper finds scene controllers via a scene root convention (e.g. querying root GameObjects of the loaded scene) instead of FindFirstObjectByType.
- Validated by: M007/S03 — FindSceneController<T>(sceneName) private static helper in GameBootstrapper.cs using SceneManager.GetSceneByName() + scene.IsValid() + GetRootGameObjects() + GetComponent<T>(). Three call sites confirmed at lines 99, 113, 126.
- Proof: rg "FindSceneController|GetSceneByName|GetRootGameObjects" GameBootstrapper.cs → 5 matches

### R075 — Zero FindFirstObjectByType calls in production code
- Class: constraint
- Status: validated
- Description: No FindFirstObjectByType, FindObjectOfType, FindObjectsOfType, or FindAnyObjectByType anywhere in Assets/Scripts/.
- Validated by: M007/S03 — rg -g "*.cs" "FindFirstObjectByType|..." Assets/ → exit 1 (zero matches across entire Assets/ tree). 2026-03-17.
- Proof: grep across all .cs files in Assets/ — zero matches

### R076 — All existing tests pass after refactor
- Class: quality-attribute
- Status: validated
- Description: All 164+ edit-mode tests pass in Unity batchmode after the refactor.
- Validated by: M007/S03 — Unity EditMode test run (job ID 766d31f2ff0b434eaad592ac6a7a8796): total=169, passed=169, failed=0, skipped=0.
- Proof: Unity MCP test job; last test: ViewContainerGetTests.Get_ReturnsNull_WhenInterfaceNotFound

### R045 — Main screen shows environment with restorable objects, balance, play, settings
- Class: primary-user-loop
- Status: validated
- Validated by: M006 — MainMenuPresenter shows environment name, objects with progress/blocked/complete, golden piece balance, play button with level, settings. 25 DemoWiringTests pass.
- Proof: MainMenuPresenter + MainMenuView + ObjectDisplayData; DemoWiringTests (25/25 pass)

### R046 — Stub gameplay with hearts and piece counter
- Class: primary-user-loop
- Status: validated
- Validated by: M006 — InGamePresenter has piece counter, PlaceCorrect/PlaceIncorrect, hearts display, auto-win/lose. 14 presenter + 4 controller tests pass.
- Proof: InGamePresenter + InGameView + InGameSceneController; InGameTests (18/18 pass)

### R047 — Meta world data model via ScriptableObjects
- Class: core-capability
- Status: validated
- Validated by: M006 — WorldData → EnvironmentData → RestorableObjectData as SOs with blocked-by. 3 environments, 5+ objects. 18 MetaProgressionService tests pass.
- Proof: Assets/Data/*.asset; MetaProgressionServiceTests (18/18 pass)

### R048 — Golden puzzle pieces earned on level complete, spent on object restoration
- Class: primary-user-loop
- Status: validated
- Validated by: M006 — InGameSceneController earns on win. MainMenuPresenter spends per costPerStep on tap. 15 GoldenPieceService + 25 DemoWiringTests pass.
- Proof: GoldenPieceService + MainMenuPresenter; GoldenPieceServiceTests (15/15) + DemoWiringTests (25/25)

### R049 — Meta progression persists via PlayerPrefs
- Class: continuity
- Status: validated
- Validated by: M006 — IMetaSaveService + PlayerPrefsMetaSaveService with JSON. Reload-then-merge pattern for multi-service persistence. Round-trip tests pass.
- Proof: PlayerPrefsMetaSaveService + MetaSaveData; MetaProgressionServiceTests persistence tests

### R050 — Environment unlocking with 1–3 simultaneous availability
- Class: core-capability
- Status: validated
- Validated by: M006 — GetCurrentEnvironment finds first non-complete. NextEnvironment button shown when complete + hasNext. 3 environments in WorldData.
- Proof: MainMenuSceneController + MainMenuPresenter; environment navigation tested

### R051 — LevelComplete popup with golden piece reward
- Class: primary-user-loop
- Status: validated
- Validated by: M006 — LevelCompletePresenter shows score, level, golden pieces. Continue returns to main. 4 tests pass.
- Proof: LevelCompletePresenter; PopupTests (4/4 LevelComplete pass)

### R052 — LevelFailed popup with retry/ad/quit
- Class: primary-user-loop
- Status: validated
- Validated by: M006 — LevelFailedPresenter offers Retry/WatchAd/Quit. InGameSceneController handles all three. 6 tests pass.
- Proof: LevelFailedPresenter + LevelFailedChoice; PopupTests (6/6 LevelFailed pass)

### R053 — Rewarded ad stub popup
- Class: integration
- Status: validated
- Validated by: M006 — RewardedAdPresenter + RewardedAdView with Watch/Skip, Debug.Log on watch. 5 tests pass.
- Proof: RewardedAdPresenter; PopupTests (5/5 RewardedAd pass)

### R054 — IAP purchase stub popup
- Class: integration
- Status: validated
- Validated by: M006 — IAPPurchasePresenter + IAPPurchaseView with Purchase/Cancel, Debug.Log on purchase. 5 tests pass.
- Proof: IAPPurchasePresenter; PopupTests (5/5 IAPPurchase pass)

### R055 — Object restored celebration popup
- Class: quality-attribute
- Status: validated
- Validated by: M006 — ObjectRestoredPresenter + ObjectRestoredView. MainMenuSceneController shows popup on completion. 4 tests pass.
- Proof: ObjectRestoredPresenter; PopupTests (4/4 ObjectRestored pass)

### R056 — Interstitial ad stub debug log
- Class: integration
- Status: validated
- Validated by: M006 — InGamePresenter logs "[Ads] Interstitial ad opportunity" at both win and lose.
- Proof: InGamePresenter HandlePlaceCorrect/HandlePlaceIncorrect; tested in InGameTests

### R057 — Heart system: 3 per level, incorrect costs 1, 0 = fail
- Class: core-capability
- Status: validated
- Validated by: M006 — HeartService Reset(3)/UseHeart/IsAlive. InGamePresenter auto-loses at 0 hearts. 12 + 14 tests pass.
- Proof: HeartServiceTests (12/12) + InGameTests presenter tests

### R058 — Full navigable flow end-to-end
- Class: launchability
- Status: validated
- Validated by: M006 — GameBootstrapper constructs all services. Full flow wired: MainMenu→InGame→Win/Lose→Popups→MainMenu. SceneSetup creates all scenes.
- Proof: GameBootstrapper + SceneSetup.cs + all SceneControllerTests

### R059 — All views are text-box stubs
- Class: constraint
- Status: validated
- Validated by: M006 — All views use uGUI Text + Button. Dynamic object buttons in MainMenuView. No art, no animations.
- Proof: All *View.cs files use UnityEngine.UI.Text and UnityEngine.UI.Button only

### R003 — Interface-per-view for presenter dependency
- Class: core-capability
- Status: validated
- Validated by: M001 — ISampleView + SamplePresenter (S01); IMainMenuView, ISettingsView, IConfirmDialogView + corresponding presenters (S05); 3 mock-view reflection tests pass
- Proof: TestResults.xml total="49" passed="49" failed="0"

### R005 — Constructor/init injection only (no DI framework)
- Class: constraint
- Status: validated
- Validated by: M001 — all 49 tests pass via constructor injection; no DI framework; pattern consistent across all slices
- Proof: TestResults.xml total="49" passed="49" failed="0"; no DI framework in manifest.json

### R006 — No static state (domain reload disabled support)
- Class: constraint
- Status: validated
- Validated by: M001 — static guard grep returns empty across all slices
- Proof: grep command returns no output on final project state

### R011 — Stack-based popup system
- Class: core-capability
- Status: validated
- Validated by: S03 — PopupManager with Stack<PopupId>; 5 dedicated tests pass
- Proof: TestResults.xml total="27" passed="27" failed="0"

### R012 — Full-screen raycast input blocker
- Class: core-capability
- Status: validated
- Validated by: S03 — IInputBlocker reference-counting; UnityInputBlocker CanvasGroup; 6 tests pass
- Proof: TestResults.xml total="27" passed="27" failed="0"

### R013 — Fade transitions between screens
- Class: quality-attribute
- Status: validated
- Validated by: S04 — ITransitionPlayer + ScreenManager orchestration; 5 transition tests; UnityTransitionPlayer CanvasGroup alpha
- Proof: TestResults.xml total="32" passed="32" failed="0"

### R015 — Edit-mode unit tests for presenters and core logic
- Class: quality-attribute
- Status: validated
- Validated by: M001-M003 — 58/58 edit-mode tests pass in Unity batchmode
- Proof: TestResults.xml result="Passed" total="58" passed="58" failed="0"

### R017 — Each layer testable in isolation
- Class: quality-attribute
- Status: validated
- Validated by: M001 — 8 mock test doubles in pure C#; all tests run without Unity runtime; Core grep clean
- Proof: TestResults.xml total="49" passed="49" failed="0"

### R023 — Core assembly is game-agnostic
- Class: constraint
- Status: validated
- Validated by: M002 — grep guards pass; no game types in Core
- Proof: grep returns empty for game-specific types in Core

### R024 — Game code in dedicated assembly
- Class: constraint
- Status: validated
- Validated by: M002 — SimpleGame.Game.asmdef references Core; Core does not reference Game
- Proof: One-way dependency confirmed in asmdef files

### R025 — Feature cohesion within Game
- Class: quality-attribute
- Status: validated
- Validated by: M002 — each feature folder contains interface + presenter + view
- Proof: Game/MainMenu/, Game/Settings/, Game/Popup/ all co-located

### R026 — Test assemblies mirror source structure
- Class: quality-attribute
- Status: validated
- Validated by: M002 — SimpleGame.Tests.Core (32) + SimpleGame.Tests.Game (26); ISampleView/SamplePresenter in Core test assembly
- Proof: TestResults.xml confirms both assemblies

### R027 — All existing edit-mode tests pass after restructure
- Class: quality-attribute
- Status: validated
- Validated by: M002-M003 — 58/58 tests pass
- Proof: TestResults.xml result="Passed" total="58" passed="58" failed="0"

### R028 — ScreenManager and PopupManager are generic
- Class: core-capability
- Status: validated
- Validated by: M002 — ScreenManager<TScreenId>/PopupManager<TPopupId> where T : struct, System.Enum
- Proof: Core tests use TestScreenId/TestPopupId; Game uses ScreenId/PopupId

### R029 — Game session context via shared service
- Class: core-capability
- Status: validated
- Validated by: M004 — GameSessionService holds level/score/outcome; 7 edit-mode tests pass; InGameSceneController reads from it; MainMenuPresenter writes to it
- Proof: TestResults.xml testcasecount="98" passed="98" — GameSessionServiceTests (7/7 pass)

### R030 — Progression service with in-memory level tracking
- Class: core-capability
- Status: validated
- Validated by: M004 — ProgressionService tracks level, advances on win, logs score; 5 edit-mode tests pass
- Proof: TestResults.xml — ProgressionServiceTests (5/5 pass); Debug.Log "[ProgressionService] Level N complete"

### R031 — Main menu displays current level and has Play button
- Class: primary-user-loop
- Status: validated
- Validated by: M004 — MainMenuPresenter reads CurrentLevel, displays "Level N", Play sets session context; DemoWiringTests prove wiring
- Proof: TestResults.xml — DemoWiringTests (25/25 pass)

### R032 — InGame scene receives level ID and is self-sufficient
- Class: core-capability
- Status: validated
- Validated by: M004 — InGameSceneController reads level from GameSessionService; fallback via _defaultLevelId; 4 controller tests pass
- Proof: TestResults.xml — InGameSceneControllerTests (4/4 pass)

### R033 — InGame gameplay — score counter + win/lose triggers
- Class: primary-user-loop
- Status: validated
- Validated by: M004 — InGamePresenter has score increment, win, lose actions; 10 edit-mode tests pass
- Proof: TestResults.xml — InGamePresenterTests (10/10 pass)

### R034 — Win popup with score + level, returns to main menu
- Class: primary-user-loop
- Status: validated
- Validated by: M004 — WinDialogPresenter shows score + level, WaitForContinue resolves on click; 4 tests pass
- Proof: TestResults.xml — WinDialogPresenterTests (4/4 pass)

### R035 — Lose popup with score + level, retry/back options
- Class: primary-user-loop
- Status: validated
- Validated by: M004 — LoseDialogPresenter Retry/Back flow proven by 5 tests; InGameSceneController retry creates fresh presenter
- Proof: TestResults.xml — LoseDialogPresenterTests (5/5 pass)

### R036 — Progression service logs score on win and advances level
- Class: core-capability
- Status: validated
- Validated by: M004 — InGameSceneController calls RegisterWin(score); ProgressionService logs and advances level; tested in both service and controller tests
- Proof: TestResults.xml — ProgressionServiceTests (5/5) + InGameSceneControllerTests (4/4)

### R037 — Play-from-editor bootstrapping for InGame scene
- Class: launchability
- Status: validated
- Validated by: M004 — InGameSceneController has serialized _defaultLevelId; BootInjector loads Boot if missing; tested in InGameSceneControllerTests
- Proof: TestResults.xml — InGameSceneControllerTests (4/4 pass)

### R038 — Full game loop — menu → play → outcome → menu reflects progress
- Class: primary-user-loop
- Status: validated
- Validated by: M004 — GameBootstrapper handles ScreenId.InGame; full loop wired; 98/98 tests; play-mode UAT confirmed
- Proof: TestResults.xml testcasecount="98" passed="98"; play-mode UAT

### R039 — New popup types (WinDialog, LoseDialog) with distinct views/presenters
- Class: core-capability
- Status: validated
- Validated by: M004 — WinDialog and LoseDialog are PopupId entries with own view interfaces, views, and presenters; UnityPopupContainer wires both; 9 popup tests pass
- Proof: TestResults.xml — WinDialogPresenterTests (4/4) + LoseDialogPresenterTests (5/5)

### R040 — Edit-mode tests for new presenters, services, and scene controllers
- Class: quality-attribute
- Status: validated
- Validated by: M004 — 98/98 edit-mode tests pass; all new types covered
- Proof: TestResults.xml testcasecount="98" passed="98" failed="0"

## Out of Scope

### R020 — DI framework integration
- Class: anti-feature
- Status: out-of-scope
- Description: No Zenject, VContainer, or other DI framework.
- Why it matters: Prevents framework coupling. Explicit manual wiring only.
- Source: user
- Validation: n/a

### R021 — UI Toolkit / UXML views
- Class: constraint
- Status: out-of-scope
- Description: Views use legacy uGUI, not UI Toolkit.
- Why it matters: Prevents scope confusion.
- Source: user
- Validation: n/a

### R022 — Static singletons or static state patterns
- Class: anti-feature
- Status: out-of-scope
- Description: No static fields holding state. No singleton pattern.
- Why it matters: Project must support domain-reload-disabled mode.
- Source: user
- Validation: n/a

### R043 — Real gameplay mechanics beyond score button
- Class: anti-feature
- Status: out-of-scope
- Description: No physics, enemies, timers, or complex gameplay beyond stub buttons.
- Why it matters: The milestone proves the game flow, not gameplay depth.
- Source: inferred
- Validation: n/a

## Traceability

| ID | Class | Status | Primary owner | Supporting | Proof |
|---|---|---|---|---|---|
| R001 | core-capability | active | M001/S01 | M001/S02-S05, M004/S01-S04 | M001 verified; M004 extends |
| R002 | core-capability | active | M001/S01 | M001/S05, M004/S02-S04 | M001 verified; M004 extends |
| R003 | core-capability | validated | M001/S01 | M001/S05 | validated |
| R004 | core-capability | active | M001/S01 | M001/S05, M004/S02-S04 | M001 verified; M004 extends UIFactory |
| R005 | constraint | validated | M001/S01 | all | validated |
| R006 | constraint | validated | M001/S01 | all | validated |
| R007 | core-capability | active | M001/S01 | M001/S05, M004/S01 | M001 verified; M004 adds real services |
| R008 | launchability | active | M001/S05 | M001/S02, M004/S05 | M001 verified; M004 extends boot |
| R009 | core-capability | active | M001/S02 | M001/S05, M004/S03 | M001 verified; M004 adds InGame scene |
| R010 | primary-user-loop | active | M001/S02 | M001/S04-S05, M004/S02-S03 | M001 verified; M004 extends nav |
| R011 | core-capability | validated | M001/S03 | M001/S05 | validated |
| R012 | core-capability | validated | M001/S03 | M001/S04 | validated |
| R013 | quality-attribute | validated | M001/S04 | M001/S05 | validated |
| R014 | constraint | active | M001/S01 | M001/S02-S04, M004/S03-S04 | M001-M003 verified; M004 extends |
| R015 | quality-attribute | validated | M001/S01 | M001/S02-S03 | validated |
| R016 | launchability | active | M001/S05 | M004/S05 | M001 verified; M004 upgrades demo |
| R017 | quality-attribute | validated | M001/S01 | M001/S02-S03 | validated |
| R018 | differentiator | deferred | none | none | unmapped |
| R019 | quality-attribute | deferred | none | none | unmapped |
| R020 | anti-feature | out-of-scope | none | none | n/a |
| R021 | constraint | out-of-scope | none | none | n/a |
| R022 | anti-feature | out-of-scope | none | none | n/a |
| R023 | constraint | validated | M002/S01 | M002/S02 | validated |
| R024 | constraint | validated | M002/S02 | none | validated |
| R025 | quality-attribute | validated | M002/S02 | none | validated |
| R026 | quality-attribute | validated | M002/S03 | none | validated |
| R027 | quality-attribute | validated | M002/S03 | M002/S01-S02 | validated |
| R028 | core-capability | validated | M002/S01 | none | validated |
| R029 | core-capability | validated | M004/S01 | M004/S02-S04 | validated M004 |
| R030 | core-capability | validated | M004/S01 | M004/S02-S03 | validated M004 |
| R031 | primary-user-loop | validated | M004/S02 | none | validated M004 |
| R032 | core-capability | validated | M004/S03 | none | validated M004 |
| R033 | primary-user-loop | validated | M004/S03 | none | validated M004 |
| R034 | primary-user-loop | validated | M004/S04 | M004/S03 | validated M004 |
| R035 | primary-user-loop | validated | M004/S04 | M004/S03 | validated M004 |
| R036 | core-capability | validated | M004/S03 | M004/S01 | validated M004 |
| R037 | launchability | validated | M004/S03 | none | validated M004 |
| R038 | primary-user-loop | validated | M004/S05 | M004/S01-S04 | validated M004 |
| R039 | core-capability | validated | M004/S04 | none | validated M004 |
| R040 | quality-attribute | validated | M004/S01 | M004/S02-S05 | validated M004 |
| R041 | continuity | deferred | none | none | unmapped |
| R042 | core-capability | deferred | none | none | unmapped |
| R043 | anti-feature | out-of-scope | none | none | n/a |
| R044 | quality-attribute | validated | M005/S01 | none | validated |
| R045 | primary-user-loop | validated | M006/S05 | M006/S06 | validated M006 |
| R046 | primary-user-loop | validated | M006/S03 | M006/S04 | validated M006 |
| R047 | core-capability | validated | M006/S01 | M006/S05, M006/S06 | validated M006 |
| R048 | primary-user-loop | validated | M006/S02 | M006/S04, M006/S05 | validated M006 |
| R049 | continuity | validated | M006/S01 | M006/S02 | validated M006 |
| R050 | core-capability | validated | M006/S06 | M006/S01 | validated M006 |
| R051 | primary-user-loop | validated | M006/S04 | none | validated M006 |
| R052 | primary-user-loop | validated | M006/S04 | none | validated M006 |
| R053 | integration | validated | M006/S04 | none | validated M006 |
| R054 | integration | validated | M006/S04 | none | validated M006 |
| R055 | quality-attribute | validated | M006/S05 | none | validated M006 |
| R056 | integration | validated | M006/S03 | none | validated M006 |
| R057 | core-capability | validated | M006/S02 | M006/S03 | validated M006 |
| R058 | launchability | validated | M006/S06 | M006/S01-S05 | validated M006 |
| R059 | constraint | validated | M006/S03 | M006/S04, M006/S05 | validated M006 |
| R060 | core-capability | deferred | none | none | unmapped |
| R061 | quality-attribute | deferred | none | none | unmapped |
| R062 | integration | deferred | none | none | unmapped |
| R063 | integration | deferred | none | none | unmapped |
| R064 | differentiator | deferred | none | none | unmapped |
| R065 | differentiator | deferred | none | none | unmapped |
| R066 | differentiator | deferred | none | none | unmapped |
| R067 | differentiator | deferred | none | none | unmapped |
| R068 | quality-attribute | deferred | none | none | unmapped |
| R069 | core-capability | active | M007/S01 | none | mapped |
| R070 | core-capability | active | M007/S01 | M007/S02 | mapped |
| R071 | core-capability | active | M007/S01 | M007/S02 | mapped |
| R072 | core-capability | validated | M007/S02 | none | mapped |
| R073 | core-capability | validated | M007/S02 | none | mapped |
| R074 | core-capability | active | M007/S03 | none | mapped |
| R075 | constraint | active | M007/S03 | M007/S01, M007/S02 | mapped |
| R076 | quality-attribute | active | M007/S03 | M007/S01, M007/S02 | mapped |
| R077 | constraint | active | M007/S03 | M007/S01, M007/S02 | mapped |
| R078 | core-capability | deferred | none | none | unmapped |

## Coverage Summary

- Total requirements: 79
- Active: 18
- Validated: 42
- Deferred: 14
- Out of scope: 4
- Unmapped active requirements: 0
- Note: Some validated/deferred requirements (R018, R019, R041, R042, R044) have traceability table entries only — no full body section.
