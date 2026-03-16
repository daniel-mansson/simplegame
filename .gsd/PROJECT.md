# Project

## What This Is

A Unity 6 (6000.3.10f1) project building a clean MVP-based UI architecture with a working game loop. Started as an architecture foundation (screen management, popups, transitions, MVP pattern), now extending into a real — if deliberately simple — game that proves the architecture carries actual gameplay: meta-progression, scene-to-scene context passing, outcome handling, and state reflection.

## Core Value

A proven, testable UI architecture where views are fully independent, presenters are plain C# classes, every layer can be tested in isolation — and a complete game loop (menu → play → outcome → menu reflects progress) demonstrates the architecture works for real gameplay, not just navigation demos.

## Current State

**M003 complete.** SceneController architecture established. Each scene has a MonoBehaviour SceneController with `RunAsync()` that loops internally until navigation away is decided. All control flow is linear. 58/58 edit-mode tests passing. `SimpleGame.Core` (game-agnostic framework) and `SimpleGame.Game` (game-specific code) are separate assemblies.

**M001–M002 complete.** MVP pattern, screen management, popup system, transitions, input blocking, assembly separation, feature-cohesive folder structure — all proven and tested.

## Architecture / Key Patterns

- **MVP pattern**: Views (MonoBehaviour/uGUI) expose interfaces → Presenters (plain C#) consume view interfaces and domain services → Models/Services encapsulate domain logic
- **Assembly separation**: `SimpleGame.Core` (game-agnostic framework) and `SimpleGame.Game` (game-specific code) are separate asmdefs; Core has no game-specific references
- **Generic managers**: `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` where `T : struct, System.Enum`
- **Feature cohesion**: Game code grouped by feature (`Game/MainMenu/`, `Game/Settings/`, `Game/Popup/`, `Game/Boot/`)
- **View independence**: Views have no references to presenters, models, or services
- **Explicit DI**: Constructor or Init method injection only. No DI framework, no static state, no singletons.
- **Central UI Factory**: `UIFactory` in `Game/Boot/` constructs all game presenters
- **SceneController pattern**: Per-scene MonoBehaviour with `RunAsync()` — loops internally, returns `ScreenId` for navigation. GameBootstrapper drives the navigation loop.
- **Hybrid scene management**: Persistent Boot scene with additive scene loading for screen scenes
- **UniTask**: All async operations use UniTask
- **Popup pre-instantiation**: `UnityPopupContainer` shows/hides pre-instantiated popup GameObjects in Boot scene via `SetActive`
- **Presenter results**: Presenters expose awaitable result methods (`WaitForAction`, `WaitForBack`, `WaitForConfirmation`) — no outbound callbacks
- **Boot injection**: `BootInjector` `[RuntimeInitializeOnLoadMethod]` loads Boot additively if not present (play-from-any-scene)
- **Testing**: `SimpleGame.Tests.Core` (32 tests) and `SimpleGame.Tests.Game` (26 tests); 58/58 passing

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — screen management, popups, transitions, MVP pattern, demo screens
- [x] M002: Assembly Restructure — Core/Game separation, generic managers, feature-cohesive folders
- [x] M003: SceneController Architecture — async control flow, linear scene controllers, boot-from-any-scene
- [ ] M004: Game Loop — meta-progression, context passing, InGame scene, win/lose flow, full menu→play→outcome→menu loop
