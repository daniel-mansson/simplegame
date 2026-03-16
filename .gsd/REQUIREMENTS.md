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
- Supporting slices: M001/S02, M001/S03, M001/S04, M001/S05
- Validation: S05 — 3 View MonoBehaviours (MainMenuView, SettingsView, ConfirmDialogView) implement interfaces; 3 plain C# presenters use constructor injection; GameService flows through UIFactory; 49/49 edit-mode tests pass; no layer cross-references; pending play-mode UAT for full runtime proof
- Notes: Presenters must not be MonoBehaviours. Views must not know about presenters or models.

### R002 — View independence (no backward refs to systems/services)
- Class: core-capability
- Status: active
- Description: Views function entirely on their own with no references to presenters, models, services, or any system outside their own interface. They expose events and methods, nothing more.
- Why it matters: Enables future view preview tool and guarantees testability. Views must work in complete isolation.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05
- Validation: S05 — MockMainMenuViewHasNoPresenterReference, MockSettingsViewHasNoPresenterReference, MockConfirmDialogViewHasNoPresenterReference reflection tests pass; MainMenuView/SettingsView/ConfirmDialogView MonoBehaviours have zero non-Unity references; 49/49 tests pass
- Notes: This constraint is stricter than typical MVP — views don't even have a SetPresenter method.

### R004 — Central UI factory for presenter construction
- Class: core-capability
- Status: active
- Description: One UIFactory class constructs all presenters. It receives all dependencies at its own construction and passes the correct ones to each presenter.
- Why it matters: Single wiring point — makes dependency flow explicit and traceable. No scattered "new Presenter()" calls.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05
- Validation: S05 — UIFactory expanded with CreateMainMenuPresenter, CreateSettingsPresenter, CreateConfirmDialogPresenter; GameBootstrapper is the sole caller; 17 DemoWiringTests confirm all 3 factory Create methods; callback-based constructor proven; 49/49 tests pass

### R007 — Model layer with domain services/systems
- Class: core-capability
- Status: active
- Description: The model layer is not just passive data — it includes domain services/systems that encapsulate business logic. Presenters interact with these services to trigger state changes.
- Why it matters: Keeps presenters thin and domain logic reusable. Services modify model state; presenters react to the results.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05
- Validation: S05 — GameService constructed in GameBootstrapper and injected through UIFactory all the way to MainMenuPresenter via constructor injection; full dependency chain proven; 49/49 tests pass
- Notes: Similar to a domain logic layer. Services are plain C# classes injected into presenters.

### R008 — Boot scene → main scene initialization flow
- Class: launchability
- Status: active
- Description: A dedicated boot scene handles initialization (wiring dependencies, creating services), then transitions to the first real screen via the screen manager.
- Why it matters: Clean separation of bootstrap vs runtime. Boot scene is the single entry point where all wiring happens.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: M001/S02
- Validation: S05 — Boot.unity at EditorBuildSettings index 0; GameBootstrapper in Boot scene constructs GameService → managers → UIFactory → awaits ShowScreenAsync(MainMenu); full boot log trail emitted; batchmode compile + scene setup verified; pending play-mode UAT
- Notes: Boot scene should be the only scene that needs to be open in the editor to enter play mode.

### R009 — Hybrid scene management (persistent + additive scenes)
- Class: core-capability
- Status: active
- Description: One persistent scene stays loaded. Screen scenes are loaded additively and unloaded when navigating away.
- Why it matters: Persistent scene holds shared UI (popups, transitions, input blocker). Additive loading gives screen isolation without losing shared state.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S05
- Validation: S05 — Boot scene is persistent; MainMenu/Settings are additively loaded/unloaded by ScreenManager; GameBootstrapper wires infrastructure to persistent scene; InputBlocker, TransitionOverlay, PopupCanvas all in Boot scene; pending play-mode UAT for runtime proof
- Notes: The persistent scene hosts the screen manager, popup layer, transition overlay, and input blocker.

