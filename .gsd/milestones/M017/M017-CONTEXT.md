# M017: Unity Ads Integration — Rewarded & Interstitial

**Gathered:** 2026-03-20
**Status:** Ready for planning

## Project Description

A Unity mobile jigsaw puzzle game. Players draw jigsaw pieces from a slot-based deck, placing them on a board subject to neighbour-adjacency constraints. The core game loop, meta-progression, PlayFab backend, and distribution pipeline are all complete as of M016.

## Why This Milestone

The game has a stub rewarded ad flow (`HandleRewardedAdAsync` in `InGameSceneController`) that shows no real ad — it just dismisses a popup and grants the reward unconditionally. This needs to be replaced with real Unity Ads (Advertisement Legacy SDK). Interstitial ads are not implemented at all. Both ad types are needed for monetisation before launch.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Lose a level, tap "Watch Ad", see a real Unity Ads rewarded video, and get hearts restored
- Tap "Watch Ad" when no ad is available and see the button gray out with an explanatory message — not a broken popup or crash
- Complete every N levels (N from remote config, default 3) and see a real interstitial ad before returning to main menu
- Complete a level when no interstitial is available and return to main menu normally, with no visible error

### Entry point / environment

- Entry point: Play mode in Unity Editor (test mode ads), deployed iOS/Android build (real ads)
- Environment: local dev with Unity Ads test mode; iOS/Android device for real fill verification
- Live dependencies involved: Unity Ads SDK (Advertisement Legacy), PlayFab remote config (interstitial frequency key)

## Completion Class

- Contract complete means: `NullAdService` passes all contract tests; `UnityAdService` compiles clean; all failure paths have test coverage via `NullAdService`
- Integration complete means: real rewarded and interstitial ads show in Unity Editor play mode with test game IDs
- Operational complete means: none (no daemon, no service lifecycle beyond SDK init at boot)

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- In Play mode with test game IDs: lose a level → Watch Ad → real test ad shows → completes → hearts restored
- In Play mode with test game IDs: complete a level → interstitial shows at correct frequency (every N levels)
- With ad intentionally not loaded (SDK not initialized or NullAdService): Watch Ad → button grays, message shown, no crash; interstitial → silently skipped, navigation continues
- All ad analytics events fire in test doubles

## Risks and Unknowns

- **Advertisement Legacy SDK availability in Package Manager** — the package is marked "Legacy" and may require Unity Registry to be enabled. If it's not in the registry, it can be pulled by version string from the Package Manager UI. Not a blocker but worth verifying before planning S01 implementation steps.
- **`UnityEngine.Advertisements` namespace requires platform build target** — the SDK may not compile correctly in Editor without a platform set to iOS or Android. The `IAdService` abstraction with compile-time guards (`#if UNITY_ADS` or `#if !UNITY_EDITOR`) is the mitigation.
- **Test game IDs are fixed strings** — Unity provides known test game IDs (`"5314539"` iOS, `"5314538"` Android) and test ad unit IDs. These are hardcoded in `UnityAdService` behind a `testMode: true` flag. Real IDs come via remote config or a ScriptableObject in a future milestone.
- **Interstitial frequency = remote config field** — `InterstitialEveryNLevels` added to `GameRemoteConfig` with default 3. PlayFab key: `"interstitial_every_n_levels"`. `InGameSceneController` tracks a session-level level-complete counter.

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Popup/RewardedAdPresenter.cs` — current stub presenter; `WaitForResult()` returns `UniTask<bool>`. The Watch/Skip button flow will be repurposed: Watch attempts real ad load, grays out with message if unavailable.
- `Assets/Scripts/Game/Popup/IRewardedAdView.cs` — `UpdateStatus(string)`, `OnWatchClicked`, `OnSkipClicked`. Needs `SetWatchInteractable(bool)` added for the unavailable state.
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — Unity MonoBehaviour implementing `IRewardedAdView`. Needs corresponding `_watchButton.interactable` binding.
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — `HandleRewardedAdAsync` is the stub to replace. `Initialize()` already accepts `IAnalyticsService`; it will also accept `IAdService`.
- `Assets/Scripts/Game/Services/GameRemoteConfig.cs` — add `InterstitialEveryNLevels` field, default 3.
- `Assets/Scripts/Game/Services/PlayFabRemoteConfigService.cs` — add `"interstitial_every_n_levels"` key fetch.
- `Assets/Scripts/Game/Services/IAnalyticsService.cs` — extend with ad event methods.
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — construct and wire `UnityAdService`; pass to `InGameSceneController.Initialize()`.
- `Assets/Tests/EditMode/Game/InGameTests.cs` — existing test patterns for `InGameSceneController`; `NullAdService` will be used here.

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R001 — MVP separation: `IAdService` follows the same interface-first pattern as all other services
- R002 — View independence: `RewardedAdView` must not reference `IAdService` directly
- R006 — Failure visibility: all ad failure paths must surface clearly (rewarded) or silently-but-safely (interstitial)

## Scope

### In Scope

- Unity Ads Advertisement Legacy SDK installation (Package Manager)
- `IAdService` interface with `LoadRewarded`, `ShowRewarded`, `LoadInterstitial`, `ShowInterstitial`, and `IsRewardedLoaded` / `IsInterstitialLoaded` checks
- `UnityAdService` — real implementation using `UnityEngine.Advertisements`
- `NullAdService` — test/editor implementation, configurable to simulate loaded or not-loaded state
- `RewardedAdPresenter` updated to call `IAdService.ShowRewarded` and handle unavailable state
- `IRewardedAdView` extended with `SetWatchInteractable(bool)`
- `InGameSceneController.HandleRewardedAdAsync` replaced with real flow
- Interstitial shown after level complete at configurable frequency (session counter)
- `InterstitialEveryNLevels` in `GameRemoteConfig` + PlayFab fetch
- Ad analytics events: impression, completed, skipped, failed-to-load (for both ad types)
- Edit-mode tests for all failure paths using `NullAdService`

### Out of Scope / Non-Goals

- Banner ads
- Ad mediation (Unity Mediation / LevelPlay / IronSource)
- Real Game IDs and ad unit IDs (placeholder test IDs used; real IDs wired via future remote config work)
- GDPR/consent dialogs
- Ad frequency cap storage across sessions (session-only counter is sufficient for now)

## Technical Constraints

- `UnityEngine.Advertisements` is only available after the Advertisement Legacy package is installed — all references must be guarded so the project compiles without the package during test runs if needed
- The `IAdService` callbacks are callback-based in the SDK (not async/await native); they must be bridged to `UniTask` via `UniTaskCompletionSource`, following the same pattern as PlayFab callbacks in M016
- `NullAdService` must be usable in EditMode tests without any Unity Ads SDK reference
- `RewardedAdPresenter` must remain a plain C# class (no MonoBehaviour) per R001

## Integration Points

- Unity Ads SDK — `Advertisement.Initialize`, `Advertisement.Load`, `Advertisement.Show` via `IUnityAdsInitializationListener`, `IUnityAdsLoadListener`, `IUnityAdsShowListener`
- `GameBootstrapper` — constructs `UnityAdService`, passes to `InGameSceneController`
- `PlayFabRemoteConfigService` — fetches `interstitial_every_n_levels` key
- `IAnalyticsService` / `PlayFabAnalyticsService` — new ad event methods

## Open Questions

- None — all decisions locked above.
