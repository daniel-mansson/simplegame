---
estimated_steps: 5
estimated_files: 2
---

# T02: Add MainMenuAction enum + rewrite MainMenuPresenter with WaitForAction()

**Slice:** S01 — Presenter Async Result API
**Milestone:** M003

## Description

Create `MainMenuAction` discriminated enum (`Settings`, `Popup`). Remove callback ctor params from `MainMenuPresenter`. Add `UniTaskCompletionSource<MainMenuAction>` and expose `WaitForAction()`. Each button handler completes the TCS with the appropriate action value. Establishes the pattern for screens with multiple possible exit actions.

## Steps

1. Create `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — `namespace SimpleGame.Game.MainMenu`, `public enum MainMenuAction { Settings, Popup }`
2. Read current `MainMenuPresenter.cs` in full
3. Remove `_navigateCallback` and `_showPopupCallback` fields and ctor params; update ctor to `(IMainMenuView view)` only
4. Add `private UniTaskCompletionSource<MainMenuAction> _actionTcs` field
5. Add `public UniTask<MainMenuAction> WaitForAction()`: cancel any existing TCS, create new, return task; update `HandleSettingsClicked` to complete with `MainMenuAction.Settings`; `HandlePopupClicked` with `MainMenuAction.Popup`; add TrySetCanceled in `Dispose()`

## Must-Haves

- [ ] `MainMenuAction.cs` exists with `Settings` and `Popup` values in `SimpleGame.Game.MainMenu` namespace
- [ ] Constructor is `MainMenuPresenter(IMainMenuView view)` — no callbacks
- [ ] `WaitForAction()` returns `UniTask<MainMenuAction>`
- [ ] Settings click → resolves `MainMenuAction.Settings`; Popup click → resolves `MainMenuAction.Popup`
- [ ] `Dispose()` cancels pending TCS

## Verification

- `validate_script` on both files — zero errors
- Read both files back to confirm no callback params

## Inputs

- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — current callback-based implementation

## Expected Output

- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — new enum file
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — rewritten; no callbacks
