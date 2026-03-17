# S02: Scene Controller View Resolution + Boot SerializeField Refs

**Goal:** Scene controllers get popup views from IViewResolver instead of FindFirstObjectByType. GameBootstrapper uses SerializeField refs for boot infrastructure.
**Demo:** All FindFirstObjectByType calls removed from scene controllers and boot infrastructure wiring. All existing tests pass. Only scene controller *discovery* (in nav loop) still uses FindFirstObjectByType — S03 handles that.

## Must-Haves

- InGameSceneController receives IViewResolver, uses Get<ILevelCompleteView>() and Get<ILevelFailedView>()
- MainMenuSceneController receives IViewResolver, uses Get<IConfirmDialogView>() and Get<IObjectRestoredView>()
- GameBootstrapper has SerializeField refs for UnityInputBlocker, UnityTransitionPlayer, UnityViewContainer
- FindFirstObjectByType removed from GameBootstrapper for infrastructure (3 calls)
- FindFirstObjectByType removed from InGameSceneController (2 calls) and MainMenuSceneController (2 calls)
- SetViewsForTesting test seam still works
- All 169+ tests pass

## Verification

- `rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/ Assets/Scripts/Game/MainMenu/` returns zero
- `rg "FindFirstObjectByType" Assets/Scripts/Game/Boot/GameBootstrapper.cs` returns only scene controller lookups (3 — one per ScreenId)
- All tests pass in Unity batchmode

## Tasks

- [x] **T01: Wire IViewResolver into scene controllers + boot SerializeField refs** `est:45m`
  - Why: Eliminates FindFirstObjectByType from scene controllers and boot infrastructure
  - Files: `Assets/Scripts/Game/InGame/InGameSceneController.cs`, `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`, `Assets/Scripts/Game/Boot/GameBootstrapper.cs`, `Assets/Editor/SceneSetup.cs`
  - Do: Add IViewResolver param to Initialize() on InGame and MainMenu controllers. Replace FindFirstObjectByType popup view lookups with _viewResolver.Get<T>(). Add SerializeField refs on GameBootstrapper for input blocker, transition player, view container. Remove 3 FindFirstObjectByType calls from boot infra. Update SceneSetup to wire new SerializeField refs. Pass IViewResolver from bootstrapper to controllers.
  - Verify: `rg "FindFirstObjectByType" Assets/Scripts/Game/InGame/` returns zero; `rg "FindFirstObjectByType" Assets/Scripts/Game/MainMenu/` returns zero
  - Done when: Scene controllers and boot infra use no FindFirstObjectByType

- [x] **T02: Update tests + full verification** `est:30m`
  - Why: Tests must pass with new Initialize() signatures and IViewResolver injection
  - Files: `Assets/Tests/EditMode/Game/SceneControllerTests.cs`, `Assets/Tests/EditMode/Game/InGameTests.cs`
  - Do: Update test SetUp/Initialize calls to pass MockViewResolver or null. Ensure SetViewsForTesting still overrides IViewResolver-provided views. Run full suite.
  - Verify: Unity batchmode 169+ tests all pass
  - Done when: All tests pass, test seam preserved

## Files Likely Touched

- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
