---
estimated_steps: 3
estimated_files: 1
---

# T02: UnityTransitionPlayer runtime MonoBehaviour

**Slice:** S04 — Transition System
**Milestone:** M001

## Description

Creates the concrete Unity implementation of `ITransitionPlayer` — a MonoBehaviour that uses a `CanvasGroup` to perform fade-to-black and fade-from-black transitions via alpha interpolation. Follows the same pattern as `UnityInputBlocker` (MonoBehaviour + `[SerializeField] CanvasGroup`). This component will be placed on a high-sort-order Canvas in the persistent scene by S05.

## Steps

1. Create `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs`:
   - MonoBehaviour implementing `ITransitionPlayer` in namespace `SimpleGame.Runtime.TransitionManagement`
   - `[SerializeField] private CanvasGroup _canvasGroup`
   - `[SerializeField] private float _fadeDuration = 0.3f`
   - `FadeOutAsync`: set `_canvasGroup.alpha = 0f`, activate `_canvasGroup.gameObject`, interpolate alpha 0→1 using `while (elapsed < _fadeDuration)` with `elapsed += Time.deltaTime` and `UniTask.Yield(ct)`, clamp final `alpha = 1f`
   - `FadeInAsync`: interpolate alpha 1→0 using same loop pattern, clamp final `alpha = 0f`, deactivate `_canvasGroup.gameObject`
   - Explicitly set `_canvasGroup.blocksRaycasts = false` in both methods (input blocking is `IInputBlocker`'s responsibility, not the overlay's)
2. Verify file compiles with zero errors via batchmode compile pass.
3. Run full test suite to confirm no regressions (32+ tests pass, 0 failures).

## Must-Haves

- [ ] `UnityTransitionPlayer` implements `ITransitionPlayer`
- [ ] `_canvasGroup` and `_fadeDuration` are instance fields, not static
- [ ] `blocksRaycasts = false` explicitly set in both fade methods
- [ ] Alpha clamped to exact boundary values (0f or 1f) after loop completes
- [ ] GameObject deactivated after fade-in (overlay invisible and non-interacting when not transitioning)
- [ ] `CancellationToken` threaded through `UniTask.Yield` calls

## Verification

- Batchmode compile: `Unity -batchmode -quit -logFile` → exit 0, zero `error CS`
- Full test suite still passes: `Unity -batchmode -runTests` → `failed="0"`
- `grep -r "static " --include="*.cs" Assets/Scripts/Runtime/TransitionManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output
- `grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` → at least 1 match

## Observability Impact

- **Runtime signal:** `UnityTransitionPlayer` is a MonoBehaviour — Unity's Inspector shows `_canvasGroup` and `_fadeDuration` at runtime. A missing `_canvasGroup` reference surfaces immediately as a `NullReferenceException` in `FadeOutAsync`/`FadeInAsync`.
- **Inspection surface:** After a transition completes, `_canvasGroup.gameObject.activeSelf == false` is the inspectable post-fade-in state. If the overlay stays visible after navigation, `activeSelf == true` reveals the deactivation call was skipped.
- **Diagnostic command:** `grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — must return ≥1 match; absence means the overlay will steal input during transitions.
- **Failure state:** If a `CancellationToken` fires mid-fade, `UniTask.Yield(ct)` propagates `OperationCanceledException` — alpha is left at an intermediate value. The overlay `gameObject` remains active; deactivation only happens after the full fade-in loop completes normally.

## Inputs

- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — interface to implement (from T01)
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — pattern to follow (MonoBehaviour + CanvasGroup)
- S04 Research: fade loop shape, duration default, CanvasGroup pitfalls

## Expected Output

- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — MonoBehaviour ready for S05 to place on a Canvas in the persistent scene
