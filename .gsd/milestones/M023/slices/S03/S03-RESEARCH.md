# S03 Research: Level Start Sequence & Polish

**Calibration:** Light research. This slice applies established patterns from S01/S02 (CameraMath, CameraController, CameraConfig) to a straightforward sequence: snap camera to full-board overview → hold → animate to first valid area. The "polish" aspect (extreme aspect ratios, large boards) is already handled by the existing `ComputeFraming` bounding-box math. No new technology, no unfamiliar APIs.

## Requirements Targeted

- **R181** — Level start shows full board then zooms to first valid area
- **R060** (supporting) — Advances in-game camera completeness

## Summary

S03 needs three things:
1. **CameraConfig fields** — `OverviewHoldDuration` (how long to show the full board before zooming)
2. **A level-start camera sequence** — orchestrated in `InGameFlowPresenter.RunAsync`, between `modelFactory()` (which calls `SpawnLevel`) and the first `WaitForAction` loop
3. **CameraConfig asset wiring** — `SceneSetup.CreateInGameScene()` must create/assign a CameraConfig asset to the CameraController component. S01/S02 flagged this as unresolved.

The edge-case handling for extreme aspect ratios and very large boards requires no new code — `CameraMath.ComputeFraming` already picks `max(requiredByHeight, requiredByWidth)` and clamps to `[MinZoom, MaxZoom]`. For a 10×7 board (level 10+), the board rect is ~1 world unit wide — well within the MaxZoom=15 default. The only concern is if MaxZoom is too small for the full-board overview on extreme aspect ratios, but since overview uses the board rect (≤1 unit) with padding, the computed orthoSize will be small (≈1–2), comfortably above MinZoom=2.

## Recommendation

Three tasks, all low-risk:

**T01 — CameraConfig + CameraMath additions + tests**
- Add `OverviewHoldDuration` field to `CameraConfig` (float, default 1.0s)
- Add `CameraMath.ComputeFullBoardFraming(Rect boardRect, float padding, float aspect, float minZoom, float maxZoom)` — convenience wrapper that converts a Rect to positions and delegates to `ComputeFraming`, OR just computes center + orthoSize directly from the rect (simpler, since we have the rect already)
- Add EditMode tests for the new helper
- Add `PuzzleStageController.GetAllSolvedPositions()` → `IReadOnlyList<Vector3>` that returns all values from `_solvedWorldPositions` (needed for the "zoom to first valid area" part — or we can reuse the existing `GetPlaceablePieceIds` + `GetSolvedPosition` pattern)

**T02 — Level-start sequence in InGameFlowPresenter + CameraController**
- In `RunAsync`, after `var model = modelFactory()` and `presenter.Initialize()`, insert the camera overview sequence:
  1. Set board bounds on camera: `_cameraController.SetBoardBounds(_stage.GetBoardRect())`
  2. Snap camera to full-board overview (no animation): compute framing from board rect, set camera position/size directly (not via SetTarget, which would animate)
  3. Hold for `OverviewHoldDuration` seconds via `UniTask.Delay`
  4. Animate to first valid area: compute framing from `model.GetPlaceablePieceIds()` + `_stage.GetSolvedPosition()`, call `_camera.SetTarget(center, ortho)`
  5. Wait for SmoothDamp to roughly converge (e.g. `SmoothTime * 1.5` seconds) before enabling player input, OR just let the game loop start — the camera will animate while the player can already interact
- Add `CameraController.SnapTo(Vector3 center, float orthoSize)` — instant teleport (no SmoothDamp), used for the overview snap
- All new camera wiring guarded on `_cameraController != null && _stage != null` (existing pattern from D113)
- Move the `_boardBoundsSet` one-time call from `HandlePiecePlaced` to the level-start sequence (earlier = better, since the overview needs bounds too)

**T03 — SceneSetup CameraConfig asset wiring**
- In `SceneSetup.CreateInGameScene()`, after `AddComponent<CameraController>()`:
  - Create a CameraConfig asset at `Assets/Data/CameraConfig.asset` (or load if it exists)
  - Assign it to the CameraController's `_config` serialized field via `SerializedObject`
- Run SceneSetup to regenerate the InGame scene file
- Verify via Unity MCP test runner that all tests still pass

## Implementation Landscape

### Files That Change

| File | What Changes |
|------|-------------|
| `Assets/Scripts/Game/InGame/CameraConfig.cs` | Add `OverviewHoldDuration` float field (default 1.0f) |
| `Assets/Scripts/Game/InGame/CameraMath.cs` | Add `ComputeFullBoardFraming(Rect, float, float, float, float)` convenience method |
| `Assets/Scripts/Game/InGame/CameraController.cs` | Add `SnapTo(Vector3 center, float orthoSize)` — instant camera teleport |
| `Assets/Scripts/Game/InGame/PuzzleStageController.cs` | Add `GetAllSolvedPositions()` returning all solved world positions (if needed; alternative is to compute from board rect) |
| `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` | Insert level-start camera sequence in `RunAsync` after model creation; move board bounds wiring from HandlePiecePlaced to here |
| `Assets/Scripts/Game/InGame/InGamePresenter.cs` | Remove the `_boardBoundsSet` one-time check (moved to FlowPresenter level-start) |
| `Assets/Editor/SceneSetup.cs` | Create CameraConfig asset and wire it to CameraController SerializeField |
| `Assets/Tests/EditMode/Game/CameraTests.cs` | Add tests for ComputeFullBoardFraming and SnapTo-related logic |

