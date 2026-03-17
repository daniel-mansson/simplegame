---
id: S03
milestone: M007
provides:
  - FindInScene<T>() helper using scene root convention
  - Zero FindFirstObjectByType in all production code
  - 169/169 tests pass
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
key_decisions:
  - "FindInScene<T>(ScreenId) queries scene.GetRootGameObjects() + GetComponent<T>() — scoped to target scene only"
patterns_established:
  - "Scene root convention: find components by querying loaded scene root GameObjects, not global scene search"
drill_down_paths:
  - .gsd/milestones/M007/slices/S03/S03-PLAN.md
verification_result: pass
completed_at: 2026-03-17T19:00:00Z
---

# S03: Scene Root Convention + Final Cleanup

**Scene root convention via FindInScene<T>(), zero FindFirstObjectByType in production, 169/169 pass**

## What Happened

Added `FindInScene<T>(ScreenId)` static helper to GameBootstrapper. It gets the scene by name via `SceneManager.GetSceneByName(screenId.ToString())`, iterates `scene.GetRootGameObjects()`, and calls `GetComponent<T>()` on each root until it finds the controller. This is scoped to the specific loaded scene — cleaner than `FindFirstObjectByType` which searches all loaded scenes globally.

Replaced all 3 remaining `FindFirstObjectByType` calls in the nav loop. Verified: `rg "FindFirstObjectByType" Assets/Scripts/` returns only a comment in the XML summary, zero actual calls.

169/169 tests pass. Milestone verification: zero FindObject* calls in production code.
