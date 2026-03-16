---
id: M004
provides:
  - GameSessionService for scene-to-scene context passing
  - ProgressionService for in-memory level tracking
  - InGame scene with score counter, win/lose triggers
  - WinDialog and LoseDialog popups with distinct views/presenters
  - Full game loop — menu → play → outcome → menu reflects progress
  - Play-from-editor bootstrapping for InGame scene
key_decisions:
  - D029: Context passing via shared GameSessionService, not RunAsync params
  - D030: Distinct popups (WinDialog, LoseDialog) for distinct dialogs
  - D031: In-memory progression only — no disk persistence
  - D032: Play-from-editor fallback via serialized _defaultLevelId field
patterns_established:
  - Service-mediated context passing between scene controllers
  - Awaitable popup results in scene controller flow (WaitForContinue, WaitForChoice)
  - Retry loop within RunAsync via fresh presenter creation
  - Play-from-editor fallback via serialized default fields on scene controllers
observability_surfaces:
  - "[ProgressionService] Level N complete — score: X" log on every win
  - "[GameBootstrapper] Boot sequence started" and "Infrastructure ready" logs
requirement_outcomes:
  - id: R029
    from_status: active
    to_status: validated
    proof: GameSessionService holds level/score/outcome; 7 edit-mode tests pass; InGameSceneController reads from it in RunAsync
  - id: R030
    from_status: active
    to_status: validated
    proof: ProgressionService tracks level, advances on win, logs score; 5 edit-mode tests pass
  - id: R031
    from_status: active
    to_status: validated
    proof: MainMenuPresenter reads CurrentLevel, displays "Level N", Play sets session context; 5 tests (DemoWiringTests)
  - id: R032
    from_status: active
    to_status: validated
    proof: InGameSceneController reads level from GameSessionService; fallback via _defaultLevelId; 4 controller tests pass
  - id: R033
    from_status: active
    to_status: validated
    proof: InGamePresenter has score increment, win, lose actions; 10 edit-mode tests pass
  - id: R034
    from_status: active
    to_status: validated
    proof: WinDialogPresenter shows score + level, WaitForContinue resolves on click; 4 tests pass
  - id: R035
    from_status: active
    to_status: validated
    proof: LoseDialogPresenter shows score + level, WaitForChoice returns Retry or Back; 5 tests pass
  - id: R036
    from_status: active
    to_status: validated
    proof: InGameSceneController calls RegisterWin(score) before showing win popup; ProgressionService logs and advances; tested in controller + service tests
  - id: R037
    from_status: active
    to_status: validated
    proof: InGameSceneController has serialized _defaultLevelId; BootInjector loads Boot scene if missing; tested in InGameSceneControllerTests
  - id: R038
    from_status: active
    to_status: validated
    proof: GameBootstrapper handles ScreenId.InGame; full loop wired; 98/98 tests pass; play-mode UAT confirmed by user
  - id: R039
    from_status: active
    to_status: validated
    proof: WinDialog and LoseDialog are PopupId entries with own views/presenters; UnityPopupContainer wires both; 9 popup tests pass
  - id: R040
    from_status: active
    to_status: validated
    proof: 98/98 edit-mode tests pass — 32 Core + 66 Game; all new types have dedicated test fixtures
duration: 1 day
verification_result: passed
completed_at: 2026-03-16
---

# M004: Game Loop — Meta-Progression, Context Passing, Win/Lose Flow

**Extended the architecture sample into a working game loop: main menu shows level, launches gameplay, InGame scene runs score + win/lose, progression service records outcomes, menu reflects updated state on return.**

## What Happened

S01 laid the domain service foundation — GameSessionService for session context (level ID, score, outcome) and ProgressionService for level tracking with in-memory persistence. Both are plain C# with full test coverage.

S02 extended MainMenuPresenter to read current level from ProgressionService, display it, and wire a Play button that sets session context via GameSessionService before navigating to InGame.

S03 was the highest-risk slice — the InGame scene with score counter, win/lose buttons, and a scene controller that handles the full gameplay→outcome→navigation flow. InGameSceneController.RunAsync loops through gameplay actions, calls ProgressionService.RegisterWin on win, and returns ScreenId.MainMenu. Play-from-editor fallback uses a serialized default level ID.

S04 added WinDialog and LoseDialog as distinct popup types with their own view interfaces and presenters. WinDialog shows score + level with Continue. LoseDialog shows score + level with Retry/Back. These were integrated into InGameSceneController's outcome branches — the controller shows the appropriate popup via PopupManager, awaits the result, and branches on retry vs return.

S05 wired everything into the boot flow — GameBootstrapper handles ScreenId.InGame in its navigation loop, UIFactory creates InGame and popup presenters, UnityPopupContainer registers win/lose popup GameObjects, and SceneSetup creates the InGame scene with all required UI elements.

## Cross-Slice Verification

| Success Criterion | Verification | Result |
|---|---|---|
| Player starts from menu, sees level, presses Play | MainMenuPresenter tests (DemoWiringTests) | ✅ 25/25 pass |
| InGame shows level, score, win/lose buttons | InGamePresenterTests | ✅ 10/10 pass |
| Win registers score, shows popup, returns to menu with level advanced | InGameSceneControllerTests + WinDialogPresenterTests + ProgressionServiceTests | ✅ 13/13 pass |
| Lose shows popup with retry/back | LoseDialogPresenterTests + InGameSceneControllerTests | ✅ 9/9 pass |
| Menu reflects updated level after win | DemoWiringTests (Play_SetsSession_AndReturnsInGame, UpdatesLevelDisplay) | ✅ pass |
| InGame works from editor with fallback | InGameSceneControllerTests (fallback test) | ✅ pass |
| All 58 existing tests pass + new tests | 98/98 edit-mode tests pass | ✅ |
| No .Forget(), no static state | grep guards clean | ✅ |
| Full loop in play mode | User-confirmed play-mode UAT | ✅ |

