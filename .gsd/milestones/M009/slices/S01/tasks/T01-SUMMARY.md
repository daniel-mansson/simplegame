---
id: T01
parent: S01
milestone: M009
provides:
  - IInSceneScreenManager<TScreenId> interface in Core.ScreenManagement
  - InSceneScreenManager<TScreenId> — SetActive panel swap, back stack, no-op on same screen, warning on missing panel
  - 10 EditMode tests covering all navigation cases
key_files:
  - Assets/Scripts/Core/ScreenManagement/IInSceneScreenManager.cs
  - Assets/Scripts/Core/ScreenManagement/InSceneScreenManager.cs
  - Assets/Tests/EditMode/Core/InSceneScreenManagerTests.cs
key_decisions:
  - "Constructor takes Dictionary<TScreenId, GameObject> — caller owns the map, keeps Core clean"
  - "EqualityComparer<TScreenId>.Default.Equals for enum equality — avoids boxing on value types"
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T01: Core InSceneScreenManager

**`InSceneScreenManager<TScreenId>` in Core — SetActive panel swap, back stack, 10 tests pass (180/180 total).**

## What Happened

Created `IInSceneScreenManager<TScreenId>` and `InSceneScreenManager<TScreenId>` in `Core/ScreenManagement/`. Generic, no game types. Constructor takes `Dictionary<TScreenId, GameObject>`. `ShowScreen` does SetActive swap, pushes old screen to history. `GoBack` pops and restores. No-ops on same screen or empty history.

10 EditMode tests cover all cases including multi-level back stack and same-screen no-op.

## Deviations
None.

## Files Created/Modified
- `Assets/Scripts/Core/ScreenManagement/IInSceneScreenManager.cs` — new interface
- `Assets/Scripts/Core/ScreenManagement/InSceneScreenManager.cs` — new implementation
- `Assets/Tests/EditMode/Core/InSceneScreenManagerTests.cs` — 10 tests, all pass
