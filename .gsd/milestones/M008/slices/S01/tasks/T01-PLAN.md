# T01: IPopupView + PopupViewBase + IInputBlocker Contracts

**Slice:** S01
**Milestone:** M008

## Goal

Add the animation contracts to `IPopupView` and `IInputBlocker`, create `PopupViewBase` with LitMotion default implementations, and update all 3 mock `IInputBlocker` implementations so tests compile.

## Must-Haves

### Truths
- `IPopupView` declares `UniTask AnimateInAsync(CancellationToken ct = default)` and `UniTask AnimateOutAsync(CancellationToken ct = default)`
- `IInputBlocker` declares `UniTask FadeInAsync(CancellationToken ct = default)` and `UniTask FadeOutAsync(CancellationToken ct = default)`
- `PopupViewBase : MonoBehaviour` implements `IPopupView` with default LitMotion tweens
- All 3 mocks implement the new methods returning `UniTask.CompletedTask`
- Project compiles with no errors

### Artifacts
- `Assets/Scripts/Core/MVP/IPopupView.cs` — adds `AnimateInAsync` + `AnimateOutAsync`
- `Assets/Scripts/Core/MVP/PopupViewBase.cs` — new, abstract MonoBehaviour with LitMotion default impls (CanvasGroup + RectTransform required fields)
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — adds `FadeInAsync` + `FadeOutAsync`
- `Assets/Tests/EditMode/Core/PopupManagerTests.cs` — `MockInputBlocker` updated
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — `MockInputBlockerGame` updated
- `Assets/Tests/EditMode/Game/InGameTests.cs` — `MockInputBlockerForInGame` updated

### Key Links
- `PopupViewBase` implements `IPopupView` — animation methods are the interface contract fulfilled by the base class
- All 6 `*View.cs` will inherit `PopupViewBase` in T03 — this task only creates the base; views still inherit MonoBehaviour for now

## Steps

1. Add `UniTask AnimateInAsync(CancellationToken ct = default)` and `UniTask AnimateOutAsync(CancellationToken ct = default)` to `IPopupView`
2. Add `UniTask FadeInAsync(CancellationToken ct = default)` and `UniTask FadeOutAsync(CancellationToken ct = default)` to `IInputBlocker`
3. Create `PopupViewBase.cs`: abstract `MonoBehaviour` implementing `IPopupView`. Requires `[SerializeField] CanvasGroup _canvasGroup` and `[SerializeField] RectTransform _panel`. Default `AnimateInAsync`: reset alpha=1, position to offset (-80 Y), tween position to 0 with `OutBounce`. Default `AnimateOutAsync`: concurrent tween scale 1→0.85 (InBack) and alpha 1→0 (Linear), await both.
4. Update `MockInputBlocker` in PopupManagerTests.cs to add `FadeInAsync`/`FadeOutAsync` returning `UniTask.CompletedTask`
5. Update `MockInputBlockerGame` in SceneControllerTests.cs the same way
6. Update `MockInputBlockerForInGame` in InGameTests.cs the same way

## Context
- LitMotion and LitMotion.Extensions are already referenced in `SimpleGame.Core.asmdef`
- `PopupViewBase` lives in `Assets/Scripts/Core/MVP/` alongside `IPopupView.cs`
- `SimpleGame.Game.asmdef` does NOT reference LitMotion — animation logic must stay in Core
- Bounce-up in: start at `anchoredPosition + Vector2(0, -80)`, tween to `Vector2.zero`, ease `OutBounce`, duration 0.4s
- Scale-down out: tween `localScale` from `Vector3.one` to `Vector3(0.85, 0.85, 1)` + CanvasGroup alpha from 1→0, ease `InBack`, duration 0.25s. Both tweens run concurrently — await both handles.
- Use `.Bind(x => _canvasGroup.alpha = x)` for alpha (safest — avoids BindToAlpha extension uncertainty)
- Use `.BindToLocalScale(transform)` for scale via LitMotion.Extensions
- Use `.BindToAnchoredPosition(_panel)` for position via LitMotion.Extensions
