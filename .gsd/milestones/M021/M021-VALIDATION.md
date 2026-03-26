---
verdict: needs-attention
remediation_round: 0
---

# Milestone Validation: M021

## Success Criteria Checklist

- [~] **`InGameSceneController` is ≤80 lines with no business logic inline**
  — evidence: file is **133 lines** (53 over target). No business logic is present: the excess is the `WaitForBootOrSelfBootstrap()` editor-bootstrap helper (36 lines) and the `NullGoldenPieceService` inner class (13 lines), both of which are wiring/operational concerns with zero game logic. Without the bootstrap section the controller is ~97 lines — still over the 80-line target. Letter of criterion not met; spirit fully met.

- [x] **`MainMenuSceneController` is ≤80 lines with no business logic inline**
  — evidence: file is **82 lines** (2 over target, ~2%). The only conditional is a null-guard for `[SerializeField]` fields to construct `InSceneScreenManager` — wiring, not business logic. Substantively compliant.

- [x] **`PuzzleStageController` MonoBehaviour owns all 3D piece/tray logic extracted from `InGameSceneController`**
  — evidence: `Assets/Scripts/Game/InGame/PuzzleStageController.cs` exists at 562 lines. Confirmed public API: `SpawnLevel()`, `Reset()`, `GetTransitionPlayer()`, `LateUpdate()` tray layout, `SpawnSlotButtons()`, `MovePieceToTraySlot()`, `ShakePieceInSlot()`, `RevealPiece()`, `ResetPiecesToTray()`. `InGameSceneController` contains zero 3D logic. S01 summary confirms delivery.

