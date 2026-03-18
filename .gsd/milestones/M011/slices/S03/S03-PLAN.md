# S03: InGame Wired to PuzzleSession

**Goal:** Replace stub placement events with real PuzzleSession delegation. `IInGameView` gets `OnTapPiece(int pieceId)`. `InGamePresenter` receives `IPuzzleLevel`, constructs a `PuzzleSession`, and routes taps through the model. All existing tests updated.

**Demo:** Play InGame scene — tap correct piece → placed, counter advances; tap wrong piece → heart lost; all pieces placed → win popup; `IInGameView` mock updated, all tests pass.

## Must-Haves

- `IInGameView.OnTapPiece(int pieceId)` replaces `OnPlaceCorrect`/`OnPlaceIncorrect`
- `InGamePresenter` takes `IPuzzleLevel` instead of `int totalPieces`; constructs `PuzzleSession` internally
- `InGamePresenter` routes `OnTapPiece` → `PuzzleSession.TryPlace(id)` → updates hearts/counter/win-lose
- `UIFactory.CreateInGamePresenter` updated to accept `IPuzzleLevel`
- `InGameView.cs` updated: removes placeholder buttons, exposes `OnTapPiece` (stubbed — real tap wiring is S04)
- All `MockInGameView` usages in tests updated; all 218+ tests pass after changes

## Tasks

- [ ] **T01: IInGameView, InGamePresenter, UIFactory**
  Replace `OnPlaceCorrect`/`OnPlaceIncorrect` with `OnTapPiece(int)`. Rewrite `InGamePresenter` to use `PuzzleSession`. Update `UIFactory.CreateInGamePresenter` signature.

- [ ] **T02: Update InGameView and all mocks/tests**
  Update `InGameView.cs` (remove placeholder buttons, add `OnTapPiece`). Update `MockInGameView` and all test callsites in `InGameTests.cs`. Update `InGameSceneController` fallback. Run tests — all must pass.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/IInGameView.cs`
- `Assets/Scripts/Game/InGame/InGamePresenter.cs`
- `Assets/Scripts/Game/InGame/InGameView.cs`
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
