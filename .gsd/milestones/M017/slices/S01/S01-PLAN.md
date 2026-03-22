# S01: Ad Service Abstraction & SDK

**Goal:** Install the Unity Ads Advertisement Legacy package, define `IAdService` with `AdResult`, implement `NullAdService` (test double) and `UnityAdService` (real SDK wrapper), and verify the contract with edit-mode tests.

**Demo:** `NullAdService` passes all contract tests. `UnityAdService` compiles clean. Advertisement Legacy package is present in `Packages/manifest.json`.

## Must-Haves

- `IAdService` interface exists with: `Initialize`, `LoadRewarded`, `ShowRewardedAsync`, `IsRewardedLoaded`, `LoadInterstitial`, `ShowInterstitialAsync`, `IsInterstitialLoaded`
- `AdResult` enum exists with: `Completed`, `Skipped`, `Failed`, `NotLoaded`
- `NullAdService` returns `AdResult.Completed` when `SimulateLoaded = true`, `AdResult.NotLoaded` when `false`
- `UnityAdService` compiles against Advertisement Legacy SDK with no errors
- Advertisement Legacy package is in `Packages/manifest.json` under `com.unity.ads`
- Edit-mode tests cover: rewarded completed, rewarded not loaded, interstitial completed, interstitial not loaded

## Tasks

- [x] **T01: Install SDK & Define IAdService**
  Install Advertisement Legacy via manifest, define `IAdService`, `AdResult`, and create files.

- [x] **T02: NullAdService + UnityAdService + Tests**
  Implement both service classes, add `IAdService` reference to `SimpleGame.Game.asmdef`, write contract tests.

## Files Likely Touched

- `Packages/manifest.json`
- `Assets/Scripts/Game/Services/IAdService.cs` (new)
- `Assets/Scripts/Game/Services/AdResult.cs` (new)
- `Assets/Scripts/Game/Services/NullAdService.cs` (new)
- `Assets/Scripts/Game/Services/UnityAdService.cs` (new)
- `Assets/Scripts/Game/SimpleGame.Game.asmdef`
- `Assets/Tests/EditMode/Game/AdServiceTests.cs` (new)
