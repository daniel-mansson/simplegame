---
id: M023
title: "In-Game Camera Movement"
status: complete
completed_at: 2026-03-30T14:27:34.997Z
key_decisions:
  - D112: CameraConfig uses [SerializeField] public fields (not properties) — Unity inspector serialization requires fields; public access lets CameraMath read without a wrapper layer. Still valid — SceneSetup WireSerializedField confirmed this works end-to-end.
  - D113: Optional constructor params (default null) + null-guards for all new scene-level dependencies in InGamePresenter, InGameFlowPresenter — preserves backward compat with all existing tests. Still valid — the triple null-guard pattern scaled correctly to S03's InGameFlowPresenter level-start sequence.
  - CameraMath pure static framing helper pattern — all framing math (ComputeFraming, ComputeFullBoardFraming, ClampToBounds, ComputeBoardRect) lives in a zero-dependency static class, safe for EditMode tests.
  - Level-start sequence wired in InGameFlowPresenter.RunAsync with triple null-guard (cameraController, stage, Config) — silently no-ops in test contexts and partial scenes.
  - SetBoardBounds called eagerly at level-start (before first piece placement) in InGameFlowPresenter — ensures manual pan clamping is active from the moment the board appears.
  - SceneSetup create-or-load pattern for CameraConfig.asset — LoadAssetAtPath first, CreateInstance+CreateAsset only if null — idempotent re-runs never duplicate the asset.
key_files:
  - Assets/Scripts/Game/InGame/CameraConfig.cs
  - Assets/Scripts/Game/InGame/CameraMath.cs
  - Assets/Scripts/Game/InGame/CameraController.cs
  - Assets/Scripts/Game/InGame/InGameFlowPresenter.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
  - Assets/Scripts/Puzzle/PuzzleModel.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/CameraTests.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Data/CameraConfig.asset
  - Assets/Scenes/InGame.unity
lessons_learned:
  - Optional-param wiring pattern works well at scale: adding new scene-level dependencies as last optional constructor params (default null) with null-guards at every usage site kept all 347 pre-existing tests green across all three slices without touching a single test file.
  - Pure static math helpers (CameraMath) are worth the discipline — having all framing, clamping, and board-rect math in a zero-dependency static class made every new algorithm trivially testable in EditMode without any scene or MonoBehaviour setup.
  - SceneSetup create-or-load is the right pattern for new ScriptableObject assets: LoadAssetAtPath first, CreateInstance only if null — idempotent on re-runs and matched the existing DefaultGridConfig/DefaultPieceRenderConfig convention perfectly.
  - DeckView unparenting bug: SceneSetup regeneration cleared the parent hierarchy of previously-parented UI objects (DeckView). Any UI element explicitly parented to a scene camera or canvas should be re-verified after SceneSetup runs — add a post-run checklist or automate the reparenting in SceneSetup itself.
  - Windows ripgrep OS error 123 (K012): rg fails on Windows with trailing-slash directory paths. Always use grep -c for Windows-compatible verification commands in GSD task verify blocks.
  - UniTask.Delay for the overview hold is simple and effective — no coroutine, no state machine, just await in the async RunAsync flow. This pattern is clean for any timed camera or UI sequence.
---

# M023: In-Game Camera Movement

**Delivered a complete auto-tracking camera system for the in-game puzzle scene — smoothly pans/zooms to frame placement areas after each piece is placed, supports drag-pan/pinch/scroll manual override with boundary clamping, and opens each level with a full-board overview sequence; all features are config-driven via a wired CameraConfig asset and backed by 21 EditMode tests (368 total passing).**

## What Happened

M023 delivered the complete in-game camera system across three slices, each building cleanly on the previous.

