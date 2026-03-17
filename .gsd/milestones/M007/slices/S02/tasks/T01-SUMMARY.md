---
id: T01
parent: S02
milestone: M007
provides:
  - IViewResolver injected into InGameSceneController and MainMenuSceneController via Initialize()
  - 4 FindFirstObjectByType calls eliminated from scene controllers (2 per controller)
  - GameBootstrapper passes popupContainer as IViewResolver to both scene controller Initialize() calls
  - All 8 test call sites updated with null IViewResolver parameter
key_files:
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
  - Assets/Tests/EditMode/Game/SceneControllerTests.cs
key_decisions:
  - IViewResolver parameter added as last optional parameter (= null) to Initialize() on both controllers — preserves backward-compat and makes test null-passing safe
  - Kept using SimpleGame.Game.Popup; in both controllers (PopupId in InGame; ConfirmDialogView SerializeField in MainMenu still requires concrete type)
  - Kept all 4 Debug.LogError observability signals intact in view getters — fires if resolver also returns null
patterns_established:
  - View getter pattern: override field → SerializeField ref (if present) → _viewResolver?.Get<T>() → LogError + return null
  - Test pattern: pass null for IViewResolver when SetViewsForTesting() overrides take precedence
observability_surfaces:
  - "[InGameSceneController] LevelCompleteView not found in any loaded scene." — Unity Console, fires if resolver returns null and no override set"
  - "[InGameSceneController] LevelFailedView not found in any loaded scene." — same condition
  - "[MainMenuSceneController] ConfirmDialogView not found in any loaded scene." — same condition
  - "[MainMenuSceneController] ObjectRestoredView not found in any loaded scene." — same condition
duration: ~15m
verification_result: passed
completed_at: 2026-03-17
blocker_discovered: false
---

# T01: Wire IViewResolver into scene controller Initialize() and replace popup view FindFirstObjectByType

**Replaced 4 `FindFirstObjectByType` calls in scene controllers with `_viewResolver?.Get<T>()` and wired `IViewResolver` through `GameBootstrapper` and all 8 test call sites.**

## What Happened

Executed a straight C# refactor across 5 files:

1. **InGameSceneController**: Added `private IViewResolver _viewResolver;` field and `IViewResolver viewResolver = null` as last param to `Initialize()`. Replaced `FindFirstObjectByType<LevelCompleteView>(...)` with `_viewResolver?.Get<ILevelCompleteView>()` and `FindFirstObjectByType<LevelFailedView>(...)` with `_viewResolver?.Get<ILevelFailedView>()`. `using SimpleGame.Game.Popup;` retained — `PopupId` enum still referenced throughout.

2. **MainMenuSceneController**: Same pattern. Added field + param. Replaced `FindFirstObjectByType<ConfirmDialogView>(...)` with `_viewResolver?.Get<IConfirmDialogView>()` (after the existing SerializeField check) and `FindFirstObjectByType<ObjectRestoredView>(...)` with `_viewResolver?.Get<IObjectRestoredView>()`. `using SimpleGame.Game.Popup;` retained — `ConfirmDialogView` concrete type still used for the `[SerializeField]` field.

3. **GameBootstrapper**: Added `popupContainer` as last argument to both `ctrl.Initialize()` calls — MainMenu and InGame cases. `popupContainer` is `UnityViewContainer` which implements `IViewResolver` (wired in S01).

4. **InGameTests.cs**: All 6 `ctrl.Initialize(...)` calls updated with `null` appended via PowerShell replace.

5. **SceneControllerTests.cs**: Both MainMenu `ctrl.Initialize(...)` calls updated with `null` appended.

`SetViewsForTesting()` was untouched in both controllers — it remains the test seam whose overrides take precedence over `_viewResolver`.

## Verification

```
rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs   → exit 1 (0 matches) ✓
rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs → exit 1 (0 matches) ✓
rg "IViewResolver" both controllers → 2 matches each (field + param) ✓
rg "popupContainer\)" GameBootstrapper.cs → 2 matches (MainMenu + InGame cases) ✓
rg ".Initialize(" InGameTests.cs | grep -c "null" → 6 ✓
rg ".Initialize(" SceneControllerTests.cs | grep "goldenPieces" → both lines end with ", null)" ✓
rg "LogError.*not found in any loaded scene" both controllers → 4 matches ✓
rg "FindFirstObjectByType" Assets/Scripts/ --count → GameBootstrapper.cs:6 only ✓
```

## Diagnostics

- Unity Console: filter `[InGameSceneController]` or `[MainMenuSceneController]` to see view resolution failures at runtime.
- If `_viewResolver` is null (test path) and no `SetViewsForTesting()` override is set, view getters return null silently — popup handlers guard with `if (view == null) return;`.
- `_viewResolver` being non-null is verifiable in the Boot scene: `popupContainer` is a local in `GameBootstrapper.Start()` found immediately before both `Initialize()` calls.

## Deviations

None — implemented exactly as specified in the plan.

## Known Issues

GameBootstrapper still has 6 `FindFirstObjectByType` calls (3 for boot infrastructure, 3 for scene controller lookups) — both sets are addressed in T02 and S03 respectively, as planned.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — Added `_viewResolver` field + parameter; replaced 2 FindFirstObjectByType calls with `_viewResolver?.Get<T>()`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — Added `_viewResolver` field + parameter; replaced 2 FindFirstObjectByType calls with `_viewResolver?.Get<T>()`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — Added `popupContainer` as last arg to both scene controller Initialize() calls
- `Assets/Tests/EditMode/Game/InGameTests.cs` — All 6 InGameSceneController.Initialize() calls updated with null last param
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — Both MainMenuSceneController.Initialize() calls updated with null last param
- `.gsd/milestones/M007/slices/S02/S02-PLAN.md` — Added Observability/Diagnostics section and failure-path verification check (pre-flight)
- `.gsd/milestones/M007/slices/S02/tasks/T01-PLAN.md` — Added Observability Impact section (pre-flight)
