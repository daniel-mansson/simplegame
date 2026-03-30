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
- Validation: Pending human UAT play-through: MainMenu → InGame → Win → MainMenu and InGame → Lose → Retry → Win. Boot scene regenerated 2026-03-18 to wire SerializeField refs — LevelFailed popup null resolver bug fixed. Mechanical criteria all met.
- Notes: Verified by human UAT play-through. Milestone not fully closed until UAT passes.

### R079 — Animated popup blocker overlay
- Class: primary-user-loop
- Status: validated
- Description: A visible dim overlay (semi-transparent dark backdrop) sits below the active popup in z-order. Fades in when a popup opens, fades out when a popup closes. Single layer that repositions to track the top popup.
- Why it matters: Polish essential to game feel — instant show/hide feels jarring.
- Source: user
- Primary owning slice: M008/S01
- Supporting slices: none
- Validation: validated
- Notes: UnityInputBlocker.FadeInAsync/FadeOutAsync with LitMotion. Input blocking activates at fade-in start; input unblocking activates at fade-out start via FadeOutAsync.Forget() before HidePopupAsync.

### R080 — Input block/unblock timing split
- Class: quality-attribute
- Status: validated
- Description: Input is blocked the moment the blocker starts fading in. Input is unblocked the moment the blocker starts fading out — not when the fade completes.
- Why it matters: Ensures responsive feel on dismiss while preventing accidental input during opening.
- Source: user
- Primary owning slice: M008/S01
- Supporting slices: none
- Validation: validated
- Notes: Implemented in PopupManager.DismissPopupAsync: Unblock() + FadeOutAsync.Forget() before awaiting HidePopupAsync.

### R081 — Popup animate-in/out contract on IPopupView
- Class: core-capability
- Status: validated
- Description: IPopupView exposes AnimateInAsync and AnimateOutAsync. PopupViewBase provides default LitMotion implementations. All popup views inherit from PopupViewBase.
- Why it matters: Allows per-popup animation overrides while providing a consistent default.
- Source: user
- Primary owning slice: M008/S01
- Supporting slices: M008/S03
- Validation: validated
- Notes: Default: bounce-up in (Y offset -80px + OutBounce 0.4s), scale+fade out (scale 0.85 + alpha 0, InBack 0.25s). All 6 views inherit PopupViewBase with _canvasGroup/_panel wired.

### R082 — TMP-based UI prefab kit
- Class: differentiator
- Status: validated
- Description: Prefab assets: BigPopupWindow, SmallPopupWindow, PositiveButton, DestructiveButton, NeutralButton, TitleText, BodyText, ButtonLabel — all TextMeshPro-based, ready to reskin.
- Why it matters: Reusable component kit that makes visual iteration fast and consistent across all popups.
- Source: user
- Primary owning slice: M008/S02
- Supporting slices: M008/S03
- Validation: validated
- Notes: 8 prefab assets in Assets/Prefabs/UI/. Visual polish deferred. PrefabKitSetup.cs regenerates them.

### R083 — All existing popups use prefab components
- Class: core-capability
- Status: validated
- Description: All 6 popup GameObjects (ConfirmDialog, LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored) are rebuilt in SceneSetup using BigPopup/SmallPopup window shells and prefab buttons/text.
- Why it matters: Unifies popup visual language; enables reskinning via prefab changes.
- Source: user
- Primary owning slice: M008/S03
- Supporting slices: M008/S02
- Validation: validated
- Notes: SceneSetup fully regenerates Boot scene. TMP_Text fields wired. _canvasGroup/_panel wired on all 6 views.

### R084 — In-scene screen switching within a scene
- Class: core-capability
- Status: active
- Description: Scenes can contain multiple named screens (full-panel GameObjects). An `InSceneScreenManager` switches the active screen by SetActive, maintaining a back stack. One screen is active at a time.
- Why it matters: Enables shop, game modes, and other full-panel features without scene load overhead. Scales better than a flat scene with everything always loaded.
- Source: user
- Primary owning slice: M009/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Generic in Core (no game types). MainMenu wires it with MainMenuScreenId (Home, Shop). Instant swap — no animation this milestone.

### R085 — Popup stacking with correct visual layering
- Class: core-capability
- Status: active
- Description: Multiple popups can be shown simultaneously. Only the top popup is interactive. The dim overlay sits between the bottom and top popup: bottom popup is visually dimmed, top popup is unobscured.
- Why it matters: Enables multi-step flows (LevelFailed → shop) without losing the first popup's state.
- Source: user
- Primary owning slice: M009/S02
- Supporting slices: M009/S03
- Validation: unmapped
- Notes: Implemented via per-popup Canvas with OverrideSorting. Sort scheme: base 50, +100 per depth. Blocker at 100.

### R086 — Coins currency, persisted, separate from golden pieces
- Class: primary-user-loop
- Status: active
- Description: `CoinsService` tracks a coin balance. Coins are earned via shop purchases. Balance persists via MetaSaveData. Separate from the existing golden pieces currency.
- Why it matters: Coins drive the Continue monetization flow. Needs to survive session restarts.
- Source: user
- Primary owning slice: M009/S03
- Supporting slices: none
- Validation: unmapped
- Notes: `MetaSaveData.coins` int field added. CoinsService follows GoldenPieceService pattern exactly.

### R087 — LevelFailed Continue option costing 100 coins
- Class: primary-user-loop
- Status: active
- Description: LevelFailed popup has a Continue button. Costs 100 coins. If balance sufficient: deducts and continues. If insufficient: opens Shop popup stacked on LevelFailed.
- Why it matters: Primary monetization touchpoint. Demonstrates popup stacking and coin spending in the critical fail flow.
- Source: user
- Primary owning slice: M009/S03
- Supporting slices: M009/S02
- Validation: unmapped
- Notes: `LevelFailedChoice.Continue` added. LevelFailed popup gets a fourth button.

### R088 — Shop with fake IAP coin packs
- Class: primary-user-loop
- Status: active
- Description: Shop has three coin pack tiers (e.g. 500/€1.99, 1200/€3.99, 2500/€7.99). Each uses fake IAP flow (existing IAPPurchasePresenter pattern). Purchase grants coins. Accessible from MainMenu (screen) and from LevelFailed (stacked popup).
- Why it matters: Proves the full monetization flow end-to-end, even with a stub payment.
- Source: user
- Primary owning slice: M009/S03
- Supporting slices: M009/S01, M009/S02
- Validation: unmapped
- Notes: No real store SDK. IAPPurchasePresenter extended or ShopPresenter wraps it for multi-pack selection.

### R089 — Contextual coin balance overlay HUD
- Class: primary-user-loop
- Status: active
- Description: An overlay HUD canvas (sort order between blocker and popups) shows the current coin balance. Appears and disappears with LitMotion animation when explicitly shown/hidden by the current context (e.g. shown when LevelFailed opens, hidden after dismiss).
- Why it matters: Makes coin balance visible at moments of spending/earning without cluttering persistent UI.
- Source: user
- Primary owning slice: M009/S04
- Supporting slices: M009/S03
- Validation: unmapped
- Notes: `ICurrencyOverlay` interface. `UnityCurrencyOverlay` MonoBehaviour. Canvas sort order 120 (between blocker 100 and popups 150+).