### Key Existing APIs (from S01/S02)

- `CameraController.SetTarget(Vector3 center, float orthoSize)` — animated move via SmoothDamp
- `CameraController.SetBoardBounds(Rect)` — registers board rect for clamping
- `CameraController.Config` → `CameraConfig` (may be null)
- `CameraMath.ComputeFraming(positions, padding, aspect, minZoom, maxZoom)` → `(center, orthoSize)`
- `CameraMath.ComputeBoardRect(rows, cols)` → `Rect`
- `PuzzleStageController.GetBoardRect()` → `Rect`
- `PuzzleStageController.GetSolvedPosition(int pieceId)` → `Vector3?`
- `PuzzleModel.GetPlaceablePieceIds()` → `IReadOnlyList<int>`

### Level-Start Sequence (pseudo-code)

```
// After modelFactory() and presenter.Initialize(), before WaitForAction loop:
if (_cameraController != null && _stage != null && _cameraController.Config != null)
{
    var config = _cameraController.Config;
    var cam = _cameraController.GetComponent<Camera>();
    float aspect = cam != null ? cam.aspect : 1f;

    // 1. Wire board bounds (moved from HandlePiecePlaced)
    _cameraController.SetBoardBounds(_stage.GetBoardRect());

    // 2. Snap to full-board overview
    var boardRect = _stage.GetBoardRect();
    var (overviewCenter, overviewOrtho) = CameraMath.ComputeFullBoardFraming(
        boardRect, config.Padding, aspect, config.MinZoom, config.MaxZoom);
    _cameraController.SnapTo(overviewCenter, overviewOrtho);

    // 3. Hold
    await UniTask.Delay(TimeSpan.FromSeconds(config.OverviewHoldDuration), cancellationToken: ct);

    // 4. Animate to first valid area
    var placeableIds = model.GetPlaceablePieceIds();
    var positions = new List<Vector3>();
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

### Interaction with _boardBoundsSet in InGamePresenter

Currently `InGamePresenter.HandlePiecePlaced` has a one-time `_boardBoundsSet` flag that calls `SetBoardBounds` on first piece placement. If the level-start sequence in `InGameFlowPresenter` calls `SetBoardBounds` first, the `_boardBoundsSet` flag in InGamePresenter will still be false. Two options:
1. **Remove** the `_boardBoundsSet` logic from InGamePresenter entirely (InGameFlowPresenter handles it)
2. **Leave it** as a redundant safety net — `SetBoardBounds` is idempotent (just overwrites the rect)

Option 2 is safer: leave InGamePresenter's one-time guard as-is. If FlowPresenter runs first, the second call in HandlePiecePlaced is a harmless no-op. If FlowPresenter skips (test paths), HandlePiecePlaced still works.

### CameraConfig Asset Wiring in SceneSetup

SceneSetup uses `SerializedObject` to wire fields. Pattern from existing code (e.g., `InGameView._levelLabel`):
```csharp
// Create or load CameraConfig asset
var configPath = "Assets/Data/CameraConfig.asset";
var config = AssetDatabase.LoadAssetAtPath<CameraConfig>(configPath);
if (config == null)
{
    config = ScriptableObject.CreateInstance<CameraConfig>();
    AssetDatabase.CreateAsset(config, configPath);
}

// Assign to CameraController's [SerializeField] _config
var camControllerSO = new SerializedObject(camController);
camControllerSO.FindProperty("_config").objectReferenceValue = config;
camControllerSO.ApplyModifiedProperties();
```

### Edge Cases

- **Extreme aspect ratios:** `ComputeFraming` already handles this — `requiredByWidth` and `requiredByHeight` are computed independently and the max is taken. On ultra-wide screens, `requiredByWidth` wins and the orthoSize increases. On ultra-tall, `requiredByHeight` wins. No special handling needed.
- **Very large boards (level 10+, 8×7=56 pieces):** Board rect is ~1 world unit (max(8,7)=8, so width=7/8≈0.875, height=8/8=1.0). Padding adds ~1.5 on each side → overview orthoSize ≈ 2.0, well within [MinZoom=2, MaxZoom=15]. Works fine.
- **Stub/test path (no stage):** All camera code is guarded on `_cameraController != null && _stage != null`. Stub model path has `_stage == null` or `_stage.HasGridLayoutConfig == false` → camera sequence is skipped entirely.
- **Retry loop:** `RunAsync` has a `while(true)` retry loop. The level-start sequence runs each time `modelFactory()` is called, so retries get the overview again. This is correct behavior.

### Verification Strategy

- **Unit tests:** Add 2-3 tests for `ComputeFullBoardFraming` (square board, rectangular board, tiny board)
- **Grep checks:** Confirm `SnapTo` in CameraController, `OverviewHoldDuration` in CameraConfig, `ComputeFullBoardFraming` in CameraMath
- **Unity test runner:** All 358+ existing tests pass (no regressions)
- **SceneSetup:** Run and confirm CameraConfig asset is created and wired
