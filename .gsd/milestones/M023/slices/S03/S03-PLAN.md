# S03: Level Start Sequence & Polish

**Goal:** Level begins with full board overview, then smoothly zooms into the first valid placement area. Edge cases (extreme aspect ratios, very large boards) handled gracefully by existing ComputeFraming math.
**Demo:** After this: After this: level begins with full board overview, then smoothly zooms into the first valid placement area. Edge cases (extreme aspect ratios, very large boards) handled gracefully.

## Tasks
- [x] **T01: Added overview hold duration field, full-board framing math, instant-snap API, and wired the level-start camera sequence (snap overview → hold → animate to first placement area) with 3 new EditMode tests** — Add the CameraConfig field, CameraMath convenience method, CameraController instant-teleport API, wire the level-start camera sequence into InGameFlowPresenter.RunAsync, and add EditMode tests covering the new helpers.

## Steps

1. **CameraConfig** — Add `[SerializeField] public float OverviewHoldDuration = 1.0f;` with XML doc comment, after the existing `ZoomSpeed` field.

2. **CameraMath.ComputeFullBoardFraming** — Add a new static method:
   ```csharp
   public static (Vector3 center, float orthoSize) ComputeFullBoardFraming(
       Rect boardRect, float padding, float aspect, float minZoom, float maxZoom)
   ```
   Implementation: compute center from `boardRect.center` (with z=0), compute `requiredByHeight = (boardRect.height + 2*padding) * 0.5f`, `requiredByWidth = (boardRect.width + 2*padding) / (2*aspect)`, take `max`, clamp to `[minZoom, maxZoom]`. Return `(center, orthoSize)`.

3. **CameraController.SnapTo** — Add a public method:
   ```csharp
   public void SnapTo(Vector3 center, float orthoSize)
   ```
   Implementation: set `transform.position` to `(center.x, center.y, transform.position.z)`, set `_camera.orthographicSize` to `Mathf.Clamp(orthoSize, _config.MinZoom, _config.MaxZoom)` (or raw if `_config == null`). Set `_isAutoTracking = false`, reset velocity refs. Log the snap. Guard on `_camera != null`.

4. **InGameFlowPresenter.RunAsync — level-start sequence** — In the `while(true)` loop, after `var model = modelFactory()` and `presenter.Initialize()`, BEFORE `_analytics?.TrackLevelStarted`, insert:
   ```csharp
   // Level-start camera sequence
   if (_cameraController != null && _stage != null && _cameraController.Config != null)
   {
       var config = _cameraController.Config;
       var cam = _cameraController.GetComponent<UnityEngine.Camera>();
       float aspect = cam != null ? cam.aspect : 1f;

       // Wire board bounds early (before first piece placement)
       _cameraController.SetBoardBounds(_stage.GetBoardRect());

       // Snap to full-board overview (no animation)
       var boardRect = _stage.GetBoardRect();
       var (overviewCenter, overviewOrtho) = CameraMath.ComputeFullBoardFraming(
           boardRect, config.Padding, aspect, config.MinZoom, config.MaxZoom);
       _cameraController.SnapTo(overviewCenter, overviewOrtho);

       // Hold for overview duration
       await UniTask.Delay(
           System.TimeSpan.FromSeconds(config.OverviewHoldDuration),
           cancellationToken: ct);

       // Animate to first valid placement area
       var placeableIds = model.GetPlaceablePieceIds();
       var positions = new System.Collections.Generic.List<UnityEngine.Vector3>();
       foreach (var id in placeableIds)
       {
           var pos = _stage.GetSolvedPosition(id);
           if (pos.HasValue) positions.Add(pos.Value);
       }
       if (positions.Count > 0)
       {
           var (center, ortho) = CameraMath.ComputeFraming(
               positions, config.Padding, aspect, config.MinZoom, config.MaxZoom);
           _cameraController.SetTarget(center, ortho);
       }
   }
   ```
   Note: leave the existing `_boardBoundsSet` one-time guard in `InGamePresenter.HandlePiecePlaced` as a redundant safety net (per research recommendation — `SetBoardBounds` is idempotent).

5. **EditMode tests** — In `Assets/Tests/EditMode/Game/CameraTests.cs`, add a new `[TestFixture] internal class ComputeFullBoardFramingTests` with 3 tests:
   - `ComputeFullBoardFraming_SquareBoard_ReturnsCorrectFraming` — 1×1 rect centred at origin, verify center is (0,0,0) and orthoSize matches expected calculation
   - `ComputeFullBoardFraming_RectangularBoard_AdjustsForAspect` — a wider rect (e.g. 2×1), verify orthoSize accounts for width
   - `ComputeFullBoardFraming_TinyBoard_ClampsToMinZoom` — a very small rect (0.1×0.1 with 0 padding), verify orthoSize is clamped to minZoom

