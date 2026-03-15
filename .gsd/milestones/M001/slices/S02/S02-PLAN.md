# S02: Screen Management

**Goal:** ScreenManager navigates between screens using additive scene loading, with testable navigation logic and placeholder screen scenes registered in build settings.
**Demo:** Edit-mode tests pass proving navigation logic (load/unload sequencing, history stack, back navigation, guards). Placeholder MainMenu and Settings scenes exist and are registered in EditorBuildSettings for runtime loading.

## Must-Haves

- `ScreenId` enum for type-safe screen identification (MainMenu, Settings)
- `ISceneLoader` interface abstracting scene load/unload behind UniTask async methods
- `ScreenManager` as a plain C# class (not MonoBehaviour) with `ShowScreenAsync` and `GoBackAsync`
- `UnitySceneLoader` wrapping `SceneManager.LoadSceneAsync`/`UnloadSceneAsync` with additive mode
- Edit-mode tests proving all navigation logic using `MockSceneLoader`
- Placeholder MainMenu.unity and Settings.unity scenes registered in EditorBuildSettings
- No static state in any new file

## Proof Level

- This slice proves: contract (navigation logic) + integration (scene registration)
- Real runtime required: no (logic tested via mocks; scene registration verified by asset inspection)
- Human/UAT required: no (play-mode walkthrough deferred to S05)

## Verification

- `ScreenManagerTests.cs` — 8 edit-mode tests pass via batchmode: `TestResults.xml` shows all passing, 0 failures
- Static state guard: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ScreenManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` returns nothing
- Compilation clean: batchmode compile exits 0 with zero `error CS` in log
- Scenes registered: `EditorBuildSettings.asset` contains MainMenu and Settings entries

## Observability / Diagnostics

- Runtime signals: ScreenManager tracks `CurrentScreen` and `CanGoBack` — inspectable state at any point
- Inspection surfaces: `TestResults.xml` for test results; `EditorBuildSettings.asset` for scene registration; `Logs/Editor.log` for compile errors
- Failure visibility: MockSceneLoader records all load/unload calls with ordering — test failures show exact call sequence
- Redaction constraints: none

## Integration Closure

- Upstream surfaces consumed: `IView` (marker interface), `Presenter<TView>` (base class), UniTask (async), `SimpleGame.Runtime.asmdef` (assembly), `SimpleGame.Tests.EditMode.asmdef` (test assembly)
- New wiring introduced in this slice: `ScreenManager` constructed with `ISceneLoader` via constructor injection; `ScreenId`-to-scene-name mapping; `UnitySceneLoader` wraps Unity's `SceneManager` API
- What remains before the milestone is truly usable end-to-end: S03 (popups + input blocking), S04 (transitions), S05 (boot flow wiring ScreenManager into UIFactory and connecting presenters to loaded screen views)

## Tasks

- [x] **T01: Implement ScreenManager, ISceneLoader, and edit-mode tests** `est:45m`
  - Why: Core deliverable of S02 — the navigation logic, its abstraction for testability, and the tests that prove it works. This is where R009 (additive scene management) and R010 (screen navigation) get their logic-level proof.
  - Files: `Assets/Scripts/Core/ScreenManagement/ScreenId.cs`, `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs`, `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`, `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs`, `Assets/Tests/EditMode/ScreenManagerTests.cs`
  - Do: Create `ScreenId` enum (MainMenu, Settings) with helper to convert to scene name. Create `ISceneLoader` with `LoadSceneAdditiveAsync` and `UnloadSceneAsync` returning `UniTask`. Implement `ScreenManager` as plain C# class with constructor-injected `ISceneLoader`, history stack, `ShowScreenAsync`/`GoBackAsync`, navigation-in-progress guard, `CurrentScreen`/`CanGoBack` properties. Implement `UnitySceneLoader` wrapping Unity SceneManager with additive mode. Write `ScreenManagerTests.cs` with `MockSceneLoader` (tracks load/unload calls with ordering) and 8 test methods covering: first show loads scene, show unloads previous before loading next, GoBack returns to previous, GoBack with empty history is no-op, CurrentScreen tracks active screen, CanGoBack reflects state, navigation guard prevents concurrent navigation, first show does not unload. Run tests via batchmode.
  - Verify: `Unity.exe -batchmode -projectPath ... -runTests -testPlatform EditMode -testResults TestResults.xml` → all tests pass, 0 failures
  - Done when: 8+ edit-mode tests pass in batchmode TestResults.xml; ScreenManager compiles with zero errors; no static state in new files

- [x] **T02: Create placeholder scenes and register in EditorBuildSettings** `est:20m`
  - Why: Scenes must exist as `.unity` files and be registered in EditorBuildSettings for `SceneManager.LoadSceneAsync` to work at runtime. This closes the integration gap — without it, ScreenManager's logic is proven but runtime scene loading would fail.
  - Files: `Assets/Editor/SceneSetup.cs`, `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/Settings.unity`, `ProjectSettings/EditorBuildSettings.asset`
  - Do: Create an Editor script (`SceneSetup.cs`) with a static method callable via `-executeMethod` that creates empty MainMenu and Settings scenes using `EditorSceneManager.NewScene()` + `SaveScene()`, then registers them in `EditorBuildSettings.scenes`. Run via batchmode `-executeMethod`. Verify scenes exist on disk and appear in EditorBuildSettings.asset. The Editor script goes in `Assets/Editor/` (not in the runtime assembly).
  - Verify: Batchmode `-executeMethod` succeeds; `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Settings.unity` exist; `EditorBuildSettings.asset` contains both scene paths
  - Done when: Both scene files exist on disk, EditorBuildSettings.asset lists both scenes, project compiles cleanly

## Files Likely Touched

- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs`
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs`
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs`
- `Assets/Tests/EditMode/ScreenManagerTests.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/Settings.unity`
- `ProjectSettings/EditorBuildSettings.asset`
