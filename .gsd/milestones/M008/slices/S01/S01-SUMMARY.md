---
id: S01
milestone: M008
provides:
  - IPopupView.AnimateInAsync/AnimateOutAsync — interface contract for all popup animations
  - IInputBlocker.FadeInAsync/FadeOutAsync — interface contract for overlay fade
  - PopupViewBase — abstract MonoBehaviour in Core.MVP with LitMotion default animations (bounce-up in: -80px Y + OutBounce 0.4s; scale+fade out: 0.85 scale + alpha 0, InBack 0.25s)
  - UnityInputBlocker — LitMotion alpha 0→0.5 fade in, 0.5→0 fade out; Awake inits alpha=0
  - PopupManager — Block+FadeIn+ShowPopup concurrent on show; Unblock+FadeOut.Forget before HidePopup on dismiss (timing split R080)
  - UnityViewContainer — SetActive then AnimateIn on show; AnimateOut then SetActive on hide
  - All 6 popup views inherit PopupViewBase (ConfirmDialog, LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored)
  - All 9 mock IInputBlocker/IPopupView implementations updated for new interface members
requires: []
affects: [S02, S03, S04]
key_files:
  - Assets/Scripts/Core/MVP/IPopupView.cs
  - Assets/Scripts/Core/MVP/PopupViewBase.cs
  - Assets/Scripts/Core/PopupManagement/IInputBlocker.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Scripts/Game/Popup/ConfirmDialogView.cs
  - Assets/Scripts/Game/Popup/LevelCompleteView.cs
  - Assets/Scripts/Game/Popup/LevelFailedView.cs
key_decisions:
  - "FadeOutAsync is fire-and-forget — input unblocks before animation completes (R080 timing split)"
  - "PopupViewBase uses .Bind(x => cg.alpha = x) not BindToAlpha extension — avoids extension uncertainty"
  - "AnimateOut resets panel to scale=1, alpha=1 after completion — ready for next open"
  - "GetComponentInChildren<IPopupView>(true) used in UnityViewContainer — searches inactive children"
patterns_established:
  - "Popup view inheritance: all popup views extend PopupViewBase, not MonoBehaviour directly"
  - "Mock pattern: AnimateInAsync/OutAsync return UniTask.CompletedTask"
  - "Timing split: Unblock() + FadeOutAsync.Forget() before awaiting HidePopupAsync"
drill_down_paths:
  - .gsd/milestones/M008/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M008/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M008/slices/S01/tasks/T03-SUMMARY.md
duration: 50min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# S01: Animation Plumbing

**Full async animation and blocker overlay stack wired — LitMotion tweens on popup open/close, dim overlay fades in/out, input timing split proven; 169 tests green.**

## What Happened

Three tasks delivered the complete animation infrastructure:

**T01** extended `IPopupView` and `IInputBlocker` with async animation contracts, and created `PopupViewBase` — an abstract MonoBehaviour in `Core.MVP` that provides default LitMotion tweens. The bounce-up entrance slides the panel from -80px with OutBounce ease; the exit scales down to 0.85 and fades alpha to 0 concurrently with InBack ease. Null-guard in both methods skips animation if `_canvasGroup`/`_panel` aren't wired yet. Nine mock implementations across 5 test files were updated.

**T02** implemented the fade on `UnityInputBlocker` (alpha 0→0.5/0.5→0, LitMotion linear) and rewrote `PopupManager` orchestration. The key timing split: on dismiss, `Unblock()` fires immediately, `FadeOutAsync().Forget()` fires but is not awaited, then `HidePopupAsync` is awaited. Input is available before the visual overlay finishes fading.

**T03** wired `UnityViewContainer` to call `AnimateInAsync`/`AnimateOutAsync` via `GetComponentInChildren<IPopupView>(true)`, and migrated all 6 existing popup views to inherit `PopupViewBase`. The `_canvasGroup` and `_panel` SerializeField refs will be wired in S03 SceneSetup rewrite — until then animations skip with a warning.

## Deviations

DemoWiringTests.cs had an additional `MockConfirmDialogView` that K004 didn't document — found via Unity compiler errors, fixed immediately. K004 should be updated.

## Files Created/Modified

- `Assets/Scripts/Core/MVP/IPopupView.cs` — animation contract
- `Assets/Scripts/Core/MVP/PopupViewBase.cs` — new, LitMotion defaults
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — fade contract
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — orchestration rewrite
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — LitMotion fade impl
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — animation calls
- All 6 `*View.cs` files — PopupViewBase inheritance
- 5 test files — mock updates
