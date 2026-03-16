---
estimated_steps: 4
estimated_files: 7
---

# T05: Remove game-specific MVP files from Core

**Slice:** S01 — Core Assembly Restructure + Generic Managers
**Milestone:** M002

## Description

Remove the seven game-specific files from `Core/MVP/`: three view interfaces (`IMainMenuView`, `ISettingsView`, `IConfirmDialogView`), three presenters (`MainMenuPresenter`, `SettingsPresenter`, `ConfirmDialogPresenter`), and `UIFactory`. These will be recreated in the Game assembly in S02. After this task, `Core/MVP/` contains only the three framework base types: `IView.cs`, `IPopupView.cs`, `Presenter.cs`.

## Steps

1. `git rm` the following files (and their `.meta` counterparts): `Assets/Scripts/Core/MVP/IMainMenuView.cs`, `Assets/Scripts/Core/MVP/ISettingsView.cs`, `Assets/Scripts/Core/MVP/IConfirmDialogView.cs`, `Assets/Scripts/Core/MVP/MainMenuPresenter.cs`, `Assets/Scripts/Core/MVP/SettingsPresenter.cs`, `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs`, `Assets/Scripts/Core/MVP/UIFactory.cs`
2. Confirm what remains: `find Assets/Scripts/Core/MVP -name "*.cs"` should show only `IView.cs`, `IPopupView.cs`, `Presenter.cs`
3. Confirm Core grep guard passes: `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|UIFactory\|MainMenuPresenter\|SettingsPresenter\|ConfirmDialogPresenter" Assets/Scripts/Core/` returns empty

## Must-Haves

- [ ] `Core/MVP/` contains only `IView.cs`, `IPopupView.cs`, `Presenter.cs`
- [ ] No game-specific view interfaces or presenters remain in Core
- [ ] `UIFactory.cs` is gone from Core

## Verification

- `find Assets/Scripts/Core/MVP -name "*.cs" | sort` returns exactly three files
- `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|UIFactory" Assets/Scripts/Core/` returns empty

## Inputs

- Seven game-specific files in `Assets/Scripts/Core/MVP/` — to be deleted

## Expected Output

- `Core/MVP/` contains only: `IView.cs`, `IPopupView.cs`, `Presenter.cs`
