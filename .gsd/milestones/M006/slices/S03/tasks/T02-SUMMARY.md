---
id: T02
parent: S03
milestone: M006
provides:
  - Reworked InGameView — placeCorrect/placeIncorrect buttons, hearts + pieceCounter text
  - Reworked InGameSceneController — uses new presenter API with totalPieces
  - UIFactory updated — CreateInGamePresenter(view, totalPieces), optional IHeartService in constructor
  - GameSessionService updated — TotalPieces field, ResetForNewGame(levelId, totalPieces)
  - Reworked InGameTests — 14 presenter tests + 4 controller tests
requires:
  - T01 provides reworked IInGameView, InGamePresenter, InGameAction
affects: [S04, S06]
key_files:
  - Assets/Scripts/Game/InGame/InGameView.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/Services/GameSessionService.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
duration: 15min
verification_result: pass
completed_at: 2026-03-17T13:50:00Z
---

# T02: Rework InGameView, InGameSceneController, UIFactory, and tests

**Wired reworked presenter into scene controller, updated view and factory, rewrote all InGame tests**

## What Happened

Reworked InGameView: replaced scoreButton/winButton/loseButton with placeCorrectButton/placeIncorrectButton. Replaced scoreText with heartsText + pieceCounterText. Kept levelText.

Updated GameSessionService: added TotalPieces property. ResetForNewGame now takes optional totalPieces (default 10).

Updated UIFactory: constructor takes optional IHeartService. CreateInGamePresenter now takes (IInGameView, int totalPieces) and passes hearts service from factory.

Reworked InGameSceneController: simplified main loop — presenter auto-resolves win/lose via WaitForAction, no inner while loop needed. Uses _session.TotalPieces. Play-from-editor fallback uses _defaultTotalPieces serialized field.

Rewrote InGameTests: 14 presenter tests (initialize, place correct/incorrect, auto-win, auto-lose, mixed actions, dispose) + 4 controller tests (win flow, lose→back, lose→retry→win, play-from-editor).
