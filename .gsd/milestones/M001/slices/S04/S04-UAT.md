---
id: S04
parent: M001
milestone: M001
---

# S04: Transition System — UAT

**Milestone:** M001
**Written:** 2026-03-15

## UAT Type

- UAT mode: artifact-driven (edit-mode tests) + runtime component ready (UnityTransitionPlayer for S05 play-mode)
- Why this mode is sufficient: The slice plan explicitly defers real runtime integration to S05. The orchestration contract is fully proven by the 5 edit-mode tests. `UnityTransitionPlayer` compiles and is structurally ready — runtime visual verification (actual fade playing during navigation) is the S05 play-mode walkthrough concern.

## Preconditions

- Unity project opens without compile errors
- `TestResults.xml` from the most recent batchmode run shows `total="32" passed="32" failed="0"`
- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` exists
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` exists
- `Assets/Tests/EditMode/TransitionTests.cs` exists with 5 test methods

## Smoke Test

Run: `grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs`

**Expected:** no output — ITransitionPlayer has no Unity dependency.

Then open Unity and confirm the project compiles with zero errors in the Console window.

---

## Test Cases

### 1. Transition orchestration order — fadeOut before unload, fadeIn after load

**Verifies:** `ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`

1. Open `Assets/Tests/EditMode/TransitionTests.cs` in the editor.
2. In the Unity Test Runner (Window → General → Test Runner → EditMode), locate `TransitionTests` → `ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`.
3. Run the test.
4. **Expected:** Test passes. The merged log records events in order: `fadeOut`, `unload:MainMenu`, `load:Settings`, `fadeIn`. No reordering.

### 2. Input blocking brackets the full transition sequence

**Verifies:** `ShowScreenAsync_WithTransition_BlocksAndUnblocksInput`

1. In the Unity Test Runner, locate `TransitionTests` → `ShowScreenAsync_WithTransition_BlocksAndUnblocksInput`.
2. Run the test.
3. **Expected:** Test passes. `BlockCallCount == 1`, `UnblockCallCount == 1`, `IsBlocked == false` after navigation completes. The Block/Unblock pair is symmetric.

### 3. GoBackAsync plays the same full transition sequence

**Verifies:** `GoBackAsync_WithTransition_PlaysFullTransitionSequence`

1. In the Unity Test Runner, locate `TransitionTests` → `GoBackAsync_WithTransition_PlaysFullTransitionSequence`.
2. Run the test.
3. **Expected:** Test passes. After navigating MainMenu → Settings → GoBack, the merged log for GoBack records: `fadeOut`, `unload:Settings`, `load:MainMenu`, `fadeIn`. Block/Unblock called once each for the GoBack; `IsBlocked == false` after completion.

### 4. Null transition player — no regression in original behavior

**Verifies:** `ShowScreenAsync_WithoutTransition_BehavesIdentically`

1. In the Unity Test Runner, locate `TransitionTests` → `ShowScreenAsync_WithoutTransition_BehavesIdentically`.
2. Run the test.
3. **Expected:** Test passes. A `ScreenManager` constructed with only `ISceneLoader` (no transition player) produces identical load/unload behavior: `load:MainMenu`, `unload:MainMenu`, `load:Settings`. Neither `MockTransitionPlayer.CallLog` nor `MockInputBlocker.BlockCallCount` are incremented.

### 5. Exception safety — Unblock() called even when scene load throws

**Verifies:** `ShowScreenAsync_WithTransition_UnblocksInputOnException`

1. In the Unity Test Runner, locate `TransitionTests` → `ShowScreenAsync_WithTransition_UnblocksInputOnException`.
2. Run the test.
3. **Expected:** Test passes. `InvalidOperationException` is thrown and propagated. `BlockCallCount == 1`, `UnblockCallCount == 1`, `IsBlocked == false`. The finally block guarantees input is never permanently blocked even on failure.

### 6. Full edit-mode test suite passes — no regressions

1. In the Unity Test Runner (EditMode tab), click "Run All".
2. **Expected:** All 32 tests pass. `failed = 0`. The 27 tests from S01/S02/S03 pass unchanged alongside the 5 new transition tests.

### 7. ITransitionPlayer has no Unity dependency

1. Run: `grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs`
2. **Expected:** No output. `ITransitionPlayer` is pure C# — no Unity imports allowed in the Core layer.

### 8. Static guard — no static state in new files

1. Run: `grep -r "static " --include="*.cs" Assets/Scripts/Core/TransitionManagement/ Assets/Scripts/Runtime/TransitionManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"`
2. **Expected:** No output. No static state fields in any new file.

### 9. UnityTransitionPlayer blocksRaycasts enforcement

1. Run: `grep "blocksRaycasts = false" Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs`
2. **Expected:** At least 1 match (actual: 6 matches — method entry, per-loop-iteration, and post-clamp in both FadeOutAsync and FadeInAsync). Confirms the overlay never steals input during animation.

### 10. UnityTransitionPlayer compiles and serialized fields are present

1. Open Unity. In the Project window, locate `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs`.
2. Create a temporary empty GameObject in an open scene. Add Component → `UnityTransitionPlayer`.
3. **Expected:** Component is visible in the Inspector with two serialized fields: `Canvas Group` (object reference slot, initially empty) and `Fade Duration` (float, default 0.3). No compile errors in the Console.
4. Remove the test GameObject when done.

