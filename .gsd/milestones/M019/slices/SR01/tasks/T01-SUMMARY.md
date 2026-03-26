---
id: T01
parent: SR01
milestone: M019
provides:
  - "#if UNITY_EDITOR guard in GameBootstrapper routing IAP to MockIAPService in Editor and UnityIAPService on device"
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
key_decisions:
  - Use #if UNITY_EDITOR (not a scripting symbol) so the guard is automatic — no manual scripting symbol toggling needed
patterns_established:
  - "#if UNITY_EDITOR / #else / #endif around service construction in GameBootstrapper, matching the existing IATTService pattern"
observability_surfaces:
  - "[GameBootstrapper] IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset. — logged at boot in Editor"
  - "[GameBootstrapper] IAP: UnityIAPService (device). — logged at boot in non-Editor builds"
duration: ~5m
verification_result: passed
completed_at: 2026-03-25
blocker_discovered: false
---

# T01: Add #if UNITY_EDITOR guard for MockIAPService in GameBootstrapper

**Replaced the unconditional `UnityIAPService` construction block in `GameBootstrapper.Start()` with a `#if UNITY_EDITOR` guard — Editor path now constructs `MockIAPService` with `IAPMockConfig`, device path retains `UnityIAPService` + `PlayFabCatalogService`; stale MOCK_IAP comment removed; all 347 EditMode tests pass.**

## What Happened

Found the IAP construction block at lines 197–214 of `GameBootstrapper.cs`. The `SimpleGame.Game.Services` namespace was already imported, so `MockIAPService` and `IAPMockConfig` needed no new `using`. Applied the edit in one pass: stale comments (FakeStore explanation, "set MOCK_IAP scripting symbol" guidance) removed; `_iapCatalog` load and `await _iapService.InitializeAsync()` stayed outside the `#if`; `PlayFabCatalogService`/`NullPlayFabCatalogService` construction moved entirely inside the `#else` branch.

Pre-flight fixes were also applied: added `## Observability / Diagnostics` with failure-path check to SR01-PLAN.md, and `## Observability Impact` to T01-PLAN.md.

## Verification

All four checks ran and passed:

1. `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → two matches (construction line + log line, both inside `#if UNITY_EDITOR`)
2. `rg "UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → match on `#if UNITY_EDITOR` in the IAP block
3. `rg "MOCK_IAP" Assets/Scripts/` → exit code 1, zero matches — stale comment fully removed
4. Unity MCP `run_tests` (EditMode) → job `b4fb7bd24dc446eabaf33979bc2c5a63` — 347 passed, 0 failed, 0 skipped in 13.6 s

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `rg "MockIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` | 0 | ✅ pass | <1s |
| 2 | `rg "UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs` | 0 | ✅ pass | <1s |
| 3 | `rg "MOCK_IAP" Assets/Scripts/` | 1 (no matches) | ✅ pass | <1s |
| 4 | Unity MCP run_tests EditMode (job b4fb7bd2…) — 347/347 passed | — | ✅ pass | 13.6s |

## Diagnostics

- **Check mock is active:** run in Play mode in the Editor; Console filter `[GameBootstrapper]` should show `IAP: MockIAPService (Editor). Set outcome via IAPMockConfig.asset.`
- **Check guard is present:** `rg "#if UNITY_EDITOR" Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- **Check UnityIAPService is device-only:** `rg "UnityIAPService" Assets/Scripts/Game/Boot/GameBootstrapper.cs` — both matches must be inside the `#else` block
- **IAPMockConfig.asset null risk:** if the asset is deleted from Resources, `mockConfig` is null; `MockIAPService` defaults to Success outcome silently (no warning emitted — this is the existing MockIAPService behavior)

## Deviations

None. The edit matched the plan exactly.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — replaced unconditional `UnityIAPService` construction with `#if UNITY_EDITOR` / `#else` / `#endif` guard; removed stale comments
- `.gsd/milestones/M019/slices/SR01/SR01-PLAN.md` — added `## Observability / Diagnostics` section (pre-flight fix)
- `.gsd/milestones/M019/slices/SR01/tasks/T01-PLAN.md` — added `## Observability Impact` section (pre-flight fix)
