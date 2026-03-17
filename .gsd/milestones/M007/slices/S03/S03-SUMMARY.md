---
id: S03
parent: M007
milestone: M007
provides:
  - FindSceneController<T>(sceneName) private static helper in GameBootstrapper — scene root convention
  - Zero FindObject* variants anywhere in Assets/ (entire codebase)
  - 169/169 EditMode tests passing — M007 test gate satisfied
  - R074, R075, R076 validated
requires:
  - slice: S01
    provides: IViewResolver interface + UnityViewContainer implementing it
  - slice: S02
    provides: GameBootstrapper SerializeField refs for boot infrastructure; FindFirstObjectByType removed from scene controllers and boot infrastructure
affects: []
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
key_decisions:
  - Used fully-qualified UnityEngine.SceneManagement.SceneManager style (matches DetectAlreadyLoadedScreen)
  - GetComponent<T>() on root GameObjects only (not GetComponentInChildren) per scene root convention D043
  - IsValid() guard before GetRootGameObjects() — scene not yet loaded returns null cleanly
patterns_established:
  - FindSceneController<T>(sceneName) private static helper pattern for post-load scene controller discovery
observability_surfaces:
  - Debug.LogError("[GameBootstrapper] XyzSceneController not found in scene.") at each of 3 call sites
  - rg "FindSceneController|GetSceneByName|GetRootGameObjects" Assets/Scripts/Game/Boot/GameBootstrapper.cs → 5 lines
drill_down_paths:
  - .gsd/milestones/M007/slices/S03/tasks/T01-SUMMARY.md
  - .gsd/milestones/M007/slices/S03/tasks/T02-SUMMARY.md
duration: 25m
verification_result: passed
completed_at: 2026-03-17
---

# S03: Scene Root Convention + Final Cleanup

**Zero `FindObject*` variants remain in the entire codebase; 169/169 EditMode tests pass; GameBootstrapper finds scene controllers via `FindSceneController<T>()` using the scene root convention — M007 structural refactor fully mechanically complete.**

## What Happened

S03 had a single implementation task (T01) and a verification task (T02).

**T01** replaced the last 3 `FindFirstObjectByType` calls in the codebase — all in `GameBootstrapper.cs` inside the `switch (current.Value)` navigation loop. Each case (`MainMenu`, `Settings`, `InGame`) previously called `FindFirstObjectByType<XyzSceneController>()`. These were replaced with `FindSceneController<XyzSceneController>(current.Value.ToString())`, using the `ScreenId` enum's `ToString()` value as the scene name (the same convention used by `DetectAlreadyLoadedScreen` and the existing scene loader).

The `FindSceneController<T>(string sceneName)` private static helper was added at the bottom of the class, after `DetectAlreadyLoadedScreen`:
1. `SceneManager.GetSceneByName(sceneName)` → Scene struct
2. `scene.IsValid()` guard — returns null if the scene is not yet loaded (prevents a crash on bad state)
3. Iterate `scene.GetRootGameObjects()` — only root-level GameObjects, not recursive
4. `root.GetComponent<T>()` — returns the first match

This is a single-file change. No new types, no new interfaces, no test changes.

**T02** ran all verification checks:
- `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` → exit 1 (zero matches across entire Assets/ tree)
- Unity EditMode test run via stdin pipe workaround (K006): 169/169 passed, 0 failed
- `rg "FindSceneController|GetSceneByName|GetRootGameObjects" GameBootstrapper.cs` → 5 matches confirmed

One operational note: the `mcporter call unityMCP.run_tests testMode:EditMode` CLI command consistently crashes on Windows with a UV_HANDLE_CLOSING assertion (K006). The stdin pipe workaround (`echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin`) successfully initiates the job. Polling with `mcporter call unityMCP.get_test_job job_id=<id>` works normally.

## Verification

| Check | Result |
|---|---|
| `rg -g "*.cs" "FindFirstObjectByType\|..." Assets/` | ✅ exit 1 (zero matches) |
| `rg "FindSceneController" GameBootstrapper.cs` | ✅ 4 matches (1 def + 3 call sites) |
| `rg "GetSceneByName\|GetRootGameObjects\|IsValid" GameBootstrapper.cs` | ✅ 3 matches confirmed |
| Unity EditMode tests (169 total) | ✅ 169/169 passed, 0 failed |
| R074: scene root convention in place | ✅ validated |
| R075: zero FindObject* in Assets/ | ✅ validated |
| R076: 169+ tests pass | ✅ validated |
| R077: Human UAT | ⚠️ pending — not yet performed |

