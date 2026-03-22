# T02: NullAdService + UnityAdService + Tests

**Slice:** S01
**Milestone:** M017

## Goal

Implement `NullAdService` (deterministic test double) and `UnityAdService` (real SDK wrapper bridging callbacks to UniTask), wire the assembly reference, and write edit-mode contract tests.

## Must-Haves

### Truths
- `NullAdService` with `SimulateLoaded = true` → `ShowRewardedAsync` returns `AdResult.Completed`
- `NullAdService` with `SimulateLoaded = false` → `ShowRewardedAsync` returns `AdResult.NotLoaded`
- Same two cases pass for `ShowInterstitialAsync`
- `UnityAdService` compiles with no errors (verified via LSP diagnostics)
- `SimpleGame.Game.asmdef` references `com.unity.ads` so `UnityAdService` can use `UnityEngine.Advertisements`
- Edit-mode test class `AdServiceTests` exists with at least 4 passing tests

### Artifacts
- `Assets/Scripts/Game/Services/NullAdService.cs` — implements `IAdService`, `SimulateLoaded` property, no SDK dependency
- `Assets/Scripts/Game/Services/UnityAdService.cs` — implements `IAdService`, `IUnityAdsInitializationListener`, `IUnityAdsLoadListener`, `IUnityAdsShowListener`, all under `#if UNITY_ADS` or unconditional if SDK compiles in Editor
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — updated with `"Unity.Advertisements"` reference
- `Assets/Tests/EditMode/Game/AdServiceTests.cs` — NUnit tests via `NullAdService`

### Key Links
- `UnityAdService` implements `IAdService` from `IAdService.cs`
- `AdServiceTests` uses `NullAdService` — no real SDK calls in tests

## Steps

1. Read `Assets/Scripts/Game/SimpleGame.Game.asmdef` — note current references format.
2. Implement `NullAdService`:
   - `bool SimulateLoaded` property (default `true`)
   - `IsRewardedLoaded` and `IsInterstitialLoaded` return `SimulateLoaded`
   - `LoadRewarded()` / `LoadInterstitial()` — no-op
   - `ShowRewardedAsync` — returns `UniTask.FromResult(SimulateLoaded ? AdResult.Completed : AdResult.NotLoaded)`
   - `ShowInterstitialAsync` — same
   - `Initialize(...)` — no-op
3. Implement `UnityAdService`:
   - Fields: `_rewardedAdUnitId`, `_interstitialAdUnitId` (per-platform strings)
   - `_rewardedTcs` and `_interstitialTcs` as `UniTaskCompletionSource<AdResult>` for bridging callbacks
   - `Initialize(...)` → `Advertisement.Initialize(gameId, testMode, this)` — platform-selected game ID
   - `LoadRewarded()` → `Advertisement.Load(_rewardedAdUnitId, this)`
   - `LoadInterstitial()` → `Advertisement.Load(_interstitialAdUnitId, this)`
   - `ShowRewardedAsync` — checks `IsRewardedLoaded`; if not, returns `AdResult.NotLoaded`; else creates TCS, calls `Advertisement.Show`, awaits TCS
   - `ShowInterstitialAsync` — same pattern
   - `IUnityAdsLoadListener.OnUnityAdsAdLoaded` — sets loaded flag for the matching ad unit
   - `IUnityAdsLoadListener.OnUnityAdsFailedToLoad` — logs warning, leaves loaded flag false
   - `IUnityAdsShowListener.OnUnityAdsShowComplete` — resolves TCS based on `UnityAdsShowCompletionState`
   - `IUnityAdsShowListener.OnUnityAdsShowFailure` — resolves TCS with `AdResult.Failed`
   - `IUnityAdsInitializationListener.OnInitializationComplete` — calls `LoadRewarded()` and `LoadInterstitial()` to pre-load
   - `IUnityAdsInitializationListener.OnInitializationFailed` — logs warning only
4. Add `"Unity.Advertisements"` to `SimpleGame.Game.asmdef` references array.
5. Write `AdServiceTests.cs`:
   - `RewardedCompleted_WhenSimulateLoaded` — `SimulateLoaded=true`, assert `AdResult.Completed`
   - `RewardedNotLoaded_WhenNotSimulateLoaded` — `SimulateLoaded=false`, assert `AdResult.NotLoaded`
   - `InterstitialCompleted_WhenSimulateLoaded`
   - `InterstitialNotLoaded_WhenNotSimulateLoaded`
6. Run LSP diagnostics on all new files. Fix any errors before marking done.

## Context

- Test IDs: iOS game ID `"5314539"`, Android `"5314538"`. Ad unit IDs for test: `"Rewarded_iOS"` / `"Rewarded_Android"` and `"Interstitial_iOS"` / `"Interstitial_Android"`. These are Unity's documented test IDs.
- The `UniTaskCompletionSource<AdResult>` bridge pattern is identical to what M016 uses for PlayFab callbacks — follow that pattern exactly.
- `Advertisement.Load` must be called again after each successful `Advertisement.Show` to pre-load the next ad. Handle this in `OnUnityAdsShowComplete`.
- `NullAdService` has zero dependency on `UnityEngine.Advertisements` — it must compile and run in EditMode without the SDK package installed. This is why tests use it exclusively.
- `UnityAdService` is a plain C# class, not a MonoBehaviour. It lives for the duration of the app. `GameBootstrapper` will hold the reference.
