---
id: T02
parent: S03
milestone: M001
provides:
  - PopupManagerTests.cs — 13 edit-mode NUnit tests proving popup stack logic and IInputBlocker reference-counting contract
  - MockPopupContainer test double (CallLog, UniTask.CompletedTask)
  - MockInputBlocker test double (reference-counted: BlockCount, BlockCallCount, UnblockCallCount)
key_files:
  - Assets/Tests/EditMode/PopupManagerTests.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
key_decisions:
  - DismissAllAsync must call Unblock() per popup (not once at end) to satisfy reference-counting contract when multiple popups are open — fixed during implementation
patterns_established:
  - MockPopupContainer mirrors MockSceneLoader exactly — CallLog list, UniTask.CompletedTask returns
  - MockInputBlocker exposes BlockCount (current), BlockCallCount, UnblockCallCount for assertion granularity
  - Concurrent guard test with synchronous mocks verifies sequential non-interleaved execution (matching ScreenManagerTests pattern)
observability_surfaces:
  - TestResults.xml at project root — result="Passed" total="27" failed="0" confirms all tests pass
  - MockPopupContainer.CallLog — ordered "show:X"/"hide:X" strings; include in Assert messages for readable failures
  - MockInputBlocker.BlockCount / .BlockCallCount / .UnblockCallCount — expose reference-counted state at any assertion point
duration: ~25 min
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T02: Add PopupManager edit-mode tests with mocks

**13 NUnit edit-mode tests written and passing — TestResults.xml: result="Passed", total="27", failed="0", including all 13 PopupManager tests and all 14 existing tests.**

## What Happened

Created `Assets/Tests/EditMode/PopupManagerTests.cs` with:
- `MockPopupContainer : IPopupContainer` — records ordered `CallLog` (`"show:ConfirmDialog"` / `"hide:ConfirmDialog"` strings), returns `UniTask.CompletedTask` from both methods. Mirrors `MockSceneLoader` pattern exactly.
- `MockInputBlocker : IInputBlocker` — reference-counted with `BlockCount` (current depth), `BlockCallCount`, `UnblockCallCount` for assertion granularity. `IsBlocked => BlockCount > 0`.
- 13 test methods covering: stack push/pop, container call sequence, input block/unblock, dismiss-all, empty-stack no-op, `HasActivePopup` state, concurrent guard, and `MockInputBlocker` reference-counting sequences.

**One implementation bug discovered and fixed:** `PopupManager.DismissAllAsync` originally called `_inputBlocker.Unblock()` once after the while loop. With reference counting (each `ShowPopupAsync` calls `Block()` once), dismissing 2 popups with a single `Unblock()` left `BlockCount=1` — `IsBlocked` stayed `true`. Fixed by moving `Unblock()` inside the while loop (once per popped popup), so the reference count returns to exactly 0 after dismiss-all. This is consistent with `DismissPopupAsync` which also calls `Unblock()` per-dismiss (when stack reaches empty).

First test run (27 tests, exit 2): 26 passed, 1 failed — `ShowPopupAsync_GuardsAgainstConcurrentOperation`. The failure was caused by a stale compiled assembly; the first run used the pre-edit binary. Second run (exit 0): 27/27 passed.

## Verification

```
# TestResults.xml — 27/27 passed
result="Passed" total="27" passed="27" failed="0"

# 13 PopupManager test names confirmed passed:
- DismissAllAsync_ClearsEntireStack               ✓ Passed
- DismissPopupAsync_CallsContainerHidePopup       ✓ Passed
- DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain ✓ Passed
- DismissPopupAsync_PopsTopPopup                  ✓ Passed
- DismissPopupAsync_UnblocksInputWhenStackEmpty   ✓ Passed
- DismissPopupAsync_WithEmptyStack_IsNoOp         ✓ Passed
- HasActivePopup_ReflectsStackState               ✓ Passed
- InputBlocker_BlockUnblockBlock_Sequence         ✓ Passed
- InputBlocker_NestedBlockUnblock                 ✓ Passed
- ShowPopupAsync_BlocksInput                      ✓ Passed
- ShowPopupAsync_CallsContainerShowPopup          ✓ Passed
- ShowPopupAsync_GuardsAgainstConcurrentOperation ✓ Passed
- ShowPopupAsync_PushesPopupOntoStack             ✓ Passed

# 14 existing tests still pass (MVPWiringTests: 6, ScreenManagerTests: 8)

# Static guard — returns nothing (no static state introduced)
grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask"
→ (no output, exit 1)

# No using UnityEngine in Core
grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/
→ (no output, exit 1)
```

## Diagnostics

```bash
# Quick result check
grep -E 'result=|failed=|passed=' /c/OtherWork/simplegame/TestResults.xml | head -3

# All PopupManager test results
grep 'PopupManagerTests' /c/OtherWork/simplegame/TestResults.xml | grep 'test-case' | grep -o 'name="[^"]*"\|result="[^"]*"'

# Re-run tests
"C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" \
  -batchmode -runTests \
  -projectPath "C:/OtherWork/simplegame" \
  -testPlatform editmode \
  -testResults "C:/OtherWork/simplegame/TestResults.xml" \
  -logFile "C:/OtherWork/simplegame/Logs/T02-test-run.log"
echo $?

# Mock state inspection in test output (CallLog format)
# MockPopupContainer.CallLog entries: "show:ConfirmDialog", "hide:ConfirmDialog"
# MockInputBlocker.BlockCount = current reference count (0 = unblocked)
# MockInputBlocker.BlockCallCount / UnblockCallCount = total call counts
```

## Deviations

- **`PopupManager.DismissAllAsync` bug fix:** The original implementation called `_inputBlocker.Unblock()` once after the while loop. This violated the reference-counting contract when multiple popups were shown: 2 `Block()` calls and 1 `Unblock()` left `BlockCount=1`. Fixed by moving `Unblock()` inside the while loop. This is a correctness fix, not a scope change — the contract was always "unblocked after dismiss-all" (Test 9 requirement).

## Known Issues

None.

## Files Created/Modified

- `Assets/Tests/EditMode/PopupManagerTests.cs` — NEW: MockPopupContainer + MockInputBlocker test doubles + 13 PopupManager test methods
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — MODIFIED: `DismissAllAsync` now calls `Unblock()` per popup inside while loop (reference-counting fix)
- `TestResults.xml` — UPDATED: 27 total tests, 27 passed, 0 failed
- `.gsd/milestones/M001/slices/S03/tasks/T02-PLAN.md` — UPDATED: Added missing `## Observability Impact` section (pre-flight requirement)
- `.gsd/milestones/M001/slices/S03/S03-PLAN.md` — UPDATED: T02 marked `[x]` done
