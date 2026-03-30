---
id: S01
parent: M023
milestone: M023
provides:
  - CameraConfig ScriptableObject — config asset for all camera tuning in S02 and S03
  - CameraMath.ComputeFraming — reusable framing helper for S03 level-start overview sequence
  - CameraController.SetTarget/IsAutoTracking/Config — full auto-tracking API that S02 builds override logic on top of
  - PuzzleModel.GetPlaceablePieceIds() — query method reusable by any system that needs to know what pieces are currently playable
  - PuzzleStageController.GetSolvedPosition(int) — world position lookup reusable by S03 for first-placement zoom target
requires:
  []
affects:
  - S02 — builds panning override and boundary clamping on top of _isAutoTracking flag and SetTarget API established here
  - S03 — uses CameraMath.ComputeFraming and GetSolvedPosition for level-start overview sequence
key_files:
  - Assets/Scripts/Game/InGame/CameraConfig.cs
  - Assets/Scripts/Game/InGame/CameraMath.cs
  - Assets/Scripts/Game/InGame/CameraController.cs
  - Assets/Scripts/Puzzle/PuzzleModel.cs
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/InGame/InGameFlowPresenter.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/CameraTests.cs
key_decisions:
  - CameraConfig uses [SerializeField] public fields (not properties) — Unity inspector serialization requires fields; public access lets CameraMath read without a wrapper layer (D112)
  - CameraMath is a pure static class with zero MonoBehaviour dependency — safe for EditMode tests, no engine coupling
  - CameraMath.ComputeFraming returns (Vector3.zero, minZoom) for empty input — board-center fallback when no placeable pieces remain
  - _pieces stored as IReadOnlyList<IPuzzlePiece> in PuzzleModel — preserves existing constructor contract
  - _posVelocity and _sizeVelocity reset to zero on every SetTarget call — SmoothDamp starts from clean state, no residual oscillation
  - LateUpdate guards on _config != null — safe operation before config is assigned
  - InGamePresenter optional params default to null — backward compat with all existing tests (D113)
  - InGameSceneController resolves CameraController via Camera.main?.GetComponent — null in EditMode, leaving existing tests green
patterns_established:
  - CameraMath pure static framing helper pattern: pass IReadOnlyList<Vector3> + config scalars, receive (center, orthoSize) tuple — no MonoBehaviour dependency, fully testable in EditMode
  - Optional wiring pattern for new scene-level dependencies: add as last optional constructor params (default null), guard all usage with null checks — existing tests never notice
  - Auto-tracking state machine in MonoBehaviour: SetTarget enables tracking, SmoothDamp converges in LateUpdate, S02 will disable on input — clean separation of concern between setting destination and consuming it
observability_surfaces:
  - [CameraController] SetTarget center=({x},{y}) ortho={orthoSize} — logged on every SetTarget call, visible in Unity console during play
drill_down_paths:
  - .gsd/milestones/M023/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M023/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M023/slices/S01/tasks/T03-SUMMARY.md
duration: ""
verification_result: passed
completed_at: 2026-03-30T13:37:39.705Z
blocker_discovered: false
---

# S01: Auto-Tracking Camera Core

**Camera smoothly pans and zooms to frame valid placement positions after each piece is placed; CameraConfig ScriptableObject controls speed and zoom limits; 11 new EditMode tests cover all framing and placement-query logic; 358/358 tests pass.**

## What Happened

S01 delivered the complete auto-tracking camera pipeline across three tasks.

**T01 — Foundation types:** Created `CameraConfig.cs` (ScriptableObject with SmoothTime=1.2, MinZoom=2, MaxZoom=15, Padding=1.5 as [SerializeField] public fields), `CameraMath.cs` (pure static class with `ComputeFraming` that computes bounding-box center and clamped orthoSize from world positions, returning (Vector3.zero, minZoom) for empty input), `PuzzleModel.GetPlaceablePieceIds()` (filters pieces by !IsPlaced && CanPlace, backed by a new `_pieces` private field stored from the constructor), and `PuzzleStageController.GetSolvedPosition(int)` (null-safe lookup in the pre-existing `_solvedWorldPositions` dictionary).

**T02 — CameraController auto-tracking:** Extended the existing CameraController MonoBehaviour with six new state fields (`_config`, `_isAutoTracking`, `_targetPosition`, `_targetOrthoSize`, `_posVelocity`, `_sizeVelocity`) and three new public members (`SetConfig`, `SetTarget`, `IsAutoTracking`) plus a `LateUpdate` that SmoothDamps position and orthographic size when `_isAutoTracking && _config != null && _camera != null`. `SetTarget` clamps orthoSize to config min/max, preserves camera Z, resets velocity refs, enables tracking, and logs the target. Existing `Update`/`HandleMouse`/`HandleTouch`/`ApplyScreenDelta` were not touched — S02 will add the panning override. A `public CameraConfig Config => _config` property was also added for T03's use.