### R010 — Screen navigation between full screens
- Class: primary-user-loop
- Status: active
- Description: Navigate between full-screen views via a screen manager. Each screen is an additively loaded scene with its own view hierarchy.
- Why it matters: Core user loop — moving between screens is what the app does.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S04, M001/S05
- Validation: S05 — GameBootstrapper calls ShowScreenAsync(MainMenu) at boot; NavigateAndWirePresenter handles Settings; GoBackAndWirePresenter handles back; presenter disposed before each transition; presenter initialized after; 49/49 tests pass; pending play-mode UAT for runtime proof
- Notes: Screen manager should support forward navigation and back navigation.

### R014 — UniTask async/await for async operations
- Class: constraint
- Status: active
- Description: All asynchronous operations (scene loading, transitions, popup animations, async initialization) use Cysharp.Threading.Tasks (UniTask) with proper CancellationToken support.
- Why it matters: Zero-allocation async, clean cancellation, native Unity integration. No coroutine spaghetti.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04
- Validation: S01+S02+S03+S05 — UniTask in ISceneLoader, ScreenManager, IPopupContainer, PopupManager, GameBootstrapper (async UniTaskVoid Start), presenter callbacks (Func<UniTask>); CancellationToken threaded; Mock*s return UniTask.CompletedTask; 49/49 tests pass
- Notes: Install via UPM git URL.

### R016 — Demo screens proving end-to-end dependency flow
- Class: launchability
- Status: active
- Description: 2-3 example screens (MainMenu, Settings, game placeholder) with working navigation, a popup, and transitions. Dependencies flow correctly from boot through factory to presenters.
- Why it matters: Proves the architecture works in practice, not just in tests. The demo is the proof.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: none
- Validation: S05 — Boot.unity/MainMenu.unity/Settings.unity fully populated; GameBootstrapper wires full chain; MainMenu↔Settings navigation + ConfirmDialog popup implemented; 49/49 tests pass; pending play-mode UAT for runtime proof
- Notes: Expected outcome: navigate between screens with a basic demo setup that passes dependencies correctly across the app.

## Validated

### R003 — Interface-per-view for presenter dependency
- Class: core-capability
- Status: validated
- Validated by: M001 — ISampleView + SamplePresenter (S01); IMainMenuView, ISettingsView, IConfirmDialogView + corresponding presenters (S05); 3 mock-view reflection tests (MockXxxViewHasNoPresenterReference) pass; presenter depends only on the interface, never concrete view type
- Proof: TestResults.xml total="49" passed="49" failed="0"; all interface types grep-clean of presenter/service references

### R005 — Constructor/init injection only (no DI framework)
- Class: constraint
- Status: validated
- Validated by: S01 — UIFactory constructor injection + SamplePresenter constructor injection; `UIFactoryCreatesSamplePresenterWithService` and `PresenterInitializeSetsWelcomeLabel` tests pass; S02–S05 — all new types use constructor/optional-param injection consistently
- Proof: TestResults.xml total="49" passed="49" failed="0"; no DI framework packages in manifest.json; no service locator pattern in any Core file

### R006 — No static state (domain reload disabled support)
- Class: constraint
- Status: validated
- Validated by: M001 (all slices) — static guard `grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask"` returns empty; SceneSetupHelpers pattern resolves the only false-positive edge case
- Proof: grep command returns no output (exit 1, no matches) on final project state after all 5 slices complete

### R011 — Stack-based popup system
- Class: core-capability
- Status: validated
- Validated by: S03 — PopupManager with Stack<PopupId>; ShowPopupAsync/DismissPopupAsync/DismissAllAsync; concurrency guard; 5 dedicated stack/dismiss tests pass; TestResults.xml total="27" passed="27" failed="0"
- Proof: TestResults.xml total="27" passed="27" failed="0"; PopupManager.cs has no static state; grep confirms no UnityEngine in Core

