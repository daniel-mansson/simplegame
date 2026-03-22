# M016: PlayFab Integration — Accounts, Cloud Save & Analytics

**Vision:** Every player has a PlayFab identity from the moment they first launch. Progress is backed up to the cloud and survives reinstall. Players can link their Game Center or Google Play Games account for cross-device recovery. The full session and economy loop is instrumented with analytics events.

## Success Criteria

- Fresh install creates an anonymous PlayFab account; same account recovered on next launch
- `MetaSaveData` round-trips through PlayFab cloud; take-max merge applied on boot
- Game Center and Google Play Games accounts can be linked and unlinked from Settings
- First-launch prompt offers platform linking before the main menu
- PlayFab Game Manager shows session, level, currency, and linking events from a real session

## Key Risks / Unknowns

- **Google Play Games Unity plugin** — separate SDK install, gradle config, potential Unity 6 conflicts; must be proven before platform linking slice commits to Android
- **PlayFab SDK manual install** — no UPM path; `.unitypackage` import is a manual step before auto-mode can compile S01
- **Callback-to-UniTask bridging** — all PlayFab API calls need UniTask adapters; straightforward but must be done consistently

## Proof Strategy

- Google Play Games plugin risk → retire in S01 by proving it installs cleanly and compiles in Unity 6; if blocked, Android linking scoped to stub with clear error
- PlayFab SDK install → retire in S01 by proving `PlayFabClientAPI.LoginWithCustomID` compiles and returns a valid Player ID in Play mode

## Verification Classes

- Contract verification: unit tests for take-max merge logic; mock PlayFab client for login/save/load flows
- Integration verification: Play mode boot logs a valid PlayFab Player ID; cloud push/pull verified by reading back via PlayFab Game Manager dashboard
- Operational verification: platform linking verified on a real iOS device (Game Center); Android verified if Google Play Games plugin installs cleanly
- UAT / human verification: PlayFab Game Manager Event History confirms all four event types appear from a test session

## Milestone Definition of Done

This milestone is complete only when all are true:

- All four slices deliver their must-haves
- `GameBootstrapper.Start()` completes PlayFab login and cloud pull before navigation loop starts
- `MetaSaveData` round-trips: local → PlayFab → fresh local via take-max merge
- Settings screen shows platform link status and provides working link/unlink buttons
- Analytics events appear in PlayFab Game Manager for a real Play mode or device session
- All existing edit-mode tests continue to pass (no regression)

## Requirement Coverage

- Covers: R137, R138, R139, R140, R141, R142, R143, R144, R145, R146, R147, R148
- Partially covers: none
- Leaves for later: R149 (conflict UI, deferred)
- Orphan risks: none

## Slices

- [ ] **S01: SDK + Anonymous Login** `risk:high` `depends:[]`
  > After this: Boot logs a valid PlayFab Player ID in Play mode; same Player ID returned on second launch (custom ID recovery proven).

- [ ] **S02: Cloud Save Sync** `risk:medium` `depends:[S01]`
  > After this: Uninstall and reinstall (simulated via PlayerPrefs.DeleteAll + fresh Play mode) restores progress from PlayFab cloud with take-max merge applied.

- [ ] **S03: Platform Linking & First-Launch Prompt** `risk:medium` `depends:[S01]`
  > After this: Settings screen shows link status; Game Center linking works on iOS device; first-launch prompt appears once and is permanently skippable.

- [ ] **S04: Analytics Events** `risk:low` `depends:[S01]`
  > After this: PlayFab Game Manager Event History shows session_start, level_started, level_completed, currency_earned events from a Play mode session.

## Boundary Map

### S01 → S02, S03, S04

Produces:
- `IPlayFabAuthService` — `UniTask LoginAsync()`, `string PlayFabId`, `bool IsLoggedIn`
- `PlayFabAuthService` — concrete implementation; stores Player ID in PlayerPrefs
- `PlayFabSettings.TitleId` configured via `PlayFabSharedSettings` ScriptableObject
- UniTask adapter pattern: `PlayFabCallbackExtensions.ToUniTask<T>()` bridging callback API to UniTask
- `GameBootstrapper` integration point: `await _authService.LoginAsync()` before navigation loop

Consumes:
- nothing (first slice)

### S02 → S03, S04

Produces:
- `ICloudSaveService` — `UniTask PushAsync(MetaSaveData)`, `UniTask<MetaSaveData> PullAsync()`
- `PlayFabCloudSaveService` — uses `UpdateUserData` / `GetUserData`; applies take-max merge on pull
- `MetaSaveData.savedAt` — added long timestamp field
- `GameBootstrapper` integration: pull called after login, merged result written back via `IMetaSaveService`
- Session-end push hook (called on level complete, level failed, app pause)

Consumes from S01:
- `IPlayFabAuthService.IsLoggedIn` — guard before any cloud save call
- UniTask adapter pattern

### S03 → S04

Produces:
- `IPlatformLinkService` — `UniTask<bool> LinkGameCenterAsync()`, `UniTask<bool> LinkGooglePlayAsync()`, `UniTask UnlinkGameCenterAsync()`, `UniTask UnlinkGooglePlayAsync()`, `bool IsGameCenterLinked`, `bool IsGooglePlayLinked`
- `PopupId.PlatformLink` — new popup entry
- `IPlatformLinkView` / `PlatformLinkPresenter` — first-launch popup
- `ISettingsView` extended with link status and link/unlink button events
- `SettingsPresenter` extended with link/unlink handling

Consumes from S01:
- `IPlayFabAuthService` — `LinkGameCenterAccount` / `LinkGooglePlayGamesServicesAccount` called on the authenticated session

### S04 → (nothing downstream)

Produces:
- `IAnalyticsService` — `void TrackSessionStart()`, `void TrackSessionEnd()`, `void TrackLevelStarted(string levelId)`, `void TrackLevelCompleted(string levelId)`, `void TrackLevelFailed(string levelId)`, `void TrackCurrencyEarned(string currency, int amount)`, `void TrackCurrencySpent(string currency, int amount)`, `void TrackPlatformLinked(string platform)`
- `PlayFabAnalyticsService` — uses `WritePlayerEvent`; no-ops if not logged in
- Hooks in `CoinsService`, `GoldenPieceService`, `InGameSceneController`, `GameBootstrapper`

Consumes from S01:
- `IPlayFabAuthService.IsLoggedIn` — guard before sending events
