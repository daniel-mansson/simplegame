---
id: S02
parent: M001
milestone: M001
written: 2026-03-15
---

# S02: Screen Management — UAT

**Milestone:** M001
**Written:** 2026-03-15

## UAT Type

- UAT mode: artifact-driven
- Why this mode is sufficient: S02's proof level is contract (navigation logic) + integration (scene registration). Navigation logic is fully proven by edit-mode tests (no Unity runtime required). Scene registration is proven by artifact inspection of `EditorBuildSettings.asset` and scene files on disk. Play-mode runtime walkthrough is deferred to S05 which wires ScreenManager into the full boot flow.

## Preconditions

- Unity project is at `C:\OtherWork\simplegame`
- `TestResults.xml` is present at project root from the last batchmode test run
- `ProjectSettings/EditorBuildSettings.asset` is readable
- `Assets/Scenes/` directory exists

## Smoke Test

Open `TestResults.xml` and confirm `result="Passed" total="14" passed="14" failed="0"`. If this passes, S02's core deliverable is intact.

---

## Test Cases

### 1. All 8 ScreenManagerTests pass in batchmode

**What it proves:** The ScreenManager navigation contract is correct — additive load/unload sequencing, history stack, back navigation, concurrency guard, and state properties all behave as specified.

1. Locate `TestResults.xml` at the project root.
2. Check the `test-run` element attributes: `result`, `total`, `passed`, `failed`.
3. Find the `ScreenManagerTests` test suite element.
4. Count the `test-case` elements within it.
5. **Expected:** `result="Passed" total="14" passed="14" failed="0"` on `test-run`; `testcasecount="8" result="Passed" passed="8" failed="0"` on `ScreenManagerTests` suite; all 8 test cases present with `result="Passed"`.

Expected test case names:
- `ShowScreenAsync_LoadsCorrectScene`
- `ShowScreenAsync_UnloadsPreviousBeforeLoadingNext`
- `GoBackAsync_ReturnsToPreviousScreen`
- `GoBackAsync_WithEmptyHistory_IsNoOp`
- `CurrentScreen_TracksActiveScreen`
- `CanGoBack_ReflectsHistoryState`
- `ShowScreenAsync_GuardsAgainstConcurrentNavigation`
- `FirstShowScreen_DoesNotUnload`

---

### 2. S01 tests are not regressed (all 6 MVPWiringTests still pass)

**What it proves:** Adding ScreenManagement files did not break the S01 MVP wiring.

1. In `TestResults.xml`, find the `MVPWiringTests` test suite element.
2. Check `testcasecount`, `result`, `passed`, `failed`.
3. **Expected:** `testcasecount="6" result="Passed" passed="6" failed="0"`.

---

### 3. Static state guard passes for new ScreenManagement core files

**What it proves:** No static state was introduced in the core navigation logic (R006 compliance).

1. Run: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ScreenManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"`
2. **Expected:** No output (command exits with code 1 — grep found no matches). Any output line indicates a violation that must be investigated.

---

### 4. No UnityEngine dependency in Core ScreenManagement files

**What it proves:** Core navigation logic has no Unity runtime coupling — supports edit-mode testability and layer separation (R001, R017).

1. Run: `grep -n "using UnityEngine" Assets/Scripts/Core/ScreenManagement/*.cs`
2. **Expected:** No output. The three core files (`ScreenId.cs`, `ISceneLoader.cs`, `ScreenManager.cs`) must have zero `using UnityEngine` statements.

---

### 5. MainMenu.unity and Settings.unity exist on disk

**What it proves:** Placeholder scene files were created and are available for runtime loading (R009, R010 integration readiness).

1. Run: `ls Assets/Scenes/`
2. **Expected:** Output includes `MainMenu.unity`, `MainMenu.unity.meta`, `Settings.unity`, `Settings.unity.meta`. Both `.unity` files are non-zero size (expected ~3539 bytes each).

---

### 6. Both scenes are registered and enabled in EditorBuildSettings

**What it proves:** `SceneManager.LoadSceneAsync` can find these scenes by name at runtime — the build settings entry is required for additive loading to work.

1. Open `ProjectSettings/EditorBuildSettings.asset`.
2. Locate the `m_Scenes` array.
3. **Expected:** Two entries present, both with `enabled: 1`, paths `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Settings.unity`, and non-empty `guid` values.

Quick verification command: `grep -c "MainMenu\|Settings" ProjectSettings/EditorBuildSettings.asset`
**Expected:** Count ≥ 2 (path entries for each scene).

---

### 7. ScreenManager core files compile cleanly (no UnityEngine, no static state)

**What it proves:** The core navigation files are pure C# and introduce no compilation errors.

