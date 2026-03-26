# T01: Delete Overlay Button System from PuzzleStageController

**Slice:** S02
**Milestone:** M022

## Goal

Remove all slot-button overlay code from `PuzzleStageController` — fields, method, and LateUpdate block — leaving only the 3D piece repositioning logic intact.

## Must-Haves

### Truths
- No `_slotButtons` or `_slotButtonCanvas` fields exist in `PuzzleStageController`
- `SpawnSlotButtons()` method does not exist
- `LateUpdate` contains no code that references `_slotButtons` or `_slotButtonCanvas`
- `SpawnLevel()` does not call `SpawnSlotButtons`
- 3D pieces still reposition in LateUpdate (the tray-slot piece tracking loop is untouched)
- Play mode: tapping deck panel buttons (from S01) works; no `SlotButtonCanvas` GameObject in hierarchy
- All 347 existing tests pass

### Artifacts
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — `_slotButtons`, `_slotButtonCanvas`, `SpawnSlotButtons()`, and the LateUpdate button block removed; SpawnLevel no longer calls SpawnSlotButtons

### Key Links
- No new links — this is a deletion task only

## Steps

1. Read `PuzzleStageController.cs` fully to confirm the exact lines to delete
2. Delete the `_slotButtons` field declaration (`private UnityEngine.UI.Button[] _slotButtons;`)
3. Delete the `_slotButtonCanvas` field declaration (`private Canvas _slotButtonCanvas;`)
4. Delete the `SpawnSlotButtons(int slotCount)` method entirely (from opening brace to closing brace)
5. In `SpawnLevel()`, find the `SpawnSlotButtons(slotCount)` call and remove it (only `_inGameView?.SetupDeckPanel(slotCount)` should remain)
6. In `LateUpdate()`, find the block `if (_slotButtons == null) return;` and the button-repositioning loop below it — delete both (keep everything above: the 3D piece repositioning loop for tray slots)
7. Run `lsp diagnostics` to confirm no compile errors
8. Commit: `feat(M022/S02): remove slot button overlay — deck panel is sole tap surface`

## Context

- The `LateUpdate` structure: first half repositions 3D tray slot pieces (keep); second half (after `if (_slotButtons == null) return;`) repositions the invisible overlay buttons (delete)
- The 3D piece repositioning loop uses `_traySlotPositions`, `_slotContents` from `_inGameView?.GetSlotContents()` — all of that stays
- After this task, the InGame scene should have no `SlotButtonCanvas` at runtime — verify via MCP or play mode hierarchy inspection
