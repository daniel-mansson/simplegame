# M005: Prefab-Based Transitions

**Gathered:** 2026-03-16
**Status:** Ready for planning

## Project Description

Replace the hand-rolled `UnityTransitionPlayer` (manual while-loop alpha fade on a CanvasGroup) with a prefab-based transition system using LitMotion for tweening. The API stays high-level (`ITransitionPlayer.FadeOutAsync`/`FadeInAsync`) — callers never change. The prefab self-contains all visual elements and the implementation. Swapping the prefab swaps the transition look.

## Why This Milestone

The current transition implementation is a manual `while(elapsed < duration)` loop setting `CanvasGroup.alpha` each frame. It works, but:
- It's brittle — manual interpolation with `Time.deltaTime` has subtle issues (frame-dependent, no easing)
- Future transitions will involve images, animations, and shaders — the implementation must live in the prefab, not in procedural code
- LitMotion is already installed as the project's tweening library and has `BindToAlpha` + `ToUniTask()` integration

The user explicitly said: "later this will be a combination of images, animations and shaders, so the API should be kept high level and let the actual implementation control the details."

## User-Visible Outcome

### When this milestone is complete, the user can:

- See the same 0.3s fade-to-black transition between screens, now driven by LitMotion
- Swap the transition prefab in the Boot scene to change the transition look without touching code
- The existing game loop (menu → play → outcome → menu) works identically

### Entry point / environment

- Entry point: Play mode in Unity Editor, starting from Boot scene
- Environment: local dev / Unity Editor
- Live dependencies involved: none

## Completion Class

- Contract complete means: `UnityTransitionPlayer` uses LitMotion, lives on a prefab, ITransitionPlayer interface unchanged, all 98+ tests pass
- Integration complete means: transitions work in play mode during screen navigation
- Operational complete means: none

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Screen transitions in play mode use 0.3s fade-to-black driven by LitMotion (not manual loop)
- The transition prefab is a standalone asset that can be swapped
- All existing edit-mode tests pass (98/98 minimum)
- `ITransitionPlayer` interface is unchanged — no signature changes

## Risks and Unknowns

- LitMotion's `BindToAlpha` requires `LITMOTION_SUPPORT_UGUI` define — needs to be active (it auto-activates when `com.unity.ugui` is present, which it is)
- LitMotion's `ToUniTask()` requires `LITMOTION_SUPPORT_UNITASK` define — auto-activates when `com.cysharp.unitask` is present (it is)
- SimpleGame.Core.asmdef may need `LitMotion` and `LitMotion.Extensions` assembly references added
- Boot scene needs updating to reference the prefab instead of inline objects — meta GUIDs may need care

## Existing Codebase / Prior Art

- `Assets/Scripts/Core/TransitionManagement/ITransitionPlayer.cs` — interface (FadeOutAsync, FadeInAsync). Stays unchanged.
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — current implementation with manual while-loop. Will be rewritten to use LitMotion.
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — calls FadeOutAsync/FadeInAsync. No changes needed.
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — finds `UnityTransitionPlayer` via `FindFirstObjectByType`. No changes needed if the prefab is placed in Boot scene.
- `Assets/Scenes/Boot.unity` — currently has TransitionOverlay inline. Will need the prefab placed instead.
- `Assets/Tests/EditMode/Core/TransitionTests.cs` — tests use MockTransitionPlayer. No changes needed (tests are against the interface).
- `Library/PackageCache/com.annulusgames.lit-motion@*/Runtime/Extensions/uGUI/LitMotionUGUIExtensions.cs` — has `BindToAlpha` for CanvasGroup
- `Library/PackageCache/com.annulusgames.lit-motion@*/Runtime/External/UniTask/LitMotionUniTaskExtensions.cs` — has `ToUniTask()`

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R044 — Prefab-based transition with LitMotion tweening (primary)
- R013 — Fade transitions between screens (stays validated — implementation upgrade)
- R014 — UniTask async/await for async operations (LitMotion's ToUniTask integrates with this)

## Scope

### In Scope

- Rewrite `UnityTransitionPlayer` to use LitMotion `BindToAlpha`/`ToUniTask()`
- Extract transition overlay from Boot scene into a prefab asset
- Add LitMotion/LitMotion.Extensions assembly references to SimpleGame.Core.asmdef
- Ensure the prefab is self-contained (canvas, CanvasGroup, the MonoBehaviour)
- Verify all 98+ tests still pass
- Verify play-mode transitions work

### Out of Scope / Non-Goals

- Changing `ITransitionPlayer` interface
- Adding new transition types (shaders, animated images) — that's future work
- Changing how `ScreenManager` or `GameBootstrapper` find/use the transition player
- Adding transition configuration UI or runtime transition swapping

## Technical Constraints

- LitMotion's uGUI extensions are gated behind `LITMOTION_SUPPORT_UGUI` (auto-defined when `com.unity.ugui` is present)
- LitMotion's UniTask extensions are gated behind `LITMOTION_SUPPORT_UNITASK` (auto-defined when UniTask is present)
- `SimpleGame.Core.asmdef` currently references `UniTask` and `UnityEngine.UI` — needs `LitMotion` and `LitMotion.Extensions` added
- Boot scene TransitionOverlay is currently inline — extracting to prefab must preserve or replace the component references cleanly

## Integration Points

- LitMotion package (`com.annulusgames.lit-motion`) — already installed via git URL
- Boot scene — transition overlay object needs to reference the prefab
- `GameBootstrapper` — uses `FindFirstObjectByType<UnityTransitionPlayer>()` to find the transition player at boot

## Open Questions

- None — scope is clear and constrained
