---
id: SR01
parent: M019
milestone: M019
provides:
  - "#if UNITY_EDITOR guard in GameBootstrapper routing IAP to MockIAPService in Editor, UnityIAPService on device"
requires:
  - slice: S04
    provides: ShopPresenter and IAPPurchasePresenter wired to IIAPService; UIFactory and GameBootstrapper passing IIAPService through
affects: []
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
key_decisions:
  - "D103: Use #if UNITY_EDITOR (not a custom scripting symbol) — automatic, zero-config mock activation in Editor"
patterns_established:
  - "#if UNITY_EDITOR / #else / #endif around IAP service construction in GameBootstrapper, matching the existing IATTService guard pattern"
observability_surfaces:
  - "[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset. — logged at boot whenever Editor path is taken"
  - "[GameBootstrapper] IAP: UnityIAPService (device). — logged in non-Editor builds"
drill_down_paths:
  - .gsd/milestones/M019/slices/SR01/tasks/T01-SUMMARY.md
duration: ~5m
verification_result: passed
completed_at: 2026-03-25
---

# SR01: Restore MockIAPService in Editor Runtime

**GameBootstrapper now routes IAP to MockIAPService under `#if UNITY_EDITOR` — the stale "set MOCK_IAP scripting symbol" dead end is removed, the mock activates automatically in every Editor build, and all four purchase outcomes are selectable via IAPMockConfig.asset without any device or sandbox credentials.**

## What Happened

S04 had left GameBootstrapper always constructing `UnityIAPService`, making `IAPMockConfig.asset` and the `MockIAPService` exist as infrastructure with no boot-time activation path. The milestone success criterion required Editor builds to use the mock automatically.

T01 replaced the unconditional IAP construction block (~lines 197–214 in GameBootstrapper.cs) with a `#if UNITY_EDITOR` / `#else` / `#endif` guard:

- **Editor branch:** loads `IAPMockConfig` from Resources, constructs `MockIAPService(mockConfig, _coinsService, _iapCatalog)`, logs the confirmation message.
- **Device branch (`#else`):** retains the existing `PlayFabCatalogService` / `NullPlayFabCatalogService` selection and `UnityIAPService` construction unchanged.
- `_iapCatalog` load and `await _iapService.InitializeAsync()` remain outside both branches — same code path for all targets.
- Stale comments ("FakeStore explanation", "set MOCK_IAP scripting symbol" guidance) removed.

The `#if UNITY_EDITOR` approach was chosen over a custom scripting define because it requires zero developer action — the mock is always active in the Editor and never active in builds. This matches the existing `IATTService` guard pattern already present in GameBootstrapper at line 123.

## Verification

All four checks from the slice plan passed:

| # | Check | Result |
|---|-------|--------|
| 1 | `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` | ✅ 2 matches — both inside `#if UNITY_EDITOR` |
| 2 | `rg "UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` | ✅ guard present on IAP block |
| 3 | `rg "MOCK_IAP" Assets/Scripts/` | ✅ exit 1 — zero matches, stale comment fully removed |
| 4 | Unity MCP run_tests EditMode (job b4fb7bd2…) | ✅ 347 passed, 0 failed, 0 skipped |

Additional failure-path check per the observability section:
- `rg "UnityIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → 2 matches, both inside the `#else` block only. Guard is intact.

## Requirements Advanced

- **R166** — Editor mock IAP with selectable outcomes: GameBootstrapper now automatically routes to MockIAPService in Editor. All four outcomes configurable via IAPMockConfig.asset.

## Requirements Validated

- **R166** — Fully validated by SR01. Evidence: `#if UNITY_EDITOR` guard in GameBootstrapper confirmed by grep; 347/347 EditMode tests pass; `rg "MOCK_IAP"` returns zero (stale guidance removed).

## New Requirements Surfaced

None.

## Requirements Invalidated or Re-scoped

None.

## Deviations

None. The edit matched the plan exactly. The only pre-flight work was adding `## Observability / Diagnostics` to SR01-PLAN.md and `## Observability Impact` to T01-PLAN.md before executing.

## Known Limitations

- If `IAPMockConfig.asset` is deleted from `Assets/Resources/`, `mockConfig` is null at boot. `MockIAPService` with a null config defaults to `Success` outcome silently — no warning is emitted. This is intentional (low-risk, documented in the plan).
- The `#if UNITY_EDITOR` guard does not cover Editor builds exported as development builds — those take the `#else` (device) path. This is correct behavior; the mock is only for in-Editor play.

## Follow-ups

None. SR01 closes the M019 remediation gap and completes the milestone.

## Files Created/Modified

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — replaced unconditional `UnityIAPService` construction with `#if UNITY_EDITOR` / `#else` / `#endif` guard; stale comments removed

## Forward Intelligence

### What the next milestone should know
- The `#if UNITY_EDITOR` / `#else` / `#endif` pattern for service construction in GameBootstrapper is now established for two services (IATTService at line 123, IIAPService at line 201). Any future service that needs a mock in the Editor and a real implementation on device should follow this exact pattern.
- MockIAPService, IAPMockConfig, and IAPProductCatalog are all in `Assets/Resources/` — any asset deletion from that folder silently degrades the mock (no error, just default Success behavior).

### What's fragile
- `IAPMockConfig.asset` null-safety — MockIAPService handles null gracefully but without a warning. If future requirements add configurable defaults or stricter null handling, this is the place to add a `Debug.LogWarning`.

### Authoritative diagnostics
- Boot console log `[GameBootstrapper] IAP:` — filter by this prefix to instantly confirm which IAP path ran.
- `rg "UnityIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` — both matches must be inside the `#else` block. A match outside the block means the guard was accidentally removed.
