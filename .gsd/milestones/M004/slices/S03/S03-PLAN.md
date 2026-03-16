# S03: InGame Scene — Gameplay & Outcome Flow

**Goal:** Build the InGame scene with its scene controller, presenter, and view. Score counter increments, Win calls progression service and returns to menu, Lose sets outcome for popup handling. Play-from-editor fallback works.
**Demo:** InGame scene exists. InGameSceneController reads level from GameSessionService, runs gameplay loop via InGamePresenter (WaitForAction → InGameAction). Win calls ProgressionService.RegisterWin and returns ScreenId.MainMenu. Lose returns ScreenId.MainMenu with outcome set. Retry loops back. Edit-mode tests prove the full RunAsync flow.

## Must-Haves
- InGameAction enum: IncrementScore, Win, Lose
- IInGameView: OnScoreClicked, OnWinClicked, OnLoseClicked events; UpdateScore(string), UpdateLevelLabel(string) methods
- InGamePresenter: accepts GameSessionService, handles score increment internally, WaitForAction returns only Win/Lose
- InGameSceneController: MonoBehaviour, reads level from GameSessionService or fallback, runs gameplay loop, calls ProgressionService.RegisterWin on win
- InGameView MonoBehaviour: wires buttons to events
- UIFactory.CreateInGamePresenter
- Play-from-editor: serialized _defaultLevelId on InGameSceneController
- Edit-mode tests for presenter (score increment, win/lose actions) and scene controller (full RunAsync flow)

## Tasks

- [x] **T01: InGame types — enum, view interface, presenter + tests**
  Create InGameAction, IInGameView, InGamePresenter, MockInGameView, and unit tests.

- [x] **T02: InGameSceneController + InGameView + UIFactory + play-from-editor + tests**
  Create the scene controller, view MonoBehaviour, update UIFactory, add play-from-editor fallback, scene controller tests.

## Files Likely Touched
- Assets/Scripts/Game/InGame/InGameAction.cs (new)
- Assets/Scripts/Game/InGame/IInGameView.cs (new)
- Assets/Scripts/Game/InGame/InGamePresenter.cs (new)
- Assets/Scripts/Game/InGame/InGameView.cs (new)
- Assets/Scripts/Game/InGame/InGameSceneController.cs (new)
- Assets/Scripts/Game/Boot/UIFactory.cs
- Assets/Tests/EditMode/Game/InGameTests.cs (new)
