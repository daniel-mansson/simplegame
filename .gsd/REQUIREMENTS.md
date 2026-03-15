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
- Validation: unmapped
- Notes: Presenters must not be MonoBehaviours. Views must not know about presenters or models.

### R002 — View independence (no backward refs to systems/services)
- Class: core-capability
- Status: active
- Description: Views function entirely on their own with no references to presenters, models, services, or any system outside their own interface. They expose events and methods, nothing more.
- Why it matters: Enables future view preview tool and guarantees testability. Views must work in complete isolation.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05
- Validation: unmapped
- Notes: This constraint is stricter than typical MVP — views don't even have a SetPresenter method.

### R003 — Interface-per-view for presenter dependency
- Class: core-capability
- Status: active
- Description: Each view exposes exactly one interface that its corresponding presenter depends on. The presenter never references the concrete view type.
- Why it matters: Enables mocking views in edit-mode tests and enforces the separation boundary.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: none
- Validation: unmapped
- Notes: Interface should contain events for user actions and methods for updating displayed state.

### R004 — Central UI factory for presenter construction
- Class: core-capability
- Status: active
- Description: One UIFactory class constructs all presenters. It receives all dependencies at its own construction and passes the correct ones to each presenter.
- Why it matters: Single wiring point — makes dependency flow explicit and traceable. No scattered "new Presenter()" calls.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05
- Validation: unmapped
- Notes: Factory receives view instances and services, constructs presenters with constructor/init injection.

### R005 — Constructor/init injection only (no DI framework)
- Class: constraint
- Status: active
- Description: All dependencies are passed via constructor parameters or explicit Init() methods. No service locator, no DI container, no reflection-based injection.
- Why it matters: Keeps wiring visible and debuggable. No magic — you can trace every dependency by reading the code.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04, M001/S05
- Validation: S01 — UIFactory + SamplePresenter wiring uses constructor injection throughout; verified by passing edit-mode tests; no DI framework in manifest or code
- Notes: None

### R006 — No static state (domain reload disabled support)
- Class: constraint
- Status: active
- Description: No static fields holding state anywhere in the project. Static utility methods are acceptable. The project must work correctly with Unity's "Enter Play Mode Settings" domain reload disabled.
- Why it matters: Domain reload disabled dramatically speeds up iteration in the editor. Static state breaks this because fields don't reset between play sessions.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04, M001/S05
- Validation: S01 — static guard passes with zero output; S02 — static guard passes for all Core/ScreenManagement/ files; no static fields introduced in ScreenId, ISceneLoader, ScreenManager, UnitySceneLoader, or SceneSetup; S03 — static guard passes for all Core/PopupManagement/ and Runtime/PopupManagement/ files; no static fields in PopupId, IInputBlocker, IPopupContainer, PopupManager, IPopupView, or UnityInputBlocker
- Notes: This rules out any singleton pattern that uses static Instance fields.

### R007 — Model layer with domain services/systems
- Class: core-capability
- Status: active
- Description: The model layer is not just passive data — it includes domain services/systems that encapsulate business logic. Presenters interact with these services to trigger state changes.
- Why it matters: Keeps presenters thin and domain logic reusable. Services modify model state; presenters react to the results.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05
- Validation: unmapped
- Notes: Similar to a domain logic layer. Services are plain C# classes injected into presenters.

### R008 — Boot scene → main scene initialization flow
- Class: launchability
- Status: active
- Description: A dedicated boot scene handles initialization (wiring dependencies, creating services), then transitions to the first real screen via the screen manager.
- Why it matters: Clean separation of bootstrap vs runtime. Boot scene is the single entry point where all wiring happens.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: M001/S02
- Validation: unmapped
- Notes: Boot scene should be the only scene that needs to be open in the editor to enter play mode.

### R009 — Hybrid scene management (persistent + additive scenes)
- Class: core-capability
- Status: active
- Description: One persistent scene stays loaded. Screen scenes are loaded additively and unloaded when navigating away.
- Why it matters: Persistent scene holds shared UI (popups, transitions, input blocker). Additive loading gives screen isolation without losing shared state.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S05
- Validation: S02 — UnitySceneLoader wraps LoadSceneAsync(name, LoadSceneMode.Additive) and UnloadSceneAsync; load/unload sequencing proven by ShowScreenAsync_UnloadsPreviousBeforeLoadingNext test; MainMenu.unity and Settings.unity registered in EditorBuildSettings; runtime integration with persistent scene deferred to S05
- Notes: The persistent scene hosts the screen manager, popup layer, transition overlay, and input blocker.

### R010 — Screen navigation between full screens
- Class: primary-user-loop
- Status: active
- Description: Navigate between full-screen views via a screen manager. Each screen is an additively loaded scene with its own view hierarchy.
- Why it matters: Core user loop — moving between screens is what the app does.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S04, M001/S05
- Validation: S02 — ScreenManager.ShowScreenAsync and GoBackAsync with Stack<ScreenId> history proven by 8 edit-mode tests; ScreenId enum provides type-safe identification; CurrentScreen and CanGoBack properties ready for presenter consumption; runtime presenter wiring deferred to S05
- Notes: Screen manager should support forward navigation and back navigation.

