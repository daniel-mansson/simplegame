# S01: IViewResolver + Container Refactor

**Goal:** Create IViewResolver interface in Core, rename UnityPopupContainer to UnityViewContainer, implement Get<T>() via GetComponentInChildren, and verify show/hide still works.
**Demo:** Renamed container in Boot resolves all 6 popup view interfaces via Get<IMyView>(), show/hide by PopupId still works, verified by new and existing tests.

## Must-Haves

- `IViewResolver` interface exists in `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` with `T Get<T>() where T : class`
- `UnityPopupContainer.cs` renamed to `UnityViewContainer.cs` via `git mv` (preserves GUID)
- `UnityViewContainer` implements both `IPopupContainer<PopupId>` and `IViewResolver`
- `Get<T>()` resolves view interfaces from inactive children via `GetComponentInChildren<T>(true)`
- All references updated (GameBootstrapper, SceneSetup, tests)
- New tests prove Get<T>() resolution works
- All existing 164+ tests pass

## Verification

- `rg "UnityPopupContainer" Assets/Scripts/ Assets/Tests/` returns zero results (fully renamed)
- `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/` returns the interface file
- New ViewContainer tests pass (Get<T>() resolution, null for missing type)
- All existing tests pass in Unity batchmode

## Tasks

- [x] **T01: IViewResolver interface + UnityViewContainer rename and implementation** `est:45m`
  - Why: Core deliverable of S01 — creates the interface and refactors the container
  - Files: `Assets/Scripts/Core/PopupManagement/IViewResolver.cs`, `Assets/Scripts/Game/Popup/UnityViewContainer.cs`, `Assets/Scripts/Game/Boot/GameBootstrapper.cs`, `Assets/Editor/SceneSetup.cs`
  - Do: Create IViewResolver in Core. git mv UnityPopupContainer → UnityViewContainer. Add IViewResolver implementation with GetComponentInChildren<T>(true). Update GameBootstrapper and SceneSetup references. Remove per-popup SerializeField refs from container (children are found dynamically). Keep PopupId-based show/hide working via transform.Find or child component lookup.
  - Verify: Project compiles, `rg "UnityPopupContainer" Assets/Scripts/` returns zero
  - Done when: IViewResolver exists, container renamed and implements both interfaces, all references updated

- [x] **T02: Tests + full verification** `est:30m`
  - Why: Proves Get<T>() resolution works and existing tests still pass
  - Files: `Assets/Tests/EditMode/Core/PopupManagerTests.cs`, `Assets/Tests/EditMode/Game/SceneControllerTests.cs`, new test file for ViewContainer
  - Do: Add MockViewResolver implementing IViewResolver for test doubles. Write tests proving Get<T>() returns correct view, returns null for unregistered type. Run full test suite in Unity batchmode. Fix any breakage from the rename.
  - Verify: Unity batchmode test run passes all tests including new ones
  - Done when: All tests pass, Get<T>() resolution proven by tests

## Files Likely Touched

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` (new)
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` (renamed from UnityPopupContainer.cs)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
