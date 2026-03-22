---
id: S04
milestone: M016
provides:
  - IAnalyticsService interface (8 track methods)
  - PlayFabAnalyticsService — WritePlayerEvent, no-op when not logged in
  - CoinsService: Earn/TrySpend fire TrackCurrencyEarned/Spent
  - GoldenPieceService: Earn/TrySpend fire TrackCurrencyEarned/Spent
  - InGameSceneController: TrackLevelStarted on loop entry, TrackLevelCompleted/Failed on outcomes
  - GameBootstrapper: TrackSessionStart after login, TrackSessionEnd on pause and quit
  - PlatformLinkPresenter: TrackPlatformLinked on successful link
  - MockAnalyticsService — reusable test double
  - 9 edit-mode tests: mock contract + offline guard
requires:
  - slice: S01
    provides: IPlayFabAuthService, IsLoggedIn guard
  - slice: S02
    provides: CoinsService, GoldenPieceService (modified)
  - slice: S03
    provides: PlatformLinkPresenter (modified)
affects: []
key_files:
  - Assets/Scripts/Game/Services/IAnalyticsService.cs
  - Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs
  - Assets/Scripts/Game/Services/CoinsService.cs
  - Assets/Scripts/Game/Services/GoldenPieceService.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs
  - Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs
key_decisions:
  - "Analytics is fire-and-forget — no UniTask wrapping; WritePlayerEvent callbacks are log-only"
  - "CoinsService/GoldenPieceService accept optional IAnalyticsService — backward compatible with all existing tests"
  - "IAnalyticsService injected into InGameSceneController via new analytics param on Initialize"
  - "session_end fired on both OnApplicationPause and OnApplicationQuit to cover both mobile and editor"
patterns_established:
  - "MockAnalyticsService public in test assembly — usable for future analytics verification tests"
drill_down_paths:
  - .gsd/milestones/M016/slices/S04/S04-PLAN.md
duration: 40min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# S04: Analytics Events

**All four analytics event types wired to PlayFab via WritePlayerEvent — session, level, currency, and platform account linking.**

## What Was Built

`IAnalyticsService` defines 8 track methods. `PlayFabAnalyticsService` sends events via `WritePlayerEvent` with no-op guard on `IsLoggedIn`. Analytics is fire-and-forget (no UniTask wrapping needed).

Hooks placed at all required lifecycle points: session start/end in `GameBootstrapper`, level events in `InGameSceneController`, currency events in `CoinsService` and `GoldenPieceService` (optional constructor parameter — backward compatible), platform linked in `PlatformLinkPresenter`.

`MockAnalyticsService` is public with per-method call counters and last-value tracking — useful for future tests that need to verify event dispatch.

All services constructors remain backward-compatible (analytics is optional). No changes to existing tests required.

## Deviations

None from plan.

## Files Created/Modified
- `Assets/Scripts/Game/Services/IAnalyticsService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs` — new
- `Assets/Scripts/Game/Services/CoinsService.cs` — analytics optional param + hooks
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — analytics optional param + hooks
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — session events + analytics passed to services/controllers
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — level events
- `Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs` — platform_account_linked event
- `Assets/Scripts/Game/Boot/UIFactory.cs` — CreatePlatformLinkPresenter accepts analytics
- `Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs` — new (9 tests)
