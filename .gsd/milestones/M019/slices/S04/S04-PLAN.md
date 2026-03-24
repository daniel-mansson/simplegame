# S04: Wire Presenters and UIFactory

**Goal:** Replace stub logic in ShopPresenter and IAPPurchasePresenter with real IIAPService calls. Update UIFactory and GameBootstrapper to inject IIAPService. IAPPurchase popup grants coins (not golden pieces).

**Demo:** In Editor with MockIAPService set to Success: tap a shop pack → status shows "Purchase complete! +500 coins". Set to Cancelled → "Purchase cancelled." Set to PaymentFailed → "Purchase failed. Please try again." Full flow works on device via UnityIAPService.

## Must-Haves

- `ShopPresenter` calls `IIAPService.BuyAsync(productId)` on pack click; shows status per outcome; no direct `ICoinsService.Earn` call (coins granted inside IIAPService on validation)
- `IAPPurchasePresenter` calls `IIAPService.BuyAsync(productId)` on purchase click; grants coins (not golden pieces); removes `_goldenPiecesGranted` field entirely
- `UIFactory` receives `IIAPService` in constructor, passes it to `CreateShopPresenter` and `CreateIAPPurchasePresenter`
- `GameBootstrapper` constructs `UnityIAPService` on device, `MockIAPService` in Editor (using `#if UNITY_EDITOR` or `Application.isEditor`)
- `UnityIAPService.InitializeAsync()` called early in boot sequence (after PlayFab login, before navigation loop)
- `IAPProductCatalog` passed to both presenters so pack labels come from catalog (not hardcoded strings)
- All existing EditMode tests still pass (mock interfaces unchanged)

## Tasks

- [ ] **T01: Rewrite ShopPresenter to use IIAPService**
  Remove hardcoded Packs array. Read products from IAPProductCatalog. On pack click: call BuyAsync, show status per outcome. Remove direct ICoinsService.Earn (coins now granted by IIAPService internally).

- [ ] **T02: Rewrite IAPPurchasePresenter to use IIAPService**
  Remove goldenPiecesGranted parameter. Call BuyAsync for the single product. Show status per outcome. Grants coins via IIAPService (not directly).

- [ ] **T03: UIFactory and GameBootstrapper wiring**
  Add IIAPService to UIFactory constructor and factory methods. Construct UnityIAPService (device) or MockIAPService (editor) in GameBootstrapper. Call InitializeAsync in boot sequence. Update InGameSceneController if it creates a UIFactory directly (check the NullGoldenPieceService pattern — may need IIAPService null path).

- [ ] **T04: Integration smoke test and cleanup**
  Run EditMode tests. Verify ShopPresenter and IAPPurchasePresenter compile. Remove any remaining stub golden-pieces language from IAPPurchaseView. Update STATE.md.

## Files Likely Touched

- `Assets/Scripts/Game/Popup/ShopPresenter.cs`
- `Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs`
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` (possibly — NullIAPService may be needed)
- `Assets/Scripts/Game/Services/NullIAPService.cs` (new — for scenes that don't need IAP)
- `Assets/Tests/EditMode/Game/PopupTests.cs` (update mocks if IIAPView changes)
