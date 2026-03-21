# M014: Puzzle Controls & Layout Redesign

**Vision:** Pieces sized at longest-edge = 1 unit; board in world space with camera pan; hint surface behind pieces; tray follows camera with UGUI Button slot input and full tween animations.

## Success Criteria

- A 4×2 board generates pieces with longest edge = 1 unit; board extent = 2×2 world units
- A 3×3 board generates pieces with longest edge = 1 unit; board extent = 3×3 world units
- All existing package tests pass (updated for new sizing)
- InGame scene: board renders at natural scale (puzzleParent.localScale = Vector3.one)
- Camera pans smoothly by dragging on the board
- Hint surface visible behind all pieces on the board
- Tray 3D pieces and UGUI button overlays follow camera every frame
- Slot button tap (pointer-up-inside) triggers piece placement with full tween
- Board drag and slot tap do not conflict

## Key Risks / Unknowns

- GridPlanner test suite is tightly coupled to 1/n sizing — all test assertions need rewriting
- Board parent scaling removal ripples through SpawnPieces layout math in InGameSceneController
- Screen-space button overlay positioning must stay aligned with 3D pieces every frame

## Proof Strategy

- Sizing correctness → retire in S01 by asserting cell dimensions in GridPlannerTests
- Layout correctness → retire in S02 by verifying board parent scale = (1,1,1) and hint surface exists
- Input separation → retire in S03 by verifying UGUI buttons fire and drag pans camera independently

## Verification Classes

- Contract verification: EditMode tests for GridPlanner sizing, BoardFactory SolvedPosition values
- Integration verification: Play-from-editor InGame scene — board at natural scale, camera pans, hint visible, tray follows, slot buttons place pieces with tween
- Operational verification: none
- UAT / human verification: visual confirmation that camera pan feels natural, tray stays at screen bottom

## Milestone Definition of Done

This milestone is complete only when all are true:

- GridPlanner produces cells with longest edge = 1 unit for all grid shapes
- All package EditMode tests pass with updated assertions
- InGame scene: board parent localScale = (1,1,1)
- Hint surface rendered behind pieces (z > 0)
- Camera pan works on board drag, does not fire on slot button press
- Tray 3D pieces reposition to camera-bottom each frame
- UGUI slot buttons fire placement + tween on pointer-up-inside
- No regression in existing gameplay (piece placement, hearts, win/lose)

## Requirement Coverage

- Covers: R034, R035, R036, R037, R038, R039, R040, R041
- Partially covers: none
- Leaves for later: R042 (hint surface styling)
- Orphan risks: none

## Slices

- [ ] **S01: Package — Unit-Scale Pieces** `risk:medium` `depends:[]`
  > After this: EditMode test asserts a 4×2 board has cells with longest edge = 1 unit and board extent = 2×2.

- [ ] **S02: Board — World Space, Camera Pan, Hint Surface** `risk:high` `depends:[S01]`
  > After this: Play InGame from editor — board at natural scale, camera pans by dragging, hint surface visible behind pieces.

- [ ] **S03: Tray — Camera-Following Slots + UGUI Buttons** `risk:medium` `depends:[S02]`
  > After this: Play InGame — tray follows camera pan, slot buttons place pieces with full tween, no gesture conflict.

## Boundary Map

### S01 → S02

Produces:
- `GridPlanner.ComputeCells` — cells where `Mathf.Max(cell.width, cell.height) == 1f` (within tolerance)
- `PieceDescriptor.SolvedPosition` — world positions in the new unit-scale space
- Mesh vertices centered on SolvedPosition in unit-scale space
- Updated `GridPlannerTests` asserting new sizing contract

Consumes:
- nothing (package-internal change)

### S02 → S03

Produces:
- `puzzleParent` at `localScale = (1,1,1)`, `position = (0, 0, 0)`
- `CameraController` MonoBehaviour — pan on board drag
- Hint surface GameObject parented to board at `z = +0.1f`
- Tray slot world positions derived from camera bottom (computed in SpawnPieces, updated in LateUpdate)

Consumes from S01:
- Unit-scale `SolvedPosition` values for board piece placement
- Unit-scale board extent (rows × cols in world units) for camera framing

### S03 → (done)

Produces:
- `TrayFollower` behaviour — updates 3D slot piece positions each LateUpdate
- Screen Space Overlay Canvas with one Button per slot
- Button positions projected from world-to-screen each LateUpdate
- `PieceTapHandler` removed from tray pieces; slot input entirely through UGUI
- Full tween animations (SlideToSlot, PlaceOnBoard, ShakePiece) verified working
