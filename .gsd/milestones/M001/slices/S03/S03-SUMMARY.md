---
id: S03
parent: M001
milestone: M001
provides:
  - PopupId enum (Core/PopupManagement)
  - IInputBlocker interface with reference-counting contract (Core/PopupManagement)
  - IPopupContainer interface (Core/PopupManagement)
  - PopupManager class — stack-based, concurrency-guarded (Core/PopupManagement)
  - IPopupView marker interface (Core/MVP)
  - UnityInputBlocker MonoBehaviour with CanvasGroup reference-counting (Runtime/PopupManagement)
  - 13 NUnit edit-mode tests (Tests/EditMode/PopupManagerTests.cs)
requires:
  - slice: S01
    provides: IView, Presenter<TView>, UIFactory, UniTask
key_files:
  - Assets/Scripts/Core/PopupManagement/PopupId.cs
  - Assets/Scripts/Core/PopupManagement/IInputBlocker.cs
  - Assets/Scripts/Core/PopupManagement/IPopupContainer.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/MVP/IPopupView.cs
  - Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs
  - Assets/Tests/EditMode/PopupManagerTests.cs
key_decisions:
  - DismissAllAsync must call Unblock() per popup (inside the while loop) to satisfy reference-counting contract — not once after the loop
  - MockInputBlocker exposes BlockCount (current depth), BlockCallCount, UnblockCallCount for assertion granularity
  - PopupManager mirrors ScreenManager exactly: constructor injection, _isOperating guard, try/finally reset, Stack<PopupId>
patterns_established:
  - PopupManager mirrors ScreenManager exactly: constructor injection, _isOperating guard, try/finally reset, Stack<PopupId>
  - IInputBlocker reference-counting contract documented in interface XML doc
  - UnityInputBlocker._blockCount is instance field (not static), clamped at 0 via Math.Max
  - MockPopupContainer mirrors MockSceneLoader — CallLog list, UniTask.CompletedTask returns
observability_surfaces:
  - PopupManager.TopPopup / .PopupCount / .HasActivePopup — readable stack state at any time
  - IInputBlocker.IsBlocked — block state
  - TestResults.xml at project root — result="Passed" total="27" failed="0"
  - MockPopupContainer.CallLog — ordered "show:X"/"hide:X" strings
  - MockInputBlocker.BlockCount / .BlockCallCount / .UnblockCallCount — reference-counted state
drill_down_paths:
  - .gsd/milestones/M001/slices/S03/tasks/T01-SUMMARY.md
  - .gsd/milestones/M001/slices/S03/tasks/T02-SUMMARY.md
duration: ~35m total (T01: ~10m, T02: ~25m)
verification_result: passed
completed_at: 2026-03-15
---

# S03: Popup System & Input Blocking

**PopupManager (stack-based, concurrency-guarded), IInputBlocker (reference-counted), UnityInputBlocker (CanvasGroup), and 13 new edit-mode tests — all 27 tests pass in batchmode with zero static state.**

## What Happened

T01 created all 6 production files following established S02 patterns. `PopupId.cs` mirrors `ScreenId.cs` (enum with `ConfirmDialog` value). `IInputBlocker.cs` defines a three-member interface with XML doc specifying the reference-counting contract. `IPopupContainer.cs` mirrors `ISceneLoader` — two async methods returning UniTask with CancellationToken. `PopupManager.cs` mirrors `ScreenManager` structurally: constructor injection of `IPopupContainer` + `IInputBlocker`, `Stack<PopupId>`, `_isOperating` bool guard, `try/finally` in all async methods, and three public properties for observability. `IPopupView.cs` is a marker interface extending `IView` with no members. `UnityInputBlocker.cs` is a MonoBehaviour with a `[SerializeField] CanvasGroup`, instance `_blockCount`, `Block()` incrementing + setting `blocksRaycasts=true`, and `Unblock()` decrementing with `Math.Max(0, count-1)`. No `using UnityEngine` anywhere in Core.

T02 created `PopupManagerTests.cs` with two test doubles and 13 test methods. `MockPopupContainer` records ordered `CallLog` entries (`"show:ConfirmDialog"` / `"hide:ConfirmDialog"`) and returns `UniTask.CompletedTask` — mirrors `MockSceneLoader` exactly. `MockInputBlocker` is reference-counted with `BlockCount` (current depth), `BlockCallCount`, and `UnblockCallCount` for assertion granularity.

One implementation bug was discovered during T02: `PopupManager.DismissAllAsync` originally called `_inputBlocker.Unblock()` once after the while loop. With reference counting (each `ShowPopupAsync` calls `Block()` once), dismissing 2 popups with a single `Unblock()` left `BlockCount=1` — `IsBlocked` remained `true`. Fixed by moving `Unblock()` inside the while loop so the count returns to exactly 0 after dismiss-all. This is consistent with `DismissPopupAsync` which calls `Unblock()` only when the stack becomes empty.

First batchmode test run produced a stale assembly (26/27); second run after recompile: 27/27 passed.

## Verification

```
# TestResults.xml — 27/27 passed
result="Passed" total="27" passed="27" failed="0"

# All 13 new PopupManager tests passed:
- ShowPopupAsync_PushesPopupOntoStack              ✓
- ShowPopupAsync_CallsContainerShowPopup           ✓
- ShowPopupAsync_BlocksInput                       ✓
- DismissPopupAsync_PopsTopPopup                   ✓
- DismissPopupAsync_CallsContainerHidePopup        ✓
- DismissPopupAsync_UnblocksInputWhenStackEmpty    ✓
- DismissPopupAsync_KeepsInputBlockedWhenPopupsRemain ✓
- DismissPopupAsync_WithEmptyStack_IsNoOp          ✓
- DismissAllAsync_ClearsEntireStack                ✓
- HasActivePopup_ReflectsStackState                ✓
- ShowPopupAsync_GuardsAgainstConcurrentOperation  ✓
- InputBlocker_NestedBlockUnblock                  ✓
- InputBlocker_BlockUnblockBlock_Sequence          ✓

# 14 existing tests still pass (MVPWiringTests: 6, ScreenManagerTests: 8)

# No using UnityEngine in Core
grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/
→ (no output) ✓

# No static state fields
grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask"
→ (no output) ✓
```

