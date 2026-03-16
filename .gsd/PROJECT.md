# Project

## What This Is

A Unity 6 (6000.3.10f1) project establishing a clean MVP-based UI architecture foundation. The project provides screen management, popup support, input blocking, and transition infrastructure — all built on a strict Model-View-Presenter pattern with explicit dependency injection and no static state.

## Core Value

A proven, testable UI architecture where views are fully independent (no backward references to systems/services), presenters are plain C# classes constructed via a central factory, and every layer can be tested in isolation. The pattern must support Unity's domain-reload-disabled mode.

## Current State

**M002 complete.** Assembly restructure done. `SimpleGame.Core.asmdef` (game-agnostic UI framework) and `SimpleGame.Game.asmdef` (game-specific code) are separate assemblies. `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` are generic (`where T : struct, System.Enum`). Game code uses feature-cohesive folder structure (`Game/MainMenu/`, `Game/Settings/`, `Game/Popup/`). Tests split into `SimpleGame.Tests.Core` (32 tests) and `SimpleGame.Tests.Game` (17 tests). 49/49 edit-mode tests passing.

**M001 complete** (pending play-mode UAT). All five S01–S05 slices complete and verified. Unity 6000.3.4f1 project compiling with UniTask. Full dependency chain intact. 49/49 NUnit edit-mode tests passing.

## Architecture / Key Patterns

- **MVP pattern**: Views (MonoBehaviour/uGUI) expose interfaces → Presenters (plain C#) consume view interfaces and domain services → Models/Services encapsulate domain logic
- **Assembly separation**: `SimpleGame.Core` (game-agnostic framework) and `SimpleGame.Game` (game-specific code) are separate asmdefs; Core has no game-specific references
- **Generic managers**: `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` where `T : struct, System.Enum` — Core is reusable in any Unity game
- **Feature cohesion**: Game code grouped by feature (`Game/MainMenu/`, `Game/Settings/`, `Game/Popup/`) — interface, presenter, and view co-located
- **View independence**: Views have no references to presenters, models, or services. One interface per view that the presenter depends on.
- **Explicit DI**: Constructor or Init method injection only. No DI framework, no static state, no singletons.
- **Central UI Factory**: `UIFactory` in `Game/Boot/` constructs all game presenters, receiving all dependencies at construction.
- **Hybrid scene management**: Persistent Boot scene with additive scene loading for screen scenes (MainMenu, Settings).
- **UniTask**: All async operations use UniTask.
- **Testing**: Two test assemblies — `SimpleGame.Tests.Core` (32 tests, framework) and `SimpleGame.Tests.Game` (17 tests, game); 49/49 passing. Core tests use `TestScreenId`/`TestPopupId` local enums; `ISampleView`/`SamplePresenter` are test fixtures in the Core test assembly.
- **Presenter callbacks**: Presenters receive `Action<ScreenId>`/`Action<PopupId>`/`Func<UniTask>` callbacks from UIFactory — keeps presenters independently testable.
- **Popup pre-instantiation**: `UnityPopupContainer` shows/hides pre-instantiated popup GameObjects in Boot scene via `SetActive`.

## Key Infrastructure Facts (learned during M001)

- `com.unity.test-framework` must be added manually to Packages/manifest.json — not in Unity default project.
- `-quit` must NOT be passed alongside `-runTests` in batchmode — races the async test runner.
- `com.unity.ugui` must be declared explicitly in manifest.json for Unity 6 uGUI types.
- `FindFirstObjectByType<T>()` replaces deprecated `FindObjectOfType<T>()` in Unity 6.
- Editor scripts referencing custom asmdef assemblies require their own explicit asmdef (e.g. SimpleGame.Editor.asmdef).
- Static-state grep guard does NOT cover `static string` return type — use `static void` + `out` params for editor factory helpers.

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — S01 ✅ · S02 ✅ · S03 ✅ · S04 ✅ · S05 ✅ · M001-SUMMARY ✅ (pending play-mode UAT)
- [x] M002: Assembly Restructure — Core/Game Separation — `SimpleGame.Core` (game-agnostic) + `SimpleGame.Game` (feature-cohesive) + split test assemblies; 49/49 tests
