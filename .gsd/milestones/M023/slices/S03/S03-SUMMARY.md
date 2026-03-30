---
id: S03
parent: M023
milestone: M023
provides:
  - Full level-start camera sequence: overview snap + hold + animated zoom to first placement area
  - CameraConfig.OverviewHoldDuration field (default 1.0s, designer-tunable via asset)
  - CameraMath.ComputeFullBoardFraming pure-static method (testable, no MonoBehaviour deps)
  - CameraController.SnapTo instant-teleport API (cancels tracking, resets velocity refs)
  - CameraConfig.asset at Assets/Data/ wired into InGame.unity — all M023 camera features now active at runtime
requires:
  []
affects:
  []
key_files:
  - Assets/Scripts/Game/InGame/CameraConfig.cs
  - Assets/Scripts/Game/InGame/CameraMath.cs
  - Assets/Scripts/Game/InGame/CameraController.cs
  - Assets/Scripts/Game/InGame/InGameFlowPresenter.cs
  - Assets/Tests/EditMode/Game/CameraTests.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Data/CameraConfig.asset
  - Assets/Scenes/InGame.unity
key_decisions:
  - SnapTo preserves camera Z depth to avoid depth-sorting issues with 2D sprites
  - Level-start sequence guarded by triple null check (_cameraController, _stage, Config) so it silently no-ops in test contexts and partial scenes
  - analytics TrackLevelStarted fires after the camera sequence so session timing measures from when the board is actually visible
  - SetBoardBounds called eagerly at level-start (before first piece placement) so manual pan clamping is active immediately
  - CameraConfig create-or-load pattern in SceneSetup ensures idempotent re-runs never duplicate the asset
  - CameraConfig wiring placed at end of CreateInGameScene() before SaveScene, matching existing late-wiring pattern for asset references
patterns_established:
  - ComputeFullBoardFraming follows the same height-vs-width max pattern as ComputeFraming — consistent framing API for both board-wide overview and placement-area zoom
  - SceneSetup create-or-load pattern: LoadAssetAtPath first, CreateInstance+CreateAsset only if null — use this for any new ScriptableObject asset wired to a scene component
observability_surfaces:
  - CameraController.SnapTo logs: '[CameraController] SnapTo center=(x,y) ortho=z' — visible in Unity Console and Editor.log during level-start sequence
drill_down_paths:
  - .gsd/milestones/M023/slices/S03/tasks/T01-SUMMARY.md
  - .gsd/milestones/M023/slices/S03/tasks/T02-SUMMARY.md
duration: ""
verification_result: passed
completed_at: 2026-03-30T14:16:00.767Z
blocker_discovered: false
---

# S03: Level Start Sequence & Polish

**Level-start camera sequence delivered: instant overview snap → hold → animated zoom to first valid placement area, with full-board framing math, SnapTo API, and CameraConfig asset wired into the InGame scene.**

## What Happened

S03 completed in two tasks with no blockers and no regressions.

**T01** added all runtime behaviour: the `OverviewHoldDuration` field to `CameraConfig` (default 1.0s), the `ComputeFullBoardFraming` pure-static method to `CameraMath` (uses boardRect.center, height-vs-width framing logic matching `ComputeFraming`, clamped to [MinZoom, MaxZoom]), and the `SnapTo` instant-teleport API on `CameraController` (preserves Z depth, clamps orthoSize when config present, sets `_isAutoTracking = false`, resets velocity refs, logs the snap). The level-start camera sequence was wired into `InGameFlowPresenter.RunAsync` after `presenter.Initialize()` and before `_analytics?.TrackLevelStarted`: triple null-guard (`_cameraController != null && _stage != null && Config != null`), eager `SetBoardBounds`, `SnapTo` to full-board framing, `UniTask.Delay` for `OverviewHoldDuration`, then `SetTarget` to first valid placement area (gracefully skipped if no placeable positions). Analytics fires *after* the sequence so timing reflects when the board is visible. The existing `_boardBoundsSet` guard in `InGamePresenter.HandlePiecePlaced` was left as a redundant safety net. Three new EditMode tests (`ComputeFullBoardFramingTests`) were added — square board, rectangular board (width-driven), and tiny board (min-zoom clamp) — bringing the total [Test] count to 21 in CameraTests.cs.

