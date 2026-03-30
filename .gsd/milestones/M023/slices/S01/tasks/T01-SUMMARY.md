---
id: T01
parent: S01
milestone: M023
provides: []
requires: []
affects: []
key_files: ["Assets/Scripts/Game/InGame/CameraConfig.cs", "Assets/Scripts/Game/InGame/CameraMath.cs", "Assets/Scripts/Puzzle/PuzzleModel.cs", "Assets/Scripts/Game/InGame/PuzzleStageController.cs"]
key_decisions: ["CameraConfig uses [SerializeField] public fields (not properties) to match Unity inspector conventions and allow ScriptableObject serialization", "CameraMath.ComputeFraming returns (Vector3.zero, minZoom) for empty input as board-center fallback", "_pieces stored as IReadOnlyList<IPuzzlePiece> in PuzzleModel to preserve existing constructor contract", "CameraMath is a pure static class with no MonoBehaviour dependency, making it safe for EditMode tests in T03"]
patterns_established: []
drill_down_paths: []
observability_surfaces: []
duration: ""
verification_result: "Ran all four ripgrep checks from the plan verification command: class CameraConfig, ComputeFraming, GetPlaceablePieceIds, GetSolvedPosition — all returned exactly one match with exit code 0."
completed_at: 2026-03-30T11:03:22.066Z
blocker_discovered: false
---

# T01: Created CameraConfig ScriptableObject, CameraMath framing helper, and added GetPlaceablePieceIds/GetSolvedPosition query methods that T02/T03 consume

> Created CameraConfig ScriptableObject, CameraMath framing helper, and added GetPlaceablePieceIds/GetSolvedPosition query methods that T02/T03 consume

## What Happened
---
id: T01
parent: S01
milestone: M023
key_files:
  - Assets/Scripts/Game/InGame/CameraConfig.cs
  - Assets/Scripts/Game/InGame/CameraMath.cs
  - Assets/Scripts/Puzzle/PuzzleModel.cs
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
key_decisions:
  - CameraConfig uses [SerializeField] public fields (not properties) to match Unity inspector conventions and allow ScriptableObject serialization
  - CameraMath.ComputeFraming returns (Vector3.zero, minZoom) for empty input as board-center fallback
  - _pieces stored as IReadOnlyList<IPuzzlePiece> in PuzzleModel to preserve existing constructor contract
  - CameraMath is a pure static class with no MonoBehaviour dependency, making it safe for EditMode tests in T03
duration: ""
verification_result: passed
completed_at: 2026-03-30T11:03:22.066Z
blocker_discovered: false
---

# T01: Created CameraConfig ScriptableObject, CameraMath framing helper, and added GetPlaceablePieceIds/GetSolvedPosition query methods that T02/T03 consume

**Created CameraConfig ScriptableObject, CameraMath framing helper, and added GetPlaceablePieceIds/GetSolvedPosition query methods that T02/T03 consume**

## What Happened

Four deliverables implemented: (1) CameraConfig.cs — new ScriptableObject with [CreateAssetMenu] and four [SerializeField] public fields (SmoothTime=1.2, MinZoom=2, MaxZoom=15, Padding=1.5). (2) CameraMath.cs — static class with ComputeFraming that computes bounding-box center and clamped orthoSize from a list of world positions, with (Vector3.zero, minZoom) fallback for empty input. (3) PuzzleModel.cs — added _pieces private field stored from constructor, and GetPlaceablePieceIds() filtering by !IsPlaced && CanPlace. (4) PuzzleStageController.cs — added GetSolvedPosition(int) doing a null-safe lookup in the existing _solvedWorldPositions dict.

## Verification

Ran all four ripgrep checks from the plan verification command: class CameraConfig, ComputeFraming, GetPlaceablePieceIds, GetSolvedPosition — all returned exactly one match with exit code 0.

## Verification Evidence

| # | Command | Exit Code | Verdict | Duration |
|---|---------|-----------|---------|----------|
| 1 | `rg "class CameraConfig" Assets/Scripts/` | 0 | ✅ pass | 80ms |
| 2 | `rg "ComputeFraming" Assets/Scripts/Game/InGame/CameraMath.cs` | 0 | ✅ pass | 60ms |
| 3 | `rg "GetPlaceablePieceIds" Assets/Scripts/Puzzle/PuzzleModel.cs` | 0 | ✅ pass | 60ms |
| 4 | `rg "GetSolvedPosition" Assets/Scripts/Game/InGame/PuzzleStageController.cs` | 0 | ✅ pass | 60ms |


## Deviations

None. The _solvedWorldPositions dict already existed in PuzzleStageController, so GetSolvedPosition needed only a null-safe wrapper rather than any new infrastructure.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/CameraConfig.cs`
- `Assets/Scripts/Game/InGame/CameraMath.cs`
- `Assets/Scripts/Puzzle/PuzzleModel.cs`
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs`


## Deviations
None. The _solvedWorldPositions dict already existed in PuzzleStageController, so GetSolvedPosition needed only a null-safe wrapper rather than any new infrastructure.

## Known Issues
None.
