---
estimated_steps: 44
estimated_files: 6
skills_used: []
---

# T03: Wire auto-tracking into InGamePresenter and add EditMode tests

Wire the complete auto-tracking pipeline: InGamePresenter receives PuzzleStageController + CameraController, subscribes to OnPiecePlaced, and drives camera targeting. Add comprehensive EditMode tests.

**InGamePresenter changes:**
- Add constructor parameters: `PuzzleStageController stage = null, CameraController cameraController = null` (optional, after existing params, to preserve backward compat)
- Store as `private readonly PuzzleStageController _stage;` and `private readonly CameraController _camera;`
- In `HandlePiecePlaced(int pieceId)` after existing logic, add camera targeting:
  ```
  if (_stage != null && _camera != null)
  {
      var placeableIds = _model.GetPlaceablePieceIds();
      var positions = new List<Vector3>();
      foreach (var id in placeableIds)
      {
          var pos = _stage.GetSolvedPosition(id);
          if (pos.HasValue) positions.Add(pos.Value);
      }
      if (positions.Count > 0)
      {
          var cam = _camera.GetComponent<Camera>() ?? Camera.main;
          var (center, ortho) = CameraMath.ComputeFraming(positions, config.Padding, cam.aspect, config.MinZoom, config.MaxZoom);
          _camera.SetTarget(center, ortho);
      }
  }
  ```
  The CameraConfig is accessed via `_camera`'s config reference. Add a helper `_camera.Config` or pass config values through. Simplest: add `public CameraConfig Config => _config;` on CameraController.

**UIFactory.CreateInGamePresenter changes:**
- Add optional parameters `PuzzleStageController stage = null, CameraController cameraController = null`
- Pass through to InGamePresenter constructor

**InGameFlowPresenter changes:**
- Pass `_stage` and camera controller to `_uiFactory.CreateInGamePresenter(ActiveView, model, _stage, cameraController)`
- Get camera controller: store a reference from constructor or resolve via `Camera.main.GetComponent<CameraController>()` at RunAsync time
- Add `CameraController _cameraController` parameter to constructor (optional). InGameSceneController resolves it from `Camera.main` or as a SerializeField.

**InGameSceneController changes:**
- In Initialize(), resolve CameraController: `var cam = Camera.main?.GetComponent<CameraController>();`
- Pass it to InGameFlowPresenter constructor

**EditMode tests** in new `Assets/Tests/EditMode/Game/CameraTests.cs`:
1. `CameraMath_ComputeFraming_SinglePosition_ReturnsPositionAsCenter` — one position → center equals that position, orthoSize = minZoom (clamped)
2. `CameraMath_ComputeFraming_MultiplePositions_CorrectBounds` — 4 corner positions → center is average, orthoSize covers spread + padding
3. `CameraMath_ComputeFraming_EmptyPositions_ReturnsFallback` — empty list → (0,0,0) center, minZoom ortho
4. `CameraMath_ComputeFraming_ClampsToMaxZoom` — widely spread positions → orthoSize capped at maxZoom
5. `GetPlaceablePieceIds_ReturnsOnlyValidUnplacedPieces` — build a 5-piece chain model, place seed + piece 1, verify GetPlaceablePieceIds returns only piece 2 (has placed neighbor) not pieces 3-4
6. `GetPlaceablePieceIds_AllPlaced_ReturnsEmpty` — complete the puzzle, verify empty list
7. `GetPlaceablePieceIds_AfterEachPlacement_ListShrinks` — place pieces sequentially, verify list changes correctly

**Existing test fixup:** InGamePresenter constructor now has 2 new optional params. Existing tests pass `null` implicitly (optional params). Verify all existing InGameTests still compile and pass.

**Mock note (K004):** No new interface members on IInGameView, so existing mocks don't need updating. InGamePresenter's new params are optional and default to null.

## Inputs

- ``Assets/Scripts/Game/InGame/CameraController.cs` — extended in T02 with SetTarget API and CameraConfig reference`
- ``Assets/Scripts/Game/InGame/CameraMath.cs` — static helper created in T01`
- ``Assets/Scripts/Puzzle/PuzzleModel.cs` — modified in T01 with GetPlaceablePieceIds()`
- ``Assets/Scripts/Game/InGame/PuzzleStageController.cs` — modified in T01 with GetSolvedPosition()`
- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — config SO created in T01`
- ``Assets/Scripts/Game/InGame/InGamePresenter.cs` — existing presenter to extend with camera wiring`
- ``Assets/Scripts/Game/Boot/UIFactory.cs` — existing factory to extend with new params`
- ``Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — existing flow presenter to pass camera refs through`
- ``Assets/Scripts/Game/InGame/InGameSceneController.cs` — existing scene controller to resolve CameraController`
- ``Assets/Tests/EditMode/Game/InGameTests.cs` — existing tests that must continue to pass`

## Expected Output

- ``Assets/Scripts/Game/InGame/InGamePresenter.cs` — modified with stage + camera params and HandlePiecePlaced camera targeting`
- ``Assets/Scripts/Game/Boot/UIFactory.cs` — modified CreateInGamePresenter with optional stage + camera params`
- ``Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — modified to pass camera controller through to UIFactory`
- ``Assets/Scripts/Game/InGame/InGameSceneController.cs` — modified to resolve CameraController and pass to flow presenter`
- ``Assets/Scripts/Game/InGame/CameraController.cs` — modified to add public Config property`
- ``Assets/Tests/EditMode/Game/CameraTests.cs` — new test file with 7+ tests for CameraMath and GetPlaceablePieceIds`

## Verification

echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin then poll with mcporter call unityMCP.get_test_job job_id=<id> until succeeded — all tests pass including new CameraTests
