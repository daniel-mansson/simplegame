# S03: Interstitial — Post-Level Frequency

**Goal:** Show a real interstitial ad after every N level completions (N from remote config). Failed loads skip silently. Level counter is session-scoped.

**Demo:** Complete 3 levels in a row — after the third level complete popup is dismissed, a fullscreen interstitial plays before returning to main menu. Complete 1 or 2 levels — no interstitial. Kill ad fill (NullAdService SimulateLoaded=false) — interstitial silently skipped, main menu appears normally.

## Must-Haves

- `GameRemoteConfig` has `int InterstitialEveryNLevels` field, default `3`
- `PlayFabRemoteConfigService` fetches `"interstitial_every_n_levels"` key
- `InGameSceneController` has `_levelsCompletedThisSession` counter, incremented on each win
- Interstitial is attempted when `_levelsCompletedThisSession % InterstitialEveryNLevels == 0`
- On `AdResult.Failed` or `AdResult.NotLoaded`: navigation continues normally (no error surfaced to player)
- Edit-mode tests verify: interstitial fires at correct interval; skipped when NullAdService not loaded

## Tasks

- [ ] **T01: GameRemoteConfig + Fetch + Session Counter**
  Add field to `GameRemoteConfig`, add PlayFab fetch key, add counter and interstitial call to `InGameSceneController`.

## Files Likely Touched

- `Assets/Scripts/Game/Services/GameRemoteConfig.cs`
- `Assets/Scripts/Game/Services/PlayFabRemoteConfigService.cs`
- `Assets/Scripts/Game/InGame/InGameSceneController.cs`
- `Assets/Tests/EditMode/Game/RemoteConfigServiceTests.cs`
- `Assets/Tests/EditMode/Game/InGameTests.cs`
