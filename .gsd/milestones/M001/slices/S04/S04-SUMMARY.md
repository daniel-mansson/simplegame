---
id: S04
parent: M001
milestone: M001
provides:
  - ITransitionPlayer interface (Core/TransitionManagement) — pure C#, no UnityEngine
  - ScreenManager extended with optional ITransitionPlayer + IInputBlocker injection
  - Full transition orchestration: Block → FadeOut → unload → load → FadeIn → Unblock (finally)
  - UnityTransitionPlayer MonoBehaviour (Runtime/TransitionManagement) — CanvasGroup alpha interpolation, ready for S05 wiring
  - 5 new edit-mode tests proving orchestration, input-blocking brackets, GoBack, null-player passthrough, exception safety
requires:
  - slice: S02
    provides: ScreenManager with ShowScreenAsync/GoBackAsync, ISceneLoader
  - slice: S03
    provides: IInputBlocker interface (reference-counting contract), MockInputBlocker
  - slice: S01
    provides: UniTask (async operations), base project structure
affects:
  - S05
key_files:
  - Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs
  - Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
  - Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs
  - Assets/Tests/EditMode/TransitionTests.cs
key_decisions:
  - ITransitionPlayer and IInputBlocker injected as optional constructor params (null defaults) — all 27 existing ScreenManager tests compile and pass unchanged (Decision #14)
  - MergedLogTransitionPlayer/MergedLogSceneLoader helpers share a single List<string> for deterministic ordering assertions (Decision #15)
  - blocksRaycasts = false set at method entry, per-loop-iteration, and post-clamp in UnityTransitionPlayer — 6 occurrences total — to eliminate any frame window where overlay could steal input
  - gameObject.SetActive(false) on FadeInAsync completion — overlay is fully non-interacting between transitions
patterns_established:
  - Transition brackets: Block() → FadeOutAsync → unload → load → FadeInAsync → Unblock() (finally)
  - Optional DI into ScreenManager — null guard ensures zero behavior change when no transition player supplied
  - MonoBehaviour + [SerializeField] CanvasGroup + [SerializeField] float _fadeDuration — mirrors UnityInputBlocker structural pattern
observability_surfaces:
  - MockTransitionPlayer.CallLog — ordered "fadeOut"/"fadeIn" entries; inspect after each test for call-sequence diagnosis
  - MockInputBlocker.BlockCallCount / UnblockCallCount — verify symmetric bracketing; mismatch pinpoints missing finally clause
  - UnityTransitionPlayer Inspector — _canvasGroup and _fadeDuration visible at runtime; NullReferenceException on first fade call = missing wire-up
  - Post-FadeIn state — _canvasGroup.gameObject.activeSelf == false is inspectable invariant; true after navigation = deactivation skipped
  - Diagnostic command: grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs → confirms Unblock() inside finally
drill_down_paths:
  - .gsd/milestones/M001/slices/S04/tasks/T01-SUMMARY.md
  - .gsd/milestones/M001/slices/S04/tasks/T02-SUMMARY.md
duration: ~40m
verification_result: passed
completed_at: 2026-03-15
---

# S04: Transition System

**ITransitionPlayer interface created, ScreenManager transparently gains optional fade transitions with input-blocked brackets, UnityTransitionPlayer MonoBehaviour ready for S05 wiring — 32/32 tests pass.**

## What Happened

**T01** delivered the core transition contract and orchestration integration. `ITransitionPlayer` was created as a pure C# interface in `Core/TransitionManagement/` with `FadeOutAsync(CancellationToken)` and `FadeInAsync(CancellationToken)` returning `UniTask` — no `using UnityEngine`. `ScreenManager`'s constructor gained two optional parameters: `ITransitionPlayer transitionPlayer = null` and `IInputBlocker inputBlocker = null`. When a transition player is present, both `ShowScreenAsync` and `GoBackAsync` execute the full orchestration sequence: `Block()` before fade-out, `await FadeOutAsync` before scene unload, `await FadeInAsync` after scene load, with `Unblock()` in the `finally` block for guaranteed exception safety. When null, all existing behavior is preserved exactly — the 27 prior tests pass without a single line change.

Five edit-mode tests in `TransitionTests.cs` prove the full contract: (1) call ordering — fadeOut before unload, fadeIn after load; (2) input blocking symmetry — Block/Unblock called once each, IsBlocked false after completion; (3) GoBackAsync plays the same full sequence; (4) null transition player preserves original behavior with zero side effects; (5) exception safety — Unblock() is called even when the scene loader throws. Two thin helper wrappers (`MergedLogTransitionPlayer`, `MergedLogSceneLoader`) share a single `List<string>` to produce a deterministic merged event log for ordering assertions.

**T02** delivered the concrete Unity implementation. `UnityTransitionPlayer` is a `MonoBehaviour` implementing `ITransitionPlayer` with `[SerializeField] CanvasGroup _canvasGroup` and `[SerializeField] float _fadeDuration = 0.3f`. `FadeOutAsync` sets alpha to 0, activates the overlay, then interpolates alpha 0→1 using `elapsed += Time.deltaTime` and `UniTask.Yield(ct)`. `FadeInAsync` interpolates 1→0 and deactivates the overlay after completion. `blocksRaycasts = false` is set at method entry, each loop iteration, and post-clamp — 6 total occurrences — ensuring the overlay never steals input regardless of animation state. The `_fadeDuration` SerializeField lets S05 tune the value in the Inspector without code changes.

## Verification

- `Unity -batchmode -runTests -testResults TestResults.xml` → `total="32" passed="32" failed="0"` ✅
- 5 new tests confirmed: `ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`, `ShowScreenAsync_WithTransition_BlocksAndUnblocksInput`, `GoBackAsync_WithTransition_PlaysFullTransitionSequence`, `ShowScreenAsync_WithoutTransition_BehavesIdentically`, `ShowScreenAsync_WithTransition_UnblocksInputOnException`
- All 27 prior tests pass unchanged ✅
- Static guard: `grep -r "static " --include="*.cs" Assets/Scripts/Core/TransitionManagement/ Assets/Scripts/Runtime/TransitionManagement/ | grep -v "static void|static class|static readonly|static async|static UniTask"` → no output ✅
- No UnityEngine in Core: `grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs` → no output ✅
- `blocksRaycasts = false` present: `grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` → 6 matches ✅
- `finally` guard: `grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` → lines 75, 118 (both navigation methods) ✅

## Requirements Advanced

- R013 (Fade transitions between screens) — ITransitionPlayer contract created, ScreenManager orchestration proven by 5 edit-mode tests, UnityTransitionPlayer ready for runtime integration
- R006 (No static state) — static guard clean across all new and modified files
- R014 (UniTask for async operations) — ITransitionPlayer uses UniTask+CancellationToken; UnityTransitionPlayer uses UniTask.Yield(ct) in fade loops
- R015 (Edit-mode unit tests) — 5 new transition orchestration tests; 32/32 total pass
- R017 (Each layer testable in isolation) — MockTransitionPlayer is pure C#; ITransitionPlayer has no UnityEngine; transition tests run without Unity runtime

## Requirements Validated

- R013 (Fade transitions between screens) — Full orchestration contract proven: Block → FadeOut → unload → load → FadeIn → Unblock; input blocked for duration; exception safety via finally; GoBack plays same sequence; null player preserves original behavior; UnityTransitionPlayer compiles and follows CanvasGroup pattern; 32/32 tests pass.

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

- The ordering test (`ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`) uses `MergedLogTransitionPlayer` + `MergedLogSceneLoader` helper wrappers rather than stitching separate call log indices together. This unplanned refinement produces cleaner assertion messages and more robust ordering detection. Recorded as Decision #15.
- `blocksRaycasts = false` is set more aggressively in `UnityTransitionPlayer` than the minimum required (once per method entry + per loop iteration + post-clamp, rather than just once). This ensures no frame window where alpha > 0 but `blocksRaycasts` was left true by a prior code path, and makes the invariant grep-verifiable with a single command.

## Known Limitations

- `UnityTransitionPlayer` is not yet wired into any scene — it requires S05 to place the component on a Canvas in the persistent scene, assign `_canvasGroup` in the Inspector, and pass it to the `ScreenManager` constructor at boot.
- If `CancellationToken` fires mid-fade in `UnityTransitionPlayer`, `UniTask.Yield(ct)` throws `OperationCanceledException`. Alpha is left at an intermediate value and the overlay `gameObject` remains active. S05 should handle this if cancellation is expected during transitions.
- Fade duration tuning is a S05 concern — `_fadeDuration = 0.3f` is a reasonable default but may need adjustment after visual review in a running scene.

## Follow-ups

- S05 must: place `UnityTransitionPlayer` component on a high-sort-order Canvas overlay in the persistent scene; assign `_canvasGroup` SerializeField in Inspector; pass the instance into `ScreenManager` constructor alongside `UnityInputBlocker`; verify fade plays during real screen navigation.
- S05 must: place `UnityInputBlocker` component and pass to `ScreenManager` constructor alongside `UnityTransitionPlayer` (both optional params are now wired together for the first time at runtime).

## Files Created/Modified

- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — new; pure C# interface with FadeOutAsync/FadeInAsync returning UniTask; no UnityEngine imports
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — modified; optional ITransitionPlayer + IInputBlocker constructor params; transition brackets in ShowScreenAsync and GoBackAsync with finally-block Unblock()
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — new; MonoBehaviour implementing ITransitionPlayer with CanvasGroup alpha interpolation; blocksRaycasts=false enforced at 6 points
- `Assets/Scripts/Runtime/TransitionManagement.meta` — new; Unity folder meta file
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs.meta` — new; Unity script meta file
- `Assets/Tests/EditMode/TransitionTests.cs` — new; MockTransitionPlayer + ThrowingSceneLoader + MergedLog helpers + 5 orchestration tests
- `.gsd/milestones/M001/slices/S04/S04-PLAN.md` — enriched Observability/Diagnostics section with failure-path diagnostic command
- `.gsd/milestones/M001/slices/S04/tasks/T01-PLAN.md` — added Observability Impact section
- `.gsd/milestones/M001/slices/S04/tasks/T02-PLAN.md` — added Observability Impact section
- `.gsd/DECISIONS.md` — decisions #14 and #15 appended

## Forward Intelligence

### What the next slice should know
- `ScreenManager` now accepts `(ISceneLoader loader, ITransitionPlayer transitionPlayer = null, IInputBlocker inputBlocker = null)`. S05 must pass both `UnityTransitionPlayer` and `UnityInputBlocker` instances. Passing only one or neither is valid and tested — but for the full runtime experience both must be provided.
- `UnityTransitionPlayer` expects `_canvasGroup` to be wired in the Inspector. Forgetting this causes a `NullReferenceException` on the first fade call. The error won't appear until the first real navigation in play mode — check the Inspector before entering play mode.
- `UnityInputBlocker` and `UnityTransitionPlayer` should both live on the persistent scene's overlay Canvas. They are independent components but work in concert via `ScreenManager`'s injection.
- The `_fadeDuration = 0.3f` default on `UnityTransitionPlayer` is tunable in the Inspector without code changes.

### What's fragile
- `UnityTransitionPlayer` cancellation behavior — mid-fade cancellation leaves the overlay active at an intermediate alpha. If S05 needs to cancel navigation (e.g., timeout or error), it must either handle `OperationCanceledException` explicitly or ensure the overlay is reset to a clean state.
- `ScreenManager._isNavigating` guard prevents concurrent navigation but does not queue requests — a second `ShowScreenAsync` call during a transition is silently dropped. S05 demo flow should not rely on rapid successive navigation calls.

### Authoritative diagnostics
- `grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — must show `_inputBlocker?.Unblock()` inside finally at lines 75 and 118; absence means exception-safety invariant is broken.
- `grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — must return ≥1 match; absence means the overlay can steal input during fade.
- `_canvasGroup.gameObject.activeSelf == false` after a completed navigation — if true, the overlay stayed visible; check `FadeInAsync` deactivation path.
- `TestResults.xml total="32" passed="32" failed="0"` — reference baseline for S05 regression check.

### What assumptions changed
- No assumptions changed. The optional-parameter injection approach worked cleanly — all 27 existing tests required zero modification, confirming the null-default design was the right call.
