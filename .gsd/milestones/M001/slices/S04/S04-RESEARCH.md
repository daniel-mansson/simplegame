# S04: Transition System — Research

**Date:** 2026-03-15

## Summary

S04 introduces a fade-to-black transition that plays during screen navigation, blocking input for the duration. The codebase already provides all the building blocks: `ScreenManager` handles scene load/unload (S02), `IInputBlocker` provides reference-counted input blocking (S03), and UniTask supplies zero-allocation async delays suitable for frame-by-frame alpha interpolation.

The primary design question is **where the transition orchestration lives** relative to `ScreenManager`. Two viable approaches exist: (A) inject an optional `ITransitionPlayer` into `ScreenManager` itself, making it call fade-out before unload and fade-in after load, or (B) create a standalone `TransitionManager` that wraps `ScreenManager` calls with transition brackets. Approach (A) is simpler and avoids a wrapper layer, but modifies S02's completed code. Approach (B) adds a new orchestrator without touching ScreenManager, but creates a "navigation-with-transitions" vs "raw navigation" split that S05 must resolve. **Recommendation: Approach (A)** — inject an optional `ITransitionPlayer` into `ScreenManager` so existing `ShowScreenAsync`/`GoBackAsync` transparently gain transitions. The `_isNavigating` guard already prevents concurrent calls, so the transition duration is naturally protected. The injection is optional (null = no transition), preserving all existing test behavior untouched.

The runtime Unity implementation is a `CanvasGroup` on a high-sort-order `Canvas` in the persistent scene, identical to the `UnityInputBlocker` pattern. `CanvasGroup.alpha` is interpolated from 0→1 (fade out) and 1→0 (fade in) using a simple frame loop with `UniTask.Yield()` and `Time.deltaTime`. No DOTween or animation library is needed — the fade is a ~15-line loop.

## Recommendation

**Inject an `ITransitionPlayer` interface into `ScreenManager` (optional dependency, null = no-op).** Orchestration sequence inside `ShowScreenAsync`:

1. `_inputBlocker.Block()` — via IInputBlocker (new optional injection)
2. `await _transitionPlayer.FadeOutAsync(ct)` — darken overlay to opaque
3. Unload current scene (existing code)
4. Load new scene (existing code)
5. `await _transitionPlayer.FadeInAsync(ct)` — overlay back to transparent
6. `_inputBlocker.Unblock()`

This keeps the transition flow atomic within the navigation guard, leverages the existing `_isNavigating` concurrency protection, and matches the R013 spec: "block input → fade out → unload old scene → load new scene → fade in → unblock input."

The `ITransitionPlayer` interface goes in `Core/TransitionManagement/` with two methods: `FadeOutAsync` and `FadeInAsync`. The `UnityTransitionPlayer` MonoBehaviour goes in `Runtime/TransitionManagement/` and wraps a `CanvasGroup`. A `MockTransitionPlayer` enables edit-mode testing of the full orchestration sequence.

**Alternative considered and rejected:** Standalone `TransitionManager` wrapper class. This would duplicate the navigation guard logic, create confusion about which navigation entry point to use, and force S05 boot flow to decide between `ScreenManager.ShowScreenAsync` vs `TransitionManager.NavigateAsync`. Injecting into ScreenManager avoids this entirely.

## Don't Hand-Roll

| Problem | Existing Solution | Why Use It |
|---------|------------------|------------|
| Async timing (fade delay/frame stepping) | `UniTask.Delay()`, `UniTask.Yield()` | Already installed (S01); zero-allocation; CancellationToken built-in; `Time.deltaTime` available at `PlayerLoopTiming.Update` |
| Input blocking during transitions | `IInputBlocker` (S03) | Reference-counted; already wired into PopupManager; adding transition blocking stacks correctly with popup blocking |
| Scene load/unload | `ISceneLoader` via `ScreenManager` (S02) | Already proven; transition wraps around existing calls, not beside them |

## Existing Code and Patterns

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — **Modification target.** Add optional `ITransitionPlayer` and `IInputBlocker` constructor parameters. Existing `ShowScreenAsync`/`GoBackAsync` gain transition brackets when `_transitionPlayer != null`. The `_isNavigating` guard, `try/finally` pattern, and `Stack<ScreenId>` history remain unchanged. All 8 existing tests continue to pass because they inject `null` for the transition player.
- `Assets/Scripts/Core/PopupManagement/IInputBlocker.cs` — **Reuse directly.** TransitionManager calls `Block()`/`Unblock()` using the same reference-counted interface. No changes needed.
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — **Pattern to follow.** MonoBehaviour with `[SerializeField] CanvasGroup`, instance `_blockCount`. The `UnityTransitionPlayer` follows identical structure: MonoBehaviour, `[SerializeField] CanvasGroup`, instance float `_fadeDuration`.
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — **Interface pattern to follow.** Pure C#, no UnityEngine using, two async methods returning `UniTask` with `CancellationToken`. `ITransitionPlayer` follows same shape.
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — **Test pattern to extend.** `MockSceneLoader` pattern shows how test doubles record calls in a `CallLog`. `MockTransitionPlayer` will do the same. Existing tests use `new ScreenManager(mockLoader)` — they'll use `new ScreenManager(mockLoader)` (2-arg overload or null defaults) and remain valid.
- `Assets/Tests/EditMode/PopupManagerTests.cs` — **MockInputBlocker already exists here.** Reuse it in new transition-integrated ScreenManager tests to verify `Block()`/`Unblock()` calls bracket the transition.

