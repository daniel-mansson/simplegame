# T01: IIAPService Interface, IAPResult, and Supporting Types

**Slice:** S01
**Milestone:** M019

## Goal

Define all the types that IIAPService, MockIAPService, and the presenters will depend on. No implementation logic — just contracts and ScriptableObject definitions.

## Must-Haves

### Truths
- `IIAPService` interface exists in `SimpleGame.Game.Services` with: `UniTask<IAPResult> BuyAsync(string productId)`, `UniTask InitializeAsync()`, `bool IsInitialized`
- `IAPOutcome` enum has exactly four values: `Success`, `Cancelled`, `PaymentFailed`, `ValidationFailed`
- `IAPResult` struct has `IAPOutcome Outcome` and `int CoinsGranted` fields
- `IAPMockConfig` is a `ScriptableObject` with `[SerializeField] IAPMockOutcome MockOutcome` and `[SerializeField] int CoinsGranted`
- `IAPMockOutcome` enum has: `Success`, `Cancelled`, `PaymentFailed`, `ValidationFailed`
- `IAPProductDefinition` is a `[Serializable]` class/struct with `string ProductId`, `int CoinsAmount`, `string DisplayName`
- `IAPProductCatalog` is a `ScriptableObject` with `IAPProductDefinition[] Products`
- All files compile clean with no references to `UnityEngine.Purchasing`

### Artifacts
- `Assets/Scripts/Game/Services/IIAPService.cs` — interface (min 10 lines)
- `Assets/Scripts/Game/Services/IAPResult.cs` — IAPOutcome enum + IAPResult struct
- `Assets/Scripts/Game/Services/IAPMockConfig.cs` — ScriptableObject with MockOutcome + CoinsGranted
- `Assets/Scripts/Game/Services/IAPProductDefinition.cs` — serializable product definition
- `Assets/Scripts/Game/Services/IAPProductCatalog.cs` — ScriptableObject with Products array

### Key Links
- `IIAPService.BuyAsync` returns `UniTask<IAPResult>` — imports from Cysharp.Threading.Tasks (already in asmdef)
- `IAPMockConfig` and `IAPProductCatalog` use `[CreateAssetMenu]` so they can be created from the Unity Editor

## Steps

1. Create `IAPOutcome.cs` with the four-value enum in `SimpleGame.Game.Services`
2. Create `IAPResult.cs` — simple struct wrapping `IAPOutcome Outcome` and `int CoinsGranted`
3. Create `IIAPService.cs` — interface with three members; XML doc comments on each
4. Create `IAPMockOutcome.cs` (or fold into `IAPMockConfig.cs`) — enum for mock outcomes
5. Create `IAPMockConfig.cs` — ScriptableObject with `[CreateAssetMenu]`, two serialized fields
6. Create `IAPProductDefinition.cs` — `[Serializable]` class, three fields
7. Create `IAPProductCatalog.cs` — ScriptableObject with `[CreateAssetMenu]` and `IAPProductDefinition[]`
8. Run `lsp diagnostics` to confirm compile-clean

## Context

- UniTask is already referenced in `SimpleGame.Game.asmdef` — no new reference needed for this task
- Follow the pattern of `IAdService` and `IAnalyticsService` for the interface shape
- `IAPMockOutcome` and `IAPOutcome` can be the same enum — fold them to avoid duplication. The mock just reads `IAPOutcome` from the config.
- `[CreateAssetMenu(menuName = "SimpleGame/IAP/...")]` pattern used consistently across the project
