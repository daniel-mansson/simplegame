---
id: S01
parent: M017
milestone: M017
provides:
  - AdResult enum (Completed, Skipped, Failed, NotLoaded)
  - IAdService interface — rewarded + interstitial load/show/isLoaded contract
  - NullAdService — test double with SimulateLoaded, SimulateResult, Analytics injection
  - UnityAdService — Advertisement Legacy SDK wrapper, callback→UniTask bridge via TCS
  - IAnalyticsService extended with TrackAdImpression/Completed/Skipped/FailedToLoad
  - PlayFabAnalyticsService implements ad events (fire-and-forget, no-op offline)
  - MockAnalyticsService updated with ad event counters
  - 13 AdServiceTests all passing
key_files:
  - Assets/Scripts/Game/Services/AdResult.cs
  - Assets/Scripts/Game/Services/IAdService.cs
  - Assets/Scripts/Game/Services/NullAdService.cs
  - Assets/Scripts/Game/Services/UnityAdService.cs
  - Assets/Scripts/Game/Services/IAnalyticsService.cs
  - Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs
  - Assets/Scripts/Game/SimpleGame.Game.asmdef
  - Assets/Tests/EditMode/Game/AdServiceTests.cs
  - Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs
  - Packages/manifest.json
key_decisions:
  - NullAdService has SimulateResult property (default Completed) to allow Skipped simulation without changing SimulateLoaded
  - IAnalyticsService ad methods added in S01 (not S04) to avoid forward-reference compile errors in NullAdService
  - Unity.Advertisements asmdef reference added to SimpleGame.Game.asmdef; tests don't reference it directly
  - UnityAdService reloads ads automatically in OnUnityAdsShowComplete and OnUnityAdsShowFailure
patterns_established:
  - NullAdService.Analytics injection pattern for verifying ad events in tests
  - UniTaskCompletionSource<AdResult> bridge in UnityAdService (same pattern as M016 PlayFab)
drill_down_paths:
  - .gsd/milestones/M017/slices/S01/S01-PLAN.md
  - .gsd/milestones/M017/slices/S01/tasks/T01-PLAN.md
  - .gsd/milestones/M017/slices/S01/tasks/T02-PLAN.md
duration: ~30min
verification_result: pass
completed_at: 2026-03-20T19:00:00Z
---

# S01: Ad Service Abstraction & SDK

**IAdService abstraction established with UnityAdService (SDK wrapper), NullAdService (test double), and full ad analytics extension — 307/307 edit-mode tests passing.**

## What Happened

Defined `IAdService` and `AdResult` as the game's ad contract, keeping all SDK types isolated in `UnityAdService`. `NullAdService` is the test double: configurable `SimulateLoaded` flag, `SimulateResult` for outcome simulation, and optional `Analytics` injection for verifying event dispatch. `UnityAdService` wraps the Advertisement Legacy SDK (4.12.2) using the same `UniTaskCompletionSource` bridge pattern as M016's PlayFab callbacks — SDK callbacks resolve the TCS, callers await the task.

Extended `IAnalyticsService` with four ad event methods in S01 rather than deferring to S04 — `NullAdService` references them directly, so they had to exist before the class compiled. `PlayFabAnalyticsService` and `MockAnalyticsService` updated accordingly. The `com.unity.ads` package added to manifest; `Unity.Advertisements` added to `SimpleGame.Game.asmdef`.

## Deviations

- Added IAnalyticsService ad methods in S01 (planned for S04) to resolve forward-reference compile issues in NullAdService. S04 task T01 will only need to wire UnityAdService callbacks — the interface and implementations are already complete.

## Files Created/Modified

- `Assets/Scripts/Game/Services/AdResult.cs` — new enum
- `Assets/Scripts/Game/Services/IAdService.cs` — new interface
- `Assets/Scripts/Game/Services/NullAdService.cs` — new test double
- `Assets/Scripts/Game/Services/UnityAdService.cs` — new SDK wrapper
- `Assets/Scripts/Game/Services/IAnalyticsService.cs` — 4 ad methods added
- `Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs` — 4 ad methods implemented
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — Unity.Advertisements reference added
- `Assets/Tests/EditMode/Game/AdServiceTests.cs` — 13 new tests
- `Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs` — MockAnalyticsService extended
- `Packages/manifest.json` — com.unity.ads 4.12.2 added