### R012 — Full-screen raycast input blocker
- Class: core-capability
- Status: validated
- Validated by: S03 — IInputBlocker reference-counting interface; UnityInputBlocker MonoBehaviour with CanvasGroup; MockInputBlocker reference-counting proven by 2 dedicated tests; integration with PopupManager proven by show/dismiss/dismiss-all input-blocking tests
- Proof: TestResults.xml total="27" passed="27" failed="0"; InputBlocker_NestedBlockUnblock and InputBlocker_BlockUnblockBlock_Sequence pass; ShowPopupAsync_BlocksInput, DismissPopupAsync_UnblocksInputWhenStackEmpty, DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain, DismissAllAsync_ClearsEntireStack all pass

### R013 — Fade transitions between screens
- Class: quality-attribute
- Status: validated
- Validated by: S04 — ITransitionPlayer pure C# interface with FadeOutAsync/FadeInAsync; ScreenManager orchestration sequence (Block → FadeOut → unload → load → FadeIn → Unblock in finally) proven by 5 edit-mode tests; input blocked for full duration; GoBack plays same sequence; null player preserves original behavior; exception safety proven; UnityTransitionPlayer MonoBehaviour with CanvasGroup alpha interpolation; 32/32 tests pass
- Proof: TestResults.xml total="32" passed="32" failed="0"; static guard clean; no UnityEngine in Core/TransitionManagement; blocksRaycasts=false enforced at 6 points in UnityTransitionPlayer; finally block confirmed at ScreenManager lines 75 and 118

### R015 — Edit-mode unit tests for presenters and core logic
- Class: quality-attribute
- Status: validated
- Validated by: M001 — 49/49 edit-mode tests pass in Unity batchmode EditMode across all 5 slices; all presenter types, screen manager, popup manager, transition orchestration, and factory wiring covered; all run without Unity runtime
- Proof: TestResults.xml result="Passed" total="49" passed="49" failed="0" (final state after S05)

### R017 — Each layer testable in isolation
- Class: quality-attribute
- Status: validated
- Validated by: M001 — 6 MockView types (MockSampleView, MockSceneLoader, MockPopupContainer, MockInputBlocker, MockTransitionPlayer, MockMainMenuView, MockSettingsView, MockConfirmDialogView) in pure C#; all 49 tests run without Unity runtime; no IView subtype has UnityEngine imports; Core grep clean
- Proof: TestResults.xml total="49" passed="49" failed="0"; `grep -r "using UnityEngine" Assets/Scripts/Core/` → empty

## Deferred

### R018 — View preview tool
- Class: differentiator
- Status: deferred
- Description: A tool to preview individual views in isolation without any system/service wiring.
- Why it matters: Fast visual iteration on UI without running the full app.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Deferred — but view independence (R002) is designed to enable this. Future milestone.

### R019 — Play-mode tests for views
- Class: quality-attribute
- Status: deferred
- Description: Play-mode tests that verify view behavior (button clicks trigger events, text updates display correctly) in a running Unity environment.
- Why it matters: Views are MonoBehaviours — some behaviors can only be tested in play mode.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Deferred to keep M001 focused on architecture. Views are tested indirectly through demo screens.

## Out of Scope

### R020 — DI framework integration
- Class: anti-feature
- Status: out-of-scope
- Description: No Zenject, VContainer, or other DI framework. Explicit manual wiring only.
- Why it matters: Prevents accidental framework coupling. Keeps dependency flow visible in code.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: Explicit user decision — constructor/init injection only.

### R021 — UI Toolkit / UXML views
- Class: constraint
- Status: out-of-scope
- Description: Views use legacy uGUI (Canvas, GameObjects, Button/Text components), not UI Toolkit.
- Why it matters: Prevents scope confusion — uGUI is the chosen UI system.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: Explicit user decision.

### R022 — Static singletons or static state patterns
- Class: anti-feature
- Status: out-of-scope
- Description: No static fields holding state. No singleton pattern using static Instance. Static utility methods are acceptable.
- Why it matters: Project must support domain-reload-disabled mode. Static state breaks this.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: n/a
- Notes: Explicit user constraint — reinforces explicit wiring approach.

## M002 Requirements

