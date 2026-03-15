---
id: S02
parent: M001
milestone: M001
provides:
  - ScreenId enum with MainMenu and Settings values
  - ISceneLoader interface abstracting scene load/unload with UniTask
  - ScreenManager plain C# class with history stack, ShowScreenAsync, GoBackAsync, and navigation guard
  - UnitySceneLoader wrapping Unity SceneManager with additive mode
  - Assets/Scenes/MainMenu.unity — placeholder scene registered in EditorBuildSettings
  - Assets/Scenes/Settings.unity — placeholder scene registered in EditorBuildSettings
  - EditorBuildSettings.scenes populated with both scenes (enabled)
requires:
  - slice: S01
    provides: IView, Presenter<TView>, UniTask, SimpleGame.Runtime.asmdef, SimpleGame.Tests.EditMode.asmdef
affects:
  - S04 (ScreenManager lifecycle hooks consumed by TransitionManager)
  - S05 (ScreenManager wired into UIFactory boot flow)
key_files:
  - Assets/Scripts/Core/ScreenManagement/ScreenId.cs
  - Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs
  - Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
  - Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs
  - Assets/Tests/EditMode/ScreenManagerTests.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Scenes/MainMenu.unity
  - Assets/Scenes/Settings.unity
  - ProjectSettings/EditorBuildSettings.asset
key_decisions:
  - ScreenManager does not own presenter lifecycle — only scene load/unload; presenter wiring deferred to S05
  - ToSceneName static helper omitted; enum.ToString() used directly in ScreenManager to avoid false positive on static-state grep guard
  - SceneSetup.cs placed in Assets/Editor/ with no asmdef — Unity auto-compiles as editor-only via Assembly-CSharp-Editor
  - NewSceneSetup.EmptyScene + NewSceneMode.Single for placeholder scenes — no default camera/light
patterns_established:
  - ISceneLoader interface + MockSceneLoader test double mirrors MockSampleView pattern from S01 — pure C# double, no Unity runtime
  - MockSceneLoader.CallLog ordered list ("load:X"/"unload:X") enables exact call-sequence verification in NUnit failures
  - Editor batchmode -executeMethod pattern for asset creation: static class in Assets/Editor/, ClassName.MethodName
observability_surfaces:
  - ScreenManager.CurrentScreen — null before first navigation, tracks active ScreenId thereafter
  - ScreenManager.CanGoBack — true when history stack non-empty; suitable as nav-button enabled signal
  - MockSceneLoader.CallLog — ordered call sequence; NUnit failure messages include full log
  - TestResults.xml — 14 total (6 S01 + 8 S02), all passed, 0 failures
  - ProjectSettings/EditorBuildSettings.asset — m_Scenes array; grep for MainMenu/Settings confirms registration
drill_down_paths:
  - .gsd/milestones/M001/slices/S02/tasks/T01-SUMMARY.md
  - .gsd/milestones/M001/slices/S02/tasks/T02-SUMMARY.md
duration: ~35m
verification_result: passed
completed_at: 2026-03-15
---

# S02: Screen Management

**Shipped ScreenManager navigation logic with additive scene load/unload, history stack, and concurrency guard — proven by 8 passing edit-mode tests — plus MainMenu and Settings placeholder scenes registered in EditorBuildSettings.**

## What Happened

S02 delivered two focused tasks that together satisfy R009 (hybrid scene management) and R010 (screen navigation) at the logic-proof level.

**T01 — Navigation logic and tests:** Five files were created in sequence. `ScreenId.cs` defines the `MainMenu` and `Settings` enum values that serve as the type-safe screen identifier throughout the system. `ISceneLoader.cs` defines the abstraction that makes ScreenManager fully testable in edit-mode — `LoadSceneAdditiveAsync` and `UnloadSceneAsync` returning `UniTask`, with no `UnityEngine` coupling in the core interface. `ScreenManager.cs` is a plain C# class constructed with an `ISceneLoader`. It holds a `Stack<ScreenId>` for back navigation, a `ScreenId?` for the current screen, and a bool guard (`_isNavigating`) that prevents concurrent navigation calls from corrupting state. `ShowScreenAsync` unloads the current scene (if any) before loading the new one; `GoBackAsync` pops the history stack. `finally` blocks ensure the guard is always cleared regardless of whether the async operation succeeds or throws. `UnitySceneLoader.cs` in the runtime namespace wraps `SceneManager.LoadSceneAsync` (additive mode) and `SceneManager.UnloadSceneAsync` via UniTask's `.ToUniTask()`.

