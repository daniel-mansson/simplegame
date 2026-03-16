---
id: S01
parent: M002
milestone: M002
provides:
  - SimpleGame.Core.asmdef — game-agnostic assembly; references UniTask + UnityEngine.UI; autoReferenced true
  - ScreenManager<TScreenId> where TScreenId : System.Enum — generic screen manager in SimpleGame.Core.ScreenManagement
  - PopupManager<TPopupId> where TPopupId : System.Enum — generic popup manager in SimpleGame.Core.PopupManagement
  - IPopupContainer<TPopupId> — generic container interface in SimpleGame.Core.PopupManagement
  - Unity implementations under Core/Unity/: UnitySceneLoader (SimpleGame.Core.Unity.ScreenManagement), UnityInputBlocker (SimpleGame.Core.Unity.PopupManagement), UnityTransitionPlayer (SimpleGame.Core.Unity.TransitionManagement)
  - Core/MVP contains only three framework base types: IView, IPopupView, Presenter<T>
  - Old SimpleGame.Runtime.asmdef deleted; Runtime/ folder emptied
key_files:
  - Assets/Scripts/Core/SimpleGame.Core.asmdef
  - Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/PopupManagement/IPopupContainer.cs
  - Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs
  - Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs
  - Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs
key_decisions:
  - "ScreenManager<TScreenId> where TScreenId : System.Enum — System.Enum constraint preferred over struct for clarity; C# 7.3+, fully supported in Unity 6"
  - "IPopupContainer made generic alongside PopupManager — they share the same TPopupId type parameter, keeping the contract consistent"
  - "ScreenId and PopupId remain in Core temporarily during S01 — they are deleted from Core and recreated in Game in S02"
  - ".vs/ added to .gitignore — was missing, caused permission errors on git add"
patterns_established:
  - "Generic manager pattern: ScreenManager<TScreenId>/PopupManager<TPopupId> — game code instantiates with concrete enum type; Core remains game-agnostic"
  - "Core/Unity/ subfolder convention for Unity-specific implementations of Core interfaces"
drill_down_paths:
  - .gsd/milestones/M002/slices/S01/tasks/T01-PLAN.md
  - .gsd/milestones/M002/slices/S01/tasks/T02-PLAN.md
  - .gsd/milestones/M002/slices/S01/tasks/T03-PLAN.md
  - .gsd/milestones/M002/slices/S01/tasks/T04-PLAN.md
  - .gsd/milestones/M002/slices/S01/tasks/T05-PLAN.md
  - .gsd/milestones/M002/slices/S01/tasks/T06-PLAN.md
duration: ~40m
verification_result: pass
completed_at: 2026-03-16
---

# S01: Core Assembly Restructure + Generic Managers

**SimpleGame.Core.asmdef created; ScreenManager<TScreenId> and PopupManager<TPopupId> are generic; Core/MVP contains only IView/IPopupView/Presenter; all game-specific types removed from Core runtime sources.**

## What Happened

Six tasks restructured the Core assembly in sequence. T01 created `SimpleGame.Core.asmdef` and moved the three Unity implementations (`UnitySceneLoader`, `UnityInputBlocker`, `UnityTransitionPlayer`) to `Core/Unity/` subfolders with updated namespaces (`SimpleGame.Core.Unity.*`). T02–T03 made `ScreenManager` and `PopupManager` generic using `where T : System.Enum`, with `IPopupContainer` made generic in lockstep. T04–T05 deleted all game-specific types from Core runtime: `ISampleView`, `SamplePresenter`, `GameService`, three view interfaces, three presenters, and `UIFactory`. T06 deleted the remaining game-specific MonoBehaviours from `Runtime/` (`GameBootstrapper`, `MainMenuView`, `SettingsView`, `ConfirmDialogView`, `UnityPopupContainer`) and removed `SimpleGame.Runtime.asmdef`.

`ScreenId.cs` and `PopupId.cs` remain in Core temporarily — they are needed until S02 creates the Game assembly. They will be deleted from Core and recreated under `Game/` in S02.

The project is in a known-broken state: the test assembly and Editor assembly still reference old namespaces and the now-deleted types. S02 (Game assembly) and S03 (test split) resolve this.

## Deviations

- `.vs/` was not in `.gitignore`, causing permission errors on `git add`. Added `.vs/` to `.gitignore` and removed the cached entry. No plan deviation.

## Files Created/Modified

- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — created: new assembly definition
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — modified: `ScreenManager<TScreenId> where TScreenId : System.Enum`
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — modified: `PopupManager<TPopupId> where TPopupId : System.Enum`
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — modified: `IPopupContainer<TPopupId>`
- `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs` — moved from Runtime + namespace updated
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — moved from Runtime + namespace updated
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — moved from Runtime + namespace updated
- `Assets/Scripts/Core/MVP/ISampleView.cs` — deleted
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — deleted
- `Assets/Scripts/Core/Services/GameService.cs` — deleted
- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — deleted
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — deleted
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — deleted
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/UIFactory.cs` — deleted
- `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` — deleted
- `Assets/Scripts/Runtime/MVP/MainMenuView.cs` — deleted
- `Assets/Scripts/Runtime/MVP/SettingsView.cs` — deleted
- `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs` — deleted
- `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` — deleted
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — deleted
- `.gitignore` — modified: added `.vs/`
