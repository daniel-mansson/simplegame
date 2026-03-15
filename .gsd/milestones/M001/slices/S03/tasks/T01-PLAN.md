---
estimated_steps: 8
estimated_files: 6
---

# T01: Implement PopupManager, InputBlocker interfaces, and UnityInputBlocker

**Slice:** S03 — Popup System & Input Blocking
**Milestone:** M001

## Description

Create all production types for the popup system and input blocking: the `PopupId` enum, `IPopupContainer` and `IInputBlocker` interfaces, `PopupManager` class with stack-based logic, `IPopupView` marker interface, and `UnityInputBlocker` MonoBehaviour. Follows the established patterns from S01 (MVP types) and S02 (ScreenManager + ISceneLoader).

## Steps

1. Create `Assets/Scripts/Core/PopupManagement/PopupId.cs` — enum with `ConfirmDialog` member, in `SimpleGame.Core.PopupManagement` namespace. Mirrors `ScreenId.cs` structure exactly.

2. Create `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — interface with `void Block()`, `void Unblock()`, `bool IsBlocked { get; }`. Document that implementations must use reference counting (Block increments, Unblock decrements, IsBlocked = count > 0). No `using UnityEngine`.

3. Create `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — interface with `UniTask ShowPopupAsync(PopupId, CancellationToken)` and `UniTask HidePopupAsync(PopupId, CancellationToken)`. Mirrors `ISceneLoader` structure. Uses `Cysharp.Threading.Tasks` and `System.Threading`. No `using UnityEngine`.

4. Create `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — plain C# class following `ScreenManager` pattern:
   - Constructor injects `IPopupContainer` + `IInputBlocker`
   - `Stack<PopupId> _stack`, `bool _isOperating` guard
   - `TopPopup`, `PopupCount`, `HasActivePopup` read-only properties
   - `ShowPopupAsync`: guard → block input → await container show → push stack (in try/finally)
   - `DismissPopupAsync`: guard + empty check → pop → await container hide → unblock if empty (in try/finally)
   - `DismissAllAsync`: guard + empty check → pop all with container hide → unblock (in try/finally)
   - No `using UnityEngine`

5. Create `Assets/Scripts/Core/MVP/IPopupView.cs` — `interface IPopupView : IView { }` marker interface. In `SimpleGame.Core.MVP` namespace alongside `IView.cs`. No members, no `using UnityEngine`.

6. Create `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — MonoBehaviour implementing `IInputBlocker`:
   - `[SerializeField] CanvasGroup _canvasGroup`
   - `int _blockCount` instance field (NOT static)
   - `Block()`: `_blockCount++`, set `blocksRaycasts = true`
   - `Unblock()`: `_blockCount = Math.Max(0, _blockCount - 1)`, set `blocksRaycasts = _blockCount > 0`
   - `IsBlocked => _blockCount > 0`
   - Uses `UnityEngine` (this is Runtime, not Core)

7. Run static analysis checks:
   - `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` — must return nothing
   - `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` — must return nothing

8. Run Unity batchmode compilation to verify all files compile cleanly — exit code 0, zero `error CS` hits.

## Must-Haves

- [ ] `PopupId` enum exists in `Core/PopupManagement/` with at least `ConfirmDialog`
- [ ] `IInputBlocker` interface in Core — `Block()`, `Unblock()`, `IsBlocked`; no Unity types
- [ ] `IPopupContainer` interface in Core — `ShowPopupAsync`, `HidePopupAsync` returning `UniTask`; no Unity types
- [ ] `PopupManager` in Core — constructor injection, stack, `_isOperating` guard, try/finally, block/unblock on show/dismiss
- [ ] `IPopupView : IView` marker in `Core/MVP/`
- [ ] `UnityInputBlocker` in Runtime — MonoBehaviour with CanvasGroup, reference-counted `_blockCount`
- [ ] No `using UnityEngine` in any Core file
- [ ] No static state fields (grep guard passes)
- [ ] Compiles in Unity batchmode

## Verification

- `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` returns nothing
- `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing
- Unity batchmode compilation exits 0 with zero `error CS` hits

## Inputs

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — primary architectural pattern (constructor injection, async guard, try/finally, stack)
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — interface template (UniTask, CancellationToken, no UnityEngine)
- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — enum template
- `Assets/Scripts/Core/MVP/IView.cs` — base interface that IPopupView extends
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — Runtime MonoBehaviour implementation template
- S03 Research design sketch — PopupManager API shape, IInputBlocker contract, reference-counting pattern

## Observability Impact

**Signals changed:**
- `PopupManager.TopPopup` — returns the top-of-stack `PopupId?`; null when no popup is open
- `PopupManager.PopupCount` — integer depth of the popup stack
- `PopupManager.HasActivePopup` — boolean shortcut for `PopupCount > 0`
- `IInputBlocker.IsBlocked` — true when any Block() is unmatched; `UnityInputBlocker._blockCount` mirrors this on the runtime side

**How to inspect in a future task:**
- Instantiate `PopupManager` with `MockPopupContainer` + `MockInputBlocker` in edit-mode tests and assert on the above properties
- In a running scene, add a watch on `UnityInputBlocker._blockCount` in the Unity inspector or via `Debug.Log` in the MonoBehaviour

**Failure visibility:**
- A `PopupManager` stuck with `_isOperating = true` (due to an unhandled exception in a container) will silently no-op all subsequent calls — the try/finally resets the flag, so this should not occur in normal operation; if it does, `HasActivePopup` will be stale relative to visual state
- `UnityInputBlocker` with a null `_canvasGroup` will throw a NullReferenceException on first `Block()` — caught immediately at wire-up time

## Expected Output

- `Assets/Scripts/Core/PopupManagement/PopupId.cs` — PopupId enum
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — input blocker interface
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — popup container interface
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — popup stack manager
- `Assets/Scripts/Core/MVP/IPopupView.cs` — popup view marker interface
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — Unity input blocker implementation
