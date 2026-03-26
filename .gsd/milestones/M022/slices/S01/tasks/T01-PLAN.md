# T01: InGameView — Deck Panel Setup and RefreshSlot Integration

**Slice:** S01
**Milestone:** M022

## Goal

Add `SetupDeckPanel(int slotCount)` to `InGameView` that creates N Button children in a `_pieceButtonContainer`, update `RefreshSlot` to show/hide and label them, and activate the DeckPanel.

## Must-Haves

### Truths
- `InGameView.SetupDeckPanel(3)` creates 3 Button children inside `_pieceButtonContainer`
- Each button, when clicked, fires `OnTapPiece` with the piece ID currently in that slot (or does nothing if the slot is empty)
- `RefreshSlot(slotIndex, pieceId)` sets the button at `slotIndex` active with a piece label when pieceId has a value; hides it when null
- `_deckPanel.SetActive(false)` is removed from `Awake()` — DeckPanel starts active
- `_deckLabel` and `_placeButton` fields are removed from `InGameView` (they were legacy placeholder UI removed in SceneSetup T02)
- `IInGameView` interface is unchanged (no new public interface members needed — `SetupDeckPanel` is a concrete method called by stage controller, not part of the interface)

### Artifacts
- `Assets/Scripts/Game/InGame/InGameView.cs` — updated: `_pieceButtonContainer` SerializeField, `SetupDeckPanel()`, updated `RefreshSlot()`, no `_deckPanel.SetActive(false)` in Awake, `_deckLabel`/`_placeButton` fields removed

### Key Links
- `PuzzleStageController.SpawnLevel()` → `InGameView.SetupDeckPanel(slotCount)` (wired in T02)
- `InGameView` button.onClick → `OnTapPiece?.Invoke(pieceId)` — existing presenter handler receives it

## Steps

1. Read `InGameView.cs` fully to understand current field layout and method structure
2. Remove `[SerializeField] private Text _deckLabel` and `[SerializeField] private Button _placeButton` fields (no longer used after deck panel replaces them)
3. Add `[SerializeField] private RectTransform _pieceButtonContainer` — the HorizontalLayoutGroup container child of DeckPanel
4. Add `private Button[] _deckButtons` — runtime array, one per slot, created by SetupDeckPanel
5. Implement `SetupDeckPanel(int slotCount)`: destroy any existing children in `_pieceButtonContainer`, create `slotCount` Buttons with Text children, wire each button's onClick to check slot contents and invoke `OnTapPiece`
6. Update `RefreshSlot(int slotIndex, int? pieceId)`: after updating `_slotContents`, set `_deckButtons[slotIndex]` active/inactive and update its label text
7. Remove `_deckPanel.SetActive(false)` from `Awake()` — also remove `_placeButton.gameObject.SetActive(false)` from Awake
8. Remove the `_deckLabel` update in the old `RefreshSlot` (slot 0 label update)
9. Run `lsp diagnostics` to confirm no compile errors

## Context

- `_deckPanel` is already a `[SerializeField] private GameObject` — keep it, just don't hide it in Awake
- `_pieceButtonContainer` will be wired in SceneSetup (T02) — it's a child RectTransform inside _deckPanel with HorizontalLayoutGroup
- Button text should show "Piece N" or just the slot index for now — visual polish is a later milestone
- The `_slotContents` array tracking already exists — `RefreshSlot` already maintains it; just extend it to also update the button
- Do NOT change `IInGameView` — `SetupDeckPanel` is a concrete method on `InGameView` called by the stage controller directly (it already holds an `InGameView` reference, not `IInGameView`)
- `RegisterPieceCallbacks` signature: remove `onHideTray` param reference in the body (it's already ignored) — leave signature as-is since SceneSetup wires it
