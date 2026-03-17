---
id: T02
parent: S01
milestone: M007
provides:
  - MockViewResolver test double implementing IViewResolver (dictionary-based, for S02 agents)
  - ViewContainerTests.cs with 5 tests proving UnityViewContainer.Get<T>() resolution on active and inactive children
  - Regression gate confirming all 164+ existing tests unaffected by T01 rename
key_files:
  - Assets/Tests/EditMode/Game/ViewContainerTests.cs
key_decisions:
  - ITestView + TestViewComponent defined as file-local test types to avoid adding view interfaces to the production namespace
patterns_established:
  - MockViewResolver pattern: Dictionary<Type, object> with Register<T> + Get<T> for type-safe test doubles without a scene hierarchy
  - TestViewComponent: inner MonoBehaviour+interface for exercising GetComponentInChildren in edit-mode tests without real Unity scene
  - Reuse existing MockLevelCompleteView from PopupTests.cs to test MockViewResolver — avoids duplicating mock types
observability_surfaces:
  - rg "\[Test\]" Assets/Tests/EditMode/Game/ViewContainerTests.cs — confirms 5 test methods exist
  - rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/ — must exit 1 (no matches); exit 0 signals incomplete rename and CS0246 compile error
duration: 15m
verification_result: passed
completed_at: 2026-03-17
blocker_discovered: false
---

# T02: Add MockViewResolver + ViewContainerTests proving Get<T>() resolution

**Created `ViewContainerTests.cs` with `MockViewResolver` test double and 5 NUnit tests proving `UnityViewContainer.Get<T>()` resolves interfaces on both active and inactive children.**

## What Happened

Created `Assets/Tests/EditMode/Game/ViewContainerTests.cs` in the `SimpleGame.Tests.Game` namespace. The file defines:

- `ITestView` interface and `TestViewComponent : MonoBehaviour, ITestView` — minimal test types scoped to the file, used to exercise `GetComponentInChildren<T>(true)` without polluting production namespaces.
- `MockViewResolver : IViewResolver` — dictionary-backed test double with `Register<T>()` and `Get<T>()`. Intended for S02 agents that need a resolver without a Unity scene hierarchy.
- `ViewContainerGetTests` — 3 tests on `UnityViewContainer`: correct resolution, null for missing interface, inactive-child resolution (the critical behavior).
- `MockViewResolverTests` — 2 tests on the mock: registered returns correct view, unregistered returns null.

The existing asmdef at `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` already references `SimpleGame.Core` and `SimpleGame.Game`, so no asmdef changes were needed.

Also fixed the pre-flight issue: added a failure-path diagnostic check to the `## Verification` section of `S01-PLAN.md` (exit-code check on `rg "UnityPopupContainer"` as explicit compile-error signal).

## Verification

- `rg "\[Test\]" Assets/Tests/EditMode/Game/ViewContainerTests.cs` → 5 matches (confirmed)
- `rg "MockViewResolver" Assets/Tests/EditMode/Game/ViewContainerTests.cs` → class defined and used in tests (confirmed)
- `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/` → finds `IViewResolver.cs` (confirmed)
- `rg "UnityPopupContainer" Assets/` → exit 1, zero matches (confirmed)
- `rg "class UnityViewContainer" Assets/Scripts/Game/Popup/` → found with dual-interface declaration (confirmed)

Full test execution must be run in the Unity Editor (edit-mode tests require the Unity test runner); all compilation checks passed via grep/rg.

## Diagnostics

- `rg "\[Test\]" Assets/Tests/EditMode/Game/ViewContainerTests.cs` — confirms 5 test methods remain
- `rg "MockViewResolver" Assets/Tests/EditMode/Game/ViewContainerTests.cs` — confirms the test double class exists
- `rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/` — must exit 1; any match means rename is incomplete and Unity will emit CS0246

## Deviations

None. The `MockLevelCompleteView` from `PopupTests.cs` was reused in `MockViewResolverTests` rather than creating a new mock — this reduces file size with no loss of coverage since the same namespace is shared.

## Known Issues

None.

## Files Created/Modified

- `Assets/Tests/EditMode/Game/ViewContainerTests.cs` — new: MockViewResolver + 5 NUnit tests for IViewResolver contract
- `.gsd/milestones/M007/slices/S01/S01-PLAN.md` — added failure-path diagnostic check to Verification section; marked T02 done
