# Project

## What This Is

A Unity 6 (6000.3.10f1) project establishing a clean MVP-based UI architecture foundation. The project provides screen management, popup support, input blocking, and transition infrastructure — all built on a strict Model-View-Presenter pattern with explicit dependency injection and no static state.

## Core Value

A proven, testable UI architecture where views are fully independent (no backward references to systems/services), presenters are plain C# classes constructed via a central factory, and every layer can be tested in isolation. The pattern must support Unity's domain-reload-disabled mode.

## Current State

S01, S02, and S03 complete. Unity 6000.3.4f1 project compiling with UniTask installed (git URL, resolved at commit ad5ed25e82a3). MVP base types defined: IView, Presenter<TView>, ISampleView, SamplePresenter, UIFactory, GameService. ScreenManagement layer complete: ScreenId enum, ISceneLoader interface, ScreenManager (history stack, ShowScreenAsync, GoBackAsync, concurrency guard), UnitySceneLoader. Popup system complete: PopupId enum, IInputBlocker interface (reference-counting contract), IPopupContainer interface, PopupManager (stack-based, concurrency-guarded), IPopupView marker interface, UnityInputBlocker MonoBehaviour (CanvasGroup reference-counting). MainMenu.unity and Settings.unity placeholder scenes registered in EditorBuildSettings. 27 NUnit edit-mode tests passing in Unity batchmode CLI — TestResults.xml: result="Passed", total="27", passed="27", failed="0". No static state in any C# file. Core types are pure C# with no UnityEngine coupling.

Next: S04 — Transition System (TransitionManager with fade-to-black between screens).

## Architecture / Key Patterns

- **MVP pattern**: Views (MonoBehaviour/uGUI) expose interfaces → Presenters (plain C#) consume view interfaces and domain services → Models/Services encapsulate domain logic
- **View independence**: Views have no references to presenters, models, or services. One interface per view that the presenter depends on.
- **Explicit DI**: Constructor or Init method injection only. No DI framework, no static state, no singletons.
- **Central UI Factory**: One factory constructs all presenters, receiving all dependencies and wiring the correct ones to each presenter.
- **Hybrid scene management**: Persistent scene with additive scene loading for major states.
- **UniTask**: All async operations (transitions, scene loading, popup animations) use UniTask.
- **Testing**: Edit-mode tests preferred. Views can use play-mode tests. Each layer testable in isolation.

## Capability Contract

See `.gsd/REQUIREMENTS.md` for the explicit capability contract, requirement status, and coverage mapping.

## Milestone Sequence

- [x] M001: MVP UI Architecture Foundation — S01 ✅ · S02 ✅ · S03 ✅ (popup system + input blocking, 27/27 tests passing) · S04–S05 pending
