# T01: Extend IAnalyticsService & Wire Ad Events

**Slice:** S04
**Milestone:** M017

## Goal

Add four ad analytics methods to `IAnalyticsService`, implement them in `PlayFabAnalyticsService`, wire them into `UnityAdService` callbacks and `NullAdService`, pass `IAnalyticsService` to `UnityAdService` in `GameBootstrapper`, and write tests.

## Must-Haves

### Truths
- `IAnalyticsService` has `TrackAdImpression`, `TrackAdCompleted`, `TrackAdSkipped`, `TrackAdFailedToLoad` — all taking `string adType`
- `PlayFabAnalyticsService` implements all four with `WritePlayerEvent` (fire-and-forget, no-op if not logged in)
- All existing analytics tests still pass
- `AnalyticsServiceTests` has at least 4 new tests for ad events
- `UnityAdService` fires `TrackAdImpression` when ad starts showing, `TrackAdCompleted`/`TrackAdSkipped` on complete, `TrackAdFailedToLoad` on load failure
- `NullAdService` optionally calls analytics when `IAnalyticsService` is injected (for test verification)

### Artifacts
- `Assets/Scripts/Game/Services/IAnalyticsService.cs` — four methods added
- `Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs` — implemented
- `Assets/Scripts/Game/Services/UnityAdService.cs` — analytics calls wired
- `Assets/Scripts/Game/Services/NullAdService.cs` — optional analytics injection
- `Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs` — four new test cases

### Key Links
- `UnityAdService` → `IAnalyticsService` via constructor injection in `GameBootstrapper`
- `NullAdService` → optional `IAnalyticsService` via constructor parameter (default null)

## Steps

1. Read `IAnalyticsService.cs` in full. Add the four methods with XML doc comments following the existing pattern (`/// <summary>Fired when...`). `adType` value should be `"rewarded"` or `"interstitial"` — document this convention.
2. Read `PlayFabAnalyticsService.cs` in full. Implement the four methods using `WritePlayerEvent` — event names: `"ad_impression"`, `"ad_completed"`, `"ad_skipped"`, `"ad_failed_to_load"`. Include `adType` as an event body parameter. Follow exactly the existing fire-and-forget pattern.
3. Update `UnityAdService`:
   - Add `private IAnalyticsService _analytics;` field
   - Add `IAnalyticsService analytics = null` parameter to constructor (or add a `SetAnalytics(IAnalyticsService)` setter — constructor injection is preferred)
   - In `IUnityAdsShowListener.OnUnityAdsShowStart(adUnitId)`: call `_analytics?.TrackAdImpression(AdTypeFrom(adUnitId))`
   - In `IUnityAdsShowListener.OnUnityAdsShowComplete(adUnitId, completionState)`: call `TrackAdCompleted` or `TrackAdSkipped` based on state
   - In `IUnityAdsLoadListener.OnUnityAdsFailedToLoad(adUnitId, error, message)`: call `_analytics?.TrackAdFailedToLoad(AdTypeFrom(adUnitId))`
   - Add private helper `AdTypeFrom(string adUnitId)` that returns `"rewarded"` if the unit ID matches `_rewardedAdUnitId`, else `"interstitial"`
4. Update `NullAdService`:
   - Add `IAnalyticsService Analytics { get; set; }` property (null by default)
   - In `ShowRewardedAsync`: if `SimulateLoaded` call `Analytics?.TrackAdImpression("rewarded")`; after result, call `TrackAdCompleted` or `TrackAdSkipped`; if not loaded call `TrackAdFailedToLoad("rewarded")`
   - Same for `ShowInterstitialAsync`
5. Update `GameBootstrapper` — pass `_analyticsService` to `UnityAdService` constructor (or call `_adService.SetAnalytics(_analyticsService)` before passing to `InGameSceneController`).
6. Find all mock implementations of `IAnalyticsService` in test files (grep). Add stub implementations for all four new methods.
7. Write `AnalyticsServiceTests` additions (or new test class if needed):
   - `AdImpression_CallsTrackAdImpression` — use a `NullAdService` with mock analytics, show an ad, assert impression fired
   - `AdCompleted_CallsTrackAdCompleted` — `SimulateLoaded=true`, assert completed event
   - `AdSkipped_CallsTrackAdSkipped` — need a `NullAdService` that returns `Skipped`; add `SimulateResult` property to `NullAdService`
   - `AdFailedToLoad_CallsTrackAdFailedToLoad` — `SimulateLoaded=false`, assert failed-to-load event
8. Add `AdResult.Skipped` path to `NullAdService` — add `AdResult SimulateResult { get; set; }` property (default `AdResult.Completed`). When `SimulateLoaded = true`, return `SimulateResult`. This allows tests to simulate skipped without needing to set `SimulateLoaded = false`.
9. Run LSP diagnostics on all touched files. Fix any errors.

## Context

- Event name convention: `snake_case`, matching existing analytics events (`session_start`, `level_completed` etc.). Ad events: `ad_impression`, `ad_completed`, `ad_skipped`, `ad_failed_to_load`.
- `adType` string values: `"rewarded"` and `"interstitial"` — lowercase, matching the SDK ad unit type names.
- `TrackAdImpression` fires when the ad actually starts playing (SDK `OnUnityAdsShowStart`), not when `Show` is called. This more accurately represents an impression in advertising terms.
- `NullAdService.SimulateResult` is backward-compatible: existing tests that only set `SimulateLoaded = true` still get `AdResult.Completed` as before (default value).
- The `PlayFabAnalyticsService` fire-and-forget pattern: no `await`, no try/catch — identical to `TrackLevelCompleted`. PlayFab errors are logged by the SDK internally.
- K004 applies: all mock `IAnalyticsService` implementations need the four new stub methods. Grep for `IAnalyticsService` in test files before finishing.
