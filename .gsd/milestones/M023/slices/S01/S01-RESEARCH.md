# S01: Auto-Tracking Camera Core — Research

**Date:** 2026-03-30
**Depth:** Targeted — known technology (Unity orthographic camera, SmoothDamp), moderately complex integration with existing puzzle stage systems.

## Summary

This slice adds auto-tracking behavior to the existing `CameraController` MonoBehaviour on the InGame scene camera. After each piece placement (`PuzzleModel.OnPiecePlaced`), the camera must compute a bounding box of all valid placement target positions and smoothly animate (SmoothDamp, ~1–1.5s) to frame that region with appropriate zoom. A `CameraConfig` ScriptableObject provides tuning values.

The main integration risk is bridging the placement-validity computation: we need piece IDs that `CanPlace()` returns true for, mapped to their solved world positions via `PuzzleStageController._solvedWorldPositions`. That dictionary is currently private. Exposing a targeted public method on `PuzzleStageController` is the cleanest approach — it avoids the camera needing to duplicate GridPlanner math.

The tray repositioning in `PuzzleStageController.LateUpdate` already recalculates positions relative to camera position every frame. Moving the camera won't break the tray — it's designed to follow. DeckView is parented to the camera in the scene hierarchy, so it follows automatically.

## Recommendation

1. **CameraConfig ScriptableObject** — new file with smoothTime, minZoom, maxZoom, padding fields. Created first so all other code can reference it.
2. **Expose valid-target positions on PuzzleStageController** — add a public method `GetValidTargetPositions(PuzzleModel model)` that iterates all piece IDs, checks `model.GetSlot(i)` for pieces in deck slots and `board.CanPlace(id)`, then returns world positions from `_solvedWorldPositions`. This keeps the board-state query in the domain and the world-position lookup in the stage controller where it belongs.
3. **Extend CameraController** with auto-tracking logic — add `SetTarget(Vector3 center, float orthoSize)` and `SmoothDamp` update loop. The controller already owns the camera transform. Add a `CameraConfig` reference and an `IsAutoTracking` state flag.
4. **Wire the event** — subscribe to `PuzzleModel.OnPiecePlaced` in either `InGamePresenter` or `InGameFlowPresenter`, query valid targets, compute bounding box, and call `CameraController.SetTarget()`.

SmoothDamp for both position (Vector3) and orthographic size (float) gives natural deceleration per D111.

## Implementation Landscape

### Key Files

- `Assets/Scripts/Game/InGame/CameraController.cs` — Currently handles drag-pan only (mouse + touch). 113 lines. Extend with auto-tracking state, SmoothDamp update loop, and `CameraConfig` reference. The `Update()` method handles input; add a parallel `LateUpdate()` or section in `Update()` for the smooth follow. Must not break existing pan — pan input sets `_isPanning = true` which should suppress auto-tracking.
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — Owns `_solvedWorldPositions` (pieceId → Vector3) and `_pieceObjects`. Add a public method to compute valid target world positions given a PuzzleModel. Also stores `_currentGridRows` / `_currentGridCols` which define the board extent.
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — `OnPiecePlaced` event is the trigger. `GetSlot(i)` returns current piece IDs in deck. Domain model — no changes needed.
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` — `CanPlace(pieceId)` is the validity check. `PlacedIds` returns placed set. Domain model — no changes needed.
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — Already subscribes to `OnPiecePlaced`. This is where camera target updates should be triggered (presenter owns model↔view coordination).
- `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — Creates the PuzzleModel in `RunAsync()`. Need to pass CameraController reference into the wiring chain so InGamePresenter can drive it.
- `Assets/Scripts/Game/InGame/DeckView.cs` — World-space canvas parented to camera in scene hierarchy. No changes needed — follows camera movement automatically.
- `Assets/Editor/SceneSetup.cs` — Camera created at line 377. If CameraConfig needs a SerializeField reference on CameraController, SceneSetup must wire it. But CameraConfig can also be loaded via `Resources.Load` or assigned at runtime by InGameSceneController, avoiding SceneSetup changes.
- `Packages/simple-jigsaw/Assets/SimpleJigsaw/Runtime/Pipeline/GridPlanner.cs` — Board coordinate space: `unitScale = max(rows, cols)`, board spans `(0,0)` to `(unitScale, unitScale)`. Used for boundary clamping reference rect.

### Build Order

**Task 1: CameraConfig ScriptableObject** — Create the config asset class. No dependencies. Small, fast. Unblocks everything else.

**Task 2: Expose valid-target positions on PuzzleStageController** — Add `GetValidTargetPositions()` method. Depends on understanding PuzzleModel API (already clear). Key risk: the method needs access to both the PuzzleModel (to know which pieces can be placed) and `_solvedWorldPositions` (to map IDs to world positions). The PuzzleModel is created in InGameFlowPresenter and passed to InGamePresenter — PuzzleStageController doesn't currently hold a PuzzleModel reference. Two options:
  - (a) Pass the PuzzleModel to PuzzleStageController at SpawnLevel time (clean — stage already receives piece data there)
  - (b) Pass piece IDs as a parameter to the method and let the caller query CanPlace. This is simpler and avoids coupling the stage to the domain model.

  **Recommendation: option (b)** — `GetWorldPositionsForPieces(IEnumerable<int> pieceIds) → List<Vector3>` keeps the stage controller as a pure rendering concern. The caller (InGamePresenter) queries the model for valid piece IDs, passes them to the stage controller for position lookup.

  Actually — even simpler: expose a `GetSolvedPosition(int pieceId) → Vector3?` method. The caller iterates all non-placed piece IDs, filters by `CanPlace()`, and calls `GetSolvedPosition()` for each. This is the most decoupled approach.

