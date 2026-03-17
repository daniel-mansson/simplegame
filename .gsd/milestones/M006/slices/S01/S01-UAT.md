# S01: Meta world data model and persistence — UAT

## Prerequisites
- Open the project in Unity Editor
- Run Tools/Setup/Create Test World Data to generate test assets

## Test Steps

### 1. Verify ScriptableObject assets
1. Navigate to Assets/Data/ in the Project window
2. **Observe**: WorldData.asset, Garden.asset, TownSquare.asset, Fountain.asset, Bench.asset, Gazebo.asset, ClockTower.asset, Statue.asset exist
3. Click WorldData.asset — should have 2 environments listed
4. Click Garden.asset — should have 3 objects listed
5. Click Gazebo.asset — blockedBy should reference Fountain

### 2. Verify blocked-by relationships
1. Click Gazebo — blockedBy contains Fountain
2. Click Statue — blockedBy contains ClockTower
3. Click Fountain, Bench, ClockTower — blockedBy arrays are empty

### 3. Run edit-mode tests
1. Open Window > General > Test Runner
2. Run all EditMode tests
3. **Observe**: MetaProgressionServiceTests — all 18 tests pass
4. **Observe**: All existing 98 tests still pass (total should be 116)

## Expected Results
- ScriptableObject test data exists with correct relationships
- All 116 tests pass (98 existing + 18 new)
