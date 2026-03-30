# M023: In-Game Camera Movement

**Gathered:** 2026-03-30
**Status:** Ready for planning

## Project Description

A Unity mobile jigsaw puzzle game where players place pieces from deck slots onto a board using a neighbour-adjacency rule. The game has 22 completed milestones covering domain model, UI, monetisation, and distribution.

## Why This Milestone

On larger boards (level 10+: 8×7 = 56 pieces), valid placement targets can be off-screen. Currently the camera is static at level start and only responds to manual drag. Players must manually hunt for where to place pieces. An auto-tracking camera that smoothly frames the valid placement region after each move improves gameplay flow and makes larger puzzles feel guided rather than frustrating.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Start a level and see the full board overview, then watch the camera smoothly zoom into the first valid placement area
- Place a piece and see the camera glide (~1–1.5s) to frame all currently valid placement positions
- Drag to pan or pinch/scroll to zoom, overriding auto-tracking; camera stays in manual mode until the next placement
- Never see the camera drift beyond the board edges (with small margin)

### Entry point / environment

- Entry point: Play the InGame scene
- Environment: Unity Editor (play mode) and device builds
- Live dependencies involved: none

## Completion Class

- Contract complete means: camera auto-tracks to valid placements, zoom/pan input works, boundary clamping holds, all 347+ existing tests pass
- Integration complete means: CameraController integrates with PuzzleModel events and PuzzleStageController position data without breaking the existing tray layout, deck view, or piece tweens
- Operational complete means: none (no services or lifecycle concerns)

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Play a level from start to finish: camera shows full board overview, transitions to first valid area, then auto-tracks after each placement with smooth movement
- Drag to pan mid-level, then place a piece — camera smoothly resumes auto-tracking
- Pinch/scroll to zoom — zoom limits enforced, boundary clamping holds
- Play a large board (level 10+, 8×7) — camera correctly frames distant valid targets without showing excessive dead space

## Risks and Unknowns

- **Bounding box computation for valid targets** — requires bridging PuzzleModel slot state, PuzzleBoard.CanPlace(), and PuzzleStageController._solvedWorldPositions. The _solvedWorldPositions dictionary is private; need to expose or compute positions externally.
- **LateUpdate tray repositioning interaction** — PuzzleStageController.LateUpdate repositions tray pieces relative to camera position. Moving the camera changes tray positions, which are camera-relative. The deck view is world-space and already parented to follow the camera, so this should be fine, but needs verification.
- **Extreme aspect ratios** — very wide or very tall screens could make the "all valid moves visible" constraint hard to satisfy within zoom limits.

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/InGame/CameraController.cs` — existing drag-pan controller on the orthographic camera. Handles mouse and touch input, IsOverUI guard. This gets extended/replaced.
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — owns `_solvedWorldPositions` (piece ID → world Vector3), `_currentGridRows`, `_currentGridCols`. LateUpdate repositions tray pieces relative to camera.
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — domain model. `OnPiecePlaced` event. `GetSlot(i)` returns current piece ID per slot.
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` — `CanPlace(pieceId)` checks neighbour adjacency. `PlacedIds` returns placed set.
- `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — game loop presenter. Creates PuzzleModel, subscribes InGamePresenter.
- `Assets/Scripts/Game/InGame/DeckView.cs` — world-space deck canvas, already camera-parented (follows camera movement naturally).
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/GridPlanner.cs` — board coordinate space: `unitScale = max(rows, cols)`, board spans `(0,0)` to `(unitScale, unitScale)` in world space. Puzzle parent transform is at origin with scale 1.

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R173 — Auto-tracking camera frames valid placement positions
- R174 — Camera animation is smooth and slow (~1–1.5s glide)
- R175 — All valid moves visible on screen simultaneously
- R176 — Configurable min/max zoom limits
- R177 — Camera boundary clamping with margin
- R178 — Manual drag override
- R179 — Pinch-to-zoom / scroll wheel
- R180 — Manual mode persists until next piece placement
- R181 — Level start shows full board then zooms to first valid area
- R182 — Camera config as ScriptableObject

## Scope

### In Scope

- Auto-tracking camera with valid-placement framing
- Smooth animation (~1–1.5s glide) with configurable speed
- Boundary clamping with configurable margin
- Min/max zoom enforcement
- Manual drag pan (extend existing CameraController)
- Pinch-to-zoom (mobile) and scroll wheel (PC)
- Manual → auto mode switching on piece placement
- Level-start overview → zoom-in sequence
- CameraConfig ScriptableObject for all tuning values
- Integration with existing tray/deck layout system

### Out of Scope / Non-Goals

- Camera rotation (orthographic, stays axis-aligned)
- 3D perspective camera (stays orthographic)
- Camera shake or screen effects
- Minimap or board overview toggle button

## Technical Constraints

- Orthographic camera only (existing pattern)
- Must not break the existing LateUpdate tray repositioning in PuzzleStageController
- DeckView world-space canvas is parented to camera — must remain attached
- CameraController is a MonoBehaviour on the Main Camera — extend it, don't create a parallel component
- Board coordinate space: puzzle parent at origin, scale 1, board spans (0,0) to (unitScale, unitScale)

## Integration Points

- PuzzleModel.OnPiecePlaced → triggers auto-tracking recalculation
- PuzzleBoard.CanPlace() + PuzzleModel.GetSlot() → determines which pieces are valid to place
- PuzzleStageController._solvedWorldPositions → maps piece IDs to world positions (needs exposure)
- GridPlanner board bounds → boundary clamping reference rect

## Open Questions

- Exact SmoothDamp vs. Lerp interpolation curve — start with SmoothDamp, tune via config
- Whether _solvedWorldPositions should be exposed via a public method or the camera should compute positions independently from GridPlanner — leaning toward exposing a method on PuzzleStageController
