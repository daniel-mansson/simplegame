# M009: In-Scene Screens, Popup Stack, Coins & Overlay HUD

**Vision:** In-scene screen switching (no scene load), stackable popups with correct visual layering (bottom popup behind dim overlay, top popup above it), a coins currency with a Continue option on LevelFailed, a shop for coin packs, and a contextual overlay HUD showing coin balance.

## Success Criteria

- Opening the shop from the main menu swaps to a Shop screen within the MainMenu scene (no scene transition)
- A second popup can be shown on top of an existing popup; the bottom popup is visually dimmed by the blocker overlay; only the top popup is interactive
- `CoinsService` persists coins across sessions (stored in MetaSaveData)
- LevelFailed has a Continue button costing 100 coins; insufficient balance opens the shop popup stacked on top
- Shop has three coin pack tiers backed by fake IAP; purchase grants coins; balance updates on the stacked LevelFailed after shop closes
- Overlay HUD appears/disappears with LitMotion animation when shown/hidden explicitly; displays current coin balance

## Key Risks / Unknowns

- **Popup sort order for visual stacking** — each popup needs its own Canvas with OverrideSorting; `UnityViewContainer` must set sort order based on current stack depth (bottom < blocker 100 < top)
- **InSceneScreenManager generics** — must stay in Core with no game types; `MainMenuSceneController` provides the concrete enum

## Proof Strategy

- Sort order risk → retire in S02: unit-test that `UnityViewContainer` assigns correct sort orders; verify in play mode that bottom popup appears behind dim overlay
- InSceneScreenManager → retire in S01: Core unit tests confirm SetActive contract; MainMenu wires it in play mode

## Verification Classes

- Contract verification: EditMode tests; CoinsService balance/spend tests; stacking sort order tests; InSceneScreenManager tests
- Integration verification: LevelFailed→shop stack in play mode; coin balance persists after Unity restart
- Operational verification: none
- UAT / human verification: play through LevelFailed→can't afford→shop→buy→back to LevelFailed; main menu shop screen; overlay appears/disappears

## Milestone Definition of Done

This milestone is complete only when all are true:

- `InSceneScreenManager` drives MainMenu ↔ Shop screen swap (SetActive, back stack)
- Popup stacking allows multiple simultaneous popups; bottom popup is visually behind dim overlay (sort order scheme working)
- `CoinsService` persists coins in MetaSaveData; balance survives session restart
- LevelFailed Continue costs 100 coins; opens Shop popup when insufficient; purchase grants coins
- Shop accessible from MainMenu (screen) and LevelFailed (stacked popup)
- Overlay HUD shows coin balance contextually; animates in/out with LitMotion
- All existing 169+ EditMode tests green

## Requirement Coverage

- Covers: R084, R085, R086, R087, R088, R089
- Partially covers: none
- Leaves for later: R078 (popup-on-demand instantiation), overlay for other currency types
- Orphan risks: none

## Slices

- [x] **S01: In-Scene Screen Manager** `risk:medium` `depends:[]`
  > After this: `InSceneScreenManager<TScreenId>` in Core; MainMenu has Home and Shop screens; tapping Shop swaps to the shop panel; Back returns to Home.

- [x] **S02: Popup Stack Visual Layering** `risk:medium` `depends:[]`
  > After this: `UnityViewContainer` assigns Canvas sort orders per stack depth; bottom popup is below blocker (sort 50), top popup is above blocker (sort 150); test confirms ordering; `PopupId.Shop` added; ShopPopup prefab created.

- [x] **S03: Coins, Continue & Shop Flow** `risk:low` `depends:[S02]`
  > After this: `CoinsService` persists coins; LevelFailed has Continue (100 coins); can't afford → shop popup stacks; buy coins → shop closes → LevelFailed resumes with updated balance; shop also accessible from MainMenu screen.

- [x] **S04: Overlay HUD** `risk:low` `depends:[S03]`
  > After this: Overlay HUD canvas shows coin balance; animates in/out via LitMotion; appears when LevelFailed is shown (or any context that explicitly shows it); updates balance when coins change; disappears on dismiss.

## Boundary Map

### S01 → (S03)

Produces:
- `IInSceneScreenManager` — `ShowScreen(TScreenId)`, `GoBack()`, `CurrentScreen`, `CanGoBack`
- `InSceneScreenManager<TScreenId>` — plain C# class, `GameObject[]` or `Dictionary<TScreenId, GameObject>` panel map
- `MainMenuScreenId` enum — `Home`, `Shop`
- `MainMenuSceneController` — wired with `InSceneScreenManager<MainMenuScreenId>`; Shop screen panel added to MainMenu scene

Consumes: nothing (first slice)

### S02 → S03

Produces:
- `UnityViewContainer` — extended: assigns `Canvas` (Override Sort Order) per popup based on stack depth; base sort 50, increment 100 per depth (50, 150, 250...)
- `PopupId.Shop` — new enum value
- `ShopPopup` prefab asset in `Assets/Prefabs/Game/Popups/`
- `IShopView` / `ShopView` — three coin pack buttons + cancel; inherits `PopupViewBase`

Consumes: nothing (first slice)

### S03 → S04

Produces:
- `ICoinsService` / `CoinsService` — `Balance`, `Earn(int)`, `TrySpend(int)`, `Save()`, `ResetAll()`
- `MetaSaveData.coins` — new field persisted in existing JSON blob
- `LevelFailedChoice.Continue` — new enum value
- `ILevelFailedView.OnContinueClicked` — new event; `LevelFailedView` wires new button
- `LevelFailedPresenter` — handles Continue: checks balance, deducts or signals InsufficientFunds
- `IShopPresenter` / `ShopPresenter` — wraps IAPPurchasePresenter for three coin pack tiers; grants coins on purchase
- `InGameSceneController` — handles Continue: spend 100 coins → restore hearts; insufficient → show shop popup
- `MainMenuSceneController` — navigates to Shop screen via `InSceneScreenManager`
- `UIFactory` — `CreateShopPresenter(view)`, `CreateCoinsOverlayPresenter(view)`
- `GameBootstrapper` — constructs and wires `CoinsService`

Consumes from S01:
- `InSceneScreenManager<MainMenuScreenId>` — for main menu shop navigation
Consumes from S02:
- `PopupId.Shop`, `IShopView`, `UnityViewContainer` sort-order scheme

### S04 (terminal)

Produces:
- `ICurrencyOverlay` — `ShowAsync(CancellationToken)`, `HideAsync(CancellationToken)`, `UpdateBalance(string)`
- `UnityCurrencyOverlay` — MonoBehaviour, LitMotion fade in/out, coin balance TMP_Text; Canvas sort order between blocker (100) and popups (150+) → use sort order 120
- `GameBootstrapper` — wires overlay; passes to InGame/MainMenu controllers or UIFactory
- `InGameSceneController` — shows overlay when LevelFailed is open; hides after dismiss

Consumes from S03:
- `CoinsService` — for balance display and change notification
- `IShopView`, `ShopPresenter` — shop flow already wired
