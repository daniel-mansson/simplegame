---
id: S03
parent: M001
milestone: M001
written: 2026-03-15
---

# S03: Popup System & Input Blocking — UAT

**Milestone:** M001
**Written:** 2026-03-15

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S03 proof level is "contract" — popup stack logic and input blocker interface contract are fully verified by edit-mode tests with mocks. No Unity runtime is required for this slice. Runtime CanvasGroup behavior and popup presentation are deferred to S05 per the slice plan.

## Preconditions

1. Unity 6000.3.4f1 is installed at `C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe`
2. Project is at `C:\OtherWork\simplegame`
3. No pending compilation errors (project must have compiled cleanly after T01 and T02)
4. All S01 and S02 files are present (IView, Presenter<TView>, UIFactory, ScreenManager)

## Smoke Test

Run the edit-mode tests in batchmode and verify all 27 pass with zero failures:

```bash
"C:/Program Files/Unity/Hub/Editor/6000.3.4f1/Editor/Unity.exe" \
  -batchmode -runTests \
  -projectPath "C:/OtherWork/simplegame" \
  -testPlatform editmode \
  -testResults "C:/OtherWork/simplegame/TestResults.xml" \
  -logFile "C:/OtherWork/simplegame/Logs/S03-smoke.log"

grep -E 'result=|failed=|passed=' C:/OtherWork/simplegame/TestResults.xml | head -1
```

**Expected:** `result="Passed" total="27" passed="27" failed="0"`

## Test Cases

### 1. All 13 PopupManager tests pass

Run the batchmode test command (see Smoke Test above) and inspect the XML.

```bash
grep 'PopupManagerTests' C:/OtherWork/simplegame/TestResults.xml | grep 'result="Passed"' | wc -l
```

**Expected:** `13`

Each of the following test names should appear with `result="Passed"` in `TestResults.xml`:
- `ShowPopupAsync_PushesPopupOntoStack`
- `ShowPopupAsync_CallsContainerShowPopup`
- `ShowPopupAsync_BlocksInput`
- `DismissPopupAsync_PopsTopPopup`
- `DismissPopupAsync_CallsContainerHidePopup`
- `DismissPopupAsync_UnblocksInputWhenStackEmpty`
- `DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain`
- `DismissPopupAsync_WithEmptyStack_IsNoOp`
- `DismissAllAsync_ClearsEntireStack`
- `HasActivePopup_ReflectsStackState`
- `ShowPopupAsync_GuardsAgainstConcurrentOperation`
- `InputBlocker_NestedBlockUnblock`
- `InputBlocker_BlockUnblockBlock_Sequence`

### 2. All 14 existing tests (S01 + S02) still pass

From the same `TestResults.xml`, verify the pre-existing suites are intact:

```bash
grep 'MVPWiringTests\|ScreenManagerTests' C:/OtherWork/simplegame/TestResults.xml | grep 'result="Passed"' | wc -l
```

**Expected:** `14` (6 MVPWiringTests + 8 ScreenManagerTests)

### 3. No static state in any C# file

```bash
grep -r "static " --include="*.cs" C:/OtherWork/simplegame/Assets/ \
  | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"
```

**Expected:** No output (command returns exit code 1 = no matches found)

### 4. No UnityEngine coupling in Core layer

```bash
grep -r "using UnityEngine" --include="*.cs" C:/OtherWork/simplegame/Assets/Scripts/Core/
```

**Expected:** No output (command returns exit code 1 = no matches found)

### 5. All 6 S03 production files exist

```bash
ls C:/OtherWork/simplegame/Assets/Scripts/Core/PopupManagement/
ls C:/OtherWork/simplegame/Assets/Scripts/Core/MVP/IPopupView.cs
ls C:/OtherWork/simplegame/Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs
```

**Expected:**
- `Core/PopupManagement/` contains: `PopupId.cs`, `IInputBlocker.cs`, `IPopupContainer.cs`, `PopupManager.cs` (plus `.meta` files)
- `Core/MVP/IPopupView.cs` exists
- `Runtime/PopupManagement/UnityInputBlocker.cs` exists

### 6. Stack push / pop sequence is correct (read test code)

Open `Assets/Tests/EditMode/PopupManagerTests.cs` and verify:

1. `ShowPopupAsync_PushesPopupOntoStack` — calls `ShowPopupAsync(PopupId.ConfirmDialog)`, asserts `TopPopup == ConfirmDialog` and `PopupCount == 1`
2. `DismissPopupAsync_PopsTopPopup` — show then dismiss, asserts `TopPopup == null` and `PopupCount == 0`
3. `DismissAllAsync_ClearsEntireStack` — show twice, dismiss-all, asserts `PopupCount == 0`, `IsBlocked == false`, and `hide` count == 2 in `CallLog`

