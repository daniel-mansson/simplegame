---
id: S06
milestone: M006
provides:
  - GameBootstrapper wired with all M006 services (HeartService, MetaProgressionService, GoldenPieceService, PlayerPrefsMetaSaveService)
  - GameBootstrapper takes WorldData via SerializeField
  - SceneSetup.cs fully updated for new view types (LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored)
  - SceneSetup.cs creates InGame scene with hearts/piece counter, MainMenu with environment/balance/objects
  - SceneSetup.cs wires WorldData asset to GameBootstrapper
key_files:
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/SceneSetup.cs
key_decisions:
  - "GameBootstrapper takes WorldData via SerializeField — wired by SceneSetup from Assets/Data/WorldData.asset"
  - "Environment progression determined at runtime by MainMenuSceneController.GetCurrentEnvironment()"
patterns_established:
  - "Full service construction in GameBootstrapper with injection into scene controllers"
drill_down_paths:
  - .gsd/milestones/M006/slices/S06/S06-PLAN.md
verification_result: pass
completed_at: 2026-03-17T14:25:00Z
---

# S06: Environment progression and full flow integration

**Wired all M006 services in GameBootstrapper and updated SceneSetup.cs for all new view types**

## What Happened

Updated GameBootstrapper to construct all M006 services: HeartService, PlayerPrefsMetaSaveService, MetaProgressionService, GoldenPieceService. Takes WorldData via SerializeField. Passes all services to UIFactory and scene controllers. InGameSceneController now receives IGoldenPieceService and IHeartService.

Fully rewrote SceneSetup.cs:
- Boot scene: 6 popups (ConfirmDialog, LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored) with correct view types and field wiring. Loads WorldData asset for GameBootstrapper.
- MainMenu scene: environment name, balance, level display, objects text, play/settings buttons with MainMenuView wiring.
- InGame scene: hearts text, piece counter, place-correct/place-incorrect buttons with InGameView wiring.
- Settings scene: unchanged structure.

## Tasks Completed
- T01: Update GameBootstrapper, SceneSetup.cs
