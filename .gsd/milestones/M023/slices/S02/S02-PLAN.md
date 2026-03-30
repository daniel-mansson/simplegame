# S02: Manual Input & Boundary Enforcement

**Goal:** Player can drag to pan and pinch/scroll to zoom, overriding auto-track. Zoom clamped to configured limits. Camera can't drift beyond board bounds + margin. Next placement resumes auto-tracking.
**Demo:** After this: After this: player can drag to pan and pinch/scroll to zoom, overriding auto-track. Zoom clamped to configured limits. Camera can't drift beyond board bounds + margin. Next placement resumes auto-tracking.

## Tasks
- [x] **T01: Added BoundaryMargin/ZoomSpeed to CameraConfig, ClampToBounds/ComputeBoardRect to CameraMath, GetBoardRect() to PuzzleStageController, and 7 new EditMode tests** — Add BoundaryMargin and ZoomSpeed fields to CameraConfig. Add CameraMath.ClampToBounds (pure static — clamps camera XY to keep viewport within a Rect + margin, centering when viewport exceeds bounds) and CameraMath.ComputeBoardRect (returns Rect from rows/cols matching GridPlanner convention). Add PuzzleStageController.GetBoardRect() exposing _currentGridRows/_currentGridCols. Write 6+ EditMode tests in CameraTests.cs covering ClampToBounds (inside bounds unchanged, beyond right/left clamps, viewport larger than board centers) and ComputeBoardRect (square grid, rectangular grid).
  - Estimate: 25m
  - Files: Assets/Scripts/Game/InGame/CameraConfig.cs, Assets/Scripts/Game/InGame/CameraMath.cs, Assets/Scripts/Game/InGame/PuzzleStageController.cs, Assets/Tests/EditMode/Game/CameraTests.cs
  - Verify: grep -c "ClampToBounds" Assets/Scripts/Game/InGame/CameraMath.cs && grep -c "ComputeBoardRect" Assets/Scripts/Game/InGame/CameraMath.cs && grep -c "BoundaryMargin" Assets/Scripts/Game/InGame/CameraConfig.cs && grep -c "ZoomSpeed" Assets/Scripts/Game/InGame/CameraConfig.cs && grep -c "GetBoardRect" Assets/Scripts/Game/InGame/PuzzleStageController.cs && grep -c "\[Test\]" Assets/Tests/EditMode/Game/CameraTests.cs
- [ ] **T02: Wire manual pan override, scroll/pinch zoom, and boundary clamping into CameraController** — Integrate all manual input handling and boundary enforcement into CameraController, then wire board bounds from InGamePresenter.

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
  - Estimate: 30m
  - Files: Assets/Scripts/Game/InGame/CameraController.cs, Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Verify: grep -c "mouseScrollDelta" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "_isAutoTracking = false" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "SetBoardBounds" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "ClampToBounds" Assets/Scripts/Game/InGame/CameraController.cs && grep -c "GetBoardRect\|SetBoardBounds" Assets/Scripts/Game/InGame/InGamePresenter.cs && echo '{}' | mcporter call unityMCP.refresh_asset_database --stdin
