# M022: In-Game Deck Panel

**Gathered:** 2026-03-26
**Status:** Ready for planning

## Project Description

Unity mobile jigsaw puzzle game. The InGame scene has a 3D puzzle board and a tray strip at the bottom that shows available puzzle pieces. Currently the tray uses world-space 3D GameObjects as visuals with invisible UGUI overlay buttons (`SlotButtonCanvas`, spawned by `PuzzleStageController.SpawnSlotButtons`) layered on top as tap targets. This system works but is fragile — overlay buttons must be repositioned every LateUpdate to track 3D piece positions in screen space.

## Why This Milestone

The existing tray/tap mechanism is a workaround: invisible UGUI buttons overlaid on 3D objects, repositioned every frame. Replacing it with a proper UGUI `HorizontalLayoutGroup` deck panel:
- Removes per-frame overlay repositioning
- Makes the tap surface a first-class UGUI element (reliable, no z-order fragility)
- Sets the foundation for polish work on the in-game scene (visual styling is a later pass)

User intent: "set up the deck panel to have the available pieces as UI buttons in a horizontal layout group. We will later work on the look of the in-game scene."

## User-Visible Outcome

### When this milestone is complete, the user can:
- Open the InGame scene and see a UGUI deck panel at the bottom with one button per available slot
- Tap any deck button to attempt placing that piece on the board
- See the deck panel update in real time as pieces are placed (button disappears when slot empties)

### Entry point / environment
- Entry point: InGame scene (direct play from editor, or via boot flow from MainMenu)
- Environment: Unity Editor play mode; iOS simulator
- Live dependencies involved: PuzzleModel (slot events), InGamePresenter (OnTapPiece handler)

## Completion Class

- Contract complete means: `RefreshSlot` calls from the presenter correctly add/remove deck buttons; `OnTapPiece` fires with correct piece ID on button tap
- Integration complete means: full play-through with boot → mainmenu → ingame → win/lose works correctly; no invisible overlay buttons remain
- Operational complete means: none (no daemon/service)

## Final Integrated Acceptance

To call this milestone complete, we must prove:
- Boot → MainMenu → Play → tapping deck buttons → win/lose popup → return to MainMenu works end-to-end
- No `SlotButtonCanvas` or `SpawnSlotButtons` code path exists
- All 347 existing tests still pass

## Risks and Unknowns

- `PuzzleStageController` LateUpdate currently repositions tray pieces in world-space **and** updates overlay button positions. Removing the overlay button update path while keeping the 3D-piece LateUpdate logic requires careful surgery — risk of inadvertently breaking tray-piece repositioning for board pieces.
- `SceneSetup.cs` regenerates the InGame scene from scratch — any new `[SerializeField]` on `InGameView` must be wired there too, otherwise fields are null at runtime (K007).

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/InGame/InGameView.cs` — has `_deckPanel` (GameObject), `_deckLabel` (Text), `_placeButton` (Button) wired via SceneSetup; `Awake()` hides `_deckPanel` immediately. This is the entry point for the new deck panel.
- `Assets/Scripts/Game/InGame/PuzzleStageController.cs` — `SpawnSlotButtons()` creates `SlotButtonCanvas` + invisible `Button` GameObjects; `LateUpdate` repositions them. Both must be removed in S02.
- `Assets/Editor/SceneSetup.cs` `CreateInGameScene()` — programmatically builds the InGame scene including `DeckPanel` with `DeckLabel` + `PlaceButton` children (legacy placeholder layout). Must be updated to add `HorizontalLayoutGroup` and swap in slot-button child infrastructure.
- `Assets/Scripts/Game/InGame/IInGameView.cs` / `InGamePresenter.cs` — `OnTapPiece(pieceId)` event is already how the presenter receives taps. No interface change needed.
- `Assets/Scripts/Puzzle/PuzzleModel.cs` — fires `OnSlotChanged(slotIndex, pieceId?)` which presenter bridges to `RefreshSlot`. The deck panel updates on every `RefreshSlot` call.

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- Visual tap surface for puzzle pieces is a launchability requirement — the deck panel is the long-term mechanism.

## Scope

### In Scope
- `InGameView`: activate `_deckPanel`, populate with `Button` children via `HorizontalLayoutGroup`, sync on `RefreshSlot` calls, fire `OnTapPiece` on button click
- `SceneSetup.cs` `CreateInGameScene()`: add `HorizontalLayoutGroup` to `DeckPanel`; remove `DeckLabel` + `PlaceButton` from DeckPanel (legacy placeholder); add a `_pieceButtonContainer` RectTransform child that the layout group manages; wire new field(s) on `InGameView`
- `PuzzleStageController`: remove `SpawnSlotButtons()`, `_slotButtons`, `_slotButtonCanvas`, and the overlay-button `LateUpdate` block; keep 3D piece repositioning logic intact
- Re-run `SceneSetup` to regenerate `Assets/Scenes/InGame.unity`
- All existing tests continue to pass

### Out of Scope / Non-Goals
- Visual styling of deck buttons (no sprites, no custom artwork) — plain placeholder look only
- 3D tray piece GameObjects: they still render in world-space as before (board and tray-slot visual); only the tap surface changes
- Any other in-game scene visual improvements
- New game mechanics

## Technical Constraints

- `InGameView` fields added must be wired in `SceneSetup.cs` (K007)
- After any `[SerializeField]` change on `InGameView`, SceneSetup must be re-run and scene file committed
- Existing test mocks implement `IInGameView` — if the interface changes, all mocks must update (K004)
- Do NOT change `IInGameView` signature if it can be avoided; the deck panel is a view-internal concern

## Integration Points

- `InGamePresenter` → `IInGameView.RefreshSlot(slotIndex, pieceId?)` — already the update path; deck buttons are driven by this
- `InGameView` → `InGamePresenter` via `OnTapPiece(pieceId)` — already the tap path; new buttons fire this
- `PuzzleStageController` → `InGameView` registration callbacks — unchanged (RevealPiece, MovePieceToTraySlot, ShakePieceInSlot still needed for 3D board logic)

## Open Questions

- None — scope is locked.
