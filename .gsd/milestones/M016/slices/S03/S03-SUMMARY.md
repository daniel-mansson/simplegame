---
id: S03
milestone: M016
provides:
  - PopupId.PlatformLink entry
  - IPlatformLinkService interface (Link/Unlink/Refresh for Game Center and Google Play)
  - PlayFabPlatformLinkService — platform-conditional (#if UNITY_IOS / #if UNITY_ANDROID)
  - IPlatformLinkView / PlatformLinkPresenter — first-launch popup with skip-forever logic
  - ISettingsView extended with 4 link/unlink events and UpdateLinkStatus
  - SettingsView updated with optional platform link button slots
  - SettingsPresenter updated to handle link/unlink actions
  - SettingsSceneController.Initialize accepts optional IPlatformLinkService
  - UIFactory.CreatePlatformLinkPresenter and CreateSettingsPresenter(linkService)
  - GameBootstrapper: RefreshLinkStatus after login, first-launch prompt before navigation loop
  - MockPlatformLinkService — reusable test double for S04 tests
  - 8 edit-mode tests: link contract, ShouldShow logic, MarkSeen persistence
requires:
  - slice: S01
    provides: IPlayFabAuthService, PlayFabAuthService, UniTask bridge pattern
affects: [S04]
key_files:
  - Assets/Scripts/Game/PopupId.cs
  - Assets/Scripts/Game/Services/IPlatformLinkService.cs
  - Assets/Scripts/Game/Services/PlayFabPlatformLinkService.cs
  - Assets/Scripts/Game/Popup/IPlatformLinkView.cs
  - Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs
  - Assets/Scripts/Game/Settings/ISettingsView.cs
  - Assets/Scripts/Game/Settings/SettingsView.cs
  - Assets/Scripts/Game/Settings/SettingsPresenter.cs
  - Assets/Scripts/Game/Settings/SettingsSceneController.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Tests/EditMode/Game/PlatformLinkTests.cs
key_decisions:
  - "Google Play Games linking uses #if UNITY_ANDROID — no Google Play Games plugin installed; graceful compile-time stub on non-Android"
  - "Game Center uses Social.localUser.id — built-in iOS, no extra SDK"
  - "First-launch prompt skips silently if IPlatformLinkView not found in Boot scene — view must be pre-instantiated"
  - "SettingsView platform link buttons are optional SerializeFields — missing wiring does not break existing Settings functionality"
  - "MockSettingsView in DemoWiringTests updated with all new ISettingsView members"
patterns_established:
  - "MockPlatformLinkService public in test assembly — reusable by S04 tests"
  - "Platform-conditional code via #if UNITY_IOS / #if UNITY_ANDROID — no runtime platform checks needed"
drill_down_paths:
  - .gsd/milestones/M016/slices/S03/S03-PLAN.md
duration: 60min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# S03: Platform Linking & First-Launch Prompt

**Platform account linking (Game Center / Google Play Games) wired into Settings and first-launch popup; ISettingsView extended with link/unlink actions.**

## What Was Built

`IPlatformLinkService` defines link/unlink/refresh for Game Center and Google Play Games. `PlayFabPlatformLinkService` uses `#if UNITY_IOS` / `#if UNITY_ANDROID` guards — on iOS it reads `Social.localUser.id` and calls `LinkGameCenterAccount`; on Android it reads a Google Play Games server auth code and calls `LinkGooglePlayGamesServicesAccount`. On desktop/Editor both paths compile but return false gracefully.

`ISettingsView` gained 4 new events (Link/Unlink for each platform) and `UpdateLinkStatus`. `SettingsView` (MonoBehaviour) implements them with optional SerializeField button slots. `SettingsPresenter` handles the events and delegates to `IPlatformLinkService`. `SettingsSceneController` accepts `IPlatformLinkService` as an optional parameter and calls `RefreshLinkStatusAsync` before showing the screen.

`PlatformLinkPresenter` manages the first-launch popup: `ShouldShow()` returns true if the player has not yet linked any platform and has not previously skipped. `MarkSeen()` writes a PlayerPrefs flag. `GameBootstrapper` checks `ShouldShow()` after login and before the navigation loop.

`MockSettingsView` in `DemoWiringTests.cs` updated with all new interface members per K004.

## Deviations

Google Play Games plugin not installed — `#if UNITY_ANDROID` guard means the Google Play path compiles correctly but returns false in-editor and requires the plugin for real Android linking.

## Files Created/Modified
- `Assets/Scripts/Game/PopupId.cs` — added PlatformLink
- `Assets/Scripts/Game/Services/IPlatformLinkService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabPlatformLinkService.cs` — new
- `Assets/Scripts/Game/Popup/IPlatformLinkView.cs` — new
- `Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs` — new
- `Assets/Scripts/Game/Settings/ISettingsView.cs` — extended
- `Assets/Scripts/Game/Settings/SettingsView.cs` — extended
- `Assets/Scripts/Game/Settings/SettingsPresenter.cs` — extended
- `Assets/Scripts/Game/Settings/SettingsSceneController.cs` — extended
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — platform link init, first-launch prompt
- `Assets/Scripts/Game/Boot/UIFactory.cs` — new factory methods
- `Assets/Tests/EditMode/Game/DemoWiringTests.cs` — MockSettingsView updated
- `Assets/Tests/EditMode/Game/PlatformLinkTests.cs` — new (8 tests)
