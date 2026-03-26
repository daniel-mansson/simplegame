---
id: T01
parent: S01
milestone: M020
provides:
  - Assets/Scripts/Game/IAP/ folder with 15 IAP source files (12 from Services/, 3 from Popup/)
key_files:
  - Assets/Scripts/Game/IAP/IIAPService.cs
  - Assets/Scripts/Game/IAP/UnityIAPService.cs
  - Assets/Scripts/Game/IAP/MockIAPService.cs
  - Assets/Scripts/Game/IAP/NullIAPService.cs
  - Assets/Scripts/Game/IAP/IAPPurchasePresenter.cs
  - Assets/Scripts/Game/IAP/IAPPurchaseView.cs
  - Assets/Scripts/Game/IAP/IIAPPurchaseView.cs
  - Assets/Scripts/Game/IAP/IAPProductCatalog.cs
  - Assets/Scripts/Game/IAP/IAPProductDefinition.cs
  - Assets/Scripts/Game/IAP/IAPProductInfo.cs
  - Assets/Scripts/Game/IAP/IAPOutcome.cs
  - Assets/Scripts/Game/IAP/IAPResult.cs
  - Assets/Scripts/Game/IAP/IAPMockConfig.cs
  - Assets/Scripts/Game/IAP/PlayFabCatalogService.cs
  - Assets/Scripts/Game/IAP/NullPlayFabCatalogService.cs
key_decisions:
  - Namespaces left unchanged (SimpleGame.Game.Services / SimpleGame.Game.Popup) — namespace rename deferred to a later milestone to avoid large churn during the move
  - Services/ folder retained (not deleted) — still holds Economy/Save/Progression files moved in S02
patterns_established:
  - git mv used for all moves; .meta files move automatically; GUID integrity confirmed via R100 rename detection in git show
observability_surfaces:
  - find Assets/Scripts/Game/IAP -name "*.cs" | wc -l → 15
  - git show c45a15f --name-status | grep "^R" | grep -i iap → 15 R100 renames
  - rg pattern against Services/ and Popup/ confirms no IAP source files remain
duration: completed in prior session (resuming)
verification_result: passed
completed_at: 2026-03-26
blocker_discovered: false
---

# T01: Create IAP/ Folder and Move 15 IAP Files

**Moved 15 IAP source files from Services/ and Popup/ into new Assets/Scripts/Game/IAP/ using git mv; all tracked as R100 renames.**

## What Happened

Created `Assets/Scripts/Game/IAP/` and moved all IAP-related files into it:
- 12 files from `Assets/Scripts/Game/Services/`: `IIAPService.cs`, `IAPOutcome.cs`, `IAPResult.cs`, `IAPProductDefinition.cs`, `IAPProductInfo.cs`, `IAPProductCatalog.cs`, `IAPMockConfig.cs`, `MockIAPService.cs`, `UnityIAPService.cs`, `NullIAPService.cs`, `PlayFabCatalogService.cs`, `NullPlayFabCatalogService.cs`
- 3 files from `Assets/Scripts/Game/Popup/`: `IIAPPurchaseView.cs`, `IAPPurchasePresenter.cs`, `IAPPurchaseView.cs`

`git mv` was used for all moves, so `.meta` files moved automatically and Unity GUID references are intact. Namespaces were deliberately left unchanged — the folder name is now `IAP/` but the types still declare `SimpleGame.Game.Services` and `SimpleGame.Game.Popup`, consistent with the rest of the S01 plan scope.

These moves were committed as part of commit `c45a15f` (`refactor(S01): move IAP, Ads, and ATT into feature folders`) together with T02 and T03 work.

## Verification

All must-haves confirmed:

1. `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` → **15** ✅
2. `git show c45a15f --name-status | grep "^R" | grep -i iap` → **15 R100 renames** (no add/delete pairs) ✅
3. `rg "IAPService|IAPOutcome|..." Assets/Scripts/Game/Services/` → **clean** ✅
4. `rg "IAPService|IAPOutcome|..." Assets/Scripts/Game/Popup/` → only hit in `UnityViewContainer.cs` referencing `PopupId.IAPPurchase` enum value — not a source file, expected ✅

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `find Assets/Scripts/Game/IAP -name "*.cs" \| wc -l` | 0 | ✅ pass (15) | <1s |
| 2 | `git show c45a15f --name-status \| grep "^R" \| grep -i iap \| wc -l` | 0 | ✅ pass (15 R100 renames) | <1s |
| 3 | `rg IAP-related patterns in Services/` | 1 (no match) | ✅ pass (clean) | <1s |
| 4 | `rg IAP-related patterns in Popup/` | 0 | ✅ pass (only enum ref in UnityViewContainer, not a moved file) | <1s |

## Diagnostics

To inspect the state of the IAP folder at any point:
```
find Assets/Scripts/Game/IAP -name "*.cs" | sort    # list all IAP source files
git log --diff-filter=R --name-status -- Assets/Scripts/Game/IAP/  # confirm all arrived as renames
rg "IAPService|IAPProduct" Assets/Scripts/Game/Services/  # should be empty
```

Unity compile errors after domain reload would indicate a broken GUID (`.meta` out of sync). That can't happen here since `git mv` handles `.meta` atomically.

## Deviations

None. All 15 files moved exactly as planned.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Game/IAP/` — new directory with 15 IAP source files (15 `.cs` + 15 `.meta`)
- `Assets/Scripts/Game/Services/` — 12 IAP files removed (folder retained for S02)
- `Assets/Scripts/Game/Popup/` — 3 IAP popup files removed (folder retained for remaining popups)
