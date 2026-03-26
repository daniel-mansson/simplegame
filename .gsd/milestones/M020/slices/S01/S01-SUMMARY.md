---
id: S01
parent: M020
milestone: M020
provides:
  - Assets/Scripts/Game/IAP/ — 15 files (IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo, IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService, NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService, IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView)
  - Assets/Scripts/Game/Ads/ — 7 files (IAdService, AdResult, UnityAdService, NullAdService, IRewardedAdView, RewardedAdPresenter, RewardedAdView)
  - Assets/Scripts/Game/ATT/ — 7 files (IATTService, ATTAuthorizationStatus, UnityATTService, NullATTService, IConsentGateView, ConsentGatePresenter, ConsentGateView)
requires: []
affects: [S02, S03]
key_files:
  - Assets/Scripts/Game/IAP/
  - Assets/Scripts/Game/Ads/
  - Assets/Scripts/Game/ATT/
key_decisions:
  - "git mv used for all .cs moves; .meta siblings moved separately (filesystem) and staged via git add -A"
duration: ~15min
verification_result: pass
completed_at: 2026-03-26
---

# S01: Move IAP, Ads, and ATT Feature Groups

**IAP/ (15 files), Ads/ (7 files), ATT/ (7 files) created from Services/ and Popup/ moves; 347/347 tests pass.**

## What Happened

Moved 29 files total: 12 IAP service files + 3 IAP popup files → `IAP/`; 4 ad service files + 3 rewarded ad popup files → `Ads/`; 4 ATT service files + 3 consent gate popup files → `ATT/`. All moves via `git mv` for `.cs` files; `.meta` siblings moved manually and staged with `git add -A`.

## Deviations

None.

## Files Created/Modified

- `Assets/Scripts/Game/IAP/` — created (15 files)
- `Assets/Scripts/Game/Ads/` — created (7 files)
- `Assets/Scripts/Game/ATT/` — created (7 files)
