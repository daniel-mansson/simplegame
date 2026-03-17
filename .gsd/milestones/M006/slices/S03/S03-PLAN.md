# S03: Stub gameplay screen with hearts

**Goal:** InGame scene reworked with hearts, piece counter, place-correct/place-incorrect buttons, automatic win/lose conditions, and interstitial ad debug log.
**Demo:** Tests prove: placing correct pieces increments counter, incorrect costs a heart, win at totalPieces, lose at 0 hearts, interstitial log fires at both.

## Must-Haves

- IInGameView reworked: OnPlaceCorrect, OnPlaceIncorrect replace OnScoreClicked/OnWinClicked/OnLoseClicked; UpdateHearts, UpdatePieceCounter
- InGamePresenter uses IHeartService, tracks pieces placed/total, auto-resolves win/lose
- InGameView reworked: place-correct/place-incorrect buttons, hearts label, piece counter label
- InGameSceneController: injects IHeartService, resets hearts to 3 at level start
- UIFactory: passes IHeartService to InGamePresenter
- InGameAction: PlaceCorrect, PlaceIncorrect, Win, Lose
- Debug.Log for interstitial ad at win/lose
- Edit-mode tests for reworked presenter

## Verification

- All InGamePresenter tests pass in `InGameTests.cs` (reworked)
- All InGameSceneController tests pass (reworked)
- No compile errors

## Tasks

- [x] **T01: Rework IInGameView, InGamePresenter, InGameAction** `est:25m`
  - Why: Core gameplay logic change — replace manual win/lose buttons with piece placement + hearts
  - Files: `Assets/Scripts/Game/InGame/IInGameView.cs`, `Assets/Scripts/Game/InGame/InGamePresenter.cs`, `Assets/Scripts/Game/InGame/InGameAction.cs`
  - Do:
    1. Rework InGameAction — keep Win/Lose, remove IncrementScore, add PlaceCorrect/PlaceIncorrect
    2. Rework IInGameView — OnPlaceCorrect/OnPlaceIncorrect (replace OnScoreClicked/OnWinClicked/OnLoseClicked), UpdateHearts(string), UpdatePieceCounter(string), UpdateLevelLabel(string)
    3. Rework InGamePresenter — constructor takes IInGameView + GameSessionService + IHeartService + int totalPieces. Initialize resets hearts to initial count, sets piece counter to "0/total". PlaceCorrect increments pieces, updates counter, auto-resolves Win when pieces == total. PlaceIncorrect calls UseHeart, updates hearts display, auto-resolves Lose when !IsAlive. Debug.Log for interstitial ad at win and lose.
  - Verify: Code compiles, logic is correct
  - Done when: Presenter handles piece counting, hearts, auto-win/lose, interstitial log

- [x] **T02: Rework InGameView, InGameSceneController, UIFactory, and tests** `est:30m`
  - Why: Wire the reworked presenter into scene controller, update view, update factory, rewrite tests
  - Files: `Assets/Scripts/Game/InGame/InGameView.cs`, `Assets/Scripts/Game/InGame/InGameSceneController.cs`, `Assets/Scripts/Game/Boot/UIFactory.cs`, `Assets/Tests/EditMode/Game/InGameTests.cs`
  - Do:
    1. Rework InGameView — replace scoreButton/winButton/loseButton with placeCorrectButton/placeIncorrectButton, replace scoreText with heartsText + pieceCounterText, keep levelText
    2. Update UIFactory.CreateInGamePresenter — accept IHeartService and totalPieces, pass to constructor
    3. Update InGameSceneController — store IHeartService reference via Initialize, pass to CreateInGamePresenter, use totalPieces from GameSessionService (add TotalPieces field to session). Reset hearts before each attempt.
    4. Rework all InGamePresenter tests — test piece counting, auto-win, auto-lose, hearts display
    5. Rework InGameSceneController tests — adapt for new flow (no manual win/lose buttons)
  - Verify: All tests pass
  - Done when: Full reworked InGame flow tested end-to-end in edit-mode

## Files Likely Touched

- `Assets/Scripts/Game/InGame/IInGameView.cs`
- `Assets/Scripts/Game/InGame/InGameAction.cs`
- `Assets/Scripts/Game/InGame/InGamePresenter.cs`
- `Assets/Scripts/Game/InGame/InGameView.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Scripts/Game/Services/GameSessionService.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