### R091 — Pure puzzle domain model with no Unity dependencies
- Class: core-capability
- Status: active
- Description: `SimpleGame.Puzzle` assembly contains all puzzle domain types — `IPuzzlePiece`, `IPuzzleBoard`, `IDeck`, `IPuzzleLevel`, `PuzzleSession` — as plain C# with zero Unity references. `noEngineReferences: true` enforced via asmdef.
- Why it matters: Enables EditMode testing without Play mode, keeps domain logic portable, and enforces that game rules are not entangled with rendering.
- Source: user
- Primary owning slice: M011/S01
- Supporting slices: M011/S02, M011/S03
- Validation: unmapped
- Notes: Any type in `SimpleGame.Puzzle` that imports `UnityEngine` or `UnityEditor` is a build error.

### R092 — Placement rule: neighbor-presence validation
- Class: primary-user-loop
- Status: active
- Description: A piece can be placed on the board if and only if at least one of its declared neighbors is already placed on the board (seed pieces are pre-placed and satisfy this for adjacent pieces). Attempting to place an invalid piece returns false and does not mutate board state.
- Why it matters: This is the core game rule — it defines what "correct" and "incorrect" mean.
- Source: user
- Primary owning slice: M011/S01
- Supporting slices: M011/S03
- Validation: unmapped
- Notes: Board shape agnostic — neighbor relationships are declared in piece data, not derived from grid geometry.

### R093 — Deck abstraction: ordered piece sequence
- Class: primary-user-loop
- Status: active
- Description: A `Deck` is an ordered sequence of piece IDs. The player always sees the piece at the front of the deck. On correct placement, the deck advances. Deck ordering is the primary level design lever.
- Why it matters: The order of the deck determines how the puzzle unfolds — it is the craft of level design.
- Source: user
- Primary owning slice: M011/S01
- Supporting slices: M011/S02, M011/S03
- Validation: unmapped
- Notes: Deck is an ordered set — no duplicate piece IDs.

### R094 — Configurable deck layout: one-per-slot or shared
- Class: primary-user-loop
- Status: active
- Description: Level definition supports either a single shared deck (one ordered sequence drawn from by all slots) or one deck per slot. The game model is agnostic to which mode is active.
- Why it matters: Different deck layouts create fundamentally different puzzle-solving experiences.
- Source: user
- Primary owning slice: M011/S01
- Supporting slices: M011/S02
- Validation: unmapped
- Notes: Slot count and deck assignment are part of the level definition.

### R095 — Jigsaw adapter hides SimpleJigsaw types from game code
- Class: core-capability
- Status: active
- Description: A `JigsawLevelFactory` (in `SimpleGame.Game`) converts `SimpleJigsaw.PuzzleBoard` into an `IPuzzleLevel`. No `SimpleJigsaw.*` type is visible to `InGamePresenter`, `InGameSceneController`, or any `SimpleGame.Puzzle` type.
- Why it matters: The jigsaw package is explicitly expected to change. The adapter is the only place that must change when it does.
- Source: user
- Primary owning slice: M011/S02
- Supporting slices: M011/S03, M011/S04
- Validation: unmapped
- Notes: `SimpleGame.Puzzle` asmdef must NOT reference `SimpleJigsaw`. Only `SimpleGame.Game` (or a dedicated adapter assembly) may reference it.

### R096 — InGame wired to PuzzleSession (tap → model → correct/incorrect)
- Class: primary-user-loop
- Status: active
- Description: `InGamePresenter` delegates placement decisions to `PuzzleSession.TryPlace(pieceId)`. Raw `OnPlaceCorrect`/`OnPlaceIncorrect` view events are removed; the view exposes `OnTapPiece(int pieceId)` instead. Model owns the correct/incorrect determination.
- Why it matters: The current stub treats every tap as pre-judged by the view. Real gameplay requires the model to be the source of truth.
- Source: user
- Primary owning slice: M011/S03
- Supporting slices: M011/S04
- Validation: unmapped
- Notes: PuzzleSession fires an event or returns a result; InGamePresenter reacts and updates view (hearts, counter). Win when all non-seed pieces placed; lose when hearts reach 0.

### R097 — Jigsaw pieces rendered as tappable GameObjects in InGame scene
- Class: primary-user-loop
- Status: active
- Description: InGame scene renders jigsaw piece meshes using `PieceObjectFactory`. Each piece GameObject has a thin tap-handler component that calls `IInGameView.OnTapPiece(pieceId)`. No game logic lives on piece GameObjects.
- Why it matters: The player needs to see and interact with real puzzle pieces, not placeholder buttons.
- Source: user
- Primary owning slice: M011/S04
- Supporting slices: none
- Validation: unmapped
- Notes: Tap handler is a MonoBehaviour with a single responsibility: forward the tap with piece ID to the view. `PieceDragger` from simple-jigsaw is not used.

## Deferred

### R098 — Drag-and-drop piece placement UX
- Class: primary-user-loop
- Status: deferred
- Description: Player can drag a piece from the deck and drop it onto the board to attempt placement. The model rules are identical to tap placement.
- Why it matters: UX improvement — drag is more intuitive for some players.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Explicitly deferred by user. Tap-only is the target for M011. Drag-and-drop does not change the model.

### R099 — Camera auto-adjust to puzzle size
- Class: quality-attribute
- Status: deferred
- Description: Camera automatically frames the puzzle board based on its bounding box when InGame loads.
- Why it matters: Puzzles of different sizes/shapes need the camera to fit them without manual Inspector tuning.
- Source: inferred
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Deferred — fixed camera position is acceptable for M011 since all demo puzzles will be similar size.

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
- Status: active
- Description: Real puzzle board with piece placement, neighbor validation, and tap-driven gameplay wired in InGame.
- Why it matters: The actual core gameplay mechanic.
- Source: user
- Primary owning slice: M011/S03
- Supporting slices: M011/S01, M011/S02, M011/S04
- Validation: unmapped
- Notes: Promoted from deferred by M011. Camera auto-adjust deferred to R099. "Tray" concept replaced by Deck (R093).

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
| R079 | primary-user-loop | validated | M008/S01 | none | validated |
| R080 | quality-attribute | validated | M008/S01 | none | validated |
| R081 | core-capability | validated | M008/S01 | M008/S03 | validated |
| R082 | differentiator | validated | M008/S02 | M008/S03 | validated |
| R083 | core-capability | validated | M008/S03 | M008/S02 | validated |
| R084 | core-capability | validated | M009/S01 | none | validated |
| R085 | core-capability | validated | M009/S02 | M009/S03 | validated |
| R086 | primary-user-loop | validated | M009/S03 | none | validated |
| R087 | primary-user-loop | validated | M009/S03 | M009/S02 | validated |
| R088 | primary-user-loop | validated | M009/S03 | M009/S01, M009/S02 | validated |
| R089 | primary-user-loop | validated | M009/S04 | M009/S03 | validated |
| R091 | core-capability | active | M011/S01 | M011/S02, M011/S03 | unmapped |
| R092 | primary-user-loop | active | M011/S01 | M011/S03 | unmapped |
| R093 | primary-user-loop | active | M011/S01 | M011/S02, M011/S03 | unmapped |
| R094 | primary-user-loop | active | M011/S01 | M011/S02 | unmapped |
| R095 | core-capability | active | M011/S02 | M011/S03, M011/S04 | unmapped |
| R096 | primary-user-loop | active | M011/S03 | M011/S04 | unmapped |
| R097 | primary-user-loop | active | M011/S04 | none | unmapped |
| R098 | primary-user-loop | deferred | none | none | unmapped |
| R099 | quality-attribute | deferred | none | none | unmapped |
| R060 | core-capability | active | M011/S03 | M011/S01, M011/S02, M011/S04 | unmapped |
| R101 | core-capability | active | M012/S01 | M012/S02, M012/S03 | mapped |
| R102 | quality-attribute | active | M012/S01 | M012/S03 | mapped |
| R103 | primary-user-loop | active | M012/S01 | M012/S02, M012/S03 | mapped |
| R104 | primary-user-loop | active | M012/S02 | M012/S01 | mapped |
| R105 | core-capability | active | M012/S01 | M012/S02 | mapped |
| R106 | constraint | active | M012/S01 | M012/S04 | mapped |
| R107 | core-capability | active | M012/S03 | M012/S02 | mapped |
| R108 | quality-attribute | active | M012/S04 | M012/S01 | mapped |
| R109 | integration | active | M012/S03 | M012/S01 | mapped |