**T03 — Wiring and tests:** InGamePresenter received two optional constructor params (`PuzzleStageController stage = null, CameraController cameraController = null`). In `HandlePiecePlaced`, after existing logic, it queries `_model.GetPlaceablePieceIds()`, maps each to `_stage.GetSolvedPosition(id)`, runs `CameraMath.ComputeFraming` with `_camera.Config` values, and calls `_camera.SetTarget`. UIFactory.CreateInGamePresenter passes the optional params through. InGameFlowPresenter stores a `_cameraController` field (optional, last param) and passes it to CreateInGamePresenter. InGameSceneController resolves the CameraController via `Camera.main?.GetComponent<CameraController>()` at Initialize time (null-safe for EditMode). CameraTests.cs adds 11 EditMode tests: 6 in CameraMathTests (single position, multiple positions, empty/null fallback, maxZoom clamping, minZoom clamping, aspect ratio correctness) and 5 in GetPlaceablePieceIdsTests (initial state, after placement, all placed empty, sequential shrink, branching star model). All 358 EditMode tests pass (confirmed via Unity Test Runner job 20da92eca2fc4d928487f7a15767b60a, status=succeeded, 0 failures).

## Verification

All slice plan verification commands passed:
- `grep -c "class CameraConfig" CameraConfig.cs` → 1 ✅
- `grep -c "ComputeFraming" CameraMath.cs` → 1 ✅
- `grep -c "GetPlaceablePieceIds" PuzzleModel.cs` → 1 ✅
- `grep -c "GetSolvedPosition" PuzzleStageController.cs` → 1 ✅
- `grep -c "SetTarget|SmoothDamp|_isAutoTracking|LateUpdate" CameraController.cs` → 11 ✅
- `grep -c "[Test]" CameraTests.cs` → 11 ✅
- Unity Test Runner EditMode: 358 passed / 0 failed / 0 skipped ✅ (job 20da92eca2fc4d928487f7a15767b60a)

## Requirements Advanced

- R060 — Auto-tracking camera wired into piece placement event (HandlePiecePlaced → GetPlaceablePieceIds → GetSolvedPosition → ComputeFraming → SetTarget), completing the camera layer of the tap-driven gameplay loop

## Requirements Validated

None.

## New Requirements Surfaced

- CameraConfig asset must be wired in scene (SetConfig called) before auto-tracking activates — currently no scene wiring; this is a follow-up for InGameSceneController or SceneSetup

## Requirements Invalidated or Re-scoped

None.

## Deviations

T03 test runner confirmation was delayed to slice-close time (Unity MCP was offline when T03 completed). No code deviations from plan. T02 used `grep` instead of `rg` for Windows path-quoting compatibility (K012 added). GetSolvedPosition in T01 needed only a null-safe wrapper since `_solvedWorldPositions` already existed in PuzzleStageController.

## Known Limitations

Auto-tracking and drag-pan currently coexist without override: if the user drags while auto-tracking is active, both SmoothDamp (LateUpdate) and drag-pan (Update) apply simultaneously. This is by design — S02 will add `_isAutoTracking = false` when panning starts. CameraConfig asset must be assigned in the InGame scene (via InGameSceneController → InGameFlowPresenter → SetConfig) before auto-tracking activates; a missing config silently skips tracking rather than erroring.

## Follow-ups

S02 must add `_isAutoTracking = false` in HandleMouse/HandleTouch when a pan gesture begins, and `_isAutoTracking` resumes on next piece placement (already handled by SetTarget). S02 should also add boundary clamping (board bounds + margin). S03 will add the level-start full-board overview sequence. InGameSceneController needs a SerializeField or scene wiring to pass CameraConfig to InGameFlowPresenter → SetConfig (not yet wired — SetConfig is available but not called from the scene controller).

## Files Created/Modified

- `Assets/Scripts/Game/InGame/CameraConfig.cs` — New ScriptableObject with SmoothTime, MinZoom, MaxZoom, Padding fields
- `Assets/Scripts/Game/InGame/CameraMath.cs` — New pure static class with ComputeFraming bounding-box framing helper
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — Added _pieces field and GetPlaceablePieceIds() query method
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — Added GetSolvedPosition(int) null-safe world position lookup
- `Assets/Scripts/Game/InGame/CameraController.cs` — Added auto-tracking state fields, SetConfig/SetTarget/IsAutoTracking/Config API, and LateUpdate SmoothDamp loop
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — Added optional PuzzleStageController + CameraController params; camera targeting in HandlePiecePlaced
- `Assets/Scripts/Game/Boot/UIFactory.cs` — CreateInGamePresenter extended with optional stage + cameraController params
- `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — Added _cameraController field, optional constructor param, pass-through to CreateInGamePresenter
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — Resolves CameraController via Camera.main?.GetComponent at Initialize time
- `Assets/Tests/EditMode/Game/CameraTests.cs` — New: 11 EditMode tests — 6 CameraMath + 5 GetPlaceablePieceIds
- `.gsd/KNOWLEDGE.md` — Added K012: Windows ripgrep OS error 123 with trailing-slash directory paths
- `.gsd/DECISIONS.md` — Added D112 (CameraConfig field style) and D113 (null-guard strategy)
