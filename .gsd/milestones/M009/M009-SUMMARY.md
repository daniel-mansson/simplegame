---
milestone: M009
provides:
  - IInSceneScreenManager<TScreenId> / InSceneScreenManager<TScreenId> — SetActive panel swap, back stack (Core)
  - MainMenuScreenId enum (Home, Shop); MainMenu wired with two screens
  - ICoinsService / CoinsService — persisted via MetaSaveData.coins
  - LevelFailedChoice.Continue + ILevelFailedView.OnContinueClicked — 4th LevelFailed option
  - ShopPresenter — 3 coin pack tiers (500/1200/2500), fake IAP, coins granted on purchase
  - IShopView / ShopView / ShopPopup.prefab — shop UI wired in Boot scene
  - UnityViewContainer sort-order scheme — per-popup Canvas OverrideSorting; bottom popup 50, blocker 100, top popup 150+
  - ICurrencyOverlay / UnityCurrencyOverlay — Canvas sort 120, LitMotion fade, TMP_Text coin balance
  - InGameSceneController: Continue flow (spend coins or stack shop popup); overlay show/hide/update
  - MainMenuSceneController: Shop screen via InSceneScreenManager; ShopPresenter on shop screen
  - GameBootstrapper: CoinsService + CurrencyOverlay constructed and wired
  - 192/192 EditMode tests; 9 new CoinsService tests + 10 InSceneScreenManager tests + 3 sort-order tests
slices_completed: [S01, S02, S03, S04]
key_files:
  - Assets/Scripts/Core/ScreenManagement/IInSceneScreenManager.cs
  - Assets/Scripts/Core/ScreenManagement/InSceneScreenManager.cs
  - Assets/Scripts/Core/ICurrencyOverlay.cs
  - Assets/Scripts/Core/Unity/UnityCurrencyOverlay.cs
  - Assets/Scripts/Game/Services/ICoinsService.cs
  - Assets/Scripts/Game/Services/CoinsService.cs
  - Assets/Scripts/Game/Popup/IShopView.cs
  - Assets/Scripts/Game/Popup/ShopView.cs
  - Assets/Scripts/Game/Popup/ShopPresenter.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Scripts/Game/Popup/LevelFailedChoice.cs
  - Assets/Scripts/Game/Popup/ILevelFailedView.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Prefabs/Game/Popups/ShopPopup.prefab
  - Assets/Scenes/Boot.unity
  - Assets/Scenes/MainMenu.unity
key_decisions:
  - "InSceneScreenManager uses SetActive — no animation this milestone, degrades gracefully if panels not wired"
  - "Per-popup Canvas OverrideSorting scheme: 50 / 100 (blocker) / 150+ — bottom popup dimmed when stacked"
  - "CoinsService follows GoldenPieceService pattern exactly — reload-then-merge save strategy"
  - "ShopPresenter: stub IAP — immediate grant, no store SDK"
  - "CurrencyOverlay sort order 120 — above blocker, below stacked popups"
  - "Core asmdef needed TMP reference for UnityCurrencyOverlay"
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# M009: In-Scene Screens, Popup Stack, Coins & Overlay HUD

**All four systems delivered: in-scene screen switcher, popup visual stacking, coins economy with Continue/shop flow, contextual overlay HUD — 192 tests green.**

## What Was Built

**S01 (In-Scene Screen Manager):** `IInSceneScreenManager<TScreenId>` + `InSceneScreenManager<TScreenId>` in Core — SetActive panel swap, back stack, generic. MainMenu wired with Home and Shop panels. 10 unit tests.

**S02 (Popup Stack Visual Layering):** `UnityViewContainer` assigns per-popup Canvas OverrideSorting based on stack depth (50, 150, 250...). Blocker at 100 — bottom popup is visually dimmed, top popup is above it. `PopupId.Shop`, `IShopView`/`ShopView`/`ShopPopup.prefab` added.

**S03 (Coins, Continue & Shop Flow):** `CoinsService` (persisted in `MetaSaveData.coins`). `LevelFailedChoice.Continue` — new button costs 100 coins. If insufficient: shop popup stacks on LevelFailed; purchase grants coins; balance updates; loop continues. Shop screen accessible from MainMenu via InSceneScreenManager. 8 new CoinsService tests.

**S04 (Overlay HUD):** `ICurrencyOverlay` / `UnityCurrencyOverlay` on Canvas sort 120. LitMotion alpha fade. Shows coin balance. InGameSceneController shows overlay when entering LevelFailed, updates after purchase, hides on dismiss.

## Deviations from Plan

- Core asmdef needed TMP reference added (UnityCurrencyOverlay uses TMP_Text)
- SceneSetup needs multiple menu runs in sequence for new SerializeField fields to be picked up after compile (K007 pattern)
- LevelFailed Continue flow re-loops WaitForChoice after shop closes (not a popup dismiss) — simpler than dismissing and reshowing

## Requirements Addressed

- R084 ✓ — InSceneScreenManager drives MainMenu ↔ Shop screen swap
- R085 ✓ — Popup stacking with correct visual layering (sort order scheme)
- R086 ✓ — CoinsService persists coins in MetaSaveData
- R087 ✓ — LevelFailed Continue costs 100 coins; opens shop popup if insufficient
- R088 ✓ — Shop with 3 fake IAP coin packs; accessible from MainMenu and LevelFailed
- R089 ✓ — Overlay HUD with coin balance; contextual appear/disappear with LitMotion