### R011 — Stack-based popup system
- Class: core-capability
- Status: active
- Description: Popups stack on top of each other. Most recent popup gets focus. Dismissing reveals the one below. Popups are views with presenters, following the same MVP pattern.
- Why it matters: Modal dialogs, confirmations, settings overlays — popups are everywhere in UI apps.
- Source: user
- Primary owning slice: M001/S03
- Supporting slices: M001/S05
- Validation: S03 — PopupManager with Stack<PopupId>; push/pop/dismiss-all; concurrency guard; HasActivePopup/TopPopup/PopupCount properties; 5 dedicated tests pass; TestResults.xml total="27" passed="27" failed="0"
- Notes: Popup views live in the persistent scene's popup layer, not in screen scenes.

### R012 — Full-screen raycast input blocker
- Class: core-capability
- Status: active
- Description: A full-screen transparent overlay that blocks all UI raycasts when active. Used during transitions, scene loading, and popup animations.
- Why it matters: Prevents user interaction during state changes that could cause race conditions or broken state.
- Source: user
- Primary owning slice: M001/S03
- Supporting slices: M001/S04
- Validation: S03 — IInputBlocker interface with reference-counting contract; UnityInputBlocker MonoBehaviour with CanvasGroup blocksRaycasts toggle; MockInputBlocker verifies reference-counting in 3 dedicated tests; integration with PopupManager proven by show/dismiss/dismiss-all tests
- Notes: Uses a CanvasGroup with blocksRaycasts on a high-sort-order canvas.

### R013 — Fade transitions between screens
- Class: quality-attribute
- Status: validated
- Description: Smooth fade-to-black (or similar) transition plays when navigating between screens. Input is blocked during transitions.
- Why it matters: Screens don't pop in/out — transitions give visual continuity and cover scene loading.
- Source: user
- Primary owning slice: M001/S04
- Supporting slices: M001/S05
- Validation: S04 — ITransitionPlayer pure C# interface with FadeOutAsync/FadeInAsync; ScreenManager orchestration sequence proven (Block → FadeOut → unload → load → FadeIn → Unblock in finally); input blocked for full duration; GoBack plays same sequence; null player preserves original behavior; exception safety proven; UnityTransitionPlayer MonoBehaviour with CanvasGroup alpha interpolation ready for S05 wiring; 32/32 tests pass
- Notes: Fade overlay in persistent scene. Transition flow: block input → fade out → unload old scene → load new scene → fade in → unblock input. Runtime visual integration deferred to S05.

### R014 — UniTask async/await for async operations
- Class: constraint
- Status: active
- Description: All asynchronous operations (scene loading, transitions, popup animations, async initialization) use Cysharp.Threading.Tasks (UniTask) with proper CancellationToken support.
- Why it matters: Zero-allocation async, clean cancellation, native Unity integration. No coroutine spaghetti.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04
- Validation: S01 — UniTask resolved at ad5ed25e82a3; compiles with zero errors; S02 — ISceneLoader returns UniTask; ScreenManager uses UniTask; UnitySceneLoader uses .ToUniTask(ct); MockSceneLoader returns UniTask.CompletedTask confirming interface contract; CancellationToken threaded through UnitySceneLoader calls; S03 — IPopupContainer uses UniTask+CancellationToken; PopupManager async methods; MockPopupContainer returns UniTask.CompletedTask
- Notes: Install via UPM git URL.

### R015 — Edit-mode unit tests for presenters and core logic
- Class: quality-attribute
- Status: active
- Description: Presenters, services, screen manager logic, and factory are tested with edit-mode (non-play-mode) unit tests. Views can be play-mode tested.
- Why it matters: Fast feedback loop. Edit-mode tests run in milliseconds without entering play mode.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03
- Validation: S01 — 6 MVPWiringTests pass; S02 — 8 ScreenManagerTests added; total 14/14 pass in batchmode; ScreenManager fully exercised without Unity runtime via MockSceneLoader; S03 — 13 PopupManagerTests added; total 27/27 pass in batchmode; PopupManager fully exercised without Unity runtime via MockPopupContainer and MockInputBlocker
- Notes: Mocked view interfaces enable presenter testing without Unity runtime.

### R016 — Demo screens proving end-to-end dependency flow
- Class: launchability
- Status: active
- Description: 2-3 example screens (MainMenu, Settings, game placeholder) with working navigation, a popup, and transitions. Dependencies flow correctly from boot through factory to presenters.
- Why it matters: Proves the architecture works in practice, not just in tests. The demo is the proof.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: none
- Validation: unmapped
- Notes: Expected outcome: navigate between screens with a basic demo setup that passes dependencies correctly across the app.