## New Requirements (M012)

### R101 — PuzzleModel as ID-only state machine
- Class: core-capability
- Status: active
- Description: PuzzleModel is a pure C# class with no Unity references that owns board state, a shared ordered deck, and N explicitly-tracked slots. All operations are on integer piece IDs.
- Why it matters: Eliminates the sync bugs caused by the old tray-window lookahead design.
- Source: user
- Primary owning slice: M012/S01
- Supporting slices: M012/S02, M012/S03
- Validation: unmapped
- Notes: Lives in SimpleGame.Puzzle asmdef with noEngineReferences:true.

### R102 — Configurable slot count via ScriptableObject
- Class: quality-attribute
- Status: active
- Description: Slot count is a field on a PuzzleModelConfig ScriptableObject, not hardcoded. Default is 3.
- Why it matters: Level designers can vary slot count without code changes.
- Source: user
- Primary owning slice: M012/S01
- Supporting slices: M012/S03
- Validation: unmapped
- Notes: Config asset read by InGameSceneController and passed to PuzzleModel constructor.

### R103 — Slots refill independently from shared deck top
- Class: primary-user-loop
- Status: active
- Description: When a slot's piece is correctly placed on the board, that slot immediately draws the next piece from the shared deck's top. Each slot refills independently; they share one deck.
- Why it matters: Core mechanic — the player manages which slots to play from as the deck drains.
- Source: user
- Primary owning slice: M012/S01
- Supporting slices: M012/S02, M012/S03
- Validation: unmapped
- Notes: Slot becomes null/empty when deck is exhausted and no more pieces are available.

### R104 — Wrong tap costs heart; slot piece unchanged
- Class: primary-user-loop
- Status: active
- Description: Tapping a slot whose piece cannot be legally placed (no placed neighbour on board) costs one heart. The slot piece does not change. The slot is not refilled.
- Why it matters: Classic skill-test mechanic — guess wrong and pay. Same piece stays so player can try to place it later once a neighbour is placed.
- Source: user
- Primary owning slice: M012/S02
- Supporting slices: M012/S01
- Validation: unmapped
- Notes: Consistent with R057 (heart system). Board adjacency rule unchanged (D064).

### R105 — Model fires typed events; presenter reacts
- Class: core-capability
- Status: active
- Description: PuzzleModel exposes typed events (OnSlotChanged, OnPiecePlaced, OnCompleted). InGamePresenter subscribes in Initialize() and pushes to the view reactively. No polling or state diffing in the presenter.
- Why it matters: Eliminates view sync bugs. Aligns with D028 (service/model reactive callbacks permitted).
- Source: user
- Primary owning slice: M012/S01
- Supporting slices: M012/S02
- Validation: unmapped
- Notes: Consistent with D027 (view→presenter events) and D028 (model→presenter reactive callbacks).

### R106 — PuzzleSession deleted; replaced by PuzzleModel
- Class: constraint
- Status: active
- Description: PuzzleSession, IPuzzleLevel, and PuzzleLevel are deleted. PuzzleModel is the sole domain object. IDeck/Deck may be kept as internal implementation details inside PuzzleModel or also deleted.
- Why it matters: Eliminates the old design entirely; prevents parallel maintenance burden.
- Source: user
- Primary owning slice: M012/S01
- Supporting slices: M012/S04
- Validation: unmapped
- Notes: rg "PuzzleSession" Assets/ must return zero results when S04 is complete.

### R107 — View receives slot-indexed updates (not tray window)
- Class: core-capability
- Status: active
- Description: IInGameView exposes RefreshSlot(int slotIndex, int? pieceId) instead of RefreshTray(int?[]). Each slot update is explicit and targeted.
- Why it matters: Removes the source of view/model sync bugs — the view always knows exactly what changed and where.
- Source: user
- Primary owning slice: M012/S03
- Supporting slices: M012/S02
- Validation: unmapped
- Notes: Breaking change to IInGameView interface; MockInGameView and InGameView both updated in S03.

### R108 — Domain tests for PuzzleModel contract
- Class: quality-attribute
- Status: active
- Description: PuzzleDomainTests is rewritten to cover PuzzleModel: slot refill on correct placement, heart event on wrong placement, deck exhaustion, win/lose conditions.
- Why it matters: Ensures the model contract is machine-verifiable; prevents regressions.
- Source: inferred
- Primary owning slice: M012/S04
- Supporting slices: M012/S01
- Validation: unmapped
- Notes: Old PuzzleDomainTests (for PuzzleSession) are deleted and replaced.

### R109 — JigsawLevelFactory feeds PuzzleModel cleanly
- Class: integration
- Status: active
- Description: JigsawLevelFactory.Build() produces output that can be passed directly to PuzzleModel constructor without an intermediate adapter or format conversion in the caller.
- Why it matters: Clean adapter boundary (D062) — factory is the only coupling point to SimpleJigsaw.
- Source: inferred
- Primary owning slice: M012/S03
- Supporting slices: M012/S01
- Validation: unmapped
- Notes: Factory output format may change from JigsawBuildResult{Level, RawBoard} to a richer struct that includes the flat piece list, seeds, and deck as PuzzleModel expects.

### R110 — Piece sizing: longest edge = 1 unit
- Class: core-capability
- Status: active
- Description: GridPlanner produces cells where the longest edge equals 1 world unit. For a 4×2 grid: cellW=0.5, cellH=1. Board extent = max(rows,cols) in each axis.
- Why it matters: Eliminates complex downstream scaling math; gives all consumers a predictable coordinate contract.
- Source: user
- Primary owning slice: M014/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Formula: longestNorm = Max(1/cols, 1/rows); unitScale = 1/longestNorm; cellW = unitScale/cols; cellH = unitScale/rows.

