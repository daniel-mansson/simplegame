# T03: PuzzleModelTests — EditMode domain tests

**Slice:** S01
**Milestone:** M012

## Goal

Write EditMode tests that verify the PuzzleModel contract end-to-end. These live alongside the existing PuzzleDomainTests.cs (which tests PuzzleSession and will be deleted in S04). New file: PuzzleModelTests.cs.

## Must-Haves

### Truths
- Tests compile and all pass in EditMode test runner
- Covers: seed pre-placement, correct tap → placed + slot refilled, wrong tap → rejected + slot unchanged
- Covers: deck exhaustion → slot becomes null, win condition (OnCompleted + IsComplete)
- Covers: multi-slot independence (tapping slot 1 doesn't change slot 0)
- Covers: OnRejected event fires with correct args, OnSlotChanged fires with new piece ID
- Covers: TryPlace on empty slot returns Empty
- Covers: PlacedCount increments on correct tap, does not increment on rejected tap

### Artifacts
- `Assets/Tests/EditMode/Puzzle/PuzzleModelTests.cs` — min 12 test methods

## Steps
1. Create PuzzleModelTests.cs in EditMode/Puzzle test folder
2. Write helper BuildDefaultModel() — 4-piece linear chain (seed=0, deck=[1,2,3]), 2 slots
3. Test: seeds on board after construction
4. Test: TryPlace correct slot → Placed, slot refills
5. Test: TryPlace wrong piece → Rejected, slot unchanged
6. Test: TryPlace empty slot → Empty
7. Test: OnSlotChanged fires with new piece ID after Placed
8. Test: OnPiecePlaced fires with placed piece ID
9. Test: OnRejected fires with correct slotIndex + pieceId
10. Test: PlacedCount increments on Placed, not on Rejected
11. Test: Deck exhaustion — slot becomes null after last piece drawn
12. Test: IsComplete + OnCompleted after all pieces placed
13. Test: Multi-slot — tapping slot 0 does not change slot 1 content

## Context
- PuzzleModelTests.cs lives in SimpleGame.Tests.Puzzle asmdef (already references SimpleGame.Puzzle)
- Use a 4-piece topology: piece 0 (seed, neighbours [1]), piece 1 (neighbours [0,2,3]), piece 2 (neighbours [1]), piece 3 (neighbours [1])
- For multi-slot test, use 2 slots — deck=[1,2,3], slot0=1, slot1=2
