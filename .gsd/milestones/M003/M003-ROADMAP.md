# M003: SceneController Architecture — Async Control Flow

**Vision:** Replace the callback-based presenter wiring and fire-and-forget async model with a SceneController-per-scene pattern. Each scene has a MonoBehaviour SceneController that holds serialized references to all views in that scene and exposes a `RunAsync()` entry point. `RunAsync()` does not return until the scene is truly done — it loops internally handling popups and in-scene actions, and only completes when navigation away is decided. The return value carries whatever context the next SceneController needs. Presenters expose domain-specific awaitable methods (`WaitForConfirmation()`, `WaitForNavigation()`, etc.) instead of receiving callbacks — the SceneController composes these into a readable linear async control flow. Eliminates `.Forget()` from all production paths. Adds boot-from-any-scene support for the editor. No new game screens or features — structural change only.

## Success Criteria

- Every scene has a `SceneController` MonoBehaviour with a `RunAsync()` entry point; the method loops internally handling all in-scene actions (including popups) and only returns when navigation away is decided — it never yields control mid-scene for a button press
- `RunAsync()` return value carries the context needed by the next SceneController (e.g. which screen to show next)
- No `.Forget()` in production async paths (`GameBootstrapper`, `SceneController`, presenters) — fully awaited throughout
- Each presenter exposes domain-specific async result methods (`WaitForConfirmation()` → `UniTask<bool>`, etc.) instead of receiving callbacks; `Initialize()` is still called externally before awaiting
- `UIFactory` is passed into `SceneController` at construction; `GameBootstrapper` hands it off
- Starting play mode from `MainMenu.unity` or `Settings.unity` directly (without Boot open) additively loads the Boot scene and runs the correct SceneController
- All 49 existing edit-mode tests still pass; new tests cover SceneController async flow and presenter result tasks
- Static guard clean; no game-specific types in `SimpleGame.Core`

## Key Risks / Unknowns

- **UniTask async entry point lifetime** — `RunAsync()` must be launched from a MonoBehaviour lifecycle method (`Start`) without `.Forget()`; the right pattern is `async UniTaskVoid Start()` calling `await RunAsync()` — needs validation that exceptions surface correctly
- **Presenter result task design** — a presenter that exposes `WaitForConfirmation()` needs an internal `UniTaskCompletionSource<bool>` (or similar) that completes when the user acts; this pattern must be established cleanly and be mockable for tests
- **Boot injection ordering** — additively loading Boot from inside a scene's `Start()` before infrastructure exists could trigger null refs in `UnitySceneLoader`/`UnityInputBlocker`; the order of operations needs care

## Proof Strategy

- UniTask async entry point → retire in S01 by running `BootSceneController.RunAsync()` with no `.Forget()` and no unhandled exceptions
- Presenter result task design → retire in S01 by writing `ConfirmDialogPresenter.WaitForConfirmation()` returning `UniTask<bool>`, backed by `UniTaskCompletionSource`, with a passing edit-mode test
- Boot injection ordering → retire in S02 by starting play mode from `MainMenu.unity` and confirming the Boot scene loads additively before `MainMenuSceneController.RunAsync()` proceeds

## Verification Classes

- Contract verification: all 49 existing tests pass; new tests for presenter result tasks and SceneController async flow; static guard clean; no game types in Core
- Integration verification: play mode entered from each of Boot.unity, MainMenu.unity, Settings.unity — no null refs, no missing infrastructure
- Operational verification: no `.Forget()` in production paths — `grep -rn "\.Forget()" Assets/Scripts/` returns empty
- UAT / human verification: enter play mode from MainMenu scene directly; confirm boot loads, main menu shows, navigation and popup work

## Testing Approach

Use Unity MCP server for running edit-mode tests when available — check `mcp_servers` at the start of each slice's test step and use it if present. Fall back to batchmode CLI (`-runTests -testPlatform EditMode`) if MCP is unavailable or unresponsive. Prefer MCP: it avoids the batchmode Unity license/project-open overhead and gives faster feedback.

## Milestone Definition of Done

This milestone is complete only when all are true:

- `BootSceneController`, `MainMenuSceneController`, `SettingsSceneController` exist and their `RunAsync()` methods contain all control flow with no callbacks passed to presenters
- `ConfirmDialogPresenter.WaitForConfirmation()` returns `UniTask<bool>`; the pattern is documented and mockable
- `GameBootstrapper` hands `UIFactory` to the relevant `SceneController`; contains no navigation logic itself
- `grep -rn "\.Forget()" Assets/Scripts/` returns empty
- Starting play from `MainMenu.unity` works: Boot loads additively, `MainMenuSceneController.RunAsync()` runs, navigation works
- All 49 edit-mode tests pass (new tests included in total)
- Static guard clean