### R111 — Board rendered in world space at natural scale
- Class: core-capability
- Status: active
- Description: The puzzle board parent transform has localScale = (1,1,1). No scaling applied to fit screen — the camera frames the board instead.
- Why it matters: Required for camera pan UX; eliminates scaling compensation in InGameSceneController.
- Source: user
- Primary owning slice: M014/S02
- Supporting slices: M014/S01
- Validation: unmapped
- Notes: InGameSceneController.SpawnPieces boardSize scaling math is removed entirely.

### R112 — Hint surface shown behind pieces on board
- Class: primary-user-loop
- Status: active
- Description: HintSurfaceBuilder output is rendered as a world-space GameObject parented to the board, behind all pieces (z > 0).
- Why it matters: Players need visual guides showing where pieces belong — standard jigsaw UX.
- Source: user
- Primary owning slice: M014/S02
- Supporting slices: none
- Validation: unmapped
- Notes: Thickness ~0.02f (unit-scale). Styling deferred to R116.

### R113 — Camera pans over board by dragging
- Class: primary-user-loop
- Status: active
- Description: An orthographic camera can be panned by pointer-drag on the board. Drag only fires when pointer-down hits no UGUI element.
- Why it matters: Core mobile jigsaw navigation — board is larger than screen on higher levels.
- Source: user
- Primary owning slice: M014/S02
- Supporting slices: M014/S03
- Validation: unmapped
- Notes: CameraController MonoBehaviour on Main Camera. No bounds clamping for now.

### R114 — Tray slot pieces are 3D GameObjects with tweens intact
- Class: primary-user-loop
- Status: active
- Description: Slot pieces remain 3D world-space GameObjects. SlideToSlot, PlaceOnBoard, and ShakePiece tweens are preserved and function correctly.
- Why it matters: The tween animations are part of the game feel; removing them degrades the experience.
- Source: user
- Primary owning slice: M014/S03
- Supporting slices: none
- Validation: unmapped
- Notes: PieceTapHandler removed from tray pieces (replaced by UGUI buttons); tween logic in PieceTweener unchanged.

### R115 — Slot input via UGUI Buttons
- Class: primary-user-loop
- Status: active
- Description: Each tray slot has a UGUI Button overlay. Pointer-up-inside fires OnTapPiece. Standard UGUI cancel-on-exit behaviour applies.
- Why it matters: Cleanly separates slot tap from board drag gesture; UGUI naturally blocks board pan raycasts.
- Source: user
- Primary owning slice: M014/S03
- Supporting slices: none
- Validation: unmapped
- Notes: Screen Space Overlay Canvas. Buttons repositioned each frame to match 3D slot piece screen positions.

### R116 — Tray pieces and buttons follow camera each frame
- Class: primary-user-loop
- Status: active
- Description: In LateUpdate, tray 3D slot pieces are repositioned relative to camera bottom. UGUI slot buttons are repositioned via world-to-screen projection to match.
- Why it matters: Tray must remain at screen bottom regardless of where the camera is panned on the board.
- Source: user
- Primary owning slice: M014/S03
- Supporting slices: none
- Validation: unmapped
- Notes: none

### R117 — Board drag and slot tap do not conflict
- Class: quality-attribute
- Status: active
- Description: A drag starting on a slot button triggers piece placement (not camera pan). A drag starting on the board triggers camera pan (not slot placement).
- Why it matters: Essential for usable mobile controls — conflated gestures break the UX.
- Source: inferred
- Primary owning slice: M014/S02
- Supporting slices: M014/S03
- Validation: unmapped
- Notes: Achieved naturally by UGUI Screen Space Overlay blocking physics raycasts.

## Deferred

### R118 — Hint surface styling / material
- Class: differentiator
- Status: deferred
- Description: Custom material, opacity, color, or shader for the hint surface overlay.
- Why it matters: Visual polish — the default material is functional but not stylish.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Deferred by user. Default material sufficient for M014.

## Coverage Summary

- Total requirements: 118
- Active: 42
- Validated: 53
- Deferred: 19
- Out of scope: 4
- Note: R110–R118 added for M014 puzzle controls & layout redesign.

---

## M015 Requirements — Fastlane Distribution Pipeline

### R119 — iOS app registration via produce
- Class: core-capability
- Status: active
- Description: A lane creates the app on App Store Connect via `produce`, setting bundle ID, app name, SKU, and primary language from config files. No manual portal interaction required.
- Why it matters: Zero-click app registration is the foundation of data-driven distribution.
- Source: user
- Primary owning slice: M015/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Requires App Store Connect API key. `produce` has limited API key support — may fall back to Apple ID for app creation step only.

### R120 — Android app registration (documented manual first upload)
- Class: core-capability
- Status: active
- Description: The bootstrap lane documents the required one-time manual Play Console step (app creation + first APK upload to exit Draft status). All subsequent lifecycle is fully automated.
- Why it matters: Google Play API cannot create apps programmatically — this is a hard platform limit. Documenting it prevents confusion and sets correct expectations.
- Source: inferred
- Primary owning slice: M015/S01
- Supporting slices: none
- Validation: unmapped
- Notes: After the first manual upload, all track management, metadata, and further uploads are automated.

### R121 — Certificate and provisioning via match
- Class: core-capability
- Status: active
- Description: `match` manages development, ad-hoc, and appstore certificates and provisioning profiles, backed by a private git repo. All cert lifecycle (create, renew, rotate) is lane-driven.
- Why it matters: Eliminates cert management as a manual, machine-local, human-dependent process.
- Source: user
- Primary owning slice: M015/S02
- Supporting slices: none
- Validation: unmapped
- Notes: match repo URL is config-file driven.

### R122 — App Store Connect API key auth (no 2FA)
- Class: constraint
- Status: active
- Description: All Apple interactions use App Store Connect API key (key_id, issuer_id, .p8 file) loaded from environment or config. No Apple ID password or 2FA code is ever required at runtime.
- Why it matters: 2FA breaks CI and non-interactive runs. API key auth is the correct long-term path.
- Source: inferred
- Primary owning slice: M015/S02
- Supporting slices: M015/S01, M015/S04
- Validation: unmapped
- Notes: API key stored as env var or gitignored local file, never committed.

### R123 — Unity → iOS build pipeline
- Class: core-capability
- Status: active
- Description: A lane drives Unity CLI to export an Xcode project, then invokes `gym` (xcodebuild) to produce a signed .ipa. Build number is computed from the existing convention in Docs/BUILD_NUMBERS.md and written to ProjectSettings before the build.
- Why it matters: Reproducible, script-driven iOS builds replace manual Build > Archive.
- Source: user
- Primary owning slice: M015/S03
- Supporting slices: M015/S02
- Validation: unmapped
- Notes: Unity executable path is config-driven. Xcode project output path is stable and gitignored.

### R124 — Unity → Android build pipeline (AAB)
- Class: core-capability
- Status: active
- Description: A lane drives Unity CLI to export a signed .aab. Keystore path, alias, and passwords are read from environment or config. Build number follows the same convention as iOS.
- Why it matters: Reproducible Android builds replace manual Build Settings clicks.
- Source: user
- Primary owning slice: M015/S03
- Supporting slices: none
- Validation: unmapped
- Notes: AAB preferred over APK for Play Store. APK supported for ad-hoc distribution.

