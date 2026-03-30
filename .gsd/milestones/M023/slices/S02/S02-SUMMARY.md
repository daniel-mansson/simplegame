---
id: S02
parent: M023
milestone: M023
provides:
  - CameraController.SetBoardBounds(Rect) — registers board bounds for clamping
  - CameraController manual override (drag-pan, scroll-wheel, pinch-to-zoom) with auto-track cancellation
  - CameraMath.ClampToBounds — pure static, reusable by S03 level-start sequence
  - CameraMath.ComputeBoardRect — pure static, consistent with GridPlanner convention
  - PuzzleStageController.GetBoardRect() — exposes live board dimensions for any consumer
requires:
  []
affects:
  - S03 (Level Start Sequence) — can now call CameraMath.ClampToBounds and SetBoardBounds directly; the board rect infrastructure is in place
key_files:
  - Assets/Scripts/Game/InGame/CameraConfig.cs
  - Assets/Scripts/Game/InGame/CameraMath.cs
  - Assets/Scripts/Game/InGame/CameraController.cs
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Tests/EditMode/Game/CameraTests.cs
key_decisions:
  - ClampToBounds centres the camera (rather than edge-snapping) when viewport exceeds board+margin — board always visible at any zoom level
  - ComputeBoardRect uses unitScale=max(rows,cols) matching existing SpawnLevel/LateUpdate convention — no coordinate system mismatch
  - GetBoardRect() floors rows/cols to 1 defensively so it returns a valid rect before SpawnLevel runs
  - SetBoardBounds called once on first HandlePiecePlaced (not at SpawnLevel) — deferred wiring ensures level geometry is ready when bounds are registered
  - Pinch-to-zoom skips single-finger pan when touchCount>=2 to avoid conflicts between the two input modes
patterns_established:
  - All CameraController input handlers follow: check config/camera null → disable auto-tracking → adjust value → clamp zoom → ClampToBounds. This pattern should be followed for any future input type added.
  - Board-rect wiring via a one-time deferred call in HandlePiecePlaced (not constructor or Awake) — useful pattern when geometry isn't available at component init time.
observability_surfaces:
  - CameraController.SetBoardBounds logs the rect via Debug.Log: [CameraController] SetBoardBounds rect=(x,y WxH) — visible in Unity Console on first piece placement
drill_down_paths:
  - Assets/Scripts/Game/InGame/CameraController.cs (T02 primary output)
  - Assets/Scripts/Game/InGame/CameraMath.cs (T01 primary output)
  - Assets/Scripts/Game/InGame/CameraConfig.cs (T01)
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs (T01)
  - Assets/Scripts/Game/InGame/InGamePresenter.cs (T02 wiring)
  - Assets/Tests/EditMode/Game/CameraTests.cs (T01 tests)
duration: ""
verification_result: passed
completed_at: 2026-03-30T13:50:21.734Z
blocker_discovered: false
---

# S02: Manual Input & Boundary Enforcement

**Added drag-pan / scroll-wheel / pinch-to-zoom manual override, board-boundary clamping, and InGamePresenter wiring — all with null-safe guards and auto-tracking cancellation on first manual input.**

## What Happened

S02 delivered two tightly coupled tasks that together close the full manual-input and boundary-enforcement loop.

**T01 — Foundation (CameraConfig, CameraMath, PuzzleStageController, tests)**
`BoundaryMargin` and `ZoomSpeed` were added to `CameraConfig` as serialised fields with sensible defaults (0.5 and 5). Two pure-static helpers were added to `CameraMath`: `ClampToBounds` clamps camera XY so the viewport never drifts outside `boardRect + margin`, and centres the camera when the viewport is larger than the board; `ComputeBoardRect` mirrors the `unitScale = max(rows, cols)` convention already used in `SpawnLevel`/`LateUpdate` so coordinate systems stay in sync. `PuzzleStageController.GetBoardRect()` exposes `_currentGridRows`/`_currentGridCols` with a defensive floor-to-1 guard so a valid rect is returned even before `SpawnLevel` runs. Seven new EditMode tests in `CameraTests.cs` cover: camera-inside-bounds unchanged, clamp-on-right, clamp-on-left, viewport-larger-than-board centres, square `ComputeBoardRect`, and rectangular `ComputeBoardRect`. Total `[Test]` count went from 11 to 18.

