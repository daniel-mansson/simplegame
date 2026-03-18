---
id: T03
parent: S01
milestone: M008
provides:
  - UnityViewContainer.ShowPopupAsync calls SetActive(true) then AnimateInAsync
  - UnityViewContainer.HidePopupAsync calls AnimateOutAsync then SetActive(false)
  - All 6 popup views inherit PopupViewBase instead of MonoBehaviour
  - PopupViewBase _canvasGroup/_panel refs null-guarded until wired in S03
requires:
  - slice: S01/T01
    provides: PopupViewBase abstract class, IPopupView contract
affects: [S02, S03]
key_files:
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Scripts/Game/Popup/ConfirmDialogView.cs
  - Assets/Scripts/Game/Popup/LevelCompleteView.cs
  - Assets/Scripts/Game/Popup/LevelFailedView.cs
  - Assets/Scripts/Game/Popup/RewardedAdView.cs
  - Assets/Scripts/Game/Popup/IAPPurchaseView.cs
  - Assets/Scripts/Game/Popup/ObjectRestoredView.cs
key_decisions:
  - "GetComponentInChildren<IPopupView>(true) on popup GO — searches inactive children, no registration needed"
  - "HidePopupAsync: animate out THEN SetActive(false) — ensures animation completes before GameObject deactivates"
patterns_established:
  - "PopupViewBase inheritance pattern for all popup views in Game assembly"
drill_down_paths:
  - .gsd/milestones/M008/slices/S01/tasks/T03-PLAN.md
duration: 15min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T03: UnityViewContainer Animation Wiring + View Migration

**UnityViewContainer now calls AnimateIn/Out on each popup view; all 6 views inherit PopupViewBase; 169 tests green, no compiler errors.**

## What Happened

`UnityViewContainer.ShowPopupAsync` now calls `SetActive(true)` then resolves `IPopupView` via `GetComponentInChildren<IPopupView>(true)` and awaits `AnimateInAsync`. `HidePopupAsync` resolves the view, awaits `AnimateOutAsync`, then calls `SetActive(false)` — ordering ensures the exit animation completes before the object disappears.

All 6 view files updated to `PopupViewBase, I*View`. The `using SimpleGame.Core.MVP` import added to each. The `Awake()` methods in each view remain unchanged — `PopupViewBase` defines no `Awake()`, so no base call needed.

The `_canvasGroup` and `_panel` SerializeField refs on `PopupViewBase` are not wired yet in the scene — that's S03. Until then, `ValidateRefs()` will log a warning and skip animations, but the popups still show/hide correctly.

## Deviations

None from plan.

## Files Created/Modified

- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — animation calls added
- `Assets/Scripts/Game/Popup/ConfirmDialogView.cs` — PopupViewBase
- `Assets/Scripts/Game/Popup/LevelCompleteView.cs` — PopupViewBase
- `Assets/Scripts/Game/Popup/LevelFailedView.cs` — PopupViewBase
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — PopupViewBase
- `Assets/Scripts/Game/Popup/IAPPurchaseView.cs` — PopupViewBase
- `Assets/Scripts/Game/Popup/ObjectRestoredView.cs` — PopupViewBase
