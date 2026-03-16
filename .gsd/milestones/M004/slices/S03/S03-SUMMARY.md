---
id: S03
milestone: M004
status: complete
tasks_complete: 2
tests_added: 14
tests_total: 89
---

# S03: InGame Scene — Gameplay & Outcome Flow

**Full InGame MVP stack: presenter with score tracking + scene controller with win/lose/retry flow + play-from-editor — 14 new tests, 89/89 total**

## What Was Delivered

Complete InGame scene infrastructure:
- `InGameAction` enum (IncrementScore, Win, Lose)
- `IInGameView` interface with score/win/lose events and update methods
- `InGamePresenter` manages score internally, writes to GameSessionService, resolves WaitForAction only on Win/Lose
- `InGameSceneController` reads level from session (or default for play-from-editor), runs gameplay loop, calls ProgressionService.RegisterWin on win, sets GameOutcome
- `InGameView` MonoBehaviour wires buttons
- `UIFactory.CreateInGamePresenter` added

## Key Files
- `Assets/Scripts/Game/InGame/InGameAction.cs` — action enum
- `Assets/Scripts/Game/InGame/IInGameView.cs` — view interface
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — presenter with score tracking
- `Assets/Scripts/Game/InGame/InGameView.cs` — view MonoBehaviour
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — scene controller
- `Assets/Scripts/Game/Boot/UIFactory.cs` — CreateInGamePresenter added
- `Assets/Tests/EditMode/Game/InGameTests.cs` — 14 tests

## Boundary Outputs (for downstream slices)
- `InGameSceneController` — RunAsync handles Win (RegisterWin + return MainMenu) and Lose (return MainMenu)
- `GameSessionService.Outcome` is set (Win/Lose) before returning — S04 can read it for popup display
- `GameSessionService.CurrentScore` is written throughout gameplay
- Play-from-editor fallback via `_defaultLevelId` serialized field

## Key Decisions
- Win/Lose both return ScreenId.MainMenu for now — popup integration deferred to S04
- Scene controller sets GameSessionService.Outcome before returning, enabling the caller (or S04 popup logic) to know the result
- Play-from-editor: if CurrentLevelId == 0 (uninitialized), uses serialized _defaultLevelId (defaults to 1)

## Verification
- 89/89 edit-mode tests pass (75 + 14 new)
