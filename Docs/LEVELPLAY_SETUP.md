# Unity LevelPlay (Ads Mediation) Setup

Setup guide for rewarded and interstitial ads via Unity LevelPlay.

The ad service code is already written and integrated — `UnityAdService` implements `IAdService` for the LevelPlay SDK. Until the steps below are complete, all ad calls silently return `AdResult.NotLoaded` and gameplay continues normally.

---

## Overview

LevelPlay is Unity's ad mediation platform (formerly ironSource). It connects to Unity Ads and other networks through a single SDK. For this project, only the Unity Ads adapter is needed — no other networks required.

**What's already done:**
- `IAdService` / `NullAdService` / `UnityAdService` wired throughout the game
- Rewarded ad popup grays Watch button when unavailable
- Interstitial fires after every N level wins (N from remote config, default 3)
- All ad analytics events wired to PlayFab

**What you need to do:**
1. Create a LevelPlay account and app
2. Install the Ads Mediation Unity package
3. Install the Unity Ads adapter
4. Paste your App Key into `GameBootstrapper`
5. Set a scripting define
6. Add the assembly reference
7. Add the remote config key for interstitial frequency

---

## Step 1: Create a LevelPlay Account

1. Sign up at [platform.ironsource.io](https://platform.ironsource.io)
2. Click **New App** in the dashboard
3. Fill in:
   - **Platform:** iOS or Android (create one per platform)
   - **App Name:** your game name
   - **Category:** Games → Puzzle
   - **Store URL:** leave blank for now if the app isn't published yet — you can update it later
4. Click **Add App**
5. On the next screen, note your **App Key** — it looks like `1a2b3c4d`. You'll need this in Step 4.

---

## Step 2: Configure Ad Units

LevelPlay creates default ad units automatically. Confirm these exist in your dashboard:

1. In the LevelPlay dashboard, go to your app → **Ad Units**
2. You should see:
   - `DefaultRewardedVideoStoreId` (Rewarded Video)
   - `DefaultInterstitialStoreId` (Interstitial)
3. If they don't exist, click **Create Ad Unit** for each

The code uses these default IDs. If you rename them in the dashboard, update `LevelPlayAdUnitId.Rewarded` and `LevelPlayAdUnitId.Interstitial` in `UnityAdService.cs`.

---

## Step 3: Install the Ads Mediation Package

In the Unity Editor:

1. Go to **Window → Package Manager**
2. Set the dropdown to **Unity Registry**
3. Search for **Ads Mediation**
4. Select the package and click **Install**

The package name is `com.unity.services.levelplay`. After install, an **Ads Mediation** menu appears in the Unity top bar.

---

## Step 4: Install the Unity Ads Adapter

LevelPlay needs a per-network adapter to serve Unity Ads fill:

1. In Unity, go to **Ads Mediation → Integration Manager** (or **Ads Mediation → LevelPlay Network Manager**)
2. Find **Unity Ads** in the network list
3. Click **Install** next to the Unity Ads adapter
4. Wait for the download to complete

After installing, resolve native dependencies:

- **Android:** Go to **Assets → Mobile Dependency Manager → Android Resolver → Force Resolve**
- **iOS:** Runs automatically via CocoaPods when you build — no manual step

---

## Step 5: Paste Your App Key

Open `Assets/Scripts/Game/Boot/GameBootstrapper.cs` and find:

```csharp
unityAdService.Initialize(appKey: "YOUR_LEVELPLAY_APP_KEY");
```

Replace `"YOUR_LEVELPLAY_APP_KEY"` with your actual App Key from Step 1. For example:

```csharp
unityAdService.Initialize(appKey: "1a2b3c4d");
```

If you have separate iOS and Android apps in LevelPlay (recommended), use a compile guard:

```csharp
#if UNITY_IOS
unityAdService.Initialize(appKey: "your-ios-key");
#else
unityAdService.Initialize(appKey: "your-android-key");
#endif
```

---

## Step 6: Enable the Scripting Define

The LevelPlay implementation in `UnityAdService` is compiled behind `#if LEVELPLAY_ENABLED`. Once the package is installed and your key is set:

1. Go to **Edit → Project Settings → Player**
2. Select your target platform (iOS or Android)
3. Under **Other Settings → Scripting Define Symbols**, add:
   ```
   LEVELPLAY_ENABLED
   ```
4. Click **Apply**

Do this for each platform you're building for.

---

## Step 7: Add the Assembly Reference

`UnityAdService.cs` lives in the `SimpleGame.Game` assembly. With `LEVELPLAY_ENABLED` active, it references LevelPlay types that need an explicit assembly reference:

1. Select `Assets/Scripts/Game/SimpleGame.Game.asmdef` in the Project window
2. In the Inspector, under **Assembly Definition References**, click **+**
3. Add `Unity.Services.LevelPlay`
4. Click **Apply**

---

## Step 8: Configure Interstitial Frequency (Optional)

The interstitial shows after every N level completions. N is read from PlayFab Title Data at boot (default: 3).

To change it without a code deploy:

1. Open **PlayFab Game Manager → your title → Content → Title Data**
2. Add a new item:
   - **Key:** `interstitial_every_n_levels`
   - **Value:** any integer (e.g. `5` for every 5 levels, `0` to disable entirely)
3. Save — takes effect on next game boot

---

## Verification

After completing the steps above, enter Play mode and check the console:

**Successful init:**
```
[UnityAdService] LevelPlay.Init called — appKey=1a2b3c4d
[UnityAdService] LevelPlay initialized — loading ads.
[UnityAdService] Rewarded loaded.
[UnityAdService] Interstitial loaded.
```

**Test rewarded ad:**
1. Start a level and lose with no hearts remaining
2. Tap **Watch Ad** in the level-failed popup
3. A test ad should play (LevelPlay shows test ads by default on non-production builds)
4. On completion: hearts are restored and the level continues
5. On skip or close: treated as Retry — level resets

**Test interstitial:**
1. Complete 3 levels in a row
2. After the 3rd level-complete popup dismisses, an interstitial should appear before the main menu

**Test unavailable state:**
- With `LEVELPLAY_ENABLED` off (or before SDK initializes), the Watch Ad button should be grayed out with the message "Ad not available right now." — the popup still works and Skip is functional.

---

## Troubleshooting

**`LEVELPLAY_ENABLED` is set but types are missing** — The assembly reference (`Unity.Services.LevelPlay`) in Step 7 is not added. Unity will show CS0246 errors.

**`[UnityAdService] LevelPlay init failed`** — App Key is wrong or the app hasn't been approved in the LevelPlay dashboard. Verify the key at platform.ironsource.io → your app.

**Rewarded ad doesn't load** — Unity Ads adapter is not installed (Step 4). Confirm it appears in the Integration Manager with a green checkmark.

**Android resolver errors** — Run **Assets → Mobile Dependency Manager → Android Resolver → Force Resolve** after any adapter install or update.

**CocoaPods errors on iOS build** — Run `pod repo update` in the `ios/` build output directory, then rebuild.

**Ads show on editor but not device** — Device must be registered as a test device in the LevelPlay dashboard, or test mode must be enabled at the account level. Go to platform.ironsource.io → your app → **Test Suite**.

**`interstitial_every_n_levels` not taking effect** — The key must be in **Title Data** (not Player Data) in PlayFab. Check the boot log: `[RemoteConfig] Loaded — ... interstitialEveryN:N`.

---

## Feature Availability Summary

| Feature | Requirement |
|---|---|
| Rewarded ad (unavailable state) | No SDK needed — button grays with message |
| Rewarded ad (real flow) | Steps 1–7 complete |
| Interstitial (real flow) | Steps 1–7 complete |
| Interstitial frequency control | PlayFab Title Data key `interstitial_every_n_levels` (optional) |
| Ad analytics events | Automatic once PlayFab login works (M016) |
