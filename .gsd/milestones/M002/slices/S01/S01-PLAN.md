# S01: Core Assembly Restructure + Generic Managers

**Goal:** Restructure all game-agnostic types into `Assets/Scripts/Core/` under a new `SimpleGame.Core.asmdef`, make `ScreenManager` and `PopupManager` generic, and confirm the Core assembly compiles cleanly with no game-specific type references.
**Demo:** `SimpleGame.Core.asmdef` exists and compiles; `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|GameService\|UIFactory\|ScreenId\|PopupId" Assets/Scripts/Core/` returns empty; `ScreenManager<TScreenId>` and `PopupManager<TPopupId>` exist with `where T : System.Enum` constraint.

## Must-Haves

- `Assets/Scripts/Core/SimpleGame.Core.asmdef` exists with name `SimpleGame.Core`, references `UniTask` and `UnityEngine.UI`, `autoReferenced: true`
- `ScreenManager<TScreenId> where TScreenId : System.Enum` compiles in `SimpleGame.Core.ScreenManagement` namespace
- `PopupManager<TPopupId> where TPopupId : System.Enum` compiles in `SimpleGame.Core.PopupManagement` namespace
- `IPopupContainer` interface updated to be generic: `IPopupContainer<TPopupId>`
- Core folder contains no references to `ScreenId`, `PopupId`, `IMainMenuView`, `ISettingsView`, `IConfirmDialogView`, `GameService`, `UIFactory`, `SamplePresenter`, `ISampleView`
- `ISampleView` and `SamplePresenter` deleted from `Core/MVP/` (they move to the test assembly in S03)
- Unity Core implementations (`UnitySceneLoader`, `UnityInputBlocker`, `UnityTransitionPlayer`) moved to `Core/Unity/` subfolders, namespace updated to `SimpleGame.Core.Unity.*`
- Old `Assets/Scripts/SimpleGame.Runtime.asmdef` deleted (replaced by Core + Game asmdefs)
- Batchmode compile exits 0 (project may have errors in Game/Runtime layer — that's S02's job — but Core itself must be internally consistent)

## Proof Level

- This slice proves: contract (compile-time correctness of Core assembly)
- Real runtime required: no
- Human/UAT required: no

## Verification

