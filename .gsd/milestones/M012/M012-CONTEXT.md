# M012: Stable Core Game — PuzzleModel Refactor

**Gathered:** 2026-03-18
**Status:** Ready for planning

## Project Description

Mobile-style jigsaw puzzle game. Players tap slots to attempt piece placement; correct pieces move to the board and the slot refills from the shared deck. Wrong taps cost a heart.

## Why This Milestone

The current `PuzzleSession` + `InGamePresenter` tray-window logic is buggy. The presenter hard-codes a 3-slot sliding lookahead: it walks a single deck, skips unplaceable pieces, and tries to build a 3-element window. This means the view can get out of sync with what is physically in each slot. The root cause is that **slots have no explicit tracked state** — they're inferred by the presenter at each refresh.

The fix is a `PuzzleModel` that makes slots first-class tracked state. Each slot holds a known piece ID (or is empty). The model fires typed events when slot contents change, when a piece is placed, and when the puzzle completes. The presenter subscribes and pushes exactly what changed to the view. No more inferring window state from a deck cursor.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Tap a slot and see the piece animate to its solved board position (correct placement)
- Tap a slot and see a heart deducted (wrong placement — piece stays in slot)
- See the slot immediately refill with the next piece from the deck after a correct tap
- Observe all slots drain the same shared deck independently
- Win when all pieces are placed; lose when hearts reach zero

### Entry point / environment

- Entry point: Play-from-editor in `Assets/Scenes/InGame.unity`
- Environment: Unity Editor play mode
- Live dependencies involved: none (no network, no persistence in this milestone)

## Completion Class

- Contract complete means: EditMode domain tests and presenter tests pass
- Integration complete means: Play-from-editor with a real jigsaw level (GridLayoutConfig assigned) shows correct slot/board behaviour
- Operational complete means: none

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Tap correct slot piece → piece moves to board, slot draws from deck top, slot count unchanged
- Tap wrong slot piece → heart deducted, slot piece unchanged, no board change
- Tap all pieces correctly → win event fires
- Three hearts exhausted → lose event fires
- All EditMode tests pass (no regressions in M001–M011 tests)

## Risks and Unknowns

- `JigsawLevelFactory` currently builds `IPuzzleLevel` with `IReadOnlyList<IDeck>`. The new `PuzzleModel` takes a flat deck + slot count. The factory output format must change or a thin adapter is needed. — This is S03's problem; the adapter boundary will be clear once the model API is fixed in S01.
- `PuzzleSession` deletion removes a class that `InGamePresenter`, `InGameSceneController`, and `PuzzleDomainTests` all reference. All three must update in lockstep. — Planned explicitly across S01–S04.
- `IInGameView.RefreshTray(int?[])` is a window-based API. Moving to slot-indexed updates requires changing the interface, which cascades to `InGameView`, `MockInGameView`, and all tests that assert tray state. — S03 handles this explicitly.

## Existing Codebase / Prior Art

- `Assets/Scripts/Puzzle/PuzzleSession.cs` — current domain object; **deleted** in this milestone
- `Assets/Scripts/Puzzle/PuzzleBoard.cs` — board placement logic; **reused** inside PuzzleModel
- `Assets/Scripts/Puzzle/Deck.cs` + `IDeck.cs` — deck cursor logic; **reused** (single shared deck)
- `Assets/Scripts/Puzzle/IPuzzlePiece.cs`, `PuzzlePiece.cs` — immutable piece data; **unchanged**
- `Assets/Scripts/Puzzle/PlacementResult.cs` — placement enum; **may be superseded** by model event types
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — **rewritten** in S02
- `Assets/Scripts/Game/InGame/IInGameView.cs` — **updated** in S03 (slot-indexed API)
- `Assets/Scripts/Game/InGame/InGameView.cs` — **updated** in S03
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — **updated** in S03 (passes PuzzleModelConfig)
- `Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs` — **updated** in S03 to produce PuzzleModel input
- `Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs` — **rewritten** in S04 for PuzzleModel
- `Assets/Tests/EditMode/Game/InGameTests.cs` — **updated** in S04

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R101 — PuzzleModel as ID-only state machine (new)
- R102 — Configurable slot count via ScriptableObject (new)
- R103 — Slots refill independently from shared deck top (new)
- R104 — Wrong tap costs heart, slot unchanged (new)
- R105 — Model fires typed events, presenter reacts (new)
- R106 — PuzzleSession deleted, replaced by PuzzleModel (new)
- R107 — View receives slot-indexed updates (new)
- R108 — Domain tests for PuzzleModel contract (new)
- R109 — JigsawLevelFactory feeds PuzzleModel cleanly (new)

## Scope

### In Scope

- New `PuzzleModel` class in `SimpleGame.Puzzle` (pure C#, no Unity types)
- `PuzzleModelConfig` ScriptableObject with configurable slot count
- Deletion of `PuzzleSession`, `IPuzzleLevel`, `PuzzleLevel`, `IDeck`, `Deck` (replaced by flat deck in PuzzleModel) — or keep `IDeck`/`Deck` internally if reuse is clean
- Rewrite of `InGamePresenter` to subscribe to model events
- Update of `IInGameView` to slot-indexed API
- Update of `InGameView` and `InGameSceneController`
- Update of `JigsawLevelFactory` to produce PuzzleModel input
- Rewrite of `PuzzleDomainTests` and update of `InGameTests`

### Out of Scope / Non-Goals

- Persistence (deck/board state saved between sessions)
- Shuffle logic (deck order is fixed at level creation time)
- New visual effects (piece animation, slot glow, etc.)
- Multiple puzzle levels or level selection
- Any changes to popup flow, meta-world, or MainMenu

## Technical Constraints

- `SimpleGame.Puzzle` has `noEngineReferences: true` — PuzzleModel must stay pure C#
- Assembly references: `SimpleGame.Game` → `SimpleGame.Puzzle`; no reverse reference allowed
- `JigsawLevelFactory` is the only file in `SimpleGame.Game` that may import `SimpleJigsaw.*` types (D062)
- `PuzzleBoard` can be reused internally by `PuzzleModel` — it is already correct and tested

## Integration Points

- `InGameSceneController` — reads `PuzzleModelConfig`, constructs `PuzzleModel`, passes to presenter
- `JigsawLevelFactory` — produces the flat piece list + seed IDs + ordered deck that `PuzzleModel` needs
- `UIFactory.CreateInGamePresenter` — will need to accept a `PuzzleModel` instead of `IPuzzleLevel`

## Open Questions

- Should `IPuzzleLevel` and related interfaces (`IDeck`, `Deck`) be deleted or kept as internal detail inside `PuzzleModel`? — Current thinking: delete the public interfaces; `PuzzleModel` owns its board and deck directly without exposing sub-interfaces. Revisit in S01 planning.
- Should `PlacementResult` enum survive? — It can be used internally by PuzzleModel but should not be the public surface; the public surface is events. Delete or make internal in S01.
