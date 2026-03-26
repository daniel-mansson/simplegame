---
milestone: M019
slice: S04
assessment_type: roadmap-reassessment
outcome: needs-remediation
---

# Roadmap Reassessment After S04

## Summary

S04 delivered the presenter wiring, UIFactory update, and GameBootstrapper IAP integration correctly. The core technical work (Unity Purchasing, PlayFab validation, coin grant pipeline, IAPProductCatalog as single source of truth) is complete and correct. One material gap was found that prevents milestone close: **GameBootstrapper unconditionally constructs `UnityIAPService` in the Editor instead of `MockIAPService`**, leaving `PaymentFailed` and `ValidationFailed` outcomes unreachable in runtime Editor Play Mode.

## Success Criterion Coverage

- **Tapping any coin pack in the Shop panel on a real device triggers purchase → PlayFab validation → coins** → SR01 (device path is correct; SR01 does not touch the device path)
- **IAPPurchase popup grants coins, not golden pieces** → ✅ fully delivered in S04 — no remaining slice needed
- **In the Editor, MockIAPService cycles through all four outcomes** → SR01 (GameBootstrapper must inject MockIAPService under `#if UNITY_EDITOR`)
- **All coin grant paths go through ICoinsService** → ✅ fully delivered in S04 — no remaining slice needed
- **Compile clean on iOS and Android** → ✅ fully delivered in S04 — no remaining slice needed
- **PlayFab ValidateIOSReceipt / ValidateGooglePlayPurchase called with correct receipt data** → ✅ fully delivered in S04 — no remaining slice needed

All criteria have at least one remaining owner. Coverage check passes.

## What Changed in the Roadmap

SR01 was added to the roadmap (a remediation slice). It is a small, well-scoped fix:

- **File:** `GameBootstrapper.cs` — wrap IAP service construction in `#if UNITY_EDITOR` / `#else` / `#endif`
- **Risk:** low — `MockIAPService` bypasses Unity Purchasing entirely; no SDK interaction; does not affect device builds
- **Boundary impact:** none — SR01 produces no new types; it only corrects which implementation is selected at Editor runtime
- The existing boundary map and proof strategy remain valid; no slice ordering changes are required

## Why This Gap Exists

The S04 summary listed `"Editor uses #if UNITY_EDITOR guard to route to MockIAPService"` as a key decision, but the actual `GameBootstrapper.cs` on disk does not implement this guard — a `git log` shows it was briefly implemented and then reverted (`"fix: revert to MockIAPService in Editor; FakeStore..."` appears in history, followed by a revert). The revert was the wrong call: `MockIAPService` bypasses `UnityIAPService+FakeStore` entirely and would have been the correct Editor path all along.

## Requirement Coverage

- R165–R171: addressed by S01–S04; no changes needed
- **R166** (Editor mock with selectable outcomes): partially addressed — MockIAPService exists and EditMode tests cover all four outcomes, but the runtime Editor gap means SR01 is required to fully satisfy this requirement
- R172 (sandbox device UAT): remains a human checklist item; unchanged

## Conclusion

Roadmap is sound with SR01 as the sole remaining work item. No slice reordering, merging, or splitting required. The fix is mechanical and low-risk.
