---
id: S03
milestone: M003
status: complete
tests_passed: 58
tests_total: 58
---

# S03 Summary: Boot-from-Any-Scene + Editor Tooling

## What Was Built

`BootInjector` ensures Boot always loads regardless of which scene is started from. SceneSetup was updated to wire SceneControllers into newly created scenes. MainMenuSceneController now discovers ConfirmDialogView at runtime (it lives in Boot scene, not MainMenu).

## Files Changed

- `Assets/Scripts/Game/Boot/BootInjector.cs` (new) — `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`; checks if Boot is loaded; if not, loads it additively
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — `_confirmDialogView` is now optional; `ActiveConfirmDialogView` falls back to `FindFirstObjectByType<ConfirmDialogView>()` if null
- `Assets/Editor/SceneSetup.cs` — `CreateMainMenuScene` adds `MainMenuSceneController` with `_mainMenuView` wired; `CreateSettingsScene` adds `SettingsSceneController` with `_settingsView` wired
- `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Settings.unity` — regenerated with SceneControllers present

## Key Decisions Made

- **ConfirmDialogView runtime discovery** — cross-scene SerializeField refs can't be wired at scene creation time; `FindFirstObjectByType<ConfirmDialogView>()` at popup time is clean and reliable once Boot is additively loaded
- **BootInjector is always active** — no `#if UNITY_EDITOR` guard; harmless in builds where Boot is index 0 (already loaded)

## Verification

- 58/58 edit-mode tests pass
- `grep -rn ".Forget()" Assets/Scripts/` → empty
- Scenes `MainMenu.unity` and `Settings.unity` contain SceneController components (confirmed via grep)
- Pending: play-mode UAT (enter play from MainMenu.unity, verify boot loads, navigation works)