**Expected:** Logic in each test correctly represents the contract described in the slice plan

### 7. Reference-counting input blocker contract

Inspect `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — the XML doc must specify:
- `Block()` increments the count
- `Unblock()` decrements it (clamped at 0)
- `IsBlocked` returns true when count > 0

Verify the test `InputBlocker_NestedBlockUnblock`:
- 2× `Block()`, 1× `Unblock()` → `IsBlocked == true`, `BlockCount == 1`
- 2nd `Unblock()` → `IsBlocked == false`, `BlockCount == 0`

**Expected:** Both assertions pass in TestResults.xml

### 8. DismissAllAsync reference-counting correctness

Inspect `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — `DismissAllAsync`:

```csharp
while (_stack.Count > 0)
{
    var popupId = _stack.Pop();
    await _container.HidePopupAsync(popupId, ct);
    _inputBlocker.Unblock();   // ← must be inside the loop
}
```

**Expected:** `_inputBlocker.Unblock()` is inside the `while` loop, not after it. This ensures one `Unblock()` per `Block()` call regardless of how many popups were stacked.

## Edge Cases

### Empty stack dismiss is a no-op

Test `DismissPopupAsync_WithEmptyStack_IsNoOp` verifies:
1. Call `DismissPopupAsync()` on a fresh `PopupManager` (no popups shown)
2. **Expected:** No exception thrown; `container.CallLog` is empty; `inputBlocker.UnblockCallCount == 0`

### Input stays blocked when stack is not empty after a dismiss

Test `DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain` verifies:
1. Show two popups (`ShowPopupAsync` twice)
2. Dismiss one (`DismissPopupAsync`)
3. **Expected:** `IsBlocked == true`, `PopupCount == 1` — input stays blocked because one popup remains

### Nested block/unblock stays balanced

Test `InputBlocker_NestedBlockUnblock` verifies that reference counting prevents premature unblocking:
1. `Block()` × 2, `Unblock()` × 1 → still blocked
2. `Unblock()` × 1 → now unblocked
3. **Expected:** `IsBlocked` correctly tracks depth, not a binary toggle

## Failure Signals

- `TestResults.xml` shows `failed > 0` — a test is failing; check the test name to identify which contract is broken
- `grep` for `using UnityEngine` in Core returns output — Core layer has gained a Unity coupling that must be removed
- `grep` for static fields returns output — static state has been introduced; check the file and variable name
- Missing file in `Core/PopupManagement/` — a production file was not created or was deleted
- `DismissAllAsync` has `Unblock()` outside the while loop — the reference-counting bug has been re-introduced; test `DismissAllAsync_ClearsEntireStack` will fail with `IsBlocked=true`

## Requirements Proved By This UAT

- R011 (stack-based popup system) — push/pop/dismiss-all logic proven by tests 1, 4, 9, 10
- R012 (full-screen raycast input blocker) — IInputBlocker reference-counting contract proven by tests 12 and 13; integration with PopupManager proven by tests 3, 6, 7, 9
- R014 (UniTask) — IPopupContainer interface uses UniTask; MockPopupContainer returns UniTask.CompletedTask confirming the contract compiles and runs
- R015 (edit-mode tests) — 13 new tests; 27/27 total pass without entering play mode
- R017 (isolation testing) — MockPopupContainer + MockInputBlocker run with zero Unity runtime dependency

## Not Proven By This UAT

- Runtime CanvasGroup toggle (`blocksRaycasts`) in `UnityInputBlocker` — deferred to S05 play-mode walkthrough
- Actual popup presentation (show/hide animations, prefab instantiation) — no concrete `IPopupContainer` implementation yet
- `PopupManager` wired into boot flow — deferred to S05
- `IPopupView` used by a real view — no popup view MonoBehaviour implemented in this slice

## Notes for Tester

- The `TestResults.xml` at the project root is the canonical artifact for this slice. All verification flows through it.
- The `MockInputBlocker` exposes `BlockCount` (current depth), not just `IsBlocked`. If a test fails, inspect `BlockCount` directly for the reference-count depth — `IsBlocked` alone doesn't tell you how far off you are.
- The `MockPopupContainer.CallLog` entries are strings like `"show:ConfirmDialog"` and `"hide:ConfirmDialog"`. Failure messages in the tests print the full `CallLog` — read it to see the actual call sequence vs expected.
- S03 defers runtime behavior to S05 by design. Do not attempt to wire `UnityInputBlocker` or `PopupManager` into a running scene as part of this UAT — that's the S05 demo walkthrough.