**T02** closed the last gap: `SceneSetup.CreateInGameScene()` was extended with a create-or-load block for `Assets/Data/CameraConfig.asset` (matches the established pattern for `DefaultGridConfig`/`DefaultPieceRenderConfig`), wired to `CameraController._config` via the existing `WireSerializedField` helper. SceneSetup was re-run via `Tools/Setup/Create And Register Scenes`. The InGame.unity scene file now contains `_config: {fileID: 11400000, guid: d5f1b80a58facb345bcacfdc5d3f5fac, type: 2}` matching `CameraConfig.asset.meta`. Without this step, `Config` would always return null and every camera feature across M023 would be silently skipped at runtime.

All 368 EditMode tests passed (0 failures) on the final run after both tasks were complete.

## Verification

All slice-level verification checks passed:

- `grep -c "OverviewHoldDuration" CameraConfig.cs` → 1 ✅
- `grep -c "ComputeFullBoardFraming" CameraMath.cs` → 1 ✅
- `grep -c "SnapTo" CameraController.cs` → 2 (definition + call site) ✅
- `grep -c "OverviewHoldDuration|ComputeFullBoardFraming|SnapTo" InGameFlowPresenter.cs` → 3 (≥ 2) ✅
- `grep -c "\[Test\]" CameraTests.cs` → 21 ✅
- `grep -c "CameraConfig" SceneSetup.cs` → 4 (≥ 2) ✅
- `Assets/Data/CameraConfig.asset` exists ✅
- `InGame.unity _config GUID` matches `CameraConfig.asset.meta` (d5f1b80a58facb345bcacfdc5d3f5fac) ✅
- Unity EditMode test runner: **368/368 passed, 0 failed** ✅

## Requirements Advanced

None.

## Requirements Validated

None.

## New Requirements Surfaced

None.

## Requirements Invalidated or Re-scoped

None.

## Deviations

CameraConfig wiring placed before SaveScene at the end of `CreateInGameScene()` rather than immediately after `AddComponent<CameraController>()` as specified — functionally equivalent and matches the existing late-wiring pattern for all other asset references in the same function.

## Known Limitations

None. The overview sequence is skipped gracefully when `_cameraController`, `_stage`, or `Config` is null — this covers test contexts and any future scene configuration where camera is not present.

## Follow-ups

None. M023 is now complete — all three slices delivered. Future work: tune `OverviewHoldDuration` and `SmoothTime` values via the CameraConfig asset once playtesting feedback is available.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/CameraConfig.cs` — Added OverviewHoldDuration float field (default 1.0s) with XML doc comment
- `Assets/Scripts/Game/InGame/CameraMath.cs` — Added ComputeFullBoardFraming pure-static method for full-board overview framing
- `Assets/Scripts/Game/InGame/CameraController.cs` — Added SnapTo() instant-teleport API: sets position+orthoSize, cancels tracking, resets velocity refs, logs snap
- `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` — Added level-start camera sequence: SetBoardBounds → SnapTo overview → UniTask.Delay → SetTarget first placement area
- `Assets/Tests/EditMode/Game/CameraTests.cs` — Added ComputeFullBoardFramingTests fixture with 3 tests (square, rectangular, tiny/clamped), total [Test] count = 21
- `Assets/Editor/SceneSetup.cs` — Added CameraConfig create-or-load block and WireSerializedField wiring in CreateInGameScene()
- `Assets/Data/CameraConfig.asset` — New ScriptableObject asset created by SceneSetup with default values
- `Assets/Scenes/InGame.unity` — Regenerated by SceneSetup — CameraController._config now references CameraConfig.asset
