---
id: S03
parent: M017
milestone: M017
provides:
  - GameRemoteConfig.InterstitialEveryNLevels field (default 3)
  - PlayFabRemoteConfigService fetches "interstitial_every_n_levels" PlayFab Title Data key
  - InGameSceneController._levelsCompletedThisSession counter — session-scoped, incremented on each win
  - HandleInterstitialAsync — shows ad at N-level threshold; silently continues on NotLoaded/Failed
  - RemoteConfigServiceTests: default value assertion added
key_files:
  - Assets/Scripts/Game/Services/GameRemoteConfig.cs
  - Assets/Scripts/Game/Services/PlayFabRemoteConfigService.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/RemoteConfigServiceTests.cs
key_decisions:
  - Session counter is instance field reset at app launch — no cross-session persistence (D092)
  - HandleInterstitialAsync catches all exceptions → AdResult.Failed (no crash on SDK error)
  - InterstitialEveryNLevels = 0 disables interstitials entirely
drill_down_paths:
  - .gsd/milestones/M017/slices/S03/S03-PLAN.md
duration: ~15min
verification_result: pass
completed_at: 2026-03-20T19:45:00Z
---

# S03: Interstitial — Post-Level Frequency

**Interstitial ad shown after every N level completions (default 3, remote-config-controlled); silently skipped on failure.**

## What Happened

Added `InterstitialEveryNLevels` to `GameRemoteConfig` (default 3) and fetched from PlayFab via key `"interstitial_every_n_levels"`. In `InGameSceneController`, a `_levelsCompletedThisSession` counter increments on each win. When the counter hits the N-level threshold, `HandleInterstitialAsync` is called — it shows the interstitial via `_adService.ShowInterstitialAsync` and swallows any failure silently, letting navigation continue normally. The frequency check uses `_interstitialEveryNLevels > 0` so setting it to 0 disables the feature.

## Deviations

None.

## Files Created/Modified

- `Assets/Scripts/Game/Services/GameRemoteConfig.cs` — InterstitialEveryNLevels field + Default
- `Assets/Scripts/Game/Services/PlayFabRemoteConfigService.cs` — key fetch + parse
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — counter, HandleInterstitialAsync, win path
- `Assets/Tests/EditMode/Game/RemoteConfigServiceTests.cs` — default value test added
