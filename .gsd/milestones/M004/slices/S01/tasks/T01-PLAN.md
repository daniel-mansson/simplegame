# T01: GameSessionService, ProgressionService, GameOutcome enum + tests

**Slice:** S01
**Milestone:** M004

## Goal
Implement the two domain services and outcome enum that underpin the game loop, with full edit-mode test coverage.

## Must-Haves

### Truths
- GameSessionService.ResetForNewGame(levelId) sets CurrentLevelId and resets score to 0 and outcome to None
- GameSessionService.CurrentScore can be read/written
- GameSessionService.GameOutcome can be read/written
- ProgressionService starts at level 1
- ProgressionService.RegisterWin(score) increments CurrentLevel by 1
- ProgressionService.RegisterWin(score) logs the score via Debug.Log
- All existing 58 tests still pass

### Artifacts
- `Assets/Scripts/Game/Services/GameSessionService.cs` — session context (min 20 lines)
- `Assets/Scripts/Game/Services/ProgressionService.cs` — progression tracking (min 20 lines)
- `Assets/Scripts/Game/Services/GameOutcome.cs` — enum: None, Win, Lose
- `Assets/Tests/EditMode/Game/GameServiceTests.cs` — tests for both services (min 8 tests)

### Key Links
- GameSessionService uses GameOutcome enum
- Tests import both services and exercise full API

## Steps
1. Create GameOutcome enum in Game/Services/
2. Create GameSessionService with CurrentLevelId, CurrentScore, GameOutcome properties and ResetForNewGame method
3. Create ProgressionService with CurrentLevel property and RegisterWin method
4. Write edit-mode tests in GameServiceTests.cs covering all truths
5. Run tests — verify all pass including existing 58

## Context
- Services go in `Assets/Scripts/Game/Services/` namespace `SimpleGame.Game.Services`
- Follow established pattern: plain C# classes, constructor injection, no static state
- ProgressionService Debug.Log format should be grep-friendly for verification
- GameService.cs already exists as a placeholder — leave it alone for now, it's used by existing presenters
