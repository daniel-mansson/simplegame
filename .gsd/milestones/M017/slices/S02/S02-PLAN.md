# S02: Rewarded Ad — Real Flow

**Goal:** Replace the stub `HandleRewardedAdAsync` in `InGameSceneController` with a real `IAdService`-based flow. Update `RewardedAdPresenter` to call the service and handle the unavailable state. Extend `IRewardedAdView` with `SetWatchInteractable(bool)`.

**Demo:** Lose a level, tap Watch Ad — real Unity Ads test video plays, completes, hearts restored. Tap Watch Ad when no ad is loaded — button grays out, status text says "Ad not available right now.", no crash.

## Must-Haves

- `IRewardedAdView` has `SetWatchInteractable(bool)` method
- `RewardedAdView` binds it to `_watchButton.interactable`
- `RewardedAdPresenter` calls `IAdService.ShowRewardedAsync`; on `NotLoaded` → `SetWatchInteractable(false)` + `UpdateStatus("Ad not available right now.")`; on `Completed` → resolves `WaitForResult()` with `true`; on `Skipped` or `Failed` → resolves with `false`
- `InGameSceneController.Initialize()` accepts `IAdService` parameter
- `InGameSceneController.HandleRewardedAdAsync` uses `IAdService` (no more stub dismiss)
- All existing `InGameTests` still pass with `NullAdService` injected

## Tasks

- [ ] **T01: Extend IRewardedAdView & Update RewardedAdPresenter**
  Add `SetWatchInteractable(bool)` to interface and MonoBehaviour; update `RewardedAdPresenter` to use `IAdService`.

- [ ] **T02: Wire IAdService into InGameSceneController**
  Add `IAdService` param to `Initialize()`, replace stub `HandleRewardedAdAsync`, update `GameBootstrapper` to construct and pass `UnityAdService`.

## Files Likely Touched

- `Assets/Scripts/Game/Popup/IRewardedAdView.cs`
- `Assets/Scripts/Game/Popup/RewardedAdView.cs`
- `Assets/Scripts/Game/Popup/RewardedAdPresenter.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Tests/EditMode/Game/PopupTests.cs` (mock update)
- `Assets/Tests/EditMode/Game/InGameTests.cs` (inject NullAdService)
- `Assets/Tests/EditMode/Game/SceneControllerTests.cs` (mock update)
