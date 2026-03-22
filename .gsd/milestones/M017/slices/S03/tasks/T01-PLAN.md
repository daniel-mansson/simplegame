# T01: GameRemoteConfig + Fetch + Session Counter + Interstitial Call

**Slice:** S03
**Milestone:** M017

## Goal

Add `InterstitialEveryNLevels` to `GameRemoteConfig`, fetch it from PlayFab, track a session-level win counter in `InGameSceneController`, and call `IAdService.ShowInterstitialAsync` at the correct frequency on level complete — silently continuing if the ad fails.

## Must-Haves

### Truths
- `GameRemoteConfig.InterstitialEveryNLevels` defaults to `3`
- `PlayFabRemoteConfigService` fetches key `"interstitial_every_n_levels"` and applies it
- `InGameSceneController` increments `_levelsCompletedThisSession` on every win
- Interstitial is attempted when `_levelsCompletedThisSession % _interstitialEveryNLevels == 0 && _levelsCompletedThisSession > 0`
- `AdResult.Failed` or `AdResult.NotLoaded` from `ShowInterstitialAsync` → no error logged to user, navigation continues
- Edit-mode test: after N wins with `NullAdService(SimulateLoaded=true)`, interstitial fires; with `SimulateLoaded=false`, navigation still completes without error

### Artifacts
- `Assets/Scripts/Game/Services/GameRemoteConfig.cs` — `InterstitialEveryNLevels = 3` in both struct and `Default`
- `Assets/Scripts/Game/Services/PlayFabRemoteConfigService.cs` — key fetch added
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — counter field, interstitial call in win path, `_interstitialEveryNLevels` field set from remoteConfig

### Key Links
- `InGameSceneController._interstitialEveryNLevels` ← `remoteConfig.InterstitialEveryNLevels` in `Initialize()`
- `InGameSceneController` win path → `_adService.ShowInterstitialAsync()` when counter hits threshold

## Steps

1. Read `GameRemoteConfig.cs` in full. Add `public int InterstitialEveryNLevels;` field. Update `Default` to include `InterstitialEveryNLevels = 3`.
2. Read `PlayFabRemoteConfigService.cs` in full. Add `"interstitial_every_n_levels"` to the keys list. Parse it the same way as other int keys — validate `> 0`. Assign to `cfg.InterstitialEveryNLevels`.
3. Read `InGameSceneController.cs` — find `Initialize()` and the win path in `RunAsync`.
4. Add `private int _levelsCompletedThisSession = 0;` field.
5. Add `private int _interstitialEveryNLevels = 3;` field.
6. In `Initialize()`, in the `remoteConfig.HasValue` block: add `_interstitialEveryNLevels = remoteConfig.Value.InterstitialEveryNLevels;`
7. In `RunAsync`, in the `action == InGameAction.Win` branch, after `HandleLevelCompletePopupAsync` and before `return ScreenId.MainMenu`:
   ```csharp
   _levelsCompletedThisSession++;
   if (_interstitialEveryNLevels > 0 && _levelsCompletedThisSession % _interstitialEveryNLevels == 0)
   {
       var interstitialResult = await HandleInterstitialAsync(ct);
       Debug.Log($"[InGameSceneController] Interstitial result: {interstitialResult}");
   }
   ```
8. Implement `HandleInterstitialAsync(CancellationToken ct)` as a private method:
   ```csharp
   private async UniTask<AdResult> HandleInterstitialAsync(CancellationToken ct)
   {
       if (_adService == null || !_adService.IsInterstitialLoaded)
           return AdResult.NotLoaded;
       try { return await _adService.ShowInterstitialAsync(ct); }
       catch { return AdResult.Failed; }
   }
   ```
   No user-visible error in any failure path — log only.
9. Write or extend `InGameTests` with an interstitial frequency test:
   - Create `InGameSceneController`, call `Initialize(adService: new NullAdService { SimulateLoaded = true })`, set `_interstitialEveryNLevels` via remoteConfig or directly
   - Simulate N wins, verify interstitial was attempted (track via a `CountingNullAdService` subclass or by checking analytics in S04)
   - Simulate win with `SimulateLoaded = false` — verify no exception thrown
10. Write or extend `RemoteConfigServiceTests` to assert `InterstitialEveryNLevels` is parsed correctly.
11. Run LSP diagnostics on all touched files. Fix any errors.

## Context

- The counter is session-scoped (`_levelsCompletedThisSession` is an instance field, not persisted). This is intentional — cross-session frequency caps are out of scope for M017.
- The modulo guard `&& _levelsCompletedThisSession > 0` prevents a spurious interstitial at zero (belt-and-suspenders, since the win path always increments before checking).
- `HandleInterstitialAsync` catches all exceptions because the SDK can throw if initialization failed. Treat any exception as `AdResult.Failed`.
- After `ShowInterstitialAsync` completes (any result), `UnityAdService` should automatically call `LoadInterstitial()` again to pre-load for the next time. This is handled in `UnityAdService.OnUnityAdsShowComplete` (wired in S01/T02). `InGameSceneController` does not need to call `LoadInterstitial` manually.
- Remote config key name: `"interstitial_every_n_levels"` — match exactly, PlayFab Title Data is case-sensitive.
