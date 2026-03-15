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
- Validation: unmapped
- Notes: None

### R006 — No static state (domain reload disabled support)
- Class: constraint
- Status: active
- Description: No static fields holding state anywhere in the project. Static utility methods are acceptable. The project must work correctly with Unity's "Enter Play Mode Settings" domain reload disabled.
- Why it matters: Domain reload disabled dramatically speeds up iteration in the editor. Static state breaks this because fields don't reset between play sessions.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04, M001/S05
- Validation: unmapped
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
- Validation: unmapped
- Notes: The persistent scene hosts the screen manager, popup layer, transition overlay, and input blocker.

### R010 — Screen navigation between full screens
- Class: primary-user-loop
- Status: active
- Description: Navigate between full-screen views via a screen manager. Each screen is an additively loaded scene with its own view hierarchy.
- Why it matters: Core user loop — moving between screens is what the app does.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S04, M001/S05
- Validation: unmapped
- Notes: Screen manager should support forward navigation and back navigation.

### R011 — Stack-based popup system
- Class: core-capability
- Status: active
- Description: Popups stack on top of each other. Most recent popup gets focus. Dismissing reveals the one below. Popups are views with presenters, following the same MVP pattern.
- Why it matters: Modal dialogs, confirmations, settings overlays — popups are everywhere in UI apps.
- Source: user
- Primary owning slice: M001/S03
- Supporting slices: M001/S05
- Validation: unmapped
- Notes: Popup views live in the persistent scene's popup layer, not in screen scenes.

### R012 — Full-screen raycast input blocker
- Class: core-capability
- Status: active
- Description: A full-screen transparent overlay that blocks all UI raycasts when active. Used during transitions, scene loading, and popup animations.
- Why it matters: Prevents user interaction during state changes that could cause race conditions or broken state.
- Source: user
- Primary owning slice: M001/S03
- Supporting slices: M001/S04
- Validation: unmapped
- Notes: Uses a CanvasGroup with blocksRaycasts on a high-sort-order canvas.

### R013 — Fade transitions between screens
- Class: quality-attribute
- Status: active
- Description: Smooth fade-to-black (or similar) transition plays when navigating between screens. Input is blocked during transitions.
- Why it matters: Screens don't pop in/out — transitions give visual continuity and cover scene loading.
- Source: user
- Primary owning slice: M001/S04
- Supporting slices: M001/S05
- Validation: unmapped
- Notes: Fade overlay in persistent scene. Transition flow: block input → fade out → unload old scene → load new scene → fade in → unblock input.

### R014 — UniTask async/await for async operations
- Class: constraint
- Status: active
- Description: All asynchronous operations (scene loading, transitions, popup animations, async initialization) use Cysharp.Threading.Tasks (UniTask) with proper CancellationToken support.
- Why it matters: Zero-allocation async, clean cancellation, native Unity integration. No coroutine spaghetti.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03, M001/S04
- Validation: unmapped
- Notes: Install via UPM git URL.

### R015 — Edit-mode unit tests for presenters and core logic
- Class: quality-attribute
- Status: active
- Description: Presenters, services, screen manager logic, and factory are tested with edit-mode (non-play-mode) unit tests. Views can be play-mode tested.
- Why it matters: Fast feedback loop. Edit-mode tests run in milliseconds without entering play mode.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S02, M001/S03
- Validation: unmapped
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
- Validation: unmapped
- Notes: This is the litmus test for whether the MVP separation is actually working.

## Validated

(none yet)

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
| R001 | core-capability | active | M001/S01 | M001/S02, S03, S04, S05 | unmapped |
| R002 | core-capability | active | M001/S01 | M001/S05 | unmapped |
| R003 | core-capability | active | M001/S01 | none | unmapped |
| R004 | core-capability | active | M001/S01 | M001/S05 | unmapped |
| R005 | constraint | active | M001/S01 | M001/S02, S03, S04, S05 | unmapped |
| R006 | constraint | active | M001/S01 | M001/S02, S03, S04, S05 | unmapped |
| R007 | core-capability | active | M001/S01 | M001/S05 | unmapped |
| R008 | launchability | active | M001/S05 | M001/S02 | unmapped |
| R009 | core-capability | active | M001/S02 | M001/S05 | unmapped |
| R010 | primary-user-loop | active | M001/S02 | M001/S04, S05 | unmapped |
| R011 | core-capability | active | M001/S03 | M001/S05 | unmapped |
| R012 | core-capability | active | M001/S03 | M001/S04 | unmapped |
| R013 | quality-attribute | active | M001/S04 | M001/S05 | unmapped |
| R014 | constraint | active | M001/S01 | M001/S02, S03, S04 | unmapped |
| R015 | quality-attribute | active | M001/S01 | M001/S02, S03 | unmapped |
| R016 | launchability | active | M001/S05 | none | unmapped |
| R017 | quality-attribute | active | M001/S01 | M001/S02, S03 | unmapped |
| R018 | differentiator | deferred | none | none | unmapped |
| R019 | quality-attribute | deferred | none | none | unmapped |
| R020 | anti-feature | out-of-scope | none | none | n/a |
| R021 | constraint | out-of-scope | none | none | n/a |
| R022 | anti-feature | out-of-scope | none | none | n/a |

## Coverage Summary

- Active requirements: 17
- Mapped to slices: 17
- Validated: 0
- Unmapped active requirements: 0