`ScreenManagerTests.cs` introduced `MockSceneLoader` (with an ordered `CallLog`, `LoadedScenes`, and `UnloadedScenes` sets) and `BlockingMockSceneLoader` (for the concurrency guard test). Eight NUnit test methods cover every required scenario. Batchmode test run: 14/14 passed (6 S01 + 8 S02), 0 failures, in ~11 seconds.

One deviation from the task plan: the `ToSceneName` static helper described in the plan was omitted. The static-state grep guard whitelist (`static void|static class|static readonly|static async|static UniTask`) does not include `static string`, so a `public static string ToSceneName(ScreenId)` method would produce a false positive. `enum.ToString()` in ScreenManager produces identical output — enum member names equal scene names — so no functionality was lost.

**T02 — Placeholder scenes and build settings:** `Assets/Editor/SceneSetup.cs` was created as a static class callable via Unity batchmode `-executeMethod SceneSetup.CreateAndRegisterScenes`. The method creates `Assets/Scenes/` if absent, uses `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)` + `SaveScene()` for each scene, then assigns both entries (enabled) to `EditorBuildSettings.scenes`. Running via batchmode produced both `.unity` files (3539 bytes each, valid YAML) and populated `EditorBuildSettings.asset`'s `m_Scenes` array with correct GUIDs and `enabled: 1`. A second batchmode compile pass and a full test run confirmed no regressions.

## Verification

- **Edit-mode tests**: `TestResults.xml` — `result="Passed" total="14" passed="14" failed="0"` — 8 ScreenManagerTests + 6 MVPWiringTests
- **Static guard**: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ScreenManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output (exit 1, no matches)
- **Compile clean**: batchmode compile exit 0, zero `error CS`
- **Scenes on disk**: `Assets/Scenes/MainMenu.unity` and `Assets/Scenes/Settings.unity` both present (3539 bytes each)
- **EditorBuildSettings**: `m_Scenes` contains both entries with `enabled: 1` and valid GUIDs
- **No UnityEngine in Core**: `grep -n "using UnityEngine" Assets/Scripts/Core/ScreenManagement/*.cs` → no output

## Requirements Advanced

- **R009** (hybrid scene management) — `UnitySceneLoader` wraps `SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive)` and `SceneManager.UnloadSceneAsync(name)`; the load/unload sequencing pattern is proven and ready to connect to a persistent scene host in S05
- **R010** (screen navigation) — `ScreenManager.ShowScreenAsync` and `GoBackAsync` implement forward and back navigation with history stack; `ScreenId` enum provides type-safe screen identification; 8 tests prove the navigation contract
- **R014** (UniTask async/await) — `ISceneLoader`, `ScreenManager`, and `UnitySceneLoader` all use `UniTask` for async operations; `MockSceneLoader` returns `UniTask.CompletedTask` for synchronous test execution
- **R015** (edit-mode unit tests) — 8 new ScreenManager tests join the 6 S01 tests; all run in pure C# without Unity runtime
- **R017** (each layer testable in isolation) — `MockSceneLoader` and `BlockingMockSceneLoader` test doubles enable full ScreenManager testing with no Unity dependency

## Requirements Validated

- None newly validated in this slice (R009 and R010 advance to "proven at logic level" but runtime integration proof deferred to S05 play-mode walkthrough)

## New Requirements Surfaced

- None

## Requirements Invalidated or Re-scoped

- None

## Deviations

- **`ToSceneName` static helper omitted** — Task plan Step 1 described a `static string ToSceneName(ScreenId)` helper on the `ScreenId` type. The static-state grep guard whitelist does not cover `static string`, causing a false positive. Resolution: `screenId.ToString()` called directly in `ScreenManager`. Functionally identical since enum member names equal scene names. If scene names ever diverge from enum names, a non-static lookup inside a non-static class should replace this. Documented as Decision #11.

## Known Limitations

