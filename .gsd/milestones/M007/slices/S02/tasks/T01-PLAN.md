---
estimated_steps: 5
estimated_files: 5
---

# T01: Wire IViewResolver into scene controller Initialize() and replace popup view FindFirstObjectByType

**Slice:** S02 — Scene Controller View Resolution + Boot SerializeField Refs
**Milestone:** M007

## Description

Add `IViewResolver` as a dependency to both `InGameSceneController` and `MainMenuSceneController` via their `Initialize()` methods. Replace the `FindFirstObjectByType` fallback in their popup view property getters with `_viewResolver?.Get<T>()`. Propagate the signature change to `GameBootstrapper` (which passes the view container as `IViewResolver`) and all test call sites (which pass `null` since `SetViewsForTesting()` overrides take precedence).

This eliminates 4 `FindFirstObjectByType` calls (2 in each scene controller) and establishes R072's pattern.

Relevant installed skills: none needed — straightforward C# refactor.

## Steps

1. **`InGameSceneController.cs`** — Add `using SimpleGame.Core.PopupManagement;` if not already present. Add `private IViewResolver _viewResolver;` field. Add `IViewResolver viewResolver` as the **last** parameter to `Initialize()` (after `hearts`). Store it: `_viewResolver = viewResolver;`. Replace the `ActiveLevelCompleteView` getter's fallback: change `FindFirstObjectByType<LevelCompleteView>(FindObjectsInactive.Include)` to `_viewResolver?.Get<ILevelCompleteView>()`. Same for `ActiveLevelFailedView`: change `FindFirstObjectByType<LevelFailedView>(FindObjectsInactive.Include)` to `_viewResolver?.Get<ILevelFailedView>()`. Remove unused `using SimpleGame.Game.Popup;` only if `LevelCompleteView`/`LevelFailedView` concrete types are no longer referenced (check — they may still be needed for other things; likely safe to remove since the getters now use interfaces, but the `using` for `PopupId` enum may come from there).