**Task 3: Extend CameraController with auto-tracking** — Add SmoothDamp logic, `SetTarget()` API, `CameraConfig` reference. The main implementation task.

**Task 4: Wire auto-tracking into InGamePresenter** — Subscribe to `OnPiecePlaced`, compute valid target bounding box, call `CameraController.SetTarget()`. This requires InGamePresenter to have references to both PuzzleStageController and CameraController. Currently InGamePresenter receives only `IInGameView`, `GameSessionService`, `IHeartService`, and `PuzzleModel`. Adding CameraController as an optional dependency is the cleanest path.

**Task 5: Tests** — EditMode tests for bounding box computation, valid-target filtering, and SmoothDamp convergence. The bounding box math and valid-target logic are pure — testable without MonoBehaviours.

### Wiring Chain

Current: `InGameFlowPresenter` → creates `PuzzleModel` → creates `InGamePresenter(view, session, hearts, model)` → subscribes to `OnPiecePlaced` → calls `view.RevealPiece()`.

New: `InGamePresenter` also receives `PuzzleStageController` + `CameraController` (or a camera-target interface). On `OnPiecePlaced`: query all pieces in deck slots + all unplaced pieces → filter by `CanPlace()` → get world positions via `PuzzleStageController.GetSolvedPosition()` → compute bounding box → call `CameraController.SetTarget(center, requiredOrthoSize)`.

The `UIFactory.CreateInGamePresenter()` method needs to be extended with the new dependencies. `InGameSceneController` already holds `[SerializeField] PuzzleStageController _stage` — CameraController can be obtained via `Camera.main.GetComponent<CameraController>()` or added as another SerializeField.

### Bounding Box Computation

Given a set of valid target world positions:
1. Compute min/max X and Y
2. Add padding (from CameraConfig)
3. Compute required orthoSize: `max((maxY - minY + padding) / 2, (maxX - minX + padding) / (2 * aspect))`
4. Clamp to configured min/max zoom
5. Center = `((minX + maxX) / 2, (minY + maxY) / 2, camera.z)`

This is pure math — extract into a static helper for testability.

### Valid-Target Enumeration

Which pieces are "valid placement targets"? All pieces that satisfy:
- Not yet placed on the board (`!board.IsPlaced(pieceId)`)
- At least one neighbor is placed (`board.CanPlace(pieceId)`)

The PuzzleModel doesn't expose its internal PuzzleBoard directly, but it exposes `GetSlot(i)` for deck contents. The deck holds only unplaced pieces. However, "valid targets" includes ALL unplaced pieces that can be placed, not just those currently in deck slots — future deck draws matter for framing too.

Actually, re-reading the requirement: "frame all valid placement positions." This means positions where a piece COULD be placed right now — i.e., all pieces satisfying `CanPlace()`, whether or not they're currently visible in a deck slot. This makes sense visually — the camera should show the region of the board where action will happen.

**Problem:** PuzzleModel doesn't expose its PuzzleBoard or an `IEnumerable<int>` of all piece IDs. It exposes slots and events. To enumerate all unplaced pieces that can be placed, we need either:
- (a) Expose a method on PuzzleModel: `GetPlaceablePieceIds() → IReadOnlyList<int>`
- (b) Track placed/unplaced state externally by subscribing to `OnPiecePlaced`

**Recommendation: option (a)** — Add a `GetPlaceablePieceIds()` method to PuzzleModel that iterates all piece IDs, filters by `!IsPlaced && CanPlace`. This is a clean domain query. The camera system shouldn't need to maintain shadow state.

Alternatively, since the camera only needs to fire after `OnPiecePlaced`, and PuzzleStageController already knows ALL piece IDs (from `_solvedWorldPositions.Keys`) and which are placed (pieces parented back to `_puzzleParent` in `RevealPiece()`), the stage controller could compute this itself. But mixing domain logic (CanPlace) into the stage controller violates the existing separation.

**Cleanest path:** Add to PuzzleModel:
```csharp
public IReadOnlyList<int> GetPlaceablePieceIds()
```
This delegates to the internal `_board.CanPlace()` for each non-placed piece.

### Existing Patterns

- `PuzzleStageController.LateUpdate()` already tracks camera position every frame for tray layout — proves camera movement won't break the tray.
- `CameraController` uses `Camera.main` pattern — consistent to continue using.
- `InGamePresenter.HandlePiecePlaced()` is the natural hook point — already fires on each placement.
- `PieceTweener` uses `async UniTaskVoid` for animations — SmoothDamp is frame-based (Update/LateUpdate), not async, so no conflict.

### Assembly Dependencies

- `CameraConfig` should live in `Assets/Scripts/Game/InGame/` — same assembly as CameraController.
- No new assembly references needed — PuzzleModel is in `SimpleGame.Puzzle` which `SimpleGame.Game` already references.
- Tests go in `Assets/Tests/EditMode/Game/` — existing assembly already references all needed assemblies.
