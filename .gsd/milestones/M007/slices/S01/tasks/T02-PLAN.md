---
estimated_steps: 3
estimated_files: 1
---

# T02: Add MockViewResolver + ViewContainerTests proving Get<T>() resolution

**Slice:** S01 — IViewResolver + Container Refactor
**Milestone:** M007

## Description

Proves the `IViewResolver` contract with focused tests. Creates `ViewContainerTests.cs` containing both a `MockViewResolver` test double (dictionary-based, for use by S02 executor agents) and tests that exercise the real `UnityViewContainer.Get<T>()` resolution on inactive children. Also serves as regression gate — all 164+ existing tests must still pass after the T01 rename.

The test pattern follows existing conventions in the codebase (NUnit `[Test]` attributes, `[SetUp]` for test fixtures, `new GameObject()` for Unity test objects).

## Steps

1. **Create `Assets/Tests/EditMode/Game/ViewContainerTests.cs`** with:

   **MockViewResolver** — a plain C# test double implementing `IViewResolver`:
   ```csharp
   internal class MockViewResolver : IViewResolver
   {
       private readonly Dictionary<System.Type, object> _views = new();
       public void Register<T>(T view) where T : class => _views[typeof(T)] = view;
       public T Get<T>() where T : class =>
           _views.TryGetValue(typeof(T), out var v) ? (T)v : null;
   }
   ```

   **Tests for UnityViewContainer.Get<T>():**
   - `Get_ReturnsCorrectInterface`: Create a parent GO, add `UnityViewContainer`. Create a child GO with a MonoBehaviour implementing a view interface (e.g. use one of the existing popup view classes, or create a minimal `TestView : MonoBehaviour, ITestView` inside the test file). Call `container.Get<ITestView>()`, assert it returns the child's component.
   - `Get_ReturnsNull_WhenInterfaceNotFound`: Call `Get<T>()` for an interface no child implements → returns null.
   - `Get_FindsInactiveChild`: Child GO is `SetActive(false)`, call `Get<T>()` → still resolves (this is the key behavior — `GetComponentInChildren<T>(true)` finds inactive objects).

   **Tests for MockViewResolver:**
   - `MockViewResolver_ReturnsRegistered`: Register a view, get it back.
   - `MockViewResolver_ReturnsNull_WhenNotRegistered`: Get unregistered type → null.

   Use NUnit `[Test]` and `[SetUp]`/`[TearDown]` with `Object.DestroyImmediate()` cleanup for any GameObjects created. Follow the same patterns as `Assets/Tests/EditMode/Game/PopupTests.cs` and `Assets/Tests/EditMode/Core/PopupManagerTests.cs`.

   The test file must reference `SimpleGame.Core.PopupManagement` (for `IViewResolver`) and `SimpleGame.Game.Popup` (for `UnityViewContainer`). Check the existing test `.asmdef` at `Assets/Tests/EditMode/Game/` to confirm these assemblies are already referenced.

2. **Verify all new tests pass** — look for exactly 5 new `[Test]` methods (total should be 169+).

3. **Run full test verification**: confirm zero regressions from the T01 rename by checking all 164+ existing tests still pass alongside the new ones.

## Must-Haves

- [ ] `ViewContainerTests.cs` exists at `Assets/Tests/EditMode/Game/ViewContainerTests.cs`
- [ ] `MockViewResolver` class implements `IViewResolver` with `Register<T>()` and `Get<T>()`
- [ ] At least 3 tests for `UnityViewContainer.Get<T>()` covering: correct resolution, null for missing, inactive child resolution
- [ ] At least 2 tests for `MockViewResolver` covering: registered returns correct, unregistered returns null
- [ ] All tests pass (169+ total)

## Verification

- `rg "\[Test\]" Assets/Tests/EditMode/Game/ViewContainerTests.cs` → 5 test methods
- `rg "MockViewResolver" Assets/Tests/EditMode/Game/ViewContainerTests.cs` → class exists
- All 169+ tests pass when run (existing 164 + 5 new)

## Inputs

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — the interface to test (created in T01)
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — the container to test (renamed in T01)
- `Assets/Tests/EditMode/Game/PopupTests.cs` — existing test patterns to follow
- `Assets/Tests/EditMode/Core/PopupManagerTests.cs` — existing mock patterns to follow

## Expected Output

- `Assets/Tests/EditMode/Game/ViewContainerTests.cs` — new test file with MockViewResolver + 5 tests
- All 169+ tests pass (164 existing + 5 new)