**S01 — Auto-Tracking Camera Core** established the foundation: `CameraConfig.cs` (ScriptableObject with SmoothTime, MinZoom, MaxZoom, Padding as `[SerializeField] public` fields), `CameraMath.cs` (pure static class with `ComputeFraming` returning bounding-box center + clamped orthoSize), `PuzzleModel.GetPlaceablePieceIds()` (filters unplaced, placeable pieces), and `PuzzleStageController.GetSolvedPosition(int)` (null-safe world-position lookup). The `CameraController` MonoBehaviour was extended with SmoothDamp auto-tracking (SetTarget, IsAutoTracking, LateUpdate loop). `InGamePresenter.HandlePiecePlaced` was wired to query placeable positions, compute framing, and call `SetTarget`. All wiring used optional constructor parameters (null-default) preserving backward compatibility with the 347 existing tests. 11 new EditMode tests were added (6 CameraMath + 5 GetPlaceablePieceIds). 358/358 tests passed.

**S02 — Manual Input & Boundary Enforcement** added `BoundaryMargin` and `ZoomSpeed` to CameraConfig, two new pure-static helpers to CameraMath (`ClampToBounds` clamps camera XY to boardRect+margin, `ComputeBoardRect` mirrors the unitScale convention), and `PuzzleStageController.GetBoardRect()`. CameraController gained `SetBoardBounds(Rect)`, scroll-wheel zoom in `HandleMouse`, pinch-to-zoom in `HandleTouch`, and `_isAutoTracking = false` on all manual input starts (4 sites). `ClampToBounds` is applied in both `ApplyScreenDelta` and `LateUpdate`. InGamePresenter was updated to call `SetBoardBounds` once on first piece placement. 7 new EditMode tests were added; total reached 18. 

**S03 — Level Start Sequence & Polish** completed the milestone: `OverviewHoldDuration` added to CameraConfig, `ComputeFullBoardFraming` added to CameraMath (full-board framing using height-vs-width max, same pattern as ComputeFraming), and `SnapTo` instant-teleport API added to CameraController (cancels tracking, resets velocity refs, preserves Z depth, logs snap). The level-start sequence was wired into `InGameFlowPresenter.RunAsync`: triple null-guard → `SetBoardBounds` → `SnapTo` overview → `UniTask.Delay(OverviewHoldDuration)` → `SetTarget` first valid placement area. Analytics fires after the sequence. SceneSetup was extended with create-or-load logic for `CameraConfig.asset` (matching the DefaultGridConfig pattern), wired into CameraController via `WireSerializedField`. The InGame.unity scene now references the asset (GUID d5f1b80a58facb345bcacfdc5d3f5fac confirmed). A follow-up fix restored `DeckView` as a child of `InGameCamera` after SceneSetup regeneration had unparented it. 3 new EditMode tests added; final total 21 tests / 368 total passing.

## Success Criteria Results

## Success Criteria Results

### S01: Auto-Tracking Camera Core — ✅ Met
- **Camera smoothly pans and zooms to frame all valid placement positions after each piece is placed**: `HandlePiecePlaced` → `GetPlaceablePieceIds` → `GetSolvedPosition` → `ComputeFraming` → `SetTarget` pipeline confirmed in InGamePresenter.cs (grep: `SetTarget` 4 occurrences).
- **CameraConfig ScriptableObject controls speed and zoom limits**: `CameraConfig.cs` with SmoothTime, MinZoom, MaxZoom, Padding confirmed (grep: `class CameraConfig` → 1); LateUpdate SmoothDamp reads all config values.
- **11 new EditMode tests cover framing and placement-query logic**: `[Test]` count = 11 after S01, confirmed.

### S02: Manual Input & Boundary Enforcement — ✅ Met
- **Player can drag to pan, overriding auto-track**: `_isAutoTracking = false` set in HandleMouse/HandleTouch (grep: 5 occurrences in CameraController.cs).
- **Pinch/scroll-to-zoom with clamping**: `mouseScrollDelta` (1), two-finger distance delta, both clamped to [MinZoom, MaxZoom] confirmed.
- **Camera can't drift beyond board bounds + margin**: `ClampToBounds` (4 occurrences in CameraController.cs), `SetBoardBounds` (2 occurrences) confirmed.
- **Next placement resumes auto-tracking**: `SetTarget` calls `_isAutoTracking = true` implicitly (S01 established this); confirmed by S02 summary.
- **7 new EditMode tests**: `[Test]` count = 18 after S02, confirmed.