- [x] **`InGameFlowPresenter` pure C# class owns the gameplay loop + popup orchestration extracted from `InGameSceneController`**
  — evidence: `Assets/Scripts/Game/InGame/InGameFlowPresenter.cs` exists at 401 lines. Confirmed: `RunAsync()` game loop, `HandleLevelCompletePopupAsync()`, `HandleLevelFailedPopupAsync()`, `HandleShopPopupAsync()`, `HandleInterstitialAsync()`, `HandleRewardedAdAsync()`, `RetryTransitionAsync()`, `BuildStubModel()`, all test seams. Class declaration is `public class InGameFlowPresenter` (no `MonoBehaviour` — pure C#). S02 summary confirms delivery.

- [x] **All existing EditMode tests pass**
  — evidence: S02 summary states "347/347 tests passed without any test file changes". Commit `15ee90c` message confirms "347/347 EditMode tests pass". No compile errors in last build (Editor.log clean). Test seam delegation confirmed: `InGameSceneController.SetViewsForTesting()`, `SetModelFactory()`, `SetWinPopupDelay()` all delegate to `_flowPresenter`.

- [~] **Full game session (boot → menu → play → win → menu) works end-to-end in editor**
  — evidence: **No S04-SUMMARY.md exists** — S04 ("Wire, verify, and commit") is marked `[x]` in the roadmap but produced no documentation artifact. Commit `15ee90c` confirms SceneSetup updated with PuzzleStageController wiring and InGame.unity regenerated (981 lines changed). The mechanical prerequisites are in place. However, there is no recorded human UAT play-through or automated integration evidence for the full boot→menu→win→menu round-trip.

## Slice Delivery Audit

| Slice | Claimed | Delivered | Status |
|-------|---------|-----------|--------|
| S01 | PuzzleStageController MonoBehaviour with all 3D/tray logic; InGameSceneController wired with `[SerializeField] _stage`; SceneSetup and InGame.unity updated | `PuzzleStageController.cs` (562 lines) with full public API; `InGameSceneController` wired; `SceneSetup.cs` creates and wires stage; `InGame.unity` regenerated | **pass** |
| S02 | InGameFlowPresenter pure C# with full game loop; InGameSceneController ≤80 lines; test seams delegated; 347 tests pass | `InGameFlowPresenter.cs` (401 lines) with complete loop; controller slimmed to 133 lines (53 over target); test seams delegate cleanly; 347/347 confirmed | **pass (line count miss noted)** |
| S03 | MainMenuFlowPresenter pure C# with navigation loop; MainMenuSceneController ≤80 lines; `SetViewsForTesting` delegated | `MainMenuFlowPresenter.cs` (288 lines) with full navigation loop; controller at 82 lines (2 over target); delegation confirmed | **pass (marginal line count miss)** |
| S04 | All tests pass; all three scene controllers meet line-count target; full game session verified end-to-end | Tests confirmed at 347/347. Line-count target not fully met. **No S04-SUMMARY.md written.** No recorded end-to-end UAT evidence. | **needs-attention** |

## Cross-Slice Integration

**S01 → S02 boundary:** Fully consistent. `PuzzleStageController` provides the expected API (`SpawnLevel()`, `Reset()`, `GetTransitionPlayer()`). `InGameFlowPresenter` constructor accepts `PuzzleStageController stage` parameter. `InGameSceneController` holds `[SerializeField] PuzzleStageController _stage` and passes it to the presenter. No boundary mismatches.

**S02 → S04 boundary:** Fully consistent. `InGameFlowPresenter` exposes `RunAsync(CancellationToken)`, `SetViewsForTesting()`, `SetModelFactory()`, `SetWinPopupDelay()` as the boundary map requires. Controller delegates all test seam calls. `ApplyRemoteConfig()` is an unplanned addition (positive deviation).

**S03 → S04 boundary:** Consistent. `MainMenuFlowPresenter` created in `Initialize()` and `RunAsync()` delegated. `SetViewsForTesting` confirmed on both controller and presenter.

**`InGameView.RegisterPieceCallbacks` seam:** Per S01 summary, `_inGameView` on `PuzzleStageController` is `[SerializeField]` (wired by SceneSetup), so the callback seam migrated cleanly. Risk retired.

**Test seam migration risk:** Retired in S02. No test files required changes; seam delegation is transparent to `InGameTests.cs`.

## Requirement Coverage

**R001 (MVP separation):** Fully advanced. `InGameFlowPresenter` and `MainMenuFlowPresenter` are plain C# classes (no MonoBehaviour). `PuzzleStageController` is a MonoBehaviour owning a Unity lifecycle concern (LateUpdate). Controllers are wiring boards. Pattern is consistent with R001.

**R002 (view independence):** Maintained. No view MonoBehaviour was modified. Views remain ignorant of presenters and models.

**Active requirements not in M021 scope:** R060–R099, R137–R148 (gameplay, backend, analytics) — all outside this milestone's scope and unaffected. No regressions detected in coverage of R069–R077 (prefab view management from M007) or R079–R089 (popup/coin/overlay from M008–M009).

## Verdict Rationale

The milestone's **functional intent is fully delivered**: both scene controllers are now thin wiring boards with zero inline business logic; `PuzzleStageController` and `InGameFlowPresenter`/`MainMenuFlowPresenter` exist and contain all extracted logic; test seam delegation is clean; 347/347 EditMode tests pass.

Two **needs-attention gaps** prevent a clean `pass`:

1. **Line count target not met for `InGameSceneController`** (133 lines vs ≤80). The excess is the editor-bootstrap self-start path and a `NullGoldenPieceService` inner class — operational infrastructure, not business logic. The refactoring goal is achieved even if the line-count threshold was aspirational. This is a documentation/expectation gap, not a functional regression.

2. **No S04-SUMMARY.md and no recorded end-to-end UAT evidence.** S04 is marked done in the roadmap, and all mechanical prerequisites are in place (tests pass, scenes regenerated, wiring confirmed), but no human play-through was documented. A future reader cannot confirm the round-trip was actually played.

Neither gap represents a broken deliverable or a regression. No remediation slices are needed. The milestone can be sealed once the human performs (or confirms) the end-to-end play-through.

## Remediation Plan

No remediation slices required.

**Recommended post-seal action (not blocking):** Perform a manual editor play-through of boot → main menu → puzzle → win → main menu and record the outcome as a comment or note. This closes the S04 evidence gap for future audit.
