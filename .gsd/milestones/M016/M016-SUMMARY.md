---
id: M016
provides:
  - PlayFab SDK 2.230.260123 installed at Assets/PlayFabSDK/
  - Anonymous PlayFab account on first launch via LoginWithCustomID
  - Player ID persisted in PlayerPrefs; same account recovered across sessions
  - Cloud save: MetaSaveData pushed/pulled from PlayFab User Data with take-max merge
  - Platform account linking: Game Center (iOS) and Google Play Games (Android)
  - First-launch platform link prompt (skippable, shows once)
  - Settings screen extended with link status and link/unlink buttons
  - Analytics: session_start/end, level_started/completed/failed, currency_earned/spent, platform_account_linked
  - 35+ edit-mode tests covering auth contract, merge logic, mock services, analytics dispatch
key_files:
  - Assets/PlayFabSDK/ (SDK source, 239 files)
  - Assets/Scripts/Game/Services/IPlayFabAuthService.cs
  - Assets/Scripts/Game/Services/PlayFabAuthService.cs
  - Assets/Scripts/Game/Services/ICloudSaveService.cs
  - Assets/Scripts/Game/Services/PlayFabCloudSaveService.cs
  - Assets/Scripts/Game/Services/MetaSaveMerge.cs
  - Assets/Scripts/Game/Services/IPlatformLinkService.cs
  - Assets/Scripts/Game/Services/PlayFabPlatformLinkService.cs
  - Assets/Scripts/Game/Services/IAnalyticsService.cs
  - Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Scripts/Game/Settings/ISettingsView.cs
  - Assets/Scripts/Game/Settings/SettingsPresenter.cs
  - Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs
key_decisions:
  - PlayFab SDK installed via git sparse-checkout from GitHub (no unitypackage)
  - IMetaSaveService stays synchronous; cloud sync is explicit async in GameBootstrapper
  - Take-max per field conflict resolution (coins, golden pieces, object steps)
  - PlayFab callbacks bridged to UniTask via UniTaskCompletionSource
  - Login failure is non-fatal — game continues offline
  - Google Play Games uses #if UNITY_ANDROID — graceful stub on non-Android
  - Analytics is fire-and-forget (no UniTask wrapping)
  - First-launch prompt skip persisted in PlayerPrefs
completed_at: 2026-03-20T00:00:00Z
---

# M016: PlayFab Integration — Accounts, Cloud Save & Analytics

**PlayFab backend fully integrated: anonymous accounts, cloud save with take-max merge, platform linking, and analytics events wired throughout the session lifecycle.**

## What Was Built

### S01: SDK + Anonymous Login
PlayFab Unity SDK 2.230.260123 installed from GitHub via sparse-checkout (no manual unitypackage import needed). `IPlayFabAuthService` / `PlayFabAuthService` implements silent anonymous login using `LoginWithCustomID` with `SystemInfo.deviceUniqueIdentifier`. `GameBootstrapper.Start()` awaits login before constructing any game services. Login failure is non-fatal — game continues offline with `IsLoggedIn=false` guarding all cloud operations. UniTask bridge pattern established via `UniTaskCompletionSource`.

### S02: Cloud Save Sync
`MetaSaveData` serialized to JSON and stored in PlayFab User Data under key `"MetaSave"`. `MetaSaveMerge.TakeMax` implements field-level merge: `coins = max(local, cloud)`, `goldenPieces = max(local, cloud)`, `objectProgress[i].currentSteps = max(local, cloud)`. Pull happens at boot before gameplay services are constructed — merged data is the ground truth for the session. Push happens on `OnApplicationPause`, level complete, and level quit via `onSessionEnd` callback.

### S03: Platform Linking & First-Launch Prompt
`PlayFabPlatformLinkService` links Game Center (iOS, `#if UNITY_IOS`) and Google Play Games (Android, `#if UNITY_ANDROID`) to the PlayFab account. `ISettingsView` extended with link/unlink events; `SettingsPresenter` handles the actions. First-launch prompt (`PlatformLinkPresenter`) appears once after login if no platform is linked and the player hasn't previously skipped. Skip is permanent (PlayerPrefs flag).

### S04: Analytics Events
`PlayFabAnalyticsService` sends custom events via `WritePlayerEvent` (fire-and-forget, no-op offline). All 8 event types wired: session start/end in `GameBootstrapper`, level events in `InGameSceneController`, currency events in `CoinsService` and `GoldenPieceService`, platform linked in `PlatformLinkPresenter`.

## Verification Status

- Static verification: all types confirmed against SDK source. All API calls, model fields, and asmdef references verified.
- Runtime verification: requires Unity Editor open with PlayFab Title ID configured in `PlayFabSharedSettings`. UAT scripts at S01–S04 UAT files.
- Edit-mode tests: 35+ tests covering auth contract, merge logic, mock services, analytics dispatch. Pending editor recompile to run.

## What Needs Human Verification

1. **PlayFab Title ID** — configure in `Assets/PlayFabSDK/Shared/Public/Resources/PlayFabSharedSettings` Inspector field before Play mode
2. **Play mode boot** — verify `[PlayFabAuth] Logged in. PlayFabId: XXXXX` appears in console
3. **Cloud save round-trip** — see S02 UAT
4. **Game Center linking** — iOS device required; see S03 UAT
5. **Event History** — verify events appear in PlayFab Game Manager; see S04 UAT
