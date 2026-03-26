# M022: In-Game Deck Panel — Summary

**Completed:** 2026-03-26
**Status:** Complete — 347/347 tests pass

## What Was Built

Replaced the invisible UGUI overlay button system (`SlotButtonCanvas`) with a proper `HorizontalLayoutGroup` deck panel. The `DeckPanel` at the bottom of the InGame scene now contains a `PieceButtonContainer` child with one `Button` per active tray slot, driven entirely by the existing `RefreshSlot` event chain — no new event paths, no interface changes.

## Slices Completed

### S01 — Deck Panel Wiring
- Added `SetupDeckPanel(int slotCount)` to `InGameView` — creates N UGUI Buttons in `_pieceButtonContainer`
- Each button's `onClick` fires `OnTapPiece(pieceId)` — straight into the existing presenter path
- `RefreshSlot` extended to show/hide and label the corresponding button
- Legacy `_deckLabel` and `_placeButton` fields removed from `InGameView`
- `SceneSetup.CreateInGameScene()` updated: `DeckPanel` now has `HorizontalLayoutGroup` + `PieceButtonContainer` child; no `DeckLabel`/`PlaceButton`
- `PuzzleStageController.SpawnLevel()` calls `SetupDeckPanel(slotCount)`
- Scene regenerated cleanly

### S02 — Remove Slot Button Overlay
- `SpawnSlotButtons()`, `_slotButtons`, `_slotButtonCanvas` deleted from `PuzzleStageController`
- LateUpdate overlay-button repositioning block removed
- 3D piece repositioning in LateUpdate untouched — pieces still track correctly
- `PuzzleStageController` reduced by ~100 lines

## Key Files

- `Assets/Scripts/Game/InGame/InGameView.cs`
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Scenes/InGame.unity`

## Verification

- 347/347 EditMode tests pass
- SceneSetup ran cleanly — InGame.unity regenerated with PieceButtonContainer + HorizontalLayoutGroup
- No SlotButtonCanvas or SpawnSlotButtons code in codebase
