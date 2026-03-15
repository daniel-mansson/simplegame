# S05: Boot Flow & Demo Screens

**Goal:** Boot scene initializes all services and the UI factory, transitions to MainMenu, user navigates to Settings and back, opens a popup — full dependency chain flows correctly. This is the final integration proof for M001.

**Demo:** Enter play mode from Boot scene → auto-transition to MainMenu → click "Settings" button → navigate to Settings with fade → click "Back" → return to MainMenu with fade → click "Open Popup" → ConfirmDialog appears, blocks input → click "OK" → popup dismisses. Full dependency chain from boot to presenter to view, zero static state.

## Must-Haves

- View interfaces (IMainMenuView, ISettingsView, IConfirmDialogView) in Core with `event Action` convention, no Unity types
- Presenters (MainMenuPresenter, SettingsPresenter, ConfirmDialogPresenter) using constructor injection + two-phase lifecycle
- UIFactory expanded with Create methods for all 3 demo presenters, receiving navigation/popup callbacks (not full manager references)
- View MonoBehaviours (MainMenuView, SettingsView, ConfirmDialogView) implementing interfaces, exposing only events + update methods
- UnityPopupContainer implementing IPopupContainer via show/hide (SetActive) of pre-instantiated popup GameObjects
- GameBootstrapper MonoBehaviour in Boot scene that constructs services → managers → factory → first navigation
- Boot scene with persistent Canvas hierarchy (EventSystem, InputBlocker overlay, Transition overlay, Popup layer)
- MainMenu scene with Canvas + title text + "Settings" button + "Open Popup" button
- Settings scene with Canvas + title text + "Back" button
- Boot scene at EditorBuildSettings index 0
- Edit-mode tests for new UIFactory Create methods and demo presenter construction/event-wiring
- No static state fields (grep guard clean)
- No `using UnityEngine` in Core/ files

## Proof Level

- This slice proves: final-assembly
- Real runtime required: yes (play-mode walkthrough in Unity editor)
- Human/UAT required: yes (manual walkthrough of demo screens with transitions and popups)

## Verification

