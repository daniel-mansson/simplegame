---
id: T01
parent: S01
milestone: M007
provides:
  - IViewResolver interface in Core/PopupManagement
  - UnityViewContainer (renamed from UnityPopupContainer) implementing IPopupContainer<PopupId> + IViewResolver
  - Get<T>() via GetComponentInChildren<T>(true) on UnityViewContainer
key_files:
  - Assets/Scripts/Core/PopupManagement/IViewResolver.cs
  - Assets/Scripts/Game/Popup/UnityViewContainer.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
  - Assets/Scenes/Boot.unity
key_decisions:
  - IViewResolver is a separate interface from IPopupContainer (D041)
  - Container renamed to UnityViewContainer (D042)
  - Get<T>() implemented as GetComponentInChildren<T>(true) — finds inactive children, no registration needed
patterns_established:
  - git mv both .cs and .cs.meta together to preserve Unity GUID — never delete/recreate
  - Boot.unity m_EditorClassIdentifier must be updated manually after rename (GUID bind is primary, but editor shows stale name otherwise)
observability_surfaces:
  - rg "UnityPopupContainer" Assets/ → exit 1 (zero matches) confirms rename complete
  - rg "IViewResolver" Assets/Scripts/Core/PopupManagement/IViewResolver.cs → confirms interface exists
  - git status Assets/Scripts/Game/Popup/ → shows both .cs and .cs.meta as renamed (not deleted+untracked)
duration: 15m
verification_result: passed
completed_at: 2026-03-17
blocker_discovered: false
---

# T01: Create IViewResolver, rename UnityPopupContainer → UnityViewContainer, implement Get<T>(), update all references

**Created `IViewResolver` in Core, renamed `UnityPopupContainer` → `UnityViewContainer` via `git mv` (GUID preserved), implemented `Get<T>()` as `GetComponentInChildren<T>(true)`, and updated all references including the Boot.unity scene's `m_EditorClassIdentifier`.**

## What Happened

Executed all 6 steps from the plan atomically:

1. Created `IViewResolver.cs` in `Assets/Scripts/Core/PopupManagement/` with `T Get<T>() where T : class`.
2. Used `git mv` for both `UnityPopupContainer.cs` and `UnityPopupContainer.cs.meta` → `UnityViewContainer.cs` / `UnityViewContainer.cs.meta`. Git status confirmed both files show as `renamed:` (not deleted + untracked), so the `.meta` GUID is preserved.
3. Rewrote `UnityViewContainer.cs`: renamed class, added `IViewResolver` to implements list, implemented `Get<T>()` as `return GetComponentInChildren<T>(true)`. All existing `IPopupContainer<PopupId>` logic (SerializeField refs, switch on PopupId, SetActive calls) unchanged.
4. Updated `GameBootstrapper.cs`: `FindFirstObjectByType<UnityPopupContainer>()` → `FindFirstObjectByType<UnityViewContainer>()`.
5. Updated `SceneSetup.cs`: `AddComponent<UnityPopupContainer>()` → `AddComponent<UnityViewContainer>()`.
6. After verifying `rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/` returned zero, discovered `Assets/Scenes/Boot.unity` contained `m_EditorClassIdentifier: SimpleGame.Game::SimpleGame.Game.Popup.UnityPopupContainer`. Updated it via `sed` to match the new class name. Final `rg "UnityPopupContainer" Assets/` returns exit 1 (zero matches).

## Verification

All four task-plan verification commands passed:

```
rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/   → exit 1 (zero matches) ✓
rg "class UnityViewContainer" Assets/Scripts/Game/Popup/UnityViewContainer.cs
  → "public class UnityViewContainer : MonoBehaviour, IPopupContainer<PopupId>, IViewResolver" ✓
rg "IViewResolver" Assets/Scripts/Core/PopupManagement/IViewResolver.cs
  → "public interface IViewResolver" ✓
rg "IPopupContainer.*IViewResolver|IViewResolver.*IPopupContainer" Assets/Scripts/Game/Popup/UnityViewContainer.cs
  → matches both the comment line and the class declaration ✓
```

Slice verification (T01 scope):
- `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/` → found ✓
- `rg "UnityPopupContainer" Assets/` → exit 1, zero matches ✓
- `rg "class UnityViewContainer" Assets/Scripts/Game/Popup/` → found with both interfaces ✓
- ViewContainerTests (T02 deliverable) — not yet created, expected

## Diagnostics

To inspect what this task built:
- `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — confirms interface exists
- `rg "class UnityViewContainer" Assets/Scripts/Game/Popup/UnityViewContainer.cs` — confirms rename and dual-interface
- `git log --oneline Assets/Scripts/Game/Popup/` — shows rename in history
- `git show HEAD:Assets/Scripts/Game/Popup/UnityPopupContainer.cs.meta` — GUID is the same as `UnityViewContainer.cs.meta` (rename preserved it)

## Deviations

One deviation from the written plan: The Boot.unity scene file contained `m_EditorClassIdentifier: SimpleGame.Game::SimpleGame.Game.Popup.UnityPopupContainer`. The plan noted "if the scene file contains the string 'UnityPopupContainer' as a class name, it needs manual update — check and fix." It did, so it was fixed via `sed`. This was anticipated by the plan's conditional check.

## Known Issues

None. The rename is complete and the codebase compiles cleanly.

## Files Created/Modified

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — new Core interface with `T Get<T>() where T : class`
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — renamed from UnityPopupContainer, implements IPopupContainer<PopupId> + IViewResolver, adds Get<T>()
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — type reference updated to UnityViewContainer
- `Assets/Editor/SceneSetup.cs` — type reference updated to UnityViewContainer
- `Assets/Scenes/Boot.unity` — m_EditorClassIdentifier updated to UnityViewContainer
