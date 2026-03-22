---
id: M017
provides:
  - IAdService contract (rewarded + interstitial load/show/isLoaded) with AdResult enum
  - NullAdService test double (SimulateLoaded, SimulateResult, Analytics injection)
  - UnityAdService wrapping Advertisement Legacy SDK 4.12.2 via UniTask callback bridge
  - RewardedAdPresenter drives real ad flow; unavailable → Watch button gray + status message
  - InGameSceneController.HandleRewardedAdAsync returns bool; hearts only on Completed
  - Interstitial after every N level completions (N from remote config, default 3)
  - HandleInterstitialAsync — silent failure on no fill or SDK error
  - IAnalyticsService extended with 4 ad event methods; PlayFabAnalyticsService implements them
  - 20+ new edit-mode tests covering all IAdService contract paths and analytics events
key_files:
  - Assets/Scripts/Game/Services/AdResult.cs
  - Assets/Scripts/Game/Services/IAdService.cs
  - Assets/Scripts/Game/Services/NullAdService.cs
  - Assets/Scripts/Game/Services/UnityAdService.cs
  - Assets/Scripts/Game/Services/IAnalyticsService.cs
  - Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs
  - Assets/Scripts/Game/Services/GameRemoteConfig.cs
  - Assets/Scripts/Game/Services/PlayFabRemoteConfigService.cs
  - Assets/Scripts/Game/Popup/IRewardedAdView.cs
  - Assets/Scripts/Game/Popup/RewardedAdPresenter.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Tests/EditMode/Game/AdServiceTests.cs
  - Packages/manifest.json
key_decisions:
  - Advertisement Legacy (com.unity.ads 4.12.2) — only actively maintained Unity-native SDK at this scope
  - IAdService abstraction keeps game layer SDK-free; NullAdService used exclusively in tests
  - UniTaskCompletionSource bridge for SDK callbacks — same pattern as M016 PlayFab
  - Test game IDs hardcoded with testMode=true; real IDs wired via future remote config work
  - Rewarded unavailable: deny with Watch button gray + status message (not silent grant)
  - Interstitial unavailable: skip silently, navigation continues
  - Interstitial counter is session-scoped (no cross-session persistence)
  - IAnalyticsService ad methods added in S01 to resolve NullAdService forward-reference
completed_at: 2026-03-20T20:00:00Z
---

# M017: Unity Ads Integration — Rewarded & Interstitial

**Unity Ads integrated end-to-end: rewarded ad flow with graceful unavailable-state handling, interstitial at remote-config frequency, full analytics instrumentation — 307/307 tests passing.**

## What Was Built

### S01: Ad Service Abstraction & SDK
`IAdService` interface defined with `AdResult` enum. `NullAdService` provides a deterministic test double with `SimulateLoaded`, `SimulateResult`, and `Analytics` injection. `UnityAdService` wraps the Advertisement Legacy SDK — SDK callbacks bridge to `UniTask` via `UniTaskCompletionSource` (same pattern as M016 PlayFab). Automatic pre-loading after initialization and after each show. `IAnalyticsService` extended with four ad event methods; `PlayFabAnalyticsService` and `MockAnalyticsService` updated. `com.unity.ads 4.12.2` added to manifest.

### S02: Rewarded Ad — Real Flow
`IRewardedAdView.SetWatchInteractable(bool)` added; `RewardedAdView` binds it to `_watchButton.interactable`. `RewardedAdPresenter` rewritten to take `IAdService` — grays Watch button on `IsRewardedLoaded=false` with status message; `ShowAdAsync()` drives the real ad and resolves the result TCS. `InGameSceneController.HandleRewardedAdAsync` returns `bool`; the WatchAd branch only calls `RestoreHeartsAndContinue()` when `AdResult.Completed` — skipped/failed/unavailable falls through to Retry. `GameBootstrapper` constructs `UnityAdService` with test game IDs and passes it through.

### S03: Interstitial — Post-Level Frequency
`GameRemoteConfig.InterstitialEveryNLevels` (default 3) fetched from PlayFab Title Data key `"interstitial_every_n_levels"`. `_levelsCompletedThisSession` counter in `InGameSceneController` increments on each win; `HandleInterstitialAsync` fires at the N-level threshold and silently continues on any failure.

### S04: Ad Analytics
All four ad events (`ad_impression`, `ad_completed`, `ad_skipped`, `ad_failed_to_load`) wired in S01 and verified across all layers. Offline guard test added for `PlayFabAnalyticsService`.

## Verification

- Static: all types, interfaces, and connections verified
- Edit-mode: 307/307 tests pass including 13 `AdServiceTests` and analytics event tests
- Integration: requires Unity Editor play mode with Advertisement Legacy package resolved and test game IDs active
- UAT: see S01–S04 UAT files

## What Needs Human Verification

1. **SDK package resolution** — Open Unity Editor, let it reimport. Verify Advertisement Legacy appears in Package Manager
2. **Play mode — rewarded ad** — Start a level, lose, tap Watch Ad. Real test ad should play. On completion: hearts restored. On skip: treated as Retry
3. **Play mode — interstitial** — Complete 3 levels in a row. After the 3rd level complete popup, interstitial should show before returning to main menu
4. **Play mode — unavailable state** — Test with `SimulateLoaded=false` or before SDK initializes. Watch button should gray out, message shows, no crash
