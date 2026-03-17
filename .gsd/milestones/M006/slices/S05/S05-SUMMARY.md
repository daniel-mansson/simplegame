---
id: S05
milestone: M006
provides:
  - Reworked MainMenu → meta world main screen with environment, objects, balance, play
  - MainMenuPresenter with MetaProgressionService + GoldenPieceService integration
  - ObjectDisplayData struct for presenter→view data transfer
  - ObjectRestored popup (IObjectRestoredView + ObjectRestoredPresenter)
  - MainMenuSceneController with meta progression and ObjectRestored popup flow
  - UIFactory extended with full service injection
  - Reworked DemoWiringTests and SceneControllerTests
  - 4 ObjectRestored popup tests
key_files:
  - Assets/Scripts/Game/MainMenu/IMainMenuView.cs
  - Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs
  - Assets/Scripts/Game/MainMenu/MainMenuAction.cs
  - Assets/Scripts/Game/MainMenu/ObjectDisplayData.cs
  - Assets/Scripts/Game/MainMenu/MainMenuView.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Scripts/Game/Popup/ObjectRestoredPresenter.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Tests/EditMode/Game/DemoWiringTests.cs
  - Assets/Tests/EditMode/Game/SceneControllerTests.cs
  - Assets/Tests/EditMode/Game/PopupTests.cs
key_decisions:
  - "MainMenuPresenter takes EnvironmentData to display — controller determines which environment"
  - "ObjectDisplayData struct decouples presenter from view (view has no service refs)"
  - "Removed demo Popup action from main menu — replaced with real ObjectRestored flow"
  - "UIFactory takes all services as optional params for backward compatibility"
patterns_established:
  - "Presenter→view data transfer via struct array (ObjectDisplayData[])"
  - "Controller determines current environment, passes to presenter at construction"
drill_down_paths:
  - .gsd/milestones/M006/slices/S05/S05-PLAN.md
verification_result: pass
completed_at: 2026-03-17T14:15:00Z
---

# S05: Main screen with meta world

**Reworked MainMenu into meta world main screen with environment display, tap-to-restore objects, golden piece balance, ObjectRestored popup, and comprehensive tests**

## What Happened

Major rework of the main menu screen to become the meta world hub:

**MainMenuPresenter**: Now takes MetaProgressionService, IGoldenPieceService, ProgressionService, GameSessionService, and EnvironmentData. Shows environment name, object list with progress/blocked/complete state, golden piece balance, and level display. HandleObjectTapped validates (not blocked, not complete, sufficient balance), spends golden pieces, restores one step, persists both services, refreshes view. Resolves ObjectRestored action when an object completes.

**ObjectDisplayData**: New struct for presenter→view data transfer (name, progress string, isBlocked, isComplete, costPerStep). Keeps views free of service references.

**ObjectRestored popup**: IObjectRestoredView + ObjectRestoredPresenter — shows "X Restored!" and waits for continue.

**MainMenuSceneController**: Injects MetaProgressionService, finds current environment (first non-complete), passes to presenter. Handles ObjectRestored action by showing popup then refreshing.

**UIFactory**: Extended with MetaProgressionService + IGoldenPieceService parameters (optional for backward compat). CreateMainMenuPresenter now takes EnvironmentData.

**Tests**: DemoWiringTests fully rewritten (25 tests) with ScriptableObject test data, meta service mocks, object tapping tests. SceneControllerTests simplified (4 tests). ObjectRestoredPresenter tests (4 tests).

## Tasks Completed
- T01: Rework MainMenu presenter and view interface for meta world
- T02: ObjectRestored popup, SceneController update, and tests
