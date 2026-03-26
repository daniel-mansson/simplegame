# T02: Create Ads/ Folder and Move 7 Ads Files

**Slice:** S01
**Milestone:** M020

## Goal

Move all Ads-related source files from `Services/` and `Popup/` into a new `Assets/Scripts/Game/Ads/` folder using `git mv`.

## Must-Haves

### Artifacts
- `Assets/Scripts/Game/Ads/` exists with these 7 files:
  - From `Services/`: `IAdService.cs`, `AdResult.cs`, `UnityAdService.cs`, `NullAdService.cs`
  - From `Popup/`: `IRewardedAdView.cs`, `RewardedAdPresenter.cs`, `RewardedAdView.cs`

### Truths
- `find Assets/Scripts/Game/Ads -name "*.cs" | wc -l` returns 7
- `rg "AdService\|AdResult\|RewardedAd" Assets/Scripts/Game/Services/ Assets/Scripts/Game/Popup/` returns no matches

## Steps

1. `git mv Assets/Scripts/Game/Services/IAdService.cs Assets/Scripts/Game/Ads/IAdService.cs`
2. `git mv Assets/Scripts/Game/Services/AdResult.cs Assets/Scripts/Game/Ads/AdResult.cs`
3. `git mv Assets/Scripts/Game/Services/UnityAdService.cs Assets/Scripts/Game/Ads/UnityAdService.cs`
4. `git mv Assets/Scripts/Game/Services/NullAdService.cs Assets/Scripts/Game/Ads/NullAdService.cs`
5. `git mv Assets/Scripts/Game/Popup/IRewardedAdView.cs Assets/Scripts/Game/Ads/IRewardedAdView.cs`
6. `git mv Assets/Scripts/Game/Popup/RewardedAdPresenter.cs Assets/Scripts/Game/Ads/RewardedAdPresenter.cs`
7. `git mv Assets/Scripts/Game/Popup/RewardedAdView.cs Assets/Scripts/Game/Ads/RewardedAdView.cs`
8. Verify 7 renames staged

## Context

- No source file edits — namespaces unchanged
- Do NOT commit yet — wait for T03
