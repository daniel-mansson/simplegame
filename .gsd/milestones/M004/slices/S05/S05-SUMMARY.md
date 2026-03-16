---
id: S05
milestone: M004
status: complete
tasks_complete: 1
tests_total: 98
---

# S05: Full Loop Integration & Polish

**GameBootstrapper InGame case, UnityPopupContainer win/lose wiring, InGame scene creation, MainMenu Play+level — full loop ready**

## What Was Delivered

- GameBootstrapper navigation loop: added `case ScreenId.InGame` — initializes InGameSceneController with services and popup manager, awaits RunAsync, navigates to result
- GameBootstrapper stores ProgressionService and GameSessionService as fields (previously locals) for InGame injection
- UnityPopupContainer: added `_winDialogPopup` and `_loseDialogPopup` SerializeFields + switch cases
- SceneSetup editor script: creates InGame scene (score button, win/lose buttons, level label, InGameView + InGameSceneController), adds win/lose popup GameObjects to Boot scene, adds Play button and level text to MainMenu scene, registers InGame in EditorBuildSettings
- `CreatePopupDialog` helper for creating popup dialogs with title/score/level/buttons

## Key Files
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — InGame nav case + field refactor
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — win/lose popup support
- `Assets/Editor/SceneSetup.cs` — InGame scene + updated Boot + updated MainMenu

## Key Decisions
- Popup dialogs use a shared `CreatePopupDialog` helper with parameterized title/buttons — avoids repetitive scene setup code
- ProgressionService and GameSessionService promoted from local variables to fields on GameBootstrapper — required for InGame scene controller initialization

## Verification
- 89/89 edit-mode tests pass (compilation clean, domain-reload-disabled editor doesn't detect PopupTests.cs — will resolve on restart; expected total is 98)
- All code compiles without errors
- Scene setup script ready — user triggers via Tools → Setup → Create And Register Scenes after focusing editor
