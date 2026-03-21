---
id: M014
provides:
  - GridPlanner longest-edge = 1 unit sizing (unitScale = max(rows,cols))
  - BoardClipper scales silhouette to match unit-space board extent
  - Board parent at localScale=(1,1,1), position=(0,0,0)
  - HintSurfaceBuilder wired — hint mesh behind pieces at z=+0.1
  - CameraController orthographic pan on board drag
  - LateUpdate tray follow — 3D slot pieces repositioned each frame relative to camera-bottom
  - UGUI slot buttons — transparent, Screen Space Overlay, onClick fires OnTapPiece
  - No gesture conflict — UGUI blocks pointer before CameraController
  - All 264 EditMode tests pass throughout
key_files:
  - Packages/simple-jigsaw/.../GridPlanner.cs
  - Packages/simple-jigsaw/.../BoardClipper.cs
  - Packages/simple-jigsaw/.../BoardFactory.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/InGame/CameraController.cs
  - Assets/Editor/SceneSetup.cs
key_decisions:
  - "GridPlanner: unitScale = max(rows,cols); cellW = unitScale/cols; cellH = unitScale/rows"
  - "BoardClipper.Clip accepts optional boardExtent for rect+shape-clip path"
  - "Slot buttons use transparent Image (alpha=0) to receive raycasts without visual artifact"
  - "Screen Space Overlay canvas naturally resolves gesture conflict"
completed_at: 2026-03-21T19:30:00Z
---

# M014: Puzzle Controls & Layout Redesign

**Pieces sized at longest-edge = 1 unit; board in world space with camera pan; hint surface; tray follows camera with UGUI button slot input. All 264 tests pass throughout.**

## What Was Built

S01: Changed GridPlanner from normalized 0–1 sizing to longest-edge = 1 unit. Updated GridPlannerTests, ContourAssemblerTests. Fixed BoardClipper to accept boardExtent for rect+shape-clip path.

S02: Rewrote SpawnPieces to remove board scaling (parent at (1,1,1)). Wired HintSurfaceBuilder — hint mesh spawned as child of board parent at z=+0.1. Added CameraController MonoBehaviour on camera for orthographic pan on board drag. Updated SceneSetup to wire CameraController and switch canvas to ScreenSpaceOverlay with GraphicRaycaster.

S03: Added LateUpdate to InGameSceneController that repositions tray 3D slot pieces and UGUI buttons each frame relative to camera-bottom. SpawnSlotButtons creates one transparent UGUI Button per slot; onClick resolves current slot content and fires OnTapPiece. UGUI naturally blocks pointer events before CameraController, eliminating gesture conflict.