### R017 — Each layer testable in isolation
- Class: quality-attribute
- Status: active
- Description: Views can be tested without presenters. Presenters can be tested without real views (via interface mocks). Models/services can be tested without either. No layer requires another to function in tests.
- Why it matters: Isolation testing catches coupling violations early and keeps tests fast.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03
- Validation: S01 — MockSampleView in pure C#; all 6 presenter tests run without Unity runtime; S02 — MockSceneLoader and BlockingMockSceneLoader enable full ScreenManager testing with zero Unity dependency; ISceneLoader has no UnityEngine using; all 8 ScreenManager tests run in edit-mode; S03 — MockPopupContainer + MockInputBlocker enable full PopupManager testing with zero Unity dependency; IInputBlocker and IPopupContainer have no UnityEngine using; all 13 PopupManager tests run in edit-mode
- Notes: This is the litmus test for whether the MVP separation is actually working.

## Validated

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

### R003 — Interface-per-view for presenter dependency
- Class: core-capability
- Status: validated
- Validated by: S01 — ISampleView + SamplePresenter; `MockViewHasNoPresenterReference` test passes via reflection; presenter depends only on the interface
- Proof: TestResults.xml total="6" passed="6" failed="0"; ISampleView has no presenter/service types (grep verified)

### R005 — Constructor/init injection only (no DI framework)
- Class: constraint
- Status: validated
- Validated by: S01 — UIFactory constructor injection + SamplePresenter constructor injection; `UIFactoryCreatesSamplePresenterWithService` and `PresenterInitializeSetsWelcomeLabel` tests pass
- Proof: TestResults.xml all passing; no DI framework packages in manifest.json; no service locator pattern in any Core file

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

## Traceability

| ID | Class | Status | Primary owner | Supporting | Proof |
|---|---|---|---|---|---|
| R001 | core-capability | active | M001/S01 | M001/S02, S03, S04, S05 | S01 — IView, Presenter<TView>, UIFactory, ISampleView, SamplePresenter defined; compile clean |
| R002 | core-capability | active | M001/S01 | M001/S05 | S01 — ISampleView grep clean; MockViewHasNoPresenterReference test passes |
| R003 | core-capability | validated | M001/S01 | none | S01 — 6/6 tests pass; interface-only dependency proven |
| R004 | core-capability | active | M001/S01 | M001/S05 | S01 — UIFactory.CreateSamplePresenter() is the single wiring point; no scattered new SamplePresenter() |
| R005 | constraint | validated | M001/S01 | M001/S02, S03, S04, S05 | S01 — all 6 tests pass via constructor injection; no DI framework |
| R006 | constraint | active | M001/S01 | M001/S02, S03, S04, S05 | S01+S02 — static guard passes for all ScreenManagement core files; no static fields in any new S02 file |
| R007 | core-capability | active | M001/S01 | M001/S05 | S01 — GameService plain C# class injected into SamplePresenter |
| R008 | launchability | active | M001/S05 | M001/S02 | unmapped |
| R009 | core-capability | active | M001/S02 | M001/S05 | S02 — UnitySceneLoader uses LoadSceneMode.Additive; sequencing proven by 8 tests; scenes registered in EditorBuildSettings |
| R010 | primary-user-loop | active | M001/S02 | M001/S04, S05 | S02 — ShowScreenAsync + GoBackAsync + Stack<ScreenId> history; 8/8 tests pass; CurrentScreen + CanGoBack properties ready |
| R011 | core-capability | validated | M001/S03 | M001/S05 | S03 — PopupManager Stack<PopupId> push/pop/dismiss-all; 5 stack tests + 3 input-blocking tests pass; 27/27 total |
| R012 | core-capability | validated | M001/S03 | M001/S04 | S03 — IInputBlocker reference-counting contract; UnityInputBlocker CanvasGroup; 2 reference-counting tests + 4 integration tests pass |
| R013 | quality-attribute | validated | M001/S04 | M001/S05 | S04 — ITransitionPlayer + ScreenManager orchestration + UnityTransitionPlayer; 5 transition tests; 32/32 pass; static guard clean; finally-block Unblock confirmed |
| R014 | constraint | active | M001/S01 | M001/S02, S03, S04 | S01+S02+S03 — UniTask in ISceneLoader, ScreenManager, IPopupContainer, PopupManager; CancellationToken threaded; Mock*s return UniTask.CompletedTask |
| R015 | quality-attribute | active | M001/S01 | M001/S02, S03 | S01+S02+S03 — 27/27 edit-mode tests pass; PopupManager fully exercised without Unity runtime via MockPopupContainer and MockInputBlocker |
| R016 | launchability | active | M001/S05 | none | unmapped |
| R017 | quality-attribute | active | M001/S01 | M001/S02, S03 | S01+S02+S03 — MockSceneLoader + MockPopupContainer + MockInputBlocker; 27/27 tests run without Unity runtime; ISceneLoader, IInputBlocker, IPopupContainer have no UnityEngine |
| R018 | differentiator | deferred | none | none | unmapped |
| R019 | quality-attribute | deferred | none | none | unmapped |
| R020 | anti-feature | out-of-scope | none | none | n/a |
| R021 | constraint | out-of-scope | none | none | n/a |
| R022 | anti-feature | out-of-scope | none | none | n/a |

## Coverage Summary

- Active requirements: 17
- Mapped to slices: 17
- Validated: 5 (R003, R005, R011, R012, R013)
- Unmapped active requirements: 0
