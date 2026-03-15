# S02: Screen Management — Research

**Date:** 2026-03-15

## Summary

S02 introduces `ScreenManager` — the system that navigates between full screens using Unity's additive scene loading. The core challenge is making screen navigation logic **testable in edit-mode** (no Unity runtime) while the real implementation depends on `SceneManager.LoadSceneAsync` / `UnloadSceneAsync` (Unity-only APIs). The solution is to abstract scene loading behind an `ISceneLoader` interface: ScreenManager receives it via constructor injection, tests supply a mock, and a real `UnitySceneLoader` handles actual scenes at runtime.

The second challenge is the **persistent scene model** (R009). One scene stays loaded at all times (hosting shared UI infrastructure: screen manager, input blocker, transition overlay, popup layer in later slices). Screen scenes load additively on top and unload when navigating away. This means ScreenManager must track which screen scene is currently loaded and clean it up before loading the next.

Scene creation is a Unity Editor API operation — scenes must exist as `.unity` files in `Assets/Scenes/` and be registered in `EditorBuildSettings.scenes` for runtime loading. This requires a batchmode `-executeMethod` call to create scenes and register them, not manual file creation.

## Recommendation

**Approach: ScreenManager as a plain C# class with injected ISceneLoader**

1. Define `ISceneLoader` interface with `LoadSceneAdditiveAsync(string)` and `UnloadSceneAsync(string)` returning `UniTask`
2. `ScreenManager` is a plain C# class (not MonoBehaviour) — receives `ISceneLoader` via constructor, tracks current screen name, provides `ShowScreenAsync(string)` and `GoBackAsync()` with a history stack
3. `UnitySceneLoader` is the real implementation wrapping `SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive)` and `SceneManager.UnloadSceneAsync(name)`
4. Use a `ScreenId` enum (not raw strings) for type-safe screen identification, with a helper to convert to scene name strings
5. Create placeholder scenes (MainMenu, Settings) via Editor scripting in batchmode
6. Edit-mode tests use a `MockSceneLoader` that tracks load/unload calls without any Unity runtime dependency

**Why enum over strings:** The M001-CONTEXT.md flagged this as an open question. Enum provides compile-time safety, prevents typos, and makes the set of available screens discoverable. The enum-to-scene-name mapping is a single `switch` or dictionary — trivial cost for significant safety.

**Why ISceneLoader abstraction:** Without it, ScreenManager cannot be tested in edit-mode at all — `SceneManager` is a static Unity API with no way to mock it. The interface boundary also cleanly separates navigation logic (what to load/unload, when, in what order) from Unity engine concerns (how to actually load a scene).

## Don't Hand-Roll

| Problem | Existing Solution | Why Use It |
|---------|------------------|------------|
| Async scene loading | UniTask `await SceneManager.LoadSceneAsync()` | UniTask already installed; direct `await` on `AsyncOperation` is zero-allocation, no coroutine needed |
| Cancellation on rapid navigation | `CancellationTokenSource` + `WithCancellation()` | Standard UniTask pattern; cancel pending load if user navigates again before previous completes |
| Test assertions | NUnit (already installed via `com.unity.test-framework` 1.6.0) | Pattern established in S01; `MVPWiringTests.cs` is the template |

## Existing Code and Patterns

- `Assets/Scripts/Core/MVP/IView.cs` — `IView` marker interface; new `IScreenView` should extend this
- `Assets/Scripts/Core/MVP/Presenter.cs` — `Presenter<TView>` with two-phase lifecycle (ctor + Initialize/Dispose); screen presenters follow this exactly
- `Assets/Scripts/Core/MVP/UIFactory.cs` — central factory; must receive `ScreenManager` at construction and add `CreateMainMenuPresenter()` / `CreateSettingsPresenter()` methods
- `Assets/Scripts/Core/MVP/ISampleView.cs` — pattern to follow for `IMainMenuScreenView` and `ISettingsScreenView` (events as `event Action`, update methods, no Unity types)
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — pattern to follow for screen presenters (constructor injection, Initialize subscribes events, Dispose unsubscribes)
- `Assets/Tests/EditMode/MVPWiringTests.cs` — test patterns: `MockSampleView` is the template for `MockSceneLoader` and mock screen views
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — screen management code goes here (same assembly)
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — new tests go here
- `ProjectSettings/EditorBuildSettings.asset` — currently has empty `m_Scenes: []`; must be populated with screen scenes
- `Packages/manifest.json` — UniTask already installed; no new packages needed for S02

## Constraints

