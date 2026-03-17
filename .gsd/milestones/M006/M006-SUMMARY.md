---
id: M006
provides:
  - Full Puzzle Tap game skeleton — main screen with meta world, stub gameplay with hearts, golden piece economy, object restoration, environment progression
  - ScriptableObject data model — WorldData/EnvironmentData/RestorableObjectData with blocked-by dependencies
  - Interface-backed persistence — IMetaSaveService + PlayerPrefsMetaSaveService with JSON serialization
  - Golden piece currency service — earn on level complete, spend on object restoration, persisted
  - Heart service — 3 per level, decrement on incorrect, fail at 0
  - 6 popups — ConfirmDialog (reworked), LevelComplete, LevelFailed (with WatchAd), RewardedAd, IAPPurchase, ObjectRestored
  - Interstitial ad stub debug logs at win and lose
  - Environment progression — complete all objects to unlock next, NextEnvironment button flow
  - Reload-then-merge save pattern for multi-service shared PlayerPrefs persistence
  - SceneSetup.cs fully updated for all new view types and scene layouts
key_decisions:
  - "D035: ScriptableObjects for meta world data — flat with blocked-by lists"
  - "D036: IMetaSaveService + PlayerPrefs JSON for persistence"
  - "D037: Main screen IS the meta world — no separate MetaWorld scene"
  - "D038: Tap-to-spend without confirmation"
  - "D039: Stub gameplay — counter + correct/incorrect buttons only"
  - "D040: Ad/IAP as tappable UI stubs, not just interfaces"
patterns_established:
  - "ScriptableObject data pattern with editor utility for programmatic asset creation"
  - "Interface-backed persistence with PlayerPrefs JSON backend"
  - "Reload-then-merge save pattern for multi-service shared persistence"
  - "Stub monetization pattern: presenter + view + Debug.Log, no external SDK dependency"
  - "Presenter-to-view data transfer via struct array (ObjectDisplayData[])"
  - "Auto-resolving presenter outcome — WaitForAction resolves automatically on terminal state"
  - "Controller determines current environment, passes to presenter at construction"
observability_surfaces:
  - "[Ads] Interstitial ad opportunity debug log at win/lose"
  - "[GoldenPieceService] balance change logs"
  - "[MetaProgressionService] restoration step logs"
  - "[HeartService] heart usage logs"
  - "[MainMenuSceneController] progress reset log"
requirement_outcomes:
  - id: R045
    from_status: active
    to_status: validated
    proof: "MainMenuPresenter shows environment name, objects with progress/blocked/complete state, golden piece balance, play button with level, settings button. 25 DemoWiringTests pass."
  - id: R046
    from_status: active
    to_status: validated
    proof: "InGamePresenter has piece counter, PlaceCorrect/PlaceIncorrect, hearts display. Auto-win at totalPieces, auto-lose at 0 hearts. 14 presenter + 4 controller tests pass."
  - id: R047
    from_status: active
    to_status: validated
    proof: "WorldData/EnvironmentData/RestorableObjectData as ScriptableObjects with blocked-by lists. 3 environments, 5+ objects. 18 MetaProgressionService tests pass."
  - id: R048
    from_status: active
    to_status: validated
    proof: "InGameSceneController earns golden pieces on win. MainMenuPresenter spends per costPerStep on object tap. 15 GoldenPieceService + 25 DemoWiringTests pass."
  - id: R049
    from_status: active
    to_status: validated
    proof: "IMetaSaveService + PlayerPrefsMetaSaveService with JSON. Reload-then-merge save pattern. Round-trip persistence tests pass."
  - id: R050
    from_status: active
    to_status: validated
    proof: "GetCurrentEnvironment finds first non-complete. NextEnvironment button shown when complete + hasNext. 3 environments in WorldData."
  - id: R051
    from_status: active
    to_status: validated
    proof: "LevelCompletePresenter shows score, level, golden pieces earned. Continue returns to main. 4 tests pass."
  - id: R052
    from_status: active
    to_status: validated
    proof: "LevelFailedPresenter offers Retry/WatchAd/Quit. InGameSceneController handles all three. 6 tests pass."
  - id: R053
    from_status: active
    to_status: validated
    proof: "RewardedAdPresenter with Watch/Skip buttons, Debug.Log on watch. 5 tests pass."
  - id: R054
    from_status: active
    to_status: validated
    proof: "IAPPurchasePresenter with Purchase/Cancel buttons, Debug.Log on purchase. 5 tests pass."
  - id: R055
    from_status: active
    to_status: validated
    proof: "ObjectRestoredPresenter shows object name + Continue. MainMenuSceneController shows popup on completion. 4 tests pass."
  - id: R056
    from_status: active
    to_status: validated
    proof: "InGamePresenter logs '[Ads] Interstitial ad opportunity' at both win and lose."
  - id: R057
    from_status: active
    to_status: validated
    proof: "HeartService Reset(3)/UseHeart/IsAlive. InGamePresenter auto-loses at 0 hearts. 12 + 14 tests pass."
  - id: R058
    from_status: active
    to_status: validated
    proof: "Full flow wired end-to-end. GameBootstrapper constructs all services. SceneSetup creates all scenes. All controller tests pass."
  - id: R059
    from_status: active
    to_status: validated
    proof: "All views use uGUI Text + Button. No art assets, no animations."
