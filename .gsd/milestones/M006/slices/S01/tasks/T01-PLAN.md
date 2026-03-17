# T01: ScriptableObject data types and test data

**Slice:** S01
**Milestone:** M006

## Goal
Define the three ScriptableObject types that model the meta world data (WorldData, EnvironmentData, RestorableObjectData) and create an editor utility to generate test data assets.

## Must-Haves

### Truths
- RestorableObjectData has fields: displayName, totalSteps, costPerStep, blockedBy (RestorableObjectData[])
- EnvironmentData has fields: environmentName, objects (RestorableObjectData[])
- WorldData has fields: environments (EnvironmentData[])
- Test data has 2 environments with 4+ total objects and at least one blocked-by relationship
- Editor utility can be invoked via menu item to create test data

### Artifacts
- `Assets/Scripts/Game/Meta/RestorableObjectData.cs` — ScriptableObject
- `Assets/Scripts/Game/Meta/EnvironmentData.cs` — ScriptableObject
- `Assets/Scripts/Game/Meta/WorldData.cs` — ScriptableObject
- `Assets/Editor/CreateTestWorldData.cs` — editor utility

### Key Links
- WorldData references EnvironmentData[]
- EnvironmentData references RestorableObjectData[]
- RestorableObjectData.blockedBy references other RestorableObjectData[]

## Steps
1. Create Assets/Scripts/Game/Meta/ directory
2. Write RestorableObjectData.cs — ScriptableObject with displayName, totalSteps, costPerStep, blockedBy
3. Write EnvironmentData.cs — ScriptableObject with environmentName, objects list
4. Write WorldData.cs — ScriptableObject with environments list
5. Write CreateTestWorldData.cs editor utility that creates 2 environments (Garden with 3 objects, Town Square with 2 objects), with blocked-by relationships
6. Verify compilation via Unity MCP

## Context
- First ScriptableObjects in this project (D035)
- Flat structure with blocked-by lists, not a tree (user requirement)
- Assets live in Assets/Data/ (new convention from M006-CONTEXT)
