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

- [x] **T01: Create IAP/ folder and move 15 IAP files**
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

## Observability / Diagnostics

This slice is a pure filesystem reorganisation — no runtime behaviour changes. Diagnostic surfaces:

- **Confirm move complete:** `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` (expect 15), same for Ads (7) and ATT (7)
- **Confirm sources are clean:** `ls Assets/Scripts/Game/Services/*.cs Assets/Scripts/Game/Popup/*.cs` — should show only non-IAP/Ads/ATT files after each task
- **Unity compile status:** Check Unity Editor console after reload; any namespace or reference breakage surfaces as `error CS` lines in `Editor.log`. Use the K011 `python3` snippet to read errors after the last `Starting:` line.
- **Test gate:** `run_tests EditMode` — 340 tests must pass after T03 commits; failure here indicates a `.meta` GUID mismatch or stale Bee dag (see K011).
- **Failure state:** If a `git mv` is accidentally omitted, `git status` will show the file as untracked in the old location and missing from the new one. Run `git status --short | grep "^R"` to confirm all expected renames are staged.
- **No secrets involved** — no redaction constraints apply.
