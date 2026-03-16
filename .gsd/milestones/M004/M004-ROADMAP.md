# M004: Game Loop — Meta-Progression, Context Passing, Win/Lose Flow

**Vision:** Extend the architecture sample into a working game loop: main menu shows level, launches gameplay, InGame scene runs score + win/lose, progression service records outcomes, menu reflects updated state on return.

## Success Criteria

- Player can start from main menu, see current level, press Play, and enter InGame scene
- InGame scene shows level info, score counter with increment button, Win button, Lose button
- Winning registers the score via progression service, shows win popup (score + level), returns to menu with level advanced
- Losing shows lose popup (score + level) with Retry (resets score, replays) and Back (returns to menu)
- Main menu reflects updated level after winning
- InGame scene works when played directly from editor with fallback level config
- All 58 existing tests pass, plus new tests covering all new types

## Key Risks / Unknowns

- Context passing via shared service — new pattern; initialization ordering between Boot and scene controller
- InGame scene controller complexity — gameplay loop + win/lose branching + retry in one RunAsync
- Two new popup types wired into Boot scene — more popup infrastructure than previously proven

## Proof Strategy

- Context passing via service → retire in S01 by proving GameSessionService read/write in edit-mode tests, then in S03 by proving InGame scene controller reads level from service
- InGame controller complexity → retire in S03 by proving the full RunAsync flow (gameplay → outcome → return/retry) in edit-mode tests
- New popup wiring → retire in S04 by proving WinDialog/LoseDialog show/dismiss through PopupManager in edit-mode tests

## Verification Classes

- Contract verification: edit-mode NUnit tests for all services, presenters, scene controllers
- Integration verification: GameBootstrapper navigation loop handles InGame ↔ MainMenu; popup container registers win/lose
- Operational verification: full game loop in Unity Editor play mode
- UAT / human verification: play the full loop in editor — menu → play → win → menu (level advanced)

## Milestone Definition of Done

This milestone is complete only when all are true:

- All slice deliverables are complete
- GameBootstrapper navigation loop handles MainMenu ↔ InGame with proper context passing
- Win/Lose popups are wired in Boot scene and work through PopupManager
- UIFactory creates all new presenters with correct dependency injection
- Full game loop works in Unity Editor play mode: menu → play → outcome → menu reflects progress
- InGame scene works when started directly from editor
- All existing 58 tests pass + new tests for all new types
- No `.Forget()`, no static state, no backward view references

## Requirement Coverage

- Covers: R029, R030, R031, R032, R033, R034, R035, R036, R037, R038, R039, R040
- Partially covers: R001, R004, R007, R008, R010, R016 (extends existing proven patterns)
- Leaves for later: R041 (disk persistence), R042 (level-specific content)
- Orphan risks: none

## Slices

- [ ] **S01: Game Session & Progression Services** `risk:medium` `depends:[]`
  > After this: edit-mode tests prove GameSessionService holds level/score/outcome context and ProgressionService tracks level, advances on win, logs score. No UI yet — services are pure C# with full test coverage.

- [ ] **S02: Main Menu — Level Display & Play Button** `risk:low` `depends:[S01]`
  > After this: MainMenuPresenter reads current level from ProgressionService, view shows "Level N", Play button sets session context and returns ScreenId.InGame. Edit-mode tests prove the wiring.

- [ ] **S03: InGame Scene — Gameplay & Outcome Flow** `risk:high` `depends:[S01,S02]`
  > After this: InGame scene exists with InGameSceneController. Score counter increments, Win calls progression service, Lose sets outcome. Scene controller returns ScreenId.MainMenu (win or back) or retries (lose+retry). Edit-mode tests prove the full RunAsync flow. InGame scene has play-from-editor fallback.

- [ ] **S04: Win & Lose Popups** `risk:medium` `depends:[S03]`
  > After this: WinDialog and LoseDialog are new PopupId entries with views/presenters. Win popup shows score + level with Continue. Lose popup shows score + level with Retry/Back. Both wired into PopupManager. Edit-mode tests prove popup presenter behavior.

