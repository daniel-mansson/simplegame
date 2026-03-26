# SR01 — Restore MockIAPService in Editor Runtime

**Date:** 2026-03-25
**Depth:** Light research — straightforward wiring change using established patterns

## Summary

GameBootstrapper currently always constructs `UnityIAPService` for IAP, including in the Editor. While `UnityIAPService` has internal `#if UNITY_EDITOR` branches that auto-validate FakeStore receipts and set shorter timeouts, this means the `IAPMockConfig` ScriptableObject (at `Assets/Resources/IAPMockConfig.asset`) is unused at runtime. The player/developer cannot cycle through the four purchase outcomes (Success, Cancelled, PaymentFailed, ValidationFailed) via the config asset — the FakeStore always presents a Buy/Cancel dialog instead.

The fix is a `#if UNITY_EDITOR` guard in `GameBootstrapper.Start()` at the IAP construction site, routing to `MockIAPService` in the Editor and `UnityIAPService` on device. This follows the exact pattern used for `IATTService` (`#if UNITY_IOS` → `UnityATTService` / `#else` → `NullATTService`) already in the same file. The `MockIAPService` class, `IAPMockConfig` ScriptableObject, and `IAPMockConfig.asset` all exist and are fully functional — they're just not wired in at boot.

## Recommendation

Add a `#if UNITY_EDITOR / #else / #endif` guard around IAP service construction in `GameBootstrapper.Start()`. In Editor: load `IAPMockConfig` from Resources, construct `MockIAPService` with the config + `_coinsService` + `_iapCatalog`. On device: keep the existing `UnityIAPService` construction unchanged. Remove the `PlayFabCatalogService` / `NullPlayFabCatalogService` construction from inside the `#if UNITY_EDITOR` branch since `MockIAPService` doesn't need it. Remove the stale comment about "set MOCK_IAP scripting symbol".

## Implementation Landscape

### Key Files

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — **the only file that needs a code change.** Lines ~155-170 construct `_iapService` as `UnityIAPService` unconditionally. Wrap in `#if UNITY_EDITOR` to use `MockIAPService` instead. The `_iapCatalog` load from Resources, `_coinsService`, and `IAPMockConfig` load from Resources are all available at that point in the boot sequence.
- `Assets/Scripts/Game/Services/MockIAPService.cs` — already exists, fully functional, constructor: `MockIAPService(IAPMockConfig config, ICoinsService coins = null, IAPProductCatalog catalog = null)`. No changes needed.
- `Assets/Scripts/Game/Services/IAPMockConfig.cs` — already exists, exposes `MockOutcome` (IAPOutcome enum) and `CoinsGranted` (int). No changes needed.
- `Assets/Resources/IAPMockConfig.asset` — already exists. No changes needed.
- `Assets/Tests/EditMode/Game/IAPServiceTests.cs` — already covers all four MockIAPService outcomes (6 tests). No new tests needed, but a verify pass should confirm all 347+ tests still pass.

### Current IAP Construction (GameBootstrapper ~line 155)

```csharp
// Current — always UnityIAPService:
_iapCatalog = Resources.Load<IAPProductCatalog>("IAPProductCatalog");
var catalogService = _authService.IsLoggedIn
    ? (IPlayFabCatalogService)new PlayFabCatalogService(_iapCatalog)
    : new NullPlayFabCatalogService(_iapCatalog);
_iapService = new UnityIAPService(_iapCatalog, catalogService, _coinsService, _authService);
await _iapService.InitializeAsync();
```

### Target IAP Construction

```csharp
_iapCatalog = Resources.Load<IAPProductCatalog>("IAPProductCatalog");
#if UNITY_EDITOR
var mockConfig = Resources.Load<IAPMockConfig>("IAPMockConfig");
_iapService = new MockIAPService(mockConfig, _coinsService, _iapCatalog);
Debug.Log("[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset.");
#else
var catalogService = _authService.IsLoggedIn
    ? (IPlayFabCatalogService)new PlayFabCatalogService(_iapCatalog)
    : new NullPlayFabCatalogService(_iapCatalog);
_iapService = new UnityIAPService(_iapCatalog, catalogService, _coinsService, _authService);
Debug.Log("[GameBootstrapper] IAP: UnityIAPService (device).");
#endif
await _iapService.InitializeAsync();
```

### Build Order

1. Edit `GameBootstrapper.cs` — add the `#if UNITY_EDITOR` guard
2. Clean up the stale comment about "set MOCK_IAP scripting symbol"
3. Verify compile clean
4. Run all EditMode tests (expect 347+ pass, 0 fail)

### Verification Approach

- `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → confirms MockIAPService referenced in bootstrapper
- `rg "UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → confirms the guard exists
- Compile clean check via Unity MCP `read_console`
- EditMode test run via `run_tests` → all pass

## Constraints

- `MockIAPService.InitializeAsync()` is a synchronous no-op returning `UniTask.CompletedTask` — the existing `await _iapService.InitializeAsync()` line works unchanged.
- `MockIAPService.Products` is populated from the catalog's local fallbacks in the constructor when a catalog is passed — no PlayFab fetch needed.
- The `PlayFabCatalogService` / `NullPlayFabCatalogService` construction is device-only now, so it moves inside the `#else` branch. This avoids an unnecessary PlayFab catalog fetch in the Editor.
