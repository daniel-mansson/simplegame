---
id: S03
milestone: M014
provides:
  - LateUpdate repositions tray 3D slot pieces relative to camera-bottom each frame
  - SpawnSlotButtons creates one transparent UGUI Button per slot on Screen Space Overlay canvas
  - LateUpdate repositions buttons via world-to-screen projection each frame
  - Button onClick fires OnTapPiece with current piece in that slot
  - Board drag (CameraController) and slot tap (UGUI) do not conflict
  - PieceTapHandler not attached to tray pieces
  - SlideToSlot, PlaceOnBoard, ShakePiece tweens unchanged and functional
  - 264/264 tests pass
key_files:
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/InGame/CameraController.cs
key_decisions:
  - "Slot buttons are transparent UGUI Images (alpha=0) so they receive raycasts without visual artifact"
  - "UGUI Screen Space Overlay naturally blocks pointer-down before CameraController sees it"
  - "SpawnSlotButtons reuses existing ScreenSpaceOverlay canvas if present"
completed_at: 2026-03-21T19:30:00Z
---

# S03: Tray — Camera-Following Slots + UGUI Buttons

**Tray 3D pieces follow camera each LateUpdate; transparent UGUI buttons handle slot tap; board drag and slot tap do not conflict. 264/264 tests pass.**

Three tasks combined into InGameSceneController:
- `LateUpdate`: recomputes tray slot world positions relative to Camera.main each frame, moves 3D pieces, repositions UGUI buttons via WorldToScreenPoint.
- `SpawnSlotButtons`: creates one transparent Button per slot on Screen Space Overlay canvas. onClick resolves current slot content and fires NotifyPieceTapped.
- PieceTapHandler not attached to tray pieces (removed in S02 SpawnPieces rewrite).