- `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|GameService\|UIFactory\|ScreenId\|PopupId\|ISampleView\|SamplePresenter" Assets/Scripts/Core/` returns empty
- `grep -rn "SimpleGame.Core.asmdef" Assets/Scripts/Core/` — file exists
- `grep "ScreenManager<" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — confirms generic signature
- `grep "PopupManager<" Assets/Scripts/Core/PopupManagement/PopupManager.cs` — confirms generic signature
- Static guard: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns empty

Note: Full batchmode compile is deferred to S02 completion (Game assembly doesn't exist yet, so the project has known errors in Assets/Scripts/Runtime/ during S01). The Core compile is verified by the grep guards and file inspection.

## Tasks

- [ ] **T01: Create Core asmdef and restructure Core/Unity folders** `est:30m`
  - Why: Establishes the new assembly boundary and moves Unity implementations to `Core/Unity/`
  - Files: `Assets/Scripts/Core/SimpleGame.Core.asmdef`, `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs`, `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs`, `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs`
  - Do: (1) Create `Assets/Scripts/Core/SimpleGame.Core.asmdef` with name `SimpleGame.Core`, references `["UniTask", "UnityEngine.UI"]`, `autoReferenced: true`. (2) `git mv` `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` → `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs`. (3) `git mv` `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` → `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs`. (4) `git mv` `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` → `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs`. (5) Update namespace in each moved file: `SimpleGame.Runtime.ScreenManagement` → `SimpleGame.Core.Unity.ScreenManagement`, etc. (6) Also `git mv` the corresponding `.meta` files if git doesn't handle them automatically (check with `git status`).
  - Verify: `find Assets/Scripts/Core/Unity -name "*.cs" | sort` shows all three files; `grep "namespace SimpleGame.Core.Unity" Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs` passes
  - Done when: Three Unity implementation files exist under `Core/Unity/` with correct namespaces; `SimpleGame.Core.asmdef` exists

- [ ] **T02: Make ScreenManager and ISceneLoader generic** `est:30m`
  - Why: Removes the hard dependency on `ScreenId` enum from Core — the only game-specific type `ScreenManager` references
  - Files: `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`, `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs`
  - Do: (1) `ScreenId.cs` — do NOT move or delete yet (Game assembly doesn't exist); it stays in `Core/ScreenManagement/` temporarily until S02. (2) Update `ScreenManager.cs`: change class declaration to `public class ScreenManager<TScreenId> where TScreenId : System.Enum`. Replace all `ScreenId` type references with `TScreenId`. Replace `Stack<ScreenId>` with `Stack<TScreenId>`. Replace `ScreenId?` with `TScreenId?`. The `screenId.ToString()` call remains unchanged — works on any enum. (3) `ISceneLoader.cs` needs no changes — it takes `string sceneName`, not `ScreenId`. (4) Update namespace in `ScreenManager.cs` if needed — it should stay `SimpleGame.Core.ScreenManagement`.
  - Verify: `grep "class ScreenManager<TScreenId>" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` returns a match; `grep "ScreenId[^<]" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` returns empty (no bare ScreenId references)
  - Done when: ScreenManager is generic; no bare `ScreenId` type references remain in the file

- [ ] **T03: Make PopupManager and IPopupContainer generic** `est:30m`
  - Why: Removes the hard dependency on `PopupId` enum from Core
  - Files: `Assets/Scripts/Core/PopupManagement/PopupManager.cs`, `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs`
  - Do: (1) `PopupId.cs` — do NOT move or delete yet; stays in `Core/PopupManagement/` temporarily until S02. (2) Update `IPopupContainer.cs`: change to `public interface IPopupContainer<TPopupId>`. Update `ShowPopupAsync` and `HidePopupAsync` signatures to use `TPopupId`. (3) Update `PopupManager.cs`: change class declaration to `public class PopupManager<TPopupId> where TPopupId : System.Enum`. Replace all `PopupId` type references with `TPopupId`. Replace `Stack<PopupId>` with `Stack<TPopupId>`. Replace `PopupId?` with `TPopupId?`. Update internal reference to `IPopupContainer` → `IPopupContainer<TPopupId>` in the constructor and field.
  - Verify: `grep "class PopupManager<TPopupId>" Assets/Scripts/Core/PopupManagement/PopupManager.cs` returns a match; `grep "interface IPopupContainer<" Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` returns a match; `grep "PopupId[^<]" Assets/Scripts/Core/PopupManagement/PopupManager.cs` returns empty
  - Done when: Both PopupManager and IPopupContainer are generic; no bare `PopupId` type references remain

- [ ] **T04: Remove ISampleView and SamplePresenter from Core runtime** `est:15m`
  - Why: These are test fixtures, not framework code; they move to the Core test assembly in S03
  - Files: `Assets/Scripts/Core/MVP/ISampleView.cs`, `Assets/Scripts/Core/MVP/SamplePresenter.cs`, `Assets/Scripts/Core/Services/GameService.cs`
  - Do: (1) Delete `Assets/Scripts/Core/MVP/ISampleView.cs` and its `.meta` file using `git rm`. (2) Delete `Assets/Scripts/Core/MVP/SamplePresenter.cs` and its `.meta` file using `git rm`. (3) `SamplePresenter` depends on `GameService` — `GameService` also needs to leave Core. Delete `Assets/Scripts/Core/Services/GameService.cs` and its `.meta` using `git rm`. The `Services/` folder can be left empty or removed. (4) Note: `UIFactory.cs` and game-specific presenter files also import `GameService` — those files are being deleted/moved in S02. They will have compile errors after this task, which is expected and intentional; S02 resolves them.
  - Verify: `find Assets/Scripts/Core/MVP -name "ISampleView.cs" -o -name "SamplePresenter.cs"` returns empty; `find Assets/Scripts/Core/Services -name "GameService.cs"` returns empty
  - Done when: ISampleView, SamplePresenter, GameService are gone from Core runtime sources

- [ ] **T05: Remove game-specific MVP files from Core** `est:20m`
  - Why: IMainMenuView, ISettingsView, IConfirmDialogView, their presenters, and UIFactory are game-specific and must leave Core
  - Files: `Assets/Scripts/Core/MVP/IMainMenuView.cs`, `Assets/Scripts/Core/MVP/ISettingsView.cs`, `Assets/Scripts/Core/MVP/IConfirmDialogView.cs`, `Assets/Scripts/Core/MVP/MainMenuPresenter.cs`, `Assets/Scripts/Core/MVP/SettingsPresenter.cs`, `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs`, `Assets/Scripts/Core/MVP/UIFactory.cs`
  - Do: `git rm` each of the seven files and their `.meta` counterparts. These files move to Game in S02 — for now they're just removed from Core. The `IPopupView.cs` stays in Core (it's a framework marker interface, not game-specific). `IView.cs` and `Presenter.cs` stay in Core.
  - Verify: `find Assets/Scripts/Core/MVP -name "*.cs" | sort` shows only: `IPopupView.cs`, `IView.cs`, `Presenter.cs`; `grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|UIFactory\|MainMenuPresenter\|SettingsPresenter\|ConfirmDialogPresenter" Assets/Scripts/Core/` returns empty
  - Done when: Core/MVP contains only the three framework base types

