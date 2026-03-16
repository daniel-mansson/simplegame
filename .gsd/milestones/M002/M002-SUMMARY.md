---
id: M002
milestone: M002
provides:
  - SimpleGame.Core.asmdef — game-agnostic UI framework; ScreenManager<TScreenId>, PopupManager<TPopupId>, IPopupContainer<TPopupId> generic; IView/IPopupView/Presenter<T> base types; ISceneLoader, IInputBlocker, ITransitionPlayer interfaces; Unity impls under Core/Unity/
  - SimpleGame.Game.asmdef — all game-specific code; feature-cohesive structure (interface+presenter+view co-located per screen)
  - SimpleGame.Tests.Core.asmdef — 32 Core framework tests; ISampleView/SamplePresenter as inline test fixtures; TestScreenId/TestPopupId local test enums
  - SimpleGame.Tests.Game.asmdef — 17 Game-specific tests; DemoWiringTests with all three presenters
  - 49/49 edit-mode tests preserved across new split test assemblies
  - SimpleGame.Editor.asmdef updated to reference Core + Game
key_files:
  - Assets/Scripts/Core/SimpleGame.Core.asmdef
  - Assets/Scripts/Game/SimpleGame.Game.asmdef
  - Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/PopupManagement/IPopupContainer.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Tests/EditMode/Core/SimpleGame.Tests.Core.asmdef
  - Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef
key_decisions:
  - ScreenManager<TScreenId>/PopupManager<TPopupId> use System.Enum constraint — cleaner than struct, self-documents intent
  - IPopupContainer<TPopupId> made generic in lockstep with PopupManager — consistent type parameter throughout the chain
  - ISampleView/SamplePresenter moved to Core test assembly as fixtures — not runtime code; SamplePresenter now takes a string message rather than GameService dependency
  - TestScreenId/TestPopupId defined in Core test assembly — Core tests are fully game-agnostic
  - Feature cohesion in Game: each screen's interface+presenter+view in same folder (MainMenu/, Settings/, Popup/)
  - ScreenId/PopupId moved to Game root (SimpleGame.Game namespace) — game-specific enums belong in game assembly
  - Final acceptance grep must use word-boundary: grep -n "\bScreenId\b\|\bPopupId\b" Assets/Scripts/Core/ | grep -v "TScreenId\|TPopupId" — naive grep matches type param names
patterns_established:
  - Generic manager pattern: game code instantiates ScreenManager<ScreenId>/PopupManager<PopupId>; Core stays game-agnostic
  - Core/Unity/ subfolder convention for Unity MonoBehaviour implementations of Core interfaces
  - Test fixture pattern: ISampleView/SamplePresenter as self-contained fixtures in test assembly, not production code
  - Feature-cohesion folder pattern: Game/FeatureName/ contains all layers for that feature
completed_at: 2026-03-16
verification_result: pass (static analysis; batchmode compile blocked by open Unity Editor — needs UAT)
---

# M002: Assembly Restructure — Core/Game Separation

**SimpleGame.Core (game-agnostic framework) and SimpleGame.Game (game-specific code) are now separate assemblies with feature-cohesive structure; 49 edit-mode tests split across SimpleGame.Tests.Core and SimpleGame.Tests.Game.**

## What Happened

Three slices restructured the entire codebase without changing any behavior.

**S01 (Core restructure + generic managers):** Created `SimpleGame.Core.asmdef`. Moved Unity implementations (`UnitySceneLoader`, `UnityInputBlocker`, `UnityTransitionPlayer`) to `Core/Unity/` subfolders. Made `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` generic using `where T : System.Enum`, with `IPopupContainer<TPopupId>` updated in lockstep. Deleted all game-specific types from Core: `ISampleView`, `SamplePresenter`, `GameService`, three view interfaces, three presenters, `UIFactory`. Deleted the old `SimpleGame.Runtime.asmdef` and `Runtime/` folder.

