---
id: T02
parent: S01
milestone: M012
provides:
  - PuzzleModelConfig ScriptableObject with SlotCount field (default 3)
  - CreateAssetMenu path: SimpleGame/Puzzle Model Config
requires: []
affects: [S03]
key_files:
  - Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs
key_decisions:
  - "Lives in SimpleGame.Game assembly (Unity types needed); not in SimpleGame.Puzzle"
  - "Mathf.Max(1, _slotCount) guard in property — never returns less than 1"
patterns_established: []
duration: 5min
verification_result: pass
completed_at: 2026-03-18T00:00:00Z
---

# T02: PuzzleModelConfig ScriptableObject

**PuzzleModelConfig ScriptableObject created in SimpleGame.Game.Puzzle with configurable SlotCount (default 3).**

## What Happened

Minimal ScriptableObject with a single `[SerializeField] int _slotCount = 3` and a `SlotCount` property that clamps to ≥ 1. Added `[CreateAssetMenu]` so a config asset can be created from the Project window.

## Deviations

None.

## Files Created/Modified
- `Assets/Scripts/Game/Puzzle/PuzzleModelConfig.cs` — new, 28 lines
