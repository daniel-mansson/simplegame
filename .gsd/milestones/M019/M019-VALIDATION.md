---
verdict: needs-remediation
remediation_round: 0
---

# Milestone Validation: M019

## Success Criteria Checklist

- [x] **Tapping any coin pack in the Shop panel on a real device triggers the native store purchase sheet, completes a test payment, sends the receipt to PlayFab for validation, and adds coins to the player's balance** — evidence: `UnityIAPService.BuyAsync` → `InitiatePurchase` → `ProcessPurchase` → `ValidateAndGrantAsync` → `ValidateIOSReceipt`/`ValidateGooglePlayPurchase` → `Earn`+`Save`; `ShopPresenter` reads from `IIAPService.Products` and calls `BuyAsync` per tap. Code path is complete and correct. Device sandbox verification is a human UAT checklist item (not yet confirmed, which is expected for device tests).

- [x] **The IAPPurchase popup does the same for its single product (and grants coins, not golden pieces)** — evidence: `IAPPurchasePresenter` rewritten; no `goldenPiecesGranted` parameter anywhere; constructor takes `IAPProductInfo` + `IIAPService`; delegates to `BuyAsync`; `PopupTests.IAPPurchasePresenterTests` (6 tests) confirm the new API. Old `goldenPieces` path fully removed.

- [ ] **In the Editor, MockIAPService cycles through success / payment failed / PlayFab validation failed / user cancelled without any SDK or device** — **GAP**: `GameBootstrapper` unconditionally constructs `UnityIAPService` (with Unity Purchasing `FakeStore`) for both the Editor and device. `MockIAPService` is not injected at runtime in the Editor. In Editor, `FakeStore` only exposes a Buy/Cancel dialog: `PaymentFailed` and `ValidationFailed` cannot be triggered at runtime regardless of what `IAPMockConfig.asset` is set to. `MockIAPService` is correctly used in EditMode tests, which cover all four outcomes, but the runtime development workflow (change `IAPMockConfig.MockOutcome` → Press Play → see the outcome) is broken for two of the four outcomes. The S04 summary incorrectly lists `"Editor uses #if UNITY_EDITOR guard to route to MockIAPService"` as a key decision, but this was never implemented.

- [x] **All coin grant paths go through ICoinsService — no direct balance manipulation in presenters** — evidence: `ShopPresenter` and `IAPPurchasePresenter` hold `ICoinsService` only for reading `Balance` (display); `Earn`+`Save` are called inside `UnityIAPService.ValidateAndGrantAsync` and `MockIAPService.BuyAsync`, never in a presenter. Grep for `\.Earn` and `\.TrySpend` in both presenter files returns zero matches.

- [x] **Compile clean on iOS and Android targets** — evidence: `UnityEngine.Purchasing` and `UnityEngine.Purchasing.Stores` both added to `SimpleGame.Game.asmdef`; platform guards (`#if UNITY_IOS`, `#elif UNITY_ANDROID`) present in `ValidateAndGrantAsync`; `com.unity.purchasing@4.12.2` in `Packages/manifest.json`. No runtime TestResults confirm a build, but code structure is valid.

- [x] **PlayFab `ValidateIOSReceipt` and `ValidateGooglePlayPurchase` called with correct receipt data per platform** — evidence: `ValidateIOSAsync` extracts `Payload` (JWS) from Unity receipt JSON wrapper and passes as `JwsReceiptData`; `ValidateGooglePlayAsync` extracts `json` and `signature` from nested Google Play payload; both pass `CurrencyCode` and `PurchasePrice`; TCS bridge pattern (D084) used correctly. `ConfirmPendingPurchase` called only on `validated == true` path (line 263), never on failure path.

---

## Slice Delivery Audit

