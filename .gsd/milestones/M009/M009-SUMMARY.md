---
milestone: M009
provides:
  - IInSceneScreenManager<TScreenId> / InSceneScreenManager<TScreenId> — SetActive panel swap, back stack (Core)
  - MainMenuScreenId enum (Home, Shop); MainMenu wired with two screens; all home content in HomePanel
  - ICoinsService / CoinsService — persisted via MetaSaveData.coins
  - LevelFailedChoice.Continue + ILevelFailedView.OnContinueClicked — 4th LevelFailed option; _continueButton/_continueCostText wired in prefab
  - ShopPresenter — 3 coin pack tiers (500/1200/2500), fake IAP, coins granted on purchase; stays open after purchase; balance shown on open and after buy
  - IShopView / ShopView / ShopPopup.prefab — shop UI wired in Boot scene and MainMenu ShopPanel
  - UnityViewContainer sort-order scheme — base 200, blocker base 100, blocker stacked 250 (between depth-0 and depth-1 popup); GraphicRaycaster added to popup root canvas
  - ICurrencyOverlay / UnityCurrencyOverlay — Canvas sort 120, LitMotion fade, TMP_Text coin balance
  - InGameSceneController: Continue flow (spend coins or stack shop popup); overlay show/hide/update
  - MainMenuSceneController: Shop screen via InSceneScreenManager; ShopPresenter via direct SerializeField (not viewResolver)
  - GameBootstrapper: CoinsService + CurrencyOverlay constructed and wired
  - 193/193 EditMode tests; 9 new CoinsService tests + 10 InSceneScreenManager tests + 4 sort-order tests
slices_completed: [S01, S02, S03, S04]
bugs_fixed_post_completion:
  - Popup sort order base 50 → 200 (single popup was below blocker, visually dimmed)
  - DismissPopupAsync only called Unblock() on last popup — Block/Unblock must be symmetric per popup
  - Quit path in HandleLevelFailedPopupAsync skipped DismissPopupAsync — blocker stayed active
  - GraphicRaycaster missing from popup root canvas — nested Canvas with overrideSorting breaks parent raycaster
  - _continueButton/_continueCostText not wired in LevelFailedPopup prefab
  - MainMenu ShopView resolved via viewResolver (Boot scene only) — fixed to direct SerializeField
  - All home content was at Canvas root, not inside HomePanel — SetActive(false) didn't hide it
  - ShopPresenter resolved WaitForResult on purchase — now only cancel closes the shop
status: complete
completed_at: 2026-03-18
---

# M009: In-Scene Screens, Popup Stack, Coins & Overlay HUD — COMPLETE

**All four slices delivered and all post-completion bugs resolved. 193/193 tests green.**

## What Was Built

**S01 (In-Scene Screen Manager):** `IInSceneScreenManager<TScreenId>` + `InSceneScreenManager<TScreenId>` in Core — SetActive panel swap, back stack, generic. MainMenu wired with Home and Shop panels (all home content inside HomePanel). 10 unit tests.

**S02 (Popup Stack Visual Layering):** `UnityViewContainer` assigns per-popup Canvas OverrideSorting (200, 300...) with GraphicRaycaster. Blocker at 100 base, 250 when stacking (sits between depth-0 and depth-1 popup). `PopupId.Shop`, `IShopView`/`ShopView`/`ShopPopup.prefab` added.

**S03 (Coins, Continue & Shop Flow):** `CoinsService` persisted in `MetaSaveData.coins`. `LevelFailedChoice.Continue` — button costs 100 coins, wired in prefab. Insufficient coins → shop popup stacks on LevelFailed; buy coins; balance updates; loop continues. Shop accessible from MainMenu screen with direct SerializeField wiring.

**S04 (Overlay HUD):** `ICurrencyOverlay` / `UnityCurrencyOverlay` on Canvas sort 120. LitMotion alpha fade. Shows coin balance. Appears when LevelFailed opens, updates after shop purchase, hides on dismiss.

## Requirements Addressed

- R084 ✓ — InSceneScreenManager drives MainMenu ↔ Shop screen swap
- R085 ✓ — Popup stacking with correct visual layering
- R086 ✓ — CoinsService persists coins in MetaSaveData
- R087 ✓ — LevelFailed Continue costs 100 coins; opens shop popup if insufficient
- R088 ✓ — Shop with 3 fake IAP coin packs; accessible from MainMenu and LevelFailed; stays open after purchase
- R089 ✓ — Overlay HUD with coin balance; contextual appear/disappear with LitMotion
