# S01: Auto-Tracking Camera Core

**Goal:** Camera smoothly pans and zooms to frame all valid placement positions after each piece is placed. CameraConfig ScriptableObject controls speed and zoom limits.
**Demo:** After this: After this: camera smoothly pans and zooms to frame all valid placement positions after each piece is placed. CameraConfig ScriptableObject controls speed and zoom limits.

## Tasks
- [x] **T01: Created CameraConfig ScriptableObject, CameraMath framing helper, and added GetPlaceablePieceIds/GetSolvedPosition query methods that T02/T03 consume** — Create all foundational types needed by the auto-tracking camera. This task produces the config, math, and data-access layer that T02 and T03 consume.

**CameraConfig ScriptableObject** — new `Assets/Scripts/Game/InGame/CameraConfig.cs`. Fields: `float SmoothTime = 1.2f`, `float MinZoom = 2f`, `float MaxZoom = 15f`, `float Padding = 1.5f`. All public with `[SerializeField]`. CreateAssetMenu attribute for editor creation.

**CameraMath static helper** — new `Assets/Scripts/Game/InGame/CameraMath.cs`. Static method `ComputeFraming(IReadOnlyList<Vector3> positions, float padding, float aspect, float minZoom, float maxZoom) → (Vector3 center, float orthoSize)`. Computes min/max X and Y of positions, adds padding, computes required orthoSize as `max((maxY - minY + 2*padding) / 2, (maxX - minX + 2*padding) / (2 * aspect))`, clamps to min/max zoom. Returns board center (0,0) fallback if positions is empty.

**PuzzleModel.GetPlaceablePieceIds()** — add public method to `Assets/Scripts/Puzzle/PuzzleModel.cs`. Stores the original pieces list in a private field `_pieces`. Iterates all piece IDs, filters by `!_board.IsPlaced(id) && _board.CanPlace(id)`. Returns `IReadOnlyList<int>`.

**PuzzleStageController.GetSolvedPosition()** — add public method to `Assets/Scripts/Game/InGame/PuzzleStageController.cs`: `public Vector3? GetSolvedPosition(int pieceId)`. Returns the world position from `_solvedWorldPositions` dict, or null if not found.
  - Estimate: 45m
  - Files: Assets/Scripts/Game/InGame/CameraConfig.cs, Assets/Scripts/Game/InGame/CameraMath.cs, Assets/Scripts/Puzzle/PuzzleModel.cs, Assets/Scripts/Game/InGame/PuzzleStageController.cs
  - Verify: rg "class CameraConfig" Assets/Scripts/ && rg "ComputeFraming" Assets/Scripts/Game/InGame/CameraMath.cs && rg "GetPlaceablePieceIds" Assets/Scripts/Puzzle/PuzzleModel.cs && rg "GetSolvedPosition" Assets/Scripts/Game/InGame/PuzzleStageController.cs
- [ ] **T02: Extend CameraController with SmoothDamp auto-tracking** — Add auto-tracking state and SmoothDamp update loop to the existing CameraController MonoBehaviour.

**New state fields:**
- `[SerializeField] private CameraConfig _config;` — assigned at runtime or via SceneSetup
- `private bool _isAutoTracking;`
- `private Vector3 _targetPosition;` — target camera world position
- `private float _targetOrthoSize;` — target orthographic size
- `private Vector3 _posVelocity;` — SmoothDamp velocity ref for position
- `private float _sizeVelocity;` — SmoothDamp velocity ref for ortho size

**New public API:**
- `public void SetConfig(CameraConfig config)` — runtime config injection
- `public void SetTarget(Vector3 center, float orthoSize)` — sets _targetPosition (preserving current Z), _targetOrthoSize (clamped to config min/max), enables _isAutoTracking, resets velocities
- `public bool IsAutoTracking => _isAutoTracking;`

**LateUpdate addition:** After the existing Update() handles input, add a LateUpdate() method that runs SmoothDamp when `_isAutoTracking` is true:
- `transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _posVelocity, config.SmoothTime)`
- `_camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _targetOrthoSize, ref _sizeVelocity, config.SmoothTime)`
- If both position and size are within a small epsilon of target, auto-tracking can remain enabled (it won't overshoot due to SmoothDamp)

**Important:** Do NOT modify the existing Update() input handling. S02 will add `_isPanning` → `_isAutoTracking = false` logic. For now, auto-tracking coexists with input — if the user drags, both apply (S02 fixes the override). Keep the existing HandleMouse/HandleTouch methods unchanged.

**Debug logging:** Log `[CameraController] SetTarget center=({x},{y}) ortho={orthoSize}` on each SetTarget call.
  - Estimate: 30m
  - Files: Assets/Scripts/Game/InGame/CameraController.cs
  - Verify: rg "SetTarget|SmoothDamp|_isAutoTracking|LateUpdate" Assets/Scripts/Game/InGame/CameraController.cs
- [ ] **T03: Wire auto-tracking into InGamePresenter and add EditMode tests** — Wire the complete auto-tracking pipeline: InGamePresenter receives PuzzleStageController + CameraController, subscribes to OnPiecePlaced, and drives camera targeting. Add comprehensive EditMode tests.

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
  - Estimate: 1h
  - Files: Assets/Scripts/Game/InGame/InGamePresenter.cs, Assets/Scripts/Game/Boot/UIFactory.cs, Assets/Scripts/Game/InGame/InGameFlowPresenter.cs, Assets/Scripts/Game/InGame/InGameSceneController.cs, Assets/Scripts/Game/InGame/CameraController.cs, Assets/Tests/EditMode/Game/CameraTests.cs
  - Verify: echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin then poll with mcporter call unityMCP.get_test_job job_id=<id> until succeeded — all tests pass including new CameraTests
