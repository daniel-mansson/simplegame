---
id: S03
parent: M020
milestone: M020
provides:
  - Meta/ gains ObjectRestored popup trio (IObjectRestoredView, ObjectRestoredPresenter, ObjectRestoredView)
  - Shop/ directory with 3 files (IShopView, ShopPresenter, ShopView)
  - LevelFlow/ directory with 7 files (ILevelCompleteView, LevelCompletePresenter, LevelCompleteView, ILevelFailedView, LevelFailedPresenter, LevelFailedView, LevelFailedChoice)
  - ConfirmDialog/ directory with 3 files (IConfirmDialogView, ConfirmDialogPresenter, ConfirmDialogView)
  - Popup/ confirmed containing only UnityViewContainer.cs
  - 347 EditMode tests confirmed passing
requires:
  - slice: S02
    provides: Economy/, Save/, Progression/, PlayFab/ feature folders; Services/ removed
affects:
  - S04
key_files:
  - Assets/Scripts/Game/Meta/IObjectRestoredView.cs
  - Assets/Scripts/Game/Meta/ObjectRestoredPresenter.cs
  - Assets/Scripts/Game/Meta/ObjectRestoredView.cs
  - Assets/Scripts/Game/Shop/IShopView.cs
  - Assets/Scripts/Game/Shop/ShopPresenter.cs
  - Assets/Scripts/Game/Shop/ShopView.cs
  - Assets/Scripts/Game/LevelFlow/ILevelCompleteView.cs
  - Assets/Scripts/Game/LevelFlow/LevelCompletePresenter.cs
  - Assets/Scripts/Game/LevelFlow/LevelCompleteView.cs
  - Assets/Scripts/Game/LevelFlow/ILevelFailedView.cs
  - Assets/Scripts/Game/LevelFlow/LevelFailedPresenter.cs
  - Assets/Scripts/Game/LevelFlow/LevelFailedView.cs
  - Assets/Scripts/Game/LevelFlow/LevelFailedChoice.cs
  - Assets/Scripts/Game/ConfirmDialog/IConfirmDialogView.cs
  - Assets/Scripts/Game/ConfirmDialog/ConfirmDialogPresenter.cs
  - Assets/Scripts/Game/ConfirmDialog/ConfirmDialogView.cs
key_decisions:
  - none
patterns_established:
  - none (verification-only pass)
observability_surfaces:
  - "find Assets/Scripts/Game/Popup -name '*.cs' | sort — authoritative check that only UnityViewContainer.cs remains"
  - "find Assets/Scripts/Game/{Meta,Shop,LevelFlow,ConfirmDialog} -name '*.cs' | sort — confirms all feature folders populated"
  - "Unity MCP run_tests EditMode — 347/347 pass confirms no compile or reference breakage"
drill_down_paths:
  - .gsd/milestones/M020/slices/S03/tasks/T01-SUMMARY.md
  - .gsd/milestones/M020/slices/S03/tasks/T02-SUMMARY.md
duration: ~10 minutes (2 tasks, both verification-only)
verification_result: passed
completed_at: 2026-03-26
---

# S03: Move Remaining Popup Feature Files into Feature Folders

**All S03 popup feature file moves verified in place; Popup/ contains only UnityViewContainer.cs; 347/347 EditMode tests pass.**

## What Happened

S03 was designed as a two-task slice to move the remaining Popup/ files into Shop/, LevelFlow/, ConfirmDialog/, and Meta/. By the time execution started, all moves had already been committed to the branch in a prior auto-mode run (during S02 execution). Both tasks became pure verification passes.

T01 confirmed the ObjectRestored trio (IObjectRestoredView, ObjectRestoredPresenter, ObjectRestoredView) in Meta/ and Shop/ (IShopView, ShopPresenter, ShopView) with 3 files. T02 confirmed LevelFlow/ with 7 files and ConfirmDialog/ with 3 files, verified Popup/ had exactly one .cs file (UnityViewContainer.cs), and ran the full EditMode suite via the K006 stdin workaround — 347 passed, 0 failed.

