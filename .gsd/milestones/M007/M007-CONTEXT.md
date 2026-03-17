# M007: Prefab-Based View Management

**Gathered:** 2026-03-17
**Status:** Ready for planning

## Project Description

Structural refactor of the popup view wiring system in Puzzle Tap. Each popup view becomes a standalone prefab. A renamed container in Boot holds all popup prefabs as inactive children and exposes a generic `Get<IMyView>()` interface. All `FindFirstObjectByType` calls are eliminated from production code.

## Why This Milestone

The current wiring uses `FindFirstObjectByType` which is fragile, implicit, and doesn't scale. Popup views are placed directly in the Boot scene rather than as reusable prefabs. Scene controllers scan the scene to find popup views. This milestone introduces proper prefab-based organization and a generic view resolver — the standard Unity pattern for managing UI components. It also sets up a clean path toward future instantiation-based popup management.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Run the game and see identical behavior to M006 — same flow, same popups, same transitions
- Open the Boot scene and see popup views organized under a single container as prefab instances
- See zero `FindFirstObjectByType` calls in any production C# file

### Entry point / environment

- Entry point: Unity Editor play mode from Boot scene or any screen scene
- Environment: local dev / Unity Editor
- Live dependencies involved: none

## Completion Class

- Contract complete means: zero `FindFirstObjectByType` in production code, IViewResolver interface in Core, all 164+ tests pass
- Integration complete means: full game flow works identically — MainMenu → InGame → Win/Lose → Popups → MainMenu
- Operational complete means: none (no deployment, no services)

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Full game loop plays identically to M006 — MainMenu with meta world, InGame with hearts, all 6 popup types, object restoration, environment navigation
- `rg "FindFirstObjectByType\|FindObjectOfType\|FindObjectsOfType\|FindAnyObjectByType" Assets/Scripts/` returns zero results
- All 164+ edit-mode tests pass in Unity batchmode

## Risks and Unknowns

- **Scene/prefab migration** — moving views from Boot scene into prefabs without breaking serialized references. Unity MCP can create prefabs, but the wiring of SerializeField refs between the container and child prefabs needs manual scene work or SceneSetup script updates.
- **Scene root convention** — establishing a convention for how GameBootstrapper finds scene controllers after scene load. Need to resolve: does the scene loader return the root, or does the bootstrapper query the loaded scene's root GameObjects?
- **Test seam preservation** — scene controllers have `SetViewsForTesting` methods that inject mock views. After refactor, production code uses IViewResolver, but tests must still work with direct mock injection.
- **SceneSetup editor script** — `Assets/Editor/SceneSetup.cs` creates scenes programmatically. It will need updates to match the new container structure.

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — current popup container with switch-on-PopupId and SerializeField refs to popup GameObjects
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — Core interface for show/hide popups by PopupId
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — boot loop that uses FindFirstObjectByType for infrastructure and scene controllers
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — uses FindFirstObjectByType for LevelCompleteView and LevelFailedView
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — uses FindFirstObjectByType for ConfirmDialogView and ObjectRestoredView
- `Assets/Scripts/Game/Settings/SettingsSceneController.cs` — clean (no FindFirstObjectByType), but follows the pattern we'll extend
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — scene loader interface (load/unload only, no root return)
- `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs` — production scene loader
- `Assets/Scripts/Game/Boot/UIFactory.cs` — constructs all presenters, receives view interfaces

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R069 — Popup views are individual prefabs (this milestone delivers it)
- R070 — Generic view resolver interface in Core (this milestone delivers it)
- R071 — Popup container renamed and implements IViewResolver (this milestone delivers it)
- R072 — Scene controllers get popup views via IViewResolver (this milestone delivers it)
- R073 — GameBootstrapper uses SerializeField refs for boot infrastructure (this milestone delivers it)
- R074 — Scene controllers resolved via scene root convention (this milestone delivers it)
- R075 — Zero FindFirstObjectByType in production code (this milestone delivers it)
- R076 — All existing tests pass after refactor (this milestone proves it)
- R077 — Functionally identical behavior (this milestone proves it)

## Scope

### In Scope

- New `IViewResolver` interface in Core with `Get<T>()` method
- Rename `UnityPopupContainer` to reflect expanded role (e.g. `ViewContainer`)
- Container implements both `IPopupContainer<PopupId>` and `IViewResolver`
- All popup views extracted as prefabs, held as inactive children under container
- Scene controllers receive IViewResolver and use `Get<T>()` for popup views
- GameBootstrapper uses `[SerializeField]` refs for UnityInputBlocker, UnityTransitionPlayer, and container
- Scene root convention for finding scene controllers after scene load
- Eliminate all `FindFirstObjectByType` from production code
- Update SceneSetup editor script to match new structure
- All existing tests continue to pass

### Out of Scope / Non-Goals

- Screen views (MainMenuView, SettingsView, InGameView) stay in their scenes with SerializeField refs — not moved to container
- Popup instantiation from prefabs (future — R078 deferred)
- Any functional changes to game behavior
- New popup types or screens
- Art or visual changes

## Technical Constraints

- Must use Unity 6 (6000.3.10f1) APIs
- Must maintain Core/Game assembly separation — IViewResolver goes in Core
- Must preserve `SetViewsForTesting` test seam on scene controllers
- Must not introduce static state or singletons (domain-reload-disabled support)
- Must not add DI framework dependencies

## Integration Points

- Unity scene serialization — prefab references, SerializeField wiring
- Unity MCP — for scene and prefab manipulation if available
- SceneSetup editor script — must be updated to create new structure

## Open Questions

- Exact naming for the renamed container: `ViewContainer`? `PopupViewContainer`? Agent discretion.
- Whether `ISceneLoader` interface should change to return scene root info, or if root resolution is a separate utility. Agent discretion — keep it clean.
