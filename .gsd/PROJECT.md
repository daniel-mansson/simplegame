# Project

## What This Is

A Unity 6 (6000.3.10f1) project establishing a clean MVP-based UI architecture foundation. The project provides screen management, popup support, input blocking, and transition infrastructure — all built on a strict Model-View-Presenter pattern with explicit dependency injection and no static state.

## Core Value

A proven, testable UI architecture where views are fully independent (no backward references to systems/services), presenters are plain C# classes constructed via a central factory, and every layer can be tested in isolation. The pattern must support Unity's domain-reload-disabled mode.

## Current State

**M001 complete.** All five S01–S05 slices complete and verified. M001 SUMMARY written. Unity 6000.3.4f1 project compiling with UniTask installed. Full dependency chain: Boot.unity at EditorBuildSettings index 0 → GameBootstrapper initializes GameService + UnitySceneLoader + ScreenManager + PopupManager + UIFactory with closures → ShowScreenAsync(MainMenu) → MainMenuView/SettingsView wired via FindFirstObjectByType → presenter Initialize(). 49/49 NUnit edit-mode tests passing. Three scenes: Boot (GameBootstrapper, UnityInputBlocker sort=100, UnityTransitionPlayer sort=200, UnityPopupContainer sort=300, ConfirmDialogView pre-instantiated), MainMenu (Canvas + MainMenuView + Settings button + Open Popup button), Settings (Canvas + SettingsView + Back button). Static guard clean. No UnityEngine in Core.

**Requirement status:** 9 validated (R003, R005, R006, R011, R012, R013, R015, R017 + previously R003, R005), 9 active (pending play-mode UAT: R001, R002, R004, R007, R008, R009, R010, R014, R016).

**Pending human UAT:** Play-mode walkthrough: Boot → MainMenu → Settings → MainMenu → popup open → popup dismiss. See `.gsd/milestones/M001/slices/S05/S05-UAT.md`. Completing this UAT will advance R001, R002, R004, R007, R008, R009, R010, R014, R016 to validated.

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
- [ ] M002: Assembly Restructure — Core/Game Separation — split into game-agnostic Core assembly and game-specific Game assembly; generic ScreenManager/PopupManager; feature cohesion in Game; split test assemblies