1. Inspect `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — confirm:
   - Constructor signature: `public ScreenManager(ISceneLoader sceneLoader)`
   - `_isNavigating` bool field present
   - `_history` of type `Stack<ScreenId>` present
   - `CurrentScreen` property returns `ScreenId?`
   - `CanGoBack` property returns `bool`
   - No `static` fields (only methods/classes allowed)
2. **Expected:** All checks pass. Constructor injection pattern consistent with S01 UIFactory pattern.

---

## Edge Cases

### Back navigation from first screen (empty history)

**What it proves:** `GoBackAsync` with an empty history is a safe no-op — does not throw, does not unload the current screen.

1. In `TestResults.xml`, verify `GoBackAsync_WithEmptyHistory_IsNoOp` is present with `result="Passed"`.
2. **Expected:** Test passes. `ScreenManager.CanGoBack` returns `false` before any `ShowScreenAsync` call; `GoBackAsync` at this point performs no load or unload operations.

---

### Concurrent navigation guard

**What it proves:** Two simultaneous `ShowScreenAsync` calls do not corrupt navigation state.

1. In `TestResults.xml`, verify `ShowScreenAsync_GuardsAgainstConcurrentNavigation` is present with `result="Passed"`.
2. **Expected:** Test passes. The `_isNavigating` guard causes the second call to return immediately; only one scene load occurs.

---

### First show does not unload a non-existent previous screen

**What it proves:** The initial navigation (null current screen) does not attempt to unload a scene that was never loaded.

1. In `TestResults.xml`, verify `FirstShowScreen_DoesNotUnload` is present with `result="Passed"`.
2. **Expected:** Test passes. `MockSceneLoader.UnloadedScenes` contains zero entries after the first `ShowScreenAsync` call.

---

## Failure Signals

- `failed` attribute > 0 on `test-run` element in `TestResults.xml` — one or more navigation logic tests failed; read the `failure` element inside the failing `test-case` for the NUnit assertion message and `MockSceneLoader.CallLog` output
- `grep` returns output on static guard check — a static field was introduced; inspect the matching line and remove or convert to an instance field
- `grep` returns output on UnityEngine check — a `using UnityEngine` was added to a Core file; move the Unity-coupled code to the Runtime layer
- `Assets/Scenes/MainMenu.unity` or `Settings.unity` absent — `SceneSetup.CreateAndRegisterScenes` was not run or its output was not committed; re-run via batchmode `-executeMethod`
- `EditorBuildSettings.asset` has empty or missing `m_Scenes` array — scenes not registered; `SceneManager.LoadSceneAsync` will fail at runtime with "scene not found" error
- Scene files present but GUIDs missing from `EditorBuildSettings.asset` — Unity's `AssetDatabase.Refresh()` was not called after `SaveScene()`; re-run the SceneSetup method

## Requirements Proved By This UAT

- **R009** (hybrid scene management) — `UnitySceneLoader` uses `LoadSceneMode.Additive`; load/unload sequencing proven by `ShowScreenAsync_UnloadsPreviousBeforeLoadingNext` test
- **R010** (screen navigation) — `ShowScreenAsync` and `GoBackAsync` with history stack proven by 8 tests; `ScreenId` enum provides type-safe screen identification
- **R014** (UniTask) — `ISceneLoader` and `ScreenManager` use UniTask for async operations; `MockSceneLoader` returns `UniTask.CompletedTask` confirming interface contract
- **R015** (edit-mode tests) — 8 ScreenManagerTests join 6 MVPWiringTests; all 14 run without Unity runtime
- **R017** (each layer testable in isolation) — `MockSceneLoader` enables full ScreenManager testing with zero Unity dependency

## Not Proven By This UAT

- **Runtime scene loading** — `UnitySceneLoader` wraps Unity's `SceneManager` API but this path is not exercised by edit-mode tests. Real additive loading into a persistent scene is verified in S05's play-mode walkthrough.
- **Presenter lifecycle integration** — ScreenManager loads scenes but does not construct or wire presenters. The full dependency chain (boot → UIFactory → loaded view → presenter) is proven in S05.
- **Transition hooks** — No fade/animation callbacks exist yet. S04 adds transition support.
- **Input blocking during navigation** — InputBlocker does not exist yet (S03 deliverable).

## Notes for Tester

- `TestResults.xml` is the fastest signal — if all 14 pass, the slice is healthy. Inspecting individual test cases is only necessary when failures occur.
- The `MockSceneLoader.CallLog` entries appear in NUnit failure assertion messages as `[load:MainMenu, unload:MainMenu, load:Settings]` style output — this makes ordering bugs immediately visible without needing to re-run with extra logging.
- `EditorBuildSettings.asset` is a plain YAML file — open it in any text editor and search for `m_Scenes` to see the registration state at a glance.
- The scenes are intentionally empty (no camera, no lights). Opening `MainMenu.unity` or `Settings.unity` in the Unity editor will show a blank scene — this is expected. Content is added in S05.
- `ScreenManager.CanGoBack` is the signal for whether a back button should be enabled in the UI. This is ready to consume in S05 MainMenu and Settings presenters.
