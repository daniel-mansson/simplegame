# Project

## What This Is

A Unity 6 (6000.3.10f1) project establishing a clean MVP-based UI architecture foundation. The project provides screen management, popup support, input blocking, and transition infrastructure — all built on a strict Model-View-Presenter pattern with explicit dependency injection and no static state.

## Core Value

A proven, testable UI architecture where views are fully independent (no backward references to systems/services), presenters are plain C# classes constructed via a central factory, and every layer can be tested in isolation. The pattern must support Unity's domain-reload-disabled mode.

## Current State

S01 complete. Unity 6000.3.4f1 project compiling with UniTask installed (git URL, resolved at commit ad5ed25e82a3). MVP base types defined: IView, Presenter<TView>, ISampleView, SamplePresenter, UIFactory, GameService. Assembly definitions in place for runtime and edit-mode tests. 6 NUnit edit-mode tests passing in Unity batchmode CLI — TestResults.xml: result="Passed", total="6", failed="0". No static state in any C# file. All core types are pure C# with no UnityEngine coupling.

Next: S02 — Screen Management (ScreenManager with additive scene loading).

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

- [x] M001: MVP UI Architecture Foundation — S01 ✅ (core MVP infra + tests passing) · S02–S05 pending
