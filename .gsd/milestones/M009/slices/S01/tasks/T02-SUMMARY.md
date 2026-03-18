---
id: T02
parent: S01
milestone: M009
provides:
  - MainMenuScreenId enum (Home, Shop)
  - IMainMenuView.OnShopClicked / OnShopBackClicked events
  - MainMenuAction.OpenShop / CloseShop
  - MainMenuPresenter handles shop/shopback actions
  - MainMenuSceneController wired with InSceneScreenManager<MainMenuScreenId>
  - SceneSetup adds HomePanel, ShopPanel, ShopButton, ShopBackButton to MainMenu scene
key_files:
  - Assets/Scripts/Game/MainMenu/MainMenuScreenId.cs
  - Assets/Scripts/Game/MainMenu/IMainMenuView.cs
  - Assets/Scripts/Game/MainMenu/MainMenuView.cs
  - Assets/Scripts/Game/MainMenu/MainMenuAction.cs
  - Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Tests/EditMode/Game/DemoWiringTests.cs
key_decisions:
  - "InSceneScreenManager created in Initialize() if panels are wired — degrades gracefully with warning if panels missing"
  - "SetScreenManagerForTesting() test seam added for isolation"
  - "ShopPanel starts inactive; HomePanel starts active — InSceneScreenManager calls ShowScreen(Home) on init"
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T02: MainMenu Wiring

**MainMenu scene has Home/Shop screens; ShopButton and ShopBackButton wired; InSceneScreenManager integrated; 180/180 tests pass.**

## What Happened

Added `MainMenuScreenId` enum, `OnShopClicked`/`OnShopBackClicked` events to `IMainMenuView`, `OpenShop`/`CloseShop` to `MainMenuAction`. `MainMenuPresenter` subscribes and fires. `MainMenuSceneController` creates `InSceneScreenManager<MainMenuScreenId>` in `Initialize()` using `[SerializeField]` panel refs; handles `OpenShop`/`CloseShop` actions.

SceneSetup updated: adds ShopButton, HomePanel (active), ShopPanel (inactive), ShopBackButton inside ShopPanel. All SerializeField refs wired on controller.

Updated `MockMainMenuView` in `DemoWiringTests.cs` to include the two new events.

## Deviations
None.

## Files Created/Modified
- `Assets/Scripts/Game/MainMenu/MainMenuScreenId.cs` — new
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — added OnShopClicked, OnShopBackClicked
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs` — added _shopButton, _shopBackButton
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — added OpenShop, CloseShop
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — handles shop actions
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — InSceneScreenManager wired
- `Assets/Editor/SceneSetup.cs` — HomePanel, ShopPanel, ShopButton, ShopBackButton added
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — MockMainMenuView updated
