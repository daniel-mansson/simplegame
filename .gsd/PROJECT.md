# Project

## What This Is

A Unity 6 (6000.3.10f1) project establishing a clean MVP-based UI architecture foundation. The project provides screen management, popup support, input blocking, and transition infrastructure — all built on a strict Model-View-Presenter pattern with explicit dependency injection and no static state.

## Core Value

A proven, testable UI architecture where views are fully independent (no backward references to systems/services), presenters are plain C# classes constructed via a central factory, and every layer can be tested in isolation. The pattern must support Unity's domain-reload-disabled mode.

## Current State

All five S01–S05 slices complete. M001 MVP UI Architecture Foundation is fully assembled. Unity 6000.3.4f1 project compiling with UniTask installed. Full dependency chain: Boot.unity at EditorBuildSettings index 0 → GameBootstrapper initializes GameService + UnitySceneLoader + ScreenManager + PopupManager + UIFactory with closures → ShowScreenAsync(MainMenu) → MainMenuView/SettingsView wired via FindFirstObjectByType → presenter Initialize(). 49/49 NUnit edit-mode tests passing. Three scenes: Boot (GameBootstrapper, UnityInputBlocker sort=100, UnityTransitionPlayer sort=200, UnityPopupContainer sort=300, ConfirmDialogView pre-instantiated), MainMenu (Canvas + MainMenuView + Settings button + Open Popup button), Settings (Canvas + SettingsView + Back button). Static guard clean. No UnityEngine in Core.

**Pending:** Play-mode UAT walkthrough (human-verified): Boot → MainMenu → Settings → MainMenu → popup open → popup dismiss. See `.gsd/milestones/M001/slices/S05/S05-UAT.md`.

## Architecture / Key Patterns

- **MVP pattern**: Views (MonoBehaviour/uGUI) expose interfaces → Presenters (plain C#) consume view interfaces and domain services → Models/Services encapsulate domain logic
- **View independence**: Views have no references to presenters, models, or services. One interface per view that the presenter depends on.
- **Explicit DI**: Constructor or Init method injection only. No DI framework, no static state, no singletons.
- **Central UI Factory**: One factory constructs all presenters, receiving all dependencies and wiring the correct ones to each presenter.
- **Hybrid scene management**: Persistent Boot scene with additive scene loading for screen scenes (MainMenu, Settings).
- **UniTask**: All async operations (transitions, scene loading, popup animations, boot sequence) use UniTask.
- **Testing**: Edit-mode tests preferred (49/49 passing). Views can use play-mode tests (deferred R019). Each layer testable in isolation.
- **Presenter callbacks**: Presenters receive Action<ScreenId>/Action<PopupId>/Func<UniTask> callbacks from UIFactory, not full manager references — keeps presenters independently testable.
- **Popup pre-instantiation**: UnityPopupContainer shows/hides pre-instantiated popup GameObjects in Boot scene via SetActive — avoids popup scene management complexity.

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — S01 ✅ · S02 ✅ · S03 ✅ · S04 ✅ · S05 ✅ (pending play-mode UAT)
