# M021: Scene Controller Composition Refactor

**Gathered:** 2026-03-26
**Status:** Ready for planning

## Project Description

A Unity jigsaw puzzle game. The main game loop lives across three scene controllers (InGame, MainMenu, Settings) wired via GameBootstrapper. MVP pattern is the foundational architecture: views are MonoBehaviours, presenters are plain C#, models are domain services.

## Why This Milestone

Scene controllers have accreted logic that belongs in presenters. `InGameSceneController` is 1085 lines — it directly owns: 3D piece spawning, tray slot layout in LateUpdate, UGUI button creation, all popup orchestration (LevelComplete, LevelFailed, Shop, RewardedAd, Interstitial), retry transitions, piece tracking dictionaries, and editor play-from-editor bootstrapping. `MainMenuSceneController` is 391 lines, similarly doing environment logic, screen manager construction, and debug ad flows inline.

The user's intent: scene controllers should be thin wiring boards. `[SerializeField]` fields hook up Unity views (and stage components) to presenters. The controller's only runtime job is calling `Initialize()` and `RunAsync()`. All business logic lives in presenters.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Open any scene controller and see only `[SerializeField]` fields + an `Initialize()` delegation + a `RunAsync()` delegation — no business logic inline
- Open the gameplay loop logic in a focused pure-C# class (`InGameFlowPresenter`) without wading through Unity scene lifecycle code
- Open `PuzzleStageController` to find all 3D piece/tray logic isolated in one MonoBehaviour

### Entry point / environment

- Entry point: Unity Editor, Play from Boot scene or any scene (BootInjector handles the rest)
- Environment: local Unity Editor
- Live dependencies involved: GameBootstrapper wires all services before calling Initialize()

## Completion Class

- Contract complete means: all tests pass; scene controllers are ≤80 lines each
- Integration complete means: Boot scene → MainMenu → InGame → win/lose/retry all work end-to-end
- Operational complete means: play-from-editor direct entry still works for InGame scene

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Full game session (boot → main menu → play → win → main menu) works without error
- `InGameSceneController` ≤80 lines, `MainMenuSceneController` ≤80 lines, `SettingsSceneController` unchanged
- All existing EditMode tests pass

## Risks and Unknowns

- `InGameView.RegisterPieceCallbacks` is currently the seam between controller and view for 3D positioning — this coupling needs care when moving to `PuzzleStageController`
- Test seams (`SetViewsForTesting`, `SetModelFactory`, `SetWinPopupDelay`) currently live on the scene controller — they must move to `InGameFlowPresenter` where the tested logic moves
- `GameBootstrapper.Initialize()` call sites for InGame/MainMenu need updating once the controller signatures slim down

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — 1085 lines, source of S01 + S02 extraction
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — model↔view event wiring; stays as-is
- `Assets/Scripts/Game/InGame/InGameView.cs` — MonoBehaviour view; `RegisterPieceCallbacks` is the key seam
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — 391 lines, source of S03 extraction
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — already handles all view events
- `Assets/Scripts/Game/Settings/SettingsSceneController.cs` — 61 lines, already the target shape; untouched
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — calls `Initialize()` on each scene controller; needs updating for new signatures
- `Assets/Tests/EditMode/Game/InGameTests.cs` — 591 lines; tests call `ctrl.Initialize()`, `ctrl.SetViewsForTesting()`, `ctrl.SetModelFactory()` — these must migrate to the new presenter
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — 225 lines; tests MainMenu + Settings controllers

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R001 — MVP pattern with strict separation; this milestone enforces it more fully in scene controllers
- R002 — View independence; scene controllers must not do view logic

## Scope

### In Scope

- Extract `PuzzleStageController` MonoBehaviour from `InGameSceneController` — all 3D piece/tray logic
- Extract `InGameFlowPresenter` pure C# class from `InGameSceneController` — gameplay loop, popup orchestration, retry flow
- Slim `InGameSceneController` to `[SerializeField]` fields + delegation only
- Slim `MainMenuSceneController` — move environment helper logic and debug ad flows into `MainMenuPresenter` or inline into the controller's `RunAsync` as simple one-liner calls to the presenter
- Update `GameBootstrapper` call sites for new `Initialize()` signatures
- Migrate test seams from scene controllers to the new presenter classes
- All existing tests remain passing

### Out of Scope / Non-Goals

- Changing any gameplay mechanic
- Changing `SettingsSceneController` (already the right shape)
- Changing `InGamePresenter` (model↔view event wiring is correct)
- Changing `MainMenuPresenter` (view event handling is correct)
- Namespace changes
- Any new features

## Technical Constraints

- `git mv` for all file moves to preserve `.meta` GUIDs and scene wiring
- `PuzzleStageController` must remain a MonoBehaviour — it owns `LateUpdate` and scene `GameObject` references
- `InGameFlowPresenter` must be pure C# — no MonoBehaviour, no Unity lifecycle
- Test seams that were previously on scene controllers must be accessible on the new presenter classes

## Integration Points

- `GameBootstrapper` — calls `Initialize()` on each controller; update call sites after slim-down
- `InGameView.RegisterPieceCallbacks` — key seam; `PuzzleStageController` will call this instead of `InGameSceneController`
- `UIFactory` — may need new `CreateInGameFlowPresenter()` factory method; decide during S02 planning

## Open Questions

- None — user confirmed the decomposition: PuzzleStageController for 3D, InGameFlowPresenter for game loop, slim controllers for wiring