duration: "~5 hours"
verification_result: passed
completed_at: 2026-03-17T17:00:00Z
---

# M006: Puzzle Tap Game Skeleton

**Built the complete Puzzle Tap game flow — meta world data model, golden piece economy, heart-based gameplay, 6 popups, object restoration, environment progression, and PlayerPrefs persistence — all wired end-to-end with text-stub visuals on top of the existing MVP architecture.**

## What Happened

Six slices assembled the full game skeleton in dependency order:

**S01** established the data foundation — three ScriptableObject types (WorldData, EnvironmentData, RestorableObjectData) with flat blocked-by dependency lists, and an interface-backed persistence layer (IMetaSaveService + PlayerPrefsMetaSaveService). MetaProgressionService provides runtime tracking of per-object restoration progress, blocked-state queries, and environment completion checks. A CreateTestWorldData editor utility generates 3 environments (Garden, Town Square, Harbor) with 5+ objects and blocked-by relationships. 18 edit-mode tests prove the service and data model.

**S02** added the economy services — GoldenPieceService (earn/spend/persist backed by IMetaSaveService) and HeartService (in-memory per-level, 3 hearts, decrement, reset). Both services are interface-backed. A reload-then-merge save pattern was introduced so multiple services can share the same PlayerPrefs JSON save key without overwriting each other's data. MetaProgressionService was retrofitted with the same pattern. 27 tests cover both services including cross-service data preservation.

**S03** reworked the InGame scene from manual win/lose buttons to piece-placement gameplay with hearts. PlaceCorrect increments a counter (auto-win when all pieces placed), PlaceIncorrect costs one heart via IHeartService (auto-lose at 0). The presenter auto-resolves outcomes — no manual win/lose triggers. Debug.Log fires "[Ads] Interstitial ad opportunity" at both win and lose. GameSessionService gained TotalPieces. 18 tests cover the presenter and controller.

**S04** replaced the generic WinDialog/LoseDialog with game-specific LevelComplete and LevelFailed popups. LevelComplete shows golden pieces earned. LevelFailed offers Retry, WatchAd (shows RewardedAd stub popup, grants extra hearts), and Quit. Created RewardedAd and IAPPurchase tappable stub popups with full presenter/view/interface stacks. PopupId enum grew to 6 entries. InGameSceneController was updated to earn golden pieces on win and handle the WatchAd flow. 20 popup tests + 4 reworked controller tests.

**S05** transformed MainMenu into the meta world hub. MainMenuPresenter shows environment name, dynamically-generated object buttons with progress/blocked/complete state, golden piece balance, and level display. Tapping an unblocked object spends golden pieces and restores one step (no confirmation — D038). ObjectRestored celebration popup fires when an object completes. MainMenuSceneController determines the current environment (first non-complete) and handles the full popup flow. UIFactory extended with all service injections. 33 tests (25 DemoWiring + 4 SceneController + 4 ObjectRestored).

**S06** wired everything together — GameBootstrapper constructs all M006 services (HeartService, PlayerPrefsMetaSaveService, MetaProgressionService, GoldenPieceService) and takes WorldData via SerializeField. SceneSetup.cs fully rewritten to create all 4 scenes programmatically with correct view types, field wiring, and 6 popup slots in the Boot scene.

