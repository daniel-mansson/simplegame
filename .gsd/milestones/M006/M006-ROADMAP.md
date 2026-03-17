# M006: Puzzle Tap Game Skeleton

**Vision:** Wire the complete Puzzle Tap game flow — main screen with meta world, stub gameplay with hearts, level complete/failed popups, golden piece economy, object restoration, environment progression, ad/IAP stub popups, and PlayerPrefs persistence — all with text-box stub visuals on top of the existing MVP architecture.

## Success Criteria

- Main screen shows current environment with restorable objects (text stubs), golden piece balance, play button, settings entry
- Stub gameplay screen has piece counter, place-correct/place-incorrect buttons, hearts display (3 per level), win/lose conditions
- LevelComplete popup shows golden pieces earned, continues to main screen
- LevelFailed popup offers retry, watch-ad stub, quit
- Rewarded ad and IAP purchase stub popups are tappable and functional
- Object-restored celebration popup fires when restoration completes
- One tap on an unblocked object spends one golden piece for one restoration step
- Blocked objects visible but not tappable until dependencies restored
- Completing all objects in an environment unlocks the next (1–3 environments available simultaneously)
- Meta progression persists via PlayerPrefs across play-mode restarts
- Full flow navigable end-to-end in play mode
- Debug log fires at win/lose indicating interstitial ad could trigger

## Key Risks / Unknowns

- ScriptableObjects are new to this project — need to establish authoring and runtime-access patterns cleanly
- Main screen complexity: environment with tappable objects, balance, play button — most complex single screen so far
- PlayerPrefs persistence for structured meta state — JSON serialization of save data needs careful schema design
- Popup count grows from 3 to 6+ — UnityPopupContainer scaling

## Proof Strategy

- ScriptableObject pattern → retire in S01 by building WorldData/EnvironmentData/RestorableObjectData with test data and edit-mode tests proving the data loads
- Main screen complexity → retire in S05 by wiring environment view with tappable objects into the reworked MainMenu scene
- Persistence → retire in S01 by round-tripping save data through PlayerPrefs in edit-mode tests

## Verification Classes

- Contract verification: edit-mode tests for all services (persistence, currency, hearts, meta progression, presenters); ScriptableObject data loads; all 98+ existing tests still pass
- Integration verification: full flow navigable in play mode (main screen → play → win/lose → earn → spend → restore → unlock)
- Operational verification: persistence survives play-mode restart
- UAT / human verification: visual confirmation that text-stub screens render and flow is tappable

## Milestone Definition of Done

This milestone is complete only when all are true:

- Full flow navigable: main screen → play level → win/lose → earn golden pieces → spend on objects → restore → unlock next environment
- Meta world data defined via ScriptableObjects with at least 2 test environments and 4+ objects
- Persistence works across play-mode restart (PlayerPrefs)
- All new screens/popups use text-box stub visuals
- All ad/IAP UI stubs are tappable (not just interfaces)
- Hearts system works: 3 per level, incorrect costs 1, 0 = fail
- Interstitial ad stub debug logs at win/lose
- Core framework patterns unchanged (ITransitionPlayer, ScreenManager, PopupManager)
- All existing 98 tests still pass + new tests for all new services and presenters
- ConfirmDialog reworked for game use (not generic demo)
- SceneSetup.cs updated to create all new scene content programmatically

## Requirement Coverage

- Covers: R045, R046, R047, R048, R049, R050, R051, R052, R053, R054, R055, R056, R057, R058, R059
- Partially covers: none
- Leaves for later: R060, R061, R062, R063, R064, R065, R066, R067, R068
- Orphan risks: none

## Slices

- [x] **S01: Meta world data model and persistence** `risk:medium` `depends:[]`
  > After this: ScriptableObjects for WorldData/EnvironmentData/RestorableObjectData exist with test data (2 environments, 4+ objects). Persistence service saves/loads restoration progress and golden piece balance via PlayerPrefs. All proven by edit-mode tests.

- [x] **S02: Currency and heart services** `risk:low` `depends:[]`
  > After this: GoldenPieceService tracks balance (earn/spend/persist). HeartService tracks per-level hearts (3, decrement, reset). Both interface-backed. All proven by edit-mode tests.

- [x] **S03: Stub gameplay screen with hearts** `risk:medium` `depends:[S02]`
  > After this: InGame scene reworked — shows level ID, piece counter (N/total), place-correct/place-incorrect buttons, hearts display. Place-incorrect costs a heart. Win when all pieces placed, lose at 0 hearts. Interstitial ad debug log fires at win/lose. Edit-mode tests for presenter.

- [x] **S04: LevelComplete, LevelFailed, and ad/IAP stub popups** `risk:medium` `depends:[S02,S03]`
  > After this: LevelComplete popup shows golden pieces earned + continue. LevelFailed popup offers retry, watch-ad stub, quit. RewardedAd stub popup simulates ad and grants reward. IAPPurchase stub popup simulates purchase. All popups functional with text-stub UI. Edit-mode tests for presenters.