**S02 (Game assembly + feature cohesion):** Created `SimpleGame.Game.asmdef` referencing Core. Recreated all game-specific code under `Assets/Scripts/Game/` with feature-cohesive grouping: `MainMenu/` contains `IMainMenuView` + `MainMenuPresenter` + `MainMenuView`; `Settings/` contains `ISettingsView` + `SettingsPresenter` + `SettingsView`; `Popup/` contains `IConfirmDialogView` + `ConfirmDialogPresenter` + `ConfirmDialogView` + `UnityPopupContainer`. `GameService` in `Services/`, `UIFactory` + `GameBootstrapper` in `Boot/`. `ScreenId`/`PopupId` at Game root. Updated `SimpleGame.Editor.asmdef` to reference both `SimpleGame.Core` and `SimpleGame.Game`. Updated `SceneSetup.cs` namespaces.

**S03 (Test assembly split):** Split `SimpleGame.Tests.EditMode.asmdef` into `SimpleGame.Tests.Core` (32 tests) and `SimpleGame.Tests.Game` (17 tests). Core tests define `TestScreenId` and `TestPopupId` local enums — no game assembly dependency. `ISampleView` and `SamplePresenter` moved into Core test assembly as inline fixtures (SamplePresenter now takes a `string welcomeMessage` rather than a `GameService`, removing the runtime dependency). All 49 tests preserved.

## Notable discovery

The milestone definition of done used a naive grep for `ScreenId`/`PopupId` in Core, but `TScreenId`/`TPopupId` (the type parameter names in the generic managers) contain these substrings and produce false positives. The correct verification is: `grep -n "\bScreenId\b\|\bPopupId\b" Assets/Scripts/Core/ | grep -v "TScreenId\|TPopupId"` — this returns empty, confirming no concrete game enum references in Core.

## Files Created/Modified

- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — created
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — made generic
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — made generic
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — made generic
- `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs` — moved + namespace
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — moved + namespace
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — moved + namespace
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — created
- `Assets/Scripts/Game/ScreenId.cs` — moved from Core + namespace updated
- `Assets/Scripts/Game/PopupId.cs` — moved from Core + namespace updated
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — created
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — created
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs` — created
- `Assets/Scripts/Game/Settings/ISettingsView.cs` — created
- `Assets/Scripts/Game/Settings/SettingsPresenter.cs` — created
- `Assets/Scripts/Game/Settings/SettingsView.cs` — created
- `Assets/Scripts/Game/Popup/IConfirmDialogView.cs` — created
- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs` — created
- `Assets/Scripts/Game/Popup/ConfirmDialogView.cs` — created
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — created (now IPopupContainer<PopupId>)
- `Assets/Scripts/Game/Services/GameService.cs` — created
- `Assets/Scripts/Game/Boot/UIFactory.cs` — created (no SamplePresenter; three game presenters)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — created (ScreenManager<ScreenId>, PopupManager<PopupId>)
- `Assets/Editor/SceneSetup.cs` — updated namespaces
- `Assets/Editor/SimpleGame.Editor.asmdef` — updated references
- `Assets/Tests/EditMode/Core/SimpleGame.Tests.Core.asmdef` — created
- `Assets/Tests/EditMode/Core/MVPWiringTests.cs` — updated; ISampleView/SamplePresenter inline
- `Assets/Tests/EditMode/Core/ScreenManagerTests.cs` — updated; TestScreenId; ScreenManager<TestScreenId>
- `Assets/Tests/EditMode/Core/PopupManagerTests.cs` — updated; TestPopupId; PopupManager<TestPopupId>; IPopupContainer<TestPopupId>
- `Assets/Tests/EditMode/Core/TransitionTests.cs` — updated; TestScreenId; ScreenManager<TestScreenId>
- `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` — created
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — updated; new Game namespaces
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — deleted
- `Assets/Scripts/Runtime/` folder — deleted (all files removed)
- `Assets/Scripts/Core/MVP/ISampleView.cs` — deleted
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — deleted
- `Assets/Scripts/Core/Services/GameService.cs` — deleted
- `Assets/Scripts/Core/MVP/{IMainMenuView,ISettingsView,IConfirmDialogView,MainMenuPresenter,SettingsPresenter,ConfirmDialogPresenter,UIFactory}.cs` — deleted
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — deleted
- `Assets/Tests/EditMode/{MVPWiringTests,ScreenManagerTests,PopupManagerTests,TransitionTests,DemoWiringTests}.cs` — deleted (replaced)
