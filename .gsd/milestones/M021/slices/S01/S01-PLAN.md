# S01: Extract PuzzleStageController

**Goal:** Move all 3D piece/tray logic from InGameSceneController into a new PuzzleStageController MonoBehaviour. InGameSceneController wires it via [SerializeField] and calls its public API.

**Demo:** InGame scene plays identically — pieces spawn, tray positions, slot buttons work, shake/reveal animations run. PuzzleStageController is a self-contained MonoBehaviour.

## Must-Haves

- `PuzzleStageController.cs` exists in `Assets/Scripts/Game/InGame/`
- All 3D/tray fields, LateUpdate, SpawnPieces/SpawnSlotButtons, MovePieceToTraySlot, ShakePieceInSlot, RevealPiece, ResetPiecesToTray, GetTransitionPlayer live in PuzzleStageController
- PuzzleStageController public API: `SpawnLevel(board, seedId, slotCount, deckOrder, gridCols)`, `Reset()`, `GetTransitionPlayer()`, `ResetPiecesToTray()`
- InGameSceneController loses all 3D/tray fields and methods; gains `[SerializeField] PuzzleStageController _stage`
- InGameSceneController model factory lambda calls `_stage.SpawnLevel(...)` instead of local `SpawnPieces(...)`
- `InGameView.RegisterPieceCallbacks` is called from `PuzzleStageController.SpawnLevel` (not from InGameSceneController)
- InGameSceneController `LateUpdate` is gone (moved to PuzzleStageController)
- All existing EditMode tests still pass (no test touches 3D stage logic directly)

## Tasks

- [ ] **T01: Create PuzzleStageController**
  Extract all 3D/tray fields and methods from InGameSceneController into PuzzleStageController. Wire via [SerializeField] on InGameSceneController. Update model factory lambda.

## Files Likely Touched
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` (new)
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` (remove 3D logic, add [SerializeField] _stage)