---

## Edge Cases

### Exception mid-transition — input permanently blocked

1. Construct a `ScreenManager` with a `ThrowingSceneLoader` (from `TransitionTests.cs`), a `MockTransitionPlayer`, and a `MockInputBlocker`.
2. Call `ShowScreenAsync(ScreenId.MainMenu).GetAwaiter().GetResult()`.
3. **Expected:** `InvalidOperationException` thrown, `_inputBlocker.IsBlocked == false`. The `finally` block fires and Unblock() is called regardless of the exception — input is never left permanently blocked.

### Concurrent navigation during active transition — silent drop

1. Construct a `ScreenManager` with `MockSceneLoader`, `MockTransitionPlayer`, and `MockInputBlocker`.
2. Call `ShowScreenAsync(ScreenId.MainMenu).Forget()` (first navigation in progress).
3. Immediately call `ShowScreenAsync(ScreenId.Settings).Forget()` (concurrent call while `_isNavigating = true`).
4. **Expected:** Second call is silently dropped (no-op). Only one navigation completes. `_loader.CallLog` contains only `load:MainMenu`, not `load:Settings`. No deadlock, no exception, no permanent input block.

### GoBackAsync with no history — no-op

1. Construct a `ScreenManager` with `MockSceneLoader`, `MockTransitionPlayer`, and `MockInputBlocker` (no prior `ShowScreenAsync` call).
2. Call `GoBackAsync().GetAwaiter().GetResult()`.
3. **Expected:** No scene operations. `MockTransitionPlayer.CallLog` is empty. `BlockCallCount == 0`. No exceptions.

---

## Failure Signals

- **32 tests not all passing**: One of the 5 new transition tests failing — inspect NUnit assertion message for `mergedLog` contents or `BlockCallCount`/`UnblockCallCount` mismatch. A `UnblockCallCount == 0` on the exception-safety test means the `finally` block is missing or guarded.
- **`grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs` has output**: ITransitionPlayer has a Unity dependency — violates Core layer purity. Remove the import.
- **Static guard produces output**: A `static` field was introduced in a new or modified file. Remove it and use instance state.
- **NullReferenceException on `FadeOutAsync`/`FadeInAsync` in play mode**: `_canvasGroup` SerializeField was not wired in the Inspector. Open the `UnityTransitionPlayer` component on the transition overlay Canvas and drag-assign the CanvasGroup.
- **Transition overlay visible after navigation completes**: `_canvasGroup.gameObject.activeSelf == true` post-fade-in means `SetActive(false)` was skipped. Check `FadeInAsync` for early returns before the deactivation call.
- **`blocksRaycasts` grep returns no output**: `blocksRaycasts = false` assignments were removed from `UnityTransitionPlayer`. Overlay can steal input during fade — restore the assignments.

---

## Requirements Proved By This UAT

- R013 (Fade transitions between screens) — Full orchestration contract: Block → FadeOut → unload → load → FadeIn → Unblock; input blocked for duration; exception safety via finally; GoBack plays same sequence; null player preserves original behavior; UnityTransitionPlayer compiles and follows CanvasGroup pattern.
- R006 (No static state) — static guard clean for all new/modified files.
- R014 (UniTask for async operations) — ITransitionPlayer returns UniTask; UnityTransitionPlayer uses UniTask.Yield(ct) in fade loops; CancellationToken threaded throughout.
- R015 (Edit-mode tests) — 5 transition orchestration tests + 27 prior tests = 32/32 passing in batchmode.
- R017 (Each layer testable in isolation) — MockTransitionPlayer is pure C#; ITransitionPlayer has no UnityEngine; all 5 tests run without Unity runtime.

---

## Not Proven By This UAT

- **Visual fade quality**: The actual rendered fade between screens (smoothness, color, duration feel) is not verified here — that requires S05 play-mode walkthrough with real scenes loaded.
- **Real scene swap during fade**: The test suite uses `MockSceneLoader`; actual Unity scene load/unload timing during a fade is not proven until S05 wires everything together in a running scene.
- **Cancellation mid-fade**: `OperationCanceledException` behavior when `ct` fires during `UniTask.Yield(ct)` in `UnityTransitionPlayer` is not tested — deferred to S05 if cancellation is needed.
- **`_fadeDuration` tuning**: The default 0.3f is untested against real scenes — S05 play-mode walkthrough is the opportunity to tune this in the Inspector.
- **Persistent scene overlay Canvas setup**: `UnityTransitionPlayer` requires a specific scene hierarchy (overlay Canvas, CanvasGroup component, correct sort order) — this wiring is S05's responsibility.

---

## Notes for Tester

- Test cases 1–5 correspond directly to the 5 `[Test]` methods in `TransitionTests.cs`. Each test is self-contained and uses synchronous mock doubles — no async Unity Test Runner overhead.
- Test case 6 (full suite) is the regression check — run it last to confirm S01/S02/S03 tests still pass.
- Test cases 7–9 are command-line checks — run them in a shell at the project root; no Unity editor needed.
- Test case 10 (Inspector check) requires Unity editor open. It confirms the MonoBehaviour is properly structured for S05 wiring — the empty `Canvas Group` slot is expected; S05 fills it.
- For the concurrent-navigation edge case, note that `Forget()` swallows exceptions — use `GetAwaiter().GetResult()` if you want to observe exception propagation.
