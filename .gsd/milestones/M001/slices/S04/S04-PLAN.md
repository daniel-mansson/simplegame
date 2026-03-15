# S04: Transition System

**Goal:** TransitionManager/ITransitionPlayer plays a fade-to-black transition during screen navigation, with input blocked for the duration. ScreenManager transparently gains transitions via optional dependency injection.
**Demo:** Edit-mode tests prove the full orchestration sequence (block input → fade out → unload → load → fade in → unblock input) using mock doubles. UnityTransitionPlayer MonoBehaviour is ready for play-mode integration in S05.

## Must-Haves

- `ITransitionPlayer` interface in `Core/TransitionManagement/` with `FadeOutAsync` and `FadeInAsync` — no UnityEngine imports
- `ScreenManager` constructor gains optional `ITransitionPlayer` and `IInputBlocker` parameters (null defaults) — all 27 existing tests pass unchanged
- When `ITransitionPlayer` is injected, `ShowScreenAsync` and `GoBackAsync` execute: block input → fade out → unload → load → fade in → unblock input
- When `ITransitionPlayer` is null, behavior is identical to current (no transitions, no input blocking)
- Input unblock happens in `finally` block to prevent permanent blocking on exception
- `UnityTransitionPlayer` MonoBehaviour with CanvasGroup alpha interpolation (0→1 fade out, 1→0 fade in)
- Transition overlay `blocksRaycasts = false` always — input blocking is exclusively via `IInputBlocker`
- No static state fields in any new or modified file

## Proof Level

- This slice proves: contract (orchestration sequence verified by edit-mode tests) + runtime component ready (UnityTransitionPlayer)
- Real runtime required: no (runtime integration deferred to S05 play-mode walkthrough)
- Human/UAT required: no (fade duration tuning is S05 concern)

## Verification

- `Assets/Tests/EditMode/TransitionTests.cs` — edit-mode tests for:
  - Transition-integrated ScreenManager calls FadeOutAsync before unload and FadeInAsync after load
  - Input blocker Block()/Unblock() brackets the full transition+navigation sequence
  - GoBackAsync also plays transitions
  - Null transition player preserves existing behavior (no calls, no blocking)
  - Input unblock happens even if scene load throws (finally block)
- All 27 existing tests pass: `Unity -batchmode -runTests -testResults TestResults.xml` → `passed="32+"` (27 existing + new transition tests), `failed="0"`
- Static guard: `grep -r "static " --include="*.cs" Assets/Scripts/Core/TransitionManagement/ Assets/Scripts/Runtime/TransitionManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output
- No UnityEngine in Core: `grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs` → no output

- **Failure-path diagnostic:** After a failed batchmode run, scan `Editor.log` for `NullReferenceException` in `UnityTransitionPlayer` — a missing `_canvasGroup` SerializeField wire-up is the most common cause. `grep -n "NullReferenceException" Editor.log` pinpoints the line; absence of the field in Inspector causes silent null at first fade call.

## Observability / Diagnostics

- Runtime signals: `MockTransitionPlayer.CallLog` records ordered "fadeOut"/"fadeIn" entries for test diagnostics
- Inspection surfaces: `MockInputBlocker.BlockCallCount`/`UnblockCallCount` from S03 reused to verify input blocking brackets
- Failure visibility: NUnit assertion messages include full CallLog contents on failure; a failed exception-safety test reveals `UnblockCallCount == 0` indicating the finally block is missing
- Diagnostic command: `grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — must show `_inputBlocker?.Unblock()` inside the finally block; absence indicates the exception-safety invariant is not met
- Redaction constraints: none

## Integration Closure

- Upstream surfaces consumed: `ScreenManager.cs` (S02), `IInputBlocker` (S03), UniTask (S01)
- New wiring introduced in this slice: `ITransitionPlayer` and `IInputBlocker` injected into `ScreenManager` constructor (optional, null defaults)
- What remains before the milestone is truly usable end-to-end: S05 must construct `UnityTransitionPlayer` and `UnityInputBlocker` MonoBehaviours in boot scene, pass them to `ScreenManager` constructor, and verify fade plays during real screen navigation

## Tasks

- [x] **T01: ITransitionPlayer interface, ScreenManager integration, and edit-mode tests** `est:40m`
  - Why: Creates the core transition contract, wires it into ScreenManager's existing navigation flow, and proves the orchestration sequence with edit-mode tests. This is the primary deliverable for R013.
  - Files: `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs`, `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`, `Assets/Tests/EditMode/TransitionTests.cs`, `Assets/Tests/EditMode/ScreenManagerTests.cs` (verify unchanged)
  - Do: Create `ITransitionPlayer` with `FadeOutAsync`/`FadeInAsync` in `Core/TransitionManagement/`. Modify `ScreenManager` constructor to accept optional `ITransitionPlayer transitionPlayer = null, IInputBlocker inputBlocker = null`. In `ShowScreenAsync`/`GoBackAsync`, when transition player is non-null: call `Block()` before fade-out, `await FadeOutAsync` before unload, `await FadeInAsync` after load, `Unblock()` in `finally`. Create `MockTransitionPlayer` with `CallLog`. Write tests proving orchestration sequence, input blocking brackets, GoBack transitions, null-player passthrough, and exception safety.
  - Verify: `Unity -batchmode -runTests` → all existing 27 tests pass + new transition tests pass, `failed="0"`. Static guard clean. No UnityEngine in Core/TransitionManagement/.
  - Done when: Edit-mode tests prove the full transition orchestration contract; all 27 existing tests still pass unchanged; no static state; ITransitionPlayer has no Unity imports.

- [x] **T02: UnityTransitionPlayer runtime MonoBehaviour** `est:20m`
  - Why: Provides the concrete Unity implementation that S05 will wire into the boot scene. Follows the UnityInputBlocker pattern (MonoBehaviour + CanvasGroup).
  - Files: `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs`
  - Do: Create MonoBehaviour implementing `ITransitionPlayer`. `[SerializeField] CanvasGroup _canvasGroup` and `[SerializeField] float _fadeDuration = 0.3f`. FadeOutAsync: set alpha 0, activate GameObject, interpolate alpha 0→1 using `Time.deltaTime` + `UniTask.Yield(ct)`, clamp final to 1. FadeInAsync: interpolate alpha 1→0, clamp final to 0, deactivate GameObject. Keep `blocksRaycasts = false` on the CanvasGroup always (input blocking is IInputBlocker's job). No static fields.
  - Verify: File compiles with zero errors in batchmode. Static guard clean. `blocksRaycasts` explicitly set to false in both methods.
  - Done when: `UnityTransitionPlayer.cs` compiles, follows the CanvasGroup alpha interpolation pattern from research, and is ready for S05 wiring.

## Files Likely Touched

- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — new
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — modified (constructor + transition brackets)
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — new
- `Assets/Tests/EditMode/TransitionTests.cs` — new
