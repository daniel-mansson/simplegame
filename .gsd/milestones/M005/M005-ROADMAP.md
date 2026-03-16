# M005: Prefab-Based Transitions

**Vision:** Replace the hand-rolled transition fade with a prefab-based system using LitMotion for tweening. The transition prefab owns all visual implementation — callers see only `ITransitionPlayer`. First implementation: 0.3s fade-to-black. Architecture supports future complex transitions (images, animations, shaders) by swapping the prefab.

## Success Criteria

- Screen transitions use a 0.3s fade-to-black driven by LitMotion, not a manual while loop
- Transition visual lives in a self-contained prefab asset
- Swapping the prefab changes the transition without code changes
- `ITransitionPlayer` interface is unchanged
- All existing edit-mode tests pass (98+ tests)
- Full game loop works in play mode with transitions

## Key Risks / Unknowns

- LitMotion assembly references in SimpleGame.Core.asmdef — need `LitMotion` and `LitMotion.Extensions` added without breaking existing compilation
- Boot scene prefab extraction — inline objects must be cleanly replaced by the prefab instance

## Proof Strategy

- LitMotion asmdef wiring → retire in S01 by proving compilation succeeds with LitMotion imports in UnityTransitionPlayer
- Boot scene prefab extraction → retire in S01 by proving play-mode transitions work with the prefab in Boot scene

## Verification Classes

- Contract verification: all 98+ edit-mode tests pass; ITransitionPlayer interface unchanged
- Integration verification: play-mode screen transitions work (fade visible during navigation)
- Operational verification: none
- UAT / human verification: visual confirmation that fade looks correct in play mode

## Milestone Definition of Done

This milestone is complete only when all are true:

- `UnityTransitionPlayer` uses LitMotion `BindToAlpha`/`ToUniTask()` — no manual while loop
- Transition prefab exists as a standalone asset in the project
- Boot scene references the prefab (placed or instantiated)
- `ITransitionPlayer` interface has zero changes
- All 98+ edit-mode tests pass
- Play-mode screen navigation shows 0.3s fade-to-black transitions

## Requirement Coverage

- Covers: R044
- Partially covers: none
- Leaves for later: none
- Orphan risks: none

## Slices

- [ ] **S01: Prefab transition player with LitMotion** `risk:low` `depends:[]`
  > After this: Screen transitions use a 0.3s fade-to-black driven by LitMotion via a self-contained prefab in Boot scene. All 98+ tests pass. The prefab can be swapped to change the transition look.

## Boundary Map

### S01

Produces:
- `Assets/Prefabs/TransitionOverlay.prefab` — self-contained transition prefab (Canvas + CanvasGroup + UnityTransitionPlayer MonoBehaviour)
- `UnityTransitionPlayer.cs` rewritten to use `LMotion.Create().BindToAlpha().ToUniTask()` instead of manual while loop
- `SimpleGame.Core.asmdef` updated with LitMotion assembly references

Consumes:
- `ITransitionPlayer` interface (unchanged)
- `ScreenManager<TScreenId>` calling pattern (unchanged)
- `GameBootstrapper.FindFirstObjectByType<UnityTransitionPlayer>()` (unchanged)
- LitMotion package (already installed)
