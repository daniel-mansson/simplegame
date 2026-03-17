# T02: Persistence service and MetaProgressionService with tests

**Slice:** S01
**Milestone:** M006

## Goal
Build the persistence layer (IMetaSaveService + PlayerPrefs implementation) and the MetaProgressionService that tracks runtime restoration state. Write comprehensive edit-mode tests.

## Must-Haves

### Truths
- IMetaSaveService can Save and Load MetaSaveData
- PlayerPrefsMetaSaveService serializes MetaSaveData to JSON in PlayerPrefs
- Persistence round-trips correctly (save → load → data matches)
- MetaProgressionService tracks per-object restoration progress (current step count)
- MetaProgressionService correctly identifies blocked objects (all blockers must be fully restored)
- MetaProgressionService correctly identifies environment completion (all objects fully restored)
- MetaProgressionService can restore one step on an unblocked object (returns success/failure)
- All operations covered by edit-mode tests

### Artifacts
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — data class for serialization
- `Assets/Scripts/Game/Services/IMetaSaveService.cs` — interface
- `Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs` — PlayerPrefs implementation
- `Assets/Scripts/Game/Services/MetaProgressionService.cs` — runtime progression tracking
- `Assets/Tests/EditMode/Game/MetaProgressionServiceTests.cs` — tests

### Key Links
- MetaProgressionService depends on WorldData (ScriptableObject) for structure
- MetaProgressionService depends on IMetaSaveService for persistence
- PlayerPrefsMetaSaveService implements IMetaSaveService
- MetaSaveData holds Dictionary<string, int> for per-object progress and int for golden pieces

## Steps
1. Write MetaSaveData.cs — serializable data class with per-object progress dict and golden piece balance
2. Write IMetaSaveService.cs — Save(MetaSaveData), Load() → MetaSaveData
3. Write PlayerPrefsMetaSaveService.cs — JSON serialization to/from PlayerPrefs
4. Write MetaProgressionService.cs — Initialize(WorldData, IMetaSaveService), GetProgress(object), IsBlocked(object), IsEnvironmentComplete(env), TryRestoreStep(object) → bool, Save()
5. Write MetaProgressionServiceTests.cs — test initialization, restore step, blocked check, environment completion, persistence round-trip
6. Run tests via Unity MCP to verify

## Context
- Use object asset name (or a stable ID) as the dictionary key for per-object progress
- MetaSaveData needs to be JSON-serializable — use simple types (Dictionary<string, int> may need a wrapper for JsonUtility)
- Unity's JsonUtility doesn't support Dictionary — use a list of key-value pairs or Newtonsoft.Json if available, otherwise a simple serializable wrapper
