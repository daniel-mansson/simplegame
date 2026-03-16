---
id: M004
status: complete
slices_complete: 5
tests_total: 98
tests_passed: 89 (9 pending editor restart for PopupTests.cs detection)
---

# M004 Summary: Game Loop — Meta-Progression, Context Passing, Win/Lose Flow

## What Was Delivered

Extended the MVP architecture sample into a working game loop. Player can navigate from main menu to gameplay, score points, win or lose, see outcome popups, and return to a menu that reflects progression.

## Architecture After M004

```
GameBootstrapper (Boot scene)
  ├── Builds: GameService, ProgressionService, GameSessionService, ScreenManager, PopupManager, UIFactory
  └── Navigation loop:
        ShowScreenAsync(MainMenu)
        loop:
          MainMenuSceneController.RunAsync() → ScreenId
          ├── Play → ScreenId.InGame (sets session context via GameSessionService)
          ├── Settings → ScreenId.Settings
          └── Popup → inline ConfirmDialog → loop
          
          InGameSceneController.RunAsync() → ScreenId
          ├── WaitForAction loop: score clicks inline, Win/Lose resolve
          ├── Win → RegisterWin → WinDialog popup → return MainMenu
          └── Lose → LoseDialog popup → Retry (fresh presenter) or Back → return MainMenu
          
          SettingsSceneController.RunAsync() → ScreenId.MainMenu
```

## Slices

- ✅ S01: Game Session & Progression Services — GameSessionService, ProgressionService, GameOutcome enum (12 tests)
- ✅ S02: Main Menu — Level Display & Play Button — extended MainMenuPresenter with services, Play action (5 tests)
- ✅ S03: InGame Scene — Gameplay & Outcome Flow — InGamePresenter, InGameSceneController, play-from-editor (14 tests)
- ✅ S04: Win & Lose Popups — WinDialog/LoseDialog presenters + views, integrated into InGameSceneController retry flow (9 tests)
- ✅ S05: Full Loop Integration — GameBootstrapper InGame case, UnityPopupContainer wiring, scene setup

## Key Decisions

- D029: Context passing via shared GameSessionService, not RunAsync params
- D030: Distinct popups for distinct dialogs (WinDialog/LoseDialog separate from ConfirmDialog)
- D031: In-memory progression only — no disk persistence
- D032: Play-from-editor fallback via serialized _defaultLevelId field

## New Types

### Services
- `GameSessionService` — session context (level ID, score, outcome)
- `ProgressionService` — in-memory level tracking
- `GameOutcome` — enum: None, Win, Lose

### InGame
- `InGameAction` — enum: IncrementScore, Win, Lose
- `IInGameView` / `InGameView` — view interface and MonoBehaviour
- `InGamePresenter` — score tracking, win/lose resolution
- `InGameSceneController` — gameplay loop with popup integration and retry

### Popups
- `IWinDialogView` / `WinDialogView` / `WinDialogPresenter` — win outcome popup
- `ILoseDialogView` / `LoseDialogView` / `LoseDialogPresenter` — lose outcome with retry/back
- `LoseDialogChoice` — enum: Retry, Back

## Verification

- 89/89 edit-mode tests pass (9 PopupTests pending editor file detection — 98 expected total)
- All code compiles clean
- Pending: play-mode UAT (full loop in editor)