- [ ] **S05: Full Loop Integration & Polish** `risk:low` `depends:[S04]`
  > After this: GameBootstrapper handles InGame case. UIFactory creates all new presenters. UnityPopupContainer wires win/lose popups. Boot scene has popup GameObjects. Complete loop works in play mode: Menu (Level 1) → Play → InGame → score up → Win → popup → Menu (Level 2). Lose → popup → Retry → Win → Level 3.

## Boundary Map

### S01 → S02

Produces:
- `Game/Services/ProgressionService.cs` → `CurrentLevel` (int, read), `RegisterWin(int score)` (advances level, logs score), `event Action<int> OnLevelChanged` (optional reactive)
- `Game/Services/GameSessionService.cs` → `CurrentLevelId` (int, read/write), `CurrentScore` (int, read/write), `GameOutcome` (enum, read/write), `ResetForNewGame(int levelId)`

Consumes:
- nothing (first slice)

### S01 → S03

Produces:
- `Game/Services/ProgressionService.cs` → `CurrentLevel`, `RegisterWin(int score)`
- `Game/Services/GameSessionService.cs` → `CurrentLevelId`, `CurrentScore`, `GameOutcome`, `ResetForNewGame(int levelId)`
- `Game/Services/GameOutcome.cs` → enum: `None`, `Win`, `Lose`

Consumes:
- nothing (first slice)

### S02 → S03

Produces:
- `Game/MainMenu/MainMenuAction.cs` → adds `Play` to enum
- `Game/MainMenu/IMainMenuView.cs` → adds `event Action OnPlayClicked`, `void UpdateLevelDisplay(string text)`
- MainMenuPresenter sets session context (`GameSessionService.ResetForNewGame(level)`) before returning `ScreenId.InGame`

Consumes from S01:
- `ProgressionService.CurrentLevel` — to display current level
- `GameSessionService.ResetForNewGame(int)` — to set up session before navigating to InGame

### S03 → S04

Produces:
- `Game/InGame/InGameSceneController.cs` → reads session context, runs gameplay loop, calls `ProgressionService.RegisterWin()` on win, sets `GameSessionService.GameOutcome`
- `Game/InGame/InGamePresenter.cs` → `WaitForAction()` → `InGameAction` (IncrementScore, Win, Lose)
- `Game/InGame/IInGameView.cs` → `event Action OnScoreClicked`, `OnWinClicked`, `OnLoseClicked`; `UpdateScore(string)`, `UpdateLevelLabel(string)`
- `Game/ScreenId.cs` → adds `InGame` value
- InGameSceneController exposes outcome flow hooks that S04 will wire popup display into

Consumes from S01:
- `GameSessionService` — reads `CurrentLevelId`, writes `CurrentScore`, `GameOutcome`
- `ProgressionService` — calls `RegisterWin(score)`

### S04 → S05

Produces:
- `Game/Popup/WinDialogPresenter.cs` → `Initialize(int score, int level)`, `WaitForContinue()` → UniTask
- `Game/Popup/IWinDialogView.cs` → `event Action OnContinueClicked`, `UpdateScore(string)`, `UpdateLevel(string)`
- `Game/Popup/LoseDialogPresenter.cs` → `Initialize(int score, int level)`, `WaitForChoice()` → UniTask<LoseDialogChoice>` (Retry or Back)
- `Game/Popup/ILoseDialogView.cs` → `event Action OnRetryClicked`, `OnBackClicked`, `UpdateScore(string)`, `UpdateLevel(string)`
- `Game/Popup/LoseDialogChoice.cs` → enum: `Retry`, `Back`
- `Game/PopupId.cs` → adds `WinDialog`, `LoseDialog`
- InGameSceneController updated to show win/lose popups via PopupManager after outcome

Consumes from S03:
- InGameSceneController outcome flow — S04 integrates popup display into the win/lose branches

### S05 consumes all

Consumes from S01–S04:
- All services, presenters, views, scene controllers, popup types
- Wires them into GameBootstrapper navigation loop, UIFactory, UnityPopupContainer, Boot scene
- Produces: the fully integrated, working game loop