- **Scenes must be in EditorBuildSettings to load at runtime** — `SceneManager.LoadSceneAsync` only works for scenes registered in Build Settings. Scene creation and registration must happen via Editor scripting (`-executeMethod`), not by writing `.unity` files by hand.
- **No static state (R006)** — `ScreenManager` cannot use static fields or singleton patterns. It must be instantiated and injected.
- **View interfaces must have no Unity types (Decision #3)** — `IMainMenuScreenView` and `ISettingsScreenView` use `event Action` only, no `UnityEvent`, no `Button.onClick`.
- **Two-phase lifecycle (Decision #4)** — Screen presenters use constructor injection for fields, `Initialize()` for event subscription. This is where async initialization hooks in.
- **No `-quit` with `-runTests` (Decision #7)** — test run commands must omit `-quit` flag.
- **UniTask via git URL (Decision #1)** — already installed, no action needed.
- **Assembly references use string names (Decision #2)** — no GUID references in asmdef files.
- **Additive scene loading required (R009)** — screens load via `LoadSceneMode.Additive`, not `LoadSceneMode.Single`. The persistent scene must survive.
- **`SceneManager.UnloadSceneAsync` requires scene to be loaded** — calling unload on a scene that isn't loaded throws. ScreenManager must track current scene state.
- **Play-mode test for scene loading is deferred (R019)** — actual scene loading verification is manual play-mode walkthrough, not automated play-mode tests.

## Common Pitfalls

- **Unloading the persistent scene** — If ScreenManager accidentally calls `UnloadSceneAsync` on the persistent/boot scene instead of the screen scene, everything breaks. Guard against this by only tracking screen scene names and never exposing the persistent scene name to the unload path.
- **Double-navigation race condition** — If `ShowScreenAsync` is called while a previous navigation is in progress, both unload and load operations can interleave. Use a `CancellationTokenSource` that gets cancelled and replaced on each new navigation, plus a navigation-in-progress guard flag.
- **Scene not in Build Settings** — `SceneManager.LoadSceneAsync` silently fails or throws if the scene isn't registered. Must create an Editor script that creates scenes AND adds them to `EditorBuildSettings.scenes` in a single batchmode pass.
- **Testing ScreenManager with real SceneManager** — Cannot do in edit-mode. The `ISceneLoader` abstraction is the only way to get edit-mode test coverage of navigation logic. Don't try to test scene loading in edit-mode — it simply doesn't work.
- **GoBack with empty history** — `GoBackAsync()` on an empty stack must be a safe no-op (or return false), not throw. Test this case explicitly.
- **Forgetting to create scene files** — Scene `.unity` files cannot be created by writing YAML by hand (the format is complex and version-dependent). Must use `EditorSceneManagement.EditorSceneManager.NewScene()` + `SaveScene()` in an Editor script executed via batchmode.
- **ScreenManager holding disposed presenter references** — When a screen is unloaded, its presenter must be Dispose'd. ScreenManager needs to either own presenter lifecycle or delegate it clearly. Since UIFactory creates presenters, ScreenManager should call `Dispose()` on the current screen's presenter before unloading the scene.

## Open Risks

- **Scene creation in batchmode** — Creating and registering scenes via `-executeMethod` in batchmode hasn't been tested in this project yet. If it fails, scenes would need to be created manually in the Unity Editor. Low risk (well-documented API) but needs verification.
- **UniTask `await SceneManager.LoadSceneAsync` with `LoadSceneMode.Additive`** — The UniTask docs show `await SceneManager.LoadSceneAsync("scene2")` but don't explicitly show the additive overload. The standard Unity overload `LoadSceneAsync(string, LoadSceneMode)` should work since UniTask awaits the returned `AsyncOperation`, but this needs a play-mode verification pass.
- **Screen presenter lifecycle ownership** — ScreenManager needs to know about presenters (to Dispose them), but creating presenters requires view instances that only exist after scene load. The sequence is: load scene → find view MonoBehaviour → pass to UIFactory → get presenter → Initialize. On navigation away: Dispose presenter → unload scene. If this lifecycle ordering is wrong, null references or leaked subscriptions result. This is the primary design risk.

## Design Sketch: ScreenManager API

```csharp
// Screen identification
public enum ScreenId { MainMenu, Settings }

// Scene loading abstraction (testable)
public interface ISceneLoader
{
    UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default);
    UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default);
}

// ScreenManager — plain C# class, no MonoBehaviour
public class ScreenManager
{
    private readonly ISceneLoader _sceneLoader;
    private readonly Stack<ScreenId> _history;
    private ScreenId? _currentScreen;
    private bool _isNavigating;

    public ScreenManager(ISceneLoader sceneLoader) { ... }

    public async UniTask ShowScreenAsync(ScreenId screenId, CancellationToken ct = default) { ... }
    public async UniTask GoBackAsync(CancellationToken ct = default) { ... }
    public ScreenId? CurrentScreen => _currentScreen;
    public bool CanGoBack => _history.Count > 0;
}
```

**Key design note:** Presenter lifecycle (create/initialize/dispose) is NOT inside ScreenManager for S02. ScreenManager only handles scene load/unload. Presenter wiring happens in S05 when the boot flow connects UIFactory to screen views found in loaded scenes. This keeps S02 focused on navigation logic, which is independently testable.

## Folder Structure

```
Assets/Scripts/Core/
├── MVP/             (existing — IView, Presenter, UIFactory, etc.)
├── Services/        (existing — GameService)
└── ScreenManagement/
    ├── ScreenId.cs
    ├── ScreenManager.cs
    └── ISceneLoader.cs

Assets/Scripts/Runtime/
└── ScreenManagement/
    └── UnitySceneLoader.cs    (the MonoBehaviour-free UnityEngine wrapper)

Assets/Scenes/
├── MainMenu.unity
└── Settings.unity

Assets/Tests/EditMode/
├── MVPWiringTests.cs          (existing)
└── ScreenManagerTests.cs
```

**Note:** `UnitySceneLoader` uses `UnityEngine.SceneManagement` so it belongs in Runtime, not Core. Core types should remain Unity-type-free per the established pattern (no `using UnityEngine` in Core). However, `ScreenManager` itself is pure C# and belongs in Core since it only depends on the `ISceneLoader` interface and UniTask.

Actually, on reflection: `ISceneLoader` returns `UniTask` which is already in the runtime asmdef. And `ScreenManager` only depends on `ISceneLoader` + `UniTask` — both of which are pure C# compatible. The split is clean: Core for logic, Runtime folder for Unity-dependent implementations.

## Test Plan (Edit-Mode)

Tests for `ScreenManager` using `MockSceneLoader`:

1. **ShowScreenAsync loads the correct scene** — verify MockSceneLoader received the right scene name
2. **ShowScreenAsync unloads previous screen first** — show screen A, then show screen B, verify A was unloaded before B was loaded
3. **GoBackAsync returns to previous screen** — show A → show B → GoBack → verify A is loaded
4. **GoBackAsync with no history is safe** — no exception, no-op
5. **CurrentScreen tracks the active screen** — verify property after ShowScreen
6. **CanGoBack reflects history state** — false initially, true after two navigations, false after GoBack to root
7. **Double navigation guard** — ShowScreenAsync while navigating returns or queues correctly (not interleave)
8. **First ShowScreen does not unload anything** — no unload call when there's no previous screen

## Skills Discovered

| Technology | Skill | Status |
|------------|-------|--------|
| Unity (general) | `wshobson/agents@unity-ecs-patterns` | available (3.1K installs) — ECS-focused, not directly relevant to uGUI MVP |
| Unity (general) | `rmyndharis/antigravity-skills@unity-developer` | available (568 installs) — general Unity, may have useful patterns |
| Unity Scene Management | `dev-gom/claude-code-marketplace@unity scene optimizer` | available (29 installs) — low installs, optimization focus not relevant |
| UniTask | `creator-hian/claude-code-plugins@unity-async` | available (5 installs) — very low installs |

No skills are directly relevant enough to recommend installing. The `unity-developer` skill (568 installs) is the closest match but is too general. The project's patterns are well-established from S01; following them is more valuable than a generic Unity skill.

## Sources

- UniTask README documents `await SceneManager.LoadSceneAsync("scene2")` for async scene loading (source: [UniTask GitHub](https://github.com/Cysharp/UniTask))
- UniTask `CancellationTokenSource` + `WithCancellation()` is the standard cancellation pattern for async operations (source: [UniTask GitHub](https://github.com/Cysharp/UniTask))
- Unity `SceneManager.LoadSceneAsync(string, LoadSceneMode.Additive)` loads scenes additively without unloading existing scenes (source: Unity SceneManagement API — standard knowledge)
- Unity `SceneManager.UnloadSceneAsync(string)` unloads a previously additively-loaded scene (source: Unity SceneManagement API — standard knowledge)
- Scenes must be added to `EditorBuildSettings.scenes` to be loadable via `SceneManager.LoadSceneAsync` at runtime (source: Unity SceneManagement API — standard knowledge)
- `EditorSceneManager.NewScene()` + `EditorSceneManager.SaveScene()` creates and saves `.unity` files programmatically (source: Unity Editor API — standard knowledge)
- Existing codebase analysis: all 7 C# files, 2 asmdef files, manifest.json, EditorBuildSettings.asset examined
