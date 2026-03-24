# S01: IIAPService Abstraction, Mock, and Product Catalog

**Goal:** Establish the IIAPService interface, all supporting types, the mock implementation, and the IAPProductCatalog ScriptableObject. No Unity Purchasing SDK required. All four purchase outcomes exercisable in EditMode tests.

**Demo:** EditMode test suite runs four test cases (success, cancelled, payment failed, validation failed) against MockIAPService. IAPProductCatalog asset exists with three coin packs. No device or store SDK needed.

## Must-Haves

- `IIAPService` interface exists with `InitializeAsync()`, `BuyAsync(productId)` → `UniTask<IAPResult>`, `IsInitialized` property
- `IAPResult` is a struct/class with `Outcome` (enum: Success, Cancelled, PaymentFailed, ValidationFailed) and `CoinsGranted` (int)
- `MockIAPService` implements `IIAPService`, reads outcome from `IAPMockConfig` ScriptableObject
- `IAPMockConfig` ScriptableObject: `MockOutcome` enum field + `CoinsGranted` int (used when outcome is Success)
- `IAPProductCatalog` ScriptableObject: array of `IAPProductDefinition` — each has `ProductId`, `CoinsAmount`, `DisplayName`
- Three coin packs defined in a `IAPProductCatalog` asset: 500 coins / €1.99, 1200 coins / €3.99, 2500 coins / €7.99 — product IDs TBD (placeholder IDs for now, real IDs wired when store listings created)
- EditMode tests: four test cases covering all MockIAPService outcomes pass
- All types compile clean; `SimpleGame.Game.asmdef` does NOT yet reference `UnityEngine.Purchasing`

## Tasks

- [ ] **T01: IIAPService interface, IAPResult type, and supporting types**
  Define IIAPService, IAPResult (with Outcome enum), IAPMockConfig ScriptableObject, and IAPProductCatalog ScriptableObject. No implementation yet.

- [ ] **T02: MockIAPService implementation and EditMode tests**
  Implement MockIAPService reading from IAPMockConfig. Write four EditMode tests covering all outcomes. Create the IAPProductCatalog asset with three coin pack definitions.

## Files Likely Touched

- `Assets/Scripts/Game/Services/IIAPService.cs` (new)
- `Assets/Scripts/Game/Services/IAPResult.cs` (new)
- `Assets/Scripts/Game/Services/IAPMockConfig.cs` (new ScriptableObject)
- `Assets/Scripts/Game/Services/IAPProductCatalog.cs` (new ScriptableObject)
- `Assets/Scripts/Game/Services/IAPProductDefinition.cs` (new)
- `Assets/Scripts/Game/Services/MockIAPService.cs` (new)
- `Assets/Resources/IAPMockConfig.asset` (new)
- `Assets/Resources/IAPProductCatalog.asset` (new)
- `Assets/Tests/EditMode/Game/IAPServiceTests.cs` (new)
