# M019: Real IAP — Unity Purchasing + PlayFab Receipt Validation

**Gathered:** 2026-03-20
**Status:** Ready for planning

## Project Description

A Unity mobile jigsaw puzzle game. The shop sells coins — the currency used for Continue and other in-game purchases. Both the Shop panel (main menu, three coin packs) and the IAPPurchase popup (single-item purchase flow) are currently stubs that grant coins without any real transaction. This milestone replaces both stubs with real Unity Purchasing + PlayFab server-side receipt validation.

## Why This Milestone

The game is approaching store submission readiness (M015 distribution pipeline complete). Real IAP is required before launch. Both store platforms (iOS App Store, Google Play) must be wired. The PlayFab SDK is already present and authenticated; ValidateIOSReceipt and ValidateGooglePlayPurchase are already in the client API surface.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Tap a coin pack in the Shop → real store purchase sheet appears → complete a test payment → coins appear in balance
- Tap purchase in the IAPPurchase popup → same real store flow → coins granted
- In the Editor: trigger any outcome (success, payment failed, PlayFab validation failed, user cancelled) without a device or sandbox account, via a ScriptableObject config

### Entry point / environment

- Entry point: Shop panel in main menu (ShopPresenter) and IAPPurchase popup (IAPPurchasePresenter)
- Environment: iOS device (App Store sandbox), Android device (Google Play test account), Unity Editor (mock)
- Live dependencies involved: Unity Purchasing SDK, PlayFab ValidateIOSReceipt / ValidateGooglePlayPurchase

## Completion Class

- Contract complete means: mock service exercises all code paths in EditMode tests; product catalog ScriptableObject exists with correct IDs
- Integration complete means: Unity Purchasing initialises and loads products from the store; receipt sent to PlayFab and validated; coins granted
- Operational complete means: purchase survives app backgrounding (ConfirmPendingPurchase called only after PlayFab validation)

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- On a real iOS device with sandbox account: tap pack → store sheet → test payment → PlayFab validates → coins appear
- On a real Android device with test account: same flow via Google Play
- In the Editor: mock service set to "success" grants coins; mock set to "cancelled" shows cancellation message; mock set to "payment failed" shows error; mock set to "validation failed" shows error

## Risks and Unknowns

- Unity Purchasing asmdef reference — `com.unity.purchasing` is not yet in the manifest; adding it may require asmdef updates and a domain reload
- PlayFab catalog configuration — product IDs must match exactly between Unity Purchasing, PlayFab dashboard catalog, and the ScriptableObject. Mismatch causes silent validation failure.
- ConfirmPendingPurchase timing — must be called after PlayFab validation, not immediately after the store transaction. If called too early and PlayFab later rejects, the purchase is lost.
- Google Play requires both ReceiptJson + Signature; iOS requires JwsReceiptData (StoreKit 2) or ReceiptData (legacy). Unity Purchasing surfaces both per platform.

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Popup/ShopPresenter.cs` — stub presenter, three hardcoded coin packs, calls ICoinsService.Earn directly
- `Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs` — stub presenter, currently grants golden pieces (wrong — must grant coins)
- `Assets/Scripts/Game/Popup/IIAPPurchaseView.cs` — view interface for IAPPurchase popup
- `Assets/Scripts/Game/Services/PlayFabCloudSaveService.cs` — reference for UniTaskCompletionSource TCS bridge pattern used for all PlayFab calls
- `Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs` — reference for PlayFabClientAPI callback pattern
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — where IIAPService will be constructed and injected
- `Assets/Scripts/Game/Boot/UIFactory.cs` — where IIAPService is passed through to presenters
- `Assets/PlayFabSDK/Client/PlayFabClientAPI.cs` — ValidateIOSReceipt (line 2376), ValidateGooglePlayPurchase (line 2361)
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — must add UnityEngine.Purchasing reference after package install
- `Packages/manifest.json` — must add com.unity.purchasing

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R165 — IIAPService abstraction
- R166 — Editor mock IAP with selectable outcomes
- R167 — Unity Purchasing SDK integration
- R168 — PlayFab receipt validation (iOS + Android)
- R169 — Coin packs as single source of truth ScriptableObject
- R170 — Shop panel wired to real IAP
- R171 — IAPPurchase popup wired to real IAP (coins, not golden pieces)
- R172 — Test payments in sandbox

## Scope

### In Scope

- `com.unity.purchasing` added to manifest and asmdef
- `IIAPService` interface + `MockIAPService` (Editor/tests) + `UnityIAPService` (real)
- `IAPProductCatalog` ScriptableObject — single source of truth for product IDs and coin amounts
- PlayFab receipt validation after every successful transaction (iOS + Android)
- `ShopPresenter` rewritten to use `IIAPService`
- `IAPPurchasePresenter` rewritten to use `IIAPService`, grants coins (not golden pieces)
- `UIFactory` updated to pass `IIAPService` through to presenters
- `GameBootstrapper` constructs `UnityIAPService` on device, `MockIAPService` in Editor
- EditMode tests covering all mock outcomes

### Out of Scope / Non-Goals

- Restore purchases UI (the Unity Purchasing restore flow exists but no UI trigger is being added in this milestone)
- Subscription products
- Consumable vs non-consumable distinction beyond what coins already are
- Changing the golden piece economy
- PlayFab Economy v2 / Catalog v2 (use classic Client API validate endpoints)

## Technical Constraints

- TCS bridge pattern for all PlayFab calls (D084 — established pattern)
- IIAPService must follow IAdService / IAnalyticsService interface pattern
- `ConfirmPendingPurchase` called only after PlayFab validation succeeds
- Mock outcome driven by ScriptableObject so it survives domain reloads in the Editor
- asmdef `UnityEngine.Purchasing` reference added only after package is in manifest (compile guard or version define)

## Integration Points

- Unity Purchasing SDK (`com.unity.purchasing`) — store transaction layer
- PlayFab Client API — `ValidateIOSReceipt`, `ValidateGooglePlayPurchase` for server-side validation
- `ICoinsService` — coins granted after validation, persisted via Save()
- `IPlayFabAuthService` — must be logged in before validation calls
- `GameBootstrapper` — constructs and injects IIAPService
- `UIFactory` — routes IIAPService to presenters

## Open Questions

- Whether to add a "Restore Purchases" button to the Shop UI — deferred out of scope for this milestone, but the hook in UnityIAPService should make it easy to add later.