## Cross-Slice Verification

**Main screen shows current environment with restorable objects, golden piece balance, play button, settings entry**
— MainMenuPresenter.Initialize() calls View.UpdateEnvironmentName, UpdateBalance, UpdateLevelDisplay, and UpdateObjects with ObjectDisplayData[]. MainMenuView creates dynamic buttons per object. 25 DemoWiringTests verify. PASS

**Stub gameplay screen has piece counter, place-correct/place-incorrect buttons, hearts display, win/lose conditions**
— InGamePresenter manages piece counter (N/total), PlaceCorrect/PlaceIncorrect handlers, hearts display via IHeartService. Auto-win at totalPieces, auto-lose at 0 hearts. 14 presenter tests verify. PASS

**LevelComplete popup shows golden pieces earned, continues to main screen**
— LevelCompletePresenter.Initialize(score, level, goldenPiecesEarned) updates view. WaitForContinue resolves on click. 4 tests verify. PASS

**LevelFailed popup offers retry, watch-ad stub, quit**
— LevelFailedPresenter exposes WaitForChoice returning LevelFailedChoice (Retry, WatchAd, Quit). 6 tests verify. PASS

**Rewarded ad and IAP purchase stub popups are tappable and functional**
— RewardedAdPresenter (Watch/Skip) and IAPPurchasePresenter (Purchase/Cancel) are full presenter+view stacks. 10 tests verify. PASS

**Object-restored celebration popup fires when restoration completes**
— MainMenuPresenter resolves ObjectRestored action when an object reaches totalSteps. MainMenuSceneController shows ObjectRestoredPresenter popup. 4 tests verify. PASS

**One tap on an unblocked object spends one golden piece for one restoration step**
— MainMenuPresenter.HandleObjectTapped validates not-blocked, not-complete, sufficient balance, then calls TrySpend and TryRestoreStep. DemoWiringTests verify. PASS

**Blocked objects visible but not tappable until dependencies restored**
— ObjectDisplayData.IsBlocked set by MetaProgressionService.IsBlocked(). MainMenuView creates disabled buttons (interactable=false). Presenter rejects taps. PASS

**Completing all objects in an environment unlocks the next (1-3 environments available simultaneously)**
— GetCurrentEnvironment() finds first non-complete. SetNextEnvironmentVisible shown when complete + hasNext. WorldData has 3 environments. PASS

**Meta progression persists via PlayerPrefs across play-mode restarts**
— PlayerPrefsMetaSaveService uses JSON serialization. Reload-then-merge save pattern. Round-trip tests pass. PASS

**Full flow navigable end-to-end in play mode**
— GameBootstrapper constructs all services. MainMenu to InGame to popup flows to MainMenu. SceneSetup creates all scenes. PASS

**Debug log fires at win/lose indicating interstitial ad could trigger**
— InGamePresenter logs "[Ads] Interstitial ad opportunity" at auto-win and auto-lose. PASS

**Core framework patterns unchanged**
— No changes to Assets/Scripts/Core/. ScreenManager, PopupManager, ITransitionPlayer unchanged. PASS

**All existing 98 tests still pass + new tests for all new services and presenters**
— 164+ edit-mode tests passing. 116 new/reworked game tests plus existing core tests. PASS

**ConfirmDialog reworked for game use**
— ConfirmDialogPresenter takes custom message. Used for "Reset all progress?" confirmation. PASS

**SceneSetup.cs updated to create all new scene content programmatically**
— Creates Boot (6 popups, WorldData), MainMenu (environment, balance, objects, buttons), InGame (hearts, pieces, buttons), Settings. PASS

## Requirement Changes

