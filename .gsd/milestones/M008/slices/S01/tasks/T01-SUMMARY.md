---
id: T01
parent: S01
milestone: M008
provides:
  - IPopupView with AnimateInAsync/AnimateOutAsync contract
  - IInputBlocker with FadeInAsync/FadeOutAsync contract
  - PopupViewBase abstract MonoBehaviour with LitMotion default animations (bounce-up in, scale+fade out)
  - All 3 IInputBlocker mocks updated (PopupManagerTests, SceneControllerTests, InGameTests)
  - All view mocks in PopupTests.cs and DemoWiringTests.cs updated
requires: []
affects: [T02, T03, S02, S03]
key_files:
  - Assets/Scripts/Core/MVP/IPopupView.cs
  - Assets/Scripts/Core/MVP/PopupViewBase.cs
  - Assets/Scripts/Core/PopupManagement/IInputBlocker.cs
  - Assets/Tests/EditMode/Core/PopupManagerTests.cs
  - Assets/Tests/EditMode/Game/SceneControllerTests.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
  - Assets/Tests/EditMode/Game/PopupTests.cs
  - Assets/Tests/EditMode/Game/DemoWiringTests.cs
key_decisions:
  - "PopupViewBase uses _canvasGroup.alpha via .Bind() not BindToAlpha extension — safer, avoids extension uncertainty"
  - "PopupViewBase._panel is RectTransform for position and scale animation targets"
  - "ValidateRefs() null check in base class — animation skips with warning if not yet wired"
patterns_established:
  - "All IPopupView mocks implement AnimateInAsync/OutAsync returning UniTask.CompletedTask"
drill_down_paths:
  - .gsd/milestones/M008/slices/S01/tasks/T01-PLAN.md
duration: 20min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T01: IPopupView + PopupViewBase + IInputBlocker Contracts

**Animation contracts added to IPopupView and IInputBlocker; PopupViewBase created with LitMotion default bounce-up in / scale+fade out; all mock implementations updated — 169 tests green.**

## What Happened

Added `AnimateInAsync`/`AnimateOutAsync` to `IPopupView` and `FadeInAsync`/`FadeOutAsync` to `IInputBlocker`. Created `PopupViewBase` as an abstract MonoBehaviour in Core.MVP — default AnimateIn slides the `_panel` RectTransform up from -80px with OutBounce ease; default AnimateOut tweens scale to 0.85 and alpha to 0 concurrently with InBack ease. Both tweens fire as fire-and-collect with `UniTask.WhenAll`. Null check guards both methods — skips with warning if `_canvasGroup` or `_panel` not wired yet (they won't be until S03 SceneSetup rewrite).

Found more mocks than the 3 documented in K004 — PopupTests.cs had 5 more (MockLevelCompleteView, MockLevelFailedView, MockRewardedAdView, MockIAPPurchaseView, MockObjectRestoredView) and DemoWiringTests.cs had MockConfirmDialogView. All updated.

## Deviations

DemoWiringTests.cs had a duplicate `using Cysharp.Threading.Tasks;` after adding `System.Threading` — cleaned up.

## Files Created/Modified

- `Assets/Scripts/Core/MVP/IPopupView.cs` — added AnimateInAsync/AnimateOutAsync
- `Assets/Scripts/Core/MVP/PopupViewBase.cs` — new, LitMotion default animations
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — added FadeInAsync/FadeOutAsync
- `Assets/Tests/EditMode/Core/PopupManagerTests.cs` — MockInputBlocker updated
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — MockInputBlockerGame updated
- `Assets/Tests/EditMode/Game/InGameTests.cs` — MockInputBlockerForInGame updated
- `Assets/Tests/EditMode/Game/PopupTests.cs` — 5 view mocks updated + System.Threading using added
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — MockConfirmDialogView updated
