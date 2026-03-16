# S02: SceneController MonoBehaviours + Async Control Flow

**Goal:** Replace the GameBootstrapper navigation stub with SceneController MonoBehaviours. Each scene gets a `SceneController` that owns all presenter orchestration: it creates presenters via UIFactory, wires them, and loops internally via `RunAsync()` until navigation away is decided. The bootscene drives the top-level navigation loop. No `.Forget()` anywhere in production paths.

**Demo:** `MainMenuSceneController.RunAsync()` loops: on `PopupClicked` it shows the ConfirmDialog inline and awaits the result before continuing; on `SettingsClicked` it returns `ScreenId.Settings`. `SettingsSceneController.RunAsync()` awaits back and returns. `GameBootstrapper.Start()` drives the loop: `await sceneController.RunAsync()` → act on result → navigate → await next controller. All 53 existing tests still pass.

## Must-Haves

- `ISceneController` interface with `UniTask<ScreenId> RunAsync(CancellationToken ct)` entry point
- `MainMenuSceneController` MonoBehaviour — `[SerializeField]` IMainMenuView + IConfirmDialogView refs; `RunAsync()` loops internally: popup handled inline (show, await WaitForConfirmation, dismiss, continue), returns `ScreenId.Settings` when settings clicked; disposes presenters on exit
- `SettingsSceneController` MonoBehaviour — `[SerializeField]` ISettingsView ref; `RunAsync()` creates SettingsPresenter, awaits WaitForBack(), disposes, returns `ScreenId.MainMenu`
- `GameBootstrapper` replaced — constructs infrastructure + UIFactory, then drives: `await screenManager.ShowScreenAsync(MainMenu)` → loop `await mainMenuController.RunAsync()` → navigate to result
- No `.Forget()` in any production async path — `grep -rn "\.Forget()" Assets/Scripts/` returns empty
- All 53 edit-mode tests still pass

## Proof Level

