# T01: Install SDK & Define IAdService

**Slice:** S01
**Milestone:** M017

## Goal

Install the Unity Ads Advertisement Legacy package via `Packages/manifest.json` and define the `IAdService` interface and `AdResult` enum that the rest of M017 builds against.

## Must-Haves

### Truths
- `Packages/manifest.json` contains `"com.unity.ads": "4.12.2"` (or latest 4.x)
- `Assets/Scripts/Game/Services/IAdService.cs` exists and declares the full interface
- `Assets/Scripts/Game/Services/AdResult.cs` exists with `Completed`, `Skipped`, `Failed`, `NotLoaded`
- No compiler errors after manifest edit (project still compiles clean)

### Artifacts
- `Assets/Scripts/Game/Services/IAdService.cs` — full interface with XML doc comments
- `Assets/Scripts/Game/Services/AdResult.cs` — enum

### Key Links
- `IAdService.cs` uses `AdResult` from `AdResult.cs` — same namespace, same assembly

## Steps

1. Read `Packages/manifest.json` — note current format and dependencies section.
2. Add `"com.unity.ads": "4.12.2"` to the dependencies block. (Unity Ads 4.12 is the latest Advertisement Legacy release.)
3. Create `Assets/Scripts/Game/Services/AdResult.cs` with enum values: `Completed`, `Skipped`, `Failed`, `NotLoaded`.
4. Create `Assets/Scripts/Game/Services/IAdService.cs` with the full interface:
   - `void Initialize(string gameIdIos, string gameIdAndroid, bool testMode)`
   - `void LoadRewarded()`
   - `UniTask<AdResult> ShowRewardedAsync(CancellationToken ct = default)`
   - `bool IsRewardedLoaded { get; }`
   - `void LoadInterstitial()`
   - `UniTask<AdResult> ShowInterstitialAsync(CancellationToken ct = default)`
   - `bool IsInterstitialLoaded { get; }`
5. Verify both files compile (no LSP errors).

## Context

- Unity Ads Advertisement Legacy is in the Unity Package Registry. Add it as `"com.unity.ads": "4.12.2"` — Unity will resolve it on next project open.
- `AdResult.NotLoaded` is the path taken when `IsRewardedLoaded` or `IsInterstitialLoaded` is false at the time `ShowXxxAsync` is called — the caller should check this before calling Show.
- Keep `IAdService` in `SimpleGame.Game.Services` namespace alongside all other service interfaces.
- No `#if` guards needed in the interface itself — those live only in `UnityAdService`.