## Constraints

- **No static state** — all fields (`_fadeDuration`, `_canvasGroup`, `_transitionPlayer`, `_inputBlocker`) must be instance fields. `const float` for default duration is acceptable (const is compile-time, not static state).
- **No UnityEngine in Core/** — `ITransitionPlayer.cs` must live in `Core/TransitionManagement/` with zero Unity imports. Only `UniTask` + `CancellationToken`.
- **Single `.asmdef` covers Core + Runtime** — `SimpleGame.Runtime.asmdef` at `Assets/Scripts/` root encompasses all subdirectories. No new asmdef needed for `TransitionManagement/`.
- **Existing tests must not break** — `ScreenManager` constructor change must be backward-compatible (either overload or default `null` parameters). All 27 existing tests must pass unchanged.
- **CanvasGroup.alpha range is [0,1]** — Unity clamps this, but the interpolation loop should be explicit about clamping to avoid floating-point drift past boundaries.
- **`_isNavigating` guard covers transitions** — Because FadeOutAsync/FadeInAsync execute inside the existing `ShowScreenAsync`/`GoBackAsync` body (which is guarded by `_isNavigating`), no additional concurrency protection is needed for the transition itself.

## Common Pitfalls

- **Modifying ScreenManager constructor breaks existing test compilation** — The 8 existing `ScreenManagerTests` call `new ScreenManager(mockLoader)`. If the constructor signature changes to require `ITransitionPlayer` and `IInputBlocker`, all tests break. **Avoid by:** using optional parameters with `null` defaults: `ScreenManager(ISceneLoader sceneLoader, ITransitionPlayer transitionPlayer = null, IInputBlocker inputBlocker = null)`. Existing test code compiles without changes.
- **Fade loop using `Time.deltaTime` in edit-mode tests** — `Time.deltaTime` is 0 in edit-mode, causing infinite loops. **Avoid by:** `MockTransitionPlayer` returns `UniTask.CompletedTask` synchronously (same pattern as `MockSceneLoader`). The runtime `UnityTransitionPlayer` uses real time — it's never called in edit-mode tests.
- **CanvasGroup on transition overlay intercepting input** — If the transition overlay `CanvasGroup` has `blocksRaycasts = true` while visible, it will block input independently of `UnityInputBlocker`. **Avoid by:** keeping the overlay's `blocksRaycasts = false` always. Input blocking is handled exclusively by `IInputBlocker`. The overlay is purely visual (alpha only).
- **Forgetting to unblock input on exception** — If `FadeOutAsync` succeeds but scene load throws, input stays blocked forever. **Avoid by:** placing the `_inputBlocker.Unblock()` call in the `finally` block of the existing `try/finally` in `ShowScreenAsync`/`GoBackAsync`. The pattern is already established — just add `Unblock()` alongside `_isNavigating = false`.
- **Fade overlay sort order conflicts with popup overlay** — Both the transition overlay and the popup input blocker use high-sort-order canvases. **Avoid by:** giving the transition overlay a *higher* sort order than the popup layer (e.g., popup layer sort order 100, transition overlay sort order 200). The fade should visually cover everything including popups.
- **`IInputBlocker` namespace location** — `IInputBlocker` is currently in `SimpleGame.Core.PopupManagement`. ScreenManager consuming it means a cross-namespace dependency (ScreenManagement → PopupManagement). This is acceptable — `IInputBlocker` is a shared infrastructure concern, not popup-specific. If it feels wrong, it could be moved to a shared namespace, but that's a refactor outside S04 scope. Leave it and note for potential future cleanup.

## Open Risks

- **ScreenManager constructor change is a modification of completed S02 code.** The change is additive (optional parameters, no behavior change without the new dependencies), and all existing tests pass unchanged. But if the change subtly breaks anything, the blast radius includes S02 and all downstream slices. Mitigation: run the full 27-test suite after the change.
- **Fade duration tuning** — The "right" fade duration is subjective and can only be judged in play-mode. Default to 0.3s; expose as a configurable field on `UnityTransitionPlayer`. This is a play-mode-only verification item.
- **S05 wiring complexity** — S05 must now construct `ScreenManager` with 3 dependencies (`ISceneLoader`, `ITransitionPlayer`, `IInputBlocker`) instead of 1. The boot scene must find/create both the `UnityTransitionPlayer` and `UnityInputBlocker` MonoBehaviours before constructing the `ScreenManager`. This is manageable but adds wiring steps.
- **GoBackAsync transition behavior** — The transition should play on back-navigation too, not just forward. Both methods need the same fade-out → unload → load → fade-in bracket. Verify both paths.

## Requirements Targeted

| Requirement | Role | What S04 must deliver |
|-------------|------|----------------------|
| **R013** — Fade transitions between screens | **Primary owner** | TransitionManager/ITransitionPlayer, fade overlay, input blocking during transitions |
| **R012** — Full-screen raycast input blocker | **Supporting** | Reuse IInputBlocker during transitions (already validated in S03, just consumed here) |
| **R010** — Screen navigation between full screens | **Supporting** | ScreenManager gains transition-aware navigation without changing navigation semantics |
| **R014** — UniTask async/await | **Supporting** | Fade animation uses UniTask.Yield/Delay for async interpolation |
| **R006** — No static state | **Supporting** | All new files must pass the static guard grep |
| **R005** — Constructor/init injection only | **Supporting** | ITransitionPlayer and IInputBlocker injected into ScreenManager via constructor |
| **R001** — MVP pattern with strict separation | **Supporting** | Core/Runtime split for transition player mirrors existing ISceneLoader/UnitySceneLoader |

## Skills Discovered

| Technology | Skill | Status |
|------------|-------|--------|
| Unity (general) | `wshobson/agents@unity-ecs-patterns` | available (3.1K installs) — ECS-focused, not relevant to uGUI transitions |
| Unity (general) | `rmyndharis/antigravity-skills@unity-developer` | available (568 installs) — general Unity, potentially useful but not critical for this slice |
| UI animation | `joelhooks/joelclaw@ui-animation` | available (24 installs) — web-focused (not Unity), not relevant |
| UniTask | none found | No UniTask-specific skill available |

No skills are directly relevant enough to warrant installation for this slice. The transition system is straightforward CanvasGroup alpha interpolation with UniTask — no specialized tooling needed.

## Sources

- UniTask async delay and timing patterns (source: [UniTask README](https://github.com/cysharp/unitask))
- `UniTask.Delay(TimeSpan, cancellationToken)` is the zero-allocation replacement for `WaitForSeconds` — supports `CancellationToken` natively
- `UniTask.Yield(PlayerLoopTiming.Update)` yields one frame at Update timing — suitable for per-frame alpha interpolation loops
- S02 Forward Intelligence explicitly recommends: "await a transition's fade-out before the unload call, and await the fade-in after the load call — either by adding optional ITransitionProvider injection to ScreenManager or by wrapping ShowScreenAsync in a higher-level navigation service"

## Appendix: Proposed File Layout

```
Assets/Scripts/Core/TransitionManagement/
  ITransitionPlayer.cs          — interface: FadeOutAsync, FadeInAsync (pure C#, no Unity)

Assets/Scripts/Runtime/TransitionManagement/
  UnityTransitionPlayer.cs      — MonoBehaviour: CanvasGroup.alpha interpolation loop

Assets/Scripts/Core/ScreenManagement/
  ScreenManager.cs              — MODIFIED: optional ITransitionPlayer + IInputBlocker injection

Assets/Tests/EditMode/
  TransitionTests.cs            — MockTransitionPlayer + tests for transition-integrated ScreenManager
```

## Appendix: Proposed ITransitionPlayer Interface

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleGame.Core.TransitionManagement
{
    public interface ITransitionPlayer
    {
        UniTask FadeOutAsync(CancellationToken ct = default);
        UniTask FadeInAsync(CancellationToken ct = default);
    }
}
```

## Appendix: ScreenManager Modification Shape

```csharp
// Constructor gains optional dependencies:
public ScreenManager(
    ISceneLoader sceneLoader,
    ITransitionPlayer transitionPlayer = null,
    IInputBlocker inputBlocker = null)

// ShowScreenAsync body becomes:
//   if (_transitionPlayer != null) inputBlocker.Block();
//   if (_transitionPlayer != null) await _transitionPlayer.FadeOutAsync(ct);
//   ... existing unload/load ...
//   if (_transitionPlayer != null) await _transitionPlayer.FadeInAsync(ct);
//   if (_transitionPlayer != null) inputBlocker.Unblock(); // in finally block
```

## Appendix: UnityTransitionPlayer Fade Loop Shape

```csharp
// Fade out: alpha 0 → 1
public async UniTask FadeOutAsync(CancellationToken ct = default)
{
    float elapsed = 0f;
    _canvasGroup.alpha = 0f;
    _canvasGroup.gameObject.SetActive(true);
    while (elapsed < _fadeDuration)
    {
        elapsed += Time.deltaTime;
        _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
        await UniTask.Yield(ct);
    }
    _canvasGroup.alpha = 1f;
}

// Fade in: alpha 1 → 0
public async UniTask FadeInAsync(CancellationToken ct = default)
{
    float elapsed = 0f;
    while (elapsed < _fadeDuration)
    {
        elapsed += Time.deltaTime;
        _canvasGroup.alpha = Mathf.Clamp01(1f - elapsed / _fadeDuration);
        await UniTask.Yield(ct);
    }
    _canvasGroup.alpha = 0f;
    _canvasGroup.gameObject.SetActive(false);
}
```
