# S03: Scene Root Convention + Final Cleanup

**Goal:** Eliminate the last 3 FindFirstObjectByType calls (scene controller discovery) via scene root convention. Zero FindFirstObjectByType in production code.
**Demo:** `rg "FindFirstObjectByType|FindObjectOfType" Assets/Scripts/` returns zero. All 169+ tests pass. Full game flow works.

## Must-Haves

- GameBootstrapper finds scene controllers by querying loaded scene's root GameObjects
- Zero FindFirstObjectByType in any file under Assets/Scripts/
- All 169+ tests pass
- Full game loop works identically

## Verification

- `rg "FindFirstObjectByType\|FindObjectOfType\|FindObjectsOfType\|FindAnyObjectByType" Assets/Scripts/` returns zero
- Unity batchmode test run passes all tests

## Tasks

- [x] **T01: Scene root convention for controller discovery + final verification** `est:45m`
  - Why: Eliminates the last 3 FindFirstObjectByType calls in production code
  - Files: `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
  - Do: After scene load in nav loop, get the loaded scene by name, iterate its root GameObjects, find the one with the controller component via GetComponent<T>(). Replace all 3 FindFirstObjectByType<XxxSceneController>() calls. Run full test suite + grep verification.
  - Verify: `rg "FindFirstObjectByType" Assets/Scripts/` returns zero; all tests pass
  - Done when: Zero FindObject calls in production, all tests pass

## Files Likely Touched

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
