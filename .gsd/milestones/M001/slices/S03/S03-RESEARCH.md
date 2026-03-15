# S03: Popup System & Input Blocking — Research

**Date:** 2026-03-15

## Summary

S03 delivers two related systems: **PopupManager** (a stack-based popup controller) and **InputBlocker** (a full-screen raycast blocker). Both are consumed by S04 (transitions) and S05 (boot/demo flow). The core design challenge is identical to S02's: keep navigation/popup logic testable in edit-mode as pure C# while the visual/input-blocking implementation requires Unity uGUI APIs. The solution mirrors S02 exactly — abstract the Unity-dependent parts behind interfaces (`IInputBlocker`), test the logic in Core with mocks, and place the real Canvas/CanvasGroup implementation in Runtime.

PopupManager is structurally simpler than ScreenManager because popups don't involve scene loading. Popups in this architecture are GameObjects instantiated or activated within the persistent scene's popup layer — they're not additively loaded scenes. This means PopupManager doesn't need an `ISceneLoader`-style abstraction; instead it needs a way to show/hide popup GameObjects and track a stack of what's open. The logic (stack, push, pop, "is anything open?") is trivially pure C#. The Unity-specific part — actually activating/deactivating popup GameObjects, managing sort order, and CanvasGroup manipulation — lives behind an `IPopupView`-style interface or a popup container abstraction.

InputBlocker is primarily a Unity concern (a Canvas with a CanvasGroup whose `blocksRaycasts` is toggled). The core interface is simple: `Block()` and `Unblock()` (or `SetBlocked(bool)`). The implementation is a MonoBehaviour on a high-sort-order full-screen Canvas in the persistent scene. Since S04 and the PopupManager both consume InputBlocker, defining the interface now ensures the contract is ready. The interface lives in Core; the MonoBehaviour implementation in Runtime.

## Recommendation

**Approach: PopupManager as pure C# class + IInputBlocker interface with Unity implementation**

### PopupManager

