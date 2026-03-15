# S03: Popup System & Input Blocking

**Goal:** PopupManager opens/dismisses stack-based popups over the current screen. InputBlocker prevents interaction when active. Both are pure C# in Core with interfaces for Unity-dependent parts.
**Demo:** Edit-mode tests prove popup stack logic (push, pop, dismiss-all, empty-stack safety, concurrent guard) and reference-counted input blocking — all 13+ tests pass via batchmode.

## Must-Haves

- `PopupManager` plain C# class with `Stack<PopupId>`, constructor-injected `IPopupContainer` + `IInputBlocker`
- `PopupId` enum (mirroring `ScreenId` pattern)
- `IPopupContainer` interface with `ShowPopupAsync`/`HidePopupAsync` returning `UniTask`
- `IInputBlocker` interface with `Block()`/`Unblock()`/`IsBlocked` — contract requires reference-counting implementations
- `IPopupView : IView` marker interface for popup views
- `UnityInputBlocker` MonoBehaviour runtime implementation with CanvasGroup reference-counting
- `PopupManagerTests.cs` with ≥13 edit-mode tests proving stack logic, input blocking integration, and edge cases
- No static state fields (grep guard passes)
- No `using UnityEngine` in any Core file
- All existing 14 tests still pass alongside new tests

## Proof Level

- This slice proves: contract (popup stack logic + input blocker interface contract verified by edit-mode tests with mocks)
- Real runtime required: no (runtime CanvasGroup behavior verified manually in S05)
- Human/UAT required: no (deferred to S05 integration)

## Verification

- `PopupManagerTests.cs` — 13+ edit-mode NUnit tests with MockPopupContainer and MockInputBlocker
- Batchmode test run: all tests pass (existing 14 + new 13+ = 27+), `TestResults.xml` shows `failed="0"`
- Static guard: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing
- No `using UnityEngine` in Core: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` returns nothing
- Compilation clean: Unity batchmode exits 0, zero `error CS` hits

## Observability / Diagnostics

- Runtime signals: `PopupManager.TopPopup`, `.PopupCount`, `.HasActivePopup` expose stack state; `IInputBlocker.IsBlocked` exposes block state
- Inspection surfaces: `TestResults.xml` at project root for test results; `MockPopupContainer.CallLog` / `MockInputBlocker.BlockCount` in tests for call sequence verification
- Failure visibility: test names map directly to behaviors — a failing test name tells you exactly which contract is broken
- Redaction constraints: none

## Integration Closure

- Upstream surfaces consumed: `IView` (Core/MVP), `Presenter<TView>` (Core/MVP), UniTask (package)
- New wiring introduced in this slice: none (PopupManager and InputBlocker are constructed but not wired into boot flow — that's S05)
- What remains before the milestone is truly usable end-to-end: S04 (transitions), S05 (boot flow wiring, demo screens, play-mode walkthrough)

## Tasks

- [x] **T01: Implement PopupManager, InputBlocker interfaces, and UnityInputBlocker** `est:30m`
  - Why: Creates all production types for S03 — the popup stack logic, both abstractions (IPopupContainer, IInputBlocker), the popup view marker interface, PopupId enum, and the runtime input blocker implementation. Without these, there's nothing to test.
  - Files: `Assets/Scripts/Core/PopupManagement/PopupId.cs`, `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs`, `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs`, `Assets/Scripts/Core/PopupManagement/PopupManager.cs`, `Assets/Scripts/Core/MVP/IPopupView.cs`, `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs`
  - Do: Create PopupId enum with `ConfirmDialog` value. Create IPopupContainer with ShowPopupAsync/HidePopupAsync returning UniTask with CancellationToken. Create IInputBlocker with Block()/Unblock()/IsBlocked. Create PopupManager following ScreenManager's pattern — constructor injection, `_isOperating` guard, try/finally, Block on show, Unblock when stack empties on dismiss. Create IPopupView extending IView. Create UnityInputBlocker MonoBehaviour with reference-counted `_blockCount` and CanvasGroup `blocksRaycasts` toggle. No `using UnityEngine` in Core files. No static state.
  - Verify: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` returns nothing. `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing. Unity batchmode compilation exits 0.
  - Done when: All 6 files exist, compile cleanly, follow established patterns, and pass static analysis checks.

- [x] **T02: Add PopupManager edit-mode tests with mocks** `est:30m`
  - Why: Proves the popup stack contract and input blocker interface work correctly. Validates R011 (stack-based popups), R012 (input blocking), R015 (edit-mode tests), R017 (isolation testing). Without tests, the contract is unverified.
  - Files: `Assets/Tests/EditMode/PopupManagerTests.cs`
  - Do: Create MockPopupContainer (records CallLog like MockSceneLoader, returns UniTask.CompletedTask) and MockInputBlocker (reference-counted with BlockCount and IsBlocked). Write 13 tests: (1) show pushes to stack, (2) show calls container ShowPopupAsync, (3) show blocks input, (4) dismiss pops top, (5) dismiss calls container HidePopupAsync, (6) dismiss unblocks when stack empty, (7) dismiss keeps blocked when popups remain, (8) dismiss empty stack is no-op, (9) dismiss-all clears stack and unblocks, (10) HasActivePopup reflects state, (11) concurrent operation guard, (12) InputBlocker nested block/unblock, (13) InputBlocker block-unblock-block sequence. Follow `.Forget()` pattern from ScreenManagerTests.
  - Verify: Unity batchmode test run — `TestResults.xml` shows all tests passed (27+ total, 0 failed). Existing 14 tests still pass.
  - Done when: `TestResults.xml` reports `failed="0"` with ≥27 total tests, including all 13 new PopupManager tests passing.

## Files Likely Touched

- `Assets/Scripts/Core/PopupManagement/PopupId.cs`
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs`
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs`
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs`
- `Assets/Scripts/Core/MVP/IPopupView.cs`
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs`
- `Assets/Tests/EditMode/PopupManagerTests.cs`
