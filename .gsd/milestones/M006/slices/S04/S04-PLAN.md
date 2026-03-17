# S04: LevelComplete, LevelFailed, and ad/IAP stub popups

**Goal:** Rename WinDialog/LoseDialog to LevelComplete/LevelFailed with golden piece earning. Add RewardedAd and IAPPurchase stub popups. All functional with text-stub UI.
**Demo:** Tests prove: LevelComplete shows earned pieces + continue, LevelFailed offers retry/watch-ad/quit, RewardedAd grants hearts, IAPPurchase grants golden pieces.

## Must-Haves

- LevelComplete popup: shows golden pieces earned, continues to main screen
- LevelFailed popup: retry, watch-ad (opens RewardedAd popup), quit
- RewardedAd stub popup: simulates ad, grants reward (hearts)
- IAPPurchase stub popup: simulates purchase, grants golden pieces
- PopupId enum updated: LevelComplete, LevelFailed, RewardedAd, IAPPurchase
- All references updated across codebase
- Edit-mode tests for all new/reworked presenters

## Verification

- All popup presenter tests pass
- All InGame scene controller tests pass with new popup names
- No compile errors

## Tasks

- [x] **T01: Rename WinDialog→LevelComplete, rework LoseDialog→LevelFailed** `est:25m`
  - Why: Core popup rework — rename to game-specific names, add golden pieces display, add watch-ad option
  - Files: All popup files, PopupId.cs, InGameSceneController, UIFactory, LoseDialogChoice→LevelFailedChoice
  - Do:
    1. Rename IWinDialogView → ILevelCompleteView, add UpdateGoldenPieces(string)
    2. Rename WinDialogPresenter → LevelCompletePresenter, accept goldenPiecesEarned in Initialize
    3. Rename ILoseDialogView → ILevelFailedView, add OnWatchAdClicked
    4. Rename LoseDialogPresenter → LevelFailedPresenter, add WatchAd choice
    5. Rename LoseDialogChoice → LevelFailedChoice (Retry, WatchAd, Quit)
    6. Rename view MonoBehaviours: WinDialogView→LevelCompleteView, LoseDialogView→LevelFailedView
    7. Update PopupId: WinDialog→LevelComplete, LoseDialog→LevelFailed, add RewardedAd, IAPPurchase
    8. Update InGameSceneController: use new names, pass goldenPieces to LevelComplete, handle WatchAd
    9. Update UIFactory: new Create methods
    10. Rename/update test mocks and all tests

- [x] **T02: RewardedAd and IAPPurchase stub popups** `est:15m`
  - Why: New popup types needed by LevelFailed (ad) and main screen (IAP)
  - Files: New popup files, PopupTests.cs
  - Do:
    1. Create IRewardedAdView + RewardedAdPresenter — shows "Watching ad..." text, has OnAdComplete button, resolves with reward granted
    2. Create IIAPPurchaseView + IAPPurchasePresenter — shows item + price, has OnPurchase/OnCancel, resolves with purchased bool
    3. Create RewardedAdView + IAPPurchaseView MonoBehaviours (text-stub)
    4. Write tests for both presenters

## Files Likely Touched

- `Assets/Scripts/Game/Popup/` — all popup files renamed/created
- `Assets/Scripts/Game/PopupId.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/Boot/UIFactory.cs`
- `Assets/Tests/EditMode/Game/PopupTests.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
