---
id: S01
milestone: M009
title: In-Scene Screen Manager
risk: medium
depends: []
---

# S01: In-Scene Screen Manager

**Goal:** `InSceneScreenManager<TScreenId>` in Core; MainMenu wired with Home and Shop screens; tapping Shop swaps panels; Back returns to Home.

**Demo:** In play mode, MainMenu shows a Home panel. A "Shop" button swaps to the Shop panel (instant, no scene load). A Back button returns to Home.

## Must-Haves

- `IInSceneScreenManager<TScreenId>` interface in `SimpleGame.Core.ScreenManagement` with `ShowScreen(TScreenId)`, `GoBack()`, `CurrentScreen`, `CanGoBack`
- `InSceneScreenManager<TScreenId>` plain C# implementation: maps `TScreenId` → `GameObject`, SetActive swap, back stack
- `MainMenuScreenId` enum (`Home`, `Shop`) in `SimpleGame.Game.MainMenu`
- `MainMenuSceneController` wired with `InSceneScreenManager<MainMenuScreenId>`; Shop button calls `ShowScreen(Shop)`; Back button (or existing back logic) calls `GoBack()`
- `IMainMenuView` gains `OnShopClicked` event (or existing shop button plumbing)
- EditMode tests: ShowScreen activates correct panel, GoBack restores previous, CanGoBack reflects history, no-op when history empty
- All 169 existing tests remain green

## Tasks

- [x] **T01: Core InSceneScreenManager**
  Add `IInSceneScreenManager<TScreenId>` interface and `InSceneScreenManager<TScreenId>` implementation to `Assets/Scripts/Core/ScreenManagement/`. Write EditMode tests in Core test assembly.

- [x] **T02: MainMenu wiring**
  Add `MainMenuScreenId` enum, `OnShopClicked` to `IMainMenuView`/`MainMenuView`, wire `MainMenuSceneController` to create and use `InSceneScreenManager<MainMenuScreenId>`. SceneSetup adds Home/Shop panel GameObjects to MainMenu scene.

## Files Likely Touched

- `Assets/Scripts/Core/ScreenManagement/IInSceneScreenManager.cs` (new)
- `Assets/Scripts/Core/ScreenManagement/InSceneScreenManager.cs` (new)
- `Assets/Scripts/Game/MainMenu/MainMenuScreenId.cs` (new)
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
- `Assets/Tests/EditMode/Core/InSceneScreenManagerTests.cs` (new)
- `Assets/Editor/SceneSetup.cs`
