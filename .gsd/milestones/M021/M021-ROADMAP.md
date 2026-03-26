# M021: Scene Controller Composition Refactor

**Vision:** Scene controllers become thin wiring boards — `[SerializeField]` fields connecting views to presenters, plus `Initialize()` and `RunAsync()` delegation. All business logic moves to presenters or dedicated MonoBehaviour components.

## Success Criteria

- `InGameSceneController` is ≤80 lines with no business logic inline
- `MainMenuSceneController` is ≤80 lines with no business logic inline
- `PuzzleStageController` MonoBehaviour owns all 3D piece/tray logic extracted from InGameSceneController
- `InGameFlowPresenter` pure C# class owns the gameplay loop + popup orchestration extracted from InGameSceneController
- All existing EditMode tests pass
- Full game session (boot → menu → play → win → menu) works end-to-end in editor

## Key Risks / Unknowns

- `InGameView.RegisterPieceCallbacks` seam — must migrate cleanly from controller to `PuzzleStageController` without breaking view/presenter contract
- Test seams (`SetViewsForTesting`, `SetModelFactory`, `SetWinPopupDelay`) must move to `InGameFlowPresenter` — InGameTests.cs calls them on the controller today

## Proof Strategy

- Callback seam migration → retire in S01 by running InGame scene end-to-end with pieces spawning and tray working
- Test seam migration → retire in S02 by running all InGameTests with new presenter-based seams

## Verification Classes

- Contract verification: `wc -l` on scene controllers; all EditMode tests pass
- Integration verification: play through boot → menu → ingame → win → menu in editor
- Operational verification: play-from-editor directly from InGame scene (BootInjector + self-bootstrap path)
- UAT / human verification: visual check that tray layout, piece placement, and popup flows look correct

## Milestone Definition of Done

This milestone is complete only when all are true:

- `InGameSceneController` ≤80 lines
- `MainMenuSceneController` ≤80 lines
- `PuzzleStageController.cs` exists with all 3D/tray logic
- `InGameFlowPresenter.cs` exists with gameplay loop + popup orchestration
- All EditMode tests pass
- Boot → menu → ingame → win → menu round-trip verified in editor

## Requirement Coverage

- Covers: R001 (MVP separation), R002 (view independence)
- Partially covers: none
- Leaves for later: none
- Orphan risks: none

## Slices

- [x] **S01: Extract PuzzleStageController** `risk:medium` `depends:[]`
  > After this: 3D pieces spawn, tray layout runs, slot buttons work — InGame scene plays identically; PuzzleStageController is a self-contained MonoBehaviour wired via SerializeField on InGameSceneController

- [x] **S02: Extract InGameFlowPresenter** `risk:medium` `depends:[S01]`
  > After this: win/lose/retry/popup flows work; InGameSceneController is ≤80 lines; all InGameTests pass against the new presenter seams

- [x] **S03: Slim MainMenuSceneController** `risk:low` `depends:[]`
  > After this: main menu plays identically; MainMenuSceneController is ≤80 lines; SceneControllerTests pass

- [x] **S04: Wire, verify, and commit** `risk:low` `depends:[S01,S02,S03]`
  > After this: all tests pass, all three scene controllers meet the line-count target, full game session verified end-to-end in editor

## Boundary Map

### S01 → S02

Produces:
- `PuzzleStageController` MonoBehaviour — `SpawnLevel(board, seedId, slotCount, deckOrder, gridCols)`, `Reset()`, `GetTransitionPlayer()` as public API
- Callbacks still delivered to `InGameView` via `RegisterPieceCallbacks` — wiring moved into `PuzzleStageController.SpawnLevel`
- `InGameSceneController` retains `[SerializeField] PuzzleStageController _stage` but no 3D logic

Consumes:
- nothing (first 3D extraction slice)

### S02 → S04

Produces:
- `InGameFlowPresenter` pure C# class — constructor takes all service deps + `PuzzleStageController` ref; exposes `RunAsync(CancellationToken)`, `SetViewsForTesting()`, `SetModelFactory()`, `SetWinPopupDelay()`
- `InGameSceneController` reduced to ≤80 lines: `[SerializeField]` fields + `Initialize()` storing deps + `RunAsync()` delegating to `InGameFlowPresenter`

Consumes from S01:
- `PuzzleStageController` public API — `SpawnLevel()`, `Reset()`, `GetTransitionPlayer()`

### S03 → S04

Produces:
- `MainMenuSceneController` ≤80 lines: `[SerializeField]` fields + `Initialize()` + `RunAsync()` delegating to `MainMenuPresenter`
- Environment resolution and screen manager construction moved to `Initialize()` (simple, no separate class needed)
- Debug ad methods removed from controller — debug flow handled directly in `MainMenuPresenter` via existing action dispatch

Consumes:
- nothing (independent of S01/S02)

### S04

Produces:
- All tests passing
- Verified end-to-end game session

Consumes from S01, S02, S03:
- All produced components assembled and verified together
