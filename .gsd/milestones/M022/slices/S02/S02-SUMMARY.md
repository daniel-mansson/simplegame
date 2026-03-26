---
id: S02
parent: M022
milestone: M022
provides:
  - PuzzleStageController with SpawnSlotButtons, _slotButtons, _slotButtonCanvas deleted
  - LateUpdate overlay-button repositioning block removed — only 3D piece repositioning remains
  - SpawnLevel calls SetupDeckPanel only; no SlotButtonCanvas created at runtime
key_files:
  - Assets/Scripts/Game/InGame/PuzzleStageController.cs
key_decisions:
  - Full rewrite of PuzzleStageController rather than surgical deletion — cleaner result, less risk of leftover dead code
patterns_established:
  - LateUpdate in PuzzleStageController now handles only 3D world-space piece repositioning
drill_down_paths:
  - .gsd/milestones/M022/slices/S02/tasks/T01-PLAN.md
duration: 10min
verification_result: pass
completed_at: 2026-03-26T19:15:00Z
---

# S02: Remove Slot Button Overlay

**SlotButtonCanvas and SpawnSlotButtons deleted from PuzzleStageController — deck panel is the sole tap surface**

## What Happened

Full rewrite of `PuzzleStageController.cs` removing: `_slotButtons` field, `_slotButtonCanvas` field, `SpawnSlotButtons()` method (60 lines), and the LateUpdate overlay-button repositioning block (20 lines). The `SpawnLevel()` now calls only `_inGameView?.SetupDeckPanel(slotCount)`.

The 3D piece LateUpdate repositioning loop is untouched — pieces in tray slots continue to track correctly each frame. 347/347 tests pass.

## Deviations

Chose full file rewrite over surgical `edit` calls — the deletions were spread across multiple non-contiguous blocks and a full rewrite produced a cleaner result.

## Files Created/Modified

- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — overlay button system removed; file reduced by ~100 lines
