# M022: In-Game Deck Panel

**Vision:** Replace the invisible UGUI overlay button system with a proper HorizontalLayoutGroup deck panel — one button per active tray slot, driven by existing RefreshSlot events, firing existing OnTapPiece callbacks.

## Success Criteria

- `DeckPanel` is active and shows one `Button` per occupied tray slot in a `HorizontalLayoutGroup`
- Buttons appear/disappear in sync with `RefreshSlot` calls (slot filled → button shows; slot emptied → button hidden or removed)
- Tapping a deck button fires `OnTapPiece(pieceId)` → presenter places the piece correctly
- `SlotButtonCanvas` and `SpawnSlotButtons` are gone — no invisible overlay buttons remain
- `SceneSetup.cs` regenerates the InGame scene correctly with the new layout
- All 347 existing tests pass

## Key Risks / Unknowns

- `PuzzleStageController.LateUpdate` handles both 3D-piece repositioning AND overlay-button repositioning — excising the button half without breaking the piece half requires care
- New `[SerializeField]` fields on `InGameView` must be wired in `SceneSetup.cs` and scene re-run (K007 hard rule)

## Proof Strategy

- LateUpdate overlay-button risk → retired in S02 by verifying 3D tray pieces still reposition correctly after the overlay-button block is removed
- SceneSetup wiring risk → retired in S01 by running SceneSetup and confirming no null-ref in play mode

## Verification Classes

- Contract verification: `RefreshSlot` unit tests; `OnTapPiece` fires correctly; 347 tests green
- Integration verification: boot → MainMenu → play → tap deck buttons → win → MainMenu works end-to-end in play mode
- Operational verification: none
- UAT / human verification: play through a full puzzle level using only the deck panel buttons

## Milestone Definition of Done

This milestone is complete only when all are true:

- `InGameView` deck panel populates with buttons and stays in sync with slot state
- Tapping buttons drives the presenter correctly
- `SlotButtonCanvas` / `SpawnSlotButtons` code is deleted
- `SceneSetup.cs` regenerates a correct InGame scene
- All 347 existing tests pass
- End-to-end play through confirmed in play mode (boot → game → win/lose)

## Requirement Coverage

- Covers: piece tap surface (launchability — replaces invisible overlay hack with real UGUI)
- Leaves for later: visual styling of deck buttons

## Slices

- [ ] **S01: Deck Panel Wiring** `risk:medium` `depends:[]`
  > After this: tapping a deck button fires OnTapPiece and drives the presenter — full gameplay works via the UGUI deck panel
- [ ] **S02: Remove Slot Button Overlay** `risk:low` `depends:[S01]`
  > After this: PuzzleStageController has no SlotButtonCanvas or SpawnSlotButtons; LateUpdate only repositions 3D board/tray pieces; all tap targets are the deck panel buttons

## Boundary Map

### S01 → S02

Produces:
- `InGameView._pieceButtonContainer` — `RectTransform` child of `DeckPanel` with `HorizontalLayoutGroup`; `_slotButtons` array of `Button` GameObjects, one per slot, populated in `SetupDeckPanel(int slotCount)` and refreshed by `RefreshSlot`
- `InGameView.SetupDeckPanel(int slotCount)` — public method called by `PuzzleStageController.SpawnLevel()` after spawning pieces, setting up N buttons in the layout group
- `SceneSetup.CreateInGameScene()` updated — `DeckPanel` has `HorizontalLayoutGroup`; `_pieceButtonContainer` child wired; legacy `DeckLabel` + `PlaceButton` children removed

Consumes:
- nothing (first slice)

### S02 (leaf)

Produces:
- `PuzzleStageController` with `SpawnSlotButtons`, `_slotButtons`, `_slotButtonCanvas` deleted; LateUpdate overlay-button block removed

Consumes from S01:
- `InGameView.SetupDeckPanel()` — S02 replaces the `SpawnSlotButtons()` call in `PuzzleStageController.SpawnLevel()` with `_inGameView.SetupDeckPanel(slotCount)`
