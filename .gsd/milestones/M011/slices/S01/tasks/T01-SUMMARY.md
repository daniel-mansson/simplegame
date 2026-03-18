---
id: T01
parent: S01
milestone: M011
provides:
  - SimpleGame.Puzzle asmdef with noEngineReferences:true
  - IPuzzlePiece interface (Id, NeighborIds)
  - IPuzzleBoard interface (CanPlace, Place, PlacedIds)
  - IDeck interface (Peek, Advance, IsEmpty, Count, RemainingCount)
  - IPuzzleLevel interface (Pieces, SeedIds, Decks, TotalPieceCount)
  - PlacementResult enum (Placed, Rejected, AlreadyPlaced)
requires: []
affects: [T02, T03, S02, S03]
key_files:
  - Assets/Scripts/Puzzle/SimpleGame.Puzzle.asmdef
  - Assets/Scripts/Puzzle/IPuzzlePiece.cs
  - Assets/Scripts/Puzzle/IPuzzleBoard.cs
  - Assets/Scripts/Puzzle/IDeck.cs
  - Assets/Scripts/Puzzle/IPuzzleLevel.cs
  - Assets/Scripts/Puzzle/PlacementResult.cs
key_decisions:
  - "noEngineReferences:true enforced at asmdef level — Unity refuses compile if any UnityEngine type is referenced"
  - "IDeck exposes Count and RemainingCount for presenter progress display"
  - "IPuzzleBoard.CanPlace is advisory — caller must still call Place separately"
patterns_established:
  - "Pure C# domain assembly pattern: noEngineReferences:true, empty references array, autoReferenced:false"
drill_down_paths:
  - .gsd/milestones/M011/slices/S01/tasks/T01-PLAN.md
duration: 5min
verification_result: pass
completed_at: 2026-03-18T22:00:00Z
---

# T01: Domain Types and Interfaces

**`SimpleGame.Puzzle` asmdef with `noEngineReferences:true` and all puzzle domain interfaces and enums**

## What Happened

Created `Assets/Scripts/Puzzle/` with the `SimpleGame.Puzzle.asmdef` (empty references, `noEngineReferences:true`, `autoReferenced:false`) and six files: `PlacementResult` enum, `IPuzzlePiece`, `IPuzzleBoard`, `IDeck`, `IPuzzleLevel`. All types in `SimpleGame.Puzzle` namespace with no Unity imports.

## Deviations
None.

## Files Created/Modified
- `Assets/Scripts/Puzzle/SimpleGame.Puzzle.asmdef` — pure C# assembly definition
- `Assets/Scripts/Puzzle/PlacementResult.cs` — Placed/Rejected/AlreadyPlaced enum
- `Assets/Scripts/Puzzle/IPuzzlePiece.cs` — piece identity and adjacency contract
- `Assets/Scripts/Puzzle/IPuzzleBoard.cs` — board state and placement rule contract
- `Assets/Scripts/Puzzle/IDeck.cs` — ordered piece sequence contract
- `Assets/Scripts/Puzzle/IPuzzleLevel.cs` — level definition contract
