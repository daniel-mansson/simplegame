# T01: Create PuzzleStageController

**Slice:** S01
**Milestone:** M021

## Goal
Extract all 3D piece/tray logic from InGameSceneController into a new PuzzleStageController MonoBehaviour, leaving InGameSceneController with only service/game-loop fields and a [SerializeField] reference to the stage.

## Must-Haves

### Truths
- `PuzzleStageController` compiles cleanly with all 3D/tray state and methods
- `InGameSceneController` has no `LateUpdate`, no `SpawnPieces`, no `SpawnSlotButtons`, no piece tracking dictionaries
- Model factory lambda in `InGameSceneController.RunAsync` calls `_stage.SpawnLevel(...)` 
- `InGameView.RegisterPieceCallbacks` is called from `PuzzleStageController.SpawnLevel`
- All EditMode tests pass

### Artifacts
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — new MonoBehaviour, min 200 lines, all 3D/tray logic

### Key Links
- `InGameSceneController._stage` → `PuzzleStageController` via `[SerializeField]`
- `InGameSceneController.RunAsync` model factory → `_stage.SpawnLevel()`
- `PuzzleStageController.SpawnLevel` → `InGameView.RegisterPieceCallbacks`

## Steps
1. Create `PuzzleStageController.cs` with all SerializeField stage fields (gridLayoutConfig, pieceRenderConfig, puzzleParent, transitionPlayer, puzzleSeed, seedPieceId)
2. Move all piece tracking private fields (_spawnedPieces, _pieceObjects, _solvedWorldPositions, _traySlotData, _initialTrayData, _traySlotPositions, _traySlotScales, _currentGridRows, _currentGridCols, _shakingPieces, _slotButtons, _slotButtonCanvas, _runtimeGridConfig)
3. Move LateUpdate verbatim — it references _inGameView so add `[SerializeField] InGameView _inGameView` to PuzzleStageController (or accept InGameView reference via SpawnLevel)
4. Move SpawnPieces (rename to SpawnLevel with public signature), SpawnSlotButtons, MovePieceToTraySlot, ShakePieceInSlot, ShakePieceAsync, RevealPiece, ResetPiecesToTray, GetTransitionPlayer
5. Add `public void Reset()` that calls ResetPiecesToTray and destroys any _runtimeGridConfig
6. Update InGameSceneController: remove moved fields/methods, add `[SerializeField] PuzzleStageController _stage`, update model factory lambda to call `_stage.SpawnLevel(...)`
7. Update InGameSceneController model factory to pass `_inGameView` reference to stage (or stage gets it via its own [SerializeField])
8. Verify LSP diagnostics clean — fix any compile errors
9. Run EditMode tests via mcporter stdin workaround (K006)

## Context
- LateUpdate references `_inGameView` for slot contents and `_popupManager` for HasActivePopup check — PuzzleStageController gets its own [SerializeField] InGameView _inGameView; popupManager passed via Initialize or property
- SpawnSlotButtons references `_inGameView` (for GetSlotContents) and `_popupManager` (for HasActivePopup in onClick) and casts to InGameView — stage needs access to both
- `_runtimeGridConfig` is created/destroyed in RunAsync; move lifecycle to SpawnLevel (create) and Reset (destroy)
- Keep NullGoldenPieceService and BuildStubModel in InGameSceneController for now — they belong to the game loop, not the stage
- The `_debugOverride` field stays in InGameSceneController (it affects RunAsync logic, not rendering)