- [x] **S05: Main screen with meta world** `risk:high` `depends:[S01,S02,S04]`
  > After this: MainMenu reworked into main screen — shows current environment name, restorable objects with progress bars (text), golden piece balance, play button with current level, settings button. Tap unblocked object = spend one golden piece for one restoration step. Blocked objects shown but disabled. Object-restored celebration popup fires on completion. ConfirmDialog reworked. Edit-mode tests for presenter.

- [x] **S06: Environment progression and full flow integration** `risk:medium` `depends:[S05]`
  > After this: Completing all objects in an environment unlocks the next. 1–3 environments available simultaneously. Full flow navigable end-to-end in play mode. Persistence verified across restart. SceneSetup.cs updated for all new UI. All existing + new tests pass.

## Boundary Map

### S01

Produces:
- `Assets/Data/` — ScriptableObject assets: WorldData, EnvironmentData (×2), RestorableObjectData (×4+)
- `Assets/Scripts/Game/Meta/WorldData.cs` — SO: list of EnvironmentData references
- `Assets/Scripts/Game/Meta/EnvironmentData.cs` — SO: name, list of RestorableObjectData references
- `Assets/Scripts/Game/Meta/RestorableObjectData.cs` — SO: name, totalSteps, costPerStep, blockedBy list (RestorableObjectData[])
- `Assets/Scripts/Game/Services/IMetaSaveService.cs` — interface: Save/Load meta state (per-object progress, golden piece balance, current environment)
- `Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs` — PlayerPrefs implementation with JSON serialization
- `Assets/Scripts/Game/Services/MetaProgressionService.cs` — runtime service: tracks restoration progress per object, checks blocked state, checks environment completion

Consumes:
- nothing (first slice)

### S02

Produces:
- `Assets/Scripts/Game/Services/IGoldenPieceService.cs` — interface: Earn, Spend, Balance, persist
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — implementation backed by IMetaSaveService
- `Assets/Scripts/Game/Services/IHeartService.cs` — interface: Reset(count), UseHeart, RemainingHearts, IsAlive
- `Assets/Scripts/Game/Services/HeartService.cs` — in-memory per-level implementation

Consumes:
- nothing (parallel with S01, but GoldenPieceService may use IMetaSaveService for persistence)

### S03

Produces:
- Reworked `Assets/Scripts/Game/InGame/IInGameView.cs` — adds heart display, piece counter, place-correct, place-incorrect (replaces score/win/lose buttons)
- Reworked `Assets/Scripts/Game/InGame/InGamePresenter.cs` — hearts logic, piece counting, win/lose conditions
- Reworked `Assets/Scripts/Game/InGame/InGameView.cs` — text-stub UI for new fields
- `Assets/Scripts/Game/InGame/InGameAction.cs` — updated enum if needed
- Interstitial ad debug log at win/lose points

Consumes from S02:
- `IHeartService` — per-level heart tracking

### S04

Produces:
- Reworked `Assets/Scripts/Game/Popup/WinDialogPresenter.cs` → `LevelCompletePresenter.cs` — shows golden pieces earned
- Reworked `Assets/Scripts/Game/Popup/LoseDialogPresenter.cs` → `LevelFailedPresenter.cs` — retry, watch-ad, quit
- `Assets/Scripts/Game/Popup/RewardedAdPresenter.cs` + `IRewardedAdView.cs` + `RewardedAdView.cs` — stub ad popup
- `Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs` + `IIAPPurchaseView.cs` + `IAPPurchaseView.cs` — stub purchase popup
- Updated `PopupId.cs` — new entries
- Updated `UnityPopupContainer.cs` — new popup references

Consumes from S02:
- `IGoldenPieceService` — earning golden pieces on level complete
- `IHeartService` — granting heart via rewarded ad

Consumes from S03:
- Reworked InGameSceneController flow (win/lose triggers popups)

### S05

Produces:
- Reworked `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — environment display, object list with progress, golden piece balance, play button, settings
- Reworked `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — meta world interaction logic, tap-to-restore
- Reworked `Assets/Scripts/Game/MainMenu/MainMenuView.cs` — text-stub UI
- `Assets/Scripts/Game/Popup/ObjectRestoredPresenter.cs` + `IObjectRestoredView.cs` + `ObjectRestoredView.cs` — celebration popup
- Reworked ConfirmDialog for game use (quit confirmation context)

Consumes from S01:
- `MetaProgressionService` — object restoration state, blocked checks
- `WorldData`/`EnvironmentData`/`RestorableObjectData` — static data

Consumes from S02:
- `IGoldenPieceService` — balance display, spending

Consumes from S04:
- Updated `PopupId`, `UnityPopupContainer` — ObjectRestored popup entry

### S06

Produces:
- Environment progression logic in `MetaProgressionService` — unlock next when all objects complete, 1–3 available rule
- Updated `GameBootstrapper` — wires all new services
- Updated `SceneSetup.cs` — creates all new scene UI programmatically
- Full integration wiring

Consumes from S05:
- Reworked main screen with meta world
- All services and popups from prior slices