- **No presenter lifecycle integration** — ScreenManager loads and unloads scenes but has no knowledge of presenters or views. The connection (UIFactory → loaded view → presenter) is deferred to S05. Until then, loading a screen scene shows a blank scene with no behavior.
- **No transition hooks** — ScreenManager's `ShowScreenAsync` and `GoBackAsync` do not yet invoke any fade/transition callbacks. The S04 TransitionManager will wrap or hook into these methods.
- **Play-mode walkthrough deferred to S05** — Integration of ScreenManager with boot scene, view instances, and real presenter wiring is the S05 deliverable. The R009/R010 runtime proof is incomplete until then.

## Follow-ups

- **S04**: Hook transition callbacks into `ShowScreenAsync` / `GoBackAsync` (fade out before unload, fade in after load)
- **S05**: Wire `ScreenManager` into UIFactory/boot flow; connect presenter construction to loaded screen view instances; implement persistent scene as ScreenManager host

## Files Created/Modified

- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — created: `ScreenId` enum with `MainMenu` and `Settings` values
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — created: `ISceneLoader` interface with `LoadSceneAdditiveAsync` and `UnloadSceneAsync`
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — created: navigation logic, history stack, concurrency guard, `CurrentScreen`/`CanGoBack` properties
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — created: Unity SceneManager wrapper with additive loading via UniTask
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — created: `MockSceneLoader`, `BlockingMockSceneLoader`, 8 NUnit test methods
- `Assets/Editor/SceneSetup.cs` — created: editor-only batchmode-callable scene creator/registrar
- `Assets/Scenes/MainMenu.unity` — created: empty placeholder scene for MainMenu screen
- `Assets/Scenes/Settings.unity` — created: empty placeholder scene for Settings screen
- `ProjectSettings/EditorBuildSettings.asset` — modified: `m_Scenes` populated with MainMenu and Settings entries (enabled: 1)
- `.gsd/milestones/M001/slices/S02/S02-PLAN.md` — updated: T01 and T02 marked `[x]`

## Forward Intelligence

### What the next slice should know
- **ScreenManager is a navigation primitive only** — it does not create, initialize, or dispose presenters. S05 must wire UIFactory into the boot scene so that after a scene loads, the factory finds the scene's view MonoBehaviour and constructs the matching presenter.
- **`ScreenId.ToString()` is the scene name** — enum member names (`"MainMenu"`, `"Settings"`) must exactly match scene file names. If a new screen's name cannot match its enum member name, introduce a non-static mapping dictionary (not a static helper method) inside `ScreenManager` or a new non-static `ScreenRegistry` class.
- **S04 transition hooks**: `ShowScreenAsync` currently does load/unload in sequence with no animation step. The natural integration point is to `await` a transition's fade-out before the unload call, and `await` the fade-in after the load call — either by adding optional `ITransitionProvider` injection to `ScreenManager` or by wrapping `ShowScreenAsync` in a higher-level navigation service in S05.
- **`CanGoBack`** is ready to drive a back-button's enabled state — use it directly in the MainMenu/Settings presenters built in S05.

### What's fragile
- **`enum.ToString()` as scene name mapping** — works as long as enum member names stay in sync with Unity scene file names. A rename in either place (without updating the other) silently breaks runtime scene loading with no compile error. The S05 boot scene integration test is the only current safety net.
- **`BlockingMockSceneLoader` in tests** — the concurrency guard test uses a `TaskCompletionSource`-based blocking loader. It relies on synchronous task scheduling behavior in the NUnit edit-mode runner. If Unity's test runner threading changes, this test may behave differently.

### Authoritative diagnostics
- **`TestResults.xml`** at project root — first-look signal for any test regression; check `result` attribute on `test-run` element
- **`MockSceneLoader.CallLog`** — NUnit failure messages include the full ordered call sequence as part of assertion messages; this is the fastest way to diagnose load/unload ordering bugs
- **`ProjectSettings/EditorBuildSettings.asset`** — `m_Scenes` array; `grep "MainMenu\|Settings"` to confirm scene registration is intact after any project-settings change

### What assumptions changed
- **"ScreenId should have a ToSceneName helper"** — Original plan assumed a static helper was the right approach. The static-state grep guard revealed this would produce false positives. The actual pattern (`enum.ToString()` with matching names) is simpler and requires no extra method.
