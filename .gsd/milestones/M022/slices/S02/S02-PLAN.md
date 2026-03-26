# S02: Remove Slot Button Overlay

**Goal:** Delete `SpawnSlotButtons`, `_slotButtons`, `_slotButtonCanvas`, and the LateUpdate overlay-button repositioning block from `PuzzleStageController`. The deck panel (from S01) is now the sole tap surface.

**Demo:** Play through a level — no `SlotButtonCanvas` GameObject exists in the hierarchy; 3D tray pieces still reposition correctly each frame; all tap targets are the UGUI deck panel buttons from S01.

## Must-Haves

- `PuzzleStageController` has no `_slotButtons`, `_slotButtonCanvas` fields
- `SpawnSlotButtons()` method is deleted entirely
- The `if (_slotButtons == null) return;` block in `LateUpdate` is removed (and the button-repositioning loop inside it)
- 3D piece LateUpdate repositioning is unaffected — pieces in tray slots still track correctly
- `SpawnLevel()` no longer calls `SpawnSlotButtons` — it calls only `_inGameView?.SetupDeckPanel(slotCount)` (already done in S01)
- All 347 existing tests pass
- Full boot → game → win/lose end-to-end works in play mode

## Tasks

- [ ] **T01: Delete overlay button system from PuzzleStageController**
  Remove `_slotButtons`, `_slotButtonCanvas` fields; delete `SpawnSlotButtons()` method; remove the button-repositioning `if (_slotButtons == null) return;` LateUpdate block; confirm 3D piece repositioning still works.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/PuzzleStageController.cs`
