---
estimated_steps: 5
estimated_files: 1
---

# T02: Add PopupManager edit-mode tests with mocks

**Slice:** S03 — Popup System & Input Blocking
**Milestone:** M001

## Description

Create comprehensive edit-mode NUnit tests that prove the PopupManager stack logic, IInputBlocker reference-counting contract, and edge cases (empty dismiss, concurrent guard, dismiss-all). Uses MockPopupContainer and MockInputBlocker test doubles following the exact patterns established by MockSceneLoader in ScreenManagerTests.cs.

## Steps

1. Create `Assets/Tests/EditMode/PopupManagerTests.cs` with two internal mock classes:
   - `MockPopupContainer : IPopupContainer` — records `CallLog` (ordered list of `"show:PopupId"` / `"hide:PopupId"` strings), returns `UniTask.CompletedTask` from both methods. Mirrors `MockSceneLoader` pattern exactly.
   - `MockInputBlocker : IInputBlocker` — reference-counted: `Block()` increments `BlockCount`, `Unblock()` decrements (floor 0), `IsBlocked => BlockCount > 0`. Exposes `BlockCount` and `BlockCallCount`/`UnblockCallCount` for assertion granularity.

2. Write `PopupManagerTests` fixture with `[SetUp]` creating `MockPopupContainer`, `MockInputBlocker`, and `PopupManager`:
   - **Test 1:** `ShowPopupAsync_PushesPopupOntoStack` — show ConfirmDialog, assert TopPopup == ConfirmDialog and PopupCount == 1
   - **Test 2:** `ShowPopupAsync_CallsContainerShowPopup` — show, verify mock CallLog contains "show:ConfirmDialog"
   - **Test 3:** `ShowPopupAsync_BlocksInput` — show, verify MockInputBlocker.BlockCallCount == 1
   - **Test 4:** `DismissPopupAsync_PopsTopPopup` — show then dismiss, verify TopPopup is null and PopupCount == 0
   - **Test 5:** `DismissPopupAsync_CallsContainerHidePopup` — show then dismiss, verify CallLog contains "hide:ConfirmDialog"
   - **Test 6:** `DismissPopupAsync_UnblocksInputWhenStackEmpty` — show then dismiss, verify IsBlocked == false
   - **Test 7:** `DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain` — show two, dismiss one, verify IsBlocked == true
   - **Test 8:** `DismissPopupAsync_WithEmptyStack_IsNoOp` — dismiss on fresh manager, verify no exceptions and empty CallLog
   - **Test 9:** `DismissAllAsync_ClearsEntireStack` — show two, dismiss all, verify PopupCount == 0 and IsBlocked == false, CallLog has both hide entries
   - **Test 10:** `HasActivePopup_ReflectsStackState` — false initially, true after show, false after dismiss
   - **Test 11:** `ShowPopupAsync_GuardsAgainstConcurrentOperation` — mirrors ScreenManagerTests concurrent guard pattern
   - **Test 12:** `InputBlocker_NestedBlockUnblock` — block twice, unblock once: still blocked. Unblock again: unblocked.
   - **Test 13:** `InputBlocker_BlockUnblockBlock_Sequence` — block, unblock, block: blocked with count 1

3. Use `.Forget()` pattern on all UniTask returns (matching ScreenManagerTests convention — mocks return `UniTask.CompletedTask` so synchronous execution is safe).

4. Run Unity batchmode test execution (without `-quit` per Decision #7) and verify `TestResults.xml` shows all tests passed.

5. Verify total test count is ≥27 (14 existing + 13 new) with 0 failures.

## Must-Haves

- [ ] `MockPopupContainer` records ordered `CallLog` and returns `UniTask.CompletedTask`
- [ ] `MockInputBlocker` uses reference counting with exposed `BlockCount`, `BlockCallCount`, `UnblockCallCount`
- [ ] All 13 test methods exist and pass
- [ ] Existing 14 tests (MVPWiringTests + ScreenManagerTests) still pass
- [ ] `TestResults.xml` shows `failed="0"` with ≥27 total tests

## Verification

- Unity batchmode test run: `TestResults.xml` shows `result="Passed"`, `failed="0"`, `total` ≥ 27
- All 13 new PopupManager test names appear in results as passed
- All 14 existing tests still pass (regression check)

## Inputs

- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — the class under test (from T01)
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — interface to mock (from T01)
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — interface to mock (from T01)
- `Assets/Scripts/Core/PopupManagement/PopupId.cs` — enum used in test calls (from T01)
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — MockSceneLoader pattern template, `.Forget()` convention, test structure
- `Assets/Tests/EditMode/MVPWiringTests.cs` — existing tests that must not regress

## Observability Impact

- **What changes:** `TestResults.xml` at project root gains 13 new `<test-case>` entries under `PopupManagerTests`; all show `result="Passed"`. The total test count moves from 14 → 27+.
- **How a future agent inspects this task:** Run `grep -c "result=\"Passed\"" TestResults.xml` to confirm pass count, or `grep "PopupManagerTests" TestResults.xml` to see the 13 new test cases individually. `failed="0"` in the root `<test-suite>` element is the definitive green signal.
- **What failure looks like:** Any `result="Failed"` or `result="Error"` inside a `PopupManagerTests` test case. A compilation error will instead produce no `<test-case>` entries at all and Unity will exit non-zero.
- **Mock state inspection during test runs:** `MockPopupContainer.CallLog` captures the ordered sequence of `show:`/`hide:` calls — print it in `Assert.AreEqual` messages for readable failure output. `MockInputBlocker.BlockCount`, `.BlockCallCount`, and `.UnblockCallCount` expose the exact reference-counted state at any assertion point.

## Expected Output

- `Assets/Tests/EditMode/PopupManagerTests.cs` — 2 mock classes + 13 test methods, all passing
- `TestResults.xml` — updated with ≥27 total tests, 0 failures
