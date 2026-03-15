# S05: Boot Flow & Demo Screens — Research

**Date:** 2026-03-15

## Summary

S05 is the final integration slice that proves the full dependency chain works end-to-end. All infrastructure is already built: `ScreenManager` (S02), `PopupManager` + `InputBlocker` (S03), `TransitionManager` (S04), and the core MVP types (S01). What's missing is the glue: a boot scene that constructs all services and managers, concrete view interfaces and MonoBehaviour implementations for MainMenu/Settings/ConfirmDialog, the UIFactory expansion to create screen and popup presenters, a concrete `IPopupContainer` implementation, and the scene content (Canvas hierarchies with buttons and text). No new architectural patterns are needed — every pattern was established in S01–S04 and just needs to be instantiated for real screens.

The primary risk is the volume of interconnected pieces that must all wire correctly on the first play-mode run: Boot scene MonoBehaviour → services → managers → UIFactory → scene load → view discovery → presenter creation → event wiring → navigation. Each link in this chain has been individually proven by edit-mode tests, but the runtime integration is untested. The second risk is scene setup via batchmode — creating Canvases with Buttons, Text (or TMP), EventSystem, and CanvasGroups programmatically in batchmode scripts requires careful use of Unity's editor APIs.

The approach should be: (1) define view interfaces and presenters for MainMenu, Settings, and ConfirmDialog in Core; (2) expand UIFactory with Create methods for each; (3) implement concrete View MonoBehaviours in Runtime; (4) create a concrete `IPopupContainer` (prefab instantiation or show/hide overlay); (5) build the Boot scene with an initializer MonoBehaviour; (6) update SceneSetup to populate MainMenu and Settings scenes with UI content and create the Boot scene; (7) add edit-mode tests for the new UIFactory wiring; (8) verify the full flow in play mode.

## Recommendation

**Use a single persistent Boot scene with an overlay popup approach (not scene-based popups).** Popups should be pre-instantiated in the Boot scene's Canvas and shown/hidden by the `IPopupContainer` implementation — this avoids the complexity of registering popup scenes in EditorBuildSettings and the lifecycle issues of additive-loading tiny popup scenes. The Boot scene's initializer MonoBehaviour (`GameBootstrapper`) should use `Awake()` or `Start()` to construct all services, create managers, find existing MonoBehaviour components on the persistent scene's Canvas (InputBlocker, TransitionPlayer), build the UIFactory, and call `ScreenManager.ShowScreenAsync(ScreenId.MainMenu)` to kick off the first navigation.

