# T02: Wire IAdService into InGameSceneController

**Slice:** S02
**Milestone:** M017

## Goal

Add `IAdService` to `InGameSceneController.Initialize()`, replace the stub `HandleRewardedAdAsync` with the real flow using `RewardedAdPresenter` and `IAdService`, and wire `UnityAdService` construction in `GameBootstrapper`.

## Must-Haves

### Truths
- `InGameSceneController.Initialize()` accepts `IAdService adService = null` parameter
- `HandleRewardedAdAsync` shows the `RewardedAd` popup, creates a `RewardedAdPresenter` with the real `IAdService`, awaits `WaitForResult()`, then dismisses the popup
- When `WaitForResult()` returns `true` (ad completed): `presenter.RestoreHeartsAndContinue()` is called (this already happens in the outer `RunAsync` loop — verify the flow is correct)
- When `WaitForResult()` returns `false` (skipped/failed/unavailable): behaviour is same as skip (no reward — the outer loop already handles this, review and confirm)
- `GameBootstrapper` constructs `UnityAdService` with test game IDs and `testMode: true`, calls `Initialize()`, passes it to `InGameSceneController`
- All existing `InGameTests` pass with `NullAdService(SimulateLoaded: true)` injected into `Initialize()`

### Artifacts
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — `Initialize()` signature updated, `HandleRewardedAdAsync` rewritten, `_adService` field added
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — `UnityAdService` constructed and passed

### Key Links
- `InGameSceneController` → `IAdService` via `Initialize()` parameter
- `InGameSceneController.HandleRewardedAdAsync` → `RewardedAdPresenter(view, _adService)`
- `GameBootstrapper` → `UnityAdService` → `InGameSceneController.Initialize(adService: ...)`

## Steps

1. Read `InGameSceneController.cs` `Initialize()` signature and `HandleRewardedAdAsync` in full.
2. Add `private IAdService _adService;` field to `InGameSceneController`.
3. Add `IAdService adService = null` as a new optional parameter to `Initialize()` — assign to `_adService`. Optional with null default so the self-bootstrap path still compiles.
4. Rewrite `HandleRewardedAdAsync`:
   ```
   - Get IRewardedAdView from _viewResolver (pattern matches ActiveLevelCompleteView)
   - If view null: log warning, return false (no reward)
   - Create RewardedAdPresenter(view, _adService ?? new NullAdService())
   - presenter.Initialize()
   - await _popupManager.ShowPopupAsync(PopupId.RewardedAd, ct)
   - bool result = await presenter.WaitForResult()
   - await _popupManager.DismissPopupAsync(ct)
   - presenter.Dispose()
   - return result
   ```
5. Confirm the `RunAsync` call site for `HandleRewardedAdAsync` already uses the return value correctly for `RestoreHeartsAndContinue`. The existing stub returned void — the rewrite returns `bool` but the outer loop already branches on `LevelFailedChoice.WatchAd` then calls `RestoreHeartsAndContinue()` unconditionally. Verify this is still correct: if ad is skipped/failed the player should NOT get restored hearts. Adjust outer loop to only call `RestoreHeartsAndContinue()` when `HandleRewardedAdAsync` returns true.
6. Read `GameBootstrapper.cs` — find where `InGameSceneController.Initialize()` is called (in the `ScreenId.InGame` case).
7. Add `private IAdService _adService;` field to `GameBootstrapper`.
8. In `GameBootstrapper.Start()`, after services are built, construct `UnityAdService`:
   - `_adService = new UnityAdService(); _adService.Initialize("5314539", "5314538", testMode: true);`
9. Pass `adService: _adService` to `ctrl.Initialize(...)` in the InGame case.
10. Update any `InGameTests` that call `ctrl.Initialize(...)` — add `adService: new NullAdService()` (or use the existing optional default if tests don't call Initialize directly).
11. Run LSP diagnostics on all touched files. Fix any errors.

## Context

- Step 5 is important: the existing stub always granted the reward. The real flow should only grant hearts if the ad was actually watched (result = true). The outer RunAsync loop must be adjusted: `if (choice == LevelFailedChoice.WatchAd)` → `bool adWatched = await HandleRewardedAdAsync(ct); if (adWatched) presenter.RestoreHeartsAndContinue(); else { /* treat as Retry or show LevelFailed again */ break; }`
- The fallback `new NullAdService()` in `HandleRewardedAdAsync` when `_adService` is null handles the play-from-editor self-bootstrap path. NullAdService with default `SimulateLoaded = true` means play-from-editor always grants the reward without a real ad.
- `UnityAdService.Initialize()` is synchronous (SDK initialization is async internally but the call returns immediately). The `OnInitializationComplete` callback triggers `LoadRewarded` + `LoadInterstitial` automatically. No need to await it in `GameBootstrapper`.
- Keep `IAdService adService = null` optional in `Initialize()` — don't break the method signature for existing callers. The self-bootstrap path never calls `Initialize()` (it constructs its own stub services and calls `RunAsync` directly).