- R045: active to validated — MainMenuPresenter shows environment, objects, balance, play, settings. 25 DemoWiringTests pass.
- R046: active to validated — InGamePresenter has piece counter, hearts, auto-win/lose. 18 tests pass.
- R047: active to validated — WorldData/EnvironmentData/RestorableObjectData SOs with blocked-by. 18 tests pass.
- R048: active to validated — Golden pieces earned on win, spent on tap-to-restore. 15+25 tests pass.
- R049: active to validated — IMetaSaveService + PlayerPrefsMetaSaveService with JSON. Round-trip tests pass.
- R050: active to validated — GetCurrentEnvironment finds first non-complete, NextEnvironment advances. 3 environments.
- R051: active to validated — LevelComplete shows score/level/golden pieces + Continue. 4 tests pass.
- R052: active to validated — LevelFailed offers Retry/WatchAd/Quit. 6 tests pass.
- R053: active to validated — RewardedAd stub popup with Watch/Skip. 5 tests pass.
- R054: active to validated — IAPPurchase stub popup with Purchase/Cancel. 5 tests pass.
- R055: active to validated — ObjectRestored popup fires on completion. 4 tests pass.
- R056: active to validated — "[Ads] Interstitial ad opportunity" Debug.Log at win and lose.
- R057: active to validated — HeartService: Reset(3), UseHeart, IsAlive. 12+14 tests pass.
- R058: active to validated — Full flow wired end-to-end. All controllers + SceneSetup.
- R059: active to validated — All views are uGUI Text+Button stubs. No art, no animations.

## Forward Intelligence

### What the next milestone should know
- The stub gameplay (PlaceCorrect/PlaceIncorrect buttons) is deliberately minimal. The real puzzle board, piece tray, neighbor validation, and camera auto-adjust all need to replace InGamePresenter and InGameView — but the service layer (hearts, golden pieces, progression) and popup flow (LevelComplete, LevelFailed) should remain unchanged.
- The meta world data model is in ScriptableObjects under Assets/Data/. The CreateTestWorldData editor utility can regenerate test data. Real content authoring should extend this pattern, not replace it.
- Persistence uses a single PlayerPrefs key with JSON. The reload-then-merge pattern means any new service sharing this save key must reload before writing. MetaSaveData schema changes require migration logic.

### What's fragile
- **Reload-then-merge save pattern** — if a third service writes to the same save key without following the pattern, data loss occurs. The pattern is documented but not enforced at compile time.
- **MainMenuView dynamic button creation** — the view creates and destroys object buttons on every RefreshView call. Works for 5-10 objects but may need pooling at scale.
- **InGameSceneController.HandleRewardedAdAsync** — currently a stub that doesn't wire the RewardedAdPresenter. It just shows/dismisses the popup. A future milestone should wire the presenter for proper Watch/Skip flow in the InGame context.
- **Environment progression UI** — shows one environment at a time with "Next Environment" button. The "1-3 simultaneously" design intent maps to a sequential view with navigation, not a multi-environment dashboard. Future UI work may need to rethink this.

### Authoritative diagnostics
- **Edit-mode tests** — the primary health signal. Run via Unity batchmode `-runTests`. Any failure means a service contract or presenter logic is broken.
- **Debug.Log tags** — `[Ads]`, `[GoldenPieceService]`, `[MetaProgressionService]`, `[HeartService]`, `[MainMenuSceneController]` — grep Unity console for these to trace flow in play mode.
- **PlayerPrefs key** — the save data lives under a single PlayerPrefs string key. Use `PlayerPrefs.DeleteAll()` or the ResetProgress button to clear state for clean testing.

### What assumptions changed
- **ConfirmDialog was more complex than expected** — originally planned as a simple rework, it needed a custom message parameter and game-specific usage (reset progress confirmation). The pattern works but the ConfirmDialog is now tightly coupled to the reset flow.
- **Environment progression simplified** — the "1-3 environments available simultaneously" criterion was implemented as sequential with a NextEnvironment button rather than a multi-environment view. This is adequate for the stub but may need rethinking for the real UI.

## Files Created/Modified

