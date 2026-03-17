# S02: Scene Controller View Resolution + Boot SerializeField Refs

**Goal:** Scene controllers resolve popup views via `IViewResolver.Get<T>()` instead of `FindFirstObjectByType`, and `GameBootstrapper` uses `[SerializeField]` refs for boot infrastructure — eliminating 7 of 10 `FindFirstObjectByType` calls while all existing tests pass.
**Demo:** `rg "FindFirstObjectByType" Assets/Scripts/` returns only 3 matches (scene controller lookups in `GameBootstrapper`, deferred to S03). All `Initialize()` signatures include `IViewResolver`. All test files compile and pass.

## Must-Haves

- `InGameSceneController.Initialize()` accepts `IViewResolver` parameter; `ActiveLevelCompleteView` and `ActiveLevelFailedView` getters use `_viewResolver?.Get<T>()` as fallback instead of `FindFirstObjectByType`
- `MainMenuSceneController.Initialize()` accepts `IViewResolver` parameter; `ActiveConfirmDialogView` and `ActiveObjectRestoredView` getters use `_viewResolver?.Get<T>()` as fallback instead of `FindFirstObjectByType`
- `GameBootstrapper` passes `IViewResolver` to both scene controllers' `Initialize()` calls
- All test files updated with `IViewResolver` parameter (null or MockViewResolver) — no CS7036 errors
- `GameBootstrapper` has `[SerializeField]` fields for `UnityInputBlocker`, `UnityTransitionPlayer`, `UnityViewContainer` — replaces 3 `FindFirstObjectByType` calls in `Start()`
- `SceneSetup.cs` wires the 3 new `[SerializeField]` fields on `GameBootstrapper`
- `SetViewsForTesting()` test seam remains untouched — override check runs before `IViewResolver` fallback
- Zero `FindFirstObjectByType` in `InGameSceneController.cs` and `MainMenuSceneController.cs`
- Exactly 3 `FindFirstObjectByType` remaining in `GameBootstrapper.cs` (scene controller lookups, deferred to S03)

## Proof Level

- This slice proves: contract (compile + test pass confirms wiring correctness)
- Real runtime required: no (Unity batchmode tests are sufficient; full play-through deferred to S03)
- Human/UAT required: no

## Verification

```bash
# Zero FindFirstObjectByType in scene controllers
rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs
# → exit 1 (zero matches)

rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
# → exit 1 (zero matches)

# Exactly 3 remaining in GameBootstrapper (scene controller lookups for S03)
rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → exactly 3 matches

# IViewResolver used in both scene controllers
rg "IViewResolver" Assets/Scripts/Game/InGame/InGameSceneController.cs Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
# → matches in both files

# 4 SerializeField on GameBootstrapper (_worldData + 3 new)
rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs
# → 4 matches

# All Initialize() calls in tests include IViewResolver parameter
rg "\.Initialize\(" Assets/Tests/EditMode/Game/SceneControllerTests.cs Assets/Tests/EditMode/Game/InGameTests.cs
# → all MainMenu/InGame controller calls have viewResolver parameter

# Overall production FindFirstObjectByType count = 3 (GameBootstrapper only)
rg "FindFirstObjectByType" Assets/Scripts/ --count
# → GameBootstrapper.cs:3 only

# SceneSetup wires new fields
rg "_inputBlocker|_transitionPlayer|_viewContainer" Assets/Editor/SceneSetup.cs
# → 3 WireSerializedField calls
```

## Integration Closure

- Upstream surfaces consumed: `IViewResolver` interface (S01), `UnityViewContainer` implementing `IViewResolver` (S01), `MockViewResolver` test double in `ViewContainerTests.cs` (S01)
- New wiring introduced in this slice: `IViewResolver` parameter added to `Initialize()` on both scene controllers; `[SerializeField]` fields on `GameBootstrapper` for boot infrastructure; `SceneSetup` wires the 3 new fields
- What remains before the milestone is truly usable end-to-end: S03 — scene root convention for finding scene controllers (3 remaining `FindFirstObjectByType` calls), full test suite run, human UAT

