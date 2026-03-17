---
estimated_steps: 5
estimated_files: 4
---

# T02: Tests + full verification

**Slice:** S01 — IViewResolver + Container Refactor
**Milestone:** M007

## Description

Write tests proving IViewResolver.Get<T>() resolution works on UnityViewContainer. Add a MockViewResolver for downstream test use. Run the full test suite to confirm all 164+ existing tests pass after the rename and refactor.

## Steps

1. Create `MockViewResolver` implementing `IViewResolver` in test helpers — stores view instances in a dictionary, returns them by type
2. Write edit-mode tests for UnityViewContainer: Get<T>() returns correct view component from child, Get<T>() returns null for unregistered type
3. Update any test files that reference `UnityPopupContainer` to use `UnityViewContainer`
4. Run full test suite via Unity batchmode
5. Fix any compilation or test failures from the rename

## Must-Haves

- [ ] MockViewResolver exists and implements IViewResolver
- [ ] At least 2 new tests: Get<T>() success case and Get<T>() null case
- [ ] All existing 164+ tests pass
- [ ] No references to `UnityPopupContainer` in `Assets/Tests/`

## Verification

- Unity batchmode test run: all tests pass including new ones
- `rg "UnityPopupContainer" Assets/Tests/` returns zero

## Inputs

- T01 output: IViewResolver interface, UnityViewContainer implementation
- Existing test patterns in `Assets/Tests/EditMode/Core/PopupManagerTests.cs`

## Expected Output

- New or updated test file with ViewContainer / IViewResolver tests
- MockViewResolver test double for use in S02 scene controller tests
- Clean test run with zero failures
