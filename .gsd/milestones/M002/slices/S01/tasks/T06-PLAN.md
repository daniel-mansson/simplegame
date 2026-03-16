---
estimated_steps: 5
estimated_files: 8
---

# T06: Delete old Runtime folder and SimpleGame.Runtime.asmdef

**Slice:** S01 — Core Assembly Restructure + Generic Managers
**Milestone:** M002

## Description

After T01–T05, the `Assets/Scripts/Runtime/` folder contains only game-specific files that will be recreated in the Game assembly during S02: `GameBootstrapper.cs`, `MainMenuView.cs`, `SettingsView.cs`, `ConfirmDialogView.cs`, `UnityPopupContainer.cs`. Remove them from Runtime now (they'll be created fresh under `Assets/Scripts/Game/` in S02). Also delete the old `SimpleGame.Runtime.asmdef`.

Note: `ScreenId.cs` and `PopupId.cs` still exist in `Core/ScreenManagement/` and `Core/PopupManagement/` — they stay there temporarily and are deleted/moved in S02 when the Game assembly is created.

## Steps

1. Check what's still in Runtime: `find Assets/Scripts/Runtime -name "*.cs" | sort`
2. `git rm` `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` and `.meta`
3. `git rm` `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs`, `MainMenuView.cs`, `SettingsView.cs` and their `.meta` files
4. `git rm` `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` and `.meta`
5. `git rm` `Assets/Scripts/SimpleGame.Runtime.asmdef` and `.meta`
6. Check `git status` — confirm no unexpected files remain in Runtime; `find Assets/Scripts/Runtime -name "*.cs"` should return empty

## Must-Haves

- [ ] `Assets/Scripts/Runtime/` contains no `.cs` files
- [ ] `Assets/Scripts/SimpleGame.Runtime.asmdef` is gone
- [ ] `SimpleGame.Core.asmdef` is the sole asmdef under `Assets/Scripts/`

## Verification

- `find Assets/Scripts/Runtime -name "*.cs"` returns empty
- `find Assets/Scripts -name "*.asmdef"` returns only `Assets/Scripts/Core/SimpleGame.Core.asmdef`
- Final Core content check: `find Assets/Scripts/Core -name "*.cs" | sort` — should show IView, IPopupView, Presenter, ISceneLoader, ScreenManager, ScreenId (temporary), IInputBlocker, IPopupContainer, PopupManager, PopupId (temporary), ITransitionPlayer, Unity/*.cs

## Inputs

- `Assets/Scripts/Runtime/` — remaining game-specific files to delete
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — old asmdef to delete

## Expected Output

- `Assets/Scripts/Runtime/` is empty (directory can remain or be removed)
- `Assets/Scripts/SimpleGame.Runtime.asmdef` deleted
- S01 complete: Core assembly is clean; game-specific types removed; project is in a known-broken state for Game/Runtime layer (expected — S02 resolves this)
