---
id: S03
milestone: M009
title: Coins, Continue & Shop Flow
risk: low
depends: [S02]
---

# S03: Coins, Continue & Shop Flow

**Goal:** CoinsService persisted in MetaSaveData; LevelFailed has Continue (100 coins); insufficient → shop popup stacks; buy coins → shop closes → LevelFailed resumes; shop accessible from MainMenu screen.

## Must-Haves

- `ICoinsService` / `CoinsService` — Balance, Earn, TrySpend, Save, ResetAll; persisted via `MetaSaveData.coins`
- `MetaSaveData.coins` int field added
- `LevelFailedChoice.Continue` added
- `ILevelFailedView.OnContinueClicked` event; `LevelFailedView` gets a Continue button
- `LevelFailedPresenter` handles Continue; `LevelFailedPresenter.WaitForChoice()` can return `Continue`
- `ShopPresenter` — shows 3 coin packs; each triggers fake IAP flow (stub: immediate grant); grants coins; disposes cleanly
- `InGameSceneController`: handles Continue — TrySpend 100 coins → restore hearts & continue; insufficient → show Shop popup (stacks on LevelFailed); after shop popup dismissed, loop back to WaitForChoice
- `MainMenuSceneController`: ShopPanel wired with ShopView (via IViewResolver); shows ShopPresenter when Shop screen is active
- `UIFactory.CreateShopPresenter(IShopView)` added
- `GameBootstrapper` constructs CoinsService, passes to UIFactory and scene controllers
- EditMode tests: CoinsService earn/spend/persist; ShopPresenter pack selection
- All 183 existing tests remain green

## Tasks

- [ ] **T01: CoinsService**
  Add `MetaSaveData.coins`; create `ICoinsService`/`CoinsService`; update `UIFactory` ctor; update `GameBootstrapper`; EditMode tests.

- [ ] **T02: LevelFailed Continue button**
  Add `LevelFailedChoice.Continue`; `OnContinueClicked` event; `LevelFailedView` Continue button; `LevelFailedPresenter` handles it; update all mocks.

- [ ] **T03: ShopPresenter + InGame continue flow**
  Create `ShopPresenter`; `InGameSceneController` handles Continue → spend/stack shop; `UIFactory.CreateShopPresenter`; wire to Boot.

- [ ] **T04: MainMenu shop screen wiring**
  `MainMenuSceneController` shows ShopPresenter on Shop screen via IViewResolver; update SceneSetup to add ShopView to ShopPanel.

## Files Likely Touched

- `Assets/Scripts/Game/Services/MetaSaveData.cs`
- `Assets/Scripts/Game/Services/ICoinsService.cs` (new)
- `Assets/Scripts/Game/Services/CoinsService.cs` (new)
- `Assets/Scripts/Game/Popup/LevelFailedChoice.cs`
- `Assets/Scripts/Game/Popup/ILevelFailedView.cs`
- `Assets/Scripts/Game/Popup/LevelFailedView.cs`
- `Assets/Scripts/Game/Popup/LevelFailedPresenter.cs`
- `Assets/Scripts/Game/Popup/ShopPresenter.cs` (new)
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
- `Assets/Tests/EditMode/Game/PopupTests.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `Assets/Editor/SceneSetup.cs`
