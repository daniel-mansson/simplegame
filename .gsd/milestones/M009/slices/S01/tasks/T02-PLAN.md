---
id: T02
parent: S01
milestone: M009
---

# T02: MainMenu Wiring

**Slice:** S01  
**Milestone:** M009

## Goal
Wire `InSceneScreenManager<MainMenuScreenId>` into `MainMenuSceneController`; add Shop screen panel to MainMenu scene via SceneSetup; add `OnShopClicked` to `IMainMenuView`/`MainMenuView`; add `MainMenuAction.Shop`.

## Must-Haves

### Truths
- `IMainMenuView` has `OnShopClicked` event
- `MainMenuView` wires a Shop button to `OnShopClicked`
- `MainMenuAction` has `Shop` value
- `MainMenuPresenter` fires `Shop` action when `OnShopClicked` fires
- `MainMenuSceneController` creates `InSceneScreenManager<MainMenuScreenId>` from `[SerializeField]` panel refs, calls `ShowScreen(Home)` on init, calls `ShowScreen(Shop)` on `MainMenuAction.Shop`, calls `GoBack()` — or a Back action from the shop — to return
- `MainMenuScreenId` enum exists at `Assets/Scripts/Game/MainMenu/MainMenuScreenId.cs`
- SceneSetup adds `HomePanel` and `ShopPanel` GameObjects to the MainMenu scene, wires `[SerializeField]` refs on `MainMenuSceneController`
- All 169+ tests remain green

### Artifacts
- `Assets/Scripts/Game/MainMenu/MainMenuScreenId.cs` — enum with `Home`, `Shop`
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — updated with `OnShopClicked`
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs` — Shop button wired
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — `Shop` added
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — handles `OnShopClicked`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — InSceneScreenManager wired

### Key Links
- `MainMenuSceneController` creates `InSceneScreenManager<MainMenuScreenId>` with `{Home: _homePanel, Shop: _shopPanel}` where `_homePanel`/`_shopPanel` are `[SerializeField] GameObject`
- `MainMenuPresenter` fires `Shop` action via existing `WaitForAction()` UniTask pattern
- SceneSetup creates `HomePanel` and `ShopPanel` as children of MainMenu scene root

## Steps
1. Create `MainMenuScreenId.cs` enum
2. Add `OnShopClicked` to `IMainMenuView` and wire it in `MainMenuView` (add Shop button)
3. Add `Shop` to `MainMenuAction`
4. Update `MainMenuPresenter` to handle `OnShopClicked` → fire `MainMenuAction.Shop`; dispose the subscription
5. Update `MainMenuSceneController`: add `[SerializeField] GameObject _homePanel`, `[SerializeField] GameObject _shopPanel`; create `InSceneScreenManager<MainMenuScreenId>` in `Initialize`; call `ShowScreen(Home)` on start; handle `Shop` action; handle back from shop (add `MainMenuAction.ShopBack` or reuse GoBack on a second Shop action — use `GoBack()` on a Back button in the shop panel)
6. Add `ShopBack` to `MainMenuAction` for the back-from-shop signal
7. Update `IMainMenuView`/`MainMenuView` with `OnShopBackClicked` event
8. Update SceneSetup: add `HomePanel` and `ShopPanel` as empty child GameObjects in the MainMenu scene; wire `_homePanel`/`_shopPanel` SerializeField refs on `MainMenuSceneController` — `HomePanel` starts active, `ShopPanel` starts inactive
9. Run `Tools/Setup/Create And Register Scenes` to regenerate Boot.unity
10. Verify tests green

## Context
- `MainMenuView` already has `_settingsButton`, `_playButton`, etc. — add `_shopButton` the same way
- SceneSetup builds the MainMenu scene inline; it creates GameObjects and wires them — look at how existing buttons are created there and mirror the pattern
- The shop screen panel content is empty for now (S03 will populate it with ShopView)
- `MainMenuSceneController.RunAsync` currently loops on `WaitForAction()` — add `Shop` case that calls `_screenManager.ShowScreen(MainMenuScreenId.Shop)` and loops back waiting for `ShopBack`