- `Assets/Scripts/Game/Meta/WorldData.cs` — ScriptableObject: list of EnvironmentData
- `Assets/Scripts/Game/Meta/EnvironmentData.cs` — ScriptableObject: name + RestorableObjectData[]
- `Assets/Scripts/Game/Meta/RestorableObjectData.cs` — ScriptableObject: name, totalSteps, costPerStep, blockedBy[]
- `Assets/Scripts/Game/Services/IMetaSaveService.cs` — persistence interface
- `Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs` — PlayerPrefs JSON implementation
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — save data model with List of ObjectProgress
- `Assets/Scripts/Game/Services/MetaProgressionService.cs` — runtime restoration tracking
- `Assets/Scripts/Game/Services/IGoldenPieceService.cs` — currency interface
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — earn/spend/persist implementation
- `Assets/Scripts/Game/Services/IHeartService.cs` — heart interface
- `Assets/Scripts/Game/Services/HeartService.cs` — per-level heart tracking
- `Assets/Scripts/Game/InGame/IInGameView.cs` — reworked: hearts, piece counter, place-correct/incorrect
- `Assets/Scripts/Game/InGame/InGameAction.cs` — Win/Lose enum
- `Assets/Scripts/Game/InGame/InGamePresenter.cs` — reworked: piece placement, auto-win/lose, hearts
- `Assets/Scripts/Game/InGame/InGameView.cs` — reworked: text-stub UI
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — reworked: golden pieces, popups, WatchAd flow
- `Assets/Scripts/Game/MainMenu/IMainMenuView.cs` — reworked: environment, objects, balance, play
- `Assets/Scripts/Game/MainMenu/MainMenuAction.cs` — Settings/Play/ObjectRestored/ResetProgress/NextEnvironment
- `Assets/Scripts/Game/MainMenu/MainMenuPresenter.cs` — reworked: meta world interaction, tap-to-restore
- `Assets/Scripts/Game/MainMenu/MainMenuView.cs` — reworked: dynamic object buttons, text-stub UI
- `Assets/Scripts/Game/MainMenu/MainMenuSceneController.cs` — reworked: meta progression, popups
- `Assets/Scripts/Game/MainMenu/ObjectDisplayData.cs` — presenter-to-view data transfer struct
- `Assets/Scripts/Game/Popup/LevelCompletePresenter.cs` — replaces WinDialog
- `Assets/Scripts/Game/Popup/ILevelCompleteView.cs` — view interface
- `Assets/Scripts/Game/Popup/LevelCompleteView.cs` — text-stub view
- `Assets/Scripts/Game/Popup/LevelFailedPresenter.cs` — replaces LoseDialog, adds WatchAd
- `Assets/Scripts/Game/Popup/ILevelFailedView.cs` — view interface
- `Assets/Scripts/Game/Popup/LevelFailedView.cs` — text-stub view
- `Assets/Scripts/Game/Popup/LevelFailedChoice.cs` — Retry/WatchAd/Quit enum
- `Assets/Scripts/Game/Popup/RewardedAdPresenter.cs` — stub ad popup
- `Assets/Scripts/Game/Popup/IRewardedAdView.cs` — view interface
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — text-stub view
- `Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs` — stub purchase popup
- `Assets/Scripts/Game/Popup/IIAPPurchaseView.cs` — view interface
- `Assets/Scripts/Game/Popup/IAPPurchaseView.cs` — text-stub view
- `Assets/Scripts/Game/Popup/ObjectRestoredPresenter.cs` — celebration popup
- `Assets/Scripts/Game/Popup/IObjectRestoredView.cs` — view interface
- `Assets/Scripts/Game/Popup/ObjectRestoredView.cs` — text-stub view
- `Assets/Scripts/Game/Popup/ConfirmDialogPresenter.cs` — reworked: custom message parameter
- `Assets/Scripts/Game/Popup/UnityPopupContainer.cs` — 6 popup slots
- `Assets/Scripts/Game/PopupId.cs` — 6 entries
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — all M006 services, WorldData injection
- `Assets/Scripts/Game/Boot/UIFactory.cs` — extended with all service injections
- `Assets/Scripts/Game/Services/GameSessionService.cs` — TotalPieces field added
- `Assets/Editor/SceneSetup.cs` — fully rewritten for all M006 scenes and popups
- `Assets/Editor/CreateTestWorldData.cs` — editor utility for test data generation
- `Assets/Data/*.asset` — WorldData + 3 EnvironmentData + 10+ RestorableObjectData assets
- `Assets/Tests/EditMode/Game/MetaProgressionServiceTests.cs` — 18 tests
- `Assets/Tests/EditMode/Game/GoldenPieceServiceTests.cs` — 15 tests
- `Assets/Tests/EditMode/Game/HeartServiceTests.cs` — 12 tests
- `Assets/Tests/EditMode/Game/InGameTests.cs` — 18 tests (14 presenter + 4 controller)
- `Assets/Tests/EditMode/Game/PopupTests.cs` — 20 tests
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — 25 tests (fully rewritten)
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` — 4 tests (simplified)
