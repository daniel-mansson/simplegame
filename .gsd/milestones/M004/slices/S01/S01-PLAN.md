# S01: Game Session & Progression Services

**Goal:** Create the two domain services that underpin the entire game loop — GameSessionService (context passing) and ProgressionService (meta-progression) — with full edit-mode test coverage.
**Demo:** Edit-mode tests prove: GameSessionService holds level/score/outcome context with reset; ProgressionService tracks current level, advances on win, logs score via Debug.Log.

## Must-Haves
- GameSessionService: CurrentLevelId (int r/w), CurrentScore (int r/w), GameOutcome (enum r/w), ResetForNewGame(int levelId)
- ProgressionService: CurrentLevel (int, starts at 1), RegisterWin(int score) advances level and logs score
- GameOutcome enum: None, Win, Lose
- All types are plain C# — no MonoBehaviour, no Unity dependencies
- Edit-mode tests cover all public API surface

## Tasks

- [x] **T01: GameSessionService, ProgressionService, GameOutcome enum + tests**
  Implement both services and the outcome enum. Write edit-mode tests covering all read/write/reset/advance behavior.

## Files Likely Touched
- Assets/Scripts/Game/Services/GameSessionService.cs (new)
- Assets/Scripts/Game/Services/ProgressionService.cs (new)
- Assets/Scripts/Game/Services/GameOutcome.cs (new)
- Assets/Tests/EditMode/Game/GameServiceTests.cs (new)
