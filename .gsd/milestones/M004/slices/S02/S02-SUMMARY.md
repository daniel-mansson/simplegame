---
id: S02
milestone: M004
status: complete
tasks_complete: 1
tests_added: 5
tests_total: 75
---

# S02: Main Menu — Level Display & Play Button

**Play button wired through full MVP stack with level display and session context — 5 new tests, 75/75 total**

## What Was Delivered

Main menu extended to be the game loop hub: shows current level from ProgressionService, Play button sets session context via GameSessionService.ResetForNewGame, and MainMenuSceneController returns ScreenId.InGame on Play. UIFactory and GameBootstrapper updated to construct and pass new services.

## Key Files
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — accepts services, handles Play
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — Play → ScreenId.InGame
- `Assets/Scripts/Game/Boot/UIFactory.cs` — constructor takes all services
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — constructs ProgressionService + GameSessionService

## Boundary Outputs (for downstream slices)
- `ScreenId.InGame` — new enum value for InGame scene
- `MainMenuAction.Play` — triggers session context setup and navigation
- `UIFactory` now holds `ProgressionService` + `GameSessionService` — available for new presenter Create methods
- `GameBootstrapper` constructs all services and detects InGame scene

## Key Decisions
- MainMenuPresenter calls `GameSessionService.ResetForNewGame(progression.CurrentLevel)` before resolving Play — context is set before scene transition
- UIFactory constructor: `(GameService, ProgressionService, GameSessionService)` — single injection point for all services

## Verification
- 75/75 edit-mode tests pass (70 + 5 new)
