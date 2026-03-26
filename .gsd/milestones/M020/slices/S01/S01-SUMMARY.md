---
id: S01
parent: M020
milestone: M020
provides:
  - Assets/Scripts/Game/IAP/ with 15 IAP source files (services + popup trio)
  - Assets/Scripts/Game/Ads/ with 7 Ads source files (services + popup trio)
  - Assets/Scripts/Game/ATT/ with 7 ATT source files (services + popup trio)
  - All .meta GUIDs preserved via git mv; namespaces unchanged
  - 347 EditMode tests passing
requires: []
affects:
  - S02
key_files:
  - Assets/Scripts/Game/IAP/IIAPService.cs
  - Assets/Scripts/Game/IAP/UnityIAPService.cs
  - Assets/Scripts/Game/IAP/IAPPurchasePresenter.cs
  - Assets/Scripts/Game/IAP/IAPPurchaseView.cs
  - Assets/Scripts/Game/Ads/IAdService.cs
  - Assets/Scripts/Game/Ads/UnityAdService.cs
  - Assets/Scripts/Game/Ads/RewardedAdPresenter.cs
  - Assets/Scripts/Game/ATT/IATTService.cs
  - Assets/Scripts/Game/ATT/UnityATTService.cs
  - Assets/Scripts/Game/ATT/ConsentGatePresenter.cs
key_decisions:
  - D104 — Feature-cohesion folder convention established (feature folders replace layer folders)
  - D105 — Namespaces left unchanged during folder restructure
patterns_established:
  - Feature folder created via git mv preserving .meta GUIDs; namespaces left unchanged
  - Service + popup files for a feature co-located in one folder (e.g. IAdService + RewardedAdPresenter both in Ads/)
observability_surfaces:
  - "find Assets/Scripts/Game/IAP -name '*.cs' | wc -l → 15"
  - "find Assets/Scripts/Game/Ads -name '*.cs' | wc -l → 7"
  - "find Assets/Scripts/Game/ATT -name '*.cs' | wc -l → 7"
  - "ls Assets/Scripts/Game/Services/ → exit 2 (directory does not exist)"
  - "ls Assets/Scripts/Game/Popup/ → UnityViewContainer.cs only"
  - "git show c45a15f --stat | grep ' => IAP\\| => Ads\\| => ATT' → all renames confirmed"
  - "EditMode test run → 347 passed, 0 failed (job c8c04c8b)"
drill_down_paths:
  - .gsd/milestones/M020/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M020/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M020/slices/S01/tasks/T03-SUMMARY.md
duration: ~30min
verification_result: passed
completed_at: 2026-03-26
---

# S01: Move IAP, Ads, and ATT Feature Groups

**28 source files across IAP, Ads, and ATT moved from layer folders into three new feature folders via `git mv`; 347 EditMode tests pass; Services/ directory gone ahead of schedule.**

## What Happened

All three feature groups (IAP, Ads, ATT) were moved in a single foundational commit (`c45a15f`) before the task pipeline began, then the three task agents verified and documented each group. The moves used `git mv` throughout, which preserved every `.meta` GUID — critical because MonoBehaviour subclasses are referenced by GUID in Unity scene files.

**IAP (T01):** 12 service files from `Services/` (IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo, IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService, NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService) plus 3 popup files from `Popup/` (IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView) → `Assets/Scripts/Game/IAP/`. Count: 15.

**Ads (T02):** 4 service files from `Services/` (IAdService, AdResult, UnityAdService, NullAdService) plus 3 popup files from `Popup/` (IRewardedAdView, RewardedAdPresenter, RewardedAdView) → `Assets/Scripts/Game/Ads/`. Count: 7.

**ATT (T03):** 4 service files from `Services/` (IATTService, ATTAuthorizationStatus, UnityATTService, NullATTService) plus 3 popup files from `Popup/` (IConsentGateView, ConsentGatePresenter, ConsentGateView) → `Assets/Scripts/Game/ATT/`. Count: 7.

An important ahead-of-schedule outcome: `Assets/Scripts/Game/Services/` **no longer exists** — it was emptied by these moves plus pre-existing earlier work, and the directory was removed. S02's goal of removing Services/ is already partially satisfied; S02 only needs to create Economy/, Save/, Progression/, PlayFab/ and verify cleanliness.

Similarly, `Popup/` now contains only `UnityViewContainer.cs` — all other popup files have already migrated (either in this slice or prior to M020). S03's Popup cleanup is already done.

Namespaces (`SimpleGame.Game.Services`, `SimpleGame.Game.Popup`) were intentionally left unchanged per D105 — no source edits were made, only file moves.

## Verification

