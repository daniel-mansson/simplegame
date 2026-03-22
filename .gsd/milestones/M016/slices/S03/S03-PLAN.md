# S03: Platform Linking & First-Launch Prompt

**Goal:** Players can link Game Center (iOS) or Google Play Games (Android) from Settings and from a first-launch popup.

**Demo:** Settings screen shows link status (Linked/Not Linked) with Link/Unlink buttons. First-launch popup appears once and is permanently skippable.

## Must-Haves
- `IPlatformLinkService` interface: `LinkGameCenterAsync()`, `LinkGooglePlayAsync()`, `UnlinkGameCenterAsync()`, `UnlinkGooglePlayAsync()`, `IsGameCenterLinked`, `IsGooglePlayLinked`
- `PlayFabPlatformLinkService` — calls `LinkGameCenterAccount` / `LinkGooglePlayGamesServicesAccount` via PlayFab client API
- `PopupId.PlatformLink` added
- `IPlatformLinkView` / `PlatformLinkPresenter` — first-launch popup (shows once, skip persists to PlayerPrefs)
- `ISettingsView` extended with link status events and update methods
- `SettingsPresenter` extended to handle link/unlink actions
- `GameBootstrapper` shows first-launch popup after login if not yet seen
- Edit-mode tests for link service contract and first-launch skip logic
- All existing edit-mode tests continue to pass

## Tasks

- [ ] **T01: IPlatformLinkService + PlayFabPlatformLinkService + PopupId**
  Define interface and implementation. Add PopupId.PlatformLink. Wire UniTask bridge for LinkGameCenter and LinkGooglePlayGamesServices.

- [ ] **T02: IPlatformLinkView + PlatformLinkPresenter + first-launch prompt in GameBootstrapper**
  Define view interface and presenter for the link popup. Wire first-launch check (PlayerPrefs flag) in GameBootstrapper — show popup before main menu on first launch.

- [ ] **T03: Settings screen extension for link/unlink**
  Extend ISettingsView, SettingsView, SettingsPresenter to show link status and link/unlink buttons.

- [ ] **T04: Edit-mode tests**
  Mock IPlatformLinkService. Test: link when logged in succeeds, skip when not logged in, first-launch prompt shown once then never again.

## Files Likely Touched
- `Assets/Scripts/Game/PopupId.cs` — add PlatformLink
- `Assets/Scripts/Game/Services/IPlatformLinkService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabPlatformLinkService.cs` — new
- `Assets/Scripts/Game/Popup/IPlatformLinkView.cs` — new
- `Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs` — new
- `Assets/Scripts/Game/Settings/ISettingsView.cs` — extended
- `Assets/Scripts/Game/Settings/SettingsPresenter.cs` — extended
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — first-launch popup
- `Assets/Tests/EditMode/Game/PlatformLinkTests.cs` — new