For view discovery after screen scene loads: after `ScreenManager` loads a scene, the boot flow needs a callback to find the scene's view MonoBehaviour and create its presenter. Since `ScreenManager` doesn't own presenter lifecycle (Decision #9), the `GameBootstrapper` should register a post-navigation callback or wrap ScreenManager calls. The simplest approach: `GameBootstrapper` calls `ScreenManager.ShowScreenAsync()`, then after awaiting it, uses `FindObjectOfType<MainMenuView>()` (or similar) to locate the view and create the presenter via UIFactory. This keeps ScreenManager clean and puts wiring in the single boot entry point.

For TextMeshPro vs legacy Text: Use `UnityEngine.UI.Text` (legacy uGUI Text) since the project constraint is uGUI and TMP may not be installed. This keeps dependencies minimal and avoids the TMP package import.

## Don't Hand-Roll

| Problem | Existing Solution | Why Use It |
|---------|------------------|------------|
| Async scene loading | `UnitySceneLoader` (S02) | Already wraps `SceneManager.LoadSceneAsync` with UniTask; proven by 8 tests |
| Input blocking during transitions | `UnityInputBlocker` (S03) + ScreenManager transition brackets (S04) | Reference-counted, CanvasGroup-based; integrated into ScreenManager's finally block |
| Fade transitions | `UnityTransitionPlayer` (S04) | CanvasGroup alpha interpolation with blocksRaycasts=false enforced; ready for Inspector wiring |
| Popup stack management | `PopupManager` (S03) | Stack-based with concurrency guard and input blocking; 13 tests prove the contract |
| Presenter construction pattern | `UIFactory` + `Presenter<TView>` (S01) | Two-phase lifecycle (ctor + Initialize); factory receives services once; proven by 6 tests |
| Scene creation in batchmode | `SceneSetup.cs` (S02) | `-executeMethod` pattern with `EditorSceneManager.NewScene`; already creates MainMenu and Settings |

## Existing Code and Patterns

- `Assets/Scripts/Core/MVP/UIFactory.cs` — Currently only has `CreateSamplePresenter()`. Must be extended with `CreateMainMenuPresenter()`, `CreateSettingsPresenter()`, `CreateConfirmDialogPresenter()`. Receives `ScreenManager`, `PopupManager` at construction alongside `GameService`.
- `Assets/Scripts/Core/MVP/Presenter.cs` — `Presenter<TView>` abstract base with `Initialize()`/`Dispose()`. All new presenters extend this. Constructor sets fields only; Initialize subscribes to view events.
- `Assets/Scripts/Core/MVP/ISampleView.cs` — Pattern to follow: `event Action OnButtonClicked` + `void UpdateLabel(string text)`. Each view interface uses `event Action` (not UnityEvent), no Unity types (Decision #3).
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — Pattern to follow: constructor injects view interface + services; `Initialize()` subscribes events + sets initial view state; `Dispose()` unsubscribes (Decision #4).
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — Ready for use. Constructor takes `(ISceneLoader, ITransitionPlayer?, IInputBlocker?)`. S05 must pass real implementations for all three.
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — Ready for use. Constructor takes `(IPopupContainer, IInputBlocker)`. S05 must provide a concrete `IPopupContainer`.
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — No implementation exists. S05 must create a `UnityPopupContainer` that shows/hides popup GameObjects. Pattern: `ShowPopupAsync` activates a popup GameObject; `HidePopupAsync` deactivates it.
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — MonoBehaviour with `[SerializeField] CanvasGroup`. Needs CanvasGroup wired in Inspector (or via code in scene setup). Will NullRef on first `Block()` if missing.
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — MonoBehaviour with `[SerializeField] CanvasGroup` and `[SerializeField] float _fadeDuration = 0.3f`. Same wiring requirement.
- `Assets/Editor/SceneSetup.cs` — Must be extended to: create Boot scene (with persistent Canvas, EventSystem, InputBlocker, TransitionPlayer, popup container); populate MainMenu scene with Canvas + buttons + text; populate Settings scene with Canvas + back button + text; register all three scenes in EditorBuildSettings with Boot as index 0.
- `Assets/Tests/EditMode/MVPWiringTests.cs` — `MockSampleView` pattern: `LastLabelText`, `UpdateLabelCallCount`, `SimulateButtonClick()`. Replicate for new view interfaces.

## Constraints

- **No static state** — `GameBootstrapper` must use instance fields only. The grep guard `grep -r "static " --include="*.cs" | grep -v "static void|static class|static readonly|static async|static UniTask"` must return empty. This means no `static string`, no `static Dictionary`, etc.
- **No `using UnityEngine` in Core/** — All view interfaces, presenters, and services must remain in `SimpleGame.Core.*` namespaces with zero Unity type references. View MonoBehaviours go in `Assets/Scripts/Runtime/`.
- **`event Action` convention (Decision #3)** — All view interface events must use `event Action` or `event Action<T>`, not UnityEvent.
- **Two-phase lifecycle (Decision #4)** — Constructor sets fields; `Initialize()` subscribes events. No async work in constructors.
- **`ScreenManager` doesn't own presenter lifecycle (Decision #9)** — ScreenManager only loads/unloads scenes. Presenter creation/disposal must happen outside ScreenManager, in the boot flow.
- **`enum.ToString()` = scene name (Decision #11)** — Scene files must be named exactly matching `ScreenId` enum members: `MainMenu.unity`, `Settings.unity`.
- **Boot scene must be EditorBuildSettings index 0** — Unity loads the first scene in build settings on play. Boot must be first, with MainMenu and Settings following.
- **`com.unity.modules.ui` is present** — uGUI is available. `UnityEngine.UI.Button`, `UnityEngine.UI.Text`, `Canvas`, `CanvasGroup`, `GraphicRaycaster`, `EventSystem` are all usable.
- **No `-quit` with `-runTests` (Decision #7)** — Test batchmode must not include `-quit`.
- **SimpleGame.Runtime.asmdef already references UniTask** — All Runtime code can use UniTask and UnityEngine without additional asmdef changes.

## Common Pitfalls

- **Forgetting to register Boot scene in EditorBuildSettings as index 0** — If Boot isn't the first scene, play mode starts on the wrong scene. `SceneSetup.cs` must be updated to list Boot first.
- **`FindObjectOfType` finding the wrong component after additive load** — After loading MainMenu additively, `FindObjectOfType<MainMenuView>()` searches all loaded scenes. Since Boot scene has its own Canvas, make sure view MonoBehaviours are only on their respective screen scene roots.
- **UnityInputBlocker / UnityTransitionPlayer CanvasGroup not wired** — Both have `[SerializeField] CanvasGroup` that must be assigned. A `NullReferenceException` on first use = missing wire-up. Scene setup script must add the CanvasGroup and assign the reference via `GetComponent<CanvasGroup>()`.
- **CanvasGroup for InputBlocker must be on a high-sort-order Canvas** — If the blocker Canvas sorts below screen Canvases, it won't block raycasts on those screens. Sort order should be 100+ for blocker, 200+ for transition overlay, 300+ for popup layer.
- **PopupManager and ScreenManager share the same `IInputBlocker` instance** — Both call `Block()`/`Unblock()` on the same reference-counted input blocker. This is correct behavior — simultaneous popup + transition blocking won't conflict because reference counting handles it. But both must receive the same instance.
- **Presenter disposal on screen unload** — When navigating away from a screen, the old presenter must be `Dispose()`d to unsubscribe events. If not, the old presenter still holds references to the view interface (which no longer exists) and could cause null reference errors on next event fire. The boot flow must track active screen presenters and dispose them before navigation.
- **Batchmode scene creation with UI components** — Adding `Button`, `Text`, `Canvas` etc. programmatically requires `using UnityEngine.UI` and `new GameObject().AddComponent<Canvas>()` patterns. The scene setup script must add `GraphicRaycaster` alongside Canvas for UI interaction to work. `EventSystem` and `StandaloneInputModule` must be added to the Boot (persistent) scene.
- **UIFactory dependencies expanding** — UIFactory currently only takes `GameService`. It needs `ScreenManager` and `PopupManager` for presenters that trigger navigation. But presenters shouldn't hold direct references to managers — they should receive callbacks or simpler interfaces. Consider passing navigation actions (`Action<ScreenId>`) to presenters instead of the full ScreenManager, keeping presenters testable.

## Open Risks

- **Programmatic scene creation complexity** — Creating Canvases with properly laid out UI (buttons centered, text positioned) via `SceneSetup.cs` batchmode script requires precise RectTransform configuration. If layout is too complex, consider creating minimal functional scenes (button + text) with no visual polish — the demo only needs to prove the dependency chain works.
- **`FindObjectOfType` after additive scene load timing** — The view MonoBehaviour may not be immediately available after `LoadSceneAsync` completes (single frame delay). May need a `UniTask.Yield()` or `UniTask.NextFrame()` before finding the view. The S04 UnityTransitionPlayer already uses `UniTask.Yield()` so the pattern is established.
- **ConfirmDialog popup approach decision** — The popup container implementation must decide: pre-instantiated popup GameObjects (show/hide) vs. prefab instantiation. Pre-instantiated is simpler for this demo — the ConfirmDialog view lives in the Boot scene, starts inactive, and the container activates/deactivates it. This avoids prefab loading concerns.
- **UIFactory growing too large** — With 3 new Create methods plus the existing one, UIFactory may need 5+ constructor parameters (GameService, ScreenManager, PopupManager, etc.). This is acceptable for the demo scope but signals that a registry or builder pattern may be needed if the project grows beyond M001.

## Skills Discovered

| Technology | Skill | Status |
|------------|-------|--------|
| Unity (general) | `rmyndharis/antigravity-skills@unity-developer` (568 installs) | available — but project patterns are well-established; not needed |
| Unity ECS | `wshobson/agents@unity-ecs-patterns` (3.1K installs) | available — irrelevant, project uses MVP not ECS |
| Unity workflows | `cryptorabea/claude_unity_dev_plugin@unity-workflows` (46 installs) | available — low install count, project already has working batchmode patterns |

No skills are recommended for installation — the project's own established patterns (from S01–S04 summaries and decisions) provide sufficient guidance, and the available Unity skills target different architectures or are too generic.

## Requirements Owned/Supported by S05

### Primary Owner
- **R008** (Boot scene → main scene initialization flow) — S05 creates the boot scene and the `GameBootstrapper` MonoBehaviour that wires all services, managers, and factory, then transitions to MainMenu.
- **R016** (Demo screens proving end-to-end dependency flow) — S05 implements MainMenu, Settings, and ConfirmDialog demo views/presenters proving the full chain.

### Supporting
- **R001** (MVP pattern) — S05 instantiates the MVP pattern for 3 real screens/popups, proving it works beyond the S01 sample.
- **R002** (View independence) — S05 view MonoBehaviours must have zero backward references to presenters/services; they only expose interfaces.
- **R004** (Central UI factory) — UIFactory is expanded with Create methods for all demo presenters; remains the single wiring point.
- **R005** (Constructor/init injection only) — All S05 wiring uses constructor injection; GameBootstrapper passes dependencies explicitly.
- **R006** (No static state) — All new files must pass the static grep guard.
- **R007** (Domain services) — GameService is connected through the real boot flow for the first time.
- **R009** (Hybrid scene management) — Runtime integration proof: Boot scene persists while screen scenes load/unload additively.
- **R010** (Screen navigation) — Runtime integration proof: navigate MainMenu → Settings → MainMenu with real scenes.
- **R011** (Stack-based popup system) — Runtime integration proof: open ConfirmDialog popup, dismiss it.
- **R012** (Input blocker) — Runtime integration proof: input blocked during transitions and popups.
- **R013** (Fade transitions) — Runtime integration proof: UnityTransitionPlayer visually fades during navigation.
- **R014** (UniTask) — All runtime async operations use UniTask (scene loading, transitions, popup show/hide).
- **R015** (Edit-mode tests) — New tests for UIFactory expansion and demo presenter construction.
- **R017** (Each layer testable in isolation) — New MockMainMenuView, MockSettingsView, MockConfirmDialogView enable presenter testing.

## What Must Be Created (Gap Analysis)

### Core (Pure C#, no UnityEngine)
1. `IMainMenuView : IView` — events: `OnSettingsClicked`, `OnPopupClicked`; methods: `UpdateTitle(string)`
2. `ISettingsView : IView` — events: `OnBackClicked`; methods: `UpdateTitle(string)`
3. `IConfirmDialogView : IPopupView` — events: `OnConfirmClicked`, `OnCancelClicked`; methods: `UpdateMessage(string)`
4. `MainMenuPresenter : Presenter<IMainMenuView>` — handles Settings navigation + popup opening
5. `SettingsPresenter : Presenter<ISettingsView>` — handles Back navigation
6. `ConfirmDialogPresenter : Presenter<IConfirmDialogView>` — handles confirm/cancel (dismiss popup)
7. `UIFactory` expansion — add `CreateMainMenuPresenter`, `CreateSettingsPresenter`, `CreateConfirmDialogPresenter`; constructor gains ScreenManager + PopupManager (or navigation/popup actions)

### Runtime (MonoBehaviours)
8. `MainMenuView : MonoBehaviour, IMainMenuView` — wires `Button.onClick` to `event Action`; serializes `Text` for title
9. `SettingsView : MonoBehaviour, ISettingsView` — same pattern
10. `ConfirmDialogView : MonoBehaviour, IConfirmDialogView` — same pattern
11. `UnityPopupContainer : MonoBehaviour, IPopupContainer` — holds reference to popup GameObjects by PopupId; show = SetActive(true), hide = SetActive(false)
12. `GameBootstrapper : MonoBehaviour` — Boot scene initializer; constructs services → managers → factory → first navigation

### Scenes
13. Boot scene — persistent Canvas with: EventSystem, InputBlocker overlay, Transition overlay, Popup layer (ConfirmDialog), GameBootstrapper script
14. MainMenu scene — Canvas with "Main Menu" title text, "Settings" button, "Open Popup" button
15. Settings scene — Canvas with "Settings" title text, "Back" button
16. EditorBuildSettings update — Boot at index 0, MainMenu at index 1, Settings at index 2

### Tests
17. Edit-mode tests for new UIFactory Create methods + demo presenter construction/event-wiring

## Sources

- All findings sourced from the existing codebase at `C:/OtherWork/simplegame/Assets/` and the preloaded slice summaries S01–S04
- Decision register at `.gsd/DECISIONS.md` (decisions #1–#15) — drives constraints on static state, event conventions, lifecycle patterns, and scene naming
- `IPopupContainer` interface at `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — gap: no runtime implementation exists
- `UIFactory` at `Assets/Scripts/Core/MVP/UIFactory.cs` — gap: only has `CreateSamplePresenter`, needs 3 more Create methods
- `SceneSetup.cs` at `Assets/Editor/SceneSetup.cs` — must be extended to create Boot scene and populate screen scenes with UI content
