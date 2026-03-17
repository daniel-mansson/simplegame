# S01: Meta world data model and persistence

**Goal:** Define the meta world data structure via ScriptableObjects (WorldData → EnvironmentData → RestorableObjectData), build a persistence service (IMetaSaveService + PlayerPrefs implementation), and create a MetaProgressionService that tracks runtime restoration state. Create test data with 2 environments and 4+ objects.

**Demo:** ScriptableObject assets exist in Assets/Data/ with test data. MetaProgressionService can restore objects step-by-step, check blocked state, check environment completion. Persistence round-trips through PlayerPrefs. All proven by edit-mode tests.

## Must-Haves
- RestorableObjectData ScriptableObject: name, totalSteps, costPerStep, blockedBy list
- EnvironmentData ScriptableObject: name, list of RestorableObjectData
- WorldData ScriptableObject: ordered list of EnvironmentData
- IMetaSaveService interface: Save/Load meta state (per-object progress, golden piece balance)
- PlayerPrefsMetaSaveService implementation with JSON serialization
- MetaProgressionService: tracks restoration progress per object, checks blocked state, checks environment completion
- Test data: 2 environments, 4+ objects with blocked-by relationships
- Edit-mode tests for MetaProgressionService and persistence round-trip

## Tasks

- [x] **T01: ScriptableObject data types and test data**
  Create WorldData, EnvironmentData, RestorableObjectData ScriptableObjects. Create a CreateTestWorldData editor utility to generate test assets (2 environments, 4+ objects with blocked-by relationships).

- [x] **T02: Persistence service and MetaProgressionService with tests**
  Create IMetaSaveService + PlayerPrefsMetaSaveService. Create MetaProgressionService that loads save data, tracks per-object progress, checks blocked state, checks environment completion. Write edit-mode tests for all operations.

## Files Likely Touched
- Assets/Scripts/Game/Meta/WorldData.cs (new)
- Assets/Scripts/Game/Meta/EnvironmentData.cs (new)
- Assets/Scripts/Game/Meta/RestorableObjectData.cs (new)
- Assets/Scripts/Game/Services/IMetaSaveService.cs (new)
- Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs (new)
- Assets/Scripts/Game/Services/MetaProgressionService.cs (new)
- Assets/Scripts/Game/Services/MetaSaveData.cs (new)
- Assets/Editor/CreateTestWorldData.cs (new)
- Assets/Tests/EditMode/Game/MetaProgressionServiceTests.cs (new)
- Assets/Tests/EditMode/Game/PlayerPrefsMetaSaveServiceTests.cs (new)
