# M007: Prefab-Based View Management

**Vision:** Refactor popup view wiring from scene-placed GameObjects with FindFirstObjectByType to prefab-based organization under a generic view resolver container. Eliminate all implicit scene scanning from production code while maintaining identical game behavior.

## Success Criteria

- Full game loop plays identically to M006 — all screens, popups, transitions, and domain logic unchanged
- Zero `FindFirstObjectByType` (or any `FindObject*` variant) in any production `.cs` file under `Assets/Scripts/`
- All 164+ edit-mode tests pass in Unity batchmode
- IViewResolver interface exists in Core, implemented by the renamed container
- All 6 popup views are prefabs under the container in Boot scene
- GameBootstrapper has SerializeField refs to all boot infrastructure (no scene scanning)
- Scene controllers found via scene root convention after additive scene load

## Key Risks / Unknowns

- Scene/prefab migration — moving popup views into prefabs without breaking serialized references or Boot scene layout
- Scene root convention — need to establish clean pattern for finding scene controllers from loaded scenes without FindFirstObjectByType
- Test seam compatibility — existing SetViewsForTesting pattern must coexist with IViewResolver in production

## Proof Strategy

- Scene/prefab migration → retire in S01 by proving container resolves all 6 popup view interfaces via Get<T>()
- Scene root convention → retire in S03 by proving GameBootstrapper finds controllers without any FindObject* calls
- Test seam compatibility → retire in S02 by proving all scene controller tests pass with IViewResolver injection

## Verification Classes

- Contract verification: `rg "FindFirstObjectByType\|FindObjectOfType" Assets/Scripts/` returns zero; test suite passes
- Integration verification: full game loop in Unity play mode
- Operational verification: none
- UAT / human verification: play through MainMenu → InGame → Win → MainMenu and InGame → Lose → Retry → Win

## Milestone Definition of Done

This milestone is complete only when all are true:

- All popup views are prefabs, organized under the renamed container in Boot
- IViewResolver in Core with Get<T>(), implemented by the container
- Scene controllers receive IViewResolver via Initialize() and resolve popup views through it
- GameBootstrapper uses SerializeField refs for boot infrastructure
- Scene controllers found via scene root convention (no FindObject* calls)
- Zero FindFirstObjectByType in production code (verified by grep)
- All 164+ edit-mode tests pass in Unity batchmode
- Full game flow works identically (human UAT)

## Requirement Coverage

- Covers: R069, R070, R071, R072, R073, R074, R075, R076, R077
- Partially covers: none
- Leaves for later: R078 (popup instantiation from prefabs)
- Orphan risks: none

## Slices

- [x] **S01: IViewResolver + Container Refactor** `risk:medium` `depends:[]`
  > After this: renamed container in Boot holds all 6 popup views as inactive children and exposes Get<IMyView>(), show/hide by PopupId still works, verified by tests

- [x] **S02: Scene Controller View Resolution + Boot SerializeField Refs** `risk:medium` `depends:[S01]`
  > After this: scene controllers get popup views from IViewResolver instead of FindFirstObjectByType, GameBootstrapper uses SerializeField refs for boot infrastructure, all existing tests pass

- [x] **S03: Scene Root Convention + Final Cleanup** `risk:medium` `depends:[S01,S02]`
  > After this: scene controllers resolved via scene root convention, zero FindFirstObjectByType in production code, all 164+ tests pass, full game flow identical

## Boundary Map

### S01 → S02

Produces:
- `IViewResolver` interface in Core — `T Get<T>() where T : class`
- Renamed container (e.g. `ViewContainer`) implementing both `IPopupContainer<PopupId>` and `IViewResolver`
- Container holds all 6 popup views as inactive children with `GetComponentInChildren<T>(true)` resolution
- PopupId-based show/hide unchanged

Consumes:
- nothing (first slice)

### S01 → S03

Produces:
- Same as S01 → S02 (IViewResolver, renamed container)

Consumes:
- nothing (first slice)

### S02 → S03

Produces:
- Scene controllers accept `IViewResolver` in `Initialize()` and use `Get<T>()` for popup views
- GameBootstrapper has `[SerializeField]` refs for `UnityInputBlocker`, `UnityTransitionPlayer`, container
- `FindFirstObjectByType` removed from `InGameSceneController` and `MainMenuSceneController`
- `FindFirstObjectByType` removed from `GameBootstrapper` for infrastructure (but still used for scene controllers)

Consumes from S01:
- `IViewResolver` interface
- Renamed container implementing IViewResolver
