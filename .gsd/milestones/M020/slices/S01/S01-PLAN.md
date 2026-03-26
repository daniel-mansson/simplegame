# S01: Move IAP, Ads, and ATT Feature Groups

**Goal:** Create `IAP/`, `Ads/`, and `ATT/` feature folders and move all related files from `Services/` and `Popup/` into them using `git mv`.

**Demo:** `Assets/Scripts/Game/IAP/`, `Ads/`, `ATT/` exist with the correct files; those files are no longer in `Services/` or `Popup/`; Unity compiles; tests pass.

## Must-Haves

- `Assets/Scripts/Game/IAP/` contains exactly 15 files (14 `.cs` + their `.meta` files already tracked)
- `Assets/Scripts/Game/Ads/` contains 7 `.cs` files
- `Assets/Scripts/Game/ATT/` contains 7 `.cs` files  
- None of those 28 files remain in `Services/` or `Popup/`
- `git mv` used for every move (no raw filesystem copy)
- 340 EditMode tests pass after commit

## Tasks

- [ ] **T01: Create IAP/ folder and move 15 IAP files**
  Move all IAP-related files from `Services/` (IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo, IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService, NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService) and from `Popup/` (IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView) into new `Assets/Scripts/Game/IAP/` folder.

- [ ] **T02: Create Ads/ folder and move 7 Ads files**
  Move from `Services/` (IAdService, AdResult, UnityAdService, NullAdService) and `Popup/` (IRewardedAdView, RewardedAdPresenter, RewardedAdView) into `Assets/Scripts/Game/Ads/`.

- [ ] **T03: Create ATT/ folder and move 7 ATT files**
  Move from `Services/` (IATTService, ATTAuthorizationStatus, UnityATTService, NullATTService) and `Popup/` (IConsentGateView, ConsentGatePresenter, ConsentGateView) into `Assets/Scripts/Game/ATT/`. Run tests; commit.

## Files Likely Touched

- `Assets/Scripts/Game/Services/` — source (files removed, but folder remains for S02)
- `Assets/Scripts/Game/Popup/` — source (files removed, remaining popups stay for S03)
- `Assets/Scripts/Game/IAP/` — created
- `Assets/Scripts/Game/Ads/` — created
- `Assets/Scripts/Game/ATT/` — created
