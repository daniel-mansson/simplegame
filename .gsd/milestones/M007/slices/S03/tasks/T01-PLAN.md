---
estimated_steps: 5
estimated_files: 1
---

# T01: Replace FindFirstObjectByType with scene root convention in GameBootstrapper

**Slice:** S03 — Scene Root Convention + Final Cleanup
**Milestone:** M007

## Description

Replace the last 3 `FindFirstObjectByType` calls in `GameBootstrapper.cs` (lines 99, 113, 126) with a private `FindSceneController<T>(string sceneName)` helper method that uses Unity's scene root convention per D043. After `ShowScreenAsync` loads a screen scene additively, the bootstrapper queries the loaded scene's root GameObjects via `SceneManager.GetSceneByName()` + `scene.GetRootGameObjects()` + `GetComponent<T>()`, eliminating all global scene scanning.

This is the only production file that changes. No interfaces, types, or test files are affected.

**Relevant skill:** None required — this is a straightforward C# refactor.

## Steps

1. Open `Assets/Scripts/Game/Boot/GameBootstrapper.cs` and read the current file to confirm the 3 `FindFirstObjectByType` calls at lines 99, 113, 126 inside the `switch (current.Value)` block.

2. Add a private generic helper method `FindSceneController<T>(string sceneName) where T : Component` at the bottom of the class (before the closing brace, near `DetectAlreadyLoadedScreen`). Implementation:
   ```csharp
   private static T FindSceneController<T>(string sceneName) where T : Component
   {
       var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
       if (!scene.IsValid()) return null;
       foreach (var root in scene.GetRootGameObjects())
       {
           var controller = root.GetComponent<T>();
           if (controller != null) return controller;
       }
       return null;
   }
   ```
   Key constraints:
   - Use `GetComponent<T>()` (NOT `GetComponentInChildren`) — controllers are directly on root GOs
   - Include `scene.IsValid()` safety guard — returns null if scene not loaded
   - Use fully-qualified `UnityEngine.SceneManagement.SceneManager` (matches existing `DetectAlreadyLoadedScreen` style) or add `using UnityEngine.SceneManagement;` at the top
   - Make the method `private static` — it doesn't use instance state (same as `DetectAlreadyLoadedScreen`)

3. Replace the 3 `FindFirstObjectByType` calls:
   - Line ~99: `var ctrl = FindFirstObjectByType<MainMenuSceneController>();` → `var ctrl = FindSceneController<MainMenuSceneController>(current.Value.ToString());`
   - Line ~113: `var ctrl = FindFirstObjectByType<SettingsSceneController>();` → `var ctrl = FindSceneController<SettingsSceneController>(current.Value.ToString());`
   - Line ~126: `var ctrl = FindFirstObjectByType<InGameSceneController>();` → `var ctrl = FindSceneController<InGameSceneController>(current.Value.ToString());`
   
   The `current.Value` is the `ScreenId` enum. Its `.ToString()` produces `"MainMenu"`, `"Settings"`, `"InGame"` — which match the Unity scene file names exactly (already proven by `ShowScreenAsync` and `DetectAlreadyLoadedScreen`).

4. Verify no `FindObject*` variants remain in `Assets/Scripts/`:
   ```bash
   rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/
   ```
   Must return exit 1 (zero matches).

5. Verify the helper method and its 3 call sites exist:
   ```bash
   rg "FindSceneController" Assets/Scripts/Game/Boot/GameBootstrapper.cs
   ```
   Must show 4 matches (1 method definition + 3 call sites).

## Must-Haves

- [ ] Private `FindSceneController<T>(string sceneName)` helper method using `SceneManager.GetSceneByName()` + `GetRootGameObjects()` + `GetComponent<T>()`
- [ ] `scene.IsValid()` safety guard in the helper
- [ ] All 3 `FindFirstObjectByType` calls replaced with `FindSceneController<T>(current.Value.ToString())`
- [ ] Zero `FindObject*` variants in any `.cs` file under `Assets/Scripts/`
- [ ] Existing error handling (null check + `Debug.LogError` + return) preserved unchanged at each call site

## Verification

- `rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/` → exit 1 (zero matches)
- `rg "FindSceneController" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → 4 matches
- `rg "GetSceneByName\|GetRootGameObjects\|IsValid" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → confirms scene root convention APIs used
- `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType" Assets/` → exit 1 (entire Assets/ tree including Editor/ and Tests/)

## Inputs

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — current file with 3 `FindFirstObjectByType` calls at lines 99, 113, 126 in the `switch (current.Value)` block. Has a `DetectAlreadyLoadedScreen` method at the bottom that already uses `UnityEngine.SceneManagement.SceneManager` — follow the same fully-qualified style.
- D043 decision: scene root convention — query loaded scene's root GameObjects, not FindFirstObjectByType.
- Scene structure: each screen scene has exactly one root GameObject carrying the scene controller component (`MainMenuSceneController`, `SettingsSceneController`, `InGameSceneController`).
- `ScreenId` enum values `MainMenu`, `Settings`, `InGame` match scene file names exactly.

## Expected Output

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — modified: new `FindSceneController<T>` helper method added; 3 `FindFirstObjectByType` calls replaced; zero `FindObject*` variants remaining in entire file