### R125 — Build number management using existing convention
- Class: constraint
- Status: active
- Description: Lanes read, compute, and write the bundle number using the `aabbccdd` encoding scheme defined in Docs/BUILD_NUMBERS.md. The build counter increments automatically for each distribution build.
- Why it matters: Build numbers must be strictly increasing — the stores reject anything else. Automation prevents human error.
- Source: inferred
- Primary owning slice: M015/S03
- Supporting slices: none
- Validation: unmapped
- Notes: Source of truth is ProjectSettings.asset. Lane reads current, increments counter, writes back before build.

### R126 — TestFlight upload and external tester management
- Class: core-capability
- Status: active
- Description: A lane uploads the .ipa to TestFlight via `pilot` and adds/updates external tester groups from a data file. Changelog comes from a version-controlled release notes file.
- Why it matters: Automates the most common distribution step — getting a build into testers' hands.
- Source: user
- Primary owning slice: M015/S04
- Supporting slices: M015/S03
- Validation: unmapped
- Notes: External tester group names and emails sourced from fastlane/config/testers.json.

### R127 — Google Play track-based upload
- Class: core-capability
- Status: active
- Description: A lane uploads the .aab to a specified Play track (internal/alpha/beta/production) via `upload_to_play_store`. Track is a parameter; default is internal. Release notes come from version-controlled files.
- Why it matters: Automates Play Console uploads across all tracks.
- Source: user
- Primary owning slice: M015/S05
- Supporting slices: M015/S03
- Validation: unmapped
- Notes: Google Play service account JSON is read from env var or gitignored local file.

### R128 — Store metadata management from version-controlled files
- Class: core-capability
- Status: active
- Description: App name, subtitle, description, keywords, support URL, and release notes live in fastlane/metadata/ (iOS) and fastlane/metadata/android/ (Android) as plain text files. A lane pushes metadata to both stores without touching store portals.
- Why it matters: Data-driven distribution — metadata changes are code reviews, not portal sessions.
- Source: user
- Primary owning slice: M015/S06
- Supporting slices: none
- Validation: unmapped
- Notes: Screenshots deferred (R132). Localization structure present but single-locale content for now.

### R129 — Status and query lanes (structured output)
- Class: operability
- Status: active
- Description: A `status` lane queries both platforms and returns JSON-structured state: current build version, latest TestFlight build + status, Play Console track versions, provisioning expiry, and app review status.
- Why it matters: Makes the tooling composable — external scripts, dashboards, or a future multi-project layer can consume this without parsing lane prose output.
- Source: user
- Primary owning slice: M015/S07
- Supporting slices: none
- Validation: unmapped
- Notes: Output to stdout as JSON. Platform-specific sub-lanes: `status_ios`, `status_android`. Structured even in dry-run mode.

### R130 — Dry-run mode across all lanes
- Class: quality-attribute
- Status: active
- Description: Every lane accepts a `dry_run: true` option. In dry-run mode, all API calls and file mutations are replaced with logged intent. No real state is changed. Exit code 0 on clean dry-run.
- Why it matters: Validate the flow and configuration before committing to real API calls. Essential for onboarding and debugging.
- Source: user
- Primary owning slice: M015/S01
- Supporting slices: M015/S02, M015/S03, M015/S04, M015/S05, M015/S06, M015/S07
- Validation: unmapped
- Notes: Dry-run implemented via a shared helper method in Fastfile. Dry-run output is clearly labeled.

### R131 — Two-tier folder structure
- Class: constraint
- Status: active
- Description: Project-specific lanes live in fastlane/ (standard location). Higher-level orchestration (bootstrap coordinator, multi-project plumbing) lives in tools/fastlane/. The tools/ layer has a clean internal interface and is designed to be extracted to a separate repo later.
- Why it matters: Keeps project-specific config separate from reusable orchestration. The separation is the first step toward a shared tooling layer.
- Source: user
- Primary owning slice: M015/S01
- Supporting slices: none
- Validation: unmapped
- Notes: tools/ is gitignore-safe to exclude from a package export.

## Deferred (M015)

### R132 — Automated screenshot upload
- Class: quality-attribute
- Status: deferred
- Description: Fastlane deliver/supply screenshot upload to both stores from version-controlled image assets.
- Why it matters: Completes the data-driven distribution picture.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Deferred by user. Metadata directory structure will include a screenshots/ placeholder.

### R133 — GitHub Actions CI wiring
- Class: integration
- Status: deferred
- Description: GitHub Actions workflow files that trigger lanes on push/tag.
- Why it matters: Full CI/CD pipeline — lanes run automatically without developer intervention.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Deferred by user. Lanes are designed to be CI-callable.

### R134 — Multi-project support
- Class: differentiator
- Status: deferred
- Description: The tools/ layer supports N projects with per-project config, shared cert repo, and unified status dashboard.
- Why it matters: Reuse the tooling across the studio's portfolio.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Single-project focus for M015. tools/ API designed with this in mind.

## Out of Scope (M015)

### R135 — App review submission automation
- Class: anti-feature
- Status: out-of-scope
- Description: Lanes do not submit apps for review or approve releases to production.
- Why it matters: Review submission is a deliberate human decision, not an automated step.
- Source: inferred
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: Promotion to production track is a separate lane requiring explicit invocation.

### R136 — In-app purchase or subscription management
- Class: anti-feature
- Status: out-of-scope
- Description: No IAP, subscription, or pricing management via lanes.
- Why it matters: Out of scope for distribution automation.
- Source: inferred
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a

## Active (M016)

### R137 — Anonymous PlayFab account on first launch
- Class: primary-user-loop
- Status: active
- Description: On first launch, the game silently creates an anonymous PlayFab account using `LoginWithCustomID` with a stable device-derived ID. No login UI is shown. Player can start playing immediately.
- Why it matters: Every player needs a PlayFab identity before cloud save or analytics can function.
- Source: user
- Primary owning slice: M016/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Custom ID derived from `SystemInfo.deviceUniqueIdentifier`. `CreateAccount: true` ensures account is created if absent.

### R138 — PlayFab entity token and Player ID persisted locally
- Class: continuity
- Status: active
- Description: After successful login, the PlayFab Player ID is stored in PlayerPrefs. On subsequent launches, `LoginWithCustomID` is called again with the same device ID — same account is recovered automatically.
- Why it matters: Without persistence, the same device could create multiple anonymous accounts across sessions.
- Source: user
- Primary owning slice: M016/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Entity token is session-scoped and re-obtained on each login — only Player ID needs local persistence for diagnostics/logging.

### R139 — Cloud save: push MetaSaveData to PlayFab User Data
- Class: continuity
- Status: active
- Description: After each game session ends (level complete, level failed, or app backgrounded), `MetaSaveData` is serialized as JSON and written to PlayFab User Data under a single key (`MetaSave`).
- Why it matters: Cloud backup means progress survives device loss or reinstall.
- Source: user
- Primary owning slice: M016/S02
- Supporting slices: none
- Validation: unmapped
- Notes: Serialized via `JsonUtility.ToJson`. Max value ~10KB — well within PlayFab limits.

