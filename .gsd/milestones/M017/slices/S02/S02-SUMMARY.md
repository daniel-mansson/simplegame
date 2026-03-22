---
id: S02
parent: M017
milestone: M017
provides:
  - IRewardedAdView.SetWatchInteractable(bool) — new interface method
  - RewardedAdPresenter accepts IAdService; drives real ad flow; handles NotLoaded gracefully
  - InGameSceneController.HandleRewardedAdAsync returns bool; hearts only restored on AdResult.Completed
  - InGameSceneController.Initialize() accepts IAdService (optional, defaults to NullAdService)
  - GameBootstrapper constructs UnityAdService with test game IDs and passes to InGameSceneController
key_files:
  - Assets/Scripts/Game/Popup/IRewardedAdView.cs
  - Assets/Scripts/Game/Popup/RewardedAdView.cs
  - Assets/Scripts/Game/Popup/RewardedAdPresenter.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Tests/EditMode/Game/PopupTests.cs
key_decisions:
  - HandleRewardedAdAsync now returns bool — outer RunAsync only calls RestoreHeartsAndContinue when true
  - When ad skipped/failed/unavailable in WatchAd branch: fall through to Retry (not free continue)
  - NullAdService fallback in HandleRewardedAdAsync when _adService is null (play-from-editor path)
  - RewardedAdPresenter constructor keeps IAdService optional (default null → NullAdService) for backward compat
patterns_established:
  - Popup view resolver pattern for IRewardedAdView matches ILevelCompleteView/ILevelFailedView
drill_down_paths:
  - .gsd/milestones/M017/slices/S02/S02-PLAN.md
duration: ~25min
verification_result: pass
completed_at: 2026-03-20T19:30:00Z
---

# S02: Rewarded Ad — Real Flow

**RewardedAdPresenter wired to IAdService — real ad shown on Watch, button grays with message when unavailable; hearts only restored on completion.**

## What Happened

Extended `IRewardedAdView` with `SetWatchInteractable(bool)` and bound it to `_watchButton.interactable` in `RewardedAdView`. Rewrote `RewardedAdPresenter` to take `IAdService`: `Initialize()` grays the Watch button immediately if `IsRewardedLoaded` is false; `HandleWatch()` checks again and calls `ShowAdAsync()` which awaits `ShowRewardedAsync()` and resolves the result TCS.

`InGameSceneController.HandleRewardedAdAsync` replaces the stub: resolves `IRewardedAdView` from `_viewResolver`, creates a `RewardedAdPresenter`, shows the popup, awaits `WaitForResult()`, dismisses. Returns `bool` — the WatchAd branch in `RunAsync` now only calls `RestoreHeartsAndContinue()` when the result is `true`. A skipped or failed ad falls through to Retry.

`GameBootstrapper` constructs `UnityAdService` with test game IDs (D089), sets analytics, calls `Initialize()`, and passes to `InGameSceneController`.

## Deviations

None.

## Files Created/Modified

- `Assets/Scripts/Game/Popup/IRewardedAdView.cs` — SetWatchInteractable added
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — button interactable binding
- `Assets/Scripts/Game/Popup/RewardedAdPresenter.cs` — full rewrite with IAdService
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — _adService field, Initialize param, HandleRewardedAdAsync rewrite
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — UnityAdService construction and wiring
- `Assets/Tests/EditMode/Game/PopupTests.cs` — MockRewardedAdView updated, new test cases