The moved files retain their `namespace SimpleGame.Game.Popup` declarations. This is correct: C# namespaces are path-independent and Unity compiles by assembly membership, not directory structure. The 347 passing tests confirm no reference or compile breakage from the directory reorganisation.

## Verification

| Check | Result |
|---|---|
| `find Assets/Scripts/Game/Popup -name "*.cs"` | Only `UnityViewContainer.cs` ✅ |
| `find Assets/Scripts/Game/Meta -name "*.cs"` | ObjectRestored trio present (+ existing Meta files) ✅ |
| `find Assets/Scripts/Game/Shop -name "*.cs"` | 3 files ✅ |
| `find Assets/Scripts/Game/LevelFlow -name "*.cs"` | 7 files ✅ |
| `find Assets/Scripts/Game/ConfirmDialog -name "*.cs"` | 3 files ✅ |
| EditMode tests | 347/347 pass ✅ |

## Requirements Advanced

- none (structural refactor, no capability change)

## Requirements Validated

- none

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

All file moves were committed in a prior auto-mode run before S03 tasks executed. Both T01 and T02 ran as verification-only tasks rather than executing `git mv` commands. This is the same pattern observed in S01 and S02 — ahead-of-schedule commits from earlier runs. No functional difference; all files are at correct destinations.

## Known Limitations

- Files in LevelFlow/, ConfirmDialog/, Shop/ still declare `namespace SimpleGame.Game.Popup`. This is intentional and correct — namespaces are independent of directory structure in this project. Changing them would require test and reference updates and is out of scope for this structural refactor.

## Follow-ups

- S04 (final verification) should confirm zero orphaned `.meta` files, check Services/ absence, and run a full compile+test pass as the milestone close gate.

## Files Created/Modified

- `Assets/Scripts/Game/Meta/IObjectRestoredView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/Meta/ObjectRestoredPresenter.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/Meta/ObjectRestoredView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/Shop/IShopView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/Shop/ShopPresenter.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/Shop/ShopView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/ILevelCompleteView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/LevelCompletePresenter.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/LevelCompleteView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/ILevelFailedView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/LevelFailedPresenter.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/LevelFailedView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/LevelFlow/LevelFailedChoice.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/ConfirmDialog/IConfirmDialogView.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/ConfirmDialog/ConfirmDialogPresenter.cs` — moved from Popup/ (prior run)
- `Assets/Scripts/Game/ConfirmDialog/ConfirmDialogView.cs` — moved from Popup/ (prior run)

## Forward Intelligence

### What the next slice should know
- S03 is complete. The feature folder layout is fully established. S04's job is final audit: confirm Services/ is gone, confirm no orphaned `.meta` files, confirm 347 tests still pass after any last-minute changes.
- The `Popup/` directory still exists and contains `UnityViewContainer.cs` — this is intentional and correct. It should remain; only the feature-specific popup files were moved out.
- `PlayFab/` contains 17 files in the current tree (16 from S02 + IPlayFabCatalogService.cs swept in as an extra). S04 should treat this as correct.

### What's fragile
- Namespace mismatch: files in Shop/, LevelFlow/, ConfirmDialog/ retain `namespace SimpleGame.Game.Popup`. This will not cause a runtime or compile issue, but any future refactor that tries to match directory structure to namespace will need to touch these 13 files. Flag it as known technical debt, not a bug.
- If any `.meta` file for the now-empty `Popup/` subdirectory structure was left behind, S04 will find it. The `find Assets/Scripts/Game -name "*.meta" -path "*/Popup/*"` command is the right check.

### Authoritative diagnostics
- `find Assets/Scripts/Game -name "*.cs" | grep -v "\.meta" | sort` — complete picture of the current script layout; trustworthy because it reads directly from filesystem, not Git index
- Unity MCP `run_tests EditMode` via K006 stdin workaround — 347/347 is the definitive compile+reference health signal; if this passes, all assembly references are intact
- `git status --short` — if clean after all moves, no orphan cleanup is needed in this slice; S04 should re-verify

### What assumptions changed
- Original plan assumed S03 would execute `git mv` commands. In practice, the moves had already been committed in an earlier auto-mode run during S02 execution, making S03 a pure verification pass. This was already noted in the roadmap annotation for S03.
