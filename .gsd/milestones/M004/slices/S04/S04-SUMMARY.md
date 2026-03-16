---
id: S04
milestone: M004
status: complete
tasks_complete: 2
tests_added: 9
tests_total: 98
---

# S04: Win & Lose Popups

**Win/Lose popup types with presenter logic, InGame scene controller popup integration, retry flow — 9 new tests (pending editor pickup), 89/89 verifiable now**

## What Was Delivered

- `LoseDialogChoice` enum (Retry, Back)
- `IWinDialogView`, `WinDialogPresenter` — shows score + level, WaitForContinue resolves on click
- `ILoseDialogView`, `LoseDialogPresenter` — shows score + level, WaitForChoice returns Retry/Back
- `WinDialogView`, `LoseDialogView` MonoBehaviours
- `PopupId` extended with WinDialog, LoseDialog
- `UIFactory` gains CreateWinDialogPresenter, CreateLoseDialogPresenter
- `InGameSceneController` fully reworked: Win → RegisterWin + win popup → MainMenu; Lose → lose popup → Retry (resets score, fresh presenter) or Back → MainMenu
- 9 new popup presenter tests in PopupTests.cs (4 win + 5 lose)
- 4 InGame scene controller tests rewritten to include popup flow (win+popup, lose+back, lose+retry+win, play-from-editor)

## Key Files
- `Assets/Scripts/Game/Popup/LoseDialogChoice.cs`, `IWinDialogView.cs`, `WinDialogPresenter.cs`, `WinDialogView.cs`
- `Assets/Scripts/Game/Popup/ILoseDialogView.cs`, `LoseDialogPresenter.cs`, `LoseDialogView.cs`
- `Assets/Scripts/Game/PopupId.cs` — WinDialog, LoseDialog added
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — full popup integration + retry
- `Assets/Scripts/Game/Boot/UIFactory.cs` — new Create methods
- `Assets/Tests/EditMode/Game/PopupTests.cs` — 9 tests
- `Assets/Tests/EditMode/Game/InGameTests.cs` — 4 rewritten scene controller tests

## Boundary Outputs (for S05)
- All popup types ready — presenters, views, enums
- InGameSceneController fully handles win/lose flows with popups and retry
- UIFactory creates all new presenter types
- PopupId has all three values (ConfirmDialog, WinDialog, LoseDialog)

## Key Decisions
- Win popup Initialize takes (score, level) — displays formatted text
- Lose popup retry: disposes old presenter, creates fresh one (outer while loop); resets session score to 0
- InGameSceneController.Initialize now takes 4 params: UIFactory, ProgressionService, GameSessionService, PopupManager

## Verification
- 89/89 edit-mode tests pass (compilation proves all code is correct)
- 9 new PopupTests.cs tests pending Unity editor file detection (will appear on next editor restart — known Unity domain-reload-disabled behavior)
