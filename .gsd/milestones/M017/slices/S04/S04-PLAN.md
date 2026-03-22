# S04: Ad Analytics

**Goal:** Extend `IAnalyticsService` with ad event methods, implement in `PlayFabAnalyticsService`, wire calls into `UnityAdService` (show callbacks) and `NullAdService` (no-op). Verify with edit-mode tests.

**Demo:** All ad outcomes — impression, completed, skipped, failed-to-load — emit the correct analytics event in the test double.

## Must-Haves

- `IAnalyticsService` declares: `TrackAdImpression(string adType)`, `TrackAdCompleted(string adType)`, `TrackAdSkipped(string adType)`, `TrackAdFailedToLoad(string adType)`
- `PlayFabAnalyticsService` implements all four (fire-and-forget, no-op offline — same pattern as existing methods)
- `UnityAdService` calls `TrackAdImpression` in `OnUnityAdsShowStart`, `TrackAdCompleted` in `OnUnityAdsShowComplete` when `Completed`, `TrackAdSkipped` when `Skipped`, `TrackAdFailedToLoad` in `OnUnityAdsFailedToLoad`
- `NullAdService` accepts an optional `IAnalyticsService` and calls events when configured
- `AnalyticsServiceTests` has tests for all four new events
- All existing `AnalyticsServiceTests` still pass

## Tasks

- [ ] **T01: Extend IAnalyticsService & Wire Ad Events**
  Add methods to interface and `PlayFabAnalyticsService`, wire into `UnityAdService`, update mocks.

## Files Likely Touched

- `Assets/Scripts/Game/Services/IAnalyticsService.cs`
- `Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs`
- `Assets/Scripts/Game/Services/NullAdService.cs`
- `Assets/Scripts/Game/Services/UnityAdService.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` (pass analytics to UnityAdService)
- `Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs`
