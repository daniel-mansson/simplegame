---
id: T01
parent: S03
milestone: M001
provides:
  - PopupId enum (Core/PopupManagement)
  - IInputBlocker interface (Core/PopupManagement)
  - IPopupContainer interface (Core/PopupManagement)
  - PopupManager class (Core/PopupManagement)
  - IPopupView marker interface (Core/MVP)
  - UnityInputBlocker MonoBehaviour (Runtime/PopupManagement)
key_files:
  - Assets/Scripts/Core/PopupManagement/PopupId.cs
  - Assets/Scripts/Core/PopupManagement/IInputBlocker.cs
  - Assets/Scripts/Core/PopupManagement/IPopupContainer.cs
  - Assets/Scripts/Core/PopupManagement/PopupManager.cs
  - Assets/Scripts/Core/MVP/IPopupView.cs
  - Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs
key_decisions:
  - No new decisions — all patterns follow S02 precedents (constructor injection, _isOperating guard, try/finally, Stack<T>)
patterns_established:
  - PopupManager mirrors ScreenManager exactly: constructor injection, _isOperating guard, try/finally reset, Stack<PopupId>
  - IInputBlocker reference-counting contract documented in interface XML doc
  - UnityInputBlocker._blockCount is instance field (not static), clamped at 0 via Math.Max
observability_surfaces:
  - PopupManager.TopPopup / .PopupCount / .HasActivePopup — readable stack state at any time
  - IInputBlocker.IsBlocked — block state, mirrored by UnityInputBlocker._blockCount
duration: ~10m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T01: Implement PopupManager, InputBlocker interfaces, and UnityInputBlocker

**Created 6 production files (PopupId, IInputBlocker, IPopupContainer, PopupManager, IPopupView, UnityInputBlocker) following established S02 patterns; all pass static analysis and Unity batchmode compilation.**

## What Happened

Read ScreenManager, ISceneLoader, ScreenId, IView, and UnitySceneLoader as reference patterns, then created all 6 files in order:

1. `PopupId.cs` — enum with `ConfirmDialog`, mirrors `ScreenId.cs` exactly
2. `IInputBlocker.cs` — three-member interface with XML doc specifying reference-counting contract; no UnityEngine
3. `IPopupContainer.cs` — two-method async interface mirroring ISceneLoader; no UnityEngine
4. `PopupManager.cs` — constructor injection of IPopupContainer + IInputBlocker; Stack<PopupId> _stack; _isOperating bool guard; ShowPopupAsync (block → show → push), DismissPopupAsync (pop → hide → unblock if empty), DismissAllAsync (pop all → hide each → unblock); all try/finally; TopPopup/PopupCount/HasActivePopup properties; no UnityEngine
5. `IPopupView.cs` — marker interface extending IView; no members; no UnityEngine
6. `UnityInputBlocker.cs` — MonoBehaviour implementing IInputBlocker; [SerializeField] CanvasGroup; instance _blockCount; Block() increments + sets blocksRaycasts=true; Unblock() Math.Max(0,count-1) + sets blocksRaycasts=count>0; IsBlocked expression body

## Verification

Static analysis — no UnityEngine in Core:
```
grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/
# → no output (exit 1 = no matches) ✓
```

Static analysis — no static state fields:
```
grep -r "static " --include="*.cs" Assets/ | grep -v "static void|static class|static readonly|static async|static UniTask"
# → no output (exit 1 = no matches) ✓
```

Unity batchmode compilation (Unity 6000.3.4f1):
- Exit code: 0 ✓
- `grep -c "error CS" unity_compile.log` → 0 ✓

## Diagnostics

- `PopupManager.TopPopup` / `.PopupCount` / `.HasActivePopup` expose stack state for inspection in tests and runtime debugging
- `IInputBlocker.IsBlocked` / `UnityInputBlocker._blockCount` expose block depth
- `UnityInputBlocker` with null `_canvasGroup` NullRefs immediately on first `Block()` — caught at wire-up time (S05)
- T02 will use `MockPopupContainer.CallLog` and `MockInputBlocker.BlockCount` to assert call sequences

## Deviations

none

## Known Issues

none

## Files Created/Modified

- `Assets/Scripts/Core/PopupManagement/PopupId.cs` — PopupId enum with ConfirmDialog member
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — input blocker interface with reference-counting contract doc
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — popup container async interface
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — stack-based popup manager with guard and try/finally
- `Assets/Scripts/Core/MVP/IPopupView.cs` — IPopupView : IView marker interface
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — MonoBehaviour with CanvasGroup reference-counting
- `.gsd/milestones/M001/slices/S03/tasks/T01-PLAN.md` — added missing Observability Impact section (pre-flight fix)
