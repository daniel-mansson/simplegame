---
id: S03
milestone: M006
provides:
  - Reworked InGame scene — piece placement + hearts instead of manual win/lose buttons
  - Auto-win at totalPieces placed, auto-lose at 0 hearts
  - Interstitial ad debug log at win and lose
  - GameSessionService.TotalPieces for per-level piece count
  - UIFactory accepts IHeartService
  - 14 presenter tests + 4 controller tests
key_files:
  - Assets/Scripts/Game/InGame/IInGameView.cs
  - Assets/Scripts/Game/InGame/InGameAction.cs
  - Assets/Scripts/Game/InGame/InGamePresenter.cs
  - Assets/Scripts/Game/InGame/InGameView.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Boot/UIFactory.cs
  - Assets/Scripts/Game/Services/GameSessionService.cs
  - Assets/Tests/EditMode/Game/InGameTests.cs
key_decisions:
  - "Presenter auto-resolves win/lose based on game state — no manual win/lose buttons"
  - "UIFactory takes optional IHeartService to avoid breaking existing test sites"
  - "GameSessionService carries TotalPieces — caller sets before navigation, controller reads"
patterns_established:
  - "Auto-resolving presenter outcome — WaitForAction resolves automatically on terminal state"
drill_down_paths:
  - .gsd/milestones/M006/slices/S03/tasks/T01-SUMMARY.md
  - .gsd/milestones/M006/slices/S03/tasks/T02-SUMMARY.md
verification_result: pass
completed_at: 2026-03-17T13:50:00Z
---

# S03: Stub gameplay screen with hearts

**Reworked InGame scene: piece placement with auto-win/lose, hearts system, interstitial ad stub, 18 tests**

## What Happened

Replaced the manual score/win/lose button gameplay with piece placement and hearts:
- PlaceCorrect increments piece counter, auto-resolves Win when all pieces placed
- PlaceIncorrect costs one heart via IHeartService, auto-resolves Lose at 0 hearts
- Debug.Log fires "[Ads] Interstitial ad opportunity" at both win and lose

Updated supporting infrastructure:
- GameSessionService gains TotalPieces field
- UIFactory accepts optional IHeartService, CreateInGamePresenter takes totalPieces
- InGameSceneController simplified — no inner loop, presenter auto-resolves

14 presenter tests + 4 controller tests cover: initialization, piece counting, auto-win, auto-lose, mixed actions, dispose, retry flow, play-from-editor.

## Tasks Completed
- T01: Rework IInGameView, InGamePresenter, InGameAction
- T02: Rework InGameView, InGameSceneController, UIFactory, and tests
