---
id: T01
parent: S03
milestone: M007
provides:
  - FindSceneController<T> helper using scene root convention
  - Zero FindObject* variants in Assets/Scripts/
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
key_decisions:
  - Used fully-qualified UnityEngine.SceneManagement.SceneManager style (matches DetectAlreadyLoadedScreen)
  - GetComponent<T>() on root GOs only (not GetComponentInChildren) per scene root convention D043
patterns_established:
  - FindSceneController<T>(sceneName) private static helper pattern for scene controller lookup
observability_surfaces:
  - Debug.LogError("[GameBootstrapper] XyzSceneController not found in scene.") at each null-return call site
duration: 10m
verification_result: passed
completed_at: 2026-03-17
blocker_discovered: false
---

# T01: Replace FindFirstObjectByType with scene root convention in GameBootstrapper

**Replaced all 3 `FindFirstObjectByType` calls in `GameBootstrapper.cs` with a `FindSceneController<T>(sceneName)` private static helper using `SceneManager.GetSceneByName()` + `GetRootGameObjects()` + `GetComponent<T>()`, achieving zero `FindObject*` variants in `Assets/Scripts/`.**

## What Happened

Read `GameBootstrapper.cs` and confirmed the 3 `FindFirstObjectByType` calls at lines ~99, ~113, ~126 inside the `switch (current.Value)` block.

Applied two edits:
1. Replaced all 3 `FindFirstObjectByType<XyzSceneController>()` calls with `FindSceneController<XyzSceneController>(current.Value.ToString())` — preserving the existing null-check + `Debug.LogError` + `return` error handling at each site unchanged.
2. Added `private static T FindSceneController<T>(string sceneName) where T : Component` at the bottom of the class (after `DetectAlreadyLoadedScreen`), using fully-qualified `UnityEngine.SceneManagement.SceneManager` style consistent with the existing helper.

The helper implementation: `GetSceneByName(sceneName)` → `scene.IsValid()` safety guard → iterate `scene.GetRootGameObjects()` → `GetComponent<T>()` → return first match or null.

Pre-flight fixes were also applied: added `## Observability / Diagnostics` section to `S03-PLAN.md` and `## Observability Impact` section to `T01-PLAN.md`.

## Verification

- `rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/` → **exit 1** (zero matches) ✅
- `rg "FindSceneController" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → **4 matches** (1 definition + 3 call sites) ✅
- `rg "GetSceneByName|GetRootGameObjects|IsValid" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → **3 matches** confirming scene root convention APIs in use ✅
- `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` → **exit 1** (zero matches across entire Assets/ tree) ✅

## Diagnostics

To inspect the lookup at runtime: set a breakpoint or add a temporary `Debug.Log` inside `FindSceneController<T>` after `scene.GetRootGameObjects()` to verify the scene is valid and root objects are returned. On failure the existing `Debug.LogError("[GameBootstrapper] XyzSceneController not found in scene.")` fires immediately at each call site — no silent failures.

## Deviations

none

## Known Issues

none — Unity batchmode test run deferred to T02 (slice verification task).

## Files Created/Modified

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — 3 `FindFirstObjectByType` calls replaced with `FindSceneController<T>(current.Value.ToString())`; `FindSceneController<T>` private static helper added
- `.gsd/milestones/M007/slices/S03/S03-PLAN.md` — added `## Observability / Diagnostics` section (pre-flight fix)
- `.gsd/milestones/M007/slices/S03/tasks/T01-PLAN.md` — added `## Observability Impact` section (pre-flight fix)
