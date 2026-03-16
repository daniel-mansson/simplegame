# T01: Extend MainMenu interfaces, presenter, and action enum + tests

**Slice:** S02
**Milestone:** M004

## Goal
Wire the Play button through the full MVP stack: view interface → presenter → scene controller → navigation.

## Must-Haves

### Truths
- MainMenuAction enum has Play value
- IMainMenuView has OnPlayClicked event and UpdateLevelDisplay method
- MainMenuPresenter.Initialize() calls UpdateLevelDisplay with current level from ProgressionService
- Simulating Play click resolves WaitForAction() as MainMenuAction.Play
- When Play resolves, GameSessionService.ResetForNewGame has been called with the current level
- MainMenuSceneController returns ScreenId.InGame when action is Play
- ScreenId has InGame value
- UIFactory.CreateMainMenuPresenter accepts updated parameters
- All existing tests still pass (some mock views need updating)

### Artifacts
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — adds Play
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — adds OnPlayClicked + UpdateLevelDisplay
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — accepts services, handles Play
- `Assets/Scripts/Game/ScreenId.cs` — adds InGame
- `Assets/Scripts/Game/Boot/UIFactory.cs` — updated CreateMainMenuPresenter
- Updated mock views and tests in DemoWiringTests.cs

### Key Links
- MainMenuPresenter → ProgressionService via constructor (reads CurrentLevel)
- MainMenuPresenter → GameSessionService via constructor (calls ResetForNewGame on Play)
- MainMenuSceneController → maps Play to ScreenId.InGame

## Steps
1. Add `Play` to MainMenuAction enum
2. Add `InGame` to ScreenId enum
3. Extend IMainMenuView with OnPlayClicked and UpdateLevelDisplay
4. Update MainMenuPresenter: accept ProgressionService + GameSessionService, handle Play action
5. Update UIFactory.CreateMainMenuPresenter signature
6. Update MainMenuView MonoBehaviour (add Play button + level label — serialized fields)
7. Update MainMenuSceneController to handle MainMenuAction.Play → return ScreenId.InGame
8. Update MockMainMenuView in tests, add new tests
9. Run all tests

## Context
- Follow D026: presenter exposes WaitForAction, no outbound callbacks
- Follow D029: context passing via GameSessionService
- MainMenuPresenter should call GameSessionService.ResetForNewGame(progression.CurrentLevel) when Play is clicked, before resolving the action
