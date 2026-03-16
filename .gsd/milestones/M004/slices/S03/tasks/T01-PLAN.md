# T01: InGame types — enum, view interface, presenter + tests

**Slice:** S03
**Milestone:** M004

## Goal
Create the InGame MVP types and prove presenter behavior in edit-mode tests.

## Must-Haves

### Truths
- InGameAction enum has IncrementScore, Win, Lose values
- IInGameView has OnScoreClicked, OnWinClicked, OnLoseClicked events + UpdateScore, UpdateLevelLabel methods
- InGamePresenter accepts GameSessionService; Initialize sets score to "0" and level label
- Score click increments internal score and updates view
- WaitForAction resolves Win on win click, Lose on lose click
- Score clicks don't resolve WaitForAction — only Win and Lose do
- Dispose cancels pending task and unsubscribes events

### Artifacts
- `Assets/Scripts/Game/InGame/InGameAction.cs` — enum
- `Assets/Scripts/Game/InGame/IInGameView.cs` — view interface
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — presenter (min 40 lines)
- `Assets/Tests/EditMode/Game/InGameTests.cs` — tests (min 8 tests)

### Key Links
- InGamePresenter → GameSessionService (reads CurrentLevelId, writes CurrentScore)
- Tests use MockInGameView

## Steps
1. Create InGameAction enum
2. Create IInGameView interface
3. Create InGamePresenter — score tracking, WaitForAction for Win/Lose only
4. Create MockInGameView and tests
5. Run all tests

## Context
- Presenter owns score state internally, writes to GameSessionService.CurrentScore before resolving Win/Lose
- WaitForAction only resolves on Win or Lose — score clicks are handled inline without resolving the TCS
- Follow D026: awaitable result methods
- Follow D027: event Action from view
