# M006: Puzzle Tap Game Skeleton

**Gathered:** 2026-03-17
**Status:** Ready for planning

## Project Description

Build the full Puzzle Tap game skeleton on top of the existing MVP architecture (M001–M005). This means every screen, popup, domain service, data model, and persistence layer needed for the complete game flow — wired together with simple text-box stub visuals. No real art, no real puzzle rendering, no real ad/IAP SDKs. After this milestone, a player can tap through the entire flow: main screen → play level → win/lose → earn golden pieces → spend them restoring objects → unlock environments → repeat.

The core gameplay is deliberately minimal: level ID, piece counter, place-correct/place-incorrect buttons, hearts. Real puzzle mechanics (board, tray, neighbor validation, camera) come in a future milestone. The user will manually implement core gameplay and fix visuals later — this milestone provides the scaffolding and mocks.

## Why This Milestone

The GDD describes a full mobile puzzle game with meta-progression. Before any real art or gameplay can be built, the entire screen flow and data model must exist. This skeleton proves the architecture carries the full Puzzle Tap design — screens, popups, services, persistence, and progression — so that future milestones can focus on one concern at a time (real gameplay, real art, real monetization) without restructuring the flow.

## User-Visible Outcome

### When this milestone is complete, the user can:

- See the main screen showing an environment with restorable objects, golden piece balance, and a play button
- Tap play to enter a stub level with hearts (3), piece counter, place-correct/place-incorrect buttons
- Win a level (all pieces placed) or lose (all hearts gone), see appropriate popups
- Earn golden pieces on level complete, see them added to balance
- Tap restorable objects on the main screen to spend golden pieces (one tap = one step)
- See blocked objects that can't be tapped until dependencies are restored
- See an object-restored celebration popup when restoration completes
- Complete all objects in an environment to unlock the next
- Tap through rewarded ad and IAP purchase stub popups
- Quit and restart — meta progress persists via PlayerPrefs

### Entry point / environment

- Entry point: Play mode in Unity Editor, starting from Boot scene
- Environment: local dev / Unity Editor
- Live dependencies involved: none

## Completion Class

- Contract complete means: all services tested in edit-mode, all presenters tested, persistence round-trips, ScriptableObject data loads correctly
- Integration complete means: full flow navigable in play mode end-to-end
- Operational complete means: persistence survives play-mode restart

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- The full flow is navigable end-to-end in play mode: main screen → play → win/lose → earn golden pieces → spend on objects → restore → unlock next environment → play again
- Meta progression persists across play-mode restarts
- All popups (LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored, ConfirmDialog) are functional with stub UI
- All stub visuals are text boxes — no real art required

## Risks and Unknowns

- ScriptableObjects are new to this project — need to establish the pattern for data authoring and runtime access
- The main screen becomes significantly more complex than the current MainMenu — environment rendering with tappable objects as stub UI needs careful layout
- Persistence via PlayerPrefs for structured meta-world state (per-object progress across environments) — JSON serialization of the save data is the likely approach
- The number of popups grows from 3 to 6 — UnityPopupContainer and PopupId enum need to scale

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/ScreenId.cs` — InGame stays, MainMenu stays (renamed conceptually to "main screen")
- `Assets/Scripts/Game/PopupId.cs` — needs new entries: LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — navigation loop with switch on ScreenId, service construction
- `Assets/Scripts/Game/Boot/UIFactory.cs` — central presenter factory, needs new Create methods
- `Assets/Scripts/Game/Services/ProgressionService.cs` — in-memory level tracking, needs rework for golden pieces and persistence
- `Assets/Scripts/Game/Services/GameSessionService.cs` — session context passing between scenes
- `Assets/Scripts/Game/InGame/` — existing InGame screen, needs rework for hearts + piece counter
- `Assets/Scripts/Game/MainMenu/` — existing MainMenu, needs major rework into main screen with meta world
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — pre-instantiated popup switching, needs new popup entries
- `Assets/Editor/SceneSetup.cs` — programmatic scene creation, needs updates for new UI elements
- `Assets/Scripts/Core/` — framework layer (ScreenManager, PopupManager, MVP base) — should NOT change

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R045 — Main screen with environment, objects, balance, play button
- R046 — Stub gameplay with hearts and piece counter
- R047 — Meta world data model via ScriptableObjects
- R048 — Golden pieces earned and spent
- R049 — Meta persistence via PlayerPrefs
- R050 — Environment unlocking (1–3 available, all must complete)
- R051 — LevelComplete popup
- R052 — LevelFailed popup with ad/IAP stubs
- R053 — Rewarded ad stub popup
- R054 — IAP purchase stub popup
- R055 — Object restored celebration popup
- R056 — Interstitial ad debug log stub
- R057 — Heart system (3 per level)
- R058 — Full navigable flow
- R059 — All views are text-box stubs

## Scope

### In Scope

- Meta world data model (ScriptableObjects: WorldData, EnvironmentData, RestorableObjectData)
- Meta persistence service (interface + PlayerPrefs implementation)
- Currency service (golden pieces — earn, spend, balance, persist)
- Heart service (per-level, 3 hearts, decrement, reset)
- Rework MainMenu into main screen with environment view and object restoration
- Rework InGame into stub gameplay with hearts and piece counter
- Rework WinDialog/LoseDialog into LevelComplete/LevelFailed
- New popups: RewardedAd, IAPPurchase, ObjectRestored
- Rework ConfirmDialog for actual game use
- Environment progression (complete all objects → unlock next)
- Interstitial ad stub (debug log at win/lose)
- Test data: at least 2 environments with 4+ objects total
- Edit-mode tests for all new services and presenters

### Out of Scope / Non-Goals

- Real puzzle rendering (board, pieces, tray, neighbor validation, camera)
- Real art/illustrations
- Real ad SDK integration
- Real IAP integration
- Powerups, daily challenges, new piece types
- Object restoration animations (looping animations)
- LiveOps / seasonal events
- Level data authoring tools

## Technical Constraints

- `SimpleGame.Core` must not reference `SimpleGame.Game` (one-way dependency)
- No DI framework, no static state, no singletons
- All async operations via UniTask with CancellationToken
- Views are MonoBehaviours exposing interfaces — no references to presenters or services
- Presenters are plain C# classes
- uGUI only (no UI Toolkit)
- ScriptableObjects live in `Assets/Data/` (new convention)

## Integration Points

- Existing ScreenManager<ScreenId> — screen navigation unchanged
- Existing PopupManager<PopupId> — popup show/dismiss unchanged, new PopupId entries
- Existing GameBootstrapper — navigation loop extended for reworked screens
- Existing UIFactory — new Create methods for new presenters
- Existing SceneSetup.cs — updated for new scene UI elements
- PlayerPrefs — persistence backend for meta state

## Open Questions

- None — scope is clear and constrained
