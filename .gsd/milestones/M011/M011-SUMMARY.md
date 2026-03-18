---
id: M011
provides:
  - SimpleGame.Puzzle assembly — pure C# domain model (noEngineReferences:true)
  - IPuzzlePiece, IPuzzleBoard, IDeck, IPuzzleLevel, PuzzleSession, PlacementResult
  - JigsawLevelFactory — sole SimpleJigsaw coupling point; returns IPuzzleLevel + PuzzleBoard
  - PieceTapHandler — thin MonoBehaviour forwarding taps with piece ID to InGameView
  - InGamePresenter wired to PuzzleSession (tap → model → correct/incorrect)
  - IInGameView.OnTapPiece replaces OnPlaceCorrect/OnPlaceIncorrect
  - InGameSceneController spawns jigsaw pieces via PieceObjectFactory + wires tap handlers
  - SceneSetup updated — placeholder buttons removed, PuzzleParent added
  - 218 EditMode tests, all passing
key_files:
  - Assets/Scripts/Puzzle/SimpleGame.Puzzle.asmdef
  - Assets/Scripts/Puzzle/PuzzleSession.cs
  - Assets/Scripts/Puzzle/PuzzleBoard.cs
  - Assets/Scripts/Puzzle/IPuzzleLevel.cs
  - Assets/Scripts/Game/Puzzle/JigsawLevelFactory.cs
  - Assets/Scripts/Game/InGame/IInGameView.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/InGame/InGameView.cs
  - Assets/Scripts/Game/InGame/PieceTapHandler.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Puzzle/PuzzleDomainTests.cs
  - Assets/Tests/EditMode/Game/JigsawAdapterTests.cs
key_decisions:
  - "SimpleGame.Puzzle assembly has noEngineReferences:true — zero Unity imports enforced at asmdef level"
  - "JigsawLevelFactory + InGameSceneController are the only files with SimpleJigsaw imports"
  - "PieceTapHandler has one job: NotifyPieceTapped(id) on view — no game logic"
  - "InGameSceneController.SetLevelFactory() injects fresh-level factory for test isolation"
  - "Deck is mutable; controller rebuilds level each retry via factory lambda (fresh cursor)"
  - "PuzzleBoard.IsPlaced(id) added to avoid CS7036 ambiguous Contains on noEngineReferences assembly"
  - "SimpleJigsaw.Runtime added explicitly to SimpleGame.Game.asmdef (not auto-referenced)"
verification_result: pass
completed_at: 2026-03-18T23:30:00Z
---

# M011: Puzzle Domain Model & API

**Pure C# puzzle domain model, jigsaw adapter, InGame wired to real placement logic, 218/218 EditMode tests**

## What Was Built

Four slices delivered:

**S01** — `SimpleGame.Puzzle` assembly (`noEngineReferences:true`): all domain interfaces, `PuzzleBoard` (HashSet O(1) neighbor lookup), `Deck` (index cursor), `PuzzleSession` (seed pre-placement, `TryPlace`, `OnPlacementResolved` event). 16 EditMode tests.

**S02** — `JigsawLevelFactory` adapter: the sole file importing `SimpleJigsaw.*`. Converts `PieceDescriptor.Neighbors` → `PuzzlePiece`, builds `PuzzleLevel` with configurable seeds and decks. Returns `JigsawBuildResult` with both `IPuzzleLevel` and raw `PuzzleBoard` for rendering. 9 adapter tests.

**S03** — `IInGameView` redesigned: `OnTapPiece(int pieceId)` replaces `OnPlaceCorrect`/`OnPlaceIncorrect`. `InGamePresenter` delegates entirely to `PuzzleSession.TryPlace`. `InGameSceneController` uses a level factory lambda (rebuilt each retry for fresh deck state). All InGame tests rewritten around the new model.

**S04** — `PieceTapHandler` MonoBehaviour (single responsibility: forward tap → view). `InGameSceneController` calls `JigsawLevelFactory.Build` + `PieceObjectFactory.CreateAll` + attaches `PieceTapHandler` to each piece. `SceneSetup` updated to remove placeholder buttons, add `PuzzleParent`. All 4 scenes regenerated.

## Key Technical Findings

- `noEngineReferences:true` plus `IReadOnlyCollection<int>.Contains(int)` triggers CS7036 on Unity's C# version — `MemoryExtensions.Contains(ReadOnlySpan<char>, ...)` is picked up. Fixed by adding `PuzzleBoard.IsPlaced(id)` that calls `HashSet.Contains` directly.
- `SimpleJigsaw.Runtime` asmdef has `autoReferenced:true` — available to `Assembly-CSharp` but NOT to custom asmdef assemblies. Must be declared explicitly in `SimpleGame.Game.asmdef` and `SimpleGame.Tests.Game.asmdef`.
- `using SimpleJigsaw` + `using SimpleGame.Puzzle` causes `PuzzleBoard` ambiguity — solved by fully qualifying all `SimpleJigsaw.*` references in `JigsawLevelFactory.cs` and test files.
- `Deck` is mutable state — controller must rebuild the level via factory on each retry, not reuse the same instance.

## Deviations

- `InGameSceneController` also imports `SimpleJigsaw` directly (for `SpawnPieces`). Noted as acceptable: the controller is the Unity boundary layer, same as the adapter. Any future refactor can extract rendering to a dedicated `PuzzleRenderer` component.
- `BuildStubLevel` retained as fallback when no `GridLayoutConfig` is assigned (play-from-editor without asset).
