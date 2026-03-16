---
estimated_steps: 5
estimated_files: 2
---

# T03: Make PopupManager and IPopupContainer generic

**Slice:** S01 — Core Assembly Restructure + Generic Managers
**Milestone:** M002

## Description

Replace the concrete `PopupId` dependency in `PopupManager` and `IPopupContainer` with type parameters. `IPopupContainer<TPopupId>` becomes generic; `PopupManager<TPopupId> where TPopupId : System.Enum` consumes it. `PopupId.cs` stays in its current location during S01 (deleted from Core and recreated in Game in S02).

## Steps

1. Read `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` and `PopupManager.cs` in full
2. Update `IPopupContainer.cs`: change to `public interface IPopupContainer<TPopupId>`, update both method signatures to use `TPopupId` instead of `PopupId`
3. Update `PopupManager.cs` class declaration: `public class PopupManager<TPopupId> where TPopupId : System.Enum`
4. Replace every `PopupId` type reference with `TPopupId`: `Stack<PopupId>` → `Stack<TPopupId>`, `PopupId?` → `TPopupId?`, `IPopupContainer` field → `IPopupContainer<TPopupId>`, constructor parameter type → `IPopupContainer<TPopupId>`
5. Confirm `popupId.ToString()` calls (if any) remain unchanged — works on any enum

## Must-Haves

- [ ] `IPopupContainer` reads `public interface IPopupContainer<TPopupId>`
- [ ] `PopupManager` reads `public class PopupManager<TPopupId> where TPopupId : System.Enum`
- [ ] No bare `PopupId` type references remain in either file
- [ ] Constructor of `PopupManager<TPopupId>` accepts `IPopupContainer<TPopupId>`

## Verification

- `grep "interface IPopupContainer<" Assets/Scripts/Core/PopupManagement/IPopupContainer.cs`
- `grep "class PopupManager<TPopupId>" Assets/Scripts/Core/PopupManagement/PopupManager.cs`
- `grep -n " PopupId[^<]" Assets/Scripts/Core/PopupManagement/PopupManager.cs` returns empty
- `grep -n " PopupId[^<]" Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` returns empty

## Inputs

- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — current implementation using concrete PopupId
- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — current interface using concrete PopupId

## Expected Output

- `Assets/Scripts/Core/PopupManagement/IPopupContainer.cs` — generic interface `IPopupContainer<TPopupId>`
- `Assets/Scripts/Core/PopupManagement/PopupManager.cs` — generic class `PopupManager<TPopupId>`
