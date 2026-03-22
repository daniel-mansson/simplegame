---
id: S04
parent: M017
milestone: M017
provides:
  - IAnalyticsService: TrackAdImpression, TrackAdCompleted, TrackAdSkipped, TrackAdFailedToLoad
  - PlayFabAnalyticsService: implements all four (fire-and-forget, no-op offline)
  - UnityAdService: calls all four events in show/load callbacks
  - NullAdService: forwards events to injected IAnalyticsService
  - MockAnalyticsService: tracks all four with counters and LastAdType
  - PlayFabAnalytics_AdEvents_WhenNotLoggedIn_DoNotThrow test
key_files:
  - Assets/Scripts/Game/Services/IAnalyticsService.cs
  - Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs
  - Assets/Scripts/Game/Services/UnityAdService.cs
  - Assets/Scripts/Game/Services/NullAdService.cs
  - Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs
  - Assets/Tests/EditMode/Game/AdServiceTests.cs
key_decisions:
  - Ad analytics methods added to IAnalyticsService in S01 (not deferred to S04) to avoid forward-reference compile errors
  - adType convention: "rewarded" or "interstitial" string values
  - TrackAdImpression fires in OnUnityAdsShowStart (actual playback start, not Show() call)
drill_down_paths:
  - .gsd/milestones/M017/slices/S04/S04-PLAN.md
duration: ~10min
verification_result: pass
completed_at: 2026-03-20T19:55:00Z
---

# S04: Ad Analytics

**All ad analytics events wired — impression, completed, skipped, failed-to-load — across IAnalyticsService, PlayFabAnalyticsService, UnityAdService, and NullAdService.**

## What Happened

Analytics wiring was largely done in S01 to resolve compile dependencies. S04 confirmed all paths are covered by the 13 AdServiceTests (impression/completed/skipped/failed-to-load via NullAdService) and added an offline guard test for the four new PlayFabAnalyticsService methods. MockAnalyticsService tracks all four events with counters for test assertions.

## Deviations

- S04 was primarily a verification slice — the implementation was already complete from S01. T01 added one test; all other work was done.

## Files Created/Modified

- `Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs` — PlayFabAnalytics_AdEvents_WhenNotLoggedIn_DoNotThrow test added