| Slice | Claimed | Delivered | Status |
|-------|---------|-----------|--------|
| S01 | IIAPService, IAPOutcome, IAPResult, MockIAPService, IAPMockConfig SO, IAPProductCatalog SO (3 packs), IAPProductDefinition, 11 EditMode tests | All types present on disk; IAPProductCatalog.asset has 3 packs (500/1200/2500, correct IDs); IAPMockConfig.asset defaults to Success/500; 10 `[Test]` methods in IAPServiceTests.cs (11 counting test structure — minor count discrepancy, all scenarios covered) | **pass** |
| S02 | com.unity.purchasing in manifest; UnityIAPService stub; asmdef updated | No separate S02-SUMMARY.md — bundled into S04 summary. Deliverables verified on disk: `manifest.json` has `com.unity.purchasing@4.12.2`; `SimpleGame.Game.asmdef` references `UnityEngine.Purchasing` and `UnityEngine.Purchasing.Stores`; `UnityIAPService.cs` exists (~290 lines) | **pass** |
| S03 | PlayFab validation calls; ConfirmPendingPurchase after validation; coins granted after success | No separate S03-SUMMARY.md — bundled into S04 summary. Deliverables verified: `ValidateIOSAsync` calls `PlayFabClientAPI.ValidateIOSReceipt`; `ValidateGooglePlayAsync` calls `PlayFabClientAPI.ValidateGooglePlayPurchase`; `ConfirmPendingPurchase` called at line 263 (after `validated == true` only); `Earn`+`Save` called before `ConfirmPendingPurchase` | **pass** |
| S04 | ShopPresenter + IAPPurchasePresenter use IIAPService; UIFactory updated; GameBootstrapper wires IAP; 347 tests pass | ShopPresenter rewritten (reads `Products`, calls `BuyAsync`); IAPPurchasePresenter rewritten (no golden pieces); UIFactory has `IIAPService` + `IAPProductCatalog` params and `CreateShopPresenter`/`CreateIAPPurchasePresenter` methods; GameBootstrapper constructs IAP service and awaits `InitializeAsync`; 340 `[Test]` methods on disk (~347 when counted by Unity runner). **Gap**: GameBootstrapper uses UnityIAPService in Editor, not MockIAPService — contradicts S04 summary's stated key decision and milestone success criterion. | **partial** |

---

## Cross-Slice Integration

### S01 → S02 boundary alignment

S01 produced `IIAPService` (interface with `BuyAsync`, `InitializeAsync`, `IsInitialized`). The boundary map also lists `IReadOnlyList<IAPProductInfo> Products` on the interface — this is **an addition beyond the original S01→S02 boundary** (`IAPProductInfo` and the `Products` property were added during S02-S04 execution). This is a coherent improvement (presenters read live catalog data from the service rather than the raw ScriptableObject) and is correctly documented. No integration gap.

### S01 → S04 boundary alignment

`IAPProductCatalog` is passed to `UIFactory` and used to construct `MockIAPService` in tests. Presenters read from `IIAPService.Products` (not `IAPProductCatalog` directly) — this is a clean improvement over the planned boundary. Consistent with D100.

### S02 → S03 boundary alignment

`UnityIAPService` implements both the store transaction layer (S02) and the PlayFab validation layer (S03) as a single class. The S04 summary explicitly notes: "S02–S04 executed as one branch iteration." The `BuyAsync` → `ProcessPurchase` → `ValidateAndGrantAsync` chain is internally coherent. `ConfirmPendingPurchase` is correctly deferred until after PlayFab success.

### S03 → S04 boundary alignment

`ICoinsService.Earn`+`Save` called inside `UnityIAPService.ValidateAndGrantAsync` (not in presenters) — correct per D101. `IAPResult.CoinsGranted` is returned to the caller for display purposes only. UIFactory passes `ICoinsService` to presenters for balance reads, not for grants.

### New boundary not in original plan: `IPlayFabCatalogService`

`IPlayFabCatalogService` / `PlayFabCatalogService` / `NullPlayFabCatalogService` were added to enable live PlayFab catalog data (display names, descriptions, icon URLs) to override local catalog values. This is a clean addition that satisfies D100 without hardcoding. Not in the original boundary map but not a gap — it is strictly additive.

---

## Requirement Coverage

| Requirement | Status | Notes |
|-------------|--------|-------|
| R165 — IIAPService abstraction | Addressed | Interface exists; MockIAPService, UnityIAPService, NullIAPService all implement it |
| R166 — Editor mock IAP with selectable outcomes | **Partial gap** | MockIAPService exists and is used in EditMode tests. But GameBootstrapper does NOT inject MockIAPService in the Editor at runtime — `IAPMockConfig.MockOutcome` has no effect in Play Mode. PaymentFailed and ValidationFailed are unreachable in the runtime Editor. |
| R167 — Unity Purchasing SDK integration | Addressed | `com.unity.purchasing@4.12.2` in manifest; `IStoreListener` implemented; `InitializeAsync` called at boot |
| R168 — PlayFab receipt validation (iOS + Android) | Addressed (code complete) | `ValidateIOSReceipt` and `ValidateGooglePlayPurchase` called correctly; coins granted only after validation; `ConfirmPendingPurchase` after success only |
| R169 — Coin packs as single source of truth | Addressed | `IAPProductCatalog.asset` is the only source; no hardcoded product IDs in any `.cs` file |
| R170 — Shop panel wired to real IAP | Addressed | `ShopPresenter` calls `IIAPService.BuyAsync`; no direct `ICoinsService.Earn` in presenter |
| R171 — IAPPurchase popup wired to real IAP (coins not golden pieces) | Addressed | `IAPPurchasePresenter` rewritten; `goldenPiecesGranted` parameter removed entirely |
| R172 — Test payments in sandbox | Unverified | Human UAT checklist present but not reported as completed. Expected — cannot be automated. |

