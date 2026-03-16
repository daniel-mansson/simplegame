---
estimated_steps: 4
estimated_files: 2
---

# T02: Make ScreenManager generic

**Slice:** S01 — Core Assembly Restructure + Generic Managers
**Milestone:** M002

## Description

Replace the concrete `ScreenId` dependency in `ScreenManager` with a type parameter `TScreenId`. The manager becomes `ScreenManager<TScreenId> where TScreenId : System.Enum`, making it usable with any enum — not just `ScreenId`. `ScreenId.cs` stays in its current location during S01 (it will be deleted from Core and recreated in Game in S02).

## Steps

1. Read `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` in full to see every usage of `ScreenId`
2. Update the class declaration: `public class ScreenManager` → `public class ScreenManager<TScreenId> where TScreenId : System.Enum`
3. Replace every `ScreenId` type reference with `TScreenId`: field `Stack<ScreenId>` → `Stack<TScreenId>`, property `ScreenId? CurrentScreen` → `TScreenId? CurrentScreen`, parameter `ScreenId screenId` → `TScreenId screenId`, local `ScreenId? _currentScreen` → `TScreenId? _currentScreen`
4. `screenId.ToString()` requires no change — works on any enum type

## Must-Haves

- [ ] Class declaration reads `public class ScreenManager<TScreenId> where TScreenId : System.Enum`
- [ ] No bare `ScreenId` type references remain in the file (only the file `ScreenId.cs` still exists separately, which is fine)
- [ ] `CurrentScreen` property returns `TScreenId?`

## Verification

- `grep "class ScreenManager<TScreenId>" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs`
- `grep -n " ScreenId[^<]" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` returns empty (no bare ScreenId type refs — note the leading space to avoid matching the filename comment)

## Inputs

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — current implementation using concrete ScreenId

## Expected Output

- `Assets/Scripts/Core/ScreenManagement/ScreenManager.cs` — updated with generic type parameter; behavior unchanged
