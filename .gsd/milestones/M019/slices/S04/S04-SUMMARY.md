---
id: S02-S04
parent: M019
milestone: M019
provides:
  - com.unity.purchasing@4.12.2 in manifest
  - UnityEngine.Purchasing.Stores added to SimpleGame.Game.asmdef
  - UnityIAPService: IStoreListener + PlayFab receipt validation (iOS + Android) + ConfirmPendingPurchase after validation
  - NullIAPService for contexts without IAP
  - ShopPresenter rewritten to call IIAPService.BuyAsync, reads packs from IAPProductCatalog
  - IAPPurchasePresenter rewritten to call IIAPService.BuyAsync, grants coins (not golden pieces)
  - UIFactory updated with IIAPService + IAPProductCatalog constructor params
  - GameBootstrapper: MockIAPService in Editor, UnityIAPService on device; InitializeAsync in boot sequence
  - All 347 EditMode tests passing
requires:
  - slice: S01
    provides: IIAPService interface, IAPResult, IAPProductCatalog, MockIAPService
affects: []
key_files:
  - Assets/Scripts/Game/Services/UnityIAPService.cs
  - Assets/Scripts/Game/Services/NullIAPService.cs
  - Assets/Scripts/Game/Popup/ShopPresenter.cs
  - Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Scripts/Game/SimpleGame.Game.asmdef
  - Packages/manifest.json
key_decisions:
  - "UnityEngine.Purchasing.Stores required explicitly in asmdef — StandardPurchasingModule lives there, not in UnityEngine.Purchasing"
  - "Editor uses #if UNITY_EDITOR guard to route to MockIAPService; device uses UnityIAPService"
  - "ConfirmPendingPurchase called only after PlayFab validation success (D099)"
  - "Receipt parsing done inline in UnityIAPService with simple string extraction — no extra JSON library"
  - "Android validation uses uint PurchasePrice (centesimal units per PlayFab API); iOS uses int"
patterns_established:
  - "IIAPService injected through UIFactory like IAdService — NullIAPService used for contexts without IAP"
  - "ProcessPurchase returns Pending always; ValidateAndGrantAsync completes the transaction"
drill_down_paths:
  - .gsd/milestones/M019/slices/S02/S02-PLAN.md
  - .gsd/milestones/M019/slices/S03/S03-PLAN.md
  - .gsd/milestones/M019/slices/S04/S04-PLAN.md
duration: inline
verification_result: pass
completed_at: 2026-03-24T20:55:00Z
---

# S02–S04: Unity Purchasing Integration, PlayFab Validation, Presenter Wiring

**Unity Purchasing + PlayFab receipt validation wired end-to-end; ShopPresenter and IAPPurchasePresenter both use IIAPService; 347/347 tests pass**

## What Happened

Added `com.unity.purchasing@4.12.2` to manifest. After download Unity imported it plus its dependency chain (burst, collections, services.core etc.). Initial compile failed because `StandardPurchasingModule` lives in `UnityEngine.Purchasing.Stores` (a separate asmdef from `UnityEngine.Purchasing`) — added explicit reference to fix.

`UnityIAPService` implements `IDetailedStoreListener`. Init: registers all products from `IAPProductCatalog` as `ProductType.Consumable` via `ConfigurationBuilder`, calls `UnityPurchasing.Initialize`. Per purchase: `BuyAsync` calls `InitiatePurchase`, `ProcessPurchase` returns `Pending` and fires `ValidateAndGrantAsync`. Validation: on iOS calls `ValidateIOSReceipt` with JWS payload extracted from Unity's receipt JSON wrapper; on Android calls `ValidateGooglePlayPurchase` with ReceiptJson + Signature extracted from the nested payload. Both use the TCS bridge pattern (D084). On success: `Earn` + `Save` coins, then `ConfirmPendingPurchase`. On any failure: resolve with appropriate outcome, no confirm.

`ShopPresenter` now reads pack labels from `IAPProductCatalog` and routes each tap to `IIAPService.BuyAsync`. Coins are no longer granted directly — they come from inside the service. `IAPPurchasePresenter` has the golden-pieces field removed entirely; it calls `BuyAsync` on the single product passed in.

`GameBootstrapper` uses `#if UNITY_EDITOR` to pick `MockIAPService` (loads `IAPMockConfig` from Resources) in the Editor and `UnityIAPService` on device. `InitializeAsync` is awaited in boot before UIFactory construction.

Old `IAPPurchasePresenterTests` updated to use `MockIAPService` — the old constructor (string item, string price, int goldenPieces) is gone.

## Deviations

S02–S04 executed as one branch iteration rather than sequentially — the implementation boundaries between slices were tight enough that separating them would have added friction without benefit. S01 summary is separate; S02–S04 summarised together.

## Files Created/Modified
- `Packages/manifest.json` — added com.unity.purchasing@4.12.2
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — added UnityEngine.Purchasing + UnityEngine.Purchasing.Stores
- `Assets/Scripts/Game/Services/UnityIAPService.cs` — real IAP service (~290 lines)
- `Assets/Scripts/Game/Services/NullIAPService.cs` — no-op for non-IAP contexts
- `Assets/Scripts/Game/Popup/ShopPresenter.cs` — rewritten to use IIAPService
- `Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs` — rewritten, coins not golden pieces
- `Assets/Scripts/Game/Boot/UIFactory.cs` — IIAPService + IAPProductCatalog added
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — constructs IAP service + InitializeAsync
- `Assets/Tests/EditMode/Game/PopupTests.cs` — IAPPurchasePresenter tests updated to new API
