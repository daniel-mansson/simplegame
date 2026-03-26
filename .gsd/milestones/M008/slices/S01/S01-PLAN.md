# S01: Animation Plumbing

**Goal:** Wire the full animation + blocker stack so PopupManager orchestrates LitMotion-driven popup tweens and blocker fade with correct input timing.

**Demo:** Opening a popup logs "AnimateIn" and shows the popup; closing logs "AnimateOut" and immediately restores input before the animation completes.

## Must-Haves

- `IPopupView` has `AnimateInAsync(CancellationToken): UniTask` and `AnimateOutAsync(CancellationToken): UniTask`
- `PopupViewBase : MonoBehaviour` provides default LitMotion implementations: bounce-up in (anchoredPosition Y offset + OutBounce), scale+fade out (localScale to 0.85 + alpha to 0, InBack)
- `IInputBlocker` has `FadeInAsync(CancellationToken): UniTask` and `FadeOutAsync(CancellationToken): UniTask`
- `UnityInputBlocker` implements fade methods: CanvasGroup alpha 0→0.5 (in), 0.5→0 (out), LitMotion
- `PopupManager.ShowPopupAsync`: calls `Block()` then fires `FadeInAsync` + `ShowPopupAsync` concurrently
- `PopupManager.DismissPopupAsync`: calls `Unblock()` + fires `FadeOutAsync` (not awaited), then awaits `HidePopupAsync`
- `UnityViewContainer.ShowPopupAsync`: calls `SetActive(true)` then `AnimateInAsync` on the view
- `UnityViewContainer.HidePopupAsync`: calls `AnimateOutAsync` then `SetActive(false)`
- All 3 mock `IInputBlocker` implementations updated for new interface members
- EditMode tests still green (169 pass)

## Tasks

- [x] **T01: IPopupView + PopupViewBase + IInputBlocker contracts**
  Add `AnimateInAsync`/`AnimateOutAsync` to `IPopupView`. Create `PopupViewBase` with LitMotion default implementations. Add `FadeInAsync`/`FadeOutAsync` to `IInputBlocker`. Update all 3 mocks. Existing views still compile (inherit MonoBehaviour for now — base class migration in T03).

- [x] **T02: UnityInputBlocker fade + PopupManager orchestration**
  Implement `FadeInAsync`/`FadeOutAsync` on `UnityInputBlocker` using LitMotion. Update `PopupManager.ShowPopupAsync`, `DismissPopupAsync`, and `DismissAllAsync` for the new async blocker + timing split.

- [x] **T03: UnityViewContainer animation wiring**
  Update `ShowPopupAsync`/`HidePopupAsync` in `UnityViewContainer` to call `AnimateInAsync`/`AnimateOutAsync`. Migrate all 6 existing `*View.cs` files to inherit `PopupViewBase` instead of `MonoBehaviour`.

## Files Likely Touched

- `Assets/Scripts/Core/MVP/IPopupView.cs`
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs`
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs`
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs`
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs`
- `Assets/Scripts/Game/Popup/ConfirmDialogView.cs`
- `Assets/Scripts/Game/Popup/LevelCompleteView.cs`
- `Assets/Scripts/Game/Popup/LevelFailedView.cs`
- `Assets/Scripts/Game/Popup/RewardedAdView.cs`
- `Assets/Scripts/Game/Popup/IAPPurchaseView.cs`
- `Assets/Scripts/Game/Popup/ObjectRestoredView.cs`
- `Assets/Tests/EditMode/Core/PopupManagerTests.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
- New: `Assets/Scripts/Core/MVP/PopupViewBase.cs`
