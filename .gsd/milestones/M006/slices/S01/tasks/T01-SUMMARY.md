---
id: T01
parent: S01
milestone: M006
provides:
  - RestorableObjectData ScriptableObject — displayName, totalSteps, costPerStep, blockedBy[]
  - EnvironmentData ScriptableObject — environmentName, objects[]
  - WorldData ScriptableObject — environments[]
  - CreateTestWorldData editor utility — creates 2 environments, 5 objects with blocked-by relationships
requires: []
affects: [S05, S06]
key_files:
  - Assets/Scripts/Game/Meta/RestorableObjectData.cs
  - Assets/Scripts/Game/Meta/EnvironmentData.cs
  - Assets/Scripts/Game/Meta/WorldData.cs
  - Assets/Editor/CreateTestWorldData.cs
key_decisions:
  - "D035: ScriptableObjects for meta world data, flat structure with blocked-by lists"
  - "ObjectId uses asset name (ScriptableObject.name) as stable dictionary key for saves"
patterns_established:
  - "ScriptableObject data pattern: CreateAssetMenu for designer authoring, editor utility for programmatic creation"
drill_down_paths:
  - .gsd/milestones/M006/slices/S01/tasks/T01-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-17T12:30:00Z
---

# T01: ScriptableObject data types and test data

**Defined WorldData/EnvironmentData/RestorableObjectData ScriptableObjects with CreateTestWorldData editor utility generating 2 environments and 5 objects**

## What Happened

Created three ScriptableObject types in `Game/Meta/`:
- `RestorableObjectData` — displayName, totalSteps, costPerStep, blockedBy[] with ObjectId property using asset name
- `EnvironmentData` — environmentName, objects[]
- `WorldData` — ordered environments[]

Created `CreateTestWorldData` editor utility (menu: Tools/Setup/Create Test World Data) that generates test data in Assets/Data/:
- Garden environment: Fountain (3 steps, no blockers), Bench (2 steps, no blockers), Gazebo (4 steps, blocked by Fountain)
- Town Square environment: ClockTower (5 steps, no blockers), Statue (3 steps, blocked by ClockTower)

## Deviations
None.

## Files Created
- `Assets/Scripts/Game/Meta/RestorableObjectData.cs` — ScriptableObject with blocked-by support
- `Assets/Scripts/Game/Meta/EnvironmentData.cs` — ScriptableObject grouping objects
- `Assets/Scripts/Game/Meta/WorldData.cs` — Top-level world data asset
- `Assets/Editor/CreateTestWorldData.cs` — Editor utility for test data generation
