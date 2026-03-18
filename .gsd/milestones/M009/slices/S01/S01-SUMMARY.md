---
id: S01
milestone: M009
provides:
  - IInSceneScreenManager<TScreenId> — Core interface for in-scene panel switching
  - InSceneScreenManager<TScreenId> — SetActive swap, back stack, generic, no game types
  - MainMenuScreenId enum — Home, Shop
  - IMainMenuView.OnShopClicked / OnShopBackClicked events
  - MainMenuSceneController wired with InSceneScreenManager
  - MainMenu scene has HomePanel (active) and ShopPanel (inactive) with wired controller refs
  - 10 new InSceneScreenManager tests; 180/180 total
slices_completed: [S01]
key_files:
  - Assets/Scripts/Core/ScreenManagement/IInSceneScreenManager.cs
  - Assets/Scripts/Core/ScreenManagement/InSceneScreenManager.cs
  - Assets/Scripts/Game/MainMenu/MainMenuScreenId.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Tests/EditMode/Core/InSceneScreenManagerTests.cs
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S01: In-Scene Screen Manager

**`InSceneScreenManager<TScreenId>` in Core; MainMenu wired with Home/Shop screens; 180/180 tests pass.**

## What Was Built

**T01:** `IInSceneScreenManager<TScreenId>` interface and `InSceneScreenManager<TScreenId>` implementation in `Core/ScreenManagement/`. Plain C#, no Unity types except `GameObject`. Constructor takes `Dictionary<TScreenId, GameObject>`. SetActive swap, back stack, no-op on same screen, LogWarning on missing panel. 10 EditMode tests.

**T02:** `MainMenuScreenId` enum, shop/shopback events on `IMainMenuView`, shop actions on `MainMenuAction`, presenter handler wiring. `MainMenuSceneController` creates `InSceneScreenManager` in `Initialize()` if panels are wired (degrades gracefully). SceneSetup adds HomePanel (active), ShopPanel (inactive), ShopButton, ShopBackButton to MainMenu scene with SerializeField refs wired.

## Deviations
None.
