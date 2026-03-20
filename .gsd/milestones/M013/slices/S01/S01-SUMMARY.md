---
id: S01
milestone: M013
provides:
  - SolvableShuffle standalone class in SimpleGame.Puzzle (no Unity deps)
  - Static Shuffle() method: (seedIds, pieces, slotCount, rng) → List<int>
  - Window invariant guarantee: at least one piece per slotCount window is placeable
  - Anti-trivialisation: unlockable-neighbor filter ensures non-trivial windows
  - Backtracking: up to 50 steps; cascade resolve for committed invalid picks
  - SolvableShuffleTests: 20 tests covering all correctness properties
key_files:
  - Assets/Scripts/Puzzle/SolvableShuffle.cs
  - Assets/Tests/EditMode/Puzzle/SolvableShuffleTests.cs
key_decisions:
  - "Anti-trivialisation only allows invalid picks whose neighbor is currently in valid set — prevents deadlocks from disconnected invalid chains"
  - "consecutiveInvalidPicks counter limits invalid picks per window to slotCount-1"
  - "Cascade step after each valid pick: resolves committed invalid pieces now unlocked"
  - "AssertWindowInvariant simulates actual slot-based game (not static window scan) to correctly model cascaded placements"
patterns_established:
  - "SolvableShuffle.Shuffle() is the canonical deck ordering entry point for all puzzle builds"
drill_down_paths:
  - .gsd/milestones/M013/slices/S01/tasks/T01-PLAN.md
  - .gsd/milestones/M013/slices/S01/tasks/T02-PLAN.md
duration: 45min
verification_result: pass
completed_at: 2026-03-18T10:45:00Z
---

# S01: SolvableShuffle Algorithm

**Standalone topology-aware deck shuffle in SimpleGame.Puzzle: window invariant + anti-trivialisation + backtracking — 261/261 tests pass**

## What Happened

Implemented `SolvableShuffle` as a pure C# static class in the `SimpleGame.Puzzle` assembly (no Unity dependencies). The algorithm builds deck order incrementally: at each position, it classifies remaining pieces into valid (placeable now) and invalid (not yet placeable), then either picks a valid piece or — with slotCount > 1 — applies anti-trivialisation.

Anti-trivialisation required three iterations to get right:

1. **Initial version**: naively picked from `invalid` at 40% probability. Caused deadlocks because invalid pieces could have no unlockable neighbor in the active window.

2. **`consecutiveInvalidPicks` guard**: capped consecutive invalid picks at `slotCount - 1`. Fixed some cases but still broke when invalid picks were placed before their unlock pieces arrived in the deck.

3. **Final version**: unlockable-neighbor filter — only allows an invalid pick if at least one of the invalid piece's neighbors is currently in `valid`. This ensures the unlock piece will be placed in the same window round, making the invalid piece reachable within the active slots. Combined with a cascade step that resolves committed invalid pieces when a valid pick unlocks them.

The test helper `AssertWindowInvariant` was also revised: the original sliding-window scan was too pessimistic (didn't account for cascaded unlocks). The final version simulates the actual slot-based game — fills `slotCount` slots, scans all slots for placeable pieces, advances on any progress — which correctly models how the game handles cascaded placements.

## Deviations

Algorithm design evolved through three bug-fix iterations beyond the original plan. The final approach is cleaner than originally designed.

## Files Created/Modified

- `Assets/Scripts/Puzzle/SolvableShuffle.cs` — new, ~300 lines
- `Assets/Tests/EditMode/Puzzle/SolvableShuffleTests.cs` — new, ~400 lines