## Tasks

- [ ] **T01: Wire IViewResolver into scene controller Initialize() and replace popup view FindFirstObjectByType** `est:25m`
  - Why: Eliminates 4 `FindFirstObjectByType` calls in scene controllers (R072) and establishes the `IViewResolver` injection pattern. This is the higher-risk task — it changes `Initialize()` signatures which propagate to `GameBootstrapper` and 8 test call sites.
  - Files: `Assets/Scripts/Game/InGame/InGameSceneController.cs`, `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`, `Assets/Scripts/Game/Boot/GameBootstrapper.cs`, `Assets/Tests/EditMode/Game/InGameTests.cs`, `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
  - Do: (1) Add `IViewResolver _viewResolver` field + parameter to `InGameSceneController.Initialize()`. Replace `ActiveLevelCompleteView`/`ActiveLevelFailedView` getters: fallback changes from `FindFirstObjectByType<T>()` to `_viewResolver?.Get<T>()`. (2) Same for `MainMenuSceneController` — add field/param, replace `ActiveConfirmDialogView`/`ActiveObjectRestoredView` getters. Note `MainMenuSceneController` has an existing `[SerializeField] _confirmDialogView` check before the fallback — preserve that. (3) Update `GameBootstrapper` to pass `popupContainer` (cast as `IViewResolver`) in both `ctrl.Initialize()` calls for MainMenu and InGame cases. (4) Update test files: add `null` as the `IViewResolver` parameter to all `Initialize()` calls (tests use `SetViewsForTesting()` which overrides, so null is safe).
  - Verify: `rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/InGameSceneController.cs` → exit 1; same for MainMenuSceneController; `rg "IViewResolver" Assets/Scripts/Game/InGame/InGameSceneController.cs Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` → matches in both
  - Done when: Zero `FindFirstObjectByType` in both scene controllers, all `Initialize()` call sites compile (no CS7036), test seam (`SetViewsForTesting`) untouched

- [ ] **T02: Add SerializeField refs to GameBootstrapper for boot infrastructure and wire in SceneSetup** `est:15m`
  - Why: Eliminates 3 `FindFirstObjectByType` calls for boot infrastructure in `GameBootstrapper.Start()` (R073). Completes the explicit-wiring pattern for all Boot scene components.
  - Files: `Assets/Scripts/Game/Boot/GameBootstrapper.cs`, `Assets/Editor/SceneSetup.cs`
  - Do: (1) Add three `[SerializeField] private` fields to `GameBootstrapper`: `UnityInputBlocker _inputBlocker`, `UnityTransitionPlayer _transitionPlayer`, `UnityViewContainer _viewContainer`. (2) In `Start()`, remove the 3 `FindFirstObjectByType` calls (lines 57-59) and use `_inputBlocker`, `_transitionPlayer`, `_viewContainer` directly. Rename local vars or use fields directly — the `popupContainer` local used in `PopupManager` construction becomes `_viewContainer`. (3) In `SceneSetup.CreateBootScene()`, after the bootstrapper is created, add 3 `WireSerializedField` calls: `_inputBlocker` → `inputBlocker`, `_transitionPlayer` → the `UnityTransitionPlayer` component on the transition instance (with null-guard for missing prefab), `_viewContainer` → `popupContainer`.
  - Verify: `rg "\[SerializeField\]" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → 4 matches; `rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → exactly 3 matches (scene controller lookups only); `rg "_inputBlocker|_transitionPlayer|_viewContainer" Assets/Editor/SceneSetup.cs` → 3 WireSerializedField calls
  - Done when: Boot infrastructure wired via SerializeField, no `FindFirstObjectByType` for `UnityInputBlocker`/`UnityTransitionPlayer`/`UnityViewContainer` in `GameBootstrapper`

## Files Likely Touched

- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