### R140 — Cloud save: pull at boot with take-max-per-field merge
- Class: continuity
- Status: active
- Description: On boot, after PlayFab login, the game fetches cloud `MetaSaveData` and merges it with local data using take-max per field: `coins = max(local.coins, cloud.coins)`, `goldenPieces = max(...)`, and per-object `currentSteps = max(...)`.
- Why it matters: Protects against rolling back progress due to clock skew or offline play. Safe for monotonically increasing fields.
- Source: user
- Primary owning slice: M016/S02
- Supporting slices: none
- Validation: unmapped
- Notes: `IMetaSaveService` stays synchronous. Cloud pull is an explicit async step in `GameBootstrapper.Start()` before the navigation loop begins.

### R141 — Platform account linking: Game Center (iOS)
- Class: core-capability
- Status: active
- Description: Players can link their anonymous PlayFab account to their Game Center identity via `LinkGameCenterAccount`. Once linked, the same PlayFab account is recoverable from any iOS device signed into the same Game Center account.
- Why it matters: Cross-device progression recovery for iOS players.
- Source: user
- Primary owning slice: M016/S03
- Supporting slices: none
- Validation: unmapped
- Notes: Game Center APIs are built into iOS — no extra SDK. Requires `GameKit` framework on iOS build.

### R142 — Platform account linking: Google Play Games (Android)
- Class: core-capability
- Status: active
- Description: Players can link their anonymous PlayFab account to their Google Play Games identity via `LinkGooglePlayGamesServicesAccount`. Requires the Google Play Games Unity plugin.
- Why it matters: Cross-device progression recovery for Android players.
- Source: user
- Primary owning slice: M016/S03
- Supporting slices: none
- Validation: unmapped
- Notes: Highest-risk item in the milestone — Google Play Games Unity plugin is a separate install with its own App ID config. Should be proven early.

### R143 — First-launch platform link prompt
- Class: primary-user-loop
- Status: active
- Description: On first launch, after the anonymous account is created and before the main menu is shown, a skippable popup offers the player the option to link a platform account. Skipping dismisses the prompt permanently.
- Why it matters: Best moment to capture platform identity while the player has context. Settings remains the secondary path.
- Source: user
- Primary owning slice: M016/S03
- Supporting slices: M016/S01
- Validation: unmapped
- Notes: Uses existing popup stack. "Skip" stores a flag in PlayerPrefs so the prompt never appears again.

### R144 — Platform account link/unlink in Settings
- Class: core-capability
- Status: active
- Description: The Settings screen shows the player's current link status (linked/unlinked per platform) and provides buttons to link or unlink each platform account.
- Why it matters: Settings is the persistent home for account management after the first-launch prompt is dismissed.
- Source: user
- Primary owning slice: M016/S03
- Supporting slices: none
- Validation: unmapped
- Notes: Extends existing `ISettingsView` / `SettingsPresenter` / `SettingsView`.

### R145 — Analytics: session start and end
- Class: failure-visibility
- Status: active
- Description: PlayFab custom event `session_start` is fired when `GameBootstrapper` completes login. `session_end` is fired on `OnApplicationPause(true)` or `OnApplicationQuit`.
- Why it matters: DAU and session length baseline for retention analysis.
- Source: user
- Primary owning slice: M016/S04
- Supporting slices: M016/S01
- Validation: unmapped
- Notes: Events visible in PlayFab Game Manager > Analytics > Event History.

### R146 — Analytics: level started, completed, failed
- Class: failure-visibility
- Status: active
- Description: PlayFab custom events `level_started`, `level_completed`, `level_failed` are fired at corresponding points in `InGameSceneController`. Events include level ID as a property.
- Why it matters: Level funnel — which puzzles players finish vs abandon.
- Source: user
- Primary owning slice: M016/S04
- Supporting slices: none
- Validation: unmapped
- Notes: Fired via `PlayFabClientAPI.WritePlayerEvent`.

### R147 — Analytics: currency earned and spent
- Class: failure-visibility
- Status: active
- Description: PlayFab custom events `currency_earned` and `currency_spent` are fired when coins or golden pieces are earned or spent. Events include currency type and amount as properties.
- Why it matters: Economy health — coins and golden pieces flow visible in dashboard.
- Source: user
- Primary owning slice: M016/S04
- Supporting slices: none
- Validation: unmapped
- Notes: Hooked into `CoinsService` and `GoldenPieceService` via event callbacks or direct calls.

### R148 — Analytics: platform account linked
- Class: failure-visibility
- Status: active
- Description: PlayFab custom event `platform_account_linked` is fired when a player successfully links a platform account. Includes platform name (GameCenter/GooglePlayGames) as a property.
- Why it matters: Conversion funnel — how many anonymous players upgrade to a persistent identity.
- Source: user
- Primary owning slice: M016/S04
- Supporting slices: M016/S03
- Validation: unmapped
- Notes: Fired from the link success callback in the platform linking service.

## Deferred (M016)

### R149 — Conflict resolution UI
- Class: admin/support
- Status: deferred
- Description: When cloud and local save both have meaningful diverged data, show the player both saves and let them pick.
- Why it matters: Maximum data safety for players who play extensively on multiple devices simultaneously.
- Source: inferred
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Take-max-per-field handles the common case correctly. Full conflict UI is friction for a casual game and deferred indefinitely.

## Out of Scope (M016)

### R150 — Email/password authentication
- Class: anti-feature
- Status: out-of-scope
- Description: No email/password login or account creation flow.
- Why it matters: Scope clarity — platform identity (Game Center, Google Play Games) covers cross-device recovery without requiring a registration step.
- Source: user
- Primary owning slice: none
- Suppor

- Supporting slices: none
- Validation: n/a
- Notes: None.

### R151 � Leaderboards and multiplayer
- Class: anti-feature
- Status: out-of-scope
- Description: No leaderboard, friend, or multiplayer features.
- Why it matters: Out of scope for this milestone.
- Source: inferred
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: None.

---

## M017 Requirements — Unity Ads Integration

### R152 — Rewarded ad flow
- Class: primary-user-loop
- Status: active
- Description: When the player loses a level and taps "Watch Ad", a real Unity Ads rewarded video plays. Hearts are restored only if the ad completes. If the ad fails to load, the Watch button grays out and a message is shown — no crash, no silent grant.
- Why it matters: Core monetisation mechanic; replaces the stub that always granted rewards.
- Source: user
- Primary owning slice: M017/S02
- Supporting slices: M017/S01
- Validation: mapped
- Notes: Unavailable state: SetWatchInteractable(false) + UpdateStatus message. Player can still Skip.

### R153 — Interstitial ad flow
- Class: primary-user-loop
- Status: active
- Description: After every N level completions (N from remote config, default 3), a fullscreen interstitial ad shows before returning to main menu. If the ad fails to load, navigation proceeds silently.
- Why it matters: Passive monetisation revenue between levels.
- Source: user
- Primary owning slice: M017/S03
- Supporting slices: M017/S01
- Validation: mapped
- Notes: Session-scoped counter. N is configurable via PlayFab Title Data key "interstitial_every_n_levels".

### R154 — IAdService abstraction
- Class: core-capability
- Status: active
- Description: All game code interacts with ads through IAdService. UnityAdService wraps the SDK. NullAdService is used in all edit-mode tests. No SDK types leak into game logic.
- Why it matters: Keeps game layer testable; follows established interface-first pattern.
- Source: inferred
- Primary owning slice: M017/S01
- Supporting slices: none
- Validation: mapped
- Notes: Consistent with IAnalyticsService, ICloudSaveService pattern from M016.

