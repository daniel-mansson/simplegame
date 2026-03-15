---
estimated_steps: 7
estimated_files: 7
---

# T02: Runtime views, popup container, GameBootstrapper, scene setup, and batchmode verification

**Slice:** S05 — Boot Flow & Demo Screens
**Milestone:** M001

## Description

Create all Unity runtime MonoBehaviours that implement the pure C# view interfaces from T01, a concrete `IPopupContainer`, and the `GameBootstrapper` that composes everything at play-mode startup. Extend `SceneSetup.cs` to programmatically build three scenes with full UI content via batchmode. This is the final assembly task that connects all S01–S04 infrastructure into a runnable demo.

## Steps

1. **Create `MainMenuView` MonoBehaviour** in `Assets/Scripts/Runtime/MVP/MainMenuView.cs` — implements `IMainMenuView`. Serializes `Button _settingsButton`, `Button _popupButton`, `Text _titleText`. In `Awake()`: wires `_settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke())` and same for popup button. `UpdateTitle(string text)` sets `_titleText.text`. Exposes `event Action OnSettingsClicked`, `event Action OnPopupClicked`. No references to presenters, services, or managers.

2. **Create `SettingsView` and `ConfirmDialogView` MonoBehaviours** — Same pattern. `SettingsView`: `Button _backButton`, `Text _titleText`, `event Action OnBackClicked`. `ConfirmDialogView`: `Button _confirmButton`, `Button _cancelButton`, `Text _messageText`, `event Action OnConfirmClicked`, `event Action OnCancelClicked`, `void UpdateMessage(string)`.

3. **Create `UnityPopupContainer` MonoBehaviour** in `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` — implements `IPopupContainer`. Holds `[SerializeField] GameObject _confirmDialogPopup` (the popup root to show/hide). `ShowPopupAsync`: `_confirmDialogPopup.SetActive(true)`, return `UniTask.CompletedTask`. `HidePopupAsync`: `_confirmDialogPopup.SetActive(false)`, return `UniTask.CompletedTask`. Uses a simple if/switch on PopupId to select the correct GameObject (extensible via dictionary if more popups added later).

4. **Create `GameBootstrapper` MonoBehaviour** in `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` — In `async void Start()` (or `async UniTaskVoid Start()`): (a) Create `GameService`. (b) Get `UnityInputBlocker` from scene via `FindObjectOfType`. (c) Get `UnityTransitionPlayer` from scene via `FindObjectOfType`. (d) Get `UnityPopupContainer` from scene via `FindObjectOfType`. (e) Create `UnitySceneLoader`. (f) Create `ScreenManager(sceneLoader, transitionPlayer, inputBlocker)`. (g) Create `PopupManager(popupContainer, inputBlocker)`. (h) Create `UIFactory` with GameService + navigation/popup callbacks that delegate to ScreenManager/PopupManager methods. (i) `await screenManager.ShowScreenAsync(ScreenId.MainMenu)`. (j) After await, `FindObjectOfType<MainMenuView>()` to locate view, create presenter via factory, initialize it. (k) Store active presenter reference for disposal on next navigation. Debug.Log at each step for diagnostics. Wire a post-navigation pattern: after each ScreenManager call, find the new view and create/initialize its presenter while disposing the old one.

5. **Extend `SceneSetup.cs`** to create Boot scene with full Canvas hierarchy: (a) Boot scene with `GameBootstrapper` script on root GO; (b) EventSystem + StandaloneInputModule; (c) InputBlocker Canvas (sort order 100) with CanvasGroup + `UnityInputBlocker` component, full-screen stretch; (d) Transition Canvas (sort order 200) with CanvasGroup + `UnityTransitionPlayer` component, black Image, full-screen stretch, starts inactive; (e) Popup Canvas (sort order 300) with `UnityPopupContainer` component; (f) ConfirmDialog popup child under popup canvas — starts inactive, has Canvas + CanvasGroup, contains Text ("Are you sure?") + Confirm button + Cancel button, with `ConfirmDialogView` component; (g) Wire all serialized field references via `GetComponent`/assignment in code. Also populate MainMenu scene: Canvas + "Main Menu" Text + "Settings" Button + "Open Popup" Button + `MainMenuView` component with wired references. Populate Settings scene: Canvas + "Settings" Text + "Back" Button + `SettingsView` component with wired references. All scenes use `UnityEngine.UI.Text` (legacy uGUI). Register scenes in EditorBuildSettings: Boot at index 0, MainMenu at 1, Settings at 2.

6. **Run batchmode scene creation** via `-executeMethod SceneSetup.CreateAndRegisterScenes`. Verify scenes exist, Boot is at index 0, compile clean.

