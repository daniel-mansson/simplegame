---
id: T01
parent: S04
milestone: M001
provides:
  - ITransitionPlayer interface (Core/TransitionManagement)
  - ScreenManager optional transition injection (ShowScreenAsync + GoBackAsync)
  - 5 edit-mode tests proving full transition orchestration contract
key_files:
  - Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs
  - Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
  - Assets/Tests/EditMode/TransitionTests.cs
key_decisions:
  - ITransitionPlayer and IInputBlocker injected as optional constructor params (null defaults) — existing 1-arg constructors unchanged
  - MergedLogTransitionPlayer/MergedLogSceneLoader helpers share a single List<string> for deterministic ordering assertions
patterns_established:
  - Transition brackets: Block() → FadeOutAsync → unload → load → FadeInAsync → Unblock() (finally)
  - Optional DI into ScreenManager — null guard ensures zero behavior change when no transition player supplied
observability_surfaces:
  - MockTransitionPlayer.CallLog — ordered "fadeOut"/"fadeIn" entries; inspect after each test for call-sequence diagnosis
  - MockInputBlocker.BlockCallCount / UnblockCallCount — verify symmetric bracketing; mismatch pinpoints missing finally clause
  - grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs — confirms Unblock() inside finally; absence is root cause of exception-safety defect
duration: ~25m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T01: ITransitionPlayer interface, ScreenManager integration, and edit-mode tests

**ITransitionPlayer contract created, ScreenManager extended with optional fade-transition injection, and 5 edit-mode tests prove the full orchestration sequence — 32/32 tests pass.**

## What Happened

1. **Pre-flight fixes**: Added `## Observability Impact` section to T01-PLAN.md and enriched the `## Observability / Diagnostics` section in S04-PLAN.md with a diagnostic command and failure-path check.

2. **ITransitionPlayer interface** (`Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs`): Pure C# interface with `FadeOutAsync(CancellationToken)` and `FadeInAsync(CancellationToken)` returning `UniTask`. No `using UnityEngine`. Lives in `SimpleGame.Core.TransitionManagement` namespace.

3. **ScreenManager modified** (`Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`): Constructor gains optional `ITransitionPlayer transitionPlayer = null, IInputBlocker inputBlocker = null` parameters. When `_transitionPlayer != null`, both `ShowScreenAsync` and `GoBackAsync` execute: `Block()` → `FadeOutAsync` → unload → load → `FadeInAsync`, with `Unblock()` in the `finally` block for exception safety. When null, all existing behavior is preserved exactly.

4. **TransitionTests.cs** (`Assets/Tests/EditMode/TransitionTests.cs`): Five tests using `MockTransitionPlayer` (new), `MockSceneLoader` (reused from ScreenManagerTests), `MockInputBlocker` (reused from PopupManagerTests), and `ThrowingSceneLoader` (new, throws on load). Two thin helpers — `MergedLogTransitionPlayer` and `MergedLogSceneLoader` — share a single `List<string>` to produce a single deterministic ordering log across interleaved async calls.

## Verification

- `Unity -batchmode -runTests -testResults TestResults.xml` → `total="32" passed="32" failed="0"`
- 5 new tests confirmed: `GoBackAsync_WithTransition_PlaysFullTransitionSequence`, `ShowScreenAsync_WithoutTransition_BehavesIdentically`, `ShowScreenAsync_WithTransition_BlocksAndUnblocksInput`, `ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`, `ShowScreenAsync_WithTransition_UnblocksInputOnException`
- `grep -r "static " --include="*.cs" Assets/Scripts/Core/TransitionManagement/ | grep -v "static void|static class|static readonly|static async|static UniTask"` → no output ✓
- `grep -n "using UnityEngine" Assets/Scripts/Core/TransitionManagement/*.cs` → no output ✓
- `grep "new ScreenManager" Assets/Tests/EditMode/ScreenManagerTests.cs` → all three calls are single-arg ✓

## Diagnostics

- **MockTransitionPlayer.CallLog**: List of "fadeOut"/"fadeIn" strings in call order. If `CallsFadeOutBeforeUnload` fails, inspect `mergedLog` contents in the assertion message.
- **MockInputBlocker.BlockCallCount / UnblockCallCount**: If `UnblocksInputOnException` fails, `UnblockCallCount == 0` means the finally block is missing or guarded by a condition that short-circuits on exception.
- **Diagnostic command**: `grep -n "finally" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — confirms `_inputBlocker?.Unblock()` is inside finally; absence is the root cause.

## Deviations

The ordering test (`ShowScreenAsync_WithTransition_CallsFadeOutBeforeUnloadAndFadeInAfterLoad`) uses `MergedLogTransitionPlayer` + `MergedLogSceneLoader` helper wrappers rather than stitching separate `_transition.CallLog` and `_loader.CallLog` indices together. This was an unplanned refinement: a shared merged log is more robust and produces cleaner assertion messages. Recorded as Decision #15.

## Known Issues

none

## Files Created/Modified

- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — new; pure C# interface with FadeOutAsync/FadeInAsync
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — modified; optional ITransitionPlayer + IInputBlocker, transition brackets in ShowScreenAsync/GoBackAsync
- `Assets/Tests/EditMode/TransitionTests.cs` — new; MockTransitionPlayer + ThrowingSceneLoader + MergedLog helpers + 5 orchestration tests
- `.gsd/milestones/M001/slices/S04/S04-PLAN.md` — pre-flight: enriched Observability/Diagnostics section with failure-path diagnostic command
- `.gsd/milestones/M001/slices/S04/tasks/T01-PLAN.md` — pre-flight: added Observability Impact section
- `.gsd/DECISIONS.md` — decisions #14 and #15 appended
