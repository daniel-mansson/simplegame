---
id: S01
milestone: M007
provides:
  - IViewResolver interface in Core (Assets/Scripts/Core/PopupManagement/IViewResolver.cs)
  - UnityViewContainer (renamed from UnityPopupContainer, GUID preserved)
  - UnityViewContainer implements IPopupContainer<PopupId> + IViewResolver
  - Get<T>() resolves views via GetComponentInChildren<T>(true)
  - MockViewResolver test double for downstream use
  - 169/169 tests passing
key_files:
  - Assets/Scripts/Core/PopupManagement/IViewResolver.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Tests/EditMode/Game/ViewContainerTests.cs
key_decisions:
  - "IViewResolver.Get<T>() uses GetComponentInChildren<T>(true) — simple, finds inactive children"
  - "Kept SerializeField refs for PopupId→GameObject show/hide — only view resolution changed"
patterns_established:
  - "IViewResolver: Core interface for generic view resolution"
  - "MockViewResolver: dictionary-backed test double"
drill_down_paths:
  - .gsd/milestones/M007/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M007/slices/S01/tasks/T02-SUMMARY.md
verification_result: pass
completed_at: 2026-03-17T18:42:00Z
---

# S01: IViewResolver + Container Refactor

**IViewResolver in Core with Get<T>(), UnityPopupContainer renamed to UnityViewContainer, 169/169 tests pass**

## What Happened

Created the `IViewResolver` interface in Core's PopupManagement folder with `T Get<T>() where T : class`. Renamed `UnityPopupContainer` to `UnityViewContainer` via `git mv` (preserving .meta GUID). The container now implements both `IPopupContainer<PopupId>` and `IViewResolver`. View resolution via `GetComponentInChildren<T>(true)` finds views on inactive children.

All references updated across GameBootstrapper, SceneSetup, and test files. MockViewResolver created as a reusable test double. 5 new tests + 164 existing = 169/169 passing.

## What This Unlocks

S02 can now inject `IViewResolver` into scene controllers, replacing their `FindFirstObjectByType` calls for popup view resolution. The `MockViewResolver` is ready for test use.
