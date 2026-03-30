# S02 Research: Manual Input & Boundary Enforcement

**Depth:** Targeted — known technology (Unity Input, orthographic camera math), moderately complex integration with existing CameraController state machine established in S01.

## Summary

S02 adds four capabilities to the existing CameraController: (1) drag-pan disables auto-tracking, (2) pinch-to-zoom on mobile and scroll-wheel zoom on desktop, (3) zoom clamped to CameraConfig min/max, (4) camera position clamped to board bounds + configurable margin. Auto-tracking resumes on next piece placement (already handled by `SetTarget`).

The existing CameraController already has working drag-pan (HandleMouse/HandleTouch/ApplyScreenDelta) and auto-tracking (SetTarget/LateUpdate SmoothDamp). The work is:
- **Interrupt auto-tracking on pan gesture** — set `_isAutoTracking = false` when panning starts
- **Add scroll wheel zoom** — `Input.mouseScrollDelta.y` in HandleMouse
- **Add pinch-to-zoom** — two-finger touch distance delta in HandleTouch
- **Clamp zoom to config limits** — after any manual zoom, clamp `_camera.orthographicSize` to `[_config.MinZoom, _config.MaxZoom]`
- **Add boundary clamping** — after any position/zoom change (both manual and auto-track), clamp camera XY to keep the viewport within board bounds + margin
- **Expose board bounds** from PuzzleStageController — `_currentGridRows`/`_currentGridCols` are private; need a public accessor so CameraController can compute the clamping rect
- **Add CameraConfig fields** — `BoundaryMargin` (world-unit margin beyond board edge), `ZoomSpeed` (scroll/pinch sensitivity)
- **Write EditMode tests** for CameraMath boundary clamping (pure math, no MonoBehaviour)

## Requirements Targeted

| Req | What | How |
|-----|------|-----|
| R178 | Manual drag override | Set `_isAutoTracking = false` when panning starts in HandleMouse/HandleTouch |
| R179 | Pinch-to-zoom / scroll wheel | New zoom code in HandleMouse (scroll) and HandleTouch (two-finger pinch) |
| R180 | Manual mode persists until next placement | Already handled — `SetTarget` sets `_isAutoTracking = true`; once panning sets it to false, it stays false until next `SetTarget` call |
| R176 | Configurable min/max zoom limits | Clamp `orthographicSize` after every manual zoom delta; config fields already exist on CameraConfig |
| R177 | Camera boundary clamping with margin | New `CameraMath.ClampToBounds` helper + call site in both LateUpdate (auto-track) and ApplyScreenDelta/zoom (manual) |

## Recommendation

Three tasks, linear dependency:

1. **T01 — Config + CameraMath boundary helpers + board bounds exposure** (~20 min)
   - Add `BoundaryMargin` and `ZoomSpeed` fields to CameraConfig
   - Add `CameraMath.ClampToBounds(Vector3 camPos, float orthoSize, float aspect, Rect boardRect, float margin)` — pure static, returns clamped position
   - Add `CameraMath.ComputeBoardRect(int rows, int cols)` — returns `Rect(0, 0, unitScale, unitScale)` matching GridPlanner's convention
   - Expose `PuzzleStageController.GetBoardRect()` → returns `Rect` from `_currentGridRows`/`_currentGridCols`
   - Write EditMode tests for ClampToBounds and ComputeBoardRect in CameraTests.cs

2. **T02 — Manual input (pan override + zoom)** (~25 min)
   - In HandleMouse: set `_isAutoTracking = false` when `_isPanning` first becomes true
   - In HandleMouse: read `Input.mouseScrollDelta.y`, scale by `_config.ZoomSpeed`, apply to `_camera.orthographicSize`, clamp to config limits
   - In HandleTouch: detect `Input.touchCount >= 2` → compute pinch delta → adjust orthoSize → clamp
   - In HandleTouch: set `_isAutoTracking = false` when panning or pinching
   - Guard all config reads on `_config != null`

3. **T03 — Boundary clamping integration + wiring + tests** (~20 min)
   - Store board Rect on CameraController (set via new `SetBoardBounds(Rect)` or read from a wired PuzzleStageController reference)
   - Apply `CameraMath.ClampToBounds` at end of both `ApplyScreenDelta` and `LateUpdate` (after SmoothDamp)
   - Apply after zoom changes too (zoom out can push viewport beyond bounds)
   - Wire `SetBoardBounds` from InGamePresenter (after stage.SpawnLevel, read stage.GetBoardRect)
   - Add EditMode tests verifying clamp is applied