### S03: Level Start Sequence & Polish — ✅ Met
- **Level begins with full board overview**: `SnapTo(ComputeFullBoardFraming(...))` called in InGameFlowPresenter.RunAsync (grep: `SnapTo` 2 occurrences in CameraController.cs, 3 in InGameFlowPresenter.cs).
- **Smoothly zooms into the first valid placement area**: `SetTarget` called after `UniTask.Delay(OverviewHoldDuration)` — gracefully skipped if no placeable positions.
- **Edge cases (extreme aspect ratios, very large boards) handled**: `ComputeFullBoardFraming` uses height-vs-width max with MinZoom/MaxZoom clamping; `ComputeFraming` does the same; empty input returns board-center fallback.
- **CameraConfig.asset wired into InGame scene**: Asset exists at `Assets/Data/CameraConfig.asset`; GUID `d5f1b80a58facb345bcacfdc5d3f5fac` confirmed present in `Assets/Scenes/InGame.unity`.
- **21 new EditMode tests total; 368/368 pass**: Confirmed by S03 verification.

### Code Changes — ✅ Verified
`git diff --stat eb451d8..HEAD -- ':!.gsd/'` shows 22 files changed, 4069 insertions(+).

## Definition of Done Results

## Definition of Done Results

| Item | Status | Evidence |
|------|--------|---------|
| All slices marked ✅ in roadmap | ✅ Done | S01 ✅, S02 ✅, S03 ✅ in M023-ROADMAP.md |
| All slice SUMMARY.md files exist | ✅ Done | S01-SUMMARY.md, S02-SUMMARY.md, S03-SUMMARY.md all present with `verification_result: passed` |
| All slice verifications passed | ✅ Done | S01 completed_at 2026-03-30T13:37Z, S02 at 13:50Z, S03 at 14:16Z — all `verification_result: passed` |
| Cross-slice integration works | ✅ Done | S01's SetTarget/CameraConfig API consumed by S02 (override) and S03 (level-start); S02's ClampToBounds consumed by S03's SetBoardBounds eager call; CameraConfig.asset wired end-to-end |
| No blocker_discovered in any slice | ✅ Done | S01: false, S02: false, S03: false |
| 368 EditMode tests pass | ✅ Done | Final run confirmed 368/368 passed, 0 failed per S03 summary |
| CameraConfig.asset in scene | ✅ Done | Asset exists; GUID confirmed in InGame.unity |
| Non-.gsd/ code changes committed | ✅ Done | 22 files changed including all key camera system files |

## Requirement Outcomes

## Requirement Outcomes

### R060 — Real puzzle board with piece placement
- **Status**: Active → Active (advanced, not yet validated)
- **Evidence**: M023 wired the camera layer of the tap-driven gameplay loop (HandlePiecePlaced → camera auto-tracking pipeline). This advances the completeness of R060's gameplay loop but full validation requires end-to-end playtesting confirming the complete piece-placement interaction feels correct. Status remains `active`.
- **Transition**: No status change — advancement recorded in context only.

## Deviations

["T03 (S01) test runner confirmation was delayed to slice-close time — Unity MCP was offline during task execution; no code impact.", "CameraConfig wiring in SceneSetup placed before SaveScene (end of CreateInGameScene) rather than immediately after AddComponent<CameraController> — functionally equivalent and matches the late-wiring convention for all other asset refs.", "rg (ripgrep) replaced with grep throughout all verification commands due to Windows OS error 123 (K012) — no impact on verification validity.", "DeckView unparenting fix added as a post-S03 commit (3cc3e11) — SceneSetup regeneration had cleared DeckView's parent; fix restored it as child of InGameCamera."]

## Follow-ups

["Tune CameraConfig.OverviewHoldDuration (default 1.0s) and SmoothTime (default 1.2s) once playtesting feedback is available — these are designer-tunable via the asset inspector.", "SetBoardBounds is called once on first piece placement (S02 pattern) — if level-restart support is added, the one-time guard in HandlePiecePlaced should be removed or reset at SpawnLevel time.", "SceneSetup should be extended to automatically re-parent DeckView as a child of InGameCamera after scene regeneration — currently this is a manual fix step.", "CameraConfig.asset is wired via SceneSetup — ensure the asset GUID is stable in source control and not regenerated on Unity editor reimport."]
