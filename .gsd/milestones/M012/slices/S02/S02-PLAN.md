# S02: InGamePresenter rewrite

**Goal:** Rewrite `InGamePresenter` to accept a `PuzzleModel` instead of `IPuzzleLevel`, subscribe to model events reactively, and drive the view without any tray-window lookahead logic. Update `UIFactory.CreateInGamePresenter` signature. Prove via updated InGamePresenter tests.

**Demo:** InGamePresenter EditMode tests pass with PuzzleModel events driving view updates; no PushTrayWindow, no deck-walking code.

## Must-Haves

- `InGamePresenter` constructor takes `PuzzleModel` (not `IPuzzleLevel`)
- Subscribes to `OnSlotChanged`, `OnPiecePlaced`, `OnRejected`, `OnCompleted` in `Initialize()`
- Unsubscribes in `Dispose()`
- On `OnSlotChanged(slotIndex, pieceId?)`: calls `view.RefreshSlot(slotIndex, pieceId)` — NOTE: IInGameView still has old `RefreshTray` at this stage; presenter calls whichever exists (temp shim acceptable until S03 updates the interface)
- On `OnPiecePlaced(pieceId)`: calls `view.RevealPiece(pieceId)`, updates piece counter
- On `OnRejected`: costs a heart, updates hearts display, fires Lose if hearts reach 0
- On `OnCompleted`: fires Win
- No `PushTrayWindow()` method, no deck-cursor walking
- `UIFactory.CreateInGamePresenter` accepts `PuzzleModel` instead of `IPuzzleLevel`
- `InGamePresenter` tests updated to construct `PuzzleModel` and simulate slot taps

## Tasks

- [x] **T01: Rewrite InGamePresenter**
  Replace IPuzzleLevel + PuzzleSession with PuzzleModel. Subscribe to events. Remove tray-window logic. Update UIFactory.

- [x] **T02: Update InGamePresenter tests**
  Update MockInGameView and InGamePresenterTests to use PuzzleModel and slot-tap API.

## Files Likely Touched

- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — rewritten
- `Assets/Scripts/Game/Boot/UIFactory.cs` — CreateInGamePresenter signature
- `Assets/Tests/EditMode/Game/InGameTests.cs` — presenter tests updated