- `Assets/Tests/EditMode/DemoWiringTests.cs` — edit-mode tests for UIFactory.CreateMainMenuPresenter, CreateSettingsPresenter, CreateConfirmDialogPresenter; event wiring; dispose unsubscribe; mock views have no backward references
- Batchmode test run: all tests pass (32 existing + new demo tests), 0 failures
- Static guard: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns empty
- No UnityEngine in Core: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` returns empty
- Batchmode compile: exit 0, zero `error CS` hits
- Boot scene at index 0 in EditorBuildSettings
- Play-mode walkthrough: Boot → MainMenu → Settings → MainMenu → popup open → popup dismiss (human-verified)

## Observability / Diagnostics

- Runtime signals: `Debug.Log` in GameBootstrapper for boot sequence steps (service creation, manager creation, factory creation, first navigation); presenter Initialize/Dispose can be traced via the event subscription pattern
- Inspection surfaces: ScreenManager.CurrentScreen tracks active screen; PopupManager.HasActivePopup / PopupCount for popup state; UnityInputBlocker.IsBlocked for input state
- Failure visibility: NullReferenceException on first fade/block = missing CanvasGroup wire-up in scene; FindObjectOfType returning null after scene load = view MonoBehaviour missing from scene; boot sequence Debug.Log trail shows which step failed
- Redaction constraints: none

## Integration Closure

- Upstream surfaces consumed: `ScreenManager` (S02), `PopupManager` + `IInputBlocker` (S03), `ITransitionPlayer` + `UnityTransitionPlayer` (S04), `UIFactory` + `Presenter<TView>` + `IView` + `GameService` (S01), `UnitySceneLoader` (S02), `UnityInputBlocker` (S03)
- New wiring introduced in this slice: `GameBootstrapper` MonoBehaviour composes all services/managers/factory at runtime; concrete view MonoBehaviours + `UnityPopupContainer` connect the pure C# contracts to Unity UI
- What remains before the milestone is truly usable end-to-end: nothing — this is the final slice

## Tasks

- [x] **T01: Core view interfaces, presenters, UIFactory expansion, and edit-mode tests** `est:45m`
  - Why: All pure C# types that define the demo screen contracts and prove they wire correctly. This is the foundation that T02's runtime MonoBehaviours will implement. Tests verify the wiring independently of Unity runtime.
  - Files: `Assets/Scripts/Core/MVP/IMainMenuView.cs`, `Assets/Scripts/Core/MVP/ISettingsView.cs`, `Assets/Scripts/Core/MVP/IConfirmDialogView.cs`, `Assets/Scripts/Core/MVP/MainMenuPresenter.cs`, `Assets/Scripts/Core/MVP/SettingsPresenter.cs`, `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs`, `Assets/Scripts/Core/MVP/UIFactory.cs`, `Assets/Tests/EditMode/DemoWiringTests.cs`
  - Do: Create 3 view interfaces following ISampleView pattern (event Action, no Unity types). Create 3 presenters following SamplePresenter pattern (constructor injects view + callbacks/services, Initialize subscribes events, Dispose unsubscribes). Expand UIFactory constructor to accept navigation callback (`Action<ScreenId>`) and popup callbacks (`Action<PopupId>`, `Func<UniTask>`); add 3 Create methods. Write edit-mode tests with mock views proving construction, initialization, event wiring, disposal, and no backward references.
  - Verify: Batchmode test run — all tests pass, 0 failures. `grep -r "using UnityEngine" Assets/Scripts/Core/` returns empty. Static guard returns empty.
  - Done when: DemoWiringTests.cs has ≥9 passing tests covering all 3 presenters' construction, event wiring, and disposal; UIFactory creates all 3 correctly; no static state; no UnityEngine in Core.

- [x] **T02: Runtime views, popup container, GameBootstrapper, scene setup, and batchmode verification** `est:60m`
  - Why: Connects the pure C# contracts to Unity runtime. Creates all MonoBehaviours that implement the view interfaces, the popup container, and the boot scene initializer. Extends SceneSetup.cs to build full scene hierarchies with UI content. This is the final assembly that proves the full dependency chain works.
  - Files: `Assets/Scripts/Runtime/MVP/MainMenuView.cs`, `Assets/Scripts/Runtime/MVP/SettingsView.cs`, `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs`, `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs`, `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs`, `Assets/Editor/SceneSetup.cs`
  - Do: Create 3 view MonoBehaviours implementing their interfaces (wire Button.onClick → event Action, serialize Text for updates). Create UnityPopupContainer MonoBehaviour implementing IPopupContainer (Dictionary<PopupId, GameObject>, show/hide via SetActive). Create GameBootstrapper MonoBehaviour that in Start(): constructs GameService, UnitySceneLoader, gets UnityInputBlocker + UnityTransitionPlayer from scene, constructs ScreenManager + PopupManager, constructs UIFactory with callbacks, calls ShowScreenAsync(MainMenu), then after await uses FindObjectOfType to locate view and create+initialize presenter. Extend SceneSetup.cs to create Boot scene with full Canvas hierarchy (EventSystem, InputBlocker canvas sort=100, Transition canvas sort=200, Popup canvas sort=300, GameBootstrapper), MainMenu scene with Canvas + buttons + text, Settings scene with Canvas + back button + text. Run via batchmode -executeMethod. Verify compile clean.
  - Verify: Batchmode compile exit 0, zero `error CS`. Batchmode -executeMethod creates all 3 scenes. EditorBuildSettings has Boot at index 0. All existing tests still pass. Static guard clean.
  - Done when: All 3 scenes exist with UI content. Boot scene has GameBootstrapper + InputBlocker + TransitionPlayer + popup container + ConfirmDialog view. Compile clean. All tests pass. Ready for play-mode walkthrough.

## Files Likely Touched

- `Assets/Scripts/Core/MVP/IMainMenuView.cs`
- `Assets/Scripts/Core/MVP/ISettingsView.cs`
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs`
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs`
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs`
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs`
- `Assets/Scripts/Core/MVP/UIFactory.cs`
- `Assets/Scripts/Runtime/MVP/MainMenuView.cs`
- `Assets/Scripts/Runtime/MVP/SettingsView.cs`
- `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs`
- `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs`
- `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Tests/EditMode/DemoWiringTests.cs`
