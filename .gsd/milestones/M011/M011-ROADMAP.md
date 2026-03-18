# M011: Puzzle Domain Model & API

**Vision:** A pure C# puzzle domain model with no Unity dependencies, wrapped by a jigsaw adapter, with InGame wired to real placement logic and rendered tappable piece GameObjects.

## Success Criteria

- `SimpleGame.Puzzle` assembly compiles with `noEngineReferences: true` and all EditMode tests pass
- Adapter converts `SimpleJigsaw.PuzzleBoard` → `IPuzzleLevel` without exposing any `SimpleJigsaw.*` type to game or model code
- `InGamePresenter` delegates to `PuzzleSession.TryPlace(pieceId)` — no raw correct/incorrect view events
- Tapping a piece GameObject in Play mode calls through to the model and resolves correctly
- `PieceDragger` is not used anywhere in game code

## Key Risks / Unknowns

- `IInGameView` change breaks existing InGame tests — mocks must be updated in `InGameTests.cs` and `DemoWiringTests.cs`
- Assembly reference graph: must verify no transitive `SimpleJigsaw` reference enters `SimpleGame.Puzzle`
- `PieceObjectFactory.CreateAll` returns raw GameObjects — tap handler must be attached after creation without coupling to game logic

## Proof Strategy

- `IInGameView` mock breakage → retire in S03 by updating all mocks and confirming all tests pass
- Assembly reference isolation → retire in S02 by confirming asmdef graph has no `SimpleJigsaw` edge touching `SimpleGame.Puzzle`
- Tap handler coupling risk → retire in S04 by confirming `PieceTapHandler` has exactly one responsibility (forward tap with ID to view) and no service/presenter imports

## Verification Classes

- Contract verification: EditMode tests for all domain model logic (placement rules, deck draw, win/lose detection); asmdef `noEngineReferences` compilation check
- Integration verification: Play mode in InGame scene — pieces render, tap resolves via model, win/lose triggers
- Operational verification: none
- UAT / human verification: visual confirmation that pieces render at correct positions and tap feedback is visible (hearts/counter update)

## Milestone Definition of Done

This milestone is complete only when all are true:

- `SimpleGame.Puzzle` asmdef compiles clean with `noEngineReferences: true`
- All EditMode tests pass (including updated InGame mocks)
- `JigsawLevelFactory` produces a valid `IPuzzleLevel` from a `GridLayoutConfig` — verified by EditMode test
- No `SimpleJigsaw.*` type imported anywhere outside `JigsawLevelFactory`
- InGame Play mode: pieces visible, tap correct → placed, tap wrong → heart lost, all placed → win popup, hearts exhausted → lose popup
- `PieceDragger` has zero references in game code (`rg "PieceDragger" Assets/Scripts` returns nothing)

## Requirement Coverage

- Covers: R060, R091, R092, R093, R094, R095, R096, R097
- Partially covers: none
- Leaves for later: R098 (drag-drop), R099 (camera auto-adjust)
- Orphan risks: none

## Slices

- [x] **S01: Pure Puzzle Domain Model** `risk:high` `depends:[]`
  > After this: EditMode tests prove seed placement, neighbor validation, deck draw, and win detection — all in plain C# with zero Unity references; asmdef compiles with `noEngineReferences: true`

- [ ] **S02: Jigsaw Adapter** `risk:medium` `depends:[S01]`
  > After this: `JigsawLevelFactory.Build(GridLayoutConfig, seed)` returns a valid `IPuzzleLevel` — verified by EditMode test using real `BoardFactory` output; no `SimpleJigsaw.*` type visible outside the factory

- [ ] **S03: InGame Wired to PuzzleSession** `risk:medium` `depends:[S01,S02]`
  > After this: Play InGame scene — tap correct piece → placed, counter advances; tap wrong piece → heart lost; all pieces placed → win popup; `IInGameView` mock updated, all tests pass

- [ ] **S04: Tappable Piece GameObjects** `risk:low` `depends:[S02,S03]`
  > After this: Pieces render at solved positions in InGame; tapping a piece fires the model; correct/incorrect resolves visually via hearts/counter HUD; `PieceDragger` has zero game-code references

## Boundary Map

### S01 → S02

Produces:
- `SimpleGame.Puzzle` assembly with `noEngineReferences: true`
- `IPuzzlePiece` — `int Id`, `IReadOnlyList<int> NeighborIds`
- `IPuzzleBoard` — `bool CanPlace(int pieceId)`, `bool Place(int pieceId)`, `IReadOnlyCollection<int> PlacedIds`
- `IDeck` — `int? Peek()`, `bool Advance()`, `bool IsEmpty`
- `IPuzzleLevel` — `IReadOnlyList<IPuzzlePiece> Pieces`, `IReadOnlyList<int> SeedIds`, `IReadOnlyList<IDeck> Decks`
- `PuzzleSession` — `TryPlace(int pieceId) : PlacementResult`, `IsComplete`, `CurrentDeckPiece(int slotIndex)`
- `PlacementResult` enum — `Placed`, `Rejected`, `AlreadyPlaced`

Consumes:
- nothing (first slice)

### S01 → S03

Produces:
- All of S01→S02 above
- `PuzzleSession` — full API including event `OnPlacementResolved(int pieceId, PlacementResult result)`

Consumes:
- nothing (first slice)

### S02 → S03

Produces:
- `JigsawLevelFactory` in `SimpleGame.Game` namespace
- `JigsawLevelFactory.Build(GridLayoutConfig config, int seed, int[] seedPieceIds, int[][] deckOrders) : IPuzzleLevel`
- Concrete `PuzzlePiece`, `PuzzleBoard`, `Deck`, `PuzzleLevel` implementations (internal to adapter, exposed only via interfaces)

Consumes from S01:
- `IPuzzlePiece`, `IPuzzleBoard`, `IDeck`, `IPuzzleLevel`, `PuzzleSession`

### S02 → S04

Produces:
- `JigsawLevelFactory.Build(...)` — produces `IPuzzleLevel` and retains `SimpleJigsaw.PuzzleBoard` reference for `PieceObjectFactory` call
- Rendering bridge: adapter returns both `IPuzzleLevel` and the raw `PuzzleBoard` (or a render data struct) for `PieceObjectFactory.CreateAll`

Consumes from S01:
- `IPuzzleLevel`

### S03 → S04

Produces:
- Updated `IInGameView` — `event Action<int> OnTapPiece` (replaces `OnPlaceCorrect`/`OnPlaceIncorrect`)
- Updated `InGamePresenter` — accepts `IPuzzleLevel`, constructs `PuzzleSession`, routes `OnTapPiece` → `PuzzleSession.TryPlace`
- `InGameSceneController` — constructs level via `JigsawLevelFactory`, passes to presenter

Consumes from S01:
- `PuzzleSession`, `IPuzzleLevel`

Consumes from S02:
- `JigsawLevelFactory`
