# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity mobile jigsaw puzzle game. Players draw pieces from a deck into slots, then tap to place them on a board — placement succeeds only if the piece has a placed neighbour. The game uses a pure C# domain model with an MVP presentation layer.

## Running Tests

Tests are NUnit EditMode tests in `Assets/Tests/EditMode/`. On Windows, the standard MCP test runner crashes (UV_HANDLE_CLOSING — see K006 in `.gsd/KNOWLEDGE.md`). Use the stdin workaround:

```bash
# Start test run
echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin

# Poll for results (replace <job_id> with returned ID)
mcporter call unityMCP.get_test_job job_id=<job_id>
```

There is no CLI test runner outside of Unity MCP. The Unity Editor test runner (`Window > General > Test Runner`) also works.

## Building

Build automation uses Fastlane (Ruby). All commands run from project root:

```bash
bundle exec fastlane ios build [dry_run:true]
bundle exec fastlane android build [dry_run:true]
```

Unity CLI builds are invoked via `Assets/Editor/BuildScript.cs`. Secrets come from env vars or `.env.local` (gitignored). See `fastlane/README.md` for full setup.

## Architecture

### Assembly Structure (dependency order)

1. **SimpleGame.Puzzle** — Pure C# domain model (`noEngineReferences: true`). Contains `PuzzleModel`, `PuzzleBoard`, `Deck`, `SolvableShuffle`. No Unity dependencies at all.
2. **SimpleGame.Core** — UI framework: MVP interfaces, `PopupManager<TPopupId>`, `ScreenManager<TScreenId>`, `TransitionManagement`. No game logic.
3. **SimpleGame.Game** — Game layer: scene controllers, presenters, all services (PlayFab, Ads, IAP, Economy, Save, Progression). References Core, Puzzle, and SimpleJigsaw.
4. **SimpleJigsaw.Runtime** — Submodule package at `Packages/simple-jigsaw/`. Board mesh generation and piece rendering.
5. **SimpleGame.Editor** — Editor tooling: `SceneSetup.cs` (programmatic scene construction), `BuildScript.cs`, prefab setup.

### Key Patterns

- **MVP with zero backward references**: Views (MonoBehaviours) expose interfaces (`IView`, `IPopupView`). Presenters are plain C# classes. Views never reference presenters, models, or services.
- **UIFactory**: Central wiring point that constructs all presenters with correct dependencies.
- **Scene controllers as wiring boards**: `InGameSceneController` and `MainMenuSceneController` are thin composition roots (~100 lines) — no game logic. `InGameFlowPresenter` owns the game loop, `PuzzleStageController` owns 3D piece management.
- **Event-driven domain model**: `PuzzleModel` fires `OnPiecePlaced`, `OnSlotChanged`, `OnCompleted`, `OnRejected`. Presenters subscribe reactively.
- **Additive scene loading**: Boot scene is always persistent. MainMenu, InGame, Settings loaded additively via `ScreenManager`.
- **UniTask everywhere**: All async operations use UniTask. SDK callbacks (PlayFab, Unity Ads) wrapped with `UniTaskCompletionSource`.
- **Optional constructor params for scene dependencies**: Scene-level objects (camera, stage controller) passed as optional params (default null) with compound null-guards. This keeps EditMode tests working without scene objects.

### Critical Boundaries

- **JigsawLevelFactory** (`Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs`) is the **sole file** allowed to import `SimpleJigsaw.*` types. All other game code works through domain interfaces.
- **IMetaSaveService** is synchronous. Cloud sync (PlayFab) is a separate async layer in `GameBootstrapper`.

## Scene Setup

Scenes are constructed programmatically by `Assets/Editor/SceneSetup.cs`. After changing any `[SerializeField]` on Boot scene MonoBehaviours (like `GameBootstrapper`), you **must** re-run `Tools/Setup/Create And Register Scenes` from the Unity Editor menu and commit the resulting `.unity` file changes.

SceneSetup regeneration replaces scene contents — manually-set parent-child relationships not encoded in SceneSetup will be lost. Always encode parenting in SceneSetup for idempotent re-runs.

## Test Structure

- `Tests/EditMode/Puzzle/` — Pure domain model tests (no Unity deps)
- `Tests/EditMode/Core/` — PopupManager, ScreenManager, transitions, MVP wiring
- `Tests/EditMode/Game/` — Scene controllers, InGame flow, popup presenters, e2e wiring

When adding members to `IInputBlocker` or popup view interfaces, update all mock implementations in test files (see K004 in `.gsd/KNOWLEDGE.md` for the full list).

## GSD Planning System

The `.gsd/` directory contains project planning artifacts:
- **DECISIONS.md** — Append-only register (113+ decisions). Never edit existing rows; add new rows to supersede.
- **KNOWLEDGE.md** — Project-specific rules and workarounds (K001–K015). Read this for known pitfalls.
- **REQUIREMENTS.md** — Capability contract with requirement status and coverage mapping.
- **PROJECT.md** — Feature summary, milestone sequence, architecture overview.

## Windows-Specific Notes

- Use `python3` one-liners instead of `grep`/`test`/`find` in Bash tool calls — these Unix builtins are unreliable in the Git Bash environment (K013).
- `rg` can fail with OS error 123 on Windows with forward-slash directory paths. Use the Grep tool or `grep -c` for single-file checks (K012).
- Bee compiler can get stuck on stale content hashes. Delete active `.dag` files in `Library/Bee/` to force rebuild (K011).
