---
id: T02
parent: S01
milestone: M020
provides:
  - Assets/Scripts/Game/Ads/ folder with all 7 Ads-related source files
key_files:
  - Assets/Scripts/Game/Ads/IAdService.cs
  - Assets/Scripts/Game/Ads/AdResult.cs
  - Assets/Scripts/Game/Ads/UnityAdService.cs
  - Assets/Scripts/Game/Ads/NullAdService.cs
  - Assets/Scripts/Game/Ads/IRewardedAdView.cs
  - Assets/Scripts/Game/Ads/RewardedAdPresenter.cs
  - Assets/Scripts/Game/Ads/RewardedAdView.cs
key_decisions:
  - none
patterns_established:
  - Ads feature files consolidated into Assets/Scripts/Game/Ads/ via git mv, preserving .meta GUIDs; namespaces unchanged
observability_surfaces:
  - find Assets/Scripts/Game/Ads -name "*.cs" | wc -l → expect 7
  - git show c45a15f --stat | grep Ads → shows all 7 .cs renames
duration: "<1m"
verification_result: passed
completed_at: 2026-03-26
blocker_discovered: false
---

# T02: Create Ads/ Folder and Move 7 Ads Files

**Moved 7 Ads-related source files from Services/ and Popup/ into new Assets/Scripts/Game/Ads/ using git mv, preserving all .meta GUIDs; namespaces unchanged.**

## What Happened

The 7 Ads files were already committed as part of the combined S01 commit `c45a15f` ("refactor(S01): move IAP, Ads, and ATT into feature folders"). That commit covered all three feature groups (IAP, Ads, ATT) together rather than task-by-task. The `Ads/` folder exists with all 7 files; `Services/` no longer exists; `Popup/` contains only `UnityViewContainer.cs`.

Pre-flight observability gaps were addressed: added failure-state detection guidance to `S01-PLAN.md` and an `## Observability Impact` section to `T02-PLAN.md`.

## Verification

- `find Assets/Scripts/Game/Ads -name "*.cs" | wc -l` → **7** ✅
- Files present: `AdResult.cs`, `IAdService.cs`, `IRewardedAdView.cs`, `NullAdService.cs`, `RewardedAdPresenter.cs`, `RewardedAdView.cs`, `UnityAdService.cs` ✅
- `Assets/Scripts/Game/Services/` — no longer exists ✅
- `Assets/Scripts/Game/Popup/` — contains only `UnityViewContainer.cs` ✅
- `git show c45a15f --stat | grep " => Ads"` — shows all 7 Ads renames including `.meta` files ✅

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `find Assets/Scripts/Game/Ads -name "*.cs" \| wc -l` | 0 | ✅ pass (count=7) | <1s |
| 2 | `ls Assets/Scripts/Game/Services/` | 2 (not found) | ✅ pass (folder gone) | <1s |
| 3 | `ls Assets/Scripts/Game/Popup/` | 0 | ✅ pass (only UnityViewContainer.cs) | <1s |
| 4 | `git show c45a15f --stat \| grep Ads` | 0 | ✅ pass (7 .cs + 7 .meta renames) | <1s |

## Diagnostics

- `find Assets/Scripts/Game/Ads -name "*.cs" | sort` — lists all 7 Ads source files
- `git show c45a15f --stat | grep " => Ads"` — confirms git tracked the renames (not copy+delete)
- `git status --short | grep "^R.*Ads"` — shows zero lines (all Ads renames already committed in c45a15f)

## Deviations

All three feature groups (IAP, Ads, ATT) were committed together in a single commit rather than task-by-task. T02 work is included in commit `c45a15f`. No functional deviation from the plan — all 7 files are in the correct location with correct .meta GUIDs.

## Known Issues

none

## Files Created/Modified

- `Assets/Scripts/Game/Ads/IAdService.cs` — moved from Services/; namespace unchanged
- `Assets/Scripts/Game/Ads/AdResult.cs` — moved from Services/; namespace unchanged
- `Assets/Scripts/Game/Ads/UnityAdService.cs` — moved from Services/; namespace unchanged
- `Assets/Scripts/Game/Ads/NullAdService.cs` — moved from Services/; namespace unchanged
- `Assets/Scripts/Game/Ads/IRewardedAdView.cs` — moved from Popup/; namespace unchanged
- `Assets/Scripts/Game/Ads/RewardedAdPresenter.cs` — moved from Popup/; namespace unchanged
- `Assets/Scripts/Game/Ads/RewardedAdView.cs` — moved from Popup/; namespace unchanged
- `.gsd/milestones/M020/slices/S01/S01-PLAN.md` — added failure-state detection to Observability section
- `.gsd/milestones/M020/slices/S01/tasks/T02-PLAN.md` — added Observability Impact section
