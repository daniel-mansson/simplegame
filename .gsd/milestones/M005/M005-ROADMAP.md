# M005: Prefab-Based Transitions

**Vision:** Replace the hand-rolled transition fade with a prefab-based system using LitMotion for tweening. The transition prefab owns all visual implementation — callers see only `ITransitionPlayer`. First implementation: 0.3s fade-to-black. Architecture supports future complex transitions (images, animations, shaders) by swapping the prefab.

## Success Criteria

- Screen transitions use a 0.3s fade-to-black driven by LitMotion, not a manual while loop
- Transition visual lives in a self-contained prefab asset
- `ITransitionPlayer` interface is unchanged — zero signature changes
- All existing edit-mode tests pass (98+ tests)
- Full game loop works in play mode with transitions

## Key Risks / Unknowns

This is straightforward work. No major unknowns — LitMotion is installed, the API surface is known, the interface stays unchanged.

- `SimpleGame.Core.asmdef` needs LitMotion assembly references added — low risk but must compile cleanly

## Verification Classes

- Contract verification: all 98+ edit-mode tests pass; `ITransitionPlayer` interface unchanged; `UnityTransitionPlayer.cs` contains no `while` loops and imports `LitMotion`
- Integration verification: play-mode screen transitions work (fade visible during navigation)
- Operational verification: none
- UAT / human verification: visual confirmation that fade looks correct in play mode

## Milestone Definition of Done

This milestone is complete only when all are true:

- `UnityTransitionPlayer` uses `LMotion.Create().BindToAlpha().ToUniTask()` — no manual while loop
- Transition prefab exists as `Assets/Prefabs/TransitionOverlay.prefab`
- Boot scene contains an instance of the prefab (or references it)
- `ITransitionPlayer` interface has zero changes
- All 98+ edit-mode tests pass
- Play-mode screen navigation shows 0.3s fade-to-black transitions
- SceneSetup editor script creates the transition from prefab

## Requirement Coverage

- Covers: R044
- Partially covers: none
- Leaves for later: none
- Orphan risks: none

## Slices

- [x] **S01: Prefab transition player with LitMotion** `risk:low` `depends:[]`
  > After this: Screen transitions use a 0.3s fade-to-black driven by LitMotion via a self-contained prefab in Boot scene. All 98+ tests pass. User can swap the prefab to change the transition look.

## Boundary Map

### S01

Produces:
- `Assets/Prefabs/TransitionOverlay.prefab` — self-contained transition prefab (Canvas + CanvasGroup + Image + UnityTransitionPlayer MonoBehaviour)
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — rewritten to use `LMotion.Create(from, to, duration).BindToAlpha(canvasGroup).ToUniTask(ct)` for both FadeOutAsync and FadeInAsync
- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — updated with `"LitMotion"` and `"LitMotion.Extensions"` references
- `Assets/Editor/SceneSetup.cs` — updated to instantiate the transition prefab instead of building inline objects

Consumes:
- `ITransitionPlayer` interface (unchanged)
- `ScreenManager<TScreenId>` calling pattern (unchanged)
- `GameBootstrapper.FindFirstObjectByType<UnityTransitionPlayer>()` (unchanged)
- LitMotion package (already installed)
