# S01: IViewResolver + Container Refactor — Research

**Date:** 2026-03-17
**Depth:** Light research — straightforward application of known patterns in well-understood codebase.

## Summary

S01 introduces a `IViewResolver` interface in Core and renames `UnityPopupContainer` to `UnityViewContainer`, implementing both `IPopupContainer<PopupId>` and the new `IViewResolver`. This is a clean refactor with no architectural unknowns — the codebase already uses interface-based view resolution in tests (`SetViewsForTesting`), and the container already holds all 6 popup views as SerializeField refs.

The key insight: `GetComponentInChildren<T>(true)` on the container naturally finds view interfaces on inactive children — no indexing, no registration, no mapping. This gives us a single-line `Get<T>()` implementation that resolves any view interface the container's children implement.

## Recommendation

1. Create `IViewResolver` in `Assets/Scripts/Core/PopupManagement/` with `T Get<T>() where T : class`
2. Rename `UnityPopupContainer` → `UnityViewContainer` via `git mv` to preserve .meta GUIDs
3. Add `IViewResolver` to the container's implements list; implement `Get<T>()` as `GetComponentInChildren<T>(true)`
4. Keep existing `IPopupContainer<PopupId>` implementation unchanged — PopupId-based show/hide is orthogonal
5. Update all references (GameBootstrapper, SceneSetup, tests)
6. Add focused tests for `Get<T>()` resolution

## Implementation Landscape

### Key Files

- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — existing Core interface for show/hide by PopupId. IViewResolver lives alongside this.
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` → renamed to `UnityViewContainer.cs` — implements both interfaces. Already has SerializeField refs to all 6 popup GameObjects.
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — references container by type. Needs field/variable rename.
- `Assets/Editor/SceneSetup.cs` — creates container programmatically in editor. Needs type rename.
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — has `MockPopupContainer` that needs rename awareness.
- `Assets/Tests/EditMode/Core/PopupManagerTests.cs` — has `MockPopupContainer` test double.

### Build Order

1. **IViewResolver interface** — new file in Core, zero risk, unblocks everything
2. **Rename + implement** — `git mv` preserves GUID; add `IViewResolver` to class declaration; implement `Get<T>()`
3. **Update references** — GameBootstrapper, SceneSetup, test files — all straightforward find-replace
4. **Tests** — ViewContainerTests proving `Get<T>()` resolves correct interfaces from child components

### Verification Approach

- `rg "UnityPopupContainer" Assets/` returns zero (rename complete)
- `rg "IViewResolver" Assets/Scripts/Core/` confirms interface exists
- Unity batchmode test run: all existing tests + new ViewContainerTests pass
- `Get<T>()` tests verify resolution of view interfaces from inactive children

## Constraints

- `git mv` required (not copy+delete) to preserve Unity .meta GUID — avoids breaking scene serialization
- IViewResolver must live in Core assembly (`Assets/Scripts/Core/PopupManagement/`) — cannot reference Unity types
- Container stays in Game assembly — it's the Unity-specific implementation
- Existing `SetViewsForTesting` seams on scene controllers are unaffected — they bypass the container entirely

## Requirements Targeted

- **R070** (primary) — `IViewResolver` with `Get<T>() where T : class` in Core
- **R071** (primary) — Rename container, implement both `IPopupContainer<PopupId>` and `IViewResolver`
- **R069** (supporting) — Popup views organized under container as inactive children (already the case; formalized here)
- **R075** (supporting) — No new FindObject* calls introduced
- **R076** (supporting) — All tests pass after refactor
