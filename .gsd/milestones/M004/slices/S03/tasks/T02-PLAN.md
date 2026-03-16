# T02: InGameSceneController + InGameView + UIFactory + play-from-editor + tests

**Slice:** S03
**Milestone:** M004

## Goal
Create the scene controller that runs the gameplay loop, the view MonoBehaviour, wire into UIFactory, and add play-from-editor fallback.

## Must-Haves

### Truths
- InGameSceneController.RunAsync: reads level from GameSessionService, runs gameplay loop, Win → RegisterWin + return MainMenu, Lose → return MainMenu (popup handling deferred to S04)
- InGameSceneController has serialized _defaultLevelId for play-from-editor fallback
- InGameSceneController.Initialize accepts UIFactory + ProgressionService + GameSessionService
- UIFactory.CreateInGamePresenter exists and works
- InGameView MonoBehaviour wires buttons to IInGameView events
- Scene controller tests prove: win returns MainMenu, lose returns MainMenu, score increments work through flow

### Artifacts
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — scene controller
- `Assets/Scripts/Game/InGame/InGameView.cs` — view MonoBehaviour
- Updated `Assets/Scripts/Game/Boot/UIFactory.cs`

### Key Links
- InGameSceneController → UIFactory (creates presenter)
- InGameSceneController → ProgressionService (calls RegisterWin)
- InGameSceneController → GameSessionService (reads level, sets outcome)

## Steps
1. Create InGameView MonoBehaviour
2. Create InGameSceneController with RunAsync loop and play-from-editor fallback
3. Update UIFactory with CreateInGamePresenter
4. Write scene controller tests
5. Run all tests

## Context
- For now, Win just calls RegisterWin and returns MainMenu. Lose just returns MainMenu. Popup integration comes in S04.
- The scene controller sets GameSessionService.Outcome before returning, so the caller knows what happened.
- Play-from-editor: if GameSessionService.CurrentLevelId == 0 (not initialized), use _defaultLevelId