### R023 — Core assembly is game-agnostic
- Class: constraint
- Status: active
- Description: `SimpleGame.Core` sources contain zero references to game-specific types (`IMainMenuView`, `ISettingsView`, `IConfirmDialogView`, `GameService`, `UIFactory`, `ScreenId`, `PopupId`). Core is reusable in any Unity project.
- Why it matters: The separation is meaningless if Core still knows about the game. Grep-verifiable.
- Source: user
- Primary owning slice: M002/S01
- Supporting slices: M002/S02
- Validation: unmapped
- Notes: Grep command: `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|GameService\|UIFactory\|ScreenId\|PopupId" Assets/Scripts/Core/` must return empty.

### R024 — Game code in dedicated assembly
- Class: constraint
- Status: active
- Description: All SimpleGame-specific code lives under `Assets/Scripts/Game/` in `SimpleGame.Game.asmdef`. The assembly references Core but Core does not reference Game.
- Why it matters: Enforces one-way dependency. Game depends on Core; Core is independent.
- Source: user
- Primary owning slice: M002/S02
- Supporting slices: none
- Validation: unmapped
- Notes: One-way asmdef reference: `SimpleGame.Game` → `SimpleGame.Core`. Not the reverse.

### R025 — Feature cohesion within Game
- Class: quality-attribute
- Status: active
- Description: Each game screen's interface, presenter, and view MonoBehaviour live in the same folder (e.g. `Game/MainMenu/` holds `IMainMenuView`, `MainMenuPresenter`, `MainMenuView`).
- Why it matters: Interface next to implementation — no jumping between Core/MVP and Runtime/MVP to understand one screen.
- Source: user
- Primary owning slice: M002/S02
- Supporting slices: none
- Validation: unmapped
- Notes: Cohesion over layers — group by feature, not by abstraction level.

### R026 — Test assemblies mirror source structure
- Class: quality-attribute
- Status: active
- Description: Two test assemblies: `SimpleGame.Tests.Core` (tests for framework types) and `SimpleGame.Tests.Game` (tests for game-specific types). `ISampleView`/`SamplePresenter` live in the Core test assembly as fixtures.
- Why it matters: Test structure should reflect source structure — makes it obvious where to add tests for new code.
- Source: user
- Primary owning slice: M002/S03
- Supporting slices: none
- Validation: unmapped
- Notes: `ISampleView` and `SamplePresenter` are test fixtures — they move out of runtime sources entirely.

### R027 — All 49 edit-mode tests pass after restructure
- Class: quality-attribute
- Status: active
- Description: The restructure is a pure rename/move — no behavior changes. All 49 existing tests must still pass after the restructure.
- Why it matters: Regression proof that the restructure didn't break anything.
- Source: inferred
- Primary owning slice: M002/S03
- Supporting slices: M002/S01, M002/S02
- Validation: unmapped
- Notes: TestResults.xml must show total="49" passed="49" failed="0" after M002 completes.

