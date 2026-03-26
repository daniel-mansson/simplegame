---
id: S01
parent: M021
milestone: M021
provides:
  - PuzzleStageController MonoBehaviour with SpawnLevel, LateUpdate tray layout, SpawnSlotButtons, piece tracking
  - MovePieceToTraySlot, ShakePieceInSlot, RevealPiece, ResetPiecesToTray, GetTransitionPlayer all on stage
  - InGameSceneController [SerializeField] _stage reference and model factory calling _stage.SpawnLevel()
  - SceneSetup updated to create PuzzleStageController GameObject and wire all rendering fields
  - InGame.unity scene regenerated with PuzzleStageController wired correctly
key_files:
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Scenes/InGame.unity
key_decisions:
  - "PuzzleStageController._inGameView is [SerializeField] (not runtime-set) so SceneSetup can wire it"
  - "SetContext() takes only popupManager (not inGameView) since inGameView is already serialized"
patterns_established:
  - "Stage MonoBehaviour wired to view via [SerializeField]; popup manager passed via SetContext() at Initialize time"
duration: 30min
verification_result: pass
completed_at: 2026-03-26T14:00:00Z
---

# S01: Extract PuzzleStageController

**All 3D/tray logic moved from InGameSceneController (1085 lines) to PuzzleStageController MonoBehaviour.**

## What Happened

Created `PuzzleStageController` MonoBehaviour with all piece tracking dictionaries, `LateUpdate` tray layout, `SpawnLevel` (renamed from `SpawnPieces`), `SpawnSlotButtons`, `MovePieceToTraySlot`, `ShakePieceInSlot`, `RevealPiece`, `ResetPiecesToTray`, `GetTransitionPlayer`, `CreateRuntimeGridConfig`, `Reset`, and `HasGridLayoutConfig`.

`InGameSceneController` now wires `_stage` via `[SerializeField]` and calls `_stage.SpawnLevel(...)` from the model factory lambda instead of the old local `SpawnPieces(...)`.

SceneSetup updated to create a `PuzzleStageController` GameObject in the InGame scene, wire all rendering fields (`_inGameView`, `_puzzleParent`, `_gridLayoutConfig`, `_pieceRenderConfig`, `_transitionPlayer`) on the stage, and wire `_stage` on the controller.

Needed two SceneSetup re-runs to stabilise (first run executed before the new type compiled).

## Deviations

`_inGameView` on `PuzzleStageController` made `[SerializeField]` rather than runtime-set, so SceneSetup can wire it directly. `SetContext()` consequently takes only `popupManager`.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — new MonoBehaviour, all 3D stage logic
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — 3D logic removed, _stage added
- `Assets/Editor/SceneSetup.cs` — new PuzzleStageController wiring
- `Assets/Scenes/InGame.unity` — regenerated with PuzzleStageController
