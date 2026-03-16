# M004: Game Loop — Meta-Progression, Context Passing, Win/Lose Flow

**Gathered:** 2026-03-16
**Status:** Ready for planning

## Project Description

Extend the existing MVP architecture sample into a working game loop. Main menu shows current level, launches gameplay with a level ID passed through a shared service. InGame scene receives that context, runs a simple score-incrementing game with explicit win/lose triggers, feeds the outcome back through a progression service, and the menu reflects updated state on return.

## Why This Milestone

The architecture (M001–M003) proved the UI framework works for screen management, popups, transitions, and presenter patterns. But it's still a navigation demo. This milestone proves the architecture can carry a real game loop: context passing between scenes, domain services that mutate state based on gameplay outcomes, and UI that reacts to that state. Without this, the architecture is unproven for its intended purpose.

## User-Visible Outcome

### When this milestone is complete, the user can:

- See "Level 1" on the main menu and press Play to start gameplay
- In the InGame scene, tap a button to increase a score counter, then press Win or Lose
- On Win: see a popup showing their score and level, continue to main menu which now shows "Level 2"
- On Lose: see a popup showing their score and level, choose Retry (score resets, replay same level) or Back (return to menu)
- Repeat the loop — level counter advances with each win

### Entry point / environment

- Entry point: Unity Editor play mode from Boot.unity or any game scene
- Environment: local dev (Unity Editor)
- Live dependencies involved: none

## Completion Class

- Contract complete means: all new services, presenters, and scene controllers have passing edit-mode tests; popup show/dismiss lifecycle tested; context passing proven by tests
- Integration complete means: GameBootstrapper's navigation loop handles InGame ↔ MainMenu transitions with proper context; popup container registers win/lose popups; UIFactory creates all new presenters
- Operational complete means: full game loop works in Unity Editor play mode — menu → play → outcome → menu reflects progress

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- The full loop works: Menu (Level 1) → Play → InGame → Win → Win Popup → Menu (Level 2) — in Unity Editor play mode
- The lose/retry flow works: InGame → Lose → Lose Popup → Retry → score resets, same level → Win → advances
- InGame scene can be played directly from editor with fallback/default level config
- All 58 existing tests still pass, plus new tests covering all new types

## Risks and Unknowns

- **Context passing via service** — new pattern for this codebase; GameSessionService must be readable by InGame scene controller before RunAsync starts. Risk: initialization ordering between Boot setup and scene controller read. Mitigated by: Boot sets context before ShowScreenAsync, scene controller reads in RunAsync.
- **InGame scene controller complexity** — handles gameplay loop + win/lose branching + retry. More complex than existing MainMenu/Settings controllers. Mitigated by: well-defined outcome enum, linear async flow.
- **Two new popup types** — WinDialog and LoseDialog added to PopupId, UnityPopupContainer switch, Boot scene. More popup wiring than M001 proved. Mitigated by: pattern is established and tested.

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — navigation loop, will add InGame case
- `Assets/Scripts/Game/Boot/UIFactory.cs` — factory, will add new Create methods
- `Assets/Scripts/Game/Boot/ISceneController.cs` — interface, unchanged (`RunAsync() → ScreenId`)
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — pattern reference for new InGame controller
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — will be extended with Play action + level display
- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs` — pattern reference for Win/Lose presenters
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — will add WinDialog/LoseDialog cases
- `Assets/Scripts/Game/Services/GameService.cs` — placeholder service, to be replaced/extended
- `Assets/Scripts/Game/ScreenId.cs` — will add `InGame` value
- `Assets/Scripts/Game/PopupId.cs` — will add `WinDialog` and `LoseDialog` values

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R029 — Game session context via shared service (primary: M004/S01)
- R030 — Progression service with in-memory level tracking (primary: M004/S01)
- R031 — Main menu displays current level and has Play button (primary: M004/S02)
- R032 — InGame scene receives level ID and is self-sufficient (primary: M004/S03)
- R033 — InGame gameplay — score counter + win/lose triggers (primary: M004/S03)
- R034 — Win popup with score + level, returns to main menu (primary: M004/S04)
- R035 — Lose popup with score + level, retry/back options (primary: M004/S04)
- R036 — Progression service logs score on win and advances level (primary: M004/S03)
- R037 — Play-from-editor bootstrapping for InGame scene (primary: M004/S03)
- R038 — Full game loop integration (primary: M004/S05)
- R039 — New popup types WinDialog/LoseDialog (primary: M004/S04)
- R040 — Edit-mode tests for all new types (primary: M004/S01, supporting all)

## Scope

### In Scope

- GameSessionService — holds current game context (level ID, score, outcome)
- ProgressionService — tracks current level, advances on win, logs score
- MainMenu extension — level display, Play button, session context setup
- InGame scene + scene controller — score counter, win/lose buttons, outcome handling
- WinDialog popup — shows score + level, continue to menu
- LoseDialog popup — shows score + level, retry or back
- InGame play-from-editor fallback (serialized field for default level)
- GameBootstrapper InGame case in navigation loop
- UIFactory extensions for new presenters
- Edit-mode tests for all new types

### Out of Scope / Non-Goals

- Disk persistence (deferred R041)
- Real level-specific content loading (deferred R042)
- Complex gameplay mechanics (out of scope R043)
- Play-mode tests (deferred R019)
- Changes to Core assembly — all new code goes in Game assembly

## Technical Constraints

- All new code in `SimpleGame.Game` assembly — Core stays untouched
- MVP pattern: views expose interfaces, presenters are plain C#, services are plain C#
- No static state, no singletons, no DI framework
- UniTask for all async operations
- Presenters expose awaitable result methods (D026), not outbound callbacks
- View→Presenter via `event Action` (D027)
- Feature-cohesive folders: `Game/InGame/`, `Game/Popup/` (win/lose alongside confirm)
- `RunAsync()` returns `ScreenId` — context flows through GameSessionService, not params

## Integration Points

- `GameBootstrapper` — adds `case ScreenId.InGame` to navigation loop, constructs services, passes to UIFactory
- `UIFactory` — receives GameSessionService + ProgressionService, creates InGame and popup presenters
- `ScreenId` enum — adds `InGame` value
- `PopupId` enum — adds `WinDialog` and `LoseDialog` values
- `UnityPopupContainer` — adds win/lose GameObject fields and switch cases
- `MainMenuPresenter` / `IMainMenuView` — extended with level display and Play action
- Boot scene — adds WinDialog and LoseDialog popup GameObjects (pre-instantiated, inactive)
- EditorBuildSettings — adds InGame scene

## Open Questions

- None — all gray areas resolved during discussion.
