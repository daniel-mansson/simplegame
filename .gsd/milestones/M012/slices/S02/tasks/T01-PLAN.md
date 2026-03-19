# T01: Rewrite InGamePresenter

**Slice:** S02
**Milestone:** M012

## Goal

Replace `InGamePresenter`'s `IPuzzleLevel`/`PuzzleSession` dependency with `PuzzleModel`. Subscribe to model events reactively. Remove all tray-window logic.

## Must-Haves

### Truths
- `InGamePresenter` constructor signature: `(IInGameView, GameSessionService, IHeartService, PuzzleModel, int initialHearts = 3)`
- `Initialize()` subscribes to all four PuzzleModel events
- `Dispose()` unsubscribes from all four PuzzleModel events
- No `PushTrayWindow()`, no `PeekDeckAt`, no deck-cursor walking
- `OnRejected` handler: costs heart, updates hearts display, fires Lose if dead
- `OnPiecePlaced` handler: calls `view.RevealPiece(pieceId)`, updates piece counter, updates session score
- `OnCompleted` handler: fires Win (`_actionTcs.TrySetResult(InGameAction.Win)`)
- `OnSlotChanged` handler: calls `view.RefreshTray(new int?[] { pieceId })` as temporary shim (IInGameView not yet updated to slot-indexed API — shim keeps existing view working until S03)
- `UIFactory.CreateInGamePresenter(IInGameView, PuzzleModel)` updated

### Artifacts
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — rewritten, no PuzzleSession/IPuzzleLevel imports

### Key Links
- `InGamePresenter` → `PuzzleModel` via constructor injection
- `InGamePresenter` → `UIFactory` via `CreateInGamePresenter` method
- `InGamePresenter` → `IInGameView` via inherited `View` property

## Steps
1. Read current InGamePresenter carefully — note all methods and fields
2. Rewrite: remove `_level`, `_puzzleSession`, `PushTrayWindow()`, `HandleTapPiece()` 
3. Add `_model` field (PuzzleModel), subscribe to events in Initialize, unsubscribe in Dispose
4. Implement four event handlers: HandleSlotChanged, HandlePiecePlaced, HandleRejected, HandleCompleted
5. Remove `View.OnTapPiece` subscription — view no longer fires tap by piece ID; taps come as slot index events (handled in S03 via view change)
   - WAIT: The current view fires `OnTapPiece(pieceId)`. We need to keep some tap forwarding until S03 changes the view. Approach: keep `OnTapPiece` subscription but map pieceId to slot index via model.GetSlot scan, then call TryPlace on the matching slot. This is a temp bridge until S03.
6. Update `UIFactory.CreateInGamePresenter`
7. Update piece counter initialization — use `_model.TotalNonSeedCount`

## Context
- The view interface `IInGameView` still has `RefreshTray(int?[])` until S03 — call it as a temporary bridge from OnSlotChanged
- The view still fires `OnTapPiece(pieceId)` until S03 — need to map pieceId to slotIndex in the presenter as a temporary bridge
- `RestoreHeartsAndContinue()` still needed by InGameSceneController — keep it, just reset hearts
- `WaitForAction()` pattern unchanged — still returns UniTask<InGameAction>
