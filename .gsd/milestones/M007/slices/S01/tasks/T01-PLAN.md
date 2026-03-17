---
estimated_steps: 6
estimated_files: 5
---

# T01: Create IViewResolver, rename UnityPopupContainer → UnityViewContainer, implement Get<T>(), update all references

**Slice:** S01 — IViewResolver + Container Refactor
**Milestone:** M007

## Description

Core production change for S01. Creates the `IViewResolver` interface in Core, renames the container class via `git mv` (preserving Unity .meta GUIDs), implements `Get<T>()` using `GetComponentInChildren<T>(true)`, and updates every file that references the old name. This must be atomic — a partial rename leaves the codebase in a broken state.

Key decisions: `IViewResolver` is a separate interface from `IPopupContainer` (D041). Container renamed to `UnityViewContainer` (D042). `Get<T>()` resolves via `GetComponentInChildren<T>(true)` which finds components on inactive children — no mapping, no registration needed.

## Steps

1. **Create `IViewResolver.cs`** in `Assets/Scripts/Core/PopupManagement/`:
   ```csharp
   namespace SimpleGame.Core.PopupManagement
   {
       public interface IViewResolver
       {
           T Get<T>() where T : class;
       }
   }
   ```
   No Unity dependencies — this is a Core interface.

2. **Rename container file via `git mv`** — this preserves the .meta GUID so Unity scene serialization is unbroken:
   ```bash
   git mv Assets/Scripts/Game/Popup/UnityPopupContainer.cs Assets/Scripts/Game/Popup/UnityViewContainer.cs
   git mv Assets/Scripts/Game/Popup/UnityPopupContainer.cs.meta Assets/Scripts/Game/Popup/UnityViewContainer.cs.meta
   ```

3. **Update the container class** in the renamed `UnityViewContainer.cs`:
   - Rename class `UnityPopupContainer` → `UnityViewContainer`
   - Add `IViewResolver` to the implements list: `public class UnityViewContainer : MonoBehaviour, IPopupContainer<PopupId>, IViewResolver`
   - Add the `Get<T>()` implementation:
     ```csharp
     public T Get<T>() where T : class
     {
         return GetComponentInChildren<T>(true);
     }
     ```
   - Keep all existing `IPopupContainer<PopupId>` implementation unchanged (show/hide by PopupId, SerializeField refs to popup GameObjects).

4. **Update `GameBootstrapper.cs`** — change `FindFirstObjectByType<UnityPopupContainer>()` to `FindFirstObjectByType<UnityViewContainer>()`. Note: the `FindFirstObjectByType` call itself is NOT removed in this slice — that's S02's job. Only the type name changes.

5. **Update `SceneSetup.cs`** — change `AddComponent<UnityPopupContainer>()` to `AddComponent<UnityViewContainer>()`. Update field names referencing the old name if any local variables use it.

6. **Verify no old references remain**: run `rg "UnityPopupContainer" Assets/` — must return zero matches. The `.unity` scene file may contain the old GUID reference but `git mv` preserves GUIDs so the reference still resolves. If the scene file contains the string "UnityPopupContainer" as a class name, it needs manual update — check and fix.

## Must-Haves

- [ ] `IViewResolver.cs` exists in `Assets/Scripts/Core/PopupManagement/` with `T Get<T>() where T : class`
- [ ] `UnityPopupContainer.cs` renamed to `UnityViewContainer.cs` via `git mv`
- [ ] `UnityViewContainer` class implements both `IPopupContainer<PopupId>` and `IViewResolver`
- [ ] `Get<T>()` implemented as `GetComponentInChildren<T>(true)`
- [ ] `GameBootstrapper.cs` references `UnityViewContainer` instead of `UnityPopupContainer`
- [ ] `SceneSetup.cs` references `UnityViewContainer` instead of `UnityPopupContainer`
- [ ] `rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/` returns zero matches

## Verification

- `rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/` → zero matches
- `rg "class UnityViewContainer" Assets/Scripts/Game/Popup/UnityViewContainer.cs` → finds the class
- `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/IViewResolver.cs` → finds the interface
- `rg "IPopupContainer.*IViewResolver\|IViewResolver.*IPopupContainer" Assets/Scripts/Game/Popup/UnityViewContainer.cs` → confirms both interfaces on one class

## Observability Impact

This task is a compile-time refactor. The relevant signals are:

- **Primary check:** `rg "UnityPopupContainer" Assets/Scripts/ Assets/Editor/` → must return exit 1 (zero matches). Any match is a broken-rename signal.
- **Git rename integrity:** `git status` on `Assets/Scripts/Game/Popup/` should show both `.cs` and `.cs.meta` as renamed. A missing `.meta` rename means Unity will assign a new GUID, silently breaking scene serialization for the Boot scene.
- **Compile signal:** Unity/Roslyn reports `CS0246` if any consumer still references `UnityPopupContainer`. This surfaces in the Unity Console on next recompile.
- **`IViewResolver` discovery:** `rg "IViewResolver" Assets/Scripts/Core/PopupManagement/IViewResolver.cs` confirms the interface exists and is inspectable.
- **Both-interfaces signal:** `rg "IPopupContainer.*IViewResolver" Assets/Scripts/Game/Popup/UnityViewContainer.cs` confirms the dual-interface implements clause.
- **No runtime state changes:** `Get<T>()` adds behavior but produces no logs — it either returns a component or null (Unity default). The `Debug.LogWarning` in `GetPopupObject` remains as the only diagnostic for unknown `PopupId` values.

A future agent inspecting this task should run the four `rg` commands in the Verification section — all passing means the task is complete and the codebase is in a valid state.

## Inputs

- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — existing Core interface pattern to follow
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — current container implementation to rename + extend
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — references container by type
- `Assets/Editor/SceneSetup.cs` — creates container in editor setup

## Expected Output

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — new Core interface
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — renamed container implementing both interfaces with `Get<T>()`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — updated type reference
- `Assets/Editor/SceneSetup.cs` — updated type reference
- Zero `UnityPopupContainer` references in `Assets/Scripts/` and `Assets/Editor/`
