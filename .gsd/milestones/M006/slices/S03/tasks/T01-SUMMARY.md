---
id: T01
parent: S03
milestone: M006
provides:
  - Reworked InGameAction enum — PlaceCorrect, PlaceIncorrect, Win, Lose
  - Reworked IInGameView — OnPlaceCorrect, OnPlaceIncorrect, UpdateHearts, UpdatePieceCounter, UpdateLevelLabel
  - Reworked InGamePresenter — hearts + piece counting + auto-win/lose + interstitial ad debug log
requires:
  - S02 provides IHeartService + HeartService
affects: [T02, S04]
key_files:
  - Assets/Scripts/Game/InGame/InGameAction.cs
  - Assets/Scripts/Game/InGame/IInGameView.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
duration: 10min
verification_result: pass
completed_at: 2026-03-17T13:45:00Z
---

# T01: Rework IInGameView, InGamePresenter, InGameAction

**Replaced manual win/lose buttons with piece placement + hearts system in InGame presenter**

## What Happened

Reworked InGameAction: removed IncrementScore, kept Win/Lose (now auto-resolved), added PlaceCorrect/PlaceIncorrect as player inputs.

Reworked IInGameView: replaced OnScoreClicked/OnWinClicked/OnLoseClicked with OnPlaceCorrect/OnPlaceIncorrect. Added UpdateHearts(string) and UpdatePieceCounter(string). Kept UpdateLevelLabel(string).

Reworked InGamePresenter: constructor takes IInGameView + GameSessionService + IHeartService + totalPieces + initialHearts(default 3). Initialize resets hearts, sets piece counter to "0/N". PlaceCorrect increments pieces, auto-resolves Win when pieces == total. PlaceIncorrect calls UseHeart, auto-resolves Lose when !IsAlive. Debug.Log "[Ads] Interstitial ad opportunity" fires at both win and lose.