## Requirements Advanced

- R074 — `FindSceneController<T>()` helper using scene root convention now in place in `GameBootstrapper.cs`
- R075 — Zero FindObject* variants confirmed across entire Assets/ tree (all .cs files)
- R076 — 169/169 EditMode tests pass — confirmed by live editor run

## Requirements Validated

- **R074** — `FindSceneController<T>(sceneName)` private static helper confirmed at line 158 of GameBootstrapper.cs with 3 call sites. Grep evidence on file.
- **R075** — `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` returns exit 1. Complete codebase is clean.
- **R076** — Unity MCP test job 766d31f2ff0b434eaad592ac6a7a8796: total=169, passed=169, failed=0. All new tests from S01/S02 included.

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

none — the plan was executed exactly as written. Single-file edit, two verification tasks, zero test changes needed.

## Known Limitations

- **R077 (human UAT) is pending.** The milestone requires a full play-through of MainMenu → InGame → Win → MainMenu and InGame → Lose → Retry → Win to confirm no behavioral regressions. All mechanical evidence (zero FindObject*, 169 tests passing) supports correctness, but UAT must still be performed by a human in the Unity editor before M007 is fully closed.
- **mcporter `run_tests` CLI crashes on Windows** (K006). The stdin pipe workaround is the only reliable path. This affects any future agent that needs to run tests via Unity MCP.

## Follow-ups

- Human UAT play-through before closing M007: MainMenu → InGame → Win → MainMenu, then InGame → Lose → Retry → Win.
- D047 (`IViewResolver` parameter on `Initialize()` is currently optional with `= null` default) — can be made required now that all production call sites pass a resolver. Low priority but clean-up opportunity.

## Files Created/Modified

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — 3 `FindFirstObjectByType` calls replaced with `FindSceneController<T>(current.Value.ToString())`; `FindSceneController<T>` private static helper added after `DetectAlreadyLoadedScreen`

## Forward Intelligence

### What the next slice should know
- M007 is mechanically complete. The only open item is human UAT (R077). Once UAT passes, the milestone is done — no code changes expected.
- The scene root convention (`FindSceneController<T>`) is now the established pattern for scene controller discovery. If a new scene type is added, add a new `case` in `GameBootstrapper`'s switch block and call `FindSceneController<NewSceneController>(current.Value.ToString())`.
- `ScreenId.ToString()` == scene name is the stable mapping. As long as enum values match scene names in EditorBuildSettings, the lookup works. Decision D011 (enum.ToString() used directly) captures this.
- The stdin pipe workaround for `run_tests` (K006) is now documented. Future agents must use `echo '{"testMode":"EditMode"}' | mcporter call unityMCP.run_tests --stdin` on Windows.

### What's fragile
- `FindSceneController<T>` silently returns null if the scene name doesn't match. The `Debug.LogError` at each call site catches it at runtime, but typos in `ScreenId` enum values would break the lookup silently at compile time. Enforced by convention, not compile-time check.
- The test count (169) is locked to the live editor's compiled state. If domain reload is not triggered after adding new test files, the count can lag (K003). Always run tests in the live editor, not from stale batchmode XML.

### Authoritative diagnostics
- `rg -g "*.cs" "FindFirstObjectByType|FindObjectOfType|FindObjectsOfType|FindAnyObjectByType" Assets/` → the canonical zero-match check. Run this first for any "did M007 regress?" question.
- Unity MCP test job polling: `mcporter call unityMCP.get_test_job job_id=<id>` — check `result.summary.total == 169` and `result.summary.passed == 169`.
- Runtime failure signal: Unity Console shows `[GameBootstrapper] XyzSceneController not found in scene.` — this means the scene wasn't loaded before the controller lookup.

### What assumptions changed
- The test count grew from 164+ (milestone plan) to 169 (actual). S01 and S02 added new test files that the original estimate didn't anticipate.
- The batchmode TestResults.xml was stale (49 tests from pre-S01/S02 compiled assemblies, K003). Live editor run is authoritative.