1. Define `PopupId` enum (mirroring `ScreenId` pattern) — e.g., `ConfirmDialog`, `SettingsPopup` — extensible as new popups are added
2. `PopupManager` is a plain C# class with a `Stack<PopupId>` tracking open popups
3. `ShowPopupAsync(PopupId)` pushes to stack; `DismissPopupAsync()` pops; `DismissAllAsync()` clears
4. PopupManager does NOT own popup GameObject lifecycle (parallels Decision #9 where ScreenManager doesn't own presenters). The mapping from PopupId to actual popup view instances happens in S05 boot wiring — PopupManager manages the logical stack only.
5. However, PopupManager needs a way to signal show/hide — use an `IPopupContainer` interface with `ShowPopupAsync(PopupId)` and `HidePopupAsync(PopupId)` returning `UniTask`. This mirrors `ISceneLoader` for ScreenManager. The runtime implementation activates/deactivates popup GameObjects in the persistent scene's popup layer.
6. PopupManager receives `IPopupContainer` and `IInputBlocker` via constructor injection. On show: block input → show popup → unblock input (the popup itself handles focus). On dismiss: hide popup → if stack empty, unblock input.

**Why a container abstraction:** Without it, PopupManager would need to directly reference Unity GameObjects, making it untestable in edit-mode. The `IPopupContainer` interface makes the same edit-mode testability pattern that `ISceneLoader` provides for ScreenManager.

### InputBlocker

1. Define `IInputBlocker` interface in Core with `void Block()` and `void Unblock()` (synchronous — toggling `blocksRaycasts` is instant, no async needed)
2. Runtime implementation: `UnityInputBlocker` MonoBehaviour on a Canvas with sort order higher than screen canvases but lower than popup canvases. Uses a `CanvasGroup` with `blocksRaycasts` toggled on `Block()`/`Unblock()`.
3. Reference-counting: Both PopupManager (during popup animation) and TransitionManager (S04, during fades) call Block/Unblock independently. A simple int counter ensures the blocker only actually unblocks when all callers have released: `Block()` increments, `Unblock()` decrements, `blocksRaycasts = _count > 0`.

**Why reference counting:** Without it, if PopupManager calls `Unblock()` after showing a popup but TransitionManager is still mid-fade, the blocker would incorrectly release. The counter is a standard pattern for shared blocking resources and costs zero complexity (two lines of logic). This should be reflected in the `IInputBlocker` contract so tests can verify nesting behavior.

### PopupView Interface

1. Define `IPopupView : IView` — the base interface for all popup views
2. Each concrete popup gets its own interface (e.g., `IConfirmDialogView : IPopupView`) with specific events (`OnConfirm`, `OnCancel`) and update methods
3. Popup presenters follow the `Presenter<TView>` pattern exactly as established in S01

## Don't Hand-Roll

| Problem | Existing Solution | Why Use It |
|---------|------------------|------------|
| Async popup show/hide | UniTask (already installed) | `ShowPopupAsync` / `DismissPopupAsync` return `UniTask` for animation sequencing in S05 |
| Test framework | NUnit via `com.unity.test-framework` 1.6.0 (already installed) | 14 existing tests establish the pattern; new tests follow `ScreenManagerTests.cs` template |
| Mock/test double pattern | `MockSceneLoader` / `MockSampleView` (already in codebase) | `MockPopupContainer` and `MockInputBlocker` follow the exact same pattern |

## Existing Code and Patterns

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — **Primary pattern to follow.** PopupManager mirrors ScreenManager's architecture: plain C# class, constructor injection of an abstraction (`IPopupContainer` like `ISceneLoader`), stack-based navigation, async methods returning `UniTask`, `_isNavigating` guard for concurrent calls
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — **Template for `IPopupContainer`** and `IInputBlocker`. Pure interface, no `using UnityEngine`, returns `UniTask`, takes `CancellationToken`
- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — **Template for `PopupId` enum.** Same pattern: simple enum, `ToString()` used directly (no static helper per Decision #11)
- `Assets/Scripts/Core/MVP/IView.cs` — `IPopupView` should extend this marker interface
- `Assets/Scripts/Core/MVP/Presenter.cs` — Popup presenters extend `Presenter<TView>` with two-phase lifecycle
- `Assets/Scripts/Core/MVP/ISampleView.cs` — **Template for popup view interfaces.** Events as `event Action`, update methods, no Unity types
- `Assets/Scripts/Core/MVP/UIFactory.cs` — Needs `CreateConfirmDialogPresenter(IConfirmDialogView)` added; receives PopupManager as a dependency at construction
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — **Test pattern template.** `MockSceneLoader` = template for `MockPopupContainer`; `MockSceneLoader.CallLog` ordered list pattern is reusable for verifying show/hide sequences
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — **Template for `UnityInputBlocker`.** Runtime implementation wrapping Unity APIs, placed in `Assets/Scripts/Runtime/` namespace

## Constraints

- **No static state (R006)** — PopupManager and InputBlocker cannot use static fields or singleton patterns. Both must be instantiated and injected.
- **No Unity types in Core interfaces (Decision #3)** — `IPopupView`, `IInputBlocker`, `IPopupContainer` use `event Action` and `UniTask`, not `UnityEvent`, `Canvas`, or `CanvasGroup`
- **Two-phase lifecycle (Decision #4)** — Popup presenters use constructor for injection, `Initialize()` for event subscription
- **View interfaces have no backward references (R002)** — Popup view interfaces must not reference PopupManager, presenters, or services
- **Assembly references use string names (Decision #2)** — No GUID references in asmdef files
- **No `-quit` with `-runTests` (Decision #7)** — Test run commands must omit `-quit` flag
- **`autoReferenced: true` on `SimpleGame.Runtime.asmdef`** — All new Core and Runtime code compiles into the same assembly; no new asmdef needed
- **Static-state grep guard** — `grep -r "static " --include="*.cs" | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` must return nothing. Avoid `static string` or `static int` fields. Reference-counting in `IInputBlocker` implementation uses instance fields, not static.
- **PopupManager does not own presenter lifecycle (parallels Decision #9)** — PopupManager manages the logical stack. Presenter construction/disposal is deferred to S05 boot wiring. This keeps S03 focused on stack logic, independently testable.
- **Popup views live in the persistent scene's popup layer** — They are NOT in screen scenes and NOT additively loaded. They are GameObjects in the persistent scene, activated/deactivated. This is fundamentally different from ScreenManager's scene-loading approach.

## Common Pitfalls

- **InputBlocker race between PopupManager and TransitionManager** — If Block/Unblock is a simple bool toggle, one system's Unblock can release the block while the other still needs it. Use reference counting (`_blockCount++` / `_blockCount--`, `blocksRaycasts = _blockCount > 0`). Test the nesting case explicitly.
- **Popup sort order conflicts** — Popups stack visually on top of each other. Each new popup needs a higher sort order than the previous. The `IPopupContainer` implementation (Runtime) should manage Canvas sort order incrementally. Don't hardcode sort orders.
- **Forgetting to unblock input on dismiss** — If `DismissPopupAsync` throws mid-operation, the input blocker stays blocked forever. Use `try/finally` on Block/Unblock calls (same pattern as ScreenManager's `_isNavigating` guard in `finally` blocks).
- **Testing with `.Forget()` on UniTask** — The existing tests call `.Forget()` on UniTask returns for synchronous mock execution. This works because `MockSceneLoader` returns `UniTask.CompletedTask`. The new `MockPopupContainer` must also return `UniTask.CompletedTask` for the same reason. If it returns a genuinely async task, `.Forget()` won't block and the test assertions run before the operation completes.
- **PopupManager dismissing when stack is empty** — `DismissPopupAsync()` with an empty stack must be a safe no-op (matching `GoBackAsync` pattern). Test this case explicitly.
- **Concurrent show/dismiss calls** — Same risk as ScreenManager's double-navigation. Use an `_isOperating` guard flag with `try/finally` cleanup, identical to ScreenManager's `_isNavigating` pattern.
- **Popup container receiving unknown PopupId** — The runtime `IPopupContainer` implementation needs to handle requests for popups it doesn't know about. Either throw (fail-fast) or log a warning. Define behavior in the interface contract or documentation.

## Open Risks

- **InputBlocker reference counting in the interface vs implementation** — The `IInputBlocker` interface exposes `Block()`/`Unblock()` but doesn't enforce that implementations use reference counting. A naive implementation could break when S04's TransitionManager also calls Block/Unblock. Mitigate by: (1) documenting the reference-counting requirement in the interface, and (2) testing nested Block/Unblock calls against `MockInputBlocker` that also uses counting.
- **Popup container instantiation vs activation** — Are popup views pre-instantiated in the persistent scene (just toggled active/inactive)? Or are they instantiated on-demand from prefabs? Pre-instantiation is simpler for S03 scope but scales poorly. Instantiation-on-demand is more flexible but requires a prefab registry or addressable assets. **Recommendation: pre-instantiation for S03/S05 demo scope** — the persistent scene has the popup GameObjects already placed, just deactivated. The `IPopupContainer` implementation toggles `SetActive`. Future milestones can replace with prefab instantiation behind the same interface.
- **PopupManager + InputBlocker interaction pattern** — Should PopupManager call `IInputBlocker.Block()` when showing a popup? Or should that be the caller's responsibility (S05 boot flow)? If PopupManager owns the block/unblock lifecycle for popups, the logic is self-contained and testable. If it doesn't, the caller must remember to block. **Recommendation: PopupManager calls Block on show, Unblock on dismiss-when-stack-empty.** This keeps the blocking behavior co-located with the stack logic.
- **No play-mode test for CanvasGroup raycast blocking** — The actual raycasting behavior can only be verified in play mode. Edit-mode tests verify the logical stack and the mock's Block/Unblock call sequence, but don't prove that clicks are actually blocked. This is acceptable per R019 (play-mode tests deferred) — manual play-mode walkthrough in S05 covers this.

## Folder Structure (Proposed)

```
Assets/Scripts/Core/
├── MVP/                    (existing)
├── Services/               (existing)
├── ScreenManagement/        (existing — from S02)
└── PopupManagement/         (new — S03)
    ├── PopupId.cs           (enum: ConfirmDialog, etc.)
    ├── PopupManager.cs      (pure C# — stack, show/dismiss, guards)
    ├── IPopupContainer.cs   (interface: ShowPopupAsync/HidePopupAsync)
    └── IInputBlocker.cs     (interface: Block/Unblock)

Assets/Scripts/Runtime/
├── ScreenManagement/        (existing — UnitySceneLoader)
└── PopupManagement/         (new — S03)
    └── UnityInputBlocker.cs (MonoBehaviour — Canvas/CanvasGroup wrapper)

Assets/Tests/EditMode/
├── MVPWiringTests.cs        (existing — 6 tests)
├── ScreenManagerTests.cs    (existing — 8 tests)
└── PopupManagerTests.cs     (new — S03)
```

**Note:** `IPopupView : IView` goes in `Assets/Scripts/Core/MVP/` alongside `IView.cs` since it's an MVP infrastructure type, not popup-management-specific. Concrete popup view interfaces (e.g., `IConfirmDialogView`) will be added in S05 when demo popups are built.

## Design Sketch

### PopupManager

```csharp
// Core/PopupManagement/PopupManager.cs
public class PopupManager
{
    private readonly IPopupContainer _container;
    private readonly IInputBlocker _inputBlocker;
    private readonly Stack<PopupId> _stack = new();
    private bool _isOperating;

    public PopupId? TopPopup => _stack.Count > 0 ? _stack.Peek() : null;
    public int PopupCount => _stack.Count;
    public bool HasActivePopup => _stack.Count > 0;

    public PopupManager(IPopupContainer container, IInputBlocker inputBlocker) { ... }

    public async UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default)
    {
        if (_isOperating) return;
        _isOperating = true;
        try
        {
            _inputBlocker.Block();
            await _container.ShowPopupAsync(popupId, ct);
            _stack.Push(popupId);
        }
        finally { _isOperating = false; }
    }

    public async UniTask DismissPopupAsync(CancellationToken ct = default)
    {
        if (_stack.Count == 0 || _isOperating) return;
        _isOperating = true;
        try
        {
            var top = _stack.Pop();
            await _container.HidePopupAsync(top, ct);
            if (_stack.Count == 0)
                _inputBlocker.Unblock();
        }
        finally { _isOperating = false; }
    }

    public async UniTask DismissAllAsync(CancellationToken ct = default)
    {
        if (_stack.Count == 0 || _isOperating) return;
        _isOperating = true;
        try
        {
            while (_stack.Count > 0)
            {
                var top = _stack.Pop();
                await _container.HidePopupAsync(top, ct);
            }
            _inputBlocker.Unblock();
        }
        finally { _isOperating = false; }
    }
}
```

### IInputBlocker

```csharp
// Core/PopupManagement/IInputBlocker.cs
public interface IInputBlocker
{
    void Block();
    void Unblock();
    bool IsBlocked { get; }
}
```

### IPopupContainer

```csharp
// Core/PopupManagement/IPopupContainer.cs
public interface IPopupContainer
{
    UniTask ShowPopupAsync(PopupId popupId, CancellationToken ct = default);
    UniTask HidePopupAsync(PopupId popupId, CancellationToken ct = default);
}
```

## Test Plan (Edit-Mode)

Tests for `PopupManager` using `MockPopupContainer` and `MockInputBlocker`:

1. **ShowPopupAsync pushes popup onto stack** — verify `TopPopup` and `PopupCount` after show
2. **ShowPopupAsync calls container ShowPopupAsync** — verify mock received the correct PopupId
3. **ShowPopupAsync blocks input** — verify `MockInputBlocker.Block()` was called
4. **DismissPopupAsync pops top popup** — verify stack state after dismiss
5. **DismissPopupAsync calls container HidePopupAsync** — verify mock received the correct PopupId
6. **DismissPopupAsync unblocks input when stack empty** — verify Unblock called only when last popup dismissed
7. **DismissPopupAsync keeps input blocked when popups remain** — show two, dismiss one, verify still blocked
8. **DismissPopupAsync with empty stack is no-op** — no exception, no container calls
9. **DismissAllAsync clears entire stack** — verify stack empty, all popups hidden, input unblocked
10. **HasActivePopup reflects stack state** — true when popups exist, false when empty
11. **Concurrent operation guard** — show/dismiss while operating is no-op (mirrors ScreenManager pattern)

Tests for `IInputBlocker` contract (using `MockInputBlocker` with reference counting):

12. **Block increments, Unblock decrements** — verify IsBlocked reflects count > 0
13. **Nested Block/Unblock** — Block twice, Unblock once: still blocked. Unblock again: unblocked.

## Skills Discovered

| Technology | Skill | Status |
|------------|-------|--------|
| Unity uGUI popup system | `zate/cc-godot@godot-ui` | available (390 installs) — Godot-specific, not relevant |
| Unity UI | `creator-hian/claude-code-plugins@unity-ui` | available (4 installs) — very low installs, too general |
| Unity workflows | `cryptorabea/claude_unity_dev_plugin@unity workflows` | available (46 installs) — workflow focus, not architecture |
| Unity architecture | `cryptorabea/claude_unity_dev_plugin@unity architecture` | available (50 installs) — general architecture, not specific enough |
| Unity developer | `rmyndharis/antigravity-skills@unity-developer` | available (568 installs) — most general, previously evaluated in S02 research |

No skills are directly relevant to S03's specific scope (MVP popup stack + input blocking). The project's established patterns (from S01/S02) are more valuable than any generic Unity skill. The `unity-developer` skill (568 installs) remains the closest match but was already evaluated in S02 research and deemed too general.

## Sources

- Unity `CanvasGroup.blocksRaycasts` controls whether the Graphic Raycaster considers this component for raycasting — setting it to `true` on a full-screen overlay absorbs all UI input (source: Unity uGUI API — standard knowledge)
- Unity Canvas `sortingOrder` determines rendering and input order; higher values render on top and receive input first (source: Unity uGUI API — standard knowledge)
- Reference-counting pattern for shared blocking resources is standard in UI frameworks — prevents early release when multiple callers hold a block (source: general software engineering pattern)
- Existing codebase analysis: all 13 C# files, 2 asmdef files, manifest.json, S01 summary, S02 summary examined
- `ISceneLoader` interface pattern from S02 serves as the direct template for `IPopupContainer` and `IInputBlocker` abstractions (source: project codebase `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs`)
- `ScreenManager` stack-based navigation pattern from S02 is the direct template for `PopupManager` stack logic (source: project codebase `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`)
