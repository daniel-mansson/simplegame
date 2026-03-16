# S04: Win & Lose Popups

**Goal:** Add WinDialog and LoseDialog popup types, integrate them into InGameSceneController so win shows popup → menu, lose shows popup → retry or back.
**Demo:** Win popup shows score + level with Continue. Lose popup shows score + level with Retry/Back. InGameSceneController uses PopupManager for both. Retry resets score and replays. Edit-mode tests prove popup presenter behavior and scene controller popup flow.

## Must-Haves
- PopupId gains WinDialog, LoseDialog
- IWinDialogView, WinDialogPresenter: shows score + level, WaitForContinue resolves on click
- ILoseDialogView, LoseDialogPresenter: shows score + level, WaitForChoice returns LoseDialogChoice (Retry/Back)
- LoseDialogChoice enum: Retry, Back
- WinDialogView, LoseDialogView MonoBehaviours
- UIFactory gains CreateWinDialogPresenter, CreateLoseDialogPresenter
- InGameSceneController updated: Win → show win popup → return MainMenu; Lose → show lose popup → Retry loops, Back returns MainMenu
- InGameSceneController.Initialize gains PopupManager parameter
- Edit-mode tests for popup presenters and updated scene controller flow

## Tasks

- [x] **T01: Win/Lose popup types — enums, interfaces, presenters, views + tests**
  Create all popup types and their tests.

- [x] **T02: Integrate popups into InGameSceneController + update tests**
  Wire popup flow into scene controller, update UIFactory, update scene controller tests.

## Files Likely Touched
- Assets/Scripts/Game/PopupId.cs
- Assets/Scripts/Game/Popup/IWinDialogView.cs (new)
- Assets/Scripts/Game/Popup/WinDialogPresenter.cs (new)
- Assets/Scripts/Game/Popup/WinDialogView.cs (new)
- Assets/Scripts/Game/Popup/ILoseDialogView.cs (new)
- Assets/Scripts/Game/Popup/LoseDialogPresenter.cs (new)
- Assets/Scripts/Game/Popup/LoseDialogView.cs (new)
- Assets/Scripts/Game/Popup/LoseDialogChoice.cs (new)
- Assets/Scripts/Game/InGame/InGameSceneController.cs
- Assets/Scripts/Game/Boot/UIFactory.cs
- Assets/Tests/EditMode/Game/PopupTests.cs (new)
- Assets/Tests/EditMode/Game/InGameTests.cs
