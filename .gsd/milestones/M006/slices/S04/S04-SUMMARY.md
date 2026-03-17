---
id: S04
milestone: M006
provides:
  - LevelCompletePresenter + ILevelCompleteView (replaces WinDialog) — shows golden pieces earned
  - LevelFailedPresenter + ILevelFailedView (replaces LoseDialog) — retry/watch-ad/quit
  - RewardedAdPresenter + IRewardedAdView — stub ad popup (watch/skip)
  - IAPPurchasePresenter + IIAPPurchaseView — stub purchase popup (purchase/cancel)
  - LevelFailedChoice enum (Retry, WatchAd, Quit)
  - PopupId updated: LevelComplete, LevelFailed, RewardedAd, IAPPurchase, ObjectRestored
  - UnityPopupContainer updated with all 6 popup slots
  - InGameSceneController updated: golden piece earning, LevelFailed WatchAd flow
  - 4 LevelComplete tests + 6 LevelFailed tests + 5 RewardedAd tests + 5 IAPPurchase tests
key_files:
  - Assets/Scripts/Game/Popup/LevelCompletePresenter.cs
  - Assets/Scripts/Game/Popup/LevelFailedPresenter.cs
  - Assets/Scripts/Game/Popup/RewardedAdPresenter.cs
  - Assets/Scripts/Game/Popup/IAPPurchasePresenter.cs
  - Assets/Scripts/Game/PopupId.cs
  - Assets/Scripts/Game/Popup/UnityPopupContainer.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/PopupTests.cs
key_decisions:
  - "Renamed WinDialog→LevelComplete, LoseDialog→LevelFailed for game-specific naming"
  - "LevelFailed adds WatchAd choice alongside Retry/Quit"
  - "RewardedAd and IAPPurchase are fully stub — Debug.Log on action, no real SDK"
  - "InGameSceneController earns golden pieces on win and handles WatchAd flow"
patterns_established:
  - "Stub monetization pattern: presenter + view + Debug.Log, no external SDK dependency"
drill_down_paths:
  - .gsd/milestones/M006/slices/S04/S04-PLAN.md
verification_result: pass
completed_at: 2026-03-17T14:00:00Z
---

# S04: LevelComplete, LevelFailed, and ad/IAP stub popups

**Renamed popups to game-specific names, added golden piece earning, watch-ad option, and RewardedAd/IAPPurchase stub popups with 20 tests**

## What Happened

Renamed WinDialog → LevelComplete with golden pieces earned display. Renamed LoseDialog → LevelFailed with WatchAd option. Created RewardedAd and IAPPurchase stub popups. Updated PopupId enum (6 entries including ObjectRestored placeholder). Updated UnityPopupContainer with all popup slots.

Updated InGameSceneController: earns golden pieces on win (via IGoldenPieceService), handles WatchAd flow in LevelFailed (shows RewardedAd stub popup, grants retry).

20 new/reworked popup tests: 4 LevelComplete + 6 LevelFailed + 5 RewardedAd + 5 IAPPurchase. 4 reworked InGameSceneController tests use new popup types.

Deleted old WinDialog/LoseDialog/LoseDialogChoice files.

## Tasks Completed
- T01: Rename WinDialog→LevelComplete, rework LoseDialog→LevelFailed
- T02: RewardedAd and IAPPurchase stub popups
