---
estimated_steps: 7
estimated_files: 6
---

# T01: IViewResolver interface + UnityViewContainer rename and implementation

**Slice:** S01 — IViewResolver + Container Refactor
**Milestone:** M007

## Description

Create the IViewResolver interface in Core's PopupManagement folder. Rename UnityPopupContainer to UnityViewContainer (git mv to preserve meta GUID). Implement IViewResolver on the container using GetComponentInChildren<T>(true) to resolve view interfaces from inactive children. Update all references across the codebase. Keep PopupId-based show/hide working.

## Steps

1. Create `IViewResolver.cs` in `Assets/Scripts/Core/PopupManagement/` with `T Get<T>() where T : class`
2. `git mv` UnityPopupContainer.cs → UnityViewContainer.cs (and .meta)
3. Rename the class inside from `UnityPopupContainer` to `UnityViewContainer`, add `IViewResolver` to implements list
4. Implement `Get<T>()` as `GetComponentInChildren<T>(true)` — searches inactive children
5. Refactor show/hide: replace per-popup SerializeField refs with child lookup. Each popup child has its view MonoBehaviour — find the popup GameObject by looking up the view component's gameObject
6. Update `GameBootstrapper.cs` — change `FindFirstObjectByType<UnityPopupContainer>()` to use the renamed type `UnityViewContainer` (still FindFirstObjectByType for now — S02 replaces this)
7. Update `Assets/Editor/SceneSetup.cs` — change references from UnityPopupContainer to UnityViewContainer

## Must-Haves

- [ ] `IViewResolver` interface exists at `Assets/Scripts/Core/PopupManagement/IViewResolver.cs`
- [ ] `UnityViewContainer.cs` exists (renamed via git mv)
- [ ] `UnityViewContainer` implements both `IPopupContainer<PopupId>` and `IViewResolver`
- [ ] `Get<T>()` uses `GetComponentInChildren<T>(true)` 
- [ ] Show/hide by PopupId still works (finds child GameObject from view component)
- [ ] No references to `UnityPopupContainer` remain in `Assets/Scripts/`

## Verification

- `rg "UnityPopupContainer" Assets/Scripts/` returns zero
- `rg "class UnityViewContainer" Assets/Scripts/` returns the file
- `rg "interface IViewResolver" Assets/Scripts/Core/` returns the file
- Project compiles (check via Unity batchmode or read console)

## Inputs

- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — current container to refactor
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — existing Core interface pattern to follow
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — references UnityPopupContainer
- D041, D042, D046 — decisions on IViewResolver, rename, inactive children

## Expected Output

- `Assets/Scripts/Core/PopupManagement/IViewResolver.cs` — new Core interface
- `Assets/Scripts/Game/Popup/UnityViewContainer.cs` — renamed and refactored container
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — updated reference
- `Assets/Editor/SceneSetup.cs` — updated reference