- This slice proves: integration
- Real runtime required: no (SceneController logic is pure C# testable without MonoBehaviour lifecycle)
- Human/UAT required: no (S03 covers play-mode boot-from-any-scene UAT)

## Verification

- `mcporter call unityMCP.run_tests mode=EditMode` → `get_test_job` — expect ≥57 passed (53 existing + new SceneController tests), 0 failures
- `grep -rn "\.Forget()" Assets/Scripts/` returns empty
- `grep -rn "\.Forget()" Assets/Scripts/Game/Boot/GameBootstrapper.cs` returns empty

## Integration Closure

- Upstream surfaces consumed: `WaitForAction()`, `WaitForBack()`, `WaitForConfirmation()` from S01; `UIFactory.Create*`; `ScreenManager.ShowScreenAsync()` / `GoBackAsync()`; `PopupManager.ShowPopupAsync()` / `DismissPopupAsync()`
- New wiring introduced: `GameBootstrapper.Start()` drives the loop; SceneControllers are MonoBehaviours wired via SerializeField in the scene
- What remains before milestone is usable end-to-end: S03 (boot-from-any-scene + editor tooling)

## Tasks

- [ ] **T01: Define ISceneController interface** `est:10m`
  - Why: Establishes the contract that BootSceneController uses to drive navigation; makes the loop testable without concrete MonoBehaviour refs
  - Files: `Assets/Scripts/Game/Boot/ISceneController.cs` (new)
  - Do: Create interface in `SimpleGame.Game.Boot` namespace. Single method: `UniTask<ScreenId> RunAsync(CancellationToken ct = default)`. Add `using Cysharp.Threading.Tasks; using System.Threading;`
  - Verify: Compile via next task's dependency
  - Done when: file exists with correct signature

- [ ] **T02: Implement SettingsSceneController** `est:20m`
  - Why: Simplest SceneController — single presenter, single await, returns to MainMenu; validates the pattern before the more complex MainMenu variant
  - Files: `Assets/Scripts/Game/Settings/SettingsSceneController.cs` (new)
  - Do: `public class SettingsSceneController : MonoBehaviour, ISceneController`. `[SerializeField] private SettingsView _settingsView`. `private UIFactory _uiFactory`. `public void Initialize(UIFactory factory) => _uiFactory = factory`. `public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)`: create presenter from factory, call Initialize(), await WaitForBack(), Dispose(), return ScreenId.MainMenu. No `.Forget()`.
  - Verify: Write `SettingsSceneControllerTests` in T04; compile check here
  - Done when: file compiles, implements ISceneController, no .Forget()

- [ ] **T03: Implement MainMenuSceneController** `est:45m`
  - Why: Core piece — handles the popup loop inline; this is the central pattern proof for M003
  - Files: `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` (new)
  - Do: `public class MainMenuSceneController : MonoBehaviour, ISceneController`. SerializeField refs: `[SerializeField] private MainMenuView _mainMenuView; [SerializeField] private ConfirmDialogView _confirmDialogView`. `private UIFactory _uiFactory; private PopupManager<PopupId> _popupManager`. `public void Initialize(UIFactory factory, PopupManager<PopupId> popupManager)` stores both. `public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)`: create MainMenuPresenter (factory.CreateMainMenuPresenter(_mainMenuView)), Initialize(). Loop: `while (true) { var action = await mainMenuPresenter.WaitForAction(ct); if (action == MainMenuAction.Settings) { mainMenuPresenter.Dispose(); return ScreenId.Settings; } if (action == MainMenuAction.Popup) { await ShowConfirmPopup(ct); } }`. `ShowConfirmPopup`: create ConfirmDialogPresenter, Initialize(), `await _popupManager.ShowPopupAsync(PopupId.ConfirmDialog, ct)`, `await confirmPresenter.WaitForConfirmation(ct)`, `await _popupManager.DismissPopupAsync(ct)`, confirmPresenter.Dispose(). No `.Forget()`.
  - Verify: Write tests in T04; compile check here
  - Done when: file compiles, loop is correct, popup handled inline, no .Forget()

- [ ] **T04: Write SceneController tests** `est:30m`
  - Why: Proves the async control flow without needing Unity runtime; all pure C# via mock views
  - Files: `Assets/Tests/EditMode/Game/SceneControllerTests.cs` (new)
  - Do: New `[TestFixture] SceneControllerTests` class. Use existing mock views from DemoWiringTests (or redeclare locally). Tests: (1) `SettingsSceneController_RunAsync_BackClicked_ReturnsMainMenu` — create controller, set _settingsView via field/Init, run, simulate back, assert result == MainMenu. (2) `MainMenuSceneController_RunAsync_SettingsClicked_ReturnsSettings`. (3) `MainMenuSceneController_RunAsync_PopupThenSettings_HandlesInline` — simulate popup click, confirm, then settings click; assert returns Settings. Because SceneControllers are MonoBehaviours, we cannot `new` them — test the logic through a testable pure-C# `SceneControllerLogic` helper OR test via the presenter orchestration directly (preferred: test that the presenter API used by the controller works as expected). Simplest approach: extract the RunAsync logic into a static/pure helper that takes presenter interfaces — no MonoBehaviour in test path. See Do notes.
    - Alternative: test MainMenuSceneController by subclassing with test-accessible fields. In Unity edit-mode, `new GameObject().AddComponent<T>()` works — use that. For SettingsSceneController: `var go = new GameObject(); var ctrl = go.AddComponent<SettingsSceneController>(); ctrl.Initialize(factory);` — set the private _settingsView field via reflection or add an Init overload. Actually cleanest: add `[EditorOnly]` or just `internal` setters for views in editor build. Better: make the view fields `internal` not `private` so tests can set them.
    - Decision: use `[SerializeField]` for Unity wiring but also add `internal` Init methods for testing: `internal void SetViewsForTesting(MainMenuView mmv, ConfirmDialogView cdv)`. This keeps MonoBehaviour as the tested type.
  - Verify: Run via `mcporter call unityMCP.run_tests mode=EditMode` after T05; new tests must pass
  - Done when: ≥3 new tests written and they compile

- [ ] **T05: Replace GameBootstrapper + run final verification** `est:30m`
  - Why: Closes the integration — GameBootstrapper must drive the SceneController loop, removing the navigation stub
  - Files: `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
  - Do: Rewrite GameBootstrapper: add `[SerializeField] private BootSceneController _bootSceneController` — wait, GameBootstrapper IS the boot orchestrator; there's no separate BootSceneController since the Boot scene has its own nav loop. Revised: GameBootstrapper becomes the infrastructure builder; after building, it finds the first scene's SceneController and kicks off RunAsync. Specifically: `async UniTaskVoid Start()` — build infrastructure (same as now: GameService, inputBlocker, transitionPlayer, popupContainer, sceneLoader, screenManager, popupManager, uiFactory). Then `[SerializeField] private MainMenuSceneController _mainMenuSceneController` — but MainMenuSceneController is in the MainMenu scene, not Boot. GameBootstrapper is in Boot. So boot flow: GameBootstrapper builds infra, calls `await screenManager.ShowScreenAsync(ScreenId.MainMenu)`, then needs to find and run the MainMenuSceneController from the loaded scene. Use `FindFirstObjectByType<MainMenuSceneController>()` after load. Call `mainMenuController.Initialize(uiFactory, popupManager)`. Loop: `while (true) { var next = await mainMenuController.RunAsync(); if (next == ScreenId.Settings) { await screenManager.ShowScreenAsync(ScreenId.Settings); var settingsCtrl = FindFirstObjectByType<SettingsSceneController>(); settingsCtrl.Initialize(uiFactory); var back = await settingsCtrl.RunAsync(); if (back == ScreenId.MainMenu) { await screenManager.GoBackAsync(); mainMenuController = FindFirstObjectByType<MainMenuSceneController>(); mainMenuController.Initialize(uiFactory, popupManager); } } }`. No `.Forget()`.
  - Verify: `grep -rn "\.Forget()" Assets/Scripts/` returns empty. Run `mcporter call unityMCP.run_tests mode=EditMode` → all pass.
  - Done when: 0 `.Forget()` in Assets/Scripts/, all tests pass

## Files Likely Touched

- `Assets/Scripts/Game/Boot/ISceneController.cs` (new)
- `Assets/Scripts/Game/Settings/SettingsSceneController.cs` (new)
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` (new)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` (new)
