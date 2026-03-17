# S03 — Research

**Date:** 2026-03-17

## Summary

S03 replaces the 3 remaining `FindFirstObjectByType` calls in `GameBootstrapper.cs` (lines 99, 113, 126) with a scene root convention per D043. After `ShowScreenAsync` loads a screen scene additively, the bootstrapper already knows the scene name (it's `current.Value.ToString()` — the `ScreenId` enum name matches the Unity scene name exactly). Unity's `SceneManager.GetSceneByName(name).GetRootGameObjects()` returns the root objects of that loaded scene. Each scene has exactly one root `GameObject` carrying the scene controller component (e.g., `MainMenuSceneController` is on root GO `"MainMenuSceneController"` in `MainMenu.unity`). A simple `GetComponent<T>()` loop over root objects replaces the global scene scan.

This is a straightforward refactor — all 3 call sites share an identical pattern, no interfaces change, no new Core types are needed, and no tests reference `GameBootstrapper` directly so no test call sites need updating. The only production file that changes is `GameBootstrapper.cs`. The editor file `SceneSetup.cs` needs no changes (scene controllers are already created as root GameObjects). Final verification is `rg "FindFirstObjectByType" Assets/Scripts/` returning exit 1.

## Recommendation

Extract a private helper method `FindSceneController<T>(string sceneName)` in `GameBootstrapper` that:
1. Calls `UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName)`
2. Calls `scene.GetRootGameObjects()` to get the root `GameObject[]`
3. Iterates roots calling `root.GetComponent<T>()`, returns first non-null
4. Returns `null` if not found (caller already has the `LogError + return` guard)

Then replace each `FindFirstObjectByType<XyzSceneController>()` with `FindSceneController<XyzSceneController>(current.Value.ToString())`. The scene name comes from `current.Value` which is the `ScreenId` enum — its `ToString()` matches the scene file names exactly (already proven by `ShowScreenAsync` and `DetectAlreadyLoadedScreen`).

No changes to `ISceneLoader`, `ScreenManager`, or any Core types. The helper is private to `GameBootstrapper` because it's Game-layer logic (it uses concrete `ScreenId` → scene name mapping).

## Implementation Landscape

### Key Files

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — **only production file that changes**. Lines 99, 113, 126 have the 3 `FindFirstObjectByType` calls. Add a private `FindSceneController<T>(string sceneName)` helper and replace all 3 call sites. The file already uses fully-qualified `UnityEngine.SceneManagement.SceneManager` in `DetectAlreadyLoadedScreen()` — follow the same style or add a `using` at the top.
- `Assets/Scripts/Game/Boot/ISceneController.cs` — read-only reference. The interface has `RunAsync()` only. No changes needed.
- `Assets/Scripts/Core/ScreenManagement/ISceneLoader.cs` — read-only reference. No changes. The scene loader doesn't need to return root info because the bootstrapper can query `SceneManager` directly after `ShowScreenAsync` completes.
- `Assets/Editor/SceneSetup.cs` — **no changes needed**. Scene controllers are already created as root GameObjects (`new GameObject("MainMenuSceneController")` etc.), which is exactly what `scene.GetRootGameObjects()` will find.

### Scenes (read-only reference)

- `Assets/Scenes/MainMenu.unity` — root GO `"MainMenuSceneController"` with `MainMenuSceneController` component
- `Assets/Scenes/Settings.unity` — root GO `"SettingsSceneController"` with `SettingsSceneController` component
- `Assets/Scenes/InGame.unity` — root GO `"InGameSceneController"` with `InGameSceneController` component

All three follow the identical pattern: scene controller lives on a root GameObject.

### Test Files (read-only reference)

- `Assets/Tests/EditMode/Game/InGameTests.cs` — 19 tests; no reference to `GameBootstrapper`; no changes needed
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — 4 tests; no reference to `GameBootstrapper`; no changes needed
- No test file tests `GameBootstrapper` directly — the bootstrapper is integration-level (Boot scene) and not unit-tested. This means S03 production changes have zero test file impact.

### Build Order

**Single task** — this is a single-file production change:

1. Add `FindSceneController<T>(string sceneName)` private helper to `GameBootstrapper`
2. Replace all 3 `FindFirstObjectByType<T>()` calls with `FindSceneController<T>(current.Value.ToString())`
3. Verify: `rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/` returns exit 1
4. Verify: all 169 tests pass (Unity batchmode)

There's no need for multiple tasks — the change is ~15 lines of code in one file with a clear verification.

### Verification Approach

1. **Zero FindObject* in production code:**
   ```
   rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/
   ```
   Must return exit 1 (zero matches).

2. **Helper method exists:**
   ```
   rg "FindSceneController" Assets/Scripts/Game/Boot/GameBootstrapper.cs
   ```
   Must show the helper definition and 3 call sites.

3. **No FindObject* in entire Assets/ (not just Scripts/):**
   ```
   rg "FindFirstObjectByType|FindObjectOfType" Assets/ --include="*.cs"
   ```
   Must return exit 1 for all `.cs` files under Assets/ (including Editor/ and Tests/).

4. **Test suite (169 tests):**
   ```
   Unity batchmode -runTests -testPlatform EditMode
   ```
   All 169 tests pass. (Note K003: if editor hasn't domain-reloaded since S01/S02, the 5 new ViewContainerTests may not be detected until restart.)

5. **Human UAT:** Play through MainMenu → InGame → Win → MainMenu and InGame → Lose → Retry → Win to confirm identical flow.

## Constraints

- `SceneManager.GetSceneByName()` and `scene.GetRootGameObjects()` are Unity APIs available in Unity 6. Both are synchronous — safe to call immediately after `ShowScreenAsync` completes (the scene is fully loaded at that point).
- The helper must use `GetComponent<T>()` (not `GetComponentInChildren`) because the controller is directly on the root GO, and we only want to search root objects of the specific loaded scene — not the entire hierarchy.
- Must not use `FindFirstObjectByType` or any `FindObject*` variant — that's the whole point of this slice.
- The `DetectAlreadyLoadedScreen` method at the bottom of `GameBootstrapper` also uses `SceneManager` — it does NOT use `FindFirstObjectByType` and needs no changes.

## Common Pitfalls

- **`GetSceneByName` returns invalid scene if not loaded** — The bootstrapper calls this immediately after `await _screenManager.ShowScreenAsync(next)` which awaits the scene load. By the time control returns, the scene is guaranteed loaded. However, the helper should check `scene.IsValid()` as a safety guard.
- **Scene name must match ScreenId.ToString() exactly** — Already proven by `ShowScreenAsync` and `DetectAlreadyLoadedScreen`. The enum values `MainMenu`, `Settings`, `InGame` match scene file names `MainMenu.unity`, `Settings.unity`, `InGame.unity`.
- **`GetRootGameObjects()` allocates a new array each call** — Called once per screen transition (not per frame), so the allocation is negligible.