- [ ] **T06: Delete old Runtime folder and SimpleGame.Runtime.asmdef** `est:15m`
  - Why: The old Runtime folder (`Assets/Scripts/Runtime/`) is now empty or contains files moved to Core/Unity or Game; the old asmdef is replaced
  - Files: `Assets/Scripts/SimpleGame.Runtime.asmdef`, `Assets/Scripts/Runtime/` (everything remaining)
  - Do: (1) Check what's still in `Assets/Scripts/Runtime/` with `find Assets/Scripts/Runtime -name "*.cs"`. The Boot and MVP subfolders (`GameBootstrapper.cs`, `MainMenuView.cs`, `SettingsView.cs`, `ConfirmDialogView.cs`, `UnityPopupContainer.cs`) are game-specific and move to Game in S02 — for now `git rm` them from Runtime. (2) `git rm Assets/Scripts/SimpleGame.Runtime.asmdef` and its `.meta`. (3) `git rm Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` and `.meta`. (4) `git rm Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs`, `MainMenuView.cs`, `SettingsView.cs` and their `.meta`. (5) `git rm Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` and `.meta`. (6) Clean up any empty directories.
  - Verify: `find Assets/Scripts/Runtime -name "*.cs"` returns empty; `find Assets/Scripts -name "SimpleGame.Runtime.asmdef"` returns empty
  - Done when: Old Runtime folder and asmdef are gone; Core is the sole runtime assembly under Assets/Scripts/

## Files Likely Touched

- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — created
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — deleted
- `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs` — moved + namespace updated
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — moved + namespace updated
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — moved + namespace updated
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — made generic
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — made generic
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — made generic
- `Assets/Scripts/Core/MVP/ISampleView.cs` — deleted
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — deleted
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — deleted
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — deleted
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — deleted
- `Assets/Scripts/Core/MVP/UIFactory.cs` — deleted
- `Assets/Scripts/Core/Services/GameService.cs` — deleted
- `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` — deleted (recreated in Game in S02)
- `Assets/Scripts/Runtime/MVP/*.cs` — deleted (recreated in Game in S02)
- `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` — deleted (recreated in Game in S02)
