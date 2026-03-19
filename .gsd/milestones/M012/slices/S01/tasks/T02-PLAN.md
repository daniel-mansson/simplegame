# T02: PuzzleModelConfig ScriptableObject

**Slice:** S01
**Milestone:** M012

## Goal

Create `PuzzleModelConfig` ScriptableObject in `SimpleGame.Game` assembly so `InGameSceneController` can read slot count from a project asset.

## Must-Haves

### Truths
- `PuzzleModelConfig` is a ScriptableObject in the `SimpleGame.Game.Puzzle` namespace
- It has a `SlotCount` field serialized in the Inspector, default value 3
- `SlotCount` is validated to be ≥ 1 (clamped or assertion)
- A default asset can be created via `[CreateAssetMenu]`

### Artifacts
- `Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs` — ScriptableObject, min 15 lines

## Steps
1. Create `PuzzleModelConfig.cs` in `Assets/Scripts/Game/Puzzle/`
2. Extend `ScriptableObject`, add `[CreateAssetMenu]`
3. `[SerializeField] private int _slotCount = 3`
4. Public property `SlotCount` with `Mathf.Max(1, _slotCount)` guard

## Context
- Lives in `SimpleGame.Game` asmdef (has Unity references) — not in `SimpleGame.Puzzle`
- `JigsawLevelFactory.cs` is already in `Assets/Scripts/Game/Puzzle/` — same folder
