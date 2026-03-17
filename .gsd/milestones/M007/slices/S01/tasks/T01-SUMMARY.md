---
id: T01
parent: S01
milestone: M007
provides:
  - IViewResolver interface in Core (Assets/Scripts/Core/PopupManagement/IViewResolver.cs)
  - UnityViewContainer renamed from UnityPopupContainer (GUID preserved via git mv)
  - UnityViewContainer implements both IPopupContainer<PopupId> and IViewResolver
  - Get<T>() resolves views via GetComponentInChildren<T>(true)
  - All references updated (GameBootstrapper, SceneSetup)
affects: [S02, S03]
key_files:
  - Assets/Scripts/Core/PopupManagement/IViewResolver.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
key_decisions:
  - "IViewResolver.Get<T>() uses GetComponentInChildren<T>(true) — searches inactive children, simple and sufficient"
  - "Kept SerializeField refs for PopupId→GameObject show/hide mapping — only view resolution changed"
patterns_established:
  - "IViewResolver in Core for generic view resolution"
drill_down_paths:
  - .gsd/milestones/M007/slices/S01/tasks/T01-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-17T18:30:00Z
---

# T01: IViewResolver interface + UnityViewContainer rename and implementation

**IViewResolver in Core with Get<T>(), UnityPopupContainer renamed to UnityViewContainer implementing both IPopupContainer and IViewResolver**

## What Happened

Created the `IViewResolver` interface in Core's PopupManagement folder — a pure C# contract with a single `T Get<T>() where T : class` method. Used `git mv` to rename `UnityPopupContainer.cs` → `UnityViewContainer.cs` (preserving the .meta GUID so Unity scene references stay intact). Updated the class to implement both `IPopupContainer<PopupId>` and `IViewResolver`. The `Get<T>()` implementation delegates to `GetComponentInChildren<T>(true)` which finds components on inactive children — perfect for the pre-instantiated popup pattern.

Kept the existing SerializeField refs and switch for PopupId→GameObject show/hide mapping. The view *resolution* now goes through `Get<T>()`, but popup *activation* still uses the established pattern. Updated GameBootstrapper and SceneSetup to reference `UnityViewContainer`.

## Deviations
None.

## Files Created/Modified
- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — new Core interface
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — renamed + refactored (was UnityPopupContainer.cs)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — updated FindFirstObjectByType to use UnityViewContainer
- `Assets/Editor/SceneSetup.cs` — updated AddComponent to use UnityViewContainer
