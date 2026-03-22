# Singular MMP Setup

Setup guide for Singular Mobile Measurement Partner integration — used for UA attribution and ad revenue ROAS reporting alongside LevelPlay.

Singular sits alongside LevelPlay. LevelPlay serves the ads; Singular attributes installs and tracks ad revenue back to the campaigns that drove them. No Singular account or SDK is needed for ads to work — this is purely for UA measurement.

**What's already done:**
- `ISingularService` / `NullSingularService` / `SingularService` wired throughout
- `UnityAdService` calls `ISingularService.ReportAdRevenue` on every ad impression (rewarded and interstitial)
- `GameBootstrapper` constructs `SingularService` and passes it to `UnityAdService`
- Behind `SINGULAR_ENABLED` compile guard — project compiles and runs cleanly without the SDK

**What you need to do:**
1. Create a Singular account and app
2. Install the Singular Unity SDK
3. Add the `SingularSDKObject` prefab to the Boot scene
4. Paste SDK Key + Secret into the prefab Inspector
5. Set the scripting define
6. Add the assembly reference

---

## Step 1: Create a Singular Account and App

1. Sign up at [app.singular.net](https://app.singular.net)
2. Create a new app — one per platform (iOS and Android separately)
3. Go to **Developer Tools → SDK Integration → SDK Keys**
4. Note your **SDK Key** and **SDK Secret** — you'll paste these into Unity in Step 4

> Use the SDK Key from the **SDK Integration** page — not the Reporting API Key. They are different.

---

## Step 2: Install the Singular Unity SDK

Singular is available via Unity Package Manager:

1. In Unity: **Window → Package Manager**
2. Click the **+** button → **Add package by name**
3. Enter: `com.singular.sdk`
4. Click **Add**

If the package isn't found by name, download the `.unitypackage` from the [Singular Unity SDK releases page](https://github.com/singular-labs/Singular-Unity-SDK/releases) and import it manually.

---

## Step 3: Add the SingularSDKObject to the Boot Scene

Singular initializes via a prefab GameObject — no code required:

1. Open the **Boot scene** (`Assets/Scenes/Boot.unity`)
2. In the Project pane, navigate to **Packages → Singular → SingularSDK → Prefabs**
3. Drag **SingularSDKObject** into the scene Hierarchy

> The GameObject must be named exactly `SingularSDKObject`. Do not rename it.

---

## Step 4: Paste SDK Key and Secret

1. Click **SingularSDKObject** in the Hierarchy
2. In the Inspector, find the **Singular SDK** script component
3. Paste your **SDK Key** into the **Singular API Key** field
4. Paste your **SDK Secret** into the **Singular API Secret** field

Other Inspector settings to review:

| Setting | Recommended value |
|---|---|
| Initialize On Awake | ✅ Enabled |
| SKAN Enabled (iOS) | ✅ Enabled |
| Wait For Tracking Authorization | `300` if you show ATT prompt, `0` otherwise |
| Enable Logging | ✅ during development, disable before shipping |

> If you're showing an iOS App Tracking Transparency (ATT) prompt, set **Wait For Tracking Authorization** to `300` seconds. This delays the Singular session until after the user responds, so the IDFA can be captured if consent is granted.

---

## Step 5: Enable the Scripting Define

1. Go to **Edit → Project Settings → Player**
2. Select your target platform (iOS or Android)
3. Under **Other Settings → Scripting Define Symbols**, add:
   ```
   SINGULAR_ENABLED
   ```
4. Click **Apply**

Repeat for each platform.

---

## Step 6: Add the Assembly Reference

1. Select `Assets/Scripts/Game/SimpleGame.Game.asmdef`
2. In the Inspector, under **Assembly Definition References**, click **+**
3. Add `Singular`
4. Click **Apply**

---

## What Gets Reported Automatically

Once active, Singular receives an ad revenue event on every ad impression — both rewarded and interstitial. The event includes:

| Field | Source |
|---|---|
| Network name | LevelPlay `ImpressionData.AdNetwork` (e.g. "UnityAds") |
| Currency | LevelPlay `ImpressionData.Currency` (defaults to "USD") |
| Revenue | LevelPlay `ImpressionData.Revenue` (impression-level CPM estimate) |

This data feeds Singular's ROAS calculation — you can run UA campaigns targeting users who are likely to generate ad revenue.

No additional event calls are needed in game code. Session start is handled automatically by the `SingularSDKObject` prefab on Awake.

---

## LevelPlay ↔ Singular Integration (Optional)

To connect LevelPlay's ad revenue postbacks directly to Singular at the campaign level (not just impression level), configure the integration in both dashboards:

1. In Singular: **Partner Configuration → LevelPlay** — enter your LevelPlay account credentials
2. In LevelPlay dashboard: **Monetize → Ad Revenue Attribution** — enable Singular as a revenue attribution partner

This allows Singular to receive server-to-server revenue data in addition to the client-side events from the SDK.

---

## Verification

After completing the steps above, enter Play mode and check the console:

**SDK initialized (auto on Awake):**
```
[Singular] Session started
```

**Ad revenue reported (after an ad impression):**
```
[SingularService] AdRevenue reported — network=UnityAds currency=USD revenue=0.000150
```

**Without SINGULAR_ENABLED:**
```
[SingularService] SINGULAR_ENABLED not set — AdRevenue suppressed: network=UnityAds revenue=0.000150
```

To verify events are reaching Singular, use the **Singular SDK Console** at [app.singular.net](https://app.singular.net) → your app → **SDK Console**.

---

## Troubleshooting

**`SingularSDK` type not found** — Assembly reference (`Singular`) not added in Step 6, or SDK not installed.

**No events in Singular SDK Console** — SDK Key or Secret is wrong. Confirm you're using the **SDK Integration** key, not the Reporting API key.

**IDFA not captured on iOS** — ATT prompt is not shown, or **Wait For Tracking Authorization** is set to `0` when it should be `300`. Users who deny ATT will still be attributed via SKAdNetwork.

**`SingularSDKObject` not found at runtime** — The prefab was not added to the Boot scene, or was renamed. The name must be exactly `SingularSDKObject`.

**Revenue shows as 0** — LevelPlay impression-level revenue is an estimate that may be `null` for some networks. The code defaults to `0` and skips reporting when revenue is zero or negative.

---

## Feature Availability Summary

| Feature | Requirement |
|---|---|
| Install attribution | Steps 1–5 complete |
| Ad revenue reporting | Steps 1–6 + LevelPlay active (`LEVELPLAY_ENABLED`) |
| ROAS campaigns | Singular account + campaigns configured at app.singular.net |
| LevelPlay server-to-server revenue | Optional LevelPlay ↔ Singular dashboard integration |
