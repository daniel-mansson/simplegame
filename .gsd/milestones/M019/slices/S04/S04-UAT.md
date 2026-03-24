# S02–S04 UAT: Unity Purchasing, PlayFab Validation, Presenter Wiring

**Test these manually when convenient. Device tests require store accounts.**

## 1. Editor — all mock outcomes (no device needed)

Open `Assets/Resources/IAPMockConfig.asset`.

| Step | Action | Expected |
|------|--------|----------|
| 1 | MockOutcome = Success, CoinsGranted = 1200. Play. Open Shop. Tap "1200 Coins" pack. | Coin balance increases by 1200. Status: "Purchase complete!" |
| 2 | MockOutcome = Cancelled. Tap a pack. | No coins granted. Status: "Purchase cancelled." |
| 3 | MockOutcome = PaymentFailed. Tap a pack. | No coins granted. Status: "Purchase failed. Please try again." |
| 4 | MockOutcome = ValidationFailed. Tap a pack. | No coins granted. Status: "Purchase could not be verified." |
| 5 | Open IAPPurchase popup. MockOutcome = Success. Tap Purchase. | Coins increase. No mention of "golden pieces" anywhere in the UI. |
| 6 | Confirm Console shows "[GameBootstrapper] IAP: using MockIAPService (Editor)." | ✓ |

## 2. iOS sandbox device (requires App Store Connect sandbox account)

Prerequisites:
- Build to iOS device
- Sign in with sandbox Apple ID in Settings → App Store (not main Apple ID)
- PlayFab title has correct Bundle ID and App Store Shared Secret configured

| Step | Action | Expected |
|------|--------|----------|
| 1 | Launch app. Open Shop. Tap any pack. | Real App Store purchase sheet appears with sandbox price |
| 2 | Complete test purchase with sandbox credentials. | "Purchase complete!" shown. Coin balance increases. |
| 3 | Check PlayFab dashboard (Players → [player ID] → Inventory/Transaction History) | Validated transaction entry visible |
| 4 | Force-kill app mid-purchase (before sheet completes). Relaunch. | No phantom coins. App recovers cleanly. |

## 3. Android test account (requires Google Play Console test track)

Prerequisites:
- Build to Android device (debug or release)
- Google Play test account configured in Play Console
- PlayFab title has correct Package Name and Google Play Key configured

| Step | Action | Expected |
|------|--------|----------|
| 1 | Open Shop. Tap a pack. | Google Play billing sheet appears |
| 2 | Complete with test account. | "Purchase complete!" shown. Coins granted. |
| 3 | Check PlayFab dashboard. | Validated transaction entry visible |

## 4. Product ID alignment check

Before first real-money test: verify these match in all three places:
- `Assets/Resources/IAPProductCatalog.asset` ProductIds
- App Store Connect / Google Play Console in-app product IDs
- PlayFab title catalog ItemIds

Mismatch → silent validation failure (PlayFab returns error, no coins granted, console log visible).
