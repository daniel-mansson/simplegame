# PlayFab & Platform Account Setup

Setup guide for PlayFab anonymous login, cloud save, Game Center (iOS), and Google Play Games (Android).

---

## 1. PlayFab (required for everything)

Anonymous login, cloud save, and analytics all depend on a configured PlayFab title.

### Steps

1. Create a PlayFab account at [playfab.com](https://playfab.com)
2. Create a **Title** in PlayFab Game Manager — note the **Title ID** (5-character alphanumeric, e.g. `AB12C`)
3. In the Unity Editor, select the asset:
   ```
   Assets/PlayFabSDK/Shared/Public/Resources/PlayFabSharedSettings
   ```
   Enter your Title ID in the **Title Id** Inspector field
4. Enter Play mode — you should see this in the console:
   ```
   [PlayFabAuth] Logged in. PlayFabId: XXXXX
   ```

Once this works, cloud save and analytics are also active. No further backend configuration is required for these features.

---

## 2. Game Center (iOS)

Game Center linking uses Unity's built-in `Social` API — no extra plugin is required.

### Prerequisites

- Apple Developer account
- Physical iOS device (Game Center does not work in Simulator)

### Steps

1. In [App Store Connect](https://appstoreconnect.apple.com), create an App record for the game
2. In **Certificates, Identifiers & Profiles**, select your App ID and enable the **Game Center** capability
3. In Unity: **Project Settings → Player → iOS → Capabilities** — enable **Game Center**
4. On the test device: sign into Game Center via **iOS Settings → Game Center**
5. Build and deploy to device
6. In the game, navigate to **Settings → Link Game Center** — the link status should update and a `platform_account_linked` event will appear in PlayFab Game Manager → Analytics → Event Explorer

### Notes

- The player's Game Center ID is read via `Social.localUser.id` after authentication
- If the user is not signed into Game Center, the link call returns false silently
- To verify the link: PlayFab Game Manager → Players → select player → **Linked Accounts**

---

## 3. Google Play Games (Android)

This is the most involved platform. The linking code is compiled behind `#if UNITY_ANDROID && GOOGLE_PLAY_GAMES` — it is a no-op until the scripting define is set.

### Prerequisites

- Google Play Console account
- App published to at least the **Internal Testing** track (required to enable Play Games Services)
- Google Cloud Console access (for OAuth client ID)

### Steps

#### 3a. Play Console & Cloud Console

1. In [Google Play Console](https://play.google.com/console), open your app
2. Go to **Play Games Services → Setup and management → Configuration**
3. Create a new Play Games Services project and link it to your app
4. Under **Credentials**, create an **OAuth 2.0 Web Client ID** — this is required for server auth codes
5. Note the **App ID** from the Play Games Services configuration page

#### 3b. Unity Plugin

1. Download the **Google Play Games Unity plugin** from:
   [github.com/playgameservices/play-games-plugin-for-unity/releases](https://github.com/playgameservices/play-games-plugin-for-unity/releases)
2. Import the `.unitypackage` into the project
3. In Unity: **Google → Play Games → Setup → Android Setup**
   - Paste your **App ID** into the configuration field
   - Click **Setup**

#### 3c. Scripting Define

In **Project Settings → Player → Android → Scripting Define Symbols**, add:
```
GOOGLE_PLAY_GAMES
```

This activates the Google Play Games linking code in `PlayFabPlatformLinkService.cs`.

#### 3d. Platform Initialization

Add the following initialization to `GameBootstrapper.Start()` inside the `#if UNITY_ANDROID && GOOGLE_PLAY_GAMES` block, before the navigation loop:

```csharp
#if UNITY_ANDROID && GOOGLE_PLAY_GAMES
var config = new PlayGamesClientConfiguration.Builder()
    .RequestServerAuthCode(false)
    .Build();
PlayGamesPlatform.InitializeInstance(config);
PlayGamesPlatform.Activate();
#endif
```

#### 3e. Testing

- The test device must be registered as a **tester** in Play Console, or the app must be on an internal/closed track the device has access to
- Build and deploy an Android APK/AAB
- In the game, navigate to **Settings → Link Google Play** — the link status should update

### Notes

- Until the `GOOGLE_PLAY_GAMES` define is set, the **Link Google Play** button returns false silently with a console log
- The server auth code flow requires the Web Client ID to be configured correctly in Play Console — without it, `GetServerAuthCode()` returns null
- To verify the link: PlayFab Game Manager → Players → select player → **Linked Accounts**

---

## Feature Availability Summary

| Feature | Requirement |
|---|---|
| Anonymous login | Title ID set in `PlayFabSharedSettings` |
| Cloud save (push/pull) | Anonymous login working |
| Analytics events | Anonymous login working |
| Game Center linking | iOS device + App Store Connect + Game Center capability |
| Google Play Games linking | GPGS plugin + `GOOGLE_PLAY_GAMES` define + Play Console setup |
| First-launch link prompt | Automatic once login works — shows if no platform linked and not previously skipped |

---

## Troubleshooting

**`[PlayFabAuth] Login failed`** — Title ID is missing or wrong in `PlayFabSharedSettings`. Check for typos; the ID is case-sensitive.

**`[PlatformLink] Google Play Games linking requires the GPGS plugin`** — `GOOGLE_PLAY_GAMES` scripting define is not set, or the plugin is not imported.

**`[PlatformLink] Google Play Games server auth code unavailable`** — Plugin is installed but not initialised, or the OAuth Web Client ID is not configured in Play Console.

**`[PlatformLink] Game Center link failed`** — User is not signed into Game Center on device, or the App ID does not have Game Center enabled in App Store Connect.

**Cloud save not syncing** — Login must succeed first. Check `[PlayFabAuth]` log lines. If offline, local save is used and sync resumes on next successful login.
