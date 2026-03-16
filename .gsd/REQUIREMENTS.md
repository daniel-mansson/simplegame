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
- Validation: M001 — 3 view interfaces + 3 presenters + 3 view MonoBehaviours; 49/49 tests; static guard clean; pending play-mode UAT
- Notes: Presenters must not be MonoBehaviours. Views must not know about presenters or models.

### R002 — View independence (no backward refs to systems/services)
- Class: core-capability
- Status: active
- Description: Views function entirely on their own with no references to presenters, models, services, or any system outside their own interface.
- Why it matters: Enables future view preview tool and guarantees testability.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05, M004/S02-S04
- Validation: M001 — 3 MockXxxViewHasNoPresenterReference reflection tests pass; view MonoBehaviours grep clean; pending UAT
- Notes: This constraint is stricter than typical MVP — views don't even have a SetPresenter method.

### R004 — Central UI factory for presenter construction
- Class: core-capability
- Status: active
- Description: One UIFactory class constructs all presenters. It receives all dependencies at its own construction and passes the correct ones to each presenter.
- Why it matters: Single wiring point — makes dependency flow explicit and traceable.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05, M004/S02-S04
- Validation: M001 — UIFactory 3 Create methods; GameBootstrapper sole caller; 17 DemoWiringTests pass; pending UAT
- Notes: M004 will extend UIFactory with new Create methods for InGame and popup presenters.

### R007 — Model layer with domain services/systems
- Class: core-capability
- Status: active
- Description: The model layer includes domain services/systems that encapsulate business logic. Presenters interact with these services.
- Why it matters: Keeps presenters thin and domain logic reusable.
- Source: user
- Primary owning slice: M001/S01
- Supporting slices: M001/S05, M004/S01
- Validation: M001 — GameService constructed in GameBootstrapper, injected through UIFactory; pending UAT
- Notes: M004 adds GameSessionService and ProgressionService as real domain services.

### R008 — Boot scene → main scene initialization flow
- Class: launchability
- Status: active
- Description: A dedicated boot scene handles initialization, then transitions to the first real screen.
- Why it matters: Clean separation of bootstrap vs runtime.
- Source: user
- Primary owning slice: M001/S05
- Supporting slices: M001/S02, M004/S05
- Validation: M001 — Boot.unity at index 0; GameBootstrapper wires full chain; pending UAT

### R009 — Hybrid scene management (persistent + additive scenes)
- Class: core-capability
- Status: active
- Description: One persistent scene stays loaded. Screen scenes are loaded additively.
- Why it matters: Persistent scene holds shared UI. Additive loading gives screen isolation.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S05, M004/S03
- Validation: M001 — Boot persistent; MainMenu/Settings additive; pending UAT

### R010 — Screen navigation between full screens
- Class: primary-user-loop
- Status: active
- Description: Navigate between full-screen views via a screen manager.
- Why it matters: Core user loop — moving between screens is what the app does.
- Source: user
- Primary owning slice: M001/S02
- Supporting slices: M001/S04, M001/S05, M004/S02, M004/S03
- Validation: M001 — ShowScreenAsync + GoBack + presenter lifecycle; 49/49 tests; pending UAT

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
- Validation: M001 — 3 scenes; full chain; 49/49 tests; pending UAT
- Notes: M004 upgrades the demo from navigation showcase to working game loop.

### R029 — Game session context via shared service
- Class: core-capability
- Status: active
- Description: A GameSessionService holds the current game context (level ID, accumulated score, game outcome). Scene controllers read from it to configure themselves. The service mediates context passing between scenes without changing the RunAsync signature.
- Why it matters: Keeps the ISceneController interface clean. Context flows through the service layer, not method parameters. Receiver scenes are responsible for reading what they need.
- Source: user
- Primary owning slice: M004/S01
- Supporting slices: M004/S02, M004/S03, M004/S04
- Validation: unmapped
- Notes: User explicitly chose service-mediated context over RunAsync params.

### R030 — Progression service with in-memory level tracking
- Class: core-capability
- Status: active
- Description: A ProgressionService tracks the player's current level (starting at 1). On win, it advances the level and logs the score. State is in-memory only — resets on app restart.
- Why it matters: Proves domain services can own game state and mutate it based on gameplay outcomes. The main menu reads from this service to display current progress.
- Source: user
- Primary owning slice: M004/S01
- Supporting slices: M004/S02, M004/S03
- Validation: unmapped
- Notes: No disk persistence — explicitly deferred by user.

### R031 — Main menu displays current level and has Play button
- Class: primary-user-loop
- Status: active
- Description: The main menu shows the current level from ProgressionService and has a Play button that sets the session context (level ID) and navigates to InGame.
- Why it matters: The main menu is the hub — it must reflect game state and initiate gameplay.
- Source: user
- Primary owning slice: M004/S02
- Supporting slices: none
- Validation: unmapped

### R032 — InGame scene receives level ID and is self-sufficient
- Class: core-capability
- Status: active
- Description: The InGame scene controller reads the level ID from GameSessionService and configures the scene accordingly. It is responsible for loading/setting up everything needed for that level.
- Why it matters: Proves the context-passing pattern and scene self-sufficiency.
- Source: user
- Primary owning slice: M004/S03
- Supporting slices: none
- Validation: unmapped
- Notes: For this milestone, "configuring based on level" means displaying the level ID — not loading different assets.

### R033 — InGame gameplay — score counter + win/lose triggers
- Class: primary-user-loop
- Status: active
- Description: InGame scene has a score display, a button to increment the score, a Win button, and a Lose button.
- Why it matters: Minimal but complete gameplay loop — user can take actions and trigger outcomes.
- Source: user
- Primary owning slice: M004/S03
- Supporting slices: none
- Validation: unmapped

