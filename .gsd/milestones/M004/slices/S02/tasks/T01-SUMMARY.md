---
id: T01
parent: S02
milestone: M004
provides:
  - MainMenuAction.Play in enum
  - ScreenId.InGame in enum
  - IMainMenuView extended with OnPlayClicked + UpdateLevelDisplay
  - MainMenuPresenter accepts ProgressionService + GameSessionService, handles Play (sets session context)
  - UIFactory updated to pass services to CreateMainMenuPresenter
  - MainMenuSceneController maps Play → ScreenId.InGame
  - MainMenuView MonoBehaviour extended with Play button + level text fields
requires:
  - slice: S01
    provides: GameSessionService, ProgressionService
affects: [S03, S04, S05]
key_files:
  - Assets/Scripts/Game/MainMenu/MainMenuAction.cs
  - Assets/Scripts/Game/MainMenu/IMainMenuView.cs
  - Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs
  - Assets/Scripts/Game/MainMenu/MainMenuView.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Scripts/Game/ScreenId.cs
key_decisions:
  - "MainMenuPresenter calls GameSessionService.ResetForNewGame(progression.CurrentLevel) before resolving Play"
  - "UIFactory constructor now takes GameService + ProgressionService + GameSessionService"
patterns_established:
  - "Presenter reads from services on Initialize() to set view state (level display)"
  - "Presenter writes to services on action (session context on Play)"
drill_down_paths:
  - .gsd/milestones/M004/slices/S02/tasks/T01-PLAN.md
duration: 12min
verification_result: pass
completed_at: 2026-03-16T15:30:00Z
---

# T01: Extend MainMenu interfaces, presenter, and action enum + tests

**Play button wired through full MVP stack: view → presenter → scene controller → ScreenId.InGame, with level display and session context setup — 75/75 tests passing**

## What Happened

Extended the MainMenu to support the game loop entry point:
- Added `Play` to `MainMenuAction`, `InGame` to `ScreenId`
- Extended `IMainMenuView` with `OnPlayClicked` event and `UpdateLevelDisplay` method
- Updated `MainMenuPresenter` to accept `ProgressionService` and `GameSessionService` — `Initialize()` sets level display, Play handler calls `ResetForNewGame` before resolving
- Updated `UIFactory` constructor to take all three services
- Updated `MainMenuSceneController` to map `Play` → `ScreenId.InGame`
- Updated `GameBootstrapper` to construct new services and pass to UIFactory
- Added `InGame` to `DetectAlreadyLoadedScreen`
- Updated mock views and added 5 new tests

## Deviations
None.

## Files Created/Modified
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — added Play
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — added OnPlayClicked + UpdateLevelDisplay
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — accepts services, handles Play
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs` — added Play button + level text
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — Play → InGame
- `Assets/Scripts/Game/Boot/UIFactory.cs` — updated constructor
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — constructs services, passes to factory
- `Assets/Scripts/Game/ScreenId.cs` — added InGame
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — updated mocks + 4 new tests
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — updated factory + 1 new test
