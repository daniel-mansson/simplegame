---
id: T01
parent: S02
milestone: M001
provides:
  - ScreenId enum with MainMenu and Settings values
  - ISceneLoader interface abstracting scene load/unload with UniTask
  - ScreenManager plain C# class with history stack, ShowScreenAsync, GoBackAsync, and navigation guard
  - UnitySceneLoader wrapping Unity SceneManager with additive mode
  - MockSceneLoader test double with ordered call log
  - 8 NUnit edit-mode tests proving all navigation logic
key_files:
  - Assets/Scripts/Core/ScreenManagement/ScreenId.cs
  - Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs
  - Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
  - Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs
  - Assets/Tests/EditMode/ScreenManagerTests.cs
key_decisions:
  - ToSceneName helper omitted; enum.ToString() used directly — avoids static method that fails static-state grep guard
  - ScreenId.cs contains only the enum (no helper class) to keep static guard clean
  - MockSceneLoader uses List<string> CallLog with "load:X"/"unload:X" entries for ordered verification
patterns_established:
  - ISceneLoader interface + MockSceneLoader test double follows MockSampleView pattern from S01
  - ScreenManager constructor injection with ISceneLoader enables pure C# test execution
  - UniTask.CompletedTask returned from MockSceneLoader — synchronous test execution without async infrastructure
observability_surfaces:
  - ScreenManager.CurrentScreen — inspectable property; null on construction, tracks active screen
  - ScreenManager.CanGoBack — inspectable property; true when history stack non-empty
  - MockSceneLoader.CallLog — ordered list of load/unload calls; test failures show exact sequence
  - TestResults.xml — 14 total tests (6 S01 + 8 S02), all passed, 0 failures
duration: ~25m
verification_result: passed
completed_at: 2026-03-15
blocker_discovered: false
---

# T01: Implement ScreenManager, ISceneLoader, and edit-mode tests

**Shipped ScreenManager navigation logic (additive load/unload, history stack, back navigation, concurrency guard) with ISceneLoader interface, UnitySceneLoader production implementation, and 8 edit-mode tests — all 14 tests (S01 + S02) passing.**

## What Happened

Created five files in sequence:

1. **`ScreenId.cs`** — enum with `MainMenu` and `Settings` values. The task plan called for a `ToSceneName(ScreenId)` static helper method, but this triggered the static-state grep guard (`public static string` not in the exclusion whitelist). Resolved by using `enum.ToString()` directly in `ScreenManager`, which produces identical output (`"MainMenu"`, `"Settings"`) since enum member names match scene names exactly.

2. **`ISceneLoader.cs`** — interface with `LoadSceneAdditiveAsync` and `UnloadSceneAsync` returning `UniTask`. Uses `Cysharp.Threading.Tasks` namespace, no `UnityEngine` dependency.

3. **`ScreenManager.cs`** — plain C# class. Constructor takes `ISceneLoader`. History stack (`Stack<ScreenId>`), current screen (`ScreenId?`), and `_isNavigating` bool guard. `ShowScreenAsync` checks guard, pushes current to history, unloads current (if any), loads new screen, updates `_currentScreen`. `GoBackAsync` pops history, unloads current, loads previous. All `finally` blocks ensure guard is always cleared. No static state.

4. **`UnitySceneLoader.cs`** — `SimpleGame.Runtime.ScreenManagement` namespace. Wraps `SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive)` and `SceneManager.UnloadSceneAsync(name)` using UniTask's `.ToUniTask(cancellationToken: ct)`.

5. **`ScreenManagerTests.cs`** — `MockSceneLoader` with `LoadedScenes`, `UnloadedScenes`, `List<string> CallLog` (ordered `"load:X"`/`"unload:X"` entries) returning `UniTask.CompletedTask`. `BlockingMockSceneLoader` auxiliary double for the concurrency guard test. 8 NUnit test methods covering all required scenarios.

Batchmode test run completed in ~11 seconds: 14/14 passed, 0 failures.

## Verification

- **Batchmode test run**: `Unity.exe -batchmode -projectPath C:\OtherWork\simplegame -runTests -testPlatform EditMode -testResults TestResults.xml` → exit code 0, `TestResults.xml` shows `result="Passed" total="14" passed="14" failed="0"`
- **Static guard**: `grep -r "static " --include="*.cs" Assets/Scripts/Core/ScreenManagement/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"` → no output (exit 1 = no matches)
- **No UnityEngine in Core**: `grep -n "using UnityEngine" Assets/Scripts/Core/ScreenManagement/*.cs` → no output
- All 8 ScreenManagerTests pass: `CanGoBack_ReflectsHistoryState`, `CurrentScreen_TracksActiveScreen`, `FirstShowScreen_DoesNotUnload`, `GoBackAsync_ReturnsToPreviousScreen`, `GoBackAsync_WithEmptyHistory_IsNoOp`, `ShowScreenAsync_GuardsAgainstConcurrentNavigation`, `ShowScreenAsync_LoadsCorrectScene`, `ShowScreenAsync_UnloadsPreviousBeforeLoadingNext`
- All 6 MVPWiringTests (S01) still pass

## Diagnostics

- **TestResults.xml** — at project root; inspect `result` attribute on `test-run` element for overall pass/fail; individual `test-case` elements for per-test status
- **MockSceneLoader.CallLog** — in tests, assertion messages include `[string.Join(", ", _loader.CallLog)]` so NUnit failure output shows the exact call sequence
- **ScreenManager.CurrentScreen** — publicly readable `ScreenId?`; null before first navigation
- **ScreenManager.CanGoBack** — publicly readable `bool`; use as a nav-button enabled signal

## Deviations

- **`ToSceneName` static helper omitted** — Task plan Step 1 described a `static string ToSceneName(ScreenId)` helper on the `ScreenId` type. This method's signature (`public static string`) is not in the static-state grep exclusion whitelist (`static void|static class|static readonly|static async|static UniTask`), causing a false positive on the static guard. Resolution: `enum.ToString()` called directly in `ScreenManager` — functionally identical since enum member names equal the scene names. The `ScreenId` enum must-have is fully satisfied; only the Step 1 helper was omitted.

## Known Issues

None.

## Files Created/Modified

- `Assets/Scripts/Core/ScreenManagement/ScreenId.cs` — created: `ScreenId` enum with `MainMenu` and `Settings`
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — created: `ISceneLoader` interface with `LoadSceneAdditiveAsync` and `UnloadSceneAsync`
- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — created: navigation logic, history stack, concurrency guard
- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — created: Unity SceneManager wrapper with additive loading
- `Assets/Tests/EditMode/ScreenManagerTests.cs` — created: `MockSceneLoader`, `BlockingMockSceneLoader`, 8 NUnit tests
- `.gsd/milestones/M001/slices/S02/tasks/T01-PLAN.md` — updated: added `## Observability Impact` section (pre-flight fix)
- `.gsd/milestones/M001/slices/S02/S02-PLAN.md` — updated: T01 marked `[x]`
