# S02 — Research

**Date:** 2026-03-17

## Summary

This slice is straightforward application of known patterns to known code. S01 established `IViewResolver` and `MockViewResolver`. S02 wires them into three places: (1) scene controllers (`InGameSceneController`, `MainMenuSceneController`) replace their `FindFirstObjectByType` calls for popup views with `IViewResolver.Get<T>()`, (2) `GameBootstrapper` replaces its three `FindFirstObjectByType` calls for boot infrastructure (`UnityInputBlocker`, `UnityTransitionPlayer`, `UnityViewContainer`) with `[SerializeField]` fields, and (3) `SceneSetup.cs` wires the new SerializeField refs on GameBootstrapper.

All existing tests use `SetViewsForTesting()` which injects mock views via override fields. This test seam remains untouched — the override check (`_levelCompleteViewOverride != null`) runs before the `IViewResolver` fallback, so tests are unaffected. `SettingsSceneController` has no popup views and needs no changes.

After this slice: 4 of 10 `FindFirstObjectByType` calls are eliminated (3 infrastructure + 0 popup views... actually the popup view calls are replaced too = 4 popup view calls removed). The remaining 3 calls in `GameBootstrapper` are the scene controller lookups (`FindFirstObjectByType<MainMenuSceneController>()` etc.), deferred to S03 (scene root convention, D043).

## Recommendation

Two independent tasks:

**T01 — Scene controllers: IViewResolver injection.** Add `IViewResolver` parameter to `Initialize()` on `InGameSceneController` and `MainMenuSceneController`. Store it as a field. Replace the `FindFirstObjectByType` property getters with `_viewResolver.Get<T>()` fallback. Update `GameBootstrapper` to pass the view container (already typed as `UnityViewContainer` which implements `IViewResolver`) as the new parameter. Update `SceneSetup.cs` if any scene-setup wiring of `_confirmDialogView` SerializeField on `MainMenuSceneController` exists (it does — the field is wired in `CreateMainMenuScene`; this can remain as-is since the SerializeField provides the primary view ref for the scene-placed MainMenuView, while IViewResolver handles popup views from Boot).

**T02 — GameBootstrapper: SerializeField refs for boot infrastructure.** Add three `[SerializeField]` fields on `GameBootstrapper` for `UnityInputBlocker`, `UnityTransitionPlayer`, `UnityViewContainer`. Remove the three `FindFirstObjectByType` calls in `Start()`. Update `SceneSetup.cs` `CreateBootScene()` to wire these three new fields via `WireSerializedField`. Update `Boot.unity` if needed (SceneSetup regenerates it, so the scene file will be updated on next run).

T01 and T02 are independent and can be done in parallel, but T01 is higher risk (more files, more test surface). Recommend T01 first.

## Implementation Landscape

### Key Files

- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — Add `IViewResolver _viewResolver` field. Add `IViewResolver` param to `Initialize()`. Replace `ActiveLevelCompleteView` and `ActiveLevelFailedView` property getters: change `FindFirstObjectByType<LevelCompleteView>(FindObjectsInactive.Include)` to `_viewResolver?.Get<ILevelCompleteView>()`. Same for `LevelFailedView`. Removes 2 `FindFirstObjectByType` calls.

- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — Add `IViewResolver _viewResolver` field. Add `IViewResolver` param to `Initialize()`. Replace `ActiveConfirmDialogView` and `ActiveObjectRestoredView` property getters: change `FindFirstObjectByType<ConfirmDialogView>(FindObjectsInactive.Include)` to `_viewResolver?.Get<IConfirmDialogView>()`. Same for `ObjectRestoredView`. Removes 2 `FindFirstObjectByType` calls.

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — Two changes:
  1. Add three `[SerializeField]` fields: `UnityInputBlocker _inputBlocker`, `UnityTransitionPlayer _transitionPlayer`, `UnityViewContainer _viewContainer`. Remove the three `FindFirstObjectByType` calls in `Start()` and use the fields directly.
  2. Pass `_viewContainer` (which is `IViewResolver`) as the new parameter in each scene controller's `Initialize()` call. Note: the `popupContainer` local variable currently used for `PopupManager` construction should use `_viewContainer` instead.

- `Assets/Editor/SceneSetup.cs` — In `CreateBootScene()`, after creating the `GameBootstrapper` component, wire the three new fields:
  ```
  WireSerializedField(bootstrapper, "_inputBlocker", inputBlocker);
  WireSerializedField(bootstrapper, "_transitionPlayer", transitionInstance.GetComponent<UnityTransitionPlayer>());
  WireSerializedField(bootstrapper, "_viewContainer", popupContainer);
  ```
  Note: `transitionPlayer` is instantiated from a prefab — the `GetComponent<UnityTransitionPlayer>()` must be called on the instance, not the prefab. The variable is `transitionInstance`. There's a null-guard needed since SceneSetup logs a warning when the prefab isn't found.

- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — Update `MainMenuSceneController.Initialize()` calls to include `IViewResolver` parameter. Tests use `SetViewsForTesting()` to inject mocks, so the view resolver won't be consulted — pass `null` or a `MockViewResolver`.

- `Assets/Tests/EditMode/Game/InGameTests.cs` — Update `InGameSceneController.Initialize()` calls to include `IViewResolver` parameter. Same rationale — pass `null` since `SetViewsForTesting()` overrides take precedence.

- `Assets/Tests/EditMode/Game/ViewContainerTests.cs` — Contains `MockViewResolver` (S01 artifact). No changes needed — referenced for import by test files.

### Build Order

**T01 first:** Scene controller IViewResolver injection. This is higher risk because it touches the `Initialize()` signatures, which propagates to `GameBootstrapper` and both test files. It also removes the 4 popup-view `FindFirstObjectByType` calls.

1. Add `IViewResolver _viewResolver` field + parameter to `InGameSceneController.Initialize()`
2. Replace `ActiveLevelCompleteView` and `ActiveLevelFailedView` getters to use `_viewResolver.Get<T>()`
3. Same for `MainMenuSceneController`: field, parameter, replace 2 getters
4. Update `GameBootstrapper` to pass `popupContainer` (typed as `IViewResolver`) in all three `Initialize()` calls
5. Update test files: add null/MockViewResolver to `Initialize()` calls in `SceneControllerTests.cs` and `InGameTests.cs`

**T02 second:** GameBootstrapper SerializeField refs.

1. Add `[SerializeField] private UnityInputBlocker _inputBlocker;` and two more fields to `GameBootstrapper`
2. Remove the three `FindFirstObjectByType` calls in `Start()`, use field refs instead
3. Update `SceneSetup.cs` `CreateBootScene()` to wire the three new fields on bootstrapper

### Verification Approach

After both tasks:

```bash
# S02 scope: popup-view FindFirstObjectByType removed from scene controllers
rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs
# → exit 1 (zero matches)

rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
# → exit 1 (zero matches)

# S02 scope: boot infrastructure FindFirstObjectByType removed from GameBootstrapper
# Only scene controller lookups should remain (3 calls — deferred to S03)
rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → exactly 3 matches (MainMenuSceneController, SettingsSceneController, InGameSceneController)

# Confirm IViewResolver is used in scene controllers
rg "IViewResolver" Assets/Scripts/Game/InGame/InGameSceneController.cs Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
# → matches in both files

# Confirm SerializeField refs on GameBootstrapper
rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → 4 matches (_worldData + 3 new)

# Confirm tests compile (no signature mismatch)
rg "\.Initialize\(" Assets/Tests/EditMode/Game/SceneControllerTests.cs Assets/Tests/EditMode/Game/InGameTests.cs
# → all calls include IViewResolver parameter

# Overall production code FindFirstObjectByType count
rg "FindFirstObjectByType" Assets/Scripts/ --count
# → only GameBootstrapper.cs with 3 matches (scene controller lookups for S03)
```

## Constraints

- `SetViewsForTesting()` / `SetViewForTesting()` test seam must remain — tests inject mocks via these methods, not via IViewResolver. The override-check pattern (`_levelCompleteViewOverride != null ? _levelCompleteViewOverride : _viewResolver.Get<T>()`) preserves this.
- `SettingsSceneController` has no popup views, no `FindFirstObjectByType` — no changes needed. Its `Initialize(UIFactory)` signature stays unchanged.
- Scene controller `FindFirstObjectByType` calls for finding controllers themselves (in `GameBootstrapper`'s navigation loop) are **out of scope** — deferred to S03's scene root convention (D043).
- `MainMenuSceneController` has a `[SerializeField] private ConfirmDialogView _confirmDialogView` — this field is wired in the MainMenu scene for the scene-placed view. But the `ActiveConfirmDialogView` getter also falls back to `FindFirstObjectByType` when this field is null. After this slice, the fallback becomes `_viewResolver.Get<IConfirmDialogView>()` instead. The existing SerializeField can remain (it provides the primary view when the scene has one), but the getter's fallback chain changes.

## Common Pitfalls

- **Initialize() signature drift between prod and tests** — After adding `IViewResolver` to `Initialize()`, every call site must be updated. There are exactly 5 call sites: 3 in `GameBootstrapper.cs` (one per screen case), plus `SceneControllerTests.cs` (2 calls for MainMenu tests) and `InGameTests.cs` (6 calls for InGame tests). Missing any one causes CS7036 compile error. The planner should scope the test updates into the same task as the signature change.
- **Null-safe view resolver in scene controllers** — The `_viewResolver` field could be null if `Initialize()` isn't called (play-from-editor scenario). Use `_viewResolver?.Get<T>()` with null-conditional operator, same pattern as existing null checks.
