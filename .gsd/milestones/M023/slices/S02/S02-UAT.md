# S02: Manual Input & Boundary Enforcement — UAT

**Milestone:** M023
**Written:** 2026-03-30T13:50:21.735Z

# S02 UAT: Manual Input & Boundary Enforcement

## Preconditions
- Unity Editor open on InGame scene with a loaded level
- CameraConfig asset assigned to CameraController with: MinZoom=3, MaxZoom=10, ZoomSpeed=5, BoundaryMargin=0.5
- A puzzle board of known dimensions (e.g. 4×4 grid) is active
- CameraController and InGamePresenter are both active in the scene

---

## TC01 — Drag-pan cancels auto-tracking

**Steps:**
1. Place a puzzle piece (triggers auto-tracking; camera pans to valid placement zone)
2. While camera is moving (or after it settles), click-and-drag on the game viewport

**Expected:**
- Camera follows the drag delta immediately
- Auto-tracking does NOT resume and fight the drag
- `_isAutoTracking` is false after drag starts (confirm via Debug.Log or inspector)

---

## TC02 — Scroll-wheel zooms in/out and clamps to configured limits

**Steps:**
1. Scroll mouse wheel forward (zoom in) repeatedly until no further change
2. Note the orthographic size — it should not go below `MinZoom`
3. Scroll mouse wheel backward (zoom out) repeatedly until no further change
4. Note the orthographic size — it should not exceed `MaxZoom`

**Expected:**
- Camera zooms smoothly in both directions
- Orthographic size is clamped: never < MinZoom, never > MaxZoom
- Auto-tracking is cancelled after first scroll event

---

## TC03 — Scroll-wheel zoom also cancels auto-tracking

**Steps:**
1. Place a piece (auto-tracking active)
2. Immediately scroll the mouse wheel

**Expected:**
- Camera stops auto-panning and zooms instead
- No conflict between auto-track and manual zoom

---

## TC04 — Drag-pan respects board boundary clamping

**Steps:**
1. Drag the camera toward the right edge of the board until it stops moving
2. Continue dragging right — camera should not move further right
3. Repeat for all four edges (left, up, down)

**Expected:**
- Camera never drifts beyond boardRect + BoundaryMargin (0.5 units) on any side
- Camera visually stays anchored — board edge is always visible within margin

---

## TC05 — Auto-tracking also respects board boundary clamping

**Steps:**
1. Place a puzzle piece near a board edge (valid placement zone is at the edge)
2. Watch the auto-tracking pan

**Expected:**
- Camera smoothly pans to frame the valid area
- Final camera position does not exceed board bounds + margin even if the target was near the edge

---

## TC06 — Auto-tracking resumes on next piece placement after manual override

**Steps:**
1. Manually drag the camera far from the valid placement area
2. Place the next puzzle piece

**Expected:**
- Camera smoothly pans back to frame the new valid placement positions
- Auto-tracking re-engages on piece placement (as implemented in S01)

---

## TC07 — Pinch-to-zoom on touch (mobile or simulator)

**Steps:**
1. On a touch device or Unity Remote, use two fingers to pinch in/out
2. Verify zoom changes
3. Verify orthographic size does not exceed [MinZoom, MaxZoom]
4. Verify auto-tracking is cancelled

**Expected:**
- Smooth zoom response to pinch gesture
- Zoom clamped within configured limits
- Auto-tracking cancelled after first pinch event

---

## TC08 — Single-finger pan does not conflict with two-finger pinch

**Steps:**
1. Start a single-finger drag
2. Add a second finger (mid-drag) to start a pinch

**Expected:**
- When touchCount >= 2, single-finger pan is skipped — no jitter or conflicting movement
- Camera smoothly transitions to pinch-zoom mode

---

## TC09 — SetBoardBounds called once, logged on first piece placement

**Steps:**
1. Open Unity Console before starting the level
2. Play the level; place the first puzzle piece

**Expected:**
- Console shows `[CameraController] SetBoardBounds rect=(x,y WxH)` exactly once
- Values match expected board dimensions (e.g. for 4×4 grid: width=height=4.0 if unitScale=4)

---

## TC10 — Board bounds not set before first piece placement

**Steps:**
1. Start the level
2. Before placing any pieces, drag the camera around

**Expected:**
- No boundary enforcement (camera can move freely — `_hasBoardRect` is false)
- No null-reference exceptions in console

---

## TC11 — GetBoardRect() defensive guard (zero-dimension board)

**Steps (EditMode / unit test):**
1. Call `PuzzleStageController.GetBoardRect()` before `SpawnLevel` is called (rows/cols = 0)

**Expected:**
- Returns a valid Rect with width=height > 0 (floor-to-1 guard prevents degenerate rect)
- No exceptions thrown

---

## TC12 — CameraMath.ClampToBounds: viewport larger than board centres camera

**Steps (EditMode test — already covered by CameraTests.cs):**
1. Call `ClampToBounds` with orthographicSize larger than boardRect
2. Verify returned position is centred on the board rect

**Expected:**
- Camera centred on board (not snapped to any edge)
- Consistent with ClampToBounds unit tests in CameraTests.cs

