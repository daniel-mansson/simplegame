---
id: T02
parent: S04
milestone: M001
provides:
  - UnityTransitionPlayer MonoBehaviour (Runtime/TransitionManagement) implementing ITransitionPlayer with CanvasGroup alpha interpolation
key_files:
  - Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs
key_decisions:
  - blocksRaycasts = false explicitly set at every write point in both methods (entry + per-loop-iteration + post-clamp) to guarantee no intermediate frame steals input
  - gameObject.SetActive(false) on FadeInAsync completion so the overlay is fully non-interacting between transitions
  - _fadeDuration field defaults to 0.3f — kept as SerializeField so S05 can tune in Inspector without code changes
patterns_established:
  - MonoBehaviour + [SerializeField] CanvasGroup + [SerializeField] float _fadeDuration — same structural pattern as UnityInputBlocker
  - Fade loop: set initial alpha → activate → while(elapsed < duration) { elapsed += Time.deltaTime; alpha = Clamp01(...); blocksRaycasts = false; await UniTask.Yield(ct); } → clamp final → deactivate (FadeIn only)
observability_surfaces:
  - Runtime Inspector — _canvasGroup and _fadeDuration visible at runtime; missing wire-up shows as NullReferenceException on first fade call
  - Post-FadeIn state — _canvasGroup.gameObject.activeSelf == false is the inspectable invariant; activeSelf == true after navigation means deactivation was skipped
  - Diagnostic command — grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs → ≥1 match confirms overlay never steals input
duration: ~15m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T02: UnityTransitionPlayer runtime MonoBehaviour

**`UnityTransitionPlayer` MonoBehaviour created with CanvasGroup alpha interpolation — compiles clean, 32/32 tests pass, blocksRaycasts=false enforced at every alpha write.**

## What Happened

1. **Pre-flight fixes**: Added `## Observability Impact` section to T02-PLAN.md (runtime inspection surfaces, failure state, diagnostic command). Added a failure-path diagnostic check (`grep -n "NullReferenceException" Editor.log`) to S04-PLAN.md Verification section.

2. **Created `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs`**:
   - `MonoBehaviour` implementing `ITransitionPlayer` in namespace `SimpleGame.Runtime.TransitionManagement`
   - `[SerializeField] private CanvasGroup _canvasGroup` — wired in Inspector by S05
   - `[SerializeField] private float _fadeDuration = 0.3f` — tunable in Inspector
   - `FadeOutAsync`: sets alpha 0, activates gameObject, interpolates 0→1 with `elapsed += Time.deltaTime` + `UniTask.Yield(ct)`, clamps final to 1f
   - `FadeInAsync`: interpolates 1→0 with same loop, clamps final to 0f, deactivates gameObject
   - `blocksRaycasts = false` set at: method entry, each loop iteration (after alpha write), and final post-clamp position — 6 total occurrences across both methods
   - `CancellationToken` threaded through every `UniTask.Yield(ct)` call

3. **Created meta files**: `Assets/Scripts/Runtime/TransitionManagement.meta` (folder) and `UnityTransitionPlayer.cs.meta` with deterministic GUIDs.

## Verification

| Check | Result |
|---|---|
| Batchmode compile exit 0 | ✅ |
| `error CS` count in compile log | ✅ 0 |
| `Unity -batchmode -runTests` → passed=32 failed=0 | ✅ 32/32 |
| Static guard (no unexpected static fields) | ✅ no output |
| `blocksRaycasts = false` present | ✅ 6 matches |
| No `using UnityEngine` in Core/TransitionManagement | ✅ no output |

## Diagnostics

- **Inspector signal**: `_canvasGroup` and `_fadeDuration` are visible in Unity Inspector at runtime. A `NullReferenceException` on first `FadeOutAsync`/`FadeInAsync` call means `_canvasGroup` was not wired in the scene — check the component's Inspector slot.
- **Post-fade-in state**: `_canvasGroup.gameObject.activeSelf == false` is the expected state between transitions. If the overlay stays visible after navigation, the deactivation path was skipped (check for early return before `SetActive(false)`).
- **Diagnostic command**: `grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` → must return ≥1 match.
- **Cancellation behavior**: If `ct` fires mid-fade, `UniTask.Yield(ct)` throws `OperationCanceledException`. Alpha is left at an intermediate value and the overlay `gameObject` remains active. S05 should handle this if cancellation is expected during transitions.

## Deviations

- `blocksRaycasts = false` is set more aggressively than the minimum required (once per method entry + loop + post-clamp, rather than just once). This is intentional: ensures no frame window where alpha > 0 but `blocksRaycasts` was left true by some prior code path, and makes the invariant explicit and grep-verifiable.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — new MonoBehaviour implementing ITransitionPlayer with CanvasGroup alpha interpolation, ready for S05 wiring
- `Assets/Scripts/Runtime/TransitionManagement.meta` — Unity folder meta file (new directory)
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs.meta` — Unity script meta file
- `.gsd/milestones/M001/slices/S04/tasks/T02-PLAN.md` — added `## Observability Impact` section (pre-flight fix)
- `.gsd/milestones/M001/slices/S04/S04-PLAN.md` — added failure-path NullReferenceException diagnostic check to Verification section (pre-flight fix)