## Implementation Landscape

### Files to Modify

| File | Changes |
|------|---------|
| `Assets/Scripts/Game/InGame/CameraConfig.cs` | Add `BoundaryMargin` (float, default 0.5) and `ZoomSpeed` (float, default 5.0) fields |
| `Assets/Scripts/Game/InGame/CameraMath.cs` | Add `ClampToBounds` static method and `ComputeBoardRect` static method |
| `Assets/Scripts/Game/InGame/CameraController.cs` | Disable auto-tracking on pan; add scroll-wheel zoom in HandleMouse; add pinch-to-zoom in HandleTouch; store board Rect; apply ClampToBounds in ApplyScreenDelta, zoom handlers, and LateUpdate |
| `Assets/Scripts/Game/InGame/PuzzleStageController.cs` | Add `public Rect GetBoardRect()` exposing `_currentGridRows`/`_currentGridCols` as board rect |
| `Assets/Scripts/Game/InGame/InGamePresenter.cs` | After camera SetTarget wiring, also call `_camera.SetBoardBounds(_stage.GetBoardRect())` (once, or on each placement if board doesn't change just pass once) |
| `Assets/Tests/EditMode/Game/CameraTests.cs` | Add tests for ClampToBounds and ComputeBoardRect |

### No Files to Create

All changes extend existing files. No new scripts, no new assemblies.

### Key Code Patterns

**Existing drag-pan flow (HandleMouse):**
```
GetMouseButtonDown(0) → IsOverUI check → _isPanning = true
GetMouseButton(0) → ApplyScreenDelta(delta)
GetMouseButtonUp(0) → _isPanning = false
```
**Change:** Add `_isAutoTracking = false` alongside `_isPanning = true`.

**Existing HandleTouch:**
```
touchCount == 0 → _isPanning = false
touch.phase == Began → IsOverUI check → _isPanning = true
Moved/Stationary → ApplyScreenDelta
Ended/Canceled → _isPanning = false
```
**Change:** Same auto-tracking disable. Add second branch for `touchCount >= 2` (pinch).

**Scroll wheel zoom pattern:**
```csharp
float scroll = Input.mouseScrollDelta.y;
if (Mathf.Abs(scroll) > 0.01f && _config != null && _camera != null)
{
    _isAutoTracking = false;
    _camera.orthographicSize -= scroll * _config.ZoomSpeed * Time.deltaTime;
    _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _config.MinZoom, _config.MaxZoom);
}
```

**Pinch-to-zoom pattern:**
```csharp
if (Input.touchCount >= 2)
{
    _isAutoTracking = false;
    var t0 = Input.GetTouch(0);
    var t1 = Input.GetTouch(1);
    float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
    float currDist = (t0.position - t1.position).magnitude;
    float delta = prevDist - currDist; // positive = fingers moved apart = zoom in
    _camera.orthographicSize += delta * _config.ZoomSpeed * 0.01f;
    _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _config.MinZoom, _config.MaxZoom);
}
```

**Boundary clamping (CameraMath.ClampToBounds):**
```csharp
public static Vector3 ClampToBounds(Vector3 camPos, float orthoSize, float aspect, Rect boardRect, float margin)
{
    float halfH = orthoSize;
    float halfW = orthoSize * aspect;
    float minX = boardRect.xMin - margin + halfW;
    float maxX = boardRect.xMax + margin - halfW;
    float minY = boardRect.yMin - margin + halfH;
    float maxY = boardRect.yMax + margin - halfH;
    // If board is smaller than viewport, center instead of clamping
    if (minX > maxX) camPos.x = boardRect.center.x;
    else camPos.x = Mathf.Clamp(camPos.x, minX, maxX);
    if (minY > maxY) camPos.y = boardRect.center.y;
    else camPos.y = Mathf.Clamp(camPos.y, minY, maxY);
    return camPos;
}
```

**Board rect computation:**
```csharp
public static Rect ComputeBoardRect(int rows, int cols)
{
    float unitScale = Mathf.Max(rows, cols);
    return new Rect(0f, 0f, unitScale, unitScale);
}
```
This matches GridPlanner: board spans (0,0) to (unitScale, unitScale).

### Board Coordinate Space (from GridPlanner + M023 context)

- Puzzle parent at origin, scale 1
- `unitScale = max(rows, cols)`
- Board spans `(0, 0)` to `(unitScale, unitScale)` in world space
- For 8×7 board: unitScale=8, board is 8×8 world units

### Key Constraints

1. **LateUpdate ordering** — PuzzleStageController.LateUpdate repositions tray pieces relative to camera. CameraController.LateUpdate does SmoothDamp. Both run on the same frame. Since tray repositioning reads `Camera.main.transform.position`, it doesn't matter which LateUpdate runs first — Unity's execution order is not guaranteed between MonoBehaviours, but both read/write their own state cleanly. The tray reads camera position; the camera writes its own position. No circular dependency.

2. **DeckView camera-parenting** — DeckView is a world-space canvas already parented to the camera. Moving/zooming the camera automatically moves the deck. No additional work needed.

3. **Boundary clamp must run AFTER SmoothDamp** — if placed before, SmoothDamp's next frame will target the unclamped position and oscillate. Clamping after SmoothDamp keeps the camera within bounds even as it converges.

4. **Boundary clamp must also run after manual pan/zoom** — apply at the end of ApplyScreenDelta and after scroll/pinch zoom adjustment.

5. **Board rect is static for a level** — `_currentGridRows`/`_currentGridCols` don't change after SpawnLevel. The board rect can be computed once and stored.

6. **Zoom-out can push viewport beyond bounds** — when the viewport becomes larger than the board + margin, ClampToBounds should center the camera on the board axis where the viewport exceeds the bounds (the "if minX > maxX" branch above).

### What Already Works (No Changes Needed)

- **Auto-tracking resumes on next placement** — `SetTarget` already sets `_isAutoTracking = true`. After manual override sets it to false, the next `HandlePiecePlaced` → `SetTarget` call re-enables it. R180 is inherently satisfied.
- **SmoothDamp interpolation** — established in S01, continues working.
- **CameraConfig ScriptableObject** — already exists with SmoothTime, MinZoom, MaxZoom, Padding.

### Edge Cases

- **Camera zoomed out past board** — ClampToBounds centers on the overflowed axis. This is the correct behavior for very small boards or extreme zoom-out.
- **Zero-dimension board** (no SpawnLevel called) — guard `SetBoardBounds` calls on `_stage != null`, default to no clamping when board rect is not set.
- **Simultaneous pan + zoom on touch** — when `touchCount >= 2`, disable single-finger pan and only do pinch. When `touchCount` drops back to 1, resume single-finger pan.
- **IsOverUI guard on zoom** — scroll wheel zoom should NOT be blocked by IsOverUI (scroll over UI is common and expected for camera zoom). Pinch zoom similarly should not be blocked.

### Verification Strategy

**EditMode tests (CameraTests.cs):**
- `ClampToBounds_CameraInsideBounds_ReturnsUnchanged`
- `ClampToBounds_CameraBeyondRight_ClampsToRight`
- `ClampToBounds_CameraBeyondLeft_ClampsToLeft`
- `ClampToBounds_ViewportLargerThanBoard_CentersCamera`
- `ComputeBoardRect_SquareGrid_ReturnsCorrectRect`
- `ComputeBoardRect_RectangularGrid_ReturnsSquareBasedOnMax`

**grep verification:**
- `grep -c "ClampToBounds" CameraMath.cs` → ≥1
- `grep -c "BoundaryMargin" CameraConfig.cs` → ≥1
- `grep -c "ZoomSpeed" CameraConfig.cs` → ≥1
- `grep -c "mouseScrollDelta" CameraController.cs` → ≥1
- `grep -c "_isAutoTracking = false" CameraController.cs` → ≥2 (HandleMouse + HandleTouch)
- `grep -c "GetBoardRect" PuzzleStageController.cs` → ≥1

**Unity Test Runner:** All existing 358+ tests must pass. New tests must pass.

## Skill Discovery

No unfamiliar technologies. Unity Input API (Input.mouseScrollDelta, Input.GetTouch, multi-touch) is standard and well-documented. No external skills needed.
