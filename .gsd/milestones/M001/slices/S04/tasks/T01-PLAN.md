---
estimated_steps: 7
estimated_files: 4
---

# T01: ITransitionPlayer interface, ScreenManager integration, and edit-mode tests

**Slice:** S04 — Transition System
**Milestone:** M001

## Description

Creates the `ITransitionPlayer` interface as the core transition contract, modifies `ScreenManager` to optionally consume it alongside `IInputBlocker`, and proves the full orchestration sequence with edit-mode tests. This is the primary deliverable for R013 (fade transitions between screens) and the contract proof for the slice.

The key design choice (per research and S02 forward intelligence): inject `ITransitionPlayer` and `IInputBlocker` as optional constructor parameters into `ScreenManager` rather than creating a wrapper class. This keeps navigation atomic within the existing `_isNavigating` guard and avoids a "raw vs wrapped" navigation split.

## Steps

1. Create `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — interface with `UniTask FadeOutAsync(CancellationToken ct = default)` and `UniTask FadeInAsync(CancellationToken ct = default)`. Namespace `SimpleGame.Core.TransitionManagement`. No `using UnityEngine`.
2. Modify `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`:
   - Add `using SimpleGame.Core.TransitionManagement;` and `using SimpleGame.Core.PopupManagement;`
   - Change constructor to `ScreenManager(ISceneLoader sceneLoader, ITransitionPlayer transitionPlayer = null, IInputBlocker inputBlocker = null)`. Store as `_transitionPlayer` and `_inputBlocker` instance fields.
   - In `ShowScreenAsync`, when `_transitionPlayer != null`: call `_inputBlocker.Block()` at entry, `await _transitionPlayer.FadeOutAsync(ct)` before unload, `await _transitionPlayer.FadeInAsync(ct)` after load, `_inputBlocker?.Unblock()` in the `finally` block.
   - Apply the same pattern to `GoBackAsync`.
   - Guard: only call `_inputBlocker.Block()` and transition methods when `_transitionPlayer != null`. When null, behavior is identical to current code.
3. Create `Assets/Tests/EditMode/TransitionTests.cs` with:
   - `MockTransitionPlayer` — implements `ITransitionPlayer`, records "fadeOut"/"fadeIn" to a `CallLog` list, returns `UniTask.CompletedTask`.
   - Reuse `MockSceneLoader` and `MockInputBlocker` from existing test files (they're `internal` in the same assembly).
   - Test: `ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad` — verify CallLog ordering: fadeOut → unload → load → fadeIn.
   - Test: `ShowScreenAsync_WithTransition_BlocksAndUnblocksInput` — verify Block() called once, Unblock() called once, IsBlocked is false after completion.
   - Test: `GoBackAsync_WithTransition_PlaysFullTransitionSequence` — same orchestration for back navigation.
   - Test: `ShowScreenAsync_WithoutTransition_BehavesIdentically` — null transition player, verify no transition calls, no input blocking.
   - Test: `ShowScreenAsync_WithTransition_UnblocksInputOnException` — scene loader that throws, verify Unblock() still called (finally block).
4. Run existing tests to confirm all 27 pass unchanged, plus new transition tests pass.

## Must-Haves

- [ ] `ITransitionPlayer` interface has no `using UnityEngine` — pure C# with UniTask
- [ ] `ScreenManager` constructor uses optional parameters with `null` defaults — existing `new ScreenManager(mockLoader)` calls compile unchanged
- [ ] Transition brackets execute in correct order: block → fadeOut → unload → load → fadeIn → unblock
- [ ] `Unblock()` is in `finally` block for exception safety
- [ ] All 27 existing tests pass without modification
- [ ] New tests prove orchestration sequence, input blocking, GoBack transitions, null passthrough, and exception safety
- [ ] No static state fields in any new or modified file

## Verification

- `Unity -batchmode -runTests -testResults TestResults.xml` → total includes 27 existing + 5 new transition tests, `failed="0"`
- `grep -r "static " --include="*.cs" Assets/Scripts/Core/TransitionManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output
- `grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs` → no output
- `grep "new ScreenManager" Assets/Tests/EditMode/ScreenManagerTests.cs` → all existing constructor calls unchanged (still one-arg)

## Observability Impact

- `MockTransitionPlayer.CallLog` — ordered list of "fadeOut"/"fadeIn" strings; inspect after each test to verify the exact call sequence
- `MockInputBlocker.BlockCallCount` / `UnblockCallCount` — verify symmetric bracketing; a `BlockCallCount==1, UnblockCallCount==0` result on the exception test identifies a missing finally block
- On exception-safety failure the NUnit output shows `UnblockCallCount` expected 1 got 0, pinpointing the missing `_inputBlocker?.Unblock()` in the finally clause
- Diagnostic command: `grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — confirm `_inputBlocker?.Unblock()` appears inside finally; absence is the root cause of the exception-safety defect

## Inputs

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — modification target; current constructor takes only `ISceneLoader`
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — `Block()`/`Unblock()`/`IsBlocked` interface consumed by ScreenManager
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — existing 8 tests + `MockSceneLoader` that must not break
- `Assets/Tests/EditMode/PopupManagerTests.cs` — `MockInputBlocker` class to reuse in transition tests
- S04 Research: recommended approach A (inject into ScreenManager), orchestration sequence, pitfalls documented

## Expected Output

- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — new interface with FadeOutAsync/FadeInAsync
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — modified with optional ITransitionPlayer + IInputBlocker, transition brackets in ShowScreenAsync/GoBackAsync
- `Assets/Tests/EditMode/TransitionTests.cs` — MockTransitionPlayer + 5 edit-mode tests proving the transition orchestration contract
