# S04: Tappable Piece GameObjects

**Goal:** Render jigsaw pieces in InGame using PieceObjectFactory. Each piece gets a PieceTapHandler that fires `IInGameView.OnTapPiece(pieceId)`. Replace the stub level factory in InGameSceneController with JigsawLevelFactory. Update SceneSetup to remove placeholder buttons.

**Demo:** Pieces render at solved positions in InGame; tapping a piece fires the model; correct/incorrect resolves via hearts/counter HUD; `PieceDragger` has zero game-code references.

## Must-Haves

- `PieceTapHandler : MonoBehaviour` — single responsibility: fire `IInGameView.OnTapPiece(id)` on click
- `InGameSceneController` uses `JigsawLevelFactory.Build` to create the real level and calls `PieceObjectFactory.CreateAll` to spawn pieces
- Each spawned piece GameObject gets a `PieceTapHandler` wired to the `InGameView`
- `SceneSetup.CreateInGameScene` updated: remove placeholder buttons, add `PuzzleParent` empty GameObject
- `SceneSetup` re-run to update `Assets/Scenes/InGame.unity`
- `rg "PieceDragger" Assets/Scripts` returns nothing

## Tasks

- [ ] **T01: PieceTapHandler and InGameSceneController rendering wiring**
  Create `PieceTapHandler`. Update `InGameSceneController` to use `JigsawLevelFactory.Build` and spawn pieces with tap handlers. Add `[SerializeField]` for GridLayoutConfig and seed.

- [ ] **T02: SceneSetup update and scene re-generation**
  Update `SceneSetup.CreateInGameScene` to remove placeholder buttons, add `PuzzleParent`, keep InGameView text fields. Run `Tools/Setup/Create And Register Scenes`. Verify scene saves. Run tests — all still pass.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/PieceTapHandler.cs` (new)
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Editor/SceneSetup.cs`
- `Assets/Scenes/InGame.unity`
