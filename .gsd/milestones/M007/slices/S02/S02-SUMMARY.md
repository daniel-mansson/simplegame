---
id: S02
milestone: M007
provides:
  - Scene controllers use IViewResolver.Get<T>() for popup views (no more FindFirstObjectByType)
  - GameBootstrapper has SerializeField refs for boot infrastructure
  - SceneSetup editor script wires bootstrapper refs
  - 169/169 tests pass (test seam preserved via SetViewsForTesting)
key_files:
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
key_decisions:
  - "IViewResolver as optional last param on Initialize() — tests pass null, SetViewsForTesting overrides"
  - "GameBootstrapper SerializeField refs for _inputBlocker, _transitionPlayer, _viewContainer"
patterns_established:
  - "Scene controllers receive IViewResolver via Initialize() for popup view resolution"
drill_down_paths:
  - .gsd/milestones/M007/slices/S02/S02-PLAN.md
verification_result: pass
completed_at: 2026-03-17T18:55:00Z
---

# S02: Scene Controller View Resolution + Boot SerializeField Refs

**Scene controllers and boot infra use IViewResolver + SerializeField refs, zero FindFirstObjectByType in scene controllers, 169/169 pass**

## What Happened

Added `IViewResolver viewResolver` as optional parameter to `Initialize()` on both `InGameSceneController` and `MainMenuSceneController`. Replaced all 4 `FindFirstObjectByType` calls in scene controllers with `_viewResolver.Get<T>()`. Added `[SerializeField]` refs on `GameBootstrapper` for `_inputBlocker`, `_transitionPlayer`, and `_viewContainer` — eliminated 3 `FindFirstObjectByType` calls for boot infrastructure. Updated `SceneSetup.cs` to wire the new bootstrapper refs. Tests pass unchanged because `viewResolver` defaults to `null` and tests use `SetViewsForTesting` to inject mocks directly.

## What This Unlocks

S03 can now eliminate the remaining 3 `FindFirstObjectByType` calls (scene controller discovery in the nav loop) via scene root convention.
