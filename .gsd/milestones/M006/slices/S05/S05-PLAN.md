# S05: Main screen with meta world

**Goal:** MainMenu reworked into main screen showing environment, restorable objects, golden piece balance, play button. Tap objects to restore. ObjectRestored popup. Edit-mode tests.
**Demo:** Tests prove: object tap spends golden pieces and restores, blocked objects can't be tapped, balance updates, ObjectRestored popup fires on completion, play sets session context.

## Must-Haves

- Reworked IMainMenuView: environment name, object list with progress, balance, play/settings
- Reworked MainMenuPresenter: MetaProgressionService + GoldenPieceService integration, tap-to-restore
- Reworked MainMenuAction: Play, Settings, TapObject(index)
- ObjectRestored popup: IObjectRestoredView + ObjectRestoredPresenter
- Edit-mode tests for reworked presenter

## Verification

- All MainMenuPresenter tests pass
- All MainMenuSceneController tests pass
- ObjectRestored popup tests pass
- No compile errors

## Tasks

- [x] **T01: Rework MainMenu presenter and view interface for meta world** `est:30m`
  - Why: Core screen rework — main menu becomes the meta world hub
  - Files: `IMainMenuView.cs`, `MainMenuPresenter.cs`, `MainMenuAction.cs`, `MainMenuView.cs`
  - Do:
    1. Rework IMainMenuView: UpdateEnvironmentName(string), UpdateBalance(string), UpdateObjects(ObjectDisplayData[]) where ObjectDisplayData is a struct with name/progress/isBlocked/isComplete, event OnObjectTapped(int index), OnPlayClicked, OnSettingsClicked
    2. Create ObjectDisplayData struct in MainMenu folder
    3. Rework MainMenuPresenter: takes MetaProgressionService + GoldenPieceService + GameSessionService + ProgressionService. Initialize shows current environment objects. HandleObjectTapped validates not blocked/not complete, calls GoldenPieceService.TrySpend + MetaProgressionService.TryRestoreStep, updates view. Resolves ObjectRestored action when object completes.
    4. Rework MainMenuAction: Play, Settings, ObjectRestored(objectName)
    5. Rework MainMenuView: text-stub buttons and labels

- [x] **T02: ObjectRestored popup, SceneController update, and tests** `est:25m`
  - Why: Wire popup and update scene controller, write tests
  - Files: `IObjectRestoredView.cs`, `ObjectRestoredPresenter.cs`, `ObjectRestoredView.cs`, `MainMenuSceneController.cs`, `UIFactory.cs`, tests
  - Do:
    1. Create IObjectRestoredView + ObjectRestoredPresenter — shows object name, WaitForContinue
    2. Create ObjectRestoredView MonoBehaviour
    3. Update MainMenuSceneController — inject services, handle ObjectRestored action (show popup, dismiss, re-render objects)
    4. Update UIFactory — add CreateMainMenuPresenter with new deps, CreateObjectRestoredPresenter
    5. Write MainMenuPresenter tests — initialize, tap object, insufficient balance, blocked, complete, persistence
    6. Write ObjectRestored popup tests
    7. Update existing MainMenu scene controller tests

## Files Likely Touched

- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
- `Assets/Scripts/Game/MainMenu/ObjectDisplayData.cs` (new)
- `Assets/Scripts/Game/Popup/IObjectRestoredView.cs` (new)
- `Assets/Scripts/Game/Popup/ObjectRestoredPresenter.cs` (new)
- `Assets/Scripts/Game/Popup/ObjectRestoredView.cs` (new)
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `Assets/Tests/EditMode/Game/PopupTests.cs`
