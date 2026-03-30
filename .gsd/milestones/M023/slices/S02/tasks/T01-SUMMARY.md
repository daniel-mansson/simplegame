---
id: T01
parent: S02
milestone: M023
provides: []
requires: []
affects: []
key_files: ["Assets/Scripts/Game/InGame/CameraConfig.cs", "Assets/Scripts/Game/InGame/CameraMath.cs", "Assets/Scripts/Game/InGame/PuzzleStageController.cs", "Assets/Tests/EditMode/Game/CameraTests.cs"]
key_decisions: ["ClampToBounds centres the camera on the board when the viewport exceeds board+margin (rather than snapping to one edge), ensuring the board stays visible in all zoom levels", "ComputeBoardRect uses the same unitScale=max(rows,cols) formula already used in SpawnLevel/LateUpdate to keep coordinate conventions consistent", "GetBoardRect() treats 0-dimension defensively (floor to 1) so it returns a valid rect even before SpawnLevel runs"]
patterns_established: []
drill_down_paths: []
observability_surfaces: []
duration: ""
verification_result: "Ran grep checks for all 6 required symbols: ClampToBounds (1), ComputeBoardRect (2), BoundaryMargin (1), ZoomSpeed (1), GetBoardRect (1), [Test] count 18 (was 11, +7 new). All passed exit code 0."
completed_at: 2026-03-30T13:45:25.498Z
blocker_discovered: false
---

# T01: Added BoundaryMargin/ZoomSpeed to CameraConfig, ClampToBounds/ComputeBoardRect to CameraMath, GetBoardRect() to PuzzleStageController, and 7 new EditMode tests

> Added BoundaryMargin/ZoomSpeed to CameraConfig, ClampToBounds/ComputeBoardRect to CameraMath, GetBoardRect() to PuzzleStageController, and 7 new EditMode tests

## What Happened
---
id: T01
parent: S02
milestone: M023
key_files:
  - Assets/Scripts/Game/InGame/CameraConfig.cs
  - Assets/Scripts/Game/InGame/CameraMath.cs
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
  - Assets/Tests/EditMode/Game/CameraTests.cs
key_decisions:
  - ClampToBounds centres the camera on the board when the viewport exceeds board+margin (rather than snapping to one edge), ensuring the board stays visible in all zoom levels
  - ComputeBoardRect uses the same unitScale=max(rows,cols) formula already used in SpawnLevel/LateUpdate to keep coordinate conventions consistent
  - GetBoardRect() treats 0-dimension defensively (floor to 1) so it returns a valid rect even before SpawnLevel runs
duration: ""
verification_result: passed
completed_at: 2026-03-30T13:45:25.498Z
blocker_discovered: false
---

# T01: Added BoundaryMargin/ZoomSpeed to CameraConfig, ClampToBounds/ComputeBoardRect to CameraMath, GetBoardRect() to PuzzleStageController, and 7 new EditMode tests

**Added BoundaryMargin/ZoomSpeed to CameraConfig, ClampToBounds/ComputeBoardRect to CameraMath, GetBoardRect() to PuzzleStageController, and 7 new EditMode tests**

## What Happened

CameraConfig received two new serialized fields: BoundaryMargin (float, default 0.5f) and ZoomSpeed (float, default 5f). CameraMath received two new pure static methods: ClampToBounds (clamps camera XY to keep viewport within a Rect+margin, centres the camera when viewport exceeds board) and ComputeBoardRect (returns a Rect centred on the origin using the same unitScale=max(rows,cols) convention as the existing SpawnLevel/LateUpdate code). PuzzleStageController.GetBoardRect() is a thin wrapper that reads _currentGridRows/_currentGridCols (stored during SpawnLevel) and delegates to CameraMath.ComputeBoardRect with defensive floor of 1. CameraTests.cs gained a new CameraClampAndBoardRectTests fixture with 7 tests covering all required scenarios.

## Verification

Ran grep checks for all 6 required symbols: ClampToBounds (1), ComputeBoardRect (2), BoundaryMargin (1), ZoomSpeed (1), GetBoardRect (1), [Test] count 18 (was 11, +7 new). All passed exit code 0.

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `grep -c "ClampToBounds" Assets/Scripts/Game/InGame/CameraMath.cs` | 0 | ✅ pass | 50ms |
| 2 | `grep -c "ComputeBoardRect" Assets/Scripts/Game/InGame/CameraMath.cs` | 0 | ✅ pass | 45ms |
| 3 | `grep -c "BoundaryMargin" Assets/Scripts/Game/InGame/CameraConfig.cs` | 0 | ✅ pass | 40ms |
| 4 | `grep -c "ZoomSpeed" Assets/Scripts/Game/InGame/CameraConfig.cs` | 0 | ✅ pass | 40ms |
| 5 | `grep -c "GetBoardRect" Assets/Scripts/Game/InGame/PuzzleStageController.cs` | 0 | ✅ pass | 45ms |
| 6 | `grep -c "\[Test\]" Assets/Tests/EditMode/Game/CameraTests.cs` | 0 | ✅ pass (18 ≥ 17 required) | 40ms |


## Deviations

None.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/CameraConfig.cs`
- `Assets/Scripts/Game/InGame/CameraMath.cs`
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs`
- `Assets/Tests/EditMode/Game/CameraTests.cs`


## Deviations
None.

## Known Issues
None.
