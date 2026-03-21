# S03: Tray — Camera-Following Slots + UGUI Buttons

**Goal:** Make tray 3D slot pieces follow the camera each frame. Add a Screen Space Overlay Canvas with one UGUI Button per slot. Wire buttons to OnTapPiece. Remove PieceTapHandler from tray pieces.

**Demo:** Play InGame — pan the camera, tray stays at screen bottom. Tap a slot button; the piece slides to board with full tween. Wrong tap shakes the piece. No gesture conflict.

## Must-Haves

- Tray 3D slot pieces reposition to camera-bottom each LateUpdate
- UGUI Button overlays reposition via world-to-screen projection each LateUpdate
- Slot button pointer-up-inside fires OnTapPiece with the correct piece ID
- SlideToSlot, PlaceOnBoard, ShakePiece tweens work correctly after layout change
- PieceTapHandler is NOT attached to tray pieces
- Board drag still pans camera when tray buttons are not touched
- GetSlotContents() still returns correct slot state for ShakePiece logic

## Tasks

- [ ] **T01: TrayFollower — camera-relative slot positions**
  Extract tray slot positioning into a LateUpdate loop. Each frame: compute slot world positions relative to Camera.main bottom edge. Update 3D slot piece transforms. (Piece follow; buttons follow in T02.)

- [ ] **T02: Slot Button Canvas**
  Add Screen Space Overlay Canvas to InGame scene. Create one Button per slot (dynamically in SpawnPieces). In LateUpdate, project each slot's world position to screen coords and set Button RectTransform anchoredPosition. Wire Button onClick to invoke OnTapPiece with the piece ID currently in that slot.

- [ ] **T03: Wire and verify**
  Remove PieceTapHandler from tray pieces. Run play-from-editor. Verify: tray follows pan, buttons fire correctly, tweens intact, ShakePiece still works, win/lose unaffected.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/InGame/InGameView.cs`
- `Assets/Scripts/Game/InGame/PieceTapHandler.cs` (tray usage removed; board pieces retain it or it's removed entirely)
- `Assets/Scenes/InGame.unity` (Canvas added)