### R034 — Win popup with score + level, returns to main menu
- Class: primary-user-loop
- Status: active
- Description: Winning triggers a win popup that displays the achieved score and level completed. A continue button returns the player to the main menu. The progression service registers the win before the popup shows.
- Why it matters: Proves outcome → service → UI feedback → navigation chain.
- Source: user
- Primary owning slice: M004/S04
- Supporting slices: M004/S03
- Validation: unmapped

### R035 — Lose popup with score + level, retry/back options
- Class: primary-user-loop
- Status: active
- Description: Losing triggers a lose popup showing score and level. Retry resets the score and replays the same level. Back returns to the main menu without advancing.
- Why it matters: Proves branching outcome flow — retry loops within the scene, back navigates out.
- Source: user
- Primary owning slice: M004/S04
- Supporting slices: M004/S03
- Validation: unmapped

### R036 — Progression service logs score on win and advances level
- Class: core-capability
- Status: active
- Description: When the player wins, the progression service receives the score, logs it (Debug.Log), and advances the current level counter.
- Why it matters: Domain logic lives in the service, not in the presenter or scene controller.
- Source: user
- Primary owning slice: M004/S03
- Supporting slices: M004/S01
- Validation: unmapped

### R037 — Play-from-editor bootstrapping for InGame scene
- Class: launchability
- Status: active
- Description: InGame scene works when played directly from the editor. The scene controller has a fallback/default configuration (e.g. a serialized field for default level ID) so it can bootstrap without the full menu flow.
- Why it matters: Developer workflow — shouldn't have to navigate from menu every time to test InGame.
- Source: user
- Primary owning slice: M004/S03
- Supporting slices: none
- Validation: unmapped
- Notes: Keep it simple — serialized field on scene controller for default level.

### R038 — Full game loop — menu → play → outcome → menu reflects progress
- Class: primary-user-loop
- Status: active
- Description: Complete loop works end-to-end: main menu shows level → play → score up → win or lose → outcome popup → menu reflects new state.
- Why it matters: The milestone's reason for existing — proving the architecture carries a real game loop.
- Source: inferred
- Primary owning slice: M004/S05
- Supporting slices: M004/S01-S04
- Validation: unmapped

### R039 — New popup types (WinDialog, LoseDialog) with distinct views/presenters
- Class: core-capability
- Status: active
- Description: WinDialog and LoseDialog are new PopupId entries with their own view interfaces, view MonoBehaviours, and presenters. They are distinct from ConfirmDialog — not reused.
- Why it matters: Win/lose will look and behave very differently from generic confirmations. User explicitly chose new popups for clearly different dialogs.
- Source: user
- Primary owning slice: M004/S04
- Supporting slices: none
- Validation: unmapped

### R040 — Edit-mode tests for new presenters, services, and scene controllers
- Class: quality-attribute
- Status: active
- Description: All new types (services, presenters, scene controllers) get edit-mode unit tests following the established pattern — mock views, constructor injection, no Unity runtime.
- Why it matters: Continues the quality bar established in M001-M003.
- Source: inferred
- Primary owning slice: M004/S01
- Supporting slices: M004/S02, M004/S03, M004/S04, M004/S05
- Validation: unmapped

## Validated

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
- Notes: View independence (R002) enables this. Future milestone.

### R019 — Play-mode tests for views
- Class: quality-attribute
- Status: deferred
- Description: Play-mode tests that verify view behavior in a running Unity environment.
- Why it matters: Views are MonoBehaviours — some behaviors can only be tested in play mode.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped

### R041 — Disk persistence for progression
- Class: continuity
- Status: deferred
- Description: Level progress persists across app restarts (PlayerPrefs or JSON save).
- Why it matters: Real games need persistence. Currently in-memory only.
- Source: user
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: User explicitly chose in-memory for M004. Persistence is a future milestone.

### R042 — Level-specific content/config loading from data
- Class: core-capability
- Status: deferred
- Description: InGame scene loads different content based on level data (enemies, layouts, difficulty curves).
- Why it matters: Real games have varied levels. Currently level ID is cosmetic.
- Source: inferred
- Primary owning slice: none
- Supporting slices: none
- Validation: unmapped
- Notes: Architecture supports it — InGame reads level ID from service. Real content loading deferred.

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
- Description: No physics, enemies, timers, or complex gameplay. Score button + win/lose buttons only.
- Why it matters: The milestone proves the architecture loop, not gameplay depth.
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
| R029 | core-capability | active | M004/S01 | M004/S02-S04 | unmapped |
| R030 | core-capability | active | M004/S01 | M004/S02-S03 | unmapped |
| R031 | primary-user-loop | active | M004/S02 | none | unmapped |
| R032 | core-capability | active | M004/S03 | none | unmapped |
| R033 | primary-user-loop | active | M004/S03 | none | unmapped |
| R034 | primary-user-loop | active | M004/S04 | M004/S03 | unmapped |
| R035 | primary-user-loop | active | M004/S04 | M004/S03 | unmapped |
| R036 | core-capability | active | M004/S03 | M004/S01 | unmapped |
| R037 | launchability | active | M004/S03 | none | unmapped |
| R038 | primary-user-loop | active | M004/S05 | M004/S01-S04 | unmapped |
| R039 | core-capability | active | M004/S04 | none | unmapped |
| R040 | quality-attribute | active | M004/S01 | M004/S02-S05 | unmapped |
| R041 | continuity | deferred | none | none | unmapped |
| R042 | core-capability | deferred | none | none | unmapped |
| R043 | anti-feature | out-of-scope | none | none | n/a |

## Coverage Summary

- Total requirements: 43
- Active: 21
- Validated: 14
- Deferred: 4
- Out of scope: 4
- Unmapped active requirements: 0
