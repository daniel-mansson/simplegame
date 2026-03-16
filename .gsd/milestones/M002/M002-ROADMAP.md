# M002: Assembly Restructure — Core/Game Separation

**Vision:** Reorganize the existing codebase into a game-agnostic `SimpleGame.Core` assembly and a game-specific `SimpleGame.Game` assembly. Core becomes reusable UI framework plumbing. Game holds all SimpleGame-specific code with interface/presenter/view co-located by feature. Tests split to mirror the source structure. No behavior changes — pure structural improvement.

## Success Criteria

- `SimpleGame.Core.asmdef` sources contain zero references to game-specific types (`IMainMenuView`, `ISettingsView`, `IConfirmDialogView`, `GameService`, `UIFactory`, `ScreenId`, `PopupId`)
- `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` are generic — game-agnostic
- Each game screen's interface, presenter, and view MonoBehaviour live in the same folder under `Game/`
- Two test assemblies: `SimpleGame.Tests.Core` and `SimpleGame.Tests.Game`
- All 49 edit-mode tests pass after restructure
- Static guard clean
- Project compiles in batchmode with exit 0

## Key Risks / Unknowns

- **Generic manager refactor** — `ScreenManager<TScreenId>` / `PopupManager<TPopupId>` touch all test sites and callers; enum generic constraint needs validation
- **Scene serialization** — MonoBehaviour moves must preserve `.meta` GUIDs or scenes break silently; files must be moved via `git mv` not raw copy
- **Namespace churn** — every file's namespace changes; wrong execution order causes cascading compile failures

## Proof Strategy

- Generic manager refactor → retire in S01 by compiling `ScreenManager<TScreenId>` with the existing 8 ScreenManagerTests updated and passing
- Scene serialization → retire in S02 by opening the project (or batchmode compile) and confirming scenes have no missing script references
- Namespace churn → managed by doing Core first (S01), Game second (S02), tests last (S03) — each slice compiles before the next begins

## Verification Classes

- Contract verification: batchmode compile exit 0; `TestResults.xml` total="49" passed="49" failed="0"; grep guard on Core sources
- Integration verification: Unity Editor opens project without missing script errors in Boot/MainMenu/Settings scenes
- Operational verification: none
- UAT / human verification: open Unity Editor, check for yellow/red console errors on project load

## Milestone Definition of Done

This milestone is complete only when all are true:

- `SimpleGame.Core.asmdef` and `SimpleGame.Game.asmdef` both exist and compile
- `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|GameService\|UIFactory\|ScreenId\|PopupId" Assets/Scripts/Core/` returns empty
- All 49 edit-mode tests pass: `SimpleGame.Tests.Core` + `SimpleGame.Tests.Game`
- Static guard returns empty
- `SimpleGame.Editor.asmdef` references `SimpleGame.Game` and compiles
- No missing script references in Boot, MainMenu, or Settings scenes (batchmode or Editor verification)

## Requirement Coverage

- Covers: R020, R021, R022, R023, R024, R025 (all new requirements introduced by this milestone)
- Partially covers: R015 (test restructure keeps edit-mode tests passing), R017 (test isolation maintained)
- Leaves for later: none
- Orphan risks: none

## Slices

- [x] **S01: Core assembly restructure + generic managers** `risk:high` `depends:[]`
  > After this: `SimpleGame.Core.asmdef` compiles; `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` exist; Core sources contain no game-specific type references (grep verifiable); Core-layer tests (ScreenManagerTests, PopupManagerTests, TransitionTests, base MVPWiringTests) updated and passing.

- [x] **S02: Game assembly + feature cohesion** `risk:medium` `depends:[S01]`
  > After this: `SimpleGame.Game.asmdef` compiles; all game-specific code (screens, presenters, views, boot, factory, service) lives under `Assets/Scripts/Game/` with each screen's interface+presenter+view co-located; `SimpleGame.Editor.asmdef` updated; project compiles end-to-end.

- [x] **S03: Test assembly split + full verification** `risk:low` `depends:[S02]`
  > After this: two test assemblies (`SimpleGame.Tests.Core`, `SimpleGame.Tests.Game`) compile and all 49 tests pass; `ISampleView`/`SamplePresenter` live in Core test assembly as fixtures; static guard clean; batchmode test run produces `TestResults.xml` with total="49" passed="49" failed="0".

## Boundary Map

### S01 → S02

Produces:
- `SimpleGame.Core.asmdef` — compiled assembly with name `SimpleGame.Core`
- `ScreenManager<TScreenId> where TScreenId : System.Enum` in `SimpleGame.Core.ScreenManagement`
- `PopupManager<TPopupId> where TPopupId : System.Enum` in `SimpleGame.Core.PopupManagement`
- All Core interfaces updated to new namespaces: `SimpleGame.Core.MVP`, `SimpleGame.Core.ScreenManagement`, `SimpleGame.Core.PopupManagement`, `SimpleGame.Core.TransitionManagement`
- Unity Core implementations updated: `SimpleGame.Core.Unity.ScreenManagement`, `SimpleGame.Core.Unity.PopupManagement`, `SimpleGame.Core.Unity.TransitionManagement`
- `ISampleView`/`SamplePresenter` removed from runtime sources (moved to test assembly in S03)

Consumes:
- nothing (restructure of existing code)

### S02 → S03

Produces:
- `SimpleGame.Game.asmdef` — compiled assembly with name `SimpleGame.Game`, references `SimpleGame.Core`
- `ScreenId`, `PopupId` enums in `SimpleGame.Game` namespace
- `IMainMenuView` + `MainMenuPresenter` + `MainMenuView` in `Assets/Scripts/Game/MainMenu/`
- `ISettingsView` + `SettingsPresenter` + `SettingsView` in `Assets/Scripts/Game/Settings/`
- `IConfirmDialogView` + `ConfirmDialogPresenter` + `ConfirmDialogView` + `UnityPopupContainer` in `Assets/Scripts/Game/Popup/`
- `GameService` in `Assets/Scripts/Game/Services/`
- `UIFactory` + `GameBootstrapper` in `Assets/Scripts/Game/Boot/`
- `SimpleGame.Editor.asmdef` updated to reference `SimpleGame.Game`
- Full project compile (Core + Game + Editor) clean

Consumes from S01:
- `SimpleGame.Core.asmdef` — `ScreenManager<TScreenId>`, `PopupManager<TPopupId>`, all Core interfaces
