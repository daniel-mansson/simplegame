# M019: Real IAP — Unity Purchasing + PlayFab Receipt Validation

**Vision:** Replace both stub IAP surfaces (Shop panel + IAPPurchase popup) with real Unity Purchasing backed by PlayFab server-side receipt validation. Editor gets a mock service covering all outcomes. iOS and Android both wired from the start.

## Success Criteria

- Tapping any coin pack in the Shop panel on a real device triggers the native store purchase sheet, completes a test payment, sends the receipt to PlayFab for validation, and adds coins to the player's balance
- The IAPPurchase popup does the same for its single product (and grants coins, not golden pieces)
- In the Editor, MockIAPService cycles through success / payment failed / PlayFab validation failed / user cancelled without any SDK or device
- All coin grant paths go through ICoinsService — no direct balance manipulation in presenters
- Compile clean on iOS and Android targets
- PlayFab ValidateIOSReceipt and ValidateGooglePlayPurchase called with correct receipt data per platform

## Key Risks / Unknowns

- `com.unity.purchasing` not yet in manifest — domain reload after install may require asmdef repair
- PlayFab catalog product IDs must match Unity Purchasing IDs exactly — mismatch causes silent validation failure
- `ConfirmPendingPurchase` timing — must not be called before PlayFab validation result returns

## Proof Strategy

- Unity Purchasing install risk → retire in S02 by confirming products load from store on device (sandbox)
- PlayFab validation bridge → retire in S03 by sending a real sandbox receipt and checking PlayFab dashboard for the validated transaction

## Verification Classes

- Contract verification: EditMode tests exercise all MockIAPService outcomes; IAPProductCatalog ScriptableObject has correct structure
- Integration verification: UnityIAPService initialises, products load, receipt sent to PlayFab and validated on device
- Operational verification: ConfirmPendingPurchase called only after PlayFab validation; app-backgrounding mid-purchase recovers correctly
- UAT / human verification: real test payment on iOS sandbox and Android test account

## Milestone Definition of Done

This milestone is complete only when all are true:

- All four slices complete with passing tests
- UnityIAPService initialises without errors on iOS and Android builds
- Sandbox purchase on both platforms triggers PlayFab validation and coins are granted
- Editor mock covers all four outcomes and EditMode tests pass
- IAPPurchase popup grants coins (not golden pieces) in all paths
- IAPProductCatalog ScriptableObject is the only place product IDs and coin amounts are defined

## Requirement Coverage

- Covers: R165, R166, R167, R168, R169, R170, R171, R172
- Partially covers: none
- Leaves for later: none
- Orphan risks: none

## Slices

- [x] **S01: IIAPService abstraction, mock, and product catalog** `risk:high` `depends:[]`
  > After this: Editor mock service is injectable everywhere; all four purchase outcomes work in EditMode tests; IAPProductCatalog ScriptableObject defines the three coin packs.

- [x] **S02: Unity Purchasing SDK integration** `risk:high` `depends:[S01]`
  > After this: com.unity.purchasing in manifest; UnityIAPService initialises and loads products from the store; sandbox product list visible in Editor (FakeStore) and on device.

- [x] **S03: PlayFab receipt validation** `risk:medium` `depends:[S02]`
  > After this: Sandbox receipt from Unity Purchasing is sent to PlayFab ValidateIOSReceipt / ValidateGooglePlayPurchase; coins granted only after validation succeeds; ConfirmPendingPurchase called at the right moment.

- [x] **S04: Wire presenters and UIFactory** `risk:low` `depends:[S03]`
  > After this: ShopPresenter and IAPPurchasePresenter both call IIAPService; IAPPurchase popup grants coins (not golden pieces); UIFactory and GameBootstrapper pass IIAPService through; full end-to-end purchase flow works in Editor (mock) and on device (real).

## Boundary Map

### S01 → S02

Produces:
- `IIAPService` interface: `InitializeAsync()`, `BuyAsync(productId)` → `IAPResult`, `IsInitialized`
- `IAPResult` type: `{ Success, Cancelled, PaymentFailed, ValidationFailed }` enum + `coinsGranted int`
- `MockIAPService` class: outcome driven by `IAPMockConfig` ScriptableObject
- `IAPMockConfig` ScriptableObject: `MockOutcome` enum field, `CoinsGranted` int
- `IAPProductCatalog` ScriptableObject: `IAPProductDefinition[]` — each has `ProductId string`, `CoinsAmount int`, `DisplayName string`
- All types in `SimpleGame.Game.Services` namespace

Consumes:
- nothing (first slice)

### S01 → S04

Produces:
- Same as S01 → S02 (IIAPService, IAPResult, IAPProductCatalog all consumed by presenters in S04)

### S02 → S03

Produces:
- `UnityIAPService` class implementing `IIAPService`
- Unity Purchasing initialised, `IStoreController` and `IExtensionProvider` stored
- `BuyAsync(productId)` triggers store transaction, returns pending result awaitable
- Raw receipt data accessible per platform after transaction completes (before ConfirmPendingPurchase)

Consumes from S01:
- `IIAPService` interface
- `IAPResult` type
- `IAPProductCatalog` (product IDs passed to Unity Purchasing at init)

### S03 → S04

Produces:
- `PlayFabIAPValidationService` (or validation logic embedded in `UnityIAPService`): calls `ValidateIOSReceipt` / `ValidateGooglePlayPurchase` and resolves `IAPResult`
- `ConfirmPendingPurchase` called on `IStoreController` after PlayFab success
- `ICoinsService.Earn` + `Save()` called after validated success
- Full `BuyAsync` returns resolved `IAPResult` to caller

Consumes from S02:
- `UnityIAPService.BuyAsync` raw transaction path
- `IStoreController.ConfirmPendingPurchase`

Consumes from S01:
- `IIAPService`, `IAPResult`, `IAPProductCatalog`
- `ICoinsService` (passed in via constructor)