**T02 — Integration (CameraController, InGamePresenter)**
`CameraController` gained `_boardRect`/`_hasBoardRect` private state and `SetBoardBounds(Rect)` public API. Manual-input handling was wired end-to-end:
- **Drag-pan:** `_isAutoTracking = false` set when single-finger (touch or mouse) panning begins; `ApplyScreenDelta` calls `ClampToBounds` after repositioning.
- **Scroll-wheel:** `Input.mouseScrollDelta.y` read in `HandleMouse`; if abs > 0.01 and `_config != null`, `_isAutoTracking = false`, orthographic size adjusted by `scroll * ZoomSpeed * Time.deltaTime`, clamped to `[MinZoom, MaxZoom]`, then `ClampToBounds` applied.
- **Pinch-to-zoom:** when `touchCount >= 2`, `_isAutoTracking = false`, previous/current two-finger distance computed, delta drives orthographic-size change scaled by `ZoomSpeed * 0.01f`, clamped, `ClampToBounds` applied. Two-finger active state skips single-finger pan to avoid conflicts.
- **LateUpdate:** `ClampToBounds` applied after each `SmoothDamp` tick so auto-track never exceeds bounds either.
- All `ClampToBounds` call sites are guarded with `_hasBoardRect && _config != null`.

`InGamePresenter` was updated so that on the first `HandlePiecePlaced` call after `_stage` and `_camera` are both non-null, `_camera.SetBoardBounds(_stage.GetBoardRect())` is called once to register the board rect. This deferred wiring means bounds are only set when the level geometry is actually ready.

The verification gate originally used `rg` which fails on Windows with OS error 123 for directory paths (K012). All checks were re-run with `grep -c` and passed: `mouseScrollDelta` (1), `_isAutoTracking = false` (4), `SetBoardBounds` (2), `ClampToBounds` (4) in CameraController; `GetBoardRect|SetBoardBounds` (1) in InGamePresenter.

## Verification

All T01 and T02 grep checks passed with `grep -c`:
- `mouseScrollDelta` in CameraController.cs → 1 ✅
- `_isAutoTracking = false` in CameraController.cs → 4 ✅
- `SetBoardBounds` in CameraController.cs → 2 ✅
- `ClampToBounds` in CameraController.cs → 4 ✅
- `GetBoardRect|SetBoardBounds` in InGamePresenter.cs → 1 ✅
- `ClampToBounds` in CameraMath.cs → present ✅
- `ComputeBoardRect` in CameraMath.cs → present ✅
- `BoundaryMargin` in CameraConfig.cs → 1 ✅
- `ZoomSpeed` in CameraConfig.cs → 1 ✅
- `GetBoardRect` in PuzzleStageController.cs → present ✅
- `[Test]` count in CameraTests.cs → 18 (was 11, +7) ✅

## Requirements Advanced

- R060 — Manual camera override (drag/scroll/pinch) and boundary enforcement are now part of the in-game puzzle interaction layer, advancing R060 gameplay completeness

## Requirements Validated

None.

## New Requirements Surfaced

None.

## Requirements Invalidated or Re-scoped

None.

## Deviations

The verification commands in the slice plan used `grep` which is the correct tool on Windows (rg fails with OS error 123 on directory paths — see K012). All verification logic was identical; only the tool invocation changed.

## Known Limitations

SetBoardBounds is called once on the first piece placement. If SpawnLevel is ever called mid-session (e.g. level restart), GetBoardRect would return updated dimensions but SetBoardBounds would not be called again (the one-time guard prevents it). This is fine for M023 scope but should be revisited if level-restart support is added.

## Follow-ups

S03 (Level Start Sequence) can now rely on SetBoardBounds being set — it should call it at SpawnLevel time (before piece placement) to ensure the overview camera respects bounds from the start.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/CameraConfig.cs` — Added BoundaryMargin (float, default 0.5) and ZoomSpeed (float, default 5) serialised fields
- `Assets/Scripts/Game/InGame/CameraMath.cs` — Added ClampToBounds (clamps camera XY to board rect + margin, centres when viewport exceeds bounds) and ComputeBoardRect (Rect from rows/cols using unitScale=max(rows,cols))
- `Assets/Scripts/Game/InGame/CameraController.cs` — Added _boardRect/_hasBoardRect state, SetBoardBounds API, scroll-wheel zoom in HandleMouse, pinch-to-zoom in HandleTouch, _isAutoTracking=false on all manual input starts, ClampToBounds in ApplyScreenDelta and LateUpdate
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — Added GetBoardRect() exposing _currentGridRows/_currentGridCols with floor-to-1 defensive guard
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — Added one-time SetBoardBounds call in HandlePiecePlaced when _stage and _camera are non-null
- `Assets/Tests/EditMode/Game/CameraTests.cs` — Added 7 new EditMode tests for ClampToBounds (4 cases) and ComputeBoardRect (3 cases), total [Test] count now 18
