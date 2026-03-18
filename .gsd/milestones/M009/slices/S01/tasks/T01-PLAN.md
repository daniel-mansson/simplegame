---
id: T01
parent: S01
milestone: M009
---

# T01: Core InSceneScreenManager

**Slice:** S01  
**Milestone:** M009

## Goal
Create `IInSceneScreenManager<TScreenId>` interface and `InSceneScreenManager<TScreenId>` plain C# implementation in Core. Write unit tests.

## Must-Haves

### Truths
- `ShowScreen(id)` calls `SetActive(false)` on the previously active panel, `SetActive(true)` on the new panel, updates `CurrentScreen`
- `ShowScreen(id)` pushes the previous screen onto the back stack
- `GoBack()` pops the back stack, activates the previous panel, updates `CurrentScreen`
- `GoBack()` is a no-op when history is empty (no throw)
- `CanGoBack` is false on fresh manager, true after `ShowScreen`, false again after `GoBack` clears history
- `CurrentScreen` returns null when no screen is registered

### Artifacts
- `Assets/Scripts/Core/ScreenManagement/IInSceneScreenManager.cs` — interface, min 10 lines
- `Assets/Scripts/Core/ScreenManagement/InSceneScreenManager.cs` — implementation, min 50 lines
- `Assets/Tests/EditMode/Core/InSceneScreenManagerTests.cs` — ≥6 test cases

### Key Links
- `InSceneScreenManager<TScreenId>` implements `IInSceneScreenManager<TScreenId>`
- Tests use a local `TestScreenId` enum (not game types)
- Tests use stub `GameObject`s created via `new GameObject()` (valid in EditMode)

## Steps
1. Create `IInSceneScreenManager.cs` in `Assets/Scripts/Core/ScreenManagement/`
2. Create `InSceneScreenManager.cs` — constructor takes `Dictionary<TScreenId, GameObject>` panel map; `ShowScreen` does SetActive swap + history push; `GoBack` pops and swaps; `CurrentScreen`/`CanGoBack` properties
3. Create `InSceneScreenManagerTests.cs` in `Assets/Tests/EditMode/Core/` — cover ShowScreen activates, GoBack restores, CanGoBack, empty GoBack no-op, CurrentScreen
4. Verify all tests pass

## Context
- Mirror `ScreenManager<TScreenId>` shape but replace scene loading with `SetActive`
- Constructor takes a pre-built `Dictionary<TScreenId, GameObject>` — the caller (SceneController) builds the map from `[SerializeField]` refs
- No transition, no input blocking — instant swap only
- Must stay in `SimpleGame.Core` namespace with no game-specific types
- `SimpleGame.Core.asmdef` already exists and is the correct assembly