## Must-Haves

- [ ] `OverviewHoldDuration` field on CameraConfig with default 1.0f
- [ ] `ComputeFullBoardFraming` is pure static — no MonoBehaviour deps, testable in EditMode
- [ ] `SnapTo` instantly sets camera position and orthoSize (no SmoothDamp)
- [ ] `SnapTo` sets `_isAutoTracking = false` so LateUpdate doesn't immediately overwrite the snap
- [ ] Level-start sequence is fully null-guarded (`_cameraController != null && _stage != null && Config != null`)
- [ ] Board bounds wired at level-start via `SetBoardBounds` (before first piece placement)
- [ ] Existing `_boardBoundsSet` guard in InGamePresenter left as redundant safety net
- [ ] 3 new EditMode tests for ComputeFullBoardFraming pass
- [ ] All existing 358+ tests pass with no regressions
  - Estimate: 45m
  - Files: Assets/Scripts/Game/InGame/CameraConfig.cs, Assets/Scripts/Game/InGame/CameraMath.cs, Assets/Scripts/Game/InGame/CameraController.cs, Assets/Scripts/Game/InGame/InGameFlowPresenter.cs, Assets/Tests/EditMode/Game/CameraTests.cs
  - Verify: grep -c "OverviewHoldDuration" Assets/Scripts/Game/InGame/CameraConfig.cs returns >= 1 && grep -c "ComputeFullBoardFraming" Assets/Scripts/Game/InGame/CameraMath.cs returns >= 1 && grep -c "SnapTo" Assets/Scripts/Game/InGame/CameraController.cs returns >= 1 && grep -c "OverviewHoldDuration\|ComputeFullBoardFraming\|SnapTo" Assets/Scripts/Game/InGame/InGameFlowPresenter.cs returns >= 2 && grep -c "\[Test\]" Assets/Tests/EditMode/Game/CameraTests.cs returns >= 21 && Unity EditMode test runner: all tests pass (0 failures)
- [x] **T02: Added CameraConfig asset creation + wiring to SceneSetup.CreateInGameScene(), ran scene generation — CameraConfig.asset created at Assets/Data/CameraConfig.asset and InGame.unity's CameraController._config now references it** — Create a CameraConfig ScriptableObject asset in SceneSetup.CreateInGameScene() and wire it to the CameraController's [SerializeField] _config field. This closes the last gap — without this wiring, the CameraController's Config property returns null and all camera features (auto-tracking, level-start sequence, manual input) are silently skipped.

## Steps

1. **SceneSetup.CreateInGameScene()** — After the existing `cam.gameObject.AddComponent<CameraController>();` line (around line 385), add CameraConfig asset creation and wiring:
   ```csharp
   // Create or load CameraConfig asset
   var cameraConfigPath = "Assets/Data/CameraConfig.asset";
   var cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfig>(cameraConfigPath);
   if (cameraConfig == null)
   {
       cameraConfig = ScriptableObject.CreateInstance<CameraConfig>();
       if (!System.IO.Directory.Exists("Assets/Data"))
           System.IO.Directory.CreateDirectory("Assets/Data");
       AssetDatabase.CreateAsset(cameraConfig, cameraConfigPath);
   }
   var camController = cam.gameObject.GetComponent<CameraController>();
   WireSerializedField(camController, "_config", cameraConfig);
   ```
   IMPORTANT: Get the `CameraController` reference via `cam.gameObject.GetComponent<CameraController>()` since AddComponent was already called. Wire to `_config` which is the `[SerializeField] private CameraConfig _config` field.

2. **Run SceneSetup** — Execute `Tools/Setup/Create And Register Scenes` via Unity MCP to regenerate all scene files. Per K007, scene files must be regenerated after SerializeField wiring changes.

3. **Verify** — Confirm `CameraConfig.asset` exists at `Assets/Data/CameraConfig.asset` and the InGame scene file references it.

## Must-Haves

- [ ] CameraConfig asset created at `Assets/Data/CameraConfig.asset` if it doesn't exist, loaded if it does
- [ ] CameraController's `_config` field wired to the CameraConfig asset in the InGame scene
- [ ] SceneSetup run successfully — all 4 scene files regenerated
- [ ] All existing 358+ EditMode tests still pass
  - Estimate: 20m
  - Files: Assets/Editor/SceneSetup.cs, Assets/Data/CameraConfig.asset, Assets/Scenes/InGame.unity
  - Verify: grep -c "CameraConfig" Assets/Editor/SceneSetup.cs returns >= 2 && test -f Assets/Data/CameraConfig.asset (after SceneSetup run) && Unity EditMode test runner: all tests pass (0 failures)
