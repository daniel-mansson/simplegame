---
estimated_steps: 1
estimated_files: 4
skills_used: []
---

# T01: Add CameraConfig fields, CameraMath boundary helpers, board rect exposure, and EditMode tests

Add BoundaryMargin and ZoomSpeed fields to CameraConfig. Add CameraMath.ClampToBounds (pure static — clamps camera XY to keep viewport within a Rect + margin, centering when viewport exceeds bounds) and CameraMath.ComputeBoardRect (returns Rect from rows/cols matching GridPlanner convention). Add PuzzleStageController.GetBoardRect() exposing _currentGridRows/_currentGridCols. Write 6+ EditMode tests in CameraTests.cs covering ClampToBounds (inside bounds unchanged, beyond right/left clamps, viewport larger than board centers) and ComputeBoardRect (square grid, rectangular grid).

## Inputs

- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — existing ScriptableObject with SmoothTime, MinZoom, MaxZoom, Padding fields`
- ``Assets/Scripts/Game/InGame/CameraMath.cs` — existing pure static class with ComputeFraming method`
- ``Assets/Scripts/Game/InGame/PuzzleStageController.cs` — has private _currentGridRows/_currentGridCols fields set in SpawnLevel`
- ``Assets/Tests/EditMode/Game/CameraTests.cs` — existing 11 tests (CameraMathTests + GetPlaceablePieceIdsTests)`

## Expected Output

- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — two new fields: BoundaryMargin (float, default 0.5f) and ZoomSpeed (float, default 5f)`
- ``Assets/Scripts/Game/InGame/CameraMath.cs` — two new static methods: ClampToBounds and ComputeBoardRect`
- ``Assets/Scripts/Game/InGame/PuzzleStageController.cs` — new public Rect GetBoardRect() method`
- ``Assets/Tests/EditMode/Game/CameraTests.cs` — 6+ new EditMode tests for ClampToBounds and ComputeBoardRect`

## Verification

grep -c "ClampToBounds" Assets/Scripts/Game/InGame/CameraMath.cs && grep -c "ComputeBoardRect" Assets/Scripts/Game/InGame/CameraMath.cs && grep -c "BoundaryMargin" Assets/Scripts/Game/InGame/CameraConfig.cs && grep -c "ZoomSpeed" Assets/Scripts/Game/InGame/CameraConfig.cs && grep -c "GetBoardRect" Assets/Scripts/Game/InGame/PuzzleStageController.cs && grep -c "\[Test\]" Assets/Tests/EditMode/Game/CameraTests.cs
