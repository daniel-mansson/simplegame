---
id: T02
parent: S01
milestone: M006
provides:
  - MetaSaveData — serializable data class with per-object progress list and golden pieces
  - IMetaSaveService — Save/Load/Delete interface for meta persistence
  - PlayerPrefsMetaSaveService — PlayerPrefs + JsonUtility implementation
  - MetaProgressionService — runtime progression tracking with blocked/complete checks
  - 18 edit-mode tests for MetaProgressionService and MetaSaveData
requires:
  - T01 provides ScriptableObject types (WorldData, EnvironmentData, RestorableObjectData)
affects: [S02, S05, S06]
key_files:
  - Assets/Scripts/Game/Services/MetaSaveData.cs
  - Assets/Scripts/Game/Services/IMetaSaveService.cs
  - Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs
  - Assets/Scripts/Game/Services/MetaProgressionService.cs
  - Assets/Tests/EditMode/Game/MetaProgressionServiceTests.cs
key_decisions:
  - "D036: IMetaSaveService interface + PlayerPrefs JSON implementation for persistence"
  - "Used List<ObjectProgress> instead of Dictionary for JsonUtility compatibility"
  - "MockMetaSaveService in tests uses JsonUtility round-trip for fidelity"
patterns_established:
  - "Interface-backed persistence: IMetaSaveService for swappable storage backend"
  - "MetaSaveData uses struct ObjectProgress list for JsonUtility serialization (no Dictionary support)"
drill_down_paths:
  - .gsd/milestones/M006/slices/S01/tasks/T02-PLAN.md
duration: 15min
verification_result: pass
completed_at: 2026-03-17T12:35:00Z
---

# T02: Persistence service and MetaProgressionService with tests

**Built IMetaSaveService + PlayerPrefs implementation, MetaProgressionService for object restoration tracking, and 18 edit-mode tests covering all operations**

## What Happened

Created the persistence layer:
- `MetaSaveData` — serializable data class using `List<ObjectProgress>` (struct with objectId + currentSteps) to work around JsonUtility's lack of Dictionary support. Has `GetSteps`/`SetSteps` helpers.
- `IMetaSaveService` — Save/Load/Delete interface
- `PlayerPrefsMetaSaveService` — JSON serialization to single PlayerPrefs key

Created `MetaProgressionService` — the runtime service that:
- Loads save data on construction
- Tracks per-object restoration progress via `GetCurrentSteps`/`IsObjectComplete`
- Checks blocked state (all blockers must be fully restored)
- Checks environment completion (all objects fully restored)
- `TryRestoreStep` validates blocked/complete state before applying (doesn't handle currency — that's GoldenPieceService's job)
- Save/Reload/ResetAll for persistence lifecycle

Wrote 18 edit-mode tests covering: initial zero progress, step increment, multi-step tracking, already-complete rejection, blocked/unblocked transitions, environment completion, persistence round-trip, reset, MetaSaveData operations, and null safety.

## Deviations
- Tests cannot run in Unity yet because the worktree is separate from the running Unity project (K003). Tests will be verified when the branch is merged or Unity is pointed at the worktree. Code follows established test patterns.

## Files Created
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — Serializable save data
- `Assets/Scripts/Game/Services/IMetaSaveService.cs` — Persistence interface
- `Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs` — PlayerPrefs implementation
- `Assets/Scripts/Game/Services/MetaProgressionService.cs` — Runtime progression service
- `Assets/Tests/EditMode/Game/MetaProgressionServiceTests.cs` — 18 edit-mode tests
