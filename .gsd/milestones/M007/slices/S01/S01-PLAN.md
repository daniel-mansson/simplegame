# S01: IViewResolver + Container Refactor

**Goal:** Introduce `IViewResolver` interface in Core and rename `UnityPopupContainer` → `UnityViewContainer`, implementing both `IPopupContainer<PopupId>` and `IViewResolver`. Container resolves view interfaces via `GetComponentInChildren<T>(true)` on its inactive children.
**Demo:** `rg "IViewResolver" Assets/Scripts/Core/` finds the interface; `rg "UnityPopupContainer" Assets/` returns zero; new `ViewContainerTests` prove `Get<T>()` resolves view interfaces; all 164+ existing tests pass.

## Must-Haves

- `IViewResolver` interface in `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` with `T Get<T>() where T : class`
- `UnityPopupContainer` renamed to `UnityViewContainer` via `git mv` (preserves .meta GUID)
- `UnityViewContainer` implements both `IPopupContainer<PopupId>` and `IViewResolver`
- `Get<T>()` implemented as `GetComponentInChildren<T>(true)`
- All references updated: `GameBootstrapper.cs`, `SceneSetup.cs`, test mocks
- Existing PopupId-based show/hide unchanged
- `ViewContainerTests` with assertions for `Get<T>()` resolution
- All 164+ existing tests still pass

## Proof Level

- This slice proves: contract
- Real runtime required: no
- Human/UAT required: no

## Verification

- `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/` → finds `IViewResolver.cs`
- `rg "UnityPopupContainer" Assets/` → zero matches (rename complete)
- `rg "class UnityViewContainer" Assets/Scripts/Game/Popup/` → finds the renamed class implementing both interfaces
- New test file `Assets/Tests/EditMode/Game/ViewContainerTests.cs` with ≥3 tests for `Get<T>()` + `MockViewResolver`
- All 164+ existing tests pass (no regressions from rename)

## Integration Closure

- Upstream surfaces consumed: none (first slice)
- New wiring introduced: `IViewResolver` interface in Core; `UnityViewContainer` in Game implementing it
- What remains: S02 wires scene controllers to use `IViewResolver.Get<T>()` instead of `FindFirstObjectByType`; S03 removes remaining `FindFirstObjectByType` calls and adds scene root convention

## Tasks

- [ ] **T01: Create IViewResolver, rename UnityPopupContainer → UnityViewContainer, implement Get<T>(), update all references** `est:30m`
  - Why: Core production change — creates the interface, renames the container, implements the new contract, and updates all files that reference the old name. Must be atomic (partial rename leaves codebase broken).
  - Files: `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` (new), `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` → `UnityViewContainer.cs`, `Assets/Scripts/Game/Boot/GameBootstrapper.cs`, `Assets/Editor/SceneSetup.cs`
  - Do: (1) Create `IViewResolver.cs` in Core with `T Get<T>() where T : class`. (2) `git mv` `UnityPopupContainer.cs` → `UnityViewContainer.cs` and its `.meta`. (3) Rename class to `UnityViewContainer`, add `IViewResolver` to implements list, implement `Get<T>()` as `return GetComponentInChildren<T>(true)`. (4) Update `GameBootstrapper.cs` — change type reference from `UnityPopupContainer` to `UnityViewContainer`. (5) Update `SceneSetup.cs` — change `AddComponent<UnityPopupContainer>()` to `AddComponent<UnityViewContainer>()` and field names if needed. (6) Verify no remaining `UnityPopupContainer` references via `rg`.
  - Verify: `rg "UnityPopupContainer" Assets/` returns zero; `rg "IViewResolver" Assets/Scripts/Core/` finds the interface; project compiles
  - Done when: `IViewResolver` exists in Core, container renamed and implements both interfaces, zero old-name references remain

- [ ] **T02: Add MockViewResolver + ViewContainerTests proving Get<T>() resolution** `est:25m`
  - Why: Proves the `IViewResolver` contract works — `Get<T>()` finds the correct component on inactive children. Also validates the rename didn't break any existing tests. The `MockViewResolver` test double is the boundary artifact S02 needs.
  - Files: `Assets/Tests/EditMode/Game/ViewContainerTests.cs` (new)
  - Do: (1) Create `ViewContainerTests.cs` with a `MockViewResolver` implementing `IViewResolver` via dictionary lookup. (2) Write tests: `Get_ReturnsCorrectInterface` — attach a mock MonoBehaviour implementing a view interface to a child GO, call `Get<T>()`, assert non-null and correct type. `Get_ReturnsNull_WhenNotFound` — call `Get<T>()` for an interface with no matching child. `Get_FindsInactiveChild` — child GO is inactive, `Get<T>()` still resolves. `MockViewResolver_ReturnsRegisteredView` — test the mock double itself. `MockViewResolver_ReturnsNull_WhenNotRegistered`. (3) All tests use `[Test]` attribute and NUnit assertions consistent with existing test patterns.
  - Verify: All new tests pass; all 164+ existing tests still pass (run full test suite)
  - Done when: `ViewContainerTests.cs` exists with ≥3 passing tests, `MockViewResolver` is available as test double, total test count ≥ 169

## Files Likely Touched

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` (new)
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` → `Assets/Scripts/Game/Popup/UnityViewContainer.cs` (rename + modify)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` (type reference update)
- `Assets/Editor/SceneSetup.cs` (type reference update)
- `Assets/Tests/EditMode/Game/ViewContainerTests.cs` (new)