2. **`MainMenuSceneController.cs`** — Add `using SimpleGame.Core.PopupManagement;` if not already present (it's likely already there for `IPopupContainer`). Add `private IViewResolver _viewResolver;` field. Add `IViewResolver viewResolver` as the **last** parameter to `Initialize()` (after `goldenPieces`). Store it: `_viewResolver = viewResolver;`. Replace `ActiveConfirmDialogView` getter's fallback: the current chain is `_confirmDialogViewOverride ?? _confirmDialogView ?? FindFirstObjectByType<ConfirmDialogView>(...)`. Change the `FindFirstObjectByType` fallback to `_viewResolver?.Get<IConfirmDialogView>()`. Keep the `_confirmDialogView` SerializeField check intact — the chain becomes: override → serialized field → viewResolver. Same for `ActiveObjectRestoredView`: change `FindFirstObjectByType<ObjectRestoredView>(...)` to `_viewResolver?.Get<IObjectRestoredView>()`.

3. **`GameBootstrapper.cs`** — In the `ScreenId.MainMenu` case, update the `ctrl.Initialize(...)` call to add `popupContainer` (the local variable which is `UnityViewContainer`, implementing `IViewResolver`) as the last argument. In the `ScreenId.InGame` case, same — add `popupContainer` as the last argument. Note: `ScreenId.Settings` case is unchanged — `SettingsSceneController.Initialize(UIFactory)` has no popup views. Add `using SimpleGame.Core.PopupManagement;` if needed for `IViewResolver` (check — may already be present).

4. **`InGameTests.cs`** — Find all 6 `ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts)` calls in `InGameSceneControllerTests`. Add `null` as the last parameter to each: `ctrl.Initialize(_factory, _progression, _session, _popupManager, _goldenPieces, _hearts, null)`. The `null` is safe because every test calls `SetViewsForTesting()` immediately after, which sets the override fields that take precedence over `_viewResolver`.

5. **`SceneControllerTests.cs`** — Find the 2 `ctrl.Initialize(_factory, _popupManager, _metaProgression, _progression, _goldenPieces)` calls in `MainMenuSceneController` tests. Add `null` as the last parameter: `ctrl.Initialize(_factory, _popupManager, _metaProgression, _progression, _goldenPieces, null)`. The `null` is safe for the same reason.

## Must-Haves

- [ ] `InGameSceneController` has `IViewResolver _viewResolver` field, stored from `Initialize()` parameter
- [ ] `ActiveLevelCompleteView` getter uses `_viewResolver?.Get<ILevelCompleteView>()` instead of `FindFirstObjectByType`
- [ ] `ActiveLevelFailedView` getter uses `_viewResolver?.Get<ILevelFailedView>()` instead of `FindFirstObjectByType`
- [ ] `MainMenuSceneController` has `IViewResolver _viewResolver` field, stored from `Initialize()` parameter
- [ ] `ActiveConfirmDialogView` getter uses `_viewResolver?.Get<IConfirmDialogView>()` instead of `FindFirstObjectByType` (preserving existing `_confirmDialogView` SerializeField check)
- [ ] `ActiveObjectRestoredView` getter uses `_viewResolver?.Get<IObjectRestoredView>()` instead of `FindFirstObjectByType`
- [ ] `GameBootstrapper` passes `popupContainer` as `IViewResolver` to both `MainMenuSceneController.Initialize()` and `InGameSceneController.Initialize()`
- [ ] All 6 `InGameSceneController.Initialize()` calls in `InGameTests.cs` updated with `null` last parameter
- [ ] All 2 `MainMenuSceneController.Initialize()` calls in `SceneControllerTests.cs` updated with `null` last parameter
- [ ] `SetViewsForTesting()` methods completely untouched in both controllers

## Verification

```bash
# No FindFirstObjectByType in scene controllers
rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs
# → exit 1

rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
# → exit 1

# IViewResolver used in both
rg "IViewResolver" Assets/Scripts/Game/InGame/InGameSceneController.cs Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
# → matches in both files

# GameBootstrapper passes viewResolver
rg "popupContainer\)" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → matches in MainMenu and InGame Initialize() calls

# Test call sites updated
rg "\.Initialize\(" Assets/Tests/EditMode/Game/InGameTests.cs | grep -c "null"
# → 6 (all InGameSceneController.Initialize calls end with null)

rg "\.Initialize\(" Assets/Tests/EditMode/Game/SceneControllerTests.cs | grep "goldenPieces"
# → both MainMenu calls end with null
```

## Observability Impact

**What signals change after this task:**
- `ActiveLevelCompleteView`, `ActiveLevelFailedView`, `ActiveConfirmDialogView`, `ActiveObjectRestoredView` getters now use `_viewResolver?.Get<T>()` instead of `FindFirstObjectByType`. The `Debug.LogError` fallback remains intact — it fires if the resolver also returns null (view is truly missing). A future agent diagnosing popup failures should filter the Unity Console for `[InGameSceneController]` or `[MainMenuSceneController]` to identify missing views.
- When `_viewResolver` is null (e.g., in tests that pass `null`), and no `SetViewsForTesting()` override is set, the getter returns null silently without a log — this is the expected "no view" path (popup handlers guard with `if (view == null) return;`).
- `GameBootstrapper` now passes `popupContainer` to both `Initialize()` calls — verifiable by checking the `popupContainer` variable is not null before the navigation loop via existing `[GameBootstrapper] Infrastructure ready.` log.

**How to inspect this task's work at runtime:**
- Play mode: Unity Console shows `[GameBootstrapper] Boot sequence started.` followed by `Infrastructure ready.` — if `popupContainer` is missing, subsequent `PopupManager` calls will log errors.
- Edit-mode tests: all 6 InGame and 2 MainMenu `Initialize()` calls pass `null` for `IViewResolver`; tests pass because `SetViewsForTesting()` overrides take precedence.

**No redaction constraints** — log messages contain only type/controller names.

## Inputs

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — The interface created in S01: `T Get<T>() where T : class` in namespace `SimpleGame.Core.PopupManagement`
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — Implements `IViewResolver` (S01). The `popupContainer` local in `GameBootstrapper` is already typed as `UnityViewContainer`, which implements `IViewResolver`
- `Assets/Tests/EditMode/Game/ViewContainerTests.cs` — Contains `MockViewResolver` in `SimpleGame.Tests.Game` namespace (not directly needed by this task since we pass `null`, but available if needed)

## Expected Output

- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — `IViewResolver` field + Initialize param; 2 getters use `_viewResolver?.Get<T>()` instead of `FindFirstObjectByType`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — `IViewResolver` field + Initialize param; 2 getters use `_viewResolver?.Get<T>()` instead of `FindFirstObjectByType`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — `popupContainer` passed as last arg in MainMenu and InGame `Initialize()` calls
- `Assets/Tests/EditMode/Game/InGameTests.cs` — All 6 `ctrl.Initialize()` calls have `null` appended
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — Both `ctrl.Initialize()` calls for MainMenu have `null` appended
