# Project

## What This Is

A Unity 6 (6000.3.10f1) project with a clean MVP-based UI architecture and a working game loop. The architecture handles screen management, popups, transitions, and the MVP pattern. The game loop proves the architecture carries real gameplay: meta-progression, scene-to-scene context passing, outcome handling with win/lose popups, retry flow, and state reflection on the main menu.

## Core Value

A proven, testable UI architecture where views are fully independent, presenters are plain C# classes, every layer can be tested in isolation — and a complete game loop (menu → play → outcome → menu reflects progress) demonstrates the architecture works for real gameplay, not just navigation demos.

## Current State

**M005 complete.** Transition system upgraded: LitMotion tweening replaces manual while-loop, transition overlay lives in a self-contained swappable prefab. 98/98 tests passing.

**M001–M004 complete.** MVP pattern, screen management, popup system, transitions, input blocking, assembly separation, SceneController architecture, boot-from-any-scene, full game loop — all proven and tested.

## Architecture / Key Patterns

- **MVP pattern**: Views (MonoBehaviour/uGUI) expose interfaces → Presenters (plain C#) consume view interfaces and domain services → Models/Services encapsulate domain logic
- **Assembly separation**: `SimpleGame.Core` (game-agnostic framework) and `SimpleGame.Game` (game-specific code) are separate asmdefs; Core has no game-specific references
- **Generic managers**: `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` where `T : struct, System.Enum`
- **Feature cohesion**: Game code grouped by feature (`Game/MainMenu/`, `Game/Settings/`, `Game/Popup/`, `Game/InGame/`, `Game/Boot/`, `Game/Services/`)
- **View independence**: Views have no references to presenters, models, or services
- **Explicit DI**: Constructor or Init method injection only. No DI framework, no static state, no singletons.
- **Central UI Factory**: `UIFactory` in `Game/Boot/` constructs all game presenters
- **SceneController pattern**: Per-scene MonoBehaviour with `RunAsync()` — loops internally, returns `ScreenId` for navigation. GameBootstrapper drives the navigation loop.
- **Hybrid scene management**: Persistent Boot scene with additive scene loading for screen scenes (MainMenu, Settings, InGame)
- **UniTask**: All async operations use UniTask
- **Popup pre-instantiation**: `UnityPopupContainer` shows/hides pre-instantiated popup GameObjects in Boot scene via `SetActive`
- **Presenter results**: Presenters expose awaitable result methods (`WaitForAction`, `WaitForBack`, `WaitForConfirmation`, `WaitForContinue`, `WaitForChoice`) — no outbound callbacks
- **Boot injection**: `BootInjector` `[RuntimeInitializeOnLoadMethod]` loads Boot additively if not present (play-from-any-scene)
- **Service-mediated context**: `GameSessionService` passes context between scenes; controllers read what they need
- **In-memory progression**: `ProgressionService` tracks level, advances on win, logs score (no persistence)
- **Testing**: `SimpleGame.Tests.Core` (32 tests) and `SimpleGame.Tests.Game` (66 tests); 98/98 passing

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — screen management, popups, transitions, MVP pattern, demo screens
- [x] M002: Assembly Restructure — Core/Game separation, generic managers, feature-cohesive folders
- [x] M003: SceneController Architecture — async control flow, linear scene controllers, boot-from-any-scene
- [x] M004: Game Loop — meta-progression, context passing, InGame scene, win/lose flow, full menu→play→outcome→menu loop
- [x] M005: Prefab-Based Transitions — LitMotion tweening, prefab-driven transition visuals, extensible for future complex transitions