### R155 — Ad analytics events
- Class: failure-visibility
- Status: active
- Description: Ad impression, completion, skip, and failed-to-load events fire through IAnalyticsService for both rewarded and interstitial ad types.
- Why it matters: Revenue attribution and failure rate visibility.
- Source: inferred
- Primary owning slice: M017/S04
- Supporting slices: M017/S01, M017/S02, M017/S03
- Validation: mapped
- Notes: Event names: ad_impression, ad_completed, ad_skipped, ad_failed_to_load. adType: "rewarded" or "interstitial".

### R156 — Banner ads
- Class: anti-feature
- Status: out-of-scope
- Description: No banner ad format.
- Why it matters: Scope clarity — only rewarded and interstitial in M017.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: None.

### R157 — Ad mediation
- Class: anti-feature
- Status: out-of-scope
- Description: No mediation layer (Unity LevelPlay, IronSource, AdMob). Direct Unity Ads only.
- Why it matters: Scope clarity.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: None.

### R158 — First-launch consent gate
- Class: launchability
- Status: active
- Description: On first launch (and every subsequent launch until accepted), a consent popup blocks the main menu. The player must tap Accept to proceed. There is no close button, no skip, no dismiss. The flag is stored in PlayerPrefs; once accepted it never shows again.
- Why it matters: Legal requirement for app store distribution — ToS acceptance must be explicit.
- Source: user
- Primary owning slice: M018/S01
- Supporting slices: none
- Validation: mapped
- Notes: Gate fires before PlayFab login and before main menu loads.

### R159 — ToS and Privacy Policy links
- Class: launchability
- Status: active
- Description: The consent popup displays tappable links to the Terms of Service and Privacy Policy, both pointing to https://simplemagicstudios.com/play. Links open in the device browser.
- Why it matters: App store requirement — users must be able to read the policies they are accepting.
- Source: user
- Primary owning slice: M018/S01
- Supporting slices: none
- Validation: mapped
- Notes: Same URL for both ToS and Privacy Policy per user specification.

### R160 — iOS App Tracking Transparency dialog
- Class: launchability
- Status: active
- Description: Immediately after the player accepts the consent popup, the native iOS ATT system dialog fires (RequestAuthorizationTracking). The result (authorized or denied) does not block game progression — ads and analytics work in both cases, just with or without IDFA.
- Why it matters: Apple App Store requirement since iOS 14.5 — apps that access IDFA without ATT authorization are rejected.
- Source: user
- Primary owning slice: M018/S02
- Supporting slices: M018/S01
- Validation: mapped
- Notes: ATT must fire before any SDK that uses IDFA (LevelPlay ads). On non-iOS platforms, ATT call is compiled out.

### R161 — ATT precedes ad SDK initialization
- Class: constraint
- Status: active
- Description: LevelPlay (Unity Ads) initialization is deferred until after the ATT result is known. This ensures the SDK can use the IDFA if authorized.
- Why it matters: Per Apple guidelines and Unity Ads docs — initializing the ad SDK before ATT result means it cannot access IDFA even if the user later authorizes.
- Source: inferred
- Primary owning slice: M018/S02
- Supporting slices: none
- Validation: mapped
- Notes: Boot sequence becomes: consent gate → ATT dialog → ads init → PlayFab login → main menu.

### R162 — NSUserTrackingUsageDescription in built plist
- Class: launchability
- Status: active
- Description: The Xcode build's Info.plist must contain NSUserTrackingUsageDescription with a human-readable explanation of tracking use. Injected via Unity post-build script.
- Why it matters: Apple rejects builds without this key if ATT is requested — mandatory for App Store submission.
- Source: inferred
- Primary owning slice: M018/S02
- Supporting slices: none
- Validation: PostBuildATT.cs uses IPostprocessBuildWithReport to inject NSUserTrackingUsageDescription via PlistDocument. Runs on iOS builds only. Key: "We use your device advertising ID to show you relevant ads and measure ad performance."
- Notes: Injected automatically; no manual Xcode editing required. Full verification requires an iOS build.

### R163 — GDPR/CCPA consent toggles
- Class: anti-feature
- Status: out-of-scope
- Description: No separate GDPR or CCPA opt-in/opt-out UI.
- Why it matters: Scope clarity — consent for this project is iOS ATT only.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: None.

### R164 — Android tracking consent
- Class: anti-feature
- Status: out-of-scope
- Description: No Android-specific tracking consent dialog.
- Why it matters: Scope clarity — ATT is iOS only.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: None.

---

## M019 — Real IAP

### R165 — IIAPService abstraction
- Class: core-capability
- Status: active
- Description: All game code purchases coins through an IIAPService interface. Real and mock implementations are swappable. No presenter or scene controller calls Unity Purchasing or PlayFab IAP APIs directly.
- Why it matters: Keeps game layer testable without the store SDK; consistent with IAdService, IAnalyticsService pattern.
- Source: inferred
- Primary owning slice: M019/S01
- Supporting slices: M019/S04
- Validation: mapped
- Notes: Mock implementation must be usable from Editor without any store SDK present.

### R166 — Editor mock IAP with selectable outcomes
- Class: quality-attribute
- Status: active
- Description: In the Editor (and in tests), IIAPService resolves to a MockIAPService whose outcome (success, payment failed, PlayFab validation failed, user cancelled) is configurable via a ScriptableObject. All code paths exercisable without a device.
- Why it matters: Development and QA velocity — every outcome must be testable without sandbox credentials or a real device.
- Source: user
- Primary owning slice: M019/S01
- Supporting slices: none
- Validation: mapped
- Notes: None.

### R167 — Unity Purchasing SDK integration
- Class: integration
- Status: active
- Description: com.unity.purchasing is added to the project. Products are initialised via IStoreListener. The real IIAPService implementation wraps Unity Purchasing, initialises on app start, and exposes a BuyAsync(productId) method.
- Why it matters: Unity Purchasing is the standard Unity-native way to communicate with App Store and Google Play.
- Source: user
- Primary owning slice: M019/S02
- Supporting slices: none
- Validation: mapped
- Notes: asmdef must reference UnityEngine.Purchasing.

### R168 — PlayFab receipt validation (iOS + Android)
- Class: integration
- Status: active
- Description: After a successful Unity Purchasing transaction, the receipt is sent to PlayFab via ValidateIOSReceipt (iOS) or ValidateGooglePlayPurchase (Android). Coins are granted only after PlayFab confirms the receipt. The store transaction is finished (ConfirmPendingPurchase) only after validation succeeds.
- Why it matters: Server-side validation prevents receipt spoofing and fraudulent coin grants.
- Source: user
- Primary owning slice: M019/S03
- Supporting slices: none
- Validation: mapped
- Notes: PlayFab catalog items must have ItemIds matching Unity Purchasing product IDs.

### R169 — Coin packs defined as a single source of truth
- Class: core-capability
- Status: active
- Description: The three coin pack definitions (product ID, display name, coin amount) live in one ScriptableObject (IAPProductCatalog). Both ShopPresenter and IAPPurchasePresenter read from it. No hardcoded pack data in presenters.
- Why it matters: Product IDs must match exactly between Unity Purchasing, PlayFab catalog, and in-game display. A single source prevents drift.
- Source: inferred
- Primary owning slice: M019/S01
- Supporting slices: M019/S04
- Validation: mapped
- Notes: Prices shown in UI come from the store (Unity Purchasing product metadata), not from the ScriptableObject.