## Requirements Advanced

- R011 (stack-based popup system) — PopupManager with `Stack<PopupId>`, push on show, pop on dismiss, LIFO dismiss-all implemented and tested
- R012 (full-screen raycast input blocker) — `IInputBlocker` interface + `UnityInputBlocker` MonoBehaviour with CanvasGroup reference-counting; contract verified by 3 dedicated tests
- R014 (UniTask) — IPopupContainer uses UniTask+CancellationToken; PopupManager async methods; MockPopupContainer returns UniTask.CompletedTask confirming interface contract
- R015 (edit-mode tests) — 13 new PopupManager tests; total 27/27 pass in batchmode
- R017 (isolation testing) — MockPopupContainer and MockInputBlocker allow full PopupManager testing with zero Unity runtime dependency

## Requirements Validated

- R011 — Stack-based popup push/pop/dismiss-all with input blocking and concurrency guard proven by 13 edit-mode tests; TestResults.xml total="27" passed="27" failed="0"
- R012 — IInputBlocker reference-counting contract proven by `InputBlocker_NestedBlockUnblock` and `InputBlocker_BlockUnblockBlock_Sequence`; integration with PopupManager proven by show/dismiss/dismiss-all tests

## New Requirements Surfaced

none

## Requirements Invalidated or Re-scoped

none

## Deviations

- **`PopupManager.DismissAllAsync` bug fix:** The original T01 implementation called `_inputBlocker.Unblock()` once after the while loop. This violated the reference-counting contract with multiple popups open (2 Block() calls, 1 Unblock() → BlockCount=1 remaining). Fixed in T02 by moving `Unblock()` inside the while loop. This is a correctness fix, not a scope change — the contract was always "IsBlocked=false after DismissAllAsync".

## Known Limitations

- `UnityInputBlocker` CanvasGroup wire-up and runtime behavior (blocksRaycasts toggling) is not tested — deferred to S05 manual walkthrough per slice plan.
- `IPopupContainer` has no real Unity implementation yet — `UnityInputBlocker` handles input blocking but popup show/hide animation is deferred to S05.
- `PopupManager` is not wired into boot flow — that's S05.

## Follow-ups

- S05 must wire `UnityInputBlocker` (with a CanvasGroup reference from the persistent scene) and implement a concrete `IPopupContainer` (or add popup show/hide to the demo scene setup)
- S05 must register `ConfirmDialog` scene in EditorBuildSettings if popups need their own scenes, or implement a single-scene overlay pattern for the persistent scene popup layer

## Files Created/Modified

- `Assets/Scripts/Core/PopupManagement/PopupId.cs` — PopupId enum with ConfirmDialog member
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — input blocker interface with reference-counting contract doc
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — popup container async interface
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — stack-based popup manager (modified in T02: DismissAllAsync reference-counting fix)
- `Assets/Scripts/Core/MVP/IPopupView.cs` — IPopupView : IView marker interface
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — MonoBehaviour with CanvasGroup reference-counting
- `Assets/Tests/EditMode/PopupManagerTests.cs` — 13 edit-mode tests with MockPopupContainer and MockInputBlocker
- `TestResults.xml` — updated: 27 total, 27 passed, 0 failed

## Forward Intelligence

### What the next slice should know
- `PopupManager` is not wired anywhere yet — S05 needs to construct it with a real `IPopupContainer` and the `UnityInputBlocker` instance. The natural place is in the boot flow initializer alongside `ScreenManager`.
- `UnityInputBlocker` needs a `CanvasGroup` assigned in the Inspector (or via `GetComponent` in `Awake`). It will NullRef immediately on first `Block()` if the reference is missing — designed to fail loudly at wire-up time.
- The `IPopupContainer` interface is abstract — S05 needs to decide whether popups are prefab-based (instantiate/destroy) or pre-instantiated in the persistent scene (show/hide). Neither approach is committed to yet.
- `DismissAllAsync` calls `Unblock()` once per popup (inside the loop) — this is important to understand when writing S05 integration tests. Each `ShowPopupAsync` increments the block count by 1; each dismiss decrements by 1.

### What's fragile
- `_isOperating` guard is synchronous — with async mocks completing in the same frame, the guard is only exercised in the sequential sense, not truly concurrent. True concurrent guard behavior requires an async mock that doesn't complete immediately. Sufficient for this architecture but worth noting.
- `UnityInputBlocker` has no null check on `_canvasGroup` — intentional design to fail loudly at wire-up, but a missing reference in the Inspector will cause a NullReferenceException the moment any popup is shown.

### Authoritative diagnostics
- `TestResults.xml` at project root — single source of truth for test pass/fail state
- `MockInputBlocker.BlockCount` at assertion point — tells you the exact reference-count depth, not just IsBlocked boolean
- `MockPopupContainer.CallLog` — ordered list reveals whether show/hide were called in correct sequence and for the correct PopupId

### What assumptions changed
- Original T01 assumption: `DismissAllAsync` could call `Unblock()` once at the end. Actual requirement: reference counting requires one `Unblock()` per `ShowPopupAsync` call (i.e., per popup). Fixed before T02 tests ran.
