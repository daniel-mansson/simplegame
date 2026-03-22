# T01: Extend IRewardedAdView & Update RewardedAdPresenter

**Slice:** S02
**Milestone:** M017

## Goal

Add `SetWatchInteractable(bool)` to `IRewardedAdView` and `RewardedAdView`, then update `RewardedAdPresenter` to accept `IAdService` and drive the real ad flow — including the unavailable-state UI.

## Must-Haves

### Truths
- `IRewardedAdView` declares `void SetWatchInteractable(bool interactable)`
- `RewardedAdView.SetWatchInteractable` sets `_watchButton.interactable`
- `RewardedAdPresenter` constructor accepts `IRewardedAdView` and `IAdService`
- When `IAdService.IsRewardedLoaded` is false at Watch click: `SetWatchInteractable(false)` is called, `UpdateStatus("Ad not available right now.")` is called, `WaitForResult()` task remains pending (player must use Skip)
- When `ShowRewardedAsync` returns `AdResult.Completed`: `WaitForResult()` resolves `true`
- When `ShowRewardedAsync` returns `AdResult.Skipped` or `AdResult.Failed`: `WaitForResult()` resolves `false`
- All mock implementations of `IRewardedAdView` in test files are updated with the new method

### Artifacts
- `Assets/Scripts/Game/Popup/IRewardedAdView.cs` — `SetWatchInteractable(bool)` added
- `Assets/Scripts/Game/Popup/RewardedAdView.cs` — binding implemented
- `Assets/Scripts/Game/Popup/RewardedAdPresenter.cs` — rewired to use `IAdService`

### Key Links
- `RewardedAdPresenter` → `IAdService` via constructor injection
- `RewardedAdPresenter` → `IRewardedAdView.SetWatchInteractable` on ad unavailable
- `RewardedAdPresenter` → `IAdService.ShowRewardedAsync` on Watch click

## Steps

1. Read `IRewardedAdView.cs`, `RewardedAdView.cs`, `RewardedAdPresenter.cs` in full.
2. Add `void SetWatchInteractable(bool interactable)` to `IRewardedAdView`.
3. Implement in `RewardedAdView`: `public void SetWatchInteractable(bool interactable) => _watchButton.interactable = interactable;`
4. Rewrite `RewardedAdPresenter`:
   - Constructor: `(IRewardedAdView view, IAdService adService)`
   - `Initialize()`: subscribe buttons, call `View.UpdateStatus("Watch a short ad for a reward?")`, call `View.SetWatchInteractable(adService.IsRewardedLoaded)` — gray out immediately if not loaded
   - `HandleWatch()`: if `!_adService.IsRewardedLoaded` → `View.SetWatchInteractable(false); View.UpdateStatus("Ad not available right now."); return;` — else `ShowAdAsync().Forget()`
   - `ShowAdAsync()`: private `UniTaskVoid`, calls `await _adService.ShowRewardedAsync()`, resolves `_completeTcs` based on `AdResult`
   - `Dispose()`: unsubscribe, cancel TCS
5. Find all mock implementations of `IRewardedAdView` in test files (grep for `MockRewardedAdView` or `IRewardedAdView`). Add `public void SetWatchInteractable(bool interactable) { }` stub to each.
6. Run LSP diagnostics on all touched files. Fix any errors.

## Context

- `RewardedAdPresenter` is a plain C# class — constructor injection is the right pattern (consistent with all other presenters in the project).
- `IAdService` is provided by `InGameSceneController` which holds the reference. `RewardedAdPresenter` does not own the service lifetime.
- The "ad not available" state: Watch button grays out, message updates, but the popup stays open. The player can still tap Skip to close it and return to the LevelFailed options. The TCS is intentionally left pending until Skip is clicked.
- `ShowAdAsync` is a fire-and-forget `UniTaskVoid` because `HandleWatch` is an event handler (void return). The TCS bridges the result back to `WaitForResult()`.
- K004 applies: any change to `IRewardedAdView` requires updating all mock implementations in test files.
