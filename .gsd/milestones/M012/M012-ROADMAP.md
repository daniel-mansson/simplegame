# M012: Stable Core Game — PuzzleModel Refactor

**Vision:** Replace the buggy PuzzleSession + tray-window presenter with a PuzzleModel that owns board state, a shared deck, and N explicitly-tracked slots. Each slot refills independently from the deck top on correct placement. The view reacts to typed model events. Wrong taps cost a heart; the slot piece is unchanged. The result is a stable, testable core game loop.

## Success Criteria

- Tapping a slot with a placeable piece moves it to the board and refills the slot from the deck top
- Tapping a slot with an unplaceable piece deducts one heart; slot is unchanged
- Winning (all pieces placed) and losing (hearts exhausted) events fire correctly
- No tray-window lookahead logic remains in the presenter
- `PuzzleSession` is deleted; all tests pass with `PuzzleModel` replacing it
- Slot count is configurable via a `PuzzleModelConfig` ScriptableObject

## Key Risks / Unknowns

- `JigsawLevelFactory` output format must change to feed `PuzzleModel` — retiring in S03
- `IInGameView` slot-indexed API change cascades to view, mocks, and tests — retiring in S03
- `PuzzleSession` deletion touches presenter, scene controller, and all domain tests in lockstep — mitigated by doing model first (S01), then presenter (S02), then wiring (S03), then tests (S04)

## Proof Strategy

- JigsawLevelFactory format mismatch → retire in S03 by proving play-from-editor with real GridLayoutConfig works
- View API cascade → retire in S03 by confirming all mocks compile and InGameTests still pass
- Deletion lockstep risk → retire in S04 by running all EditMode tests clean with no PuzzleSession references remaining

## Verification Classes

- Contract verification: EditMode tests for PuzzleModel (slot/deck/board/events), InGamePresenter tests
- Integration verification: play-from-editor with GridLayoutConfig assigned; tap correct → slot refills; tap wrong → heart lost
- Operational verification: none
- UAT / human verification: play-from-editor visual confirmation that slots show correct pieces and refill after placement

## Milestone Definition of Done

This milestone is complete only when all are true:

- `PuzzleModel` exists in `SimpleGame.Puzzle`, pure C#, no Unity refs, all slot/deck/board behaviour covered by tests
- `InGamePresenter` subscribes to model events; no PushTrayWindow or deck-walking logic remains
- `IInGameView` is slot-indexed; `InGameView` and `MockInGameView` updated accordingly
- `InGameSceneController` reads slot count from `PuzzleModelConfig` ScriptableObject
- `JigsawLevelFactory` produces a flat piece list + seeds + deck that `PuzzleModel` accepts directly
- `PuzzleSession` class (and any now-orphaned interfaces) is deleted
- All EditMode tests pass with zero compiler errors or warnings
- Play-from-editor confirmed: correct slot tap places piece + refills slot; wrong tap costs heart; win/lose fire correctly

## Requirement Coverage

- Covers: R101, R102, R103, R104, R105, R106, R107, R108, R109
- Partially covers: R001 (MVP pattern extended to PuzzleModel), R007 (domain model layer updated)
- Leaves for later: none relevant
- Orphan risks: none

## Slices

- [ ] **S01: PuzzleModel — board, deck, N slots** `risk:high` `depends:[]`
  > After this: EditMode tests prove PuzzleModel handles slot refill, heart events, win/lose — no Unity, no presenter.

- [ ] **S02: InGamePresenter rewrite** `risk:medium` `depends:[S01]`
  > After this: InGamePresenter tests pass with PuzzleModel events driving view; tray-window logic gone.

- [ ] **S03: View + SceneController wiring** `risk:medium` `depends:[S02]`
  > After this: play-from-editor works end-to-end — slot-indexed view, PuzzleModelConfig slot count, JigsawLevelFactory feeding PuzzleModel.

- [ ] **S04: Tests & cleanup** `risk:low` `depends:[S03]`
  > After this: all EditMode tests green, PuzzleSession deleted, no regressions, no orphaned references.

## Boundary Map

### S01 → S02

Produces:
- `PuzzleModel` class — constructor takes `IReadOnlyList<IPuzzlePiece>`, seed IDs, ordered deck, slot count
- `PuzzleModel.TryPlace(slotIndex)` → `SlotTapResult` (Placed, Rejected, Empty)
- `PuzzleModel.OnSlotChanged(slotIndex, int? pieceId)` event
- `PuzzleModel.OnPiecePlaced(int pieceId)` event
- `PuzzleModel.OnCompleted()` event
- `PuzzleModel.SlotCount`, `PuzzleModel.IsComplete`, `PuzzleModel.PlacedCount`, `PuzzleModel.TotalNonSeedCount`
- `PuzzleModelConfig` ScriptableObject with `SlotCount` field

Consumes:
- nothing (first slice)

### S02 → S03

Produces:
- `InGamePresenter` — subscribes to `PuzzleModel` events, drives `IInGameView`
- No `PushTrayWindow()`, no deck lookahead
- `InGamePresenter` constructor accepts `PuzzleModel` (not `IPuzzleLevel`)
- `UIFactory.CreateInGamePresenter` updated signature

Consumes from S01:
- `PuzzleModel` API — all events and public methods listed above

### S03 → S04

Produces:
- `IInGameView` updated: `RefreshSlot(int slotIndex, int? pieceId)` replaces `RefreshTray(int?[])`
- `InGameView` updated to slot-indexed callbacks
- `InGameSceneController` reads `PuzzleModelConfig`, constructs `PuzzleModel`, passes to presenter
- `JigsawLevelFactory` updated to produce flat piece list + seeds + deck (drops `IPuzzleLevel` output or wraps it as adapter)
- `MockInGameView` in test files updated to new interface

Consumes from S02:
- `InGamePresenter` that expects slot-indexed view methods

### S04 → done

Produces:
- `PuzzleDomainTests` rewritten for `PuzzleModel`
- `InGamePresenterTests` updated for new constructor signature
- `InGameSceneControllerTests` updated for new flow
- `PuzzleSession`, `IPuzzleLevel`, `PuzzleLevel` deleted (and `IDeck`/`Deck` if now internal)
- All EditMode tests green

Consumes from S03:
- Full wired stack — model, presenter, view, scene controller
