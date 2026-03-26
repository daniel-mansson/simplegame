---
id: S02
parent: M021
milestone: M021
provides:
  - InGameFlowPresenter pure C# class with full game loop and popup orchestration
  - InGameSceneController slimmed to 133 lines (wiring board + editor bootstrap only)
  - Test seams (SetViewsForTesting, SetModelFactory, SetWinPopupDelay) delegated from controller to presenter
  - ApplyRemoteConfig() method on InGameFlowPresenter for remote config application
key_files:
  - Assets/Scripts/Game/InGame/InGameFlowPresenter.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
key_decisions:
  - "Test seam methods stay on InGameSceneController but delegate to _flowPresenter — no test changes required"
  - "InGameFlowPresenter created in Initialize() so seams are available immediately after"
patterns_established:
  - "Flow presenter pattern: pure C# class owns RunAsync loop; controller stores and delegates"
duration: 20min
verification_result: pass
completed_at: 2026-03-26T14:20:00Z
---

# S02: Extract InGameFlowPresenter

**Game loop, popup orchestration, retry flow, and model factory all moved to pure C# InGameFlowPresenter.**

## What Happened

Created `InGameFlowPresenter` with the complete `RunAsync` game loop previously in `InGameSceneController`: model factory execution, `InGamePresenter` lifecycle, win/lose/retry branching, `HandleLevelCompletePopupAsync`, `HandleLevelFailedPopupAsync`, `HandleShopPopupAsync`, `HandleInterstitialAsync`, `HandleRewardedAdAsync`, `RetryTransitionAsync`, `BuildStubModel`, and all test seams.

`InGameSceneController` creates `InGameFlowPresenter` in `Initialize()` and delegates all seam methods and `RunAsync` to it. The editor play-from-editor bootstrap path calls `Initialize()` to create the presenter before calling `RunAsync`.

347/347 tests passed without any test file changes — all seam methods still present on the controller via delegation.

## Deviations

Added `using SimpleGame.Game.Boot` to `InGameFlowPresenter.cs` (needed for `UIFactory`). Removed unused `PuzzleModelConfig` `[SerializeField]` from controller.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — new, full game loop
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — delegating wiring board
