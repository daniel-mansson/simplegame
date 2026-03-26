# SR01: Restore MockIAPService in Editor Runtime

**Goal:** GameBootstrapper uses MockIAPService under `#if UNITY_EDITOR` so all four IAP outcomes (Success, Cancelled, PaymentFailed, ValidationFailed) are selectable via `IAPMockConfig.asset` at runtime in the Editor. UnityIAPService used on device only.
**Demo:** In the Editor, changing `IAPMockConfig.asset`'s MockOutcome field and buying a coin pack in the Shop or IAPPurchase popup produces the selected outcome. On device builds, UnityIAPService + PlayFab validation path is unchanged.

## Must-Haves

- `#if UNITY_EDITOR` guard in GameBootstrapper routes to MockIAPService in Editor
- `#else` branch keeps existing UnityIAPService + PlayFabCatalogService construction for device
- Stale "set MOCK_IAP scripting symbol" comment removed
- `await _iapService.InitializeAsync()` works unchanged for both paths
- All 347+ EditMode tests pass with 0 failures

## Verification

- `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → match found inside `#if UNITY_EDITOR` block
- `rg "UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → guard present around IAP construction
- `rg "MOCK_IAP" Assets/Scripts/` → zero matches (stale comment removed)
- EditMode test run → all 347+ pass, 0 fail (via Unity MCP `run_tests`)

## Tasks

- [x] **T01: Add #if UNITY_EDITOR guard for MockIAPService in GameBootstrapper** `est:20m`
  - Why: GameBootstrapper currently always constructs UnityIAPService, making the IAPMockConfig asset unused at runtime. This is the only code change needed to satisfy R166 and the milestone success criterion for Editor mock coverage.
  - Files: `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
  - Do: Replace the IAP construction block (~lines 197–214) with a `#if UNITY_EDITOR` / `#else` / `#endif` guard. In the Editor branch: load `IAPMockConfig` from Resources, construct `MockIAPService(mockConfig, _coinsService, _iapCatalog)`, log a message. In the `#else` branch: keep the existing `PlayFabCatalogService` / `NullPlayFabCatalogService` + `UnityIAPService` construction unchanged. Remove the stale comment about "set MOCK_IAP scripting symbol". The `_iapCatalog` load and `await _iapService.InitializeAsync()` remain outside both branches.
  - Verify: `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` shows match; `rg "MOCK_IAP" Assets/Scripts/` returns no matches; EditMode tests all pass via Unity MCP
  - Done when: `#if UNITY_EDITOR` guard is in GameBootstrapper, MockIAPService is wired in Editor, UnityIAPService on device, stale comment gone, all tests pass

## Observability / Diagnostics

**Runtime signals added by this slice:**
- `[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset.` — logged at boot whenever the Editor path is taken. Confirms mock is active; absence means the `#if UNITY_EDITOR` guard was compiled out.
- `[GameBootstrapper] IAP: UnityIAPService (device).` — logged in device/non-Editor builds; confirms the real IAP path is in use.

**Inspection surfaces:**
- `IAPMockConfig.asset` (Assets/Resources/) — change `MockOutcome` in the Inspector to switch between Success / Cancelled / PaymentFailed / ValidationFailed before entering Play mode.
- Unity Console: filter by `[GameBootstrapper]` to verify which IAP branch ran.

**Failure visibility:**
- If `IAPMockConfig` is null (asset deleted or not in Resources/), `MockIAPService` still constructs with `config=null` — the mock treats null config as `Success` outcome, so purchases always succeed silently. The missing-asset case is not currently guarded with a warning; this is intentional (low-risk default).
- If the `#if UNITY_EDITOR` block is accidentally removed, the device IAP path runs in the Editor; the FakeStore receipt fails PlayFab validation — visible as `[UnityIAPService] Receipt validation failed: Invalid receipt` in the Console.

**Redaction constraints:** no secrets or user-identifiable data flow through the mock path.

**Failure-path verification check:** `rg "UnityIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` must only match lines inside the `#else` block — a match outside signals the guard was lost.

## Files Likely Touched

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
