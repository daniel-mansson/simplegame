# M002: Assembly Restructure — Core/Game Separation

**Gathered:** 2026-03-16
**Status:** Ready for planning

## Project Description

Unity 6 project with a complete MVP UI architecture (M001). All five slices delivered: screen navigation, popup stack, input blocking, fade transitions, three demo screens. 49/49 edit-mode tests passing.

## Why This Milestone

Core currently contains game-specific types (`IMainMenuView`, `ISettingsView`, `IConfirmDialogView`, their presenters, `ScreenId`, `PopupId`, `GameService`, `UIFactory`). The intent was always that Core should be game-agnostic UI framework plumbing — reusable in any Unity game. This milestone makes that true.

The secondary goal is cohesion: interfaces should live next to their implementations. Right now `IMainMenuView` is in `Core/MVP/` while `MainMenuView` (the MonoBehaviour) is in `Runtime/MVP/`. That split makes no sense once Core and Game are separated.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Drop `SimpleGame.Core.asmdef` into a different Unity project and it compiles with zero SimpleGame-specific references
- Navigate the Game folder and find `IMainMenuView`, `MainMenuPresenter`, and `MainMenuView` all in the same place
- Run all 49 edit-mode tests across two test assemblies (Core tests, Game tests)

### Entry point / environment

- Entry point: Unity Editor — project compiles, tests pass in batchmode
- Environment: Unity 6000.3.4f1, local development
- Live dependencies involved: none (pure restructure)

## Completion Class

- Contract complete means: both asmdefs compile; 49/49 tests pass; grep confirms no game-specific types in Core asmdef sources
- Integration complete means: Unity Editor opens the project without compile errors; all three scenes load correctly
- Operational complete means: none

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|GameService\|UIFactory\|ScreenId\|PopupId" Assets/Scripts/Core/` returns empty
- All 49 edit-mode tests pass across `SimpleGame.Tests.Core` and `SimpleGame.Tests.Game` assemblies
- Static guard clean: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns empty
- Project compiles in batchmode with exit 0

## Risks and Unknowns

- **Generic manager refactor** — `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` require touching all test sites and all callers. The generic constraint (`where TScreenId : struct`) needs to be validated — Unity/C# enum generics have limitations. Medium risk.
- **Namespace churn** — every file changes namespace. Tests that reference old namespaces break immediately. Order of operations matters: Core first, then Game, then tests.
- **asmdef GUID references** — Unity asmdef files can use name or GUID references. Current project uses name references (Decision #2). New asmdefs must follow the same convention.
- **Editor asmdef** — `SimpleGame.Editor.asmdef` currently references `SimpleGame.Runtime`. After restructure it must reference `SimpleGame.Game` (which holds the MonoBehaviours SceneSetup needs).
- **Scene serialization** — Unity scenes store component type references. Moving MonoBehaviours to a new namespace/asmdef may break scene references if Unity doesn't resolve them. The `m_Script` GUID in scene files is stable across moves as long as the `.cs` file's meta GUID doesn't change — files must be moved via git mv or Unity's own asset database, not raw file copy.

## Existing Codebase / Prior Art

- `Assets/Scripts/SimpleGame.Runtime.asmdef` — current single runtime assembly; will be replaced by two asmdefs
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — current single test assembly; will be split into Core and Game test assemblies
- `Assets/Editor/SimpleGame.Editor.asmdef` — references `SimpleGame.Runtime`; must be updated
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — uses `ScreenId` directly; becomes `ScreenManager<TScreenId>`
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — uses `PopupId` directly; becomes `PopupManager<TPopupId>`
- `Assets/Tests/EditMode/MVPWiringTests.cs` — uses `ISampleView`/`SamplePresenter`; these move into the Core test assembly as test fixtures
- `Assets/Tests/EditMode/DemoWiringTests.cs` — all game-specific; moves to Game test assembly

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R020 (new) — Core assembly is game-agnostic: no game-specific type references in Core sources
- R021 (new) — Game code in dedicated assembly: all SimpleGame-specific code in `SimpleGame.Game.asmdef`
- R022 (new) — Feature cohesion within Game: interface, presenter, and view for each screen in the same folder
- R023 (new) — Test assemblies mirror source structure: `SimpleGame.Tests.Core` + `SimpleGame.Tests.Game`
- R024 (new) — All 49 edit-mode tests pass after restructure
- R025 (new) — ScreenManager/PopupManager are generic (game-agnostic ID type)

## Scope

### In Scope

- Create `SimpleGame.Core.asmdef` covering all game-agnostic framework types and their Unity implementations
- Create `SimpleGame.Game.asmdef` covering all SimpleGame-specific code
- Make `ScreenManager` generic: `ScreenManager<TScreenId> where TScreenId : struct`
- Make `PopupManager` generic: `PopupManager<TPopupId> where TPopupId : struct`
- Move `ScreenId`, `PopupId` to Game assembly
- Move `IMainMenuView`, `ISettingsView`, `IConfirmDialogView` + their presenters to Game, co-located with their View MonoBehaviours
- Move `ISampleView`, `SamplePresenter` to Core test assembly (test fixtures only)
- Move `GameService`, `UIFactory`, `GameBootstrapper` to Game assembly
- Move `UnityPopupContainer` to Game assembly (knows about `ConfirmDialog` specifically)
- Split test assembly into `SimpleGame.Tests.Core` and `SimpleGame.Tests.Game`
- Update `SimpleGame.Editor.asmdef` to reference `SimpleGame.Game`
- Update all namespaces to match new folder structure
- Verify 49/49 tests pass and static guard clean

### Out of Scope / Non-Goals

- Any behavior changes — pure restructure only
- New features, new screens, new tests
- Changing the MVP pattern or presenter lifecycle
- Removing `autoReferenced: true` from the new Core asmdef (noted as future cleanup in M001 forward intelligence but not this milestone's job)

## Technical Constraints

- Unity 6000.3.4f1 — same version
- C# enum generic constraint: `where TScreenId : struct` (enums satisfy `struct` in C#; `where T : System.Enum` requires C# 7.3+ which Unity supports)
- File moves must preserve `.meta` file GUIDs to avoid breaking scene serialization — use `git mv` or Unity Editor asset moves, not raw filesystem copy
- Name-based asmdef references (Decision #2) — no GUID references in new asmdef files
- `autoReferenced` on Core asmdef: keep `true` for now to avoid breaking anything; Game asmdef: `false` (explicit references only)

## Integration Points

- Unity Editor asset database — scene `.meta` GUIDs must be preserved during file moves
- `SimpleGame.Editor.asmdef` — SceneSetup.cs uses Game-layer MonoBehaviours; must reference `SimpleGame.Game`
- Unity batchmode test runner — both test asmdefs must compile and be picked up by `-runTests`

## Open Questions

- `where TScreenId : struct` vs `where TScreenId : System.Enum` — `System.Enum` constraint is cleaner but was added in C# 7.3. Unity 6 uses C# 9+ so both work. Prefer `System.Enum` for clarity; revisit if compile errors appear.
- Folder name for Core's Unity implementations — `Core/Runtime/` to mirror current naming, or `Core/Unity/` to be explicit that these are Unity-specific implementations of Core interfaces. Agent's discretion — `Core/Unity/` is clearer.