7. **Run full test suite** via batchmode `-runTests`. Verify all tests pass (32 existing + new from T01), 0 failures. Run static guard and Core/UnityEngine grep checks.

## Must-Haves

- [ ] MainMenuView, SettingsView, ConfirmDialogView MonoBehaviours implement their interfaces correctly
- [ ] View MonoBehaviours have zero references to presenters, services, or managers
- [ ] UnityPopupContainer implements IPopupContainer via SetActive show/hide
- [ ] GameBootstrapper constructs full dependency chain: services → managers → factory → first navigation → presenter
- [ ] GameBootstrapper disposes presenters on screen transitions
- [ ] Boot scene has EventSystem, InputBlocker (sort 100), Transition overlay (sort 200), Popup layer (sort 300)
- [ ] MainMenu and Settings scenes have Canvas with buttons, text, and view components with wired references
- [ ] Boot scene is EditorBuildSettings index 0
- [ ] All existing tests still pass
- [ ] Static guard clean
- [ ] Compile clean in batchmode

## Verification

- Batchmode compile: exit 0, zero `error CS`
- Batchmode `-executeMethod SceneSetup.CreateAndRegisterScenes` completes successfully
- Boot scene exists at `Assets/Scenes/Boot.unity` with GameBootstrapper, InputBlocker, TransitionPlayer, PopupContainer components
- MainMenu scene has MainMenuView component with wired buttons and text
- Settings scene has SettingsView component with wired button and text
- EditorBuildSettings: Boot at index 0
- Batchmode test run: all pass, 0 failures
- Static guard: `grep -r "static " --include="*.cs" Assets/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns empty
- No UnityEngine in Core: `grep -r "using UnityEngine" --include="*.cs" Assets/Scripts/Core/` returns empty

## Observability Impact

- Signals added: Debug.Log messages in GameBootstrapper marking each boot phase (service creation, manager creation, factory creation, first navigation, view discovery, presenter initialization)
- How a future agent inspects this: Console log in play mode shows boot sequence; NullReferenceException at specific phase pinpoints which wire-up failed; ScreenManager.CurrentScreen and PopupManager.HasActivePopup queryable at runtime
- Failure state exposed: Missing CanvasGroup wire-up → NullRef on first Block/Fade; Missing view component → FindObjectOfType returns null → NullRef on presenter creation; Wrong scene order → Boot isn't loaded first

## Inputs

- `Assets/Scripts/Core/MVP/IMainMenuView.cs` — T01 output: view interface that MainMenuView implements
- `Assets/Scripts/Core/MVP/ISettingsView.cs` — T01 output: view interface that SettingsView implements
- `Assets/Scripts/Core/MVP/IConfirmDialogView.cs` — T01 output: view interface that ConfirmDialogView implements
- `Assets/Scripts/Core/MVP/UIFactory.cs` — T01 output: expanded factory with Create methods for all 3 presenters
- `Assets/Scripts/Core/MVP/MainMenuPresenter.cs` — T01 output: presenter that GameBootstrapper creates after MainMenu scene loads
- `Assets/Scripts/Core/MVP/SettingsPresenter.cs` — T01 output
- `Assets/Scripts/Core/MVP/ConfirmDialogPresenter.cs` — T01 output
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — S03 output: MonoBehaviour that Boot scene needs
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — S04 output: MonoBehaviour that Boot scene needs
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — S02 output: scene loader used by GameBootstrapper
- `Assets/Editor/SceneSetup.cs` — S02 output: existing scene creation script to extend

## Expected Output

- `Assets/Scripts/Runtime/MVP/MainMenuView.cs` — MonoBehaviour implementing IMainMenuView
- `Assets/Scripts/Runtime/MVP/SettingsView.cs` — MonoBehaviour implementing ISettingsView
- `Assets/Scripts/Runtime/MVP/ConfirmDialogView.cs` — MonoBehaviour implementing IConfirmDialogView
- `Assets/Scripts/Runtime/PopupManagement/UnityPopupContainer.cs` — MonoBehaviour implementing IPopupContainer
- `Assets/Scripts/Runtime/Boot/GameBootstrapper.cs` — Boot scene initializer MonoBehaviour
- `Assets/Editor/SceneSetup.cs` — modified: creates Boot scene + populates MainMenu/Settings with UI content
- `Assets/Scenes/Boot.unity` — persistent scene with full Canvas hierarchy
- `Assets/Scenes/MainMenu.unity` — modified: populated with UI content and MainMenuView
- `Assets/Scenes/Settings.unity` — modified: populated with UI content and SettingsView
