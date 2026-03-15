---
estimated_steps: 8
estimated_files: 5
---

# T01: Implement ScreenManager, ISceneLoader, and edit-mode tests

**Slice:** S02 — Screen Management
**Milestone:** M001

## Description

Create the core screen navigation system: `ScreenId` enum for type-safe screen identification, `ISceneLoader` interface to abstract Unity's scene loading API for testability, `ScreenManager` as a plain C# class handling navigation logic (additive scene load/unload, history stack, back navigation, concurrency guard), and `UnitySceneLoader` as the real Unity implementation. Then write comprehensive edit-mode tests using a `MockSceneLoader` to prove all navigation logic without Unity runtime.

## Steps

1. Create `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — enum with `MainMenu`, `Settings` values and a static helper method `ToSceneName(ScreenId)` returning the string scene name
2. Create `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — interface with `UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct = default)` and `UniTask UnloadSceneAsync(string sceneName, CancellationToken ct = default)`
3. Create `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — plain C# class with:
   - Constructor takes `ISceneLoader`, stores it as `readonly` field
   - `Stack<ScreenId> _history` for back navigation
   - `ScreenId? _currentScreen` tracking active screen
   - `bool _isNavigating` guard flag
   - `ShowScreenAsync(ScreenId, CancellationToken)` — guards against concurrent nav, unloads current screen (if any, pushing to history), loads new screen, updates `_currentScreen`
   - `GoBackAsync(CancellationToken)` — if history is empty, returns early; otherwise pops previous screen, unloads current, loads previous
   - Properties: `CurrentScreen`, `CanGoBack`
4. Create `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — implements `ISceneLoader`, wraps `SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)` and `SceneManager.UnloadSceneAsync(sceneName)` using UniTask's `await` on `AsyncOperation`
5. Create `Assets/Tests/EditMode/ScreenManagerTests.cs` with:
   - `MockSceneLoader` — implements `ISceneLoader`, records calls in `List<string> LoadedScenes` and `List<string> UnloadedScenes` plus `List<string> CallLog` (ordered "load:X" / "unload:X" entries), returns completed UniTask
   - 8 test methods:
     1. `ShowScreenAsync_LoadsCorrectScene` — show MainMenu → verify "MainMenu" in LoadedScenes
     2. `ShowScreenAsync_UnloadsPreviousBeforeLoadingNext` — show MainMenu → show Settings → verify CallLog order: load MainMenu, unload MainMenu, load Settings
     3. `GoBackAsync_ReturnsToPreviousScreen` — show MainMenu → show Settings → GoBack → verify MainMenu loaded again
     4. `GoBackAsync_WithEmptyHistory_IsNoOp` — GoBack on fresh manager → no loads, no unloads, no exception
     5. `CurrentScreen_TracksActiveScreen` — verify null initially, MainMenu after show, Settings after second show
     6. `CanGoBack_ReflectsHistoryState` — false initially, true after 2 navigations, false after GoBack to root
     7. `ShowScreenAsync_GuardsAgainstConcurrentNavigation` — verify navigation-in-progress flag prevents interleaving
     8. `FirstShowScreen_DoesNotUnload` — show MainMenu → verify UnloadedScenes is empty
6. Run batchmode test command and verify TestResults.xml shows all passing
7. Run static state guard grep on new files — verify no violations
8. Run batchmode compile — verify zero `error CS` hits

## Must-Haves

- [ ] `ScreenId` enum with `MainMenu` and `Settings` values
- [ ] `ISceneLoader` interface with `LoadSceneAdditiveAsync` and `UnloadSceneAsync` returning `UniTask`
- [ ] `ScreenManager` is a plain C# class (not MonoBehaviour) with no static state
- [ ] `ScreenManager` tracks current screen via `CurrentScreen` property
- [ ] `ScreenManager.ShowScreenAsync` unloads previous screen before loading next
- [ ] `ScreenManager.GoBackAsync` pops history stack and navigates to previous screen
- [ ] `ScreenManager` has navigation-in-progress guard preventing concurrent navigation
- [ ] `UnitySceneLoader` uses `LoadSceneMode.Additive` for scene loading
- [ ] All 8 edit-mode tests pass
- [ ] No `using UnityEngine` in Core/ScreenManagement files (except UniTask which is pure C#)

## Verification

- Batchmode test run: `Unity.exe -batchmode -projectPath C:\OtherWork\simplegame -runTests -testPlatform EditMode -testResults TestResults.xml` → TestResults.xml shows all tests passing, 0 failures
- Static guard: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ScreenManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output
- Compile: exit code 0, zero `error CS` in log

## Observability Impact

- **`ScreenManager.CurrentScreen`** — readable at any point; after `ShowScreenAsync` completes it reflects the newly loaded screen; `null` on construction
- **`ScreenManager.CanGoBack`** — readable at any point; `true` when the history stack is non-empty
- **`MockSceneLoader.CallLog`** — ordered list of `"load:X"` / `"unload:X"` entries captured during tests; test failure messages show the exact call sequence to diagnose ordering bugs
- **`TestResults.xml`** — written by the batchmode test runner; inspect via `Select-String -Pattern "testcase"` for pass/fail per test; zero failures is the pass criterion
- **`Logs/Editor.log`** — batchmode compile and test output; inspect for `error CS` lines if compilation fails
- **Failure shape** — if `ScreenManager._isNavigating` guard fires, `ShowScreenAsync` returns without loading/unloading; `CurrentScreen` stays unchanged and `CallLog` shows no new entries for that call; detectable in tests by asserting expected scene was NOT loaded

## Inputs

- `Assets/Scripts/Core/MVP/IView.cs` — marker interface pattern to follow
- `Assets/Scripts/Core/MVP/Presenter.cs` — two-phase lifecycle pattern
- `Assets/Scripts/SimpleGame.Runtime.asmdef` — runtime assembly that new code compiles into
- `Assets/Tests/EditMode/SimpleGame.Tests.EditMode.asmdef` — test assembly for ScreenManagerTests
- `Assets/Tests/EditMode/MVPWiringTests.cs` — MockSampleView pattern to follow for MockSceneLoader
- S01 forward intelligence: do NOT add `-quit` to `-runTests`; use `event Action` pattern; pure C# in Core

## Expected Output

- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — enum + ToSceneName helper
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — scene loading abstraction
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — navigation logic with history stack
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — real Unity scene loader
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — MockSceneLoader + 8 NUnit test methods
- Updated `TestResults.xml` — all tests passing (S01's 6 + S02's 8 = 14 total)
