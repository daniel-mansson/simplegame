---
estimated_steps: 4
estimated_files: 1
---

# T01: Add #if UNITY_EDITOR guard for MockIAPService in GameBootstrapper

**Slice:** SR01 — Restore MockIAPService in Editor Runtime
**Milestone:** M019

## Description

GameBootstrapper currently always constructs `UnityIAPService` for IAP, including in the Editor. This means the `IAPMockConfig` ScriptableObject (at `Assets/Resources/IAPMockConfig.asset`) is unused at runtime — developers cannot cycle through the four purchase outcomes (Success, Cancelled, PaymentFailed, ValidationFailed) via the config asset. The fix is a `#if UNITY_EDITOR` guard in the IAP construction block of `GameBootstrapper.Start()`, routing to `MockIAPService` in the Editor and keeping `UnityIAPService` on device. This follows the exact pattern used for `IATTService` (`#if UNITY_IOS` → real / `#else` → null) already in the same file.

## Steps

1. Open `Assets/Scripts/Game/Boot/GameBootstrapper.cs`. Find the IAP construction block (around lines 197–214). The block currently looks like:

```csharp
            // --- IAP: load catalog and construct service ---
            // UnityIAPService is used in both Editor (FakeStore with StandardUser UI — shows
            // Buy/Cancel per purchase, no dialog at init) and on device (real store).
            // UGS must be initialised above before this runs.
            // In the Editor: FakeStore receipt is fake so PlayFab validation will fail with
            // "Invalid receipt" — this is expected. Coin grant is skipped on validation failure.
            // To test coin grant flow without a device, use MockIAPService by swapping
            // the #if below temporarily, or set MOCK_IAP scripting symbol.
            _iapCatalog = UnityEngine.Resources.Load<IAPProductCatalog>("IAPProductCatalog");
            if (_iapCatalog == null)
                Debug.LogWarning("[GameBootstrapper] IAPProductCatalog not found in Resources. Run Tools/Setup/Create IAP Assets.");

            var catalogService = _authService.IsLoggedIn
                ? (IPlayFabCatalogService)new PlayFabCatalogService(_iapCatalog)
                : new NullPlayFabCatalogService(_iapCatalog);

            _iapService = new UnityIAPService(_iapCatalog, catalogService, _coinsService, _authService);
            Debug.Log("[GameBootstrapper] IAP: UnityIAPService (FakeStore/StandardUser in Editor, real store on device).");
            await _iapService.InitializeAsync();
```

2. Replace that entire block with:

```csharp
            // --- IAP: load catalog and construct service ---
            _iapCatalog = UnityEngine.Resources.Load<IAPProductCatalog>("IAPProductCatalog");
            if (_iapCatalog == null)
                Debug.LogWarning("[GameBootstrapper] IAPProductCatalog not found in Resources. Run Tools/Setup/Create IAP Assets.");

#if UNITY_EDITOR
            var mockConfig = UnityEngine.Resources.Load<IAPMockConfig>("IAPMockConfig");
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

Key points:
- `_iapCatalog` load stays OUTSIDE the `#if` — both branches need it.
- `await _iapService.InitializeAsync()` stays OUTSIDE — MockIAPService.InitializeAsync() returns `UniTask.CompletedTask` so this is a no-op.
- `PlayFabCatalogService` / `NullPlayFabCatalogService` construction moves inside `#else` — Editor doesn't need it.
- The stale comments about "FakeStore", "MOCK_IAP scripting symbol", "UGS must be initialised" are all removed — replaced by the self-documenting `#if UNITY_EDITOR` guard.

3. Verify `MockIAPService` and `IAPMockConfig` are already imported. Check the `using` directives at the top of `GameBootstrapper.cs` — both types live in `SimpleGame.Game.Services` namespace which should already be imported. If not, add the using.

4. Run verification:
   - `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → confirms MockIAPService referenced
   - `rg "UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → confirms guard exists
   - `rg "MOCK_IAP" Assets/Scripts/` → zero matches (stale comment removed)
   - Run EditMode tests via Unity MCP:
     ```
     echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin
     ```
     Then poll with `mcporter call unityMCP.get_test_job job_id=<id>` until `status == "succeeded"`.
     Expect: all 347+ pass, 0 fail.

## Observability Impact

**Signals added:**
- `[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset.` — logged every time the Editor boot path executes. A future agent can grep the Unity Console or log file for this string to confirm the mock is active.
- `[GameBootstrapper] IAP: UnityIAPService (device).` — emitted only in non-Editor builds (the `#else` branch), confirming real-store path is in use.

**How to inspect:**
- In the Editor: open the Console, filter `[GameBootstrapper]`, enter Play mode, and verify which IAP log line appears.
- `rg "#if UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` confirms the guard is present.
- `rg "UnityIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` must only match inside the `#else` block.

**Failure state visibility:** if `IAPMockConfig.asset` is missing, `mockConfig` is null and `MockIAPService` receives a null config — purchases silently default to Success. No warning is currently emitted for this case. If the `#if` guard is stripped, the FakeStore receipt path runs and PlayFab validation logs `Invalid receipt` in the Console.

## Must-Haves

- [ ] `#if UNITY_EDITOR` guard wraps IAP service construction in GameBootstrapper
- [ ] Editor branch loads `IAPMockConfig` from Resources and constructs `MockIAPService`
- [ ] Device branch (`#else`) keeps existing `UnityIAPService` + `PlayFabCatalogService` construction unchanged
- [ ] Stale "MOCK_IAP scripting symbol" comment removed
- [ ] `_iapCatalog` load and `await _iapService.InitializeAsync()` remain outside the `#if` block
- [ ] All 347+ EditMode tests pass

## Verification

- `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` returns a match
- `rg "MOCK_IAP" Assets/Scripts/` returns no matches
- `rg "#if UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` returns a match in the IAP block
- EditMode tests: 347+ pass, 0 fail

## Inputs

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — current file with unconditional UnityIAPService construction at ~line 197–214
- `Assets/Scripts/Game/Services/MockIAPService.cs` — already exists, constructor: `MockIAPService(IAPMockConfig config, ICoinsService coins = null, IAPProductCatalog catalog = null)`
- `Assets/Scripts/Game/Services/IAPMockConfig.cs` — already exists, exposes `MockOutcome` and `CoinsGranted`
- `Assets/Resources/IAPMockConfig.asset` — already exists

## Expected Output

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — modified with `#if UNITY_EDITOR` / `#else` / `#endif` guard around IAP service construction; stale comments removed; MockIAPService wired in Editor, UnityIAPService on device
