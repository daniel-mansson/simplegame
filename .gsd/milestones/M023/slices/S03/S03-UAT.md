# S03: Level Start Sequence & Polish — UAT

**Milestone:** M023
**Written:** 2026-03-30T14:16:00.767Z

## UAT: S03 — Level Start Sequence & Polish

### Preconditions
- Unity Editor with InGame scene loaded
- Play mode available
- CameraConfig.asset exists at `Assets/Data/CameraConfig.asset` with `OverviewHoldDuration = 1.0`
- All 368 EditMode tests pass

---

### Test 1: Full-Board Overview Snap at Level Start

**Purpose:** Confirm camera instantly snaps to show the entire puzzle board when a level starts.

**Steps:**
1. Enter Play mode from the Boot scene (or navigate to InGame via MainMenu → Play)
2. Watch the camera position during the first frame of InGame

**Expected:**
- Camera immediately shows the entire puzzle board in frame (no pieces cut off)
- The view is zoomed out enough that all board cells are visible
- The transition is instant (no smooth animation on the overview itself)

---

### Test 2: Overview Hold Duration

**Purpose:** Confirm the camera holds on the full-board overview for ~1 second before zooming.

**Steps:**
1. Enter InGame as above
2. After the overview snap, count time until the camera begins animating

**Expected:**
- Camera holds the full-board overview for approximately 1 second (configurable via `CameraConfig.OverviewHoldDuration`)
- No input is required to trigger the transition

---

### Test 3: Animated Zoom to First Valid Placement Area

**Purpose:** Confirm the camera smoothly animates from overview to the first valid placement area after the hold.

**Steps:**
1. Enter InGame and wait through the overview hold
2. Observe the camera movement after the hold expires

**Expected:**
- Camera smoothly pans and zooms toward the region containing the first valid placement positions
- Animation uses SmoothDamp (natural deceleration, no linear/abrupt stop)
- Final framing shows the valid placement area with appropriate padding

---

### Test 4: Manual Override After Sequence

**Purpose:** Confirm manual pan/zoom still works after the level-start sequence completes.

**Steps:**
1. Complete the level-start sequence (overview → hold → zoom to placement area)
2. Drag to pan the camera manually

**Expected:**
- Camera responds to drag immediately
- Auto-tracking stops (camera does not snap back to placement area while dragging)

---

### Test 5: Auto-Tracking Resumes on Next Piece Placement

**Purpose:** Confirm auto-tracking resumes after the manual override when the next piece is placed.

**Steps:**
1. After the level-start sequence, pan manually to a different area
2. Place a puzzle piece correctly

**Expected:**
- Camera smoothly animates back to frame the next valid placement area
- Manual override state is cleared

---

### Test 6: CameraConfig Asset Wired at Runtime

**Purpose:** Confirm the CameraConfig ScriptableObject is correctly serialized into InGame.unity and not null at runtime.

**Steps:**
1. Select the InGame scene's Camera GameObject in the Hierarchy
2. Inspect the CameraController component in the Inspector

**Expected:**
- The `_config` field shows "CameraConfig" (not "None (Camera Config)")
- No null-ref warnings appear in the Console during Play mode

---

### Test 7: OverviewHoldDuration Designer Tuning

**Purpose:** Confirm changing OverviewHoldDuration in the asset changes the hold time at runtime.

**Steps:**
1. Open `Assets/Data/CameraConfig.asset` in the Inspector
2. Set `OverviewHoldDuration` to `0` (zero)
3. Enter Play mode and start a level

**Expected:**
- Camera snaps to overview and immediately begins animating to the placement area (no hold)
- No errors or null-refs

4. Restore `OverviewHoldDuration` to `1` after the test.

---

### Test 8: EditMode Tests — ComputeFullBoardFraming

**Purpose:** Confirm the three new unit tests pass in the EditMode test runner.

**Steps:**
1. Open Window → General → Test Runner
2. Run EditMode tests, filter by "ComputeFullBoardFraming"

**Expected:**
- `ComputeFullBoardFraming_SquareBoard_ReturnsCorrectFraming` — PASS
- `ComputeFullBoardFraming_RectangularBoard_AdjustsForAspect` — PASS
- `ComputeFullBoardFraming_TinyBoard_ClampsToMinZoom` — PASS

---

### Test 9: No Regression — Full EditMode Suite

**Steps:**
1. Run all EditMode tests

**Expected:**
- 368 tests pass, 0 failures
- No new warnings introduced

---

### Edge Cases

**No placeable positions at level start:** If `model.GetPlaceablePieceIds()` returns empty, the sequence should snap to overview, hold, then stay at overview (no SetTarget call). No errors expected.

**Very wide board (extreme aspect ratio):** `ComputeFullBoardFraming` uses `max(requiredByHeight, requiredByWidth)` — wide boards are correctly framed by the width leg rather than overflowing the viewport.

**Very small board (tiny orthoSize):** Framing result is clamped to `MinZoom` — camera never zooms closer than the configured limit even if the board is tiny.

**No CameraController in scene:** The triple null-guard in `InGameFlowPresenter` means the entire sequence is silently skipped. No errors, level starts normally.
