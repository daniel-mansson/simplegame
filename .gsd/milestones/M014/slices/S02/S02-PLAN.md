# S02: Board — World Space, Camera Pan, Hint Surface

**Goal:** Remove board-parent scaling from InGameSceneController. Add CameraController for orthographic pan. Wire HintSurfaceBuilder to spawn a hint mesh behind the pieces.

**Demo:** Play InGame from editor — board renders at natural scale (parent localScale = (1,1,1)), dragging on the board pans the camera, hint surface outlines are visible behind all pieces.

## Must-Haves

- `_puzzleParent.localScale == Vector3.one` after SpawnPieces
- `_puzzleParent.position == Vector3.zero` (or a sensible world-space anchor)
- Hint surface GameObject exists as child of puzzleParent at z > 0 (behind pieces)
- Dragging on the board (not on a UI element) translates the camera XY
- Dragging on a UI element does NOT pan the camera
- No regression in piece placement, hearts, win/lose flow

## Tasks

- [ ] **T01: Remove board scaling, reframe camera**
  Strip `boardSize`, `parent.localScale = Vector3.one * boardSize`, and all related layout math from `SpawnPieces`. Set board parent to `(1,1,1)` at world origin. Recompute tray slot world positions from camera-relative layout (interim: fixed positions, will be camera-following in S03).

- [ ] **T02: CameraController — orthographic pan**
  New MonoBehaviour `CameraController` on Main Camera. Pointer-down on world (EventSystem.current.IsPointerOverGameObject() == false) → record start. Pointer-drag → delta translate camera XY. No bounds. Wire into InGame scene.

- [ ] **T03: Hint surface spawn**
  In SpawnPieces, after creating piece GameObjects, call `HintSurfaceBuilder.Build(rawBoard.Pieces, thickness: 0.02f)`. Create a child GameObject of puzzleParent, assign MeshFilter + MeshRenderer with default material, set z = +0.1f.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/InGame/CameraController.cs` (new)
- `Assets/Scenes/InGame.unity` (CameraController wired, puzzleParent transform reset)
