# S02: Main Menu — Level Display & Play Button

**Goal:** Extend the main menu to show the current level and have a Play button that sets session context and navigates to InGame.
**Demo:** MainMenuPresenter reads current level from ProgressionService, view shows "Level N", Play button sets session context via GameSessionService.ResetForNewGame and returns ScreenId.InGame. Edit-mode tests prove the wiring.

## Must-Haves
- IMainMenuView gains: `event Action OnPlayClicked`, `void UpdateLevelDisplay(string text)`
- MainMenuAction gains: `Play`
- MainMenuPresenter receives ProgressionService and GameSessionService; Initialize() sets level display; Play sets session and resolves WaitForAction as Play
- MainMenuSceneController maps MainMenuAction.Play to return ScreenId.InGame
- ScreenId gains: `InGame`
- UIFactory.CreateMainMenuPresenter updated to pass services
- Edit-mode tests cover: play action resolves, level display is set, session context is set on play

## Tasks

- [x] **T01: Extend MainMenu interfaces, presenter, and action enum + tests**
  Add Play to MainMenuAction, extend IMainMenuView, update MainMenuPresenter to accept services and handle Play, update UIFactory, add ScreenId.InGame, update tests.

## Files Likely Touched
- Assets/Scripts/Game/MainMenu/IMainMenuView.cs
- Assets/Scripts/Game/MainMenu/MainMenuAction.cs
- Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs
- Assets/Scripts/Game/MainMenu/MainMenuView.cs
- Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
- Assets/Scripts/Game/Boot/UIFactory.cs
- Assets/Scripts/Game/ScreenId.cs
- Assets/Tests/EditMode/Game/DemoWiringTests.cs
