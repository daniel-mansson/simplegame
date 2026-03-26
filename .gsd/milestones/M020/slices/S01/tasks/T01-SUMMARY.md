---
id: T01
parent: S01
milestone: M020
provides:
  - Assets/Scripts/Game/IAP/ folder with all 15 IAP-related source files
key_files:
  - Assets/Scripts/Game/IAP/IIAPService.cs
  - Assets/Scripts/Game/IAP/IAPOutcome.cs
  - Assets/Scripts/Game/IAP/IAPResult.cs
  - Assets/Scripts/Game/IAP/IAPProductDefinition.cs
  - Assets/Scripts/Game/IAP/IAPProductInfo.cs
  - Assets/Scripts/Game/IAP/IAPProductCatalog.cs
  - Assets/Scripts/Game/IAP/IAPMockConfig.cs
  - Assets/Scripts/Game/IAP/MockIAPService.cs
  - Assets/Scripts/Game/IAP/UnityIAPService.cs
  - Assets/Scripts/Game/IAP/NullIAPService.cs
  - Assets/Scripts/Game/IAP/PlayFabCatalogService.cs
  - Assets/Scripts/Game/IAP/NullPlayFabCatalogService.cs
  - Assets/Scripts/Game/IAP/IIAPPurchaseView.cs
  - Assets/Scripts/Game/IAP/IAPPurchasePresenter.cs
  - Assets/Scripts/Game/IAP/IAPPurchaseView.cs
key_decisions:
  - none
patterns_established:
  - Feature folder created via git mv preserving .meta GUIDs; namespaces left unchanged
observability_surfaces:
  - "find Assets/Scripts/Game/IAP -name '*.cs' | wc -l → 15"
  - "git status --short | grep '^R' confirms 30 renames (15 .cs + 15 .meta)"
duration: <5 min
verification_result: passed
completed_at: 2026-03-26
blocker_discovered: false
---

# T01: Create IAP/ Folder and Move 15 IAP Files

**Moved 15 IAP source files from Services/ and Popup/ into new Assets/Scripts/Game/IAP/ using git mv, preserving all .meta GUIDs; namespaces unchanged.**

## What Happened

All 15 IAP files were moved via `git mv` in a prior session (committed as `28cb704`). The `IAP/` folder was created implicitly by the first `git mv`. Files from `Services/` (12 files: IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo, IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService, NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService) and from `Popup/` (3 files: IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView) were all relocated. Namespaces (`SimpleGame.Game.Services`, `SimpleGame.Game.Popup`) were intentionally left unchanged per the plan — no source edits required.

## Verification

- `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` → **15** ✅
- `ls Assets/Scripts/Game/Services/*.cs | grep -iE "IAP|Purchase"` → **none** ✅
- `ls Assets/Scripts/Game/Popup/*.cs | grep -iE "IAP|Purchase"` → **none** ✅

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `find Assets/Scripts/Game/IAP -name "*.cs" \| wc -l` → 15 | 0 | ✅ pass | <1s |
| 2 | `ls Assets/Scripts/Game/Services/*.cs \| grep -iE "IAP\|Purchase"` → none | 1 (no match) | ✅ pass | <1s |
| 3 | `ls Assets/Scripts/Game/Popup/*.cs \| grep -iE "IAP\|Purchase"` → none | 1 (no match) | ✅ pass | <1s |

## Diagnostics

- **Confirm file count:** `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` → expect 15
- **Confirm sources clean:** `rg "IAPService|IAPOutcome|IAPResult|IAPProduct|IAPMock|IAPPurchase" Assets/Scripts/Game/Services/ Assets/Scripts/Game/Popup/` — only reference remaining is `PopupId.IAPPurchase` inside `UnityViewContainer.cs` which is an enum value reference, not a moved file
- **Git state:** All 30 renames (15 `.cs` + 15 `.meta`) are committed in `28cb704`

## Deviations

none

## Known Issues

none

## Files Created/Modified

- `Assets/Scripts/Game/IAP/` — new feature folder with 15 IAP source files (moved from Services/ and Popup/)
- `.gsd/milestones/M020/slices/S01/tasks/T01-PLAN.md` — added `## Observability Impact` section (pre-flight requirement)
- `.gsd/milestones/M020/slices/S01/S01-PLAN.md` — added `## Observability / Diagnostics` section and diagnostic verification step (pre-flight requirement); marked T01 [x]
