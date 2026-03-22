# M017: Unity Ads Integration — Rewarded & Interstitial

**Vision:** Replace the stub rewarded ad flow with real Unity Ads (Advertisement Legacy SDK), add interstitial ads after level complete at remote-config-controlled frequency, and handle all failure paths (no fill, adblocker, SDK not ready) gracefully without breaking the game.

## Success Criteria

- Player loses a level, taps Watch Ad, sees a real Unity Ads test video, gets hearts restored on completion
- Player taps Watch Ad when no ad is available — Watch button grays out, status message explains, no crash
- Player completes every N levels (N from remote config, default 3) — fullscreen interstitial shows before returning to main menu
- Interstitial fails to load — navigation proceeds silently, player sees no error
- All ad events (impression, completion, skip, failed-to-load) fire correctly in `NullAdService` tests
- `NullAdService` is the only ad implementation used in edit-mode tests

## Key Risks / Unknowns

- Advertisement Legacy SDK availability in Package Manager (marked Legacy, may need registry search)
- `UnityEngine.Advertisements` namespace may not compile in Editor without a platform build target set

## Proof Strategy

- SDK availability risk → retired in S01 by successfully installing the package and compiling `UnityAdService`
- Editor compile risk → retired in S01 by using `#if UNITY_ADS` guards so tests compile without the SDK

## Verification Classes

- Contract verification: NUnit edit-mode tests via `NullAdService` covering all `IAdService` contract paths
- Integration verification: Play mode with Unity Ads test game IDs — real ads show and complete correctly
- Operational verification: none
- UAT / human verification: Play mode manual test — lose a level, watch ad, verify hearts restored; complete 3 levels, verify interstitial appears

## Milestone Definition of Done

This milestone is complete only when all are true:

- All four slices are complete with passing tests
- `UnityAdService` compiles clean against Advertisement Legacy SDK
- Real rewarded ad shows in Play mode (test mode) and grants hearts on completion
- Interstitial shows after every N levels in Play mode (test mode) and skips silently when unavailable
- All ad analytics events wired and verified in test doubles
- `NullAdService` covers all failure paths in edit-mode tests
- `GameBootstrapper` constructs and wires `IAdService` correctly

## Requirement Coverage

- Covers: R001 (MVP separation), R002 (view independence), R006 (failure visibility)
- Partially covers: none
- Leaves for later: real Game IDs + ad unit IDs, GDPR consent, cross-session frequency caps
- Orphan risks: none

## Slices

- [ ] **S01: Ad Service Abstraction & SDK** `risk:high` `depends:[]`
  > After this: `IAdService`, `NullAdService`, and `UnityAdService` exist and compile; `NullAdService` contract tests pass; Advertisement Legacy package is installed.

- [ ] **S02: Rewarded Ad — Real Flow** `risk:medium` `depends:[S01]`
  > After this: losing a level and tapping Watch Ad triggers a real Unity Ads rewarded video; unavailable state grays the Watch button with an explanatory message.

- [ ] **S03: Interstitial — Post-Level Frequency** `risk:medium` `depends:[S01]`
  > After this: every N levels (remote-config-controlled, default 3), a real interstitial ad shows after level complete; failed loads are skipped silently.

- [ ] **S04: Ad Analytics** `risk:low` `depends:[S01,S02,S03]`
  > After this: all ad events (impression, completed, skipped, failed-to-load) fire through `IAnalyticsService` and are verified by edit-mode tests.

## Boundary Map

### S01 → S02, S03, S04

Produces:
- `IAdService` interface — `LoadRewarded()`, `ShowRewardedAsync(CancellationToken)` → `AdResult`, `IsRewardedLoaded`, `LoadInterstitial()`, `ShowInterstitialAsync(CancellationToken)` → `AdResult`, `IsInterstitialLoaded`, `Initialize(string gameIdIos, string gameIdAndroid, bool testMode)`
- `AdResult` enum — `Completed`, `Skipped`, `Failed`, `NotLoaded`
- `NullAdService` — configurable `SimulateLoaded` flag; returns `AdResult.Completed` or `AdResult.NotLoaded` deterministically
- `UnityAdService` — real implementation, bridges SDK callbacks to UniTask via `UniTaskCompletionSource`
- Advertisement Legacy package installed (`com.unity.ads` in `Packages/manifest.json`)
- `Assets/Scripts/Game/Services/IAdService.cs`, `NullAdService.cs`, `UnityAdService.cs`

Consumes:
- nothing (first slice)

### S02 → S04

Produces:
- `IRewardedAdView.SetWatchInteractable(bool)` — new method on the interface
- `RewardedAdPresenter` updated — calls `IAdService.ShowRewardedAsync`, handles `NotLoaded` by calling `SetWatchInteractable(false)` and `UpdateStatus("Ad not available right now.")`
- `InGameSceneController.HandleRewardedAdAsync` — replaces stub with `IAdService`-based flow
- `InGameSceneController.Initialize()` — accepts `IAdService` parameter

Consumes from S01:
- `IAdService` → `ShowRewardedAsync`, `IsRewardedLoaded`, `AdResult`

### S03 → S04

Produces:
- `GameRemoteConfig.InterstitialEveryNLevels` field (default 3)
- `PlayFabRemoteConfigService` fetches `"interstitial_every_n_levels"` key
- `InGameSceneController` — session-level `_levelsCompletedThisSession` counter; calls `IAdService.ShowInterstitialAsync` at frequency; silently continues on `Failed` or `NotLoaded`

Consumes from S01:
- `IAdService` → `ShowInterstitialAsync`, `IsInterstitialLoaded`, `AdResult`

### S04 → (milestone complete)

Produces:
- `IAnalyticsService` extended: `TrackAdImpression(string adType)`, `TrackAdCompleted(string adType)`, `TrackAdSkipped(string adType)`, `TrackAdFailedToLoad(string adType)`
- `PlayFabAnalyticsService` — implements new methods (fire-and-forget, no-op offline)
- `NullAnalyticsService` or mock updated in tests
- All ad events wired in `UnityAdService` show/load callbacks

Consumes from S01:
- `UnityAdService` show/load listener callbacks

Consumes from S02:
- Rewarded ad result paths (completed, skipped, failed)

Consumes from S03:
- Interstitial ad result paths (completed, failed-to-load)
