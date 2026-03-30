---
estimated_steps: 5
estimated_files: 4
skills_used: []
---

# T01: Create CameraConfig SO, CameraMath helper, and expose domain/stage queries

Create all foundational types needed by the auto-tracking camera. This task produces the config, math, and data-access layer that T02 and T03 consume.

**CameraConfig ScriptableObject** — new `Assets/Scripts/Game/InGame/CameraConfig.cs`. Fields: `float SmoothTime = 1.2f`, `float MinZoom = 2f`, `float MaxZoom = 15f`, `float Padding = 1.5f`. All public with `[SerializeField]`. CreateAssetMenu attribute for editor creation.

**CameraMath static helper** — new `Assets/Scripts/Game/InGame/CameraMath.cs`. Static method `ComputeFraming(IReadOnlyList<Vector3> positions, float padding, float aspect, float minZoom, float maxZoom) → (Vector3 center, float orthoSize)`. Computes min/max X and Y of positions, adds padding, computes required orthoSize as `max((maxY - minY + 2*padding) / 2, (maxX - minX + 2*padding) / (2 * aspect))`, clamps to min/max zoom. Returns board center (0,0) fallback if positions is empty.

**PuzzleModel.GetPlaceablePieceIds()** — add public method to `Assets/Scripts/Puzzle/PuzzleModel.cs`. Stores the original pieces list in a private field `_pieces`. Iterates all piece IDs, filters by `!_board.IsPlaced(id) && _board.CanPlace(id)`. Returns `IReadOnlyList<int>`.

**PuzzleStageController.GetSolvedPosition()** — add public method to `Assets/Scripts/Game/InGame/PuzzleStageController.cs`: `public Vector3? GetSolvedPosition(int pieceId)`. Returns the world position from `_solvedWorldPositions` dict, or null if not found.

## Inputs

- ``Assets/Scripts/Puzzle/PuzzleModel.cs` — add GetPlaceablePieceIds method; needs access to _board.IsPlaced and _board.CanPlace`
- ``Assets/Scripts/Puzzle/PuzzleBoard.cs` — reference for IsPlaced method signature`
- ``Assets/Scripts/Game/InGame/PuzzleStageController.cs` — add GetSolvedPosition method accessing _solvedWorldPositions dict`
- ``Assets/Scripts/Game/InGame/CameraController.cs` — reference for namespace and assembly placement`

## Expected Output

- ``Assets/Scripts/Game/InGame/CameraConfig.cs` — new ScriptableObject with SmoothTime, MinZoom, MaxZoom, Padding fields`
- ``Assets/Scripts/Game/InGame/CameraConfig.cs.meta` — Unity meta file auto-generated`
- ``Assets/Scripts/Game/InGame/CameraMath.cs` — new static class with ComputeFraming method`
- ``Assets/Scripts/Game/InGame/CameraMath.cs.meta` — Unity meta file auto-generated`
- ``Assets/Scripts/Puzzle/PuzzleModel.cs` — modified with GetPlaceablePieceIds() and _pieces field`
- ``Assets/Scripts/Game/InGame/PuzzleStageController.cs` — modified with GetSolvedPosition() method`

## Verification

rg "class CameraConfig" Assets/Scripts/ && rg "ComputeFraming" Assets/Scripts/Game/InGame/CameraMath.cs && rg "GetPlaceablePieceIds" Assets/Scripts/Puzzle/PuzzleModel.cs && rg "GetSolvedPosition" Assets/Scripts/Game/InGame/PuzzleStageController.cs
