# S02: Unity Purchasing SDK Integration

**Goal:** Add `com.unity.purchasing` to the project, update the asmdef, and implement `UnityIAPService` up to the point where store transactions trigger. PlayFab validation is S03's job — this slice proves the store layer works.

**Demo:** Products load from the store in the Unity Editor FakeStore and on a sandbox device. `BuyAsync` can be called and reaches the `OnPurchaseFailed` / `ProcessPurchase` callback. No PlayFab calls yet.

## Must-Haves

- `com.unity.purchasing` in `Packages/manifest.json`
- `SimpleGame.Game.asmdef` references `UnityEngine.Purchasing`
- `UnityIAPService` implements `IIAPService`, uses `IStoreListener`, initialises via `UnityPurchasing.Initialize`
- Products loaded from `IAPProductCatalog` (all three packs registered as `ProductType.Consumable`)
- `BuyAsync(productId)` calls `_controller.InitiatePurchase(productId)`, returns a pending `UniTask<IAPResult>` resolved in `ProcessPurchase` or `OnPurchaseFailed`
- `IsInitialized` returns true after `OnInitialized` fires
- Compile clean on iOS and Android scripting backends

## Tasks

- [ ] **T01: Add com.unity.purchasing to manifest and asmdef**
  Edit manifest.json to add the package. Update SimpleGame.Game.asmdef. Confirm compile in Unity Editor.

- [ ] **T02: UnityIAPService — init and product registration**
  Implement IStoreListener. Initialize with products from IAPProductCatalog. Store IStoreController and IExtensionProvider. Handle OnInitializeFailed.

- [ ] **T03: UnityIAPService — BuyAsync and transaction callbacks**
  Wire BuyAsync to InitiatePurchase. Handle ProcessPurchase (success path — return pending result for S03 to complete) and OnPurchaseFailed (resolve IAPResult with appropriate outcome). Pending purchase token stored for S03 to call ConfirmPendingPurchase.

## Files Likely Touched

- `Packages/manifest.json`
- `Assets/Scripts/Game/SimpleGame.Game.asmdef`
- `Assets/Scripts/Game/Services/UnityIAPService.cs` (new)