## Requirement Changes

- R029: active → validated — GameSessionService proven by 7 unit tests + InGame/MainMenu integration
- R030: active → validated — ProgressionService proven by 5 unit tests + Debug.Log output
- R031: active → validated — MainMenu level display + Play button proven by DemoWiringTests
- R032: active → validated — InGame reads level from service; fallback via _defaultLevelId proven
- R033: active → validated — Score counter + win/lose triggers proven by InGamePresenterTests
- R034: active → validated — WinDialog shows score + level, Continue returns to menu; 4 tests
- R035: active → validated — LoseDialog Retry/Back flow proven by 5 tests
- R036: active → validated — RegisterWin called before popup; service logs score
- R037: active → validated — Play-from-editor with serialized default level proven
- R038: active → validated — Full loop integrated in GameBootstrapper; 98/98 tests pass
- R039: active → validated — WinDialog/LoseDialog as distinct PopupId entries with views/presenters
- R040: active → validated — 98/98 edit-mode tests; all new types covered

## Forward Intelligence

### What the next milestone should know
- The architecture now carries a real game loop. Any future milestone adding features (new game modes, persistence, real level content) should start by reading GameBootstrapper.cs and the service layer (GameSessionService, ProgressionService).
- InGameSceneController.RunAsync is the most complex flow in the codebase — it has retry logic via fresh presenter creation. Understand this pattern before extending it.
- UIFactory.cs is the central wiring point. Any new presenter type needs a Create method here and corresponding GameBootstrapper construction.

### What's fragile
- InGameSceneController creates fresh InGamePresenters on retry — this works but is unusual. If presenters gain expensive setup, this pattern could cause issues.
- UnityPopupContainer switch statement grows linearly with popup count. At 5+ popups, consider a dictionary-based registry.
- SceneSetup editor script creates scenes procedurally — if someone hand-edits scenes, running SceneSetup again may overwrite changes.

### Authoritative diagnostics
- `[ProgressionService] Level N complete — score: X` in console — confirms win flow executed
- 98/98 test count in TestResults.xml — any regression drops this number
- `grep -rn ".Forget()" Assets/Scripts/` must return empty — fire-and-forget violations

### What assumptions changed
- Expected 89 tests initially due to domain-reload-disabled editor not detecting PopupTests.cs — after editor recompilation triggered, all 98 detected and pass
- The static state grep guard flags `DetectAlreadyLoadedScreen()` as a false positive — it's a pure function reading scene state, not mutable static state

## Files Created/Modified

### New — Services
- `Assets/Scripts/Game/Services/GameSessionService.cs` — session context (level, score, outcome)
- `Assets/Scripts/Game/Services/ProgressionService.cs` — in-memory level tracking
- `Assets/Scripts/Game/Services/GameOutcome.cs` — enum: None, Win, Lose

### New — InGame
- `Assets/Scripts/Game/InGame/InGameAction.cs` — enum: IncrementScore, Win, Lose
- `Assets/Scripts/Game/InGame/IInGameView.cs` — view interface
- `Assets/Scripts/Game/InGame/InGameView.cs` — MonoBehaviour view
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — score tracking, action resolution
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — gameplay loop with popup integration

### New — Popups
- `Assets/Scripts/Game/Popup/IWinDialogView.cs` — win popup view interface
- `Assets/Scripts/Game/Popup/WinDialogView.cs` — win popup MonoBehaviour
- `Assets/Scripts/Game/Popup/WinDialogPresenter.cs` — win popup presenter
- `Assets/Scripts/Game/Popup/ILoseDialogView.cs` — lose popup view interface
- `Assets/Scripts/Game/Popup/LoseDialogView.cs` — lose popup MonoBehaviour
- `Assets/Scripts/Game/Popup/LoseDialogPresenter.cs` — lose popup presenter
- `Assets/Scripts/Game/Popup/LoseDialogChoice.cs` — enum: Retry, Back

### Modified — Boot/Integration
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — InGame nav case, service fields
- `Assets/Scripts/Game/Boot/UIFactory.cs` — new Create methods for InGame + popup presenters
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — win/lose popup support
- `Assets/Scripts/Game/ScreenId.cs` — added InGame
- `Assets/Scripts/Game/PopupId.cs` — added WinDialog, LoseDialog
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — level display + Play action
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — UpdateLevelDisplay, OnPlayClicked
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — added Play
- `Assets/Editor/SceneSetup.cs` — InGame scene creation + Boot/MainMenu updates

### New — Tests
- `Assets/Tests/EditMode/Game/GameSessionServiceTests.cs` — 7 tests
- `Assets/Tests/EditMode/Game/ProgressionServiceTests.cs` — 5 tests
- `Assets/Tests/EditMode/Game/InGameTests.cs` — 10 InGamePresenter + 4 InGameSceneController tests
- `Assets/Tests/EditMode/Game/PopupTests.cs` — 4 WinDialog + 5 LoseDialog tests
