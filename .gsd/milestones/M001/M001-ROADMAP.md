# M001: MVP UI Architecture Foundation

**Vision:** Establish a clean, testable MVP-based UI architecture in Unity 6 with screen management, popups, transitions, and a demo proving the full dependency chain — all without static state or DI frameworks.

## Success Criteria

- User can enter play mode from boot scene, navigate Main Menu → Settings → Main Menu with fade transitions
- A stack-based popup can be opened over any screen, blocks input below, and dismisses cleanly
- Input is blocked during all transitions and scene loads
- No static fields holding state exist in the codebase
- Every dependency is traceable from boot to presenter via constructor/init injection
- Edit-mode tests verify presenter construction, screen manager, popup stack, and factory wiring in isolation
- Views have no references to presenters, models, or services — only expose interfaces

## Key Risks / Unknowns

- MVP wiring pattern — if the factory/presenter/view-interface pattern is awkward, everything downstream is painful
- Hybrid scene lifecycle — additive loading with persistent scene needs careful ownership and cleanup
- UniTask setup — first integration, need to verify UPM compatibility with Unity 6000.3.10f1

## Proof Strategy

- MVP wiring pattern → retire in S01 by proving a presenter can be constructed from factory with a mocked view interface and injected model, verified by edit-mode test
- Hybrid scene lifecycle → retire in S02 by proving screen navigation loads/unloads additive scenes correctly with persistent scene surviving
- UniTask setup → retire in S01 by proving UniTask installs and compiles in the project

## Verification Classes

- Contract verification: edit-mode tests for presenters, factory, screen manager, popup manager
- Integration verification: play-mode boot → navigate → popup → dismiss flow in Unity editor
- Operational verification: none (editor-only project)
- UAT / human verification: manual walkthrough of demo screens with transitions and popups

## Milestone Definition of Done

This milestone is complete only when all are true:

- All five slices are complete with their individual verification passing
- Boot scene initializes services, constructs factory, transitions to main menu
- Screen navigation works between at least 2 screens with fade transitions
- Popup opens, stacks, blocks input, and dismisses
- `grep -r "static " --include="*.cs" | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` finds no static state fields
- Edit-mode tests all pass
- Final integrated acceptance scenarios verified in play mode

## Requirement Coverage

- Covers: R001, R002, R003, R004, R005, R006, R007, R008, R009, R010, R011, R012, R013, R014, R015, R016, R017
- Partially covers: none
- Leaves for later: R018 (view preview tool), R019 (play-mode tests)
- Orphan risks: none

## Slices

- [x] **S01: Core MVP Infrastructure & Project Setup** `risk:high` `depends:[]`
  > After this: Unity project exists with UniTask installed, MVP base types defined (IView, Presenter, UIFactory), and edit-mode tests prove a presenter can be constructed with a mocked view interface and injected service — verified by passing tests.

- [x] **S02: Screen Management** `risk:medium` `depends:[S01]`
  > After this: ScreenManager navigates between screens using additive scene loading. Two placeholder screen scenes can be loaded/unloaded. Verified by edit-mode tests for navigation logic and play-mode walkthrough showing scenes loading/unloading.

- [x] **S03: Popup System & Input Blocking** `risk:medium` `depends:[S01]`
  > After this: PopupManager opens/dismisses stack-based popups over the current screen. InputBlocker prevents interaction when active. Verified by edit-mode tests for popup stack logic and play-mode walkthrough showing popup stacking and input blocking.

- [ ] **S04: Transition System** `risk:low` `depends:[S02]`
  > After this: TransitionManager plays a fade-to-black transition during screen navigation. Input is blocked during the transition. Verified by play-mode walkthrough showing smooth fade between screens.

- [ ] **S05: Boot Flow & Demo Screens** `risk:low` `depends:[S01,S02,S03,S04]`
  > After this: Boot scene initializes all services and the UI factory, transitions to MainMenu, user navigates to Settings and back, opens a popup — full dependency chain flows correctly. This is the final integration proof.

## Boundary Map

### S01 → S02

Produces:
- `IView` — marker interface that all view interfaces extend
- `Presenter<TView>` — base presenter class that takes a view interface in constructor
- `UIFactory` — central factory that constructs presenters with wired dependencies
- `IScreenView` — interface pattern for screen-level views (events + update methods)
- UniTask installed and configured in the project

Consumes:
- nothing (first slice)

### S01 → S03

Produces:
- `IView` — marker interface
- `Presenter<TView>` — base presenter
- `UIFactory` — factory for constructing popup presenters
- `IPopupView` — interface pattern for popup views

Consumes:
- nothing (first slice)

### S02 → S04

Produces:
- `ScreenManager` — manages screen navigation with async methods (ShowScreen, GoBack)
- Screen lifecycle hooks (OnScreenShow, OnScreenHide) that the transition system can hook into

Consumes from S01:
- `Presenter<TView>`, `UIFactory`, `IView`

### S01, S03 → S04

Produces from S03:
- `InputBlocker` — blocks/unblocks UI input (used during transitions)

Consumes from S01:
- UniTask for async fade operations

### S01, S02, S03, S04 → S05

Produces from S02:
- `ScreenManager` — screen navigation
Produces from S03:
- `PopupManager` — popup management
- `InputBlocker` — input blocking
Produces from S04:
- `TransitionManager` — fade transitions

Consumes from S01:
- `UIFactory` — to construct all demo presenters
- MVP base types — to implement demo screens and popups
