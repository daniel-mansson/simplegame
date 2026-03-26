---
id: S03
parent: M020
milestone: M020
provides:
  - Assets/Scripts/Game/Shop/ — 3 files (IShopView, ShopPresenter, ShopView)
  - Assets/Scripts/Game/LevelFlow/ — 7 files (ILevelCompleteView, LevelCompletePresenter, LevelCompleteView, ILevelFailedView, LevelFailedPresenter, LevelFailedView, LevelFailedChoice)
  - Assets/Scripts/Game/ConfirmDialog/ — 3 files (IConfirmDialogView, ConfirmDialogPresenter, ConfirmDialogView)
  - Assets/Scripts/Game/Meta/ gains IObjectRestoredView, ObjectRestoredPresenter, ObjectRestoredView
  - Assets/Scripts/Game/Popup/ contains only UnityViewContainer.cs
requires:
  - slice: S02
    provides: Economy/, Save/, Progression/, PlayFab/ created; Services/ removed
affects: [S04]
key_files:
  - Assets/Scripts/Game/Shop/
  - Assets/Scripts/Game/LevelFlow/
  - Assets/Scripts/Game/ConfirmDialog/
  - Assets/Scripts/Game/Meta/ (gains 3 files)
  - Assets/Scripts/Game/Popup/ (reduced to 1 file)
duration: ~10min
verification_result: pass
completed_at: 2026-03-26
---

# S03: Move Remaining Popup Feature Files into Feature Folders

**Shop/ (3), LevelFlow/ (7), ConfirmDialog/ (3) created; ObjectRestored trio moved to Meta/; Popup/ contains only UnityViewContainer.cs; 347/347 tests pass.**

## What Happened

Moved all remaining feature files from `Popup/` using `git mv` with explicit `.meta` co-movement. `ObjectRestored` popup trio went to `Meta/` (belongs with the meta-world restoration feature). `LevelFailedChoice.cs` (enum) moved with `LevelFlow/` since it's the result type for that popup.

## Deviations

None.

## Files Created/Modified

- `Assets/Scripts/Game/Shop/` — created
- `Assets/Scripts/Game/LevelFlow/` — created
- `Assets/Scripts/Game/ConfirmDialog/` — created
- `Assets/Scripts/Game/Meta/` — gained 3 ObjectRestored files
- `Assets/Scripts/Game/Popup/` — reduced to UnityViewContainer.cs only
