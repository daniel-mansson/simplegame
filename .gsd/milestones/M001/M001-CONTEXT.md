# M001: MVP UI Architecture Foundation

**Gathered:** 2026-03-15
**Status:** Ready for planning

## Project Description

A Unity 6 (6000.3.10f1) project establishing a strict MVP-based UI architecture with screen management, popups, input blocking, and transitions. The architecture enforces view independence, plain C# presenters, explicit dependency injection, and zero static state.

## Why This Milestone

This is the foundation milestone. Every future feature, screen, and system depends on these patterns being correct. Getting MVP separation, screen navigation, and the initialization flow right now prevents costly rewrites later. The "no static state" and "view independence" constraints are load-bearing — they enable domain-reload-disabled mode and a future view preview tool.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Enter play mode from the boot scene, see the main menu, navigate to settings and back, open and dismiss a popup — with smooth fade transitions throughout
- Read the codebase and trace every dependency from boot to presenter without encountering singletons, static state, or hidden wiring
- Run edit-mode tests that verify presenter logic using mocked view interfaces, without entering play mode

### Entry point / environment

- Entry point: Boot scene in Unity Editor → Play Mode
- Environment: Unity 6000.3.10f1 editor, local development
- Live dependencies involved: none (self-contained)

## Completion Class

- Contract complete means: edit-mode tests pass for presenter construction, screen manager navigation, popup stack, and factory wiring
- Integration complete means: boot scene → main menu → settings → back → popup → dismiss works in play mode with real scene loading and transitions
- Operational complete means: none (editor-only, no deployment)

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Boot scene initializes all services, constructs the UI factory, and transitions to the main menu screen via the screen manager with a fade transition
- User can navigate Main Menu → Settings → Main Menu with fade transitions and proper scene loading/unloading
- A popup can be opened from a screen, stacks correctly, blocks input on the screen below, and can be dismissed
- No static fields holding state exist anywhere in the project (grep verification)
- All dependencies are traceable from boot to presenter via constructor/init injection
- Edit-mode tests pass verifying presenter isolation, screen manager logic, and factory construction

## Risks and Unknowns

- **MVP wiring complexity** — Getting the factory, presenter construction, and view interface pattern right on the first pass is the highest risk. If the pattern is awkward, everything downstream suffers.
- **Hybrid scene management** — Additive scene loading with a persistent scene needs careful lifecycle management (what loads when, what owns what, cleanup on navigation).
- **UniTask integration** — First-time setup in the project. Need to verify UPM git URL install works with Unity 6000.3.10f1.

## Existing Codebase / Prior Art

- Empty project — no existing code, patterns, or constraints to work around.

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R001-R007 — Core MVP pattern, view independence, interfaces, factory, injection, no static state, domain services
- R008-R010 — Boot flow, hybrid scene management, screen navigation
- R011-R013 — Popup stack, input blocker, fade transitions
- R014-R017 — UniTask, edit-mode tests, demo screens, isolation testing

## Scope

### In Scope

- Unity project creation with 6000.3.10f1
- Core MVP types: IView interfaces, Presenter base, Model/Service base patterns
- Central UIFactory
- ScreenManager with additive scene loading
- PopupManager with stack-based popups
- InputBlocker (full-screen raycast blocker)
- TransitionManager (fade overlay)
- Boot scene with initialization flow
- 2-3 demo screens (MainMenu, Settings, game placeholder)
- A demo popup
- Edit-mode tests for presenters, screen manager, factory
- UniTask setup via UPM

### Out of Scope / Non-Goals

- DI framework (Zenject, VContainer, etc.)
- UI Toolkit / UXML
- Static singletons or static state
- View preview tool (deferred — R018)
- Play-mode tests (deferred — R019)
- Any actual game logic
- Deployment or build pipeline
- Audio, analytics, or other cross-cutting concerns

## Technical Constraints

- Unity 6000.3.10f1 — must use this exact version
- uGUI (Canvas/GameObject-based UI) — not UI Toolkit
- No static fields holding state — domain reload disabled must work
- No DI framework — constructor/init injection only
- UniTask for all async operations
- Edit-mode tests preferred over play-mode

## Integration Points

- UniTask (Cysharp.Threading.Tasks) — async scene loading, transitions, popup animations
- Unity SceneManager — additive scene loading/unloading
- Unity Canvas/EventSystem — uGUI rendering and input

## Open Questions

- Exact folder structure convention (Scripts/Core/, Scripts/UI/, Scripts/Screens/ or similar) — agent's discretion, keep it flat and conventional
- Whether screen manager uses string scene names or an enum — will decide during S02 planning based on what feels cleanest