| Check | Result |
|---|---|
| `find Assets/Scripts/Game/IAP -name "*.cs" \| wc -l` | 15 ✅ |
| `find Assets/Scripts/Game/Ads -name "*.cs" \| wc -l` | 7 ✅ |
| `find Assets/Scripts/Game/ATT -name "*.cs" \| wc -l` | 7 ✅ |
| `ls Assets/Scripts/Game/Services/` | exit 2 (directory gone) ✅ |
| `ls Assets/Scripts/Game/Popup/` | UnityViewContainer.cs only ✅ |
| No orphaned `.meta` files in Services/ or Popup/ | confirmed ✅ |
| `git status --short` | only T03-VERIFY.json untracked ✅ |
| EditMode test run (K006 stdin workaround) | 347 passed, 0 failed ✅ |

## Requirements Advanced

- none (structural refactor, no capability change)

## Requirements Validated

- none

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

- **IAP file count:** The roadmap boundary map listed 14 files in IAP/, but the actual count is 15. PlayFabCatalogService and NullPlayFabCatalogService were included (they are IAP-domain services), making the real count 15 `.cs` files — consistent with T01's plan and verified result.
- **All three feature groups committed together:** Tasks T01/T02/T03 documented pre-existing work rather than each executing their own `git mv`. The commit `c45a15f` covered all three groups atomically before the task pipeline started. No functional deviation; all files are correctly placed.
- **Services/ directory already gone:** S01 was expected to leave Services/ intact for S02. Instead, Services/ is already empty and removed. S02 can skip the Services/ removal step and focus only on creating its target folders.
- **Popup/ already clean:** S03 expected Popup/ to contain all non-IAP/Ads/ATT popup files at the start of that slice. In reality, Popup/ already contains only UnityViewContainer.cs. S03's Popup cleanup work is complete.
- **Test count 347 vs 340:** Plan expected 340 EditMode tests; actual count is 347. Seven additional tests exist from work done after the plan was written. All pass.

## Known Limitations

None. This is a pure filesystem reorganisation — no runtime behaviour is affected.

## Follow-ups

- S02 planning should note that Services/ is already gone — no removal step needed. Focus: create Economy/, Save/, Progression/, PlayFab/ folders.
- S03 planning should note that Popup/ is already clean (UnityViewContainer.cs only) — no Popup cleanup needed. Focus: verify Meta/ and any remaining file placements.
- S04 remains valuable as the final verification pass to confirm zero orphaned .meta files and full test suite.

## Files Created/Modified

- `Assets/Scripts/Game/IAP/` — 15 source files moved from Services/ (12) and Popup/ (3)
- `Assets/Scripts/Game/Ads/` — 7 source files moved from Services/ (4) and Popup/ (3)
- `Assets/Scripts/Game/ATT/` — 7 source files moved from Services/ (4) and Popup/ (3)
- `Assets/Scripts/Game/Services/` — removed (emptied and deleted)
- `Assets/Scripts/Game/Popup/` — now contains only UnityViewContainer.cs

## Forward Intelligence

### What the next slice should know

- **Services/ is already gone.** S02's goal of removing Services/ after moving Economy/Save/Progression/PlayFab is already partially satisfied — the directory doesn't exist. S02 just needs to verify that its target folders (Economy/, Save/, Progression/, PlayFab/) contain the right files and that no Services/ remnants remain.
- **Popup/ is already clean.** S03's goal of leaving Popup/ with only UnityViewContainer.cs is already achieved. S03 should verify this is still true but doesn't need to execute any moves for Popup/ itself.
- **Namespace drift is intentional.** Files in `Assets/Scripts/Game/IAP/` still carry `namespace SimpleGame.Game.Services` or `namespace SimpleGame.Game.Popup`. This is correct per D105. Don't "fix" it.
- **Test count is 347, not 340.** Update any expected-count checks accordingly. The plan's 340 figure is stale.

### What's fragile

- **Namespace mismatch between folder and namespace** — All moved files retain their original namespace (Services or Popup). This is intentional (D105) but could confuse IDE navigation. Not a runtime issue.
- **Roadmap file count (14 for IAP)** is wrong — actual is 15. The S04 verification checklist uses this manifest; it should check for 15, not 14.

### Authoritative diagnostics

- `find Assets/Scripts/Game/IAP -name "*.cs" | sort` — ground truth for IAP file inventory; 15 files expected. More reliable than memory or roadmap doc.
- `find Assets/Scripts/Game/Ads -name "*.cs" | sort` — 7 files expected.
- `find Assets/Scripts/Game/ATT -name "*.cs" | sort` — 7 files expected.
- `git show c45a15f --stat` — authoritative record of which renames were tracked by git; confirms GUIDs were preserved (rename = same inode chain, not copy+delete).
- `git status --short | grep "^??"` — checks for untracked .meta files from any empty folder not yet cleaned up.
- EditMode test run via K006 stdin workaround — the only reliable test gate; 347 is the current baseline.

### What assumptions changed

- **Original assumption:** Services/ would remain intact through S01. **What actually happened:** Services/ was emptied by the moves and removed. S02 doesn't need a removal step.
- **Original assumption:** Popup/ would still contain many popup files at S01 completion. **What actually happened:** Popup/ was already reduced to UnityViewContainer.cs before M020 started. S03's Popup cleanup is a no-op.
