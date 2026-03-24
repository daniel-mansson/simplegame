# T02: MockIAPService Implementation and EditMode Tests

**Slice:** S01
**Milestone:** M019

## Goal

Implement MockIAPService driven by IAPMockConfig, create the two ScriptableObject assets, and write EditMode tests covering all four purchase outcomes.

## Must-Haves

### Truths
- `MockIAPService.InitializeAsync()` completes immediately (synchronous resolve), sets `IsInitialized = true`
- `MockIAPService.BuyAsync(productId)` returns an `IAPResult` matching the `IAPMockConfig.MockOutcome` setting: Success → CoinsGranted from config, Cancelled → CoinsGranted = 0, PaymentFailed → CoinsGranted = 0, ValidationFailed → CoinsGranted = 0
- `IAPMockConfig.asset` exists at `Assets/Resources/IAPMockConfig.asset`, default outcome: `Success`, default CoinsGranted: `500`
- `IAPProductCatalog.asset` exists at `Assets/Resources/IAPProductCatalog.asset` with three products: `com.simplegame.coins.500` / 500 / "500 Coins", `com.simplegame.coins.1200` / 1200 / "1200 Coins", `com.simplegame.coins.2500` / 2500 / "2500 Coins"
- EditMode test `IAPServiceTests.cs` has four test methods all passing: `MockIAP_Success_ReturnsCoins`, `MockIAP_Cancelled_ReturnsZeroCoins`, `MockIAP_PaymentFailed_ReturnsFailure`, `MockIAP_ValidationFailed_ReturnsFailure`

### Artifacts
- `Assets/Scripts/Game/Services/MockIAPService.cs` — min 40 lines, implements IIAPService, reads from IAPMockConfig
- `Assets/Resources/IAPMockConfig.asset` — ScriptableObject asset with default Success/500
- `Assets/Resources/IAPProductCatalog.asset` — ScriptableObject asset with three products
- `Assets/Tests/EditMode/Game/IAPServiceTests.cs` — min 60 lines, four test methods, all pass

### Key Links
- `MockIAPService` → `IAPMockConfig` via `Resources.Load<IAPMockConfig>("IAPMockConfig")`
- `IAPServiceTests` → `MockIAPService` direct construction (no MonoBehaviour, plain C# test)

## Steps

1. Read `Assets/Tests/EditMode/Game/PopupTests.cs` to understand existing EditMode test structure and NUnit patterns in use
2. Implement `MockIAPService.cs` — constructor takes `IAPMockConfig`; `BuyAsync` uses `UniTask.FromResult` with a result derived from config outcome; log each outcome with `[MockIAP]` prefix
3. Create `IAPMockConfig.asset` in `Assets/Resources/` via `ScriptableObject.CreateInstance` in an Editor script, or document that it needs to be created from the Unity Editor — actually, write an `IAPAssetCreator.cs` editor script under `Assets/Editor/` that creates both assets if missing (same pattern as existing Editor scripts)
4. Create `IAPProductCatalog.asset` with three products
5. Write `IAPServiceTests.cs` — each test creates a fresh `IAPMockConfig`, sets outcome, constructs `MockIAPService`, calls `BuyAsync`, asserts result fields
6. Run EditMode tests via `mcporter call unityMCP.run_tests` (see K006 for Windows workaround)
7. Confirm all four pass

## Context

- `Resources.Load` requires the asset to exist in an `Assets/Resources/` folder — create the folder if it doesn't exist
- UniTask `await` in EditMode tests: use `UniTask.ToCoroutine` or Unity Test Framework's async test support (check existing test patterns first)
- MockIAPService should log `[MockIAP] BuyAsync({productId}) → {outcome}` so Editor runs are traceable
- K003: new test files may not be detected until Editor is focused/recompiled
