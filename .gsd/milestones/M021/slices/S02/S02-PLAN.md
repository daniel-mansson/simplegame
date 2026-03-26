# S02: Extract InGameFlowPresenter

**Goal:** Move the game loop and all popup orchestration from InGameSceneController.RunAsync into a pure C# InGameFlowPresenter. InGameSceneController becomes a thin wiring board (≤80 lines).

**Demo:** Win/lose/retry/popup flows work identically. InGameSceneController is ≤80 lines. All 347 InGame tests pass against the new presenter seams.

## Must-Haves

- `InGameFlowPresenter.cs` exists in `Assets/Scripts/Game/InGame/`
- `InGameFlowPresenter` is pure C# (no MonoBehaviour)
- `InGameFlowPresenter` exposes: `RunAsync(CancellationToken)`, `SetViewsForTesting(...)`, `SetModelFactory(...)`, `SetWinPopupDelay(float)`, `SetDebugOverride(...)`, `ClearDebugOverride()`
- `InGameSceneController` is ≤80 lines: [SerializeField] fields + Initialize() + RunAsync() delegation only
- All EditMode tests pass (InGameTests call seams on InGameFlowPresenter via ctrl.SetViewsForTesting etc, OR InGameSceneController delegates to presenter)
- No game logic remains in InGameSceneController

## Tasks

- [ ] **T01: Create InGameFlowPresenter and slim InGameSceneController**
  Move RunAsync game loop + all private popup helpers + test seams + NullGoldenPieceService + BuildStubModel into InGameFlowPresenter. Controller calls presenter.RunAsync(). Update InGameTests to work with new structure.

## Files Likely Touched
- `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` (new)
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` (slim to ≤80 lines)
- `Assets/Tests/EditMode/Game/InGameTests.cs` (update to access seams via presenter)