## Requirement Coverage

- Covers: R001 (MVP pattern — structural improvement), R004 (UIFactory still central), R008 (boot flow), R010 (screen navigation)
- Partially covers: R014 (UniTask async — pattern improved), R015 (edit-mode tests extended)
- Leaves for later: R018 (view preview tool), R019 (play-mode tests)
- Orphan risks: none

## Slices

- [x] **S01: Presenter async result API + Core async infrastructure** `risk:high` `depends:[]`
  > After this: `ConfirmDialogPresenter` exposes `WaitForConfirmation()` returning `UniTask<bool>` backed by `UniTaskCompletionSource<bool>`; the pattern is established, tested in edit-mode, and documented; `MainMenuPresenter` and `SettingsPresenter` expose analogous result tasks; no callbacks remain on any presenter constructor; all 49 existing tests still pass with new presenter API.

- [x] **S02: SceneController MonoBehaviours + async control flow** `risk:medium` `depends:[S01]`
  > After this: `BootSceneController`, `MainMenuSceneController`, and `SettingsSceneController` exist as MonoBehaviours; `MainMenuSceneController.RunAsync()` loops internally — handles the ConfirmDialog popup inline and only returns when navigation away is decided; `GameBootstrapper` drives the top-level loop via `BootSceneController`; no `.Forget()` in any production path; all tests still pass.

- [x] **S03: Boot-from-any-scene + editor tooling** `risk:low` `depends:[S02]`
  > After this: entering play mode from `MainMenu.unity` or `Settings.unity` additively loads Boot scene and runs the correct SceneController; a `[RuntimeInitializeOnLoadMethod]` or similar editor-only mechanism handles the injection; `SceneSetup.cs` wires SceneController refs; all tests pass; UAT: start from MainMenu directly, confirm it works.

## Boundary Map

### S01 → S02

Produces:
- `ConfirmDialogPresenter.WaitForConfirmation()` → `UniTask<bool>` backed by `UniTaskCompletionSource<bool>`; completes when OnConfirm (true) or OnCancel (false) fires; resets on re-show
- `MainMenuPresenter` exposes per-action result tasks (e.g. `WaitForAction()` → `UniTask<MainMenuAction>` where `MainMenuAction` is a discriminated enum); SceneController loops on this to handle popups inline before deciding to navigate
- `SettingsPresenter.WaitForBack()` → `UniTask`; completes when back is pressed
- All presenter constructors take only `TView` — no callbacks
- Edit-mode tests covering each result task: mock fires event → task resolves → assert result value
- **Testing approach:** attempt Unity MCP server first for running edit-mode tests; fall back to batchmode CLI if MCP unavailable

Consumes:
- nothing (restructure of existing presenter code)

### S02 → S03

Produces:
- `ISceneController` interface — `UniTask RunAsync(CancellationToken ct)` entry point
- `BootSceneController` MonoBehaviour — constructs UIFactory, finds infrastructure, drives the top-level navigation loop (load scene → await SceneController.RunAsync() → act on result → repeat)
- `MainMenuSceneController` MonoBehaviour — `[SerializeField] MainMenuView`, `[SerializeField] ConfirmDialogView`; `RunAsync()` loops on presenter actions, handles popup inline (show ConfirmDialog, await WaitForConfirmation, dismiss, continue loop), returns only when navigation away is decided
- `SettingsSceneController` MonoBehaviour — `[SerializeField] SettingsView`; `RunAsync()` awaits WaitForBack(), returns
- `GameBootstrapper` simplified — constructs infrastructure + managers + UIFactory, then calls `BootSceneController.RunAsync()` via `async UniTaskVoid Start()`; contains no navigation logic
- No `.Forget()` in any production path — `grep -rn "\.Forget()" Assets/Scripts/` returns empty
- **Testing approach:** attempt Unity MCP server first for running edit-mode tests; fall back to batchmode CLI if MCP unavailable

Consumes from S01:
- Presenter result task API (`WaitForAction()`, `WaitForBack()`, `WaitForConfirmation()`)
- All presenter constructors without callbacks

### S03 → (end)

Produces:
- `BootInjector` (editor-only MonoBehaviour or `RuntimeInitializeOnLoadMethod`) — detects missing boot infrastructure at play-mode start, additively loads `Boot.unity` before the active scene's `SceneController.RunAsync()` proceeds
- `SceneSetup.cs` extended to wire `SceneController` serialized view refs in each scene
- `EditorBuildSettings` unchanged — Boot, MainMenu, Settings already registered

Consumes from S02:
- `ISceneController` interface
- `BootSceneController`, `MainMenuSceneController`, `SettingsSceneController` MonoBehaviours
