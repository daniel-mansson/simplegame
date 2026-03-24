# S03: PlayFab Receipt Validation

**Goal:** Wire PlayFab receipt validation into `UnityIAPService`. After `ProcessPurchase` fires, send the receipt to PlayFab; grant coins and call `ConfirmPendingPurchase` only after PlayFab confirms. Handle all failure paths.

**Demo:** Sandbox purchase on device: PlayFab dashboard shows a validated transaction entry. Coins appear in-game balance. `ConfirmPendingPurchase` is NOT called on validation failure.

## Must-Haves

- `UnityIAPService` calls `PlayFabClientAPI.ValidateIOSReceipt` on iOS and `PlayFabClientAPI.ValidateGooglePlayPurchase` on Android after `ProcessPurchase`
- Uses TCS bridge pattern (D084) — same as PlayFabCloudSaveService
- `ICoinsService.Earn(coinsAmount)` + `ICoinsService.Save()` called after PlayFab success
- `IStoreController.ConfirmPendingPurchase(product)` called only after PlayFab success
- On PlayFab validation failure: `BuyAsync` resolves with `IAPOutcome.ValidationFailed`, no coins granted, no ConfirmPendingPurchase
- On Unity Purchasing failure: `BuyAsync` resolves with `IAPOutcome.PaymentFailed`, no PlayFab call
- `IPlayFabAuthService.IsLoggedIn` checked before calling PlayFab — if not logged in, resolve with `ValidationFailed` and log warning
- Receipt data extracted per platform: iOS uses `product.receipt` (contains JwsReceiptData); Android uses `GooglePlay.GetProductJWSToken` or parses ReceiptJson + Signature from `product.receipt`

## Tasks

- [ ] **T01: PlayFab validation call — iOS path**
  Extract iOS receipt from product, call ValidateIOSReceipt via TCS bridge, handle success/failure.

- [ ] **T02: PlayFab validation call — Android path**
  Extract Google Play ReceiptJson and Signature, call ValidateGooglePlayPurchase via TCS bridge.

- [ ] **T03: Post-validation coin grant and ConfirmPendingPurchase**
  After PlayFab success on either platform: call ICoinsService.Earn + Save, call ConfirmPendingPurchase, resolve BuyAsync with Success result. Wire ICoinsService into UnityIAPService constructor.

## Files Likely Touched

- `Assets/Scripts/Game/Services/UnityIAPService.cs` (extend)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` (pass ICoinsService into UnityIAPService)
