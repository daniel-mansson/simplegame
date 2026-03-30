# S01: Auto-Tracking Camera Core — UAT

**Milestone:** M023
**Written:** 2026-03-30T13:37:39.706Z

## UAT: S01 — Auto-Tracking Camera Core

### Preconditions
- Unity Editor open with InGame scene loaded
- A level loaded with at least 3 puzzle pieces (seed + 2+ neighbours)
- CameraConfig ScriptableObject created (`Assets → Create → SimpleGame → Camera Config`) with default values (SmoothTime=1.2, MinZoom=2, MaxZoom=15, Padding=1.5)
- CameraConfig assigned to CameraController (via Inspector SerializeField or SetConfig call)
- InGameSceneController has PuzzleStageController wired in Inspector

---

### Test Cases

#### TC-01: Auto-tracking activates on first piece placement
**Steps:**
1. Enter Play mode and start a level.
2. Tap the current piece to place it on a valid board position.
3. Observe the camera.

**Expected:**
- Camera smoothly pans toward the centroid of all currently placeable piece positions.
- Camera orthographic size smoothly zooms to frame all valid positions with padding.
- Unity console shows `[CameraController] SetTarget center=(x,y) ortho=z` log entry.

---

#### TC-02: Camera re-targets after each successive placement
**Steps:**
1. Place piece 1 → observe camera settle.
2. Place piece 2 → observe camera move again.
3. Place piece 3 → observe camera move again.

**Expected:**
- After each placement, camera re-targets the new set of placeable positions (the set changes as the puzzle progresses).
- Each placement triggers a new `[CameraController] SetTarget` log line with different center/ortho values (unless the placeable set happens to be identical).
- SmoothDamp produces smooth interpolation — no instant jump.

---

#### TC-03: Framing respects MinZoom (single valid position)
**Steps:**
1. Use a level where only one piece is placeable at a time (linear chain).
2. Place a piece so exactly one position is valid.
3. Observe camera orthoSize.

**Expected:**
- Camera orthoSize converges to no less than `MinZoom` (2 by default).
- Camera center moves to that single position.

---

#### TC-04: Framing respects MaxZoom (many spread-out valid positions)
**Steps:**
1. Use a level with many simultaneously valid positions spread across the board (star graph topology).
2. Place the seed piece to unlock many neighbours at once.
3. Observe camera.

**Expected:**
- Camera orthoSize converges to no more than `MaxZoom` (15 by default) even if the positions are very spread out.

---

#### TC-05: Camera uses SmoothDamp (not instant snap)
**Steps:**
1. Place a piece; immediately observe the camera position and size.
2. Wait 2–3 seconds.

**Expected:**
- Immediately after placement: camera has started moving but is not yet at target.
- After ~1.2s (SmoothTime): camera position and size have converged to target values.
- No overshoot or oscillation — SmoothDamp produces critically-damped motion.

---

#### TC-06: No camera movement before first piece placement
**Steps:**
1. Enter Play mode and start a level.
2. Do NOT tap any piece. Wait 3 seconds.

**Expected:**
- Camera does not move.
- No `[CameraController] SetTarget` log entries appear.
- `IsAutoTracking` is false.

---

#### TC-07: Puzzle complete — no crash or NRE
**Steps:**
1. Play through an entire short level (3–4 pieces) to completion.
2. Observe Unity console throughout.

**Expected:**
- After final piece placed, `GetPlaceablePieceIds()` returns an empty list.
- No SetTarget is called (guard `if (positions.Count > 0)` suppresses it).
- No NullReferenceException in console.
- Win dialog appears normally.

---

#### TC-08: CameraConfig values are respected
**Steps:**
1. In Inspector, change CameraConfig `Padding` to 3.0, `SmoothTime` to 0.3, `MaxZoom` to 8.
2. Enter Play mode and place a piece.

**Expected:**
- Camera converges faster (0.3s SmoothTime).
- OrthoSize is capped at 8 even if positions are spread.
- Framing feels more padded (larger empty space around pieces).

---

#### TC-09: EditMode CameraTests — all 11 pass
**Steps:**
1. Open Unity Test Runner (Window → General → Test Runner).
2. Select EditMode tab.
3. Run `CameraMathTests` and `GetPlaceablePieceIdsTests`.

**Expected:**
- All 11 tests pass (green) with no failures.
- Total EditMode suite: 358/358 pass.

**Specific tests to verify:**
- `CameraMath_ComputeFraming_SinglePosition_ReturnsPositionAsCenter` ✅
- `CameraMath_ComputeFraming_MultiplePositions_CorrectBounds` ✅
- `CameraMath_ComputeFraming_EmptyPositions_ReturnsFallback` (returns Vector3.zero, minZoom) ✅
- `CameraMath_ComputeFraming_ClampsToMaxZoom` ✅
- `CameraMath_ComputeFraming_ClampsToMinZoom` ✅
- `CameraMath_ComputeFraming_AspectRatioApplied` ✅
- `GetPlaceablePieceIds_InitialState` ✅
- `GetPlaceablePieceIds_AfterPlacement` ✅
- `GetPlaceablePieceIds_AllPlaced_ReturnsEmpty` ✅
- `GetPlaceablePieceIds_AfterEachPlacement_ListShrinks` ✅
- `GetPlaceablePieceIds_BranchingStarModel` ✅

---

#### TC-10: Existing InGameTests unaffected
**Steps:**
1. Run EditMode → `InGameTests` suite.

**Expected:**
- All pre-existing InGame tests pass — optional constructor params defaulting to null means no test changes were needed.
