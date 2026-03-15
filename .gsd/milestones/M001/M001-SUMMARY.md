---
id: M001
provides:
  - Unity 6000.3.4f1 project at C:/OtherWork/simplegame, compiling cleanly with UniTask
  - Complete MVP base infrastructure: IView, Presenter<TView>, UIFactory with callback-based constructor
  - Six view interfaces: ISampleView, IMainMenuView, ISettingsView, IConfirmDialogView, IPopupView (IScreenView omitted; IView sufficient)
  - Six presenters: SamplePresenter, MainMenuPresenter, SettingsPresenter, ConfirmDialogPresenter
  - Domain service: GameService (pure C#, constructor-injected)
  - ScreenManager: history stack, navigation guard, optional ITransitionPlayer + IInputBlocker injection
  - PopupManager: stack-based, concurrency-guarded, reference-counted input blocking
  - IInputBlocker / UnityInputBlocker: CanvasGroup reference-counting, blocksRaycasts toggling
  - ITransitionPlayer / UnityTransitionPlayer: CanvasGroup alpha interpolation, fade-to-black
  - UnitySceneLoader: SceneManager additive load/unload wrapper
  - UnityPopupContainer: pre-instantiated show/hide via SetActive dispatch
  - GameBootstrapper: async UniTaskVoid Start() composing full dependency chain
  - Three scenes: Boot.unity (persistent), MainMenu.unity (additive), Settings.unity (additive)
  - 49 NUnit edit-mode tests — all passing (6 MVPWiringTests, 8 ScreenManagerTests, 13 PopupManagerTests, 5 TransitionTests, 17 DemoWiringTests)
  - SimpleGame.Runtime.asmdef, SimpleGame.Tests.EditMode.asmdef, SimpleGame.Editor.asmdef
key_decisions:
  - UniTask via git URL — portable, CLI-compatible, resolves cleanly in batchmode
  - event Action on view interfaces (not UnityEvent) — keeps interfaces Unity-type-free for edit-mode mocking
  - Two-phase presenter lifecycle: constructor injects fields; Initialize() subscribes events; Dispose() unsubscribes
  - ScreenManager optional ITransitionPlayer + IInputBlocker params — all 27 existing tests compile unchanged
  - Presenters receive callbacks (Action<ScreenId>, Func<UniTask>) not full manager references
  - Popup pre-instantiation: SetActive show/hide in Boot scene, not additive scene loading
  - com.unity.ugui explicitly declared in manifest.json — required for Unity 6 uGUI types
  - FindFirstObjectByType replaces deprecated FindObjectOfType in GameBootstrapper
  - SceneSetupHelpers static void + out-params satisfies static-state grep guard
  - DismissAllAsync calls Unblock() per popup inside the loop (reference-counting correctness)
  - MergedLog helpers for transition ordering tests (single shared List<string>)
  - UIFactory backward-compatible (GameService) overload preserves 6 existing MVPWiringTests
patterns_established:
  - All view interfaces extend IView; expose events as event Action (no Unity types); one interface per view
  - Presenter two-phase lifecycle: constructor for injection, Initialize() for subscription, Dispose() for cleanup
  - UIFactory receives all services/callbacks at construction; Create* methods on demand
  - MockView test double pattern: LastXxxText + UpdateCallCount + SimulateXxxClicked() in pure C#
  - MockSceneLoader/MockPopupContainer: ordered CallLog list + UniTask.CompletedTask returns
  - IInputBlocker reference-counting: each Block() increments, each Unblock() decrements; Math.Max(0) clamp
  - Transition orchestration: Block → FadeOut → unload → load → FadeIn → Unblock (finally)
  - Optional DI via null-default params: zero behavior change when no transition/blocker supplied
  - View MonoBehaviour pattern: SerializeField Button/Text, Awake() wires onClick→event Action, UpdateXxx sets text
  - GameBootstrapper boot flow: Find infrastructure → new services → new managers → new UIFactory with closures → await ShowScreenAsync → Find view → presenter.Initialize()
observability_surfaces:
  - TestResults.xml at project root — result="Passed" total="49" passed="49" failed="0"
  - Static guard: grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask" → empty
  - No UnityEngine in Core: grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/ → empty
  - GameBootstrapper Debug.Log trail with [GameBootstrapper] prefix — each boot phase logged
  - ScreenManager.CurrentScreen / CanGoBack — queryable at runtime
  - PopupManager.HasActivePopup / PopupCount — queryable at runtime
  - UnityInputBlocker.IsBlocked — queryable at runtime
  - EditorBuildSettings.asset m_Scenes — Boot(0), MainMenu(1), Settings(2), all enabled:1
requirement_outcomes:
  - id: R001
    from_status: active
    to_status: active
    proof: S05 — 3 view interfaces + 3 view MonoBehaviours (no non-Unity refs) + 3 plain C# presenters + GameService domain layer fully assembled; 49/49 tests pass; static guard clean; no UnityEngine in Core; pending play-mode UAT for full runtime proof
  - id: R002
    from_status: active
    to_status: active
    proof: S05 — MockMainMenuViewHasNoPresenterReference, MockSettingsViewHasNoPresenterReference, MockConfirmDialogViewHasNoPresenterReference reflection tests all pass; MainMenuView/SettingsView/ConfirmDialogView grep clean for non-Unity refs; pending play-mode UAT
  - id: R003
    from_status: active
    to_status: validated
    proof: S01 — ISampleView + SamplePresenter + MockViewHasNoPresenterReference test pass; S05 extends to IMainMenuView, ISettingsView, IConfirmDialogView — all 3 additional mock-view reflection tests pass; 49/49 tests pass
  - id: R004
    from_status: active
    to_status: active
    proof: S05 — UIFactory expanded with CreateMainMenuPresenter, CreateSettingsPresenter, CreateConfirmDialogPresenter; backward-compatible (GameService) overload; GameBootstrapper is sole caller; 17 DemoWiringTests confirm all factory Create methods; pending play-mode UAT
  - id: R005
    from_status: validated
    to_status: validated
    proof: S01 validated; S02–S05 extended the pattern consistently — no DI framework, no service locator; all constructors and optional params inject dependencies explicitly; 49/49 tests pass
  - id: R006
    from_status: active
    to_status: validated
    proof: S05 — static guard (grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask") returns empty across all 5 slices' files; SceneSetupHelpers static void + out-params pattern resolves false-positive edge case; domain-reload-disabled mode supported
  - id: R007
    from_status: active
    to_status: active
    proof: S05 — GameService constructed in GameBootstrapper, injected through UIFactory closures all the way to MainMenuPresenter; full dependency chain proven; 49/49 tests pass; pending play-mode UAT
  - id: R008
    from_status: active
    to_status: active
    proof: S05 — Boot.unity at EditorBuildSettings index 0; GameBootstrapper wires services → managers → factory → awaits ShowScreenAsync(MainMenu); batchmode compile + scene setup verified; pending play-mode UAT
  - id: R009
    from_status: active
    to_status: active
    proof: S05 — Boot scene persistent; MainMenu/Settings additively loaded/unloaded by ScreenManager; InputBlocker/TransitionOverlay/PopupCanvas all in Boot scene; pending play-mode UAT for runtime proof
  - id: R010
    from_status: active
    to_status: active
    proof: S05 — GameBootstrapper calls ShowScreenAsync(MainMenu) at boot; NavigateAndWirePresenter handles Settings; GoBackAndWirePresenter handles back; presenter disposed before each transition, initialized after; 49/49 tests pass; pending play-mode UAT
  - id: R011
    from_status: validated
    to_status: validated
    proof: S03 — PopupManager Stack<PopupId> push/pop/dismiss-all; concurrency guard; reference-counted input blocking; 13 tests pass; TestResults.xml total="27" passed="27" failed="0"
  - id: R012
    from_status: validated
    to_status: validated
    proof: S03 — IInputBlocker reference-counting contract; UnityInputBlocker CanvasGroup blocksRaycasts toggle; 2 reference-counting tests + 4 integration tests pass; TestResults.xml total="27" passed="27" failed="0"
  - id: R013
    from_status: validated
    to_status: validated
    proof: S04 — ITransitionPlayer + ScreenManager orchestration (Block → FadeOut → unload → load → FadeIn → Unblock in finally); 5 transition tests; 32/32 pass; static guard clean; finally-block Unblock at lines 75/118; blocksRaycasts=false enforced at 6 points in UnityTransitionPlayer
  - id: R014
    from_status: active
    to_status: active
    proof: S05 — UniTask in all async paths: ISceneLoader, ScreenManager, IPopupContainer, PopupManager, GameBootstrapper (async UniTaskVoid Start), presenter Func<UniTask> callbacks; CancellationToken threaded; Mock*s return UniTask.CompletedTask; 49/49 tests pass
  - id: R015
    from_status: active
    to_status: validated
    proof: S05 — 49/49 edit-mode tests pass: 6 MVPWiringTests + 8 ScreenManagerTests + 13 PopupManagerTests + 5 TransitionTests + 17 DemoWiringTests; all run in Unity batchmode EditMode without Unity runtime; TestResults.xml result="Passed" total="49" passed="49" failed="0"
  - id: R016
    from_status: active
    to_status: active
    proof: S05 — Boot.unity/MainMenu.unity/Settings.unity fully populated; GameBootstrapper wires full chain; MainMenu↔Settings navigation + ConfirmDialog popup implemented; 49/49 tests pass; pending play-mode UAT
  - id: R017
    from_status: active
    to_status: validated
    proof: S05 — MockMainMenuView, MockSettingsView, MockConfirmDialogView in pure C#; all 17 DemoWiringTests run without Unity runtime; all 49 total tests run without Unity runtime; no IView/IPopupView subtype has UnityEngine imports; grep clean on Core
duration: ~250m total (S01: ~50m, S02: ~35m, S03: ~35m, S04: ~40m, S05: ~90m)
verification_result: passed
completed_at: 2026-03-15
---

# M001: MVP UI Architecture Foundation

**Full MVP UI architecture assembled end-to-end: Unity 6 project with UniTask, 49/49 edit-mode tests passing, Boot→MainMenu→Settings navigation with fade transitions, stack-based popup with input blocking — all wired via constructor injection with no static state.**

## What Happened

Five slices ran sequentially to build a complete, testable MVP-based UI architecture from an empty Unity project to a fully wired demo with three screens and a popup.

**S01 established the foundation.** An empty Unity 6000.3.4f1 project was initialized with UniTask installed via git URL. Six pure C# types defined the MVP contract: `IView` (marker interface), `Presenter<TView>` (abstract base with two-phase lifecycle), `ISampleView` (first view interface using `event Action`), `SamplePresenter` (constructor injection demo), `UIFactory` (central factory), and `GameService` (domain service). Two assembly definitions established the build graph. Six NUnit edit-mode tests proved the full MVP wiring pattern — presenter construction, factory creation, event subscription, event response, disposal, and view-independence via reflection. This slice also resolved two critical infrastructure facts: `com.unity.test-framework` must be added manually to the manifest, and `-quit` must not be passed to `-runTests` in batchmode.

**S02 built screen navigation.** `ScreenId` enum (type-safe screen identification), `ISceneLoader` interface (abstracts Unity SceneManager for testing), and `ScreenManager` (history stack + navigation guard + `ShowScreenAsync`/`GoBackAsync`) were added as pure C# types. `UnitySceneLoader` wrapped Unity's `SceneManager.LoadSceneAsync` in additive mode. Two placeholder scenes (MainMenu, Settings) were created and registered in EditorBuildSettings via a batchmode `-executeMethod` script. Eight new tests proved every navigation scenario — forward, back, double-back, empty-history guard, and concurrency lock. The `ToSceneName` static helper was omitted in favor of `enum.ToString()` to avoid the static-state grep guard's false-positive for `static string`.

**S03 built the popup system and input blocker.** `PopupId` enum, `IInputBlocker` (reference-counting contract), `IPopupContainer` (async show/hide interface), and `PopupManager` (mirrors ScreenManager structurally: stack + concurrency guard + try/finally) were created. `UnityInputBlocker` is a MonoBehaviour with a `CanvasGroup` that increments/decrements a block count with `blocksRaycasts` toggling. One correctness bug was caught during test writing: `DismissAllAsync` originally called `Unblock()` once after the while loop — with reference counting this left `BlockCount` positive after multi-popup dismiss. Fixed by moving `Unblock()` inside the loop. Thirteen new tests brought the total to 27.

**S04 integrated fade transitions.** `ITransitionPlayer` (pure C# interface with `FadeOutAsync`/`FadeInAsync`) was created and injected into `ScreenManager` as optional constructor parameters (`null` defaults). Both `ShowScreenAsync` and `GoBackAsync` gained the full orchestration sequence: `Block()` → `FadeOutAsync` → unload → load → `FadeInAsync` → `Unblock()` in `finally`. `UnityTransitionPlayer` implements the interface with CanvasGroup alpha interpolation using `UniTask.Yield(ct)`, with `blocksRaycasts = false` enforced at 6 points and `gameObject.SetActive(false)` on fade-in completion. Crucially, the optional-parameter approach meant all 27 existing `new ScreenManager(loader)` test sites compiled unchanged. Five new tests (including a `MergedLog` helper for interleaved-event ordering assertions) brought the total to 32.

**S05 assembled the complete runtime.** Three view interfaces (`IMainMenuView`, `ISettingsView`, `IConfirmDialogView`) and three presenters (`MainMenuPresenter`, `SettingsPresenter`, `ConfirmDialogPresenter`) were created using the callback pattern: presenters receive `Action<ScreenId>`, `Action<PopupId>`, or `Func<UniTask>` instead of full manager references, keeping them testable in pure C#. `UIFactory` gained a full-callback constructor plus a backward-compatible `(GameService)` overload that preserved the 6 existing tests. Seventeen new `DemoWiringTests` covered all three presenters.

On the Unity runtime side, three View MonoBehaviours wire `Button.onClick` to `event Action` in `Awake()` with zero non-Unity references. `UnityPopupContainer` dispatches `PopupId` to `SetActive` on a pre-instantiated `ConfirmDialogView` in the Boot scene. `GameBootstrapper` composes the full dependency chain in an `async UniTaskVoid Start()`: finds infrastructure MonoBehaviours via `FindFirstObjectByType<T>()`, constructs services and managers, creates `UIFactory` with closures delegating to real managers, awaits the first `ShowScreenAsync`, then finds the loaded view and initializes the first presenter. Two infrastructure gaps were resolved: `com.unity.ugui` is not auto-included in Unity 6 (added to manifest + asmdef references), and editor scripts referencing custom asmdefs require an explicit `SimpleGame.Editor.asmdef`. `SceneSetup.cs` was extended to programmatically build all three scene hierarchies with full UI wiring via `SerializedObject.FindProperty`. Final test run: 49/49 passing.

## Cross-Slice Verification

**Success criterion: User can enter play mode from boot scene, navigate Main Menu → Settings → Main Menu with fade transitions**
- Verified at logic level by tests: `ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`, `GoBackAsync_WithTransition_PlaysFullTransitionSequence` (TransitionTests); `ScreenManagerNavigatesToNewScreen`, `ScreenManagerGoesBack` (ScreenManagerTests)
- Verified structurally: Boot.unity at EditorBuildSettings index 0; GameBootstrapper code wires full chain; batchmode compile exit 0
- Verified by UAT: pending human play-mode walkthrough (S05-UAT.md)

**Success criterion: A stack-based popup can be opened over any screen, blocks input below, and dismisses cleanly**
- Verified by tests: `ShowPopupAsync_PushesPopupOntoStack`, `ShowPopupAsync_BlocksInput`, `DismissPopupAsync_UnblocksInputWhenStackEmpty`, `DismissAllAsync_ClearsEntireStack` (PopupManagerTests)
- Verified structurally: UnityPopupContainer + UnityInputBlocker wired in Boot scene; ConfirmDialogPresenter wired through UIFactory callbacks
- Verified by UAT: pending human play-mode walkthrough

**Success criterion: Input is blocked during all transitions and scene loads**
- Verified by tests: `ShowScreenAsync_WithTransition_BlocksAndUnblocksInput` — confirms Block/Unblock called once each, IsBlocked=false after completion; exception safety test confirms Unblock() in finally
- Verified by diagnostic: `grep -n "finally" ScreenManager.cs` → lines 75 and 118 (both navigation methods); `grep "blocksRaycasts = false" UnityTransitionPlayer.cs` → 6 matches

**Success criterion: No static fields holding state exist in the codebase**
- Verified: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output (exit 1, no matches) ✅

**Success criterion: Every dependency is traceable from boot to presenter via constructor/init injection**
- Verified by code inspection: GameBootstrapper → `new GameService()` → `new ScreenManager(loader, transitionPlayer, inputBlocker)` → `new UIFactory(callbacks)` → `CreateMainMenuPresenter(view)` → `new MainMenuPresenter(view, navigateCallback, popupCallback)` → `Initialize()`. No service locator, no DI framework. Full chain traceable in a single file.
- Verified by tests: `UIFactoryCreatesMainMenuPresenter`, `MainMenuPresenterInitializeSetsTitle` (DemoWiringTests)

**Success criterion: Edit-mode tests verify presenter construction, screen manager, popup stack, and factory wiring in isolation**
- Verified: TestResults.xml — `result="Passed" total="49" passed="49" failed="0"` ✅
  - MVPWiringTests (6): presenter construction, factory creation, event subscription/disposal
  - ScreenManagerTests (8): navigation, history, concurrency guard, empty-stack guard
  - PopupManagerTests (13): push/pop/dismiss-all, input blocking, reference counting, concurrency
  - TransitionTests (5): orchestration ordering, input brackets, GoBack, null passthrough, exception safety
  - DemoWiringTests (17): all three demo presenters — construction, initialize, events, disposal, view independence

**Success criterion: Views have no references to presenters, models, or services — only expose interfaces**
- Verified by tests: `MockMainMenuViewHasNoPresenterReference`, `MockSettingsViewHasNoPresenterReference`, `MockConfirmDialogViewHasNoPresenterReference` (reflection tests) — all pass
- Verified by grep: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` → empty ✅
- Verified by grep: `grep -r "Presenter\|Service\|Factory\|Manager" Assets/Scripts/Runtime/MVP/` → MonoBehaviours only reference their own interface types ✅

**Definition of Done checklist:**
- All five slices marked `[x]` in M001-ROADMAP.md ✅
- All slice summaries exist (S01-SUMMARY.md through S05-SUMMARY.md) ✅
- Boot scene initializes services, constructs factory, transitions to main menu ✅ (code + batchmode verified; human UAT pending)
- Screen navigation between at least 2 screens with fade transitions ✅ (code + tests verified; human UAT pending)
- Popup opens, stacks, blocks input, and dismisses ✅ (code + tests verified; human UAT pending)
- Static grep guard returns no output ✅
- Edit-mode tests all pass (49/49) ✅
- Final integrated acceptance in play mode — ⚠️ **pending human UAT** per S05-UAT.md

## Requirement Changes

- R003 (Interface-per-view): active → validated — S01 ISampleView + SamplePresenter + MockViewHasNoPresenterReference test; S05 extends to IMainMenuView, ISettingsView, IConfirmDialogView — all 3 mock-view reflection tests pass; 49/49 total
- R006 (No static state): active → validated — static guard returns empty across all 5 slices; `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` produces no output; SceneSetupHelpers pattern resolves the only false-positive edge case
- R015 (Edit-mode unit tests): active → validated — 49/49 edit-mode tests pass in batchmode: 6 MVPWiringTests + 8 ScreenManagerTests + 13 PopupManagerTests + 5 TransitionTests + 17 DemoWiringTests; TestResults.xml result="Passed" total="49" passed="49" failed="0"
- R017 (Each layer testable in isolation): active → validated — MockMainMenuView, MockSettingsView, MockConfirmDialogView in pure C#; all 49 tests run without Unity runtime; no IView subtype has UnityEngine imports; Core grep clean
- R005 (Constructor/init injection only): validated → validated — extended through S02–S05; all new types use constructor injection; no DI framework; 49/49 tests pass
- R011 (Stack-based popup system): validated → validated — retained from S03; 49/49 total pass
- R012 (Full-screen raycast input blocker): validated → validated — retained from S03
- R013 (Fade transitions): validated → validated — retained from S04; 49/49 total pass
- R001, R002, R004, R007, R008, R009, R010, R014, R016: active → active — batchmode-verified; pending play-mode UAT for full runtime proof

## Forward Intelligence

### What the next milestone should know
- The presenter-callback pattern (`Action<ScreenId>`, `Func<UniTask>`) is clean for 2-3 callbacks but will become unwieldy past 4+ per presenter. Consider a thin `INavigationService` / `IPopupService` interface if presenter callback count grows.
- `GameBootstrapper.NavigateAndWirePresenter` has a hard-coded switch on `ScreenId`. Any new screen requires adding a case here — it is not automatic.
- `UnityPopupContainer` has a hard-coded switch on `PopupId`. New popup types require a new `PopupId` value, a new serialized field in `UnityPopupContainer`, and a new case.
- `ScreenId.ToString()` is used as the scene name — enum member names must stay in sync with `.unity` file names. A rename in either place without updating the other silently breaks runtime scene loading.
- `FindFirstObjectByType<T>()` in `GameBootstrapper` requires the view MonoBehaviour to appear exactly once in the loaded scene. Duplication or absence produces only a `Debug.LogError` — not an exception.
- `SerializedObject` property name strings in `SceneSetup.cs` are fragile — renaming a `[SerializeField]` field without updating `SceneSetup` silently skips the wire-up.

### What's fragile
- `UnityTransitionPlayer` cancellation behavior — mid-fade cancellation via `CancellationToken` leaves the overlay active at intermediate alpha. Any future slice that cancels navigation must reset the overlay or handle `OperationCanceledException` explicitly.
- `_activeScreenPresenter` in `GameBootstrapper` is stored as `object` with pattern-matching — adding a new screen presenter type requires a new `else if` branch in `DisposeScreenPresenter()`.
- UniTask installed via git URL pointing to HEAD — not pinned to a fixed tag. A breaking change upstream would affect the next project open. Low risk but worth pinning for production.
- `autoReferenced: true` in `SimpleGame.Runtime.asmdef` allows all Unity assemblies to reference it — should be removed when the project grows to avoid accidental coupling.

### Authoritative diagnostics
- `TestResults.xml` at project root — ground truth; `result="Passed" total="49" passed="49" failed="0"` is the green bar
- Static guard: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` — must return empty; any output is a regression
- No UnityEngine in Core: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` — must return empty
- GameBootstrapper Console trail — `[GameBootstrapper]` prefix lines during boot; last successful line before a failure identifies the broken phase
- `grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — must show `_inputBlocker?.Unblock()` inside finally at both navigation methods; absence means exception-safety invariant is broken

### What assumptions changed
- `com.unity.test-framework` is NOT in the Unity default project manifest — must be added manually for NUnit support.
- `-quit` is NOT compatible with `-runTests` batchmode — the test runner is async; `-quit` races it and prevents `TestResults.xml` from being written.
- `com.unity.ugui` is NOT auto-available in Unity 6 — must be declared in `Packages/manifest.json` and referenced in asmdefs.
- `FindObjectOfType` is deprecated in Unity 6 — use `FindFirstObjectByType<T>()`.
- Editor scripts that reference custom asmdef assemblies require their own explicit asmdef — they cannot rely on `Assembly-CSharp-Editor`.

## Files Created/Modified

- `Assets/Scripts/Core/MVP/IView.cs` — created: empty marker interface
- `Assets/Scripts/Core/MVP/Presenter.cs` — created: abstract generic base with Initialize()/Dispose() two-phase lifecycle
- `Assets/Scripts/Core/MVP/ISampleView.cs` — created: first view interface (event Action + UpdateLabel)
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — created: concrete presenter demonstrating constructor injection and event wiring
- `Assets/Scripts/Core/MVP/UIFactory.cs` — created (S01), modified (S05): expanded with callback-based constructor and 3 new Create methods; backward-compatible (GameService) overload preserved
- `Assets/Scripts/Core/MVP/IPopupView.cs` — created: IPopupView : IView marker interface
- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — created: view interface (OnSettingsClicked, OnPopupClicked, UpdateTitle)
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — created: view interface (OnBackClicked, UpdateTitle)
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — created: view interface extending IPopupView (OnConfirmClicked, OnCancelClicked, UpdateMessage)
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — created: presenter with Action<ScreenId> + Action<PopupId> callbacks
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — created: presenter with Func<UniTask> go-back callback
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — created: presenter with Func<UniTask> dismiss callback
- `Assets/Scripts/Core/Services/GameService.cs` — created: plain C# domain service with GetWelcomeMessage()
- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — created: ScreenId enum with MainMenu and Settings values
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — created: ISceneLoader abstraction with UniTask async methods
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — created (S02), modified (S04): optional ITransitionPlayer + IInputBlocker params; transition brackets with finally-block Unblock()
- `Assets/Scripts/Core/PopupManagement/PopupId.cs` — created: PopupId enum with ConfirmDialog value
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — created: reference-counting input blocker interface
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — created: async popup container interface
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — created: stack-based popup manager with concurrency guard (DismissAllAsync reference-counting fix applied)
- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — created: pure C# interface with FadeOutAsync/FadeInAsync
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — created: Unity SceneManager wrapper with additive loading
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — created: MonoBehaviour with CanvasGroup reference-counting
- `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` — created: MonoBehaviour implementing IPopupContainer via SetActive dispatch
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — created: MonoBehaviour with CanvasGroup alpha interpolation, blocksRaycasts=false at 6 points
- `Assets/Scripts/Runtime/MVP/MainMenuView.cs` — created: MonoBehaviour implementing IMainMenuView
- `Assets/Scripts/Runtime/MVP/SettingsView.cs` — created: MonoBehaviour implementing ISettingsView
- `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs` — created: MonoBehaviour implementing IConfirmDialogView
- `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` — created: boot scene initializer composing full dependency chain
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — created (S01), modified (S05): added "UnityEngine.UI" reference
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — created: test assembly referencing runtime, UniTask, test framework
- `Assets/Tests/EditMode/MVPWiringTests.cs` — created: 6 NUnit tests with MockSampleView
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — created: 8 NUnit tests with MockSceneLoader and BlockingMockSceneLoader
- `Assets/Tests/EditMode/PopupManagerTests.cs` — created: 13 NUnit tests with MockPopupContainer and MockInputBlocker
- `Assets/Tests/EditMode/TransitionTests.cs` — created: 5 NUnit tests with MergedLog helpers and ThrowingSceneLoader
- `Assets/Tests/EditMode/DemoWiringTests.cs` — created: 17 NUnit tests with MockMainMenuView, MockSettingsView, MockConfirmDialogView
- `Assets/Editor/SceneSetup.cs` — created (S02), modified (S05): full scene hierarchy construction with SerializedObject wiring + SceneSetupHelpers
- `Assets/Editor/SimpleGame.Editor.asmdef` — created: editor assembly referencing SimpleGame.Runtime and UnityEngine.UI
- `Assets/Scenes/Boot.unity` — created: Boot scene with GameBootstrapper, UnityInputBlocker, UnityTransitionPlayer, UnityPopupContainer, ConfirmDialogView
- `Assets/Scenes/MainMenu.unity` — created (S02, placeholder), modified (S05): populated with Canvas + MainMenuView + buttons + title
- `Assets/Scenes/Settings.unity` — created (S02, placeholder), modified (S05): populated with Canvas + SettingsView + back button + title
- `ProjectSettings/EditorBuildSettings.asset` — modified: m_Scenes populated with Boot(0), MainMenu(1), Settings(2)
- `Packages/manifest.json` — modified: UniTask git URL + com.unity.test-framework + com.unity.ugui
- `Packages/packages-lock.json` — auto-generated: confirms UniTask at commit ad5ed25e82a3
- `TestResults.xml` — final state: result="Passed" total="49" passed="49" failed="0"
- `.gitignore` — modified: Unity-standard ignore entries appended
- `.gsd/DECISIONS.md` — created: decisions 1–21 recorded
