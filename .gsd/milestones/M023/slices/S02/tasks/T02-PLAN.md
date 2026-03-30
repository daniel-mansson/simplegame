---
estimated_steps: 13
estimated_files: 2
skills_used: []
---

# T02: Wire manual pan override, scroll/pinch zoom, and boundary clamping into CameraController

Integrate all manual input handling and boundary enforcement into CameraController, then wire board bounds from InGamePresenter.

**HandleMouse changes:**
- Set `_isAutoTracking = false` when `_isPanning` first becomes true (alongside existing `_isPanning = true`)
- After existing mouse handling, read `Input.mouseScrollDelta.y`; if abs > 0.01f and _config != null: set `_isAutoTracking = false`, adjust `_camera.orthographicSize -= scroll * _config.ZoomSpeed * Time.deltaTime`, clamp to [MinZoom, MaxZoom], then apply ClampToBounds

**HandleTouch changes:**
- Set `_isAutoTracking = false` when single-finger `_isPanning` starts
- Add pinch-to-zoom branch: when `Input.touchCount >= 2`, set `_isAutoTracking = false`, compute previous/current two-finger distance, delta = prevDist - currDist, adjust `_camera.orthographicSize += delta * _config.ZoomSpeed * 0.01f`, clamp, apply ClampToBounds. When touchCount >= 2, skip single-finger pan to avoid conflicts.

**ApplyScreenDelta change:** After setting `transform.position`, apply `CameraMath.ClampToBounds` if board rect is set.

**LateUpdate change:** After SmoothDamp (existing), apply `CameraMath.ClampToBounds` if board rect is set.

**New state:** `private Rect _boardRect; private bool _hasBoardRect;`
**New API:** `public void SetBoardBounds(Rect boardRect)` stores the rect and sets the flag.

**InGamePresenter wiring:** In HandlePiecePlaced, after existing camera targeting block, add a one-time call: if `_stage != null && _camera != null` and board bounds not yet set, call `_camera.SetBoardBounds(_stage.GetBoardRect())`.

All null guards: _config != null, _camera != null, _hasBoardRect checks before ClampToBounds calls.

## Inputs

- ``Assets/Scripts/Game/InGame/CameraController.cs` — existing CameraController with HandleMouse, HandleTouch, ApplyScreenDelta, LateUpdate, SetTarget, _isAutoTracking`
- ``Assets/Scripts/Game/InGame/InGamePresenter.cs` — existing HandlePiecePlaced with camera targeting block using _stage and _camera`
- ``Assets/Scripts/Game/InGame/CameraMath.cs` — T01 output: ClampToBounds and ComputeBoardRect static methods`
- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — T01 output: BoundaryMargin and ZoomSpeed fields`
- ``Assets/Scripts/Game/InGame/PuzzleStageController.cs` — T01 output: GetBoardRect() method`

## Expected Output

- ``Assets/Scripts/Game/InGame/CameraController.cs` — manual pan override (_isAutoTracking = false in HandleMouse + HandleTouch), scroll wheel zoom, pinch-to-zoom, SetBoardBounds API, ClampToBounds applied in ApplyScreenDelta + zoom handlers + LateUpdate`
- ``Assets/Scripts/Game/InGame/InGamePresenter.cs` — SetBoardBounds call wired in HandlePiecePlaced`

## Verification

grep -c "mouseScrollDelta" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "_isAutoTracking = false" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "SetBoardBounds" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "ClampToBounds" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "GetBoardRect\|SetBoardBounds" Assets/Scripts/Game/InGame/InGamePresenter.cs && echo '{}' | mcporter call unityMCP.refresh_asset_database --stdin