---

## Verdict Rationale

The core technical work for M019 is complete and well-executed: the IAP type system, Unity Purchasing integration, PlayFab receipt validation pipeline, presenter rewiring, and UIFactory/GameBootstrapper wiring are all correct and present on disk. The EditMode test suite covers all four mock outcomes. The ConfirmPendingPurchase timing is correct. IAPProductCatalog is the single source of truth.

However, **one material gap blocks the milestone from being sealed**:

**The runtime Editor does not use MockIAPService.** `GameBootstrapper` constructs `UnityIAPService` unconditionally, even in the Editor. The consequence is:
1. The success criterion "In the Editor, MockIAPService cycles through success / payment failed / PlayFab validation failed / user cancelled" is not met — two of the four outcomes (`PaymentFailed`, `ValidationFailed`) are unreachable at runtime in the Editor regardless of `IAPMockConfig.asset` settings.
2. R166 ("In the Editor, IIAPService resolves to a MockIAPService whose outcome is configurable via a ScriptableObject") is not satisfied at runtime.
3. The S04 summary's key decision "Editor uses #if UNITY_EDITOR guard to route to MockIAPService" contradicts the actual implementation.

The fix is a small, well-defined code change: under `#if UNITY_EDITOR` in `GameBootstrapper`, load `IAPMockConfig` from Resources and construct `MockIAPService` instead of `UnityIAPService`. This was the original design (and was transiently implemented before being reverted), and `MockIAPService` does not have the UGS/PlayFab incompatibility that `UnityIAPService+FakeStore` had — it bypasses Unity Purchasing entirely.

Device sandbox testing (R172) is a human UAT item that cannot be automated; it is expected to remain unconfirmed at this stage and does not block the milestone seal.

---

## Remediation Plan

### SR01: Restore MockIAPService in Editor runtime

**Problem:** `GameBootstrapper` uses `UnityIAPService` in the Unity Editor, making `PaymentFailed` and `ValidationFailed` outcomes unreachable at runtime. The `IAPMockConfig.asset` Inspector control has no effect in Play Mode.

**Fix:** In `GameBootstrapper.Start()`, replace the unconditional `UnityIAPService` construction with a `#if UNITY_EDITOR` branch that loads `IAPMockConfig` from Resources and constructs `MockIAPService` instead. `UnityIAPService` is used on device (non-Editor builds) only. Update the log message to correctly reflect which service is active.

**Files to change:**
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — add `#if UNITY_EDITOR` / `#else` / `#endif` around IAP service construction
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — update log message from `"UnityIAPService (FakeStore/StandardUser in Editor, real store on device)"` to `"MockIAPService (Editor)" / "UnityIAPService (device)"`
- S04-SUMMARY.md — key decision is already correctly stated; no correction needed (code will now match)

**Acceptance criteria:**
- In Editor Play Mode: `IAPMockConfig.MockOutcome = PaymentFailed` → tap a pack → status shows "Purchase failed. Please try again." No coins granted.
- In Editor Play Mode: `IAPMockConfig.MockOutcome = ValidationFailed` → tap a pack → status shows "Purchase could not be verified." No coins granted.
- In Editor Play Mode: `IAPMockConfig.MockOutcome = Success` → tap a pack → coin balance increases by `CoinsGranted`.
- In Editor Play Mode: `IAPMockConfig.MockOutcome = Cancelled` → tap a pack → status shows "Purchase cancelled."
- Console log shows `"[GameBootstrapper] IAP: using MockIAPService (Editor)"` in Play Mode.
- All 340+ EditMode tests continue to pass.
- No `UnityIAPService` or FakeStore interaction in the Editor.
