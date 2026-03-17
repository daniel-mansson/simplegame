# S03: Scene Root Convention + Final Cleanup

**Goal:** Replace the last 3 `FindFirstObjectByType` calls in `GameBootstrapper.cs` with a scene root convention, achieving zero `FindObject*` variants in all production code under `Assets/Scripts/`.
**Demo:** `rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/` returns exit 1 (zero matches). All 169+ edit-mode tests pass. Full game flow identical.

## Must-Haves

- Private `FindSceneController<T>(string sceneName)` helper in `GameBootstrapper` using `SceneManager.GetSceneByName()` + `scene.GetRootGameObjects()` + `GetComponent<T>()`
- All 3 `FindFirstObjectByType` calls replaced with `FindSceneController<T>(current.Value.ToString())`
- Zero `FindObject*` variants in any `.cs` file under `Assets/Scripts/`
- All 169+ edit-mode tests pass in Unity batchmode
- No test file changes required (no tests reference `GameBootstrapper` directly)

## Proof Level

- This slice proves: final-assembly
- Real runtime required: yes (human UAT play-through)
- Human/UAT required: yes (MainMenu → InGame → Win → MainMenu, InGame → Lose → Retry → Win)

## Verification

- `rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/` → exit 1
- `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` → exit 1 (entire Assets/ tree)
- `rg "FindSceneController" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → shows helper definition + 3 call sites (4 matches)
- `rg "GetSceneByName\|GetRootGameObjects\|IsValid" Assets/Scripts/Game/Boot/GameBootstrapper.cs` → confirms scene root convention APIs used
- Unity batchmode test run: all 169+ tests pass
- Human UAT: full game flow play-through confirms identical behavior

## Observability / Diagnostics

- **Runtime signals:** `GameBootstrapper` emits `Debug.Log("[GameBootstrapper] Boot sequence started.")` and `Debug.Log("[GameBootstrapper] Infrastructure ready. Starting navigation loop.")` at startup. Each `FindSceneController<T>` failure path emits `Debug.LogError("[GameBootstrapper] XyzSceneController not found in scene.")` — these are visible in the Unity Console and in batchmode logs.
- **Inspection surface:** After this slice, the scene root convention is the sole lookup path. To inspect the lookup: attach a debugger or add a temporary `Debug.Log` inside `FindSceneController<T>` after `scene.GetRootGameObjects()` to confirm the scene is valid and root objects are returned.
- **Failure visibility:** If `FindSceneController<T>` returns null (scene not loaded or controller missing), the existing `Debug.LogError` + `return` guard halts the navigation loop with a clear message identifying which controller was not found. No silent failures — every null result produces a logged error.
- **Redaction constraints:** No user data or secrets pass through these log paths; no redaction needed.
- **Verification diagnostic:** `rg "FindSceneController|GetSceneByName|GetRootGameObjects|IsValid" Assets/Scripts/Game/Boot/GameBootstrapper.cs` confirms the scene root convention APIs are present post-refactor.

## Integration Closure

- Upstream surfaces consumed: `GameBootstrapper.cs` with S02's `[SerializeField]` fields and `IViewResolver` passing already wired; `ScreenId` enum → scene name mapping (proven by `ShowScreenAsync` and `DetectAlreadyLoadedScreen`)
- New wiring introduced in this slice: `FindSceneController<T>()` private helper using Unity `SceneManager` API — no new interfaces, no new types, no changes to any file other than `GameBootstrapper.cs`
- What remains before the milestone is truly usable end-to-end: nothing — this is the final slice

## Tasks

- [x] **T01: Replace FindFirstObjectByType with scene root convention in GameBootstrapper** `est:20m`
  - Why: Eliminates the last 3 `FindFirstObjectByType` calls in all production code, completing R074 and R075
  - Files: `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
  - Do: Add a private `FindSceneController<T>(string sceneName)` helper that uses `SceneManager.GetSceneByName()` → `scene.IsValid()` guard → `scene.GetRootGameObjects()` → `GetComponent<T>()` loop. Replace all 3 `FindFirstObjectByType<XyzSceneController>()` calls with `FindSceneController<XyzSceneController>(current.Value.ToString())`. Follow existing fully-qualified `UnityEngine.SceneManagement.SceneManager` style or add a `using` at the top.
  - Verify: `rg "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/Scripts/` returns exit 1; `rg "FindSceneController" Assets/Scripts/Game/Boot/GameBootstrapper.cs` shows 4 matches (1 definition + 3 calls)
  - Done when: Zero `FindObject*` calls in `Assets/Scripts/`, helper method exists with `IsValid()` safety guard, and all 3 call sites use it

- [ ] **T02: Run full test suite and verify milestone completion** `est:15m`
  - Why: Gate for R076 (all 169+ tests pass) and milestone-level done criteria — confirms zero regressions from S01–S03 refactoring
  - Files: none (verification only)
  - Do: Run Unity batchmode edit-mode tests. Verify all 169+ tests pass. Run comprehensive `rg` checks across `Assets/` for any `FindObject*` variants. Confirm no `.cs` file under `Assets/Scripts/`, `Assets/Editor/`, or `Assets/Tests/` contains `FindFirstObjectByType`.
  - Verify: Unity batchmode test exit code 0, test count ≥ 169, zero `FindObject*` matches across entire `Assets/` tree
  - Done when: All tests pass, zero `FindObject*` in codebase, milestone verification complete

## Files Likely Touched

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