### R028 — ScreenManager and PopupManager are generic
- Class: core-capability
- Status: active
- Description: `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` use a type parameter for the ID type, removing any dependency on game-specific `ScreenId`/`PopupId` enums from Core.
- Why it matters: Required for Core to be truly game-agnostic. Without this, Core still references game enum values.
- Source: user
- Primary owning slice: M002/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Use `where TScreenId : System.Enum` constraint (C# 7.3+, supported in Unity 6). `ScreenId` and `PopupId` move to `SimpleGame.Game`.

## Traceability

| ID | Class | Status | Primary owner | Supporting | Proof |
|---|---|---|---|---|---|
| R001 | core-capability | active | M001/S01 | M001/S02, S03, S04, S05 | M001 — 3 view interfaces + 3 presenters + 3 view MonoBehaviours; 49/49 tests; static guard clean; no UnityEngine in Core; pending play-mode UAT |
| R002 | core-capability | active | M001/S01 | M001/S05 | M001 — 3 MockXxxViewHasNoPresenterReference reflection tests pass; view MonoBehaviours grep clean; pending UAT |
| R003 | core-capability | validated | M001/S01 | M001/S05 | M001 — ISampleView+SamplePresenter (S01); 3 additional mock-view reflection tests (S05); 49/49 tests pass |
| R004 | core-capability | active | M001/S01 | M001/S05 | M001 — UIFactory 4 Create methods; GameBootstrapper is sole caller; 17 DemoWiringTests pass; pending UAT |
| R005 | constraint | validated | M001/S01 | M001/S02, S03, S04, S05 | M001 — all 49 tests pass via constructor injection; no DI framework; pattern consistent across all 5 slices |
| R006 | constraint | validated | M001/S01 | M001/S02, S03, S04, S05 | M001 — static guard returns empty across all 5 slices; grep command produces no output |
| R007 | core-capability | active | M001/S01 | M001/S05 | M001 — GameService flows through full chain: Boot → UIFactory → MainMenuPresenter; pending UAT |
| R008 | launchability | active | M001/S05 | M001/S02 | M001 — Boot.unity at index 0; GameBootstrapper wires full chain; batchmode verified; pending UAT |
| R009 | core-capability | active | M001/S02 | M001/S05 | M001 — Boot persistent; MainMenu/Settings additive; all infrastructure in Boot scene; pending UAT |
| R010 | primary-user-loop | active | M001/S02 | M001/S04, S05 | M001 — ShowScreenAsync + GoBack + presenter lifecycle wired; 49/49 tests; pending UAT |
| R011 | core-capability | validated | M001/S03 | M001/S05 | S03 — PopupManager Stack<PopupId> push/pop/dismiss-all; 5 stack tests + 3 input-blocking tests; 27/27 |
| R012 | core-capability | validated | M001/S03 | M001/S04 | S03 — IInputBlocker reference-counting; UnityInputBlocker CanvasGroup; 2 ref-counting tests + 4 integration tests pass |
| R013 | quality-attribute | validated | M001/S04 | M001/S05 | S04 — ITransitionPlayer + ScreenManager orchestration + UnityTransitionPlayer; 5 transition tests; 32/32 pass |
| R014 | constraint | active | M001/S01 | M001/S02, S03, S04 | M001 — UniTask in all async paths; GameBootstrapper + Func<UniTask> callbacks; 49/49 tests pass |
| R015 | quality-attribute | validated | M001/S01 | M001/S02, S03 | M001 — 49/49 edit-mode tests pass: 6+8+13+5+17; TestResults.xml result="Passed" |
| R016 | launchability | active | M001/S05 | none | M001 — 3 scenes built; full chain from boot to presenter; 49/49 tests pass; pending UAT |
| R017 | quality-attribute | validated | M001/S01 | M001/S02, S03 | M001 — 8 mock test doubles in pure C#; all 49 tests run without Unity runtime; Core grep clean |
| R018 | differentiator | deferred | none | none | unmapped |
| R019 | quality-attribute | deferred | none | none | unmapped |
| R020 | anti-feature | out-of-scope | none | none | n/a |
| R021 | constraint | out-of-scope | none | none | n/a |
| R022 | anti-feature | out-of-scope | none | none | n/a |
| R023 | constraint | active | M002/S01 | M002/S02 | unmapped |
| R024 | constraint | active | M002/S02 | none | unmapped |
| R025 | quality-attribute | active | M002/S02 | none | unmapped |
| R026 | quality-attribute | active | M002/S03 | none | unmapped |
| R027 | quality-attribute | active | M002/S03 | M002/S01, S02 | unmapped |
| R028 | core-capability | active | M002/S01 | none | unmapped |

## Coverage Summary

- Total requirements: 28 (15 active + 8 validated + 2 deferred + 3 out of scope)
- Validated: 8 (R003, R005, R006, R011, R012, R013, R015, R017)
- Active (M001, batchmode-verified, pending play-mode UAT): R001, R002, R004, R007, R008, R009, R010, R014, R016
- Active (M002, unmapped — queued): R023, R024, R025, R026, R027, R028
- Deferred: R018, R019
- Out of scope: R020, R021, R022
- Unmapped active requirements: 6 (R023–R028, owned by M002)