### R170 — Shop panel wired to real IAP
- Class: primary-user-loop
- Status: active
- Description: Tapping a coin pack in the Shop panel triggers a real purchase via IIAPService. On success, coins are added to ICoinsService and persisted. On failure or cancellation, an error message is shown and no coins are granted.
- Why it matters: The shop is the primary monetisation surface.
- Source: user
- Primary owning slice: M019/S04
- Supporting slices: M019/S03
- Validation: mapped
- Notes: None.

### R171 — IAPPurchase popup wired to real IAP (coins, not golden pieces)
- Class: primary-user-loop
- Status: active
- Description: The IAPPurchase popup triggers a real purchase for a single coin pack via IIAPService. Grants coins (not golden pieces — correcting the stub). On success, coins added to ICoinsService and persisted.
- Why it matters: Popup is a secondary IAP entry point; must be functionally equivalent to the shop flow.
- Source: user
- Primary owning slice: M019/S04
- Supporting slices: M019/S03
- Validation: mapped
- Notes: The stub granted golden pieces — this is a deliberate correction.

### R172 — Test payments in sandbox
- Class: quality-attribute
- Status: active
- Description: End-to-end purchase flow (Unity Purchasing → PlayFab validation → coin grant) works with Apple sandbox and Google test accounts on device. No real money charged during testing.
- Why it matters: Required for pre-submission store review and QA confidence.
- Source: user
- Primary owning slice: M019/S03
- Supporting slices: M019/S02
- Validation: mapped
- Notes: Requires PlayFab title configured with correct package name / bundle ID and shared secret.

---

## M023 Requirements — In-Game Camera Movement

### R173 — Auto-tracking camera frames valid placement positions
- Class: primary-user-loop
- Status: active
- Description: After each piece placement, the camera computes the world-space bounding box of all currently valid placement target positions (pieces in deck slots that have a placed neighbour) and smoothly animates to frame that region.
- Why it matters: Guides the player's eye to where gameplay is happening. On larger boards (8×7+), valid placements may be off-screen without camera guidance.
- Source: user
- Primary owning slice: M023/S01
- Supporting slices: M023/S03
- Validation: unmapped
- Notes: Valid placements determined by PuzzleBoard.CanPlace() against current slot contents. World positions from PuzzleStageController._solvedWorldPositions.

### R174 — Camera animation is smooth and slow (~1–1.5s glide)
- Class: quality-attribute
- Status: active
- Description: Camera transitions use smooth interpolation (SmoothDamp or similar) with a target duration of ~1–1.5 seconds. Movement feels deliberate, not snappy.
- Why it matters: User explicitly wants "smooth and slow" camera movement — this is a feel requirement.
- Source: user
- Primary owning slice: M023/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Speed configurable via ScriptableObject.

### R175 — All valid moves visible on screen simultaneously
- Class: primary-user-loop
- Status: active
- Description: The auto-tracking framing ensures every valid placement target position is visible within the camera viewport simultaneously, with appropriate padding.
- Why it matters: Player must be able to see all possible moves to make informed decisions.
- Source: user
- Primary owning slice: M023/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Zoom level is computed from the bounding box of valid targets plus padding, clamped to min/max zoom.

### R176 — Configurable min/max zoom limits (orthographic size)
- Class: quality-attribute
- Status: active
- Description: Camera zoom (orthographicSize) is clamped between configurable min and max values, enforced in both auto-tracking and manual zoom modes.
- Why it matters: Prevents over-zoom (pieces too large, player loses context) and under-zoom (pieces too small to see).
- Source: user
- Primary owning slice: M023/S02
- Supporting slices: M023/S01
- Validation: unmapped
- Notes: Limits stored in CameraConfig ScriptableObject.

### R177 — Camera boundary clamping with ~10–15% margin beyond board
- Class: quality-attribute
- Status: active
- Description: Camera position is clamped so it cannot drift beyond the puzzle board boundaries plus a configurable margin (~10–15% of board extent). Enforced in both auto and manual modes.
- Why it matters: Prevents showing dead space beyond the puzzle, maintaining visual focus.
- Source: user
- Primary owning slice: M023/S02
- Supporting slices: M023/S01
- Validation: unmapped
- Notes: Board bounds computed from GridPlanner: (0,0) to (unitScale, unitScale) where unitScale = max(rows, cols).

### R178 — Manual drag override
- Class: primary-user-loop
- Status: active
- Description: Single-finger drag (mobile) or mouse drag (PC) overrides auto-tracking and puts the camera in manual control mode. Extends the existing CameraController pan behavior.
- Why it matters: Player must be able to look around the board freely.
- Source: user
- Primary owning slice: M023/S02
- Supporting slices: none
- Validation: unmapped
- Notes: Existing CameraController already handles drag panning — needs integration with auto/manual state machine.

### R179 — Pinch-to-zoom on mobile / scroll wheel on PC
- Class: primary-user-loop
- Status: active
- Description: Multi-touch pinch gesture (mobile) and scroll wheel (PC/Editor) control camera zoom. Zoom is centered on the pinch midpoint or cursor position.
- Why it matters: Standard mobile camera control that players expect in any zooming interface.
- Source: user
- Primary owning slice: M023/S02
- Supporting slices: none
- Validation: unmapped
- Notes: Not currently implemented in CameraController — new feature.

### R180 — Manual mode persists until next piece placement
- Class: core-capability
- Status: active
- Description: Once the player interacts with the camera (drag or zoom), manual mode stays active until the next successful piece placement on the board, at which point auto-tracking resumes.
- Why it matters: Player stays in control after deliberate interaction; auto-tracking doesn't fight manual exploration.
- Source: user
- Primary owning slice: M023/S02
- Supporting slices: M023/S01
- Validation: unmapped
- Notes: PuzzleModel.OnPiecePlaced triggers return to auto-tracking.

### R181 — Level start shows full board then zooms to first valid area
- Class: primary-user-loop
- Status: active
- Description: At level start, the camera begins showing the entire puzzle board (zoomed out to fit), then smoothly transitions to frame the area around the first valid placements.
- Why it matters: Gives the player spatial context before gameplay begins.
- Source: user
- Primary owning slice: M023/S03
- Supporting slices: M023/S01, M023/S02
- Validation: unmapped
- Notes: Overview hold duration and transition speed configurable.

### R182 — Camera config as ScriptableObject
- Class: quality-attribute
- Status: active
- Description: All camera tuning values (move speed, zoom speed, min/max zoom, boundary padding, overview hold duration) are exposed on a CameraConfig ScriptableObject, tunable without code changes.
- Why it matters: Iteration speed — designer can tune feel in the Inspector without recompilation.
- Source: inferred
- Primary owning slice: M023/S01
- Supporting slices: M023/S02, M023/S03
- Validation: unmapped
- Notes: Follows project pattern of serialized ScriptableObject configs (GridLayoutConfig, PieceRenderConfig, PopupAnimationConfig).
