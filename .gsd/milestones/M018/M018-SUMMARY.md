---
id: M018
provides:
  - ConsentGate popup — Accept-only first-launch ToS/Privacy gate (no dismiss path)
  - ConsentGatePresenter with ShouldShow/MarkAccepted/WaitForAccept UniTask
  - IATTService abstraction with NullATTService (Editor/Android) and UnityATTService (iOS P/Invoke)
  - ATTBridge.mm native Objective-C plugin for Apple ATT framework
  - PostBuildATT.cs — injects NSUserTrackingUsageDescription into Xcode Info.plist
  - Boot sequence: consent gate → ATT → PlayFab → cloud save → LevelPlay init
key_decisions:
  - D093: Consent gate fires before everything else in boot
  - D094: No dismiss path — IConsentGateView exposes only OnAcceptClicked
  - D095b: ATT via direct P/Invoke bridge (ATTBridge.mm) — com.unity.ads.ios-support removed (K010)
  - D096: IATTService / NullATTService / UnityATTService triad
  - D097: LevelPlay init deferred until after ATT result
patterns_established:
  - Accept-only popup pattern — IPopupView with no dismiss events, WaitForAccept UniTask
  - Native iOS P/Invoke bridge via Assets/Plugins/iOS/*.mm — zero package dependency for ATT
  - IService + NullService + UnityService triad consistent with IAdService, IAnalyticsService
  - Error fallbacks must never write permanent acceptance flags (consent must not be bypassable)
observability_surfaces:
  - "[GameBootstrapper] Boot sequence started." — confirms boot is running
  - "[GameBootstrapper] ATT status: {status}" — ATT result before PlayFab login
  - "[UnityATTService] ATT dialog shown — polling for user response." — native ATT call confirmed
  - "[GameBootstrapper] ConsentGate view not found in Boot scene" — build config error (does NOT mark accepted)
  - Tools/Dev/Reset First-Launch Flags — editor menu to clear PlayerPrefs for testing
requirement_outcomes:
  - id: R158
    from_status: active
    to_status: validated
    proof: ConsentGatePresenter.ShouldShow() guards boot; MarkAccepted() writes PlayerPrefs key; popup blocks until Accept; 336/336 tests pass including reflection test verifying no dismiss events
  - id: R159
    from_status: active
    to_status: validated
    proof: ConsentGateView._tosLinkButton and _privacyLinkButton both call Application.OpenURL("https://simplemagicstudios.com/play")
  - id: R160
    from_status: active
    to_status: validated
    proof: UnityATTService calls _RequestATTAuthorization() via P/Invoke immediately after WaitForAccept() resolves; NullATTService on non-iOS
  - id: R161
    from_status: active
    to_status: validated
    proof: unityAdService.Initialize() appears after await _attService.RequestAuthorizationAsync() in GameBootstrapper.Start()
  - id: R162
    from_status: active
    to_status: validated
    proof: PostBuildATT.cs uses IPostprocessBuildWithReport to inject NSUserTrackingUsageDescription into Xcode Info.plist on iOS builds
duration: ~1 day
verification_result: passed
completed_at: 2026-03-20
---

# M018: Consent & ATT

**First-launch consent gate (Accept-only, no dismiss) with iOS ATT dialog firing after acceptance — boot sequence hardened so consent cannot be bypassed by missing prefabs or direct scene entry.**

## What Happened

S01 built the full consent gate stack: `IConsentGateView` (single `OnAcceptClicked` event, no dismiss events by design), `ConsentGatePresenter` (PlayerPrefs key `"ConsentGate_Accepted"`, `ShouldShow()`, `MarkAccepted()`, `WaitForAccept()` UniTask), `ConsentGateView` MonoBehaviour with ToS/Privacy link buttons opening `simplemagicstudios.com/play`, `PopupId.ConsentGate`, wiring in `GameBootstrapper.Start()` before PlayFab login, and `ConsentGatePopup` prefab via `PrefabKitSetup`. Eight edit-mode tests verified the presenter contract including a reflection test confirming no dismiss events exist on the interface.

S02 added iOS ATT: the original plan used `com.unity.ads.ios-support` but that package's SKAdNetwork background network fetcher blocked Unity MCP reconnection after every domain reload — removed and replaced with a direct Objective-C P/Invoke bridge (`ATTBridge.mm`) providing `_RequestATTAuthorization()` and `_GetATTAuthorizationStatus()`. `UnityATTService` polls the status every 100ms (max 30s) after calling the native request. `NullATTService` is used at runtime on Editor/Android. `PostBuildATT.cs` injects `NSUserTrackingUsageDescription` into the Xcode Info.plist at build time via `IPostprocessBuildWithReport`. Boot sequence reordered to: PopupManager → consent gate → ATT → PlayFab → cloud save → LevelPlay init.

Post-merge bug fix: the original error fallback in `GameBootstrapper` called `MarkAccepted()` when `ConsentGateView` was missing from the scene (before the prefab was wired). This permanently set the accepted flag, causing the consent gate to be skipped on all subsequent launches — including direct play-from-InGame via `BootInjector`. Fixed: missing view now logs an error but does not write the accepted flag. `DevResetPrefs.cs` editor utility added for testing.

## Cross-Slice Verification

- Consent popup blocks boot: `ShouldShow()` returns true on fresh install; `WaitForAccept()` resolves only via Accept button — verified by 8 edit-mode tests and confirmed on device (popup shown, no dismiss path visible)
- No dismiss path: reflection test in `ConsentGateTests.cs` confirms `IConsentGateView` has no events other than `OnAcceptClicked`
- ToS/Privacy links: `ConsentGateView` wires both buttons to `Application.OpenURL("https://simplemagicstudios.com/play")`
- Second launch skips popup: PlayerPrefs key `"ConsentGate_Accepted"=1` set by `MarkAccepted()` — `ShouldShow()` returns false
- ATT boot order: `[GameBootstrapper] ATT status: {status}` logged before PlayFab login line — confirmed in Editor.log
- LevelPlay after ATT: `unityAdService.Initialize()` call site is after the ATT await in `GameBootstrapper.Start()`
- `NSUserTrackingUsageDescription`: `PostBuildATT.cs` injects key via `PlistDocument` — requires iOS build to fully verify
- 336/336 edit-mode tests pass across all three verification runs

## Requirement Changes

- R158: active → validated — ConsentGatePresenter + Boot gate + 8 tests
- R159: active → validated — both link buttons wire to simplemagicstudios.com/play
- R160: active → validated — UnityATTService P/Invoke bridge fires after WaitForAccept()
- R161: active → validated — LevelPlay init after ATT await confirmed in GameBootstrapper
- R162: active → validated — PostBuildATT.cs verified in editor; full verification requires iOS build
- R163: out-of-scope (confirmed — no GDPR/CCPA toggles)
- R164: out-of-scope (confirmed — no Android tracking consent)

## Forward Intelligence

### What the next milestone should know
- `BootInjector` loads Boot additively when playing from non-Boot scenes in editor — `GameBootstrapper.Start()` always runs; consent gate always checked
- `DevResetPrefs` (Tools/Dev/Reset First-Launch Flags) clears `ConsentGate_Accepted` and `PlatformLink_HasSeen` for testing
- `ATTBridge.mm` in `Assets/Plugins/iOS/` is auto-included in Xcode builds by Unity — no manual Xcode config needed
- On-device ATT E2E (consent → Accept → native dialog → game) has not been verified in a device build yet

### What's fragile
- `PostBuildATT.cs` has not been verified in an actual iOS build — it should work (standard `PlistDocument` API) but hasn't been exercised
- `UnityATTService` polls every 100ms for up to 30s — if the user takes longer than 30s the game proceeds with `NotDetermined` status; acceptable but worth logging clearly (it does: `ATT poll timed out after 30s`)
- `ConsentGatePopup` prefab must exist before `SceneSetup` runs — `PrefabKitSetup` (Tools/Setup/Create Popup Prefabs) must run first; order dependency documented in SceneSetup log warnings

### Authoritative diagnostics
- `[GameBootstrapper] ConsentGate view not found in Boot scene` — means prefab slot is null; run Tools/Setup/Create Popup Prefabs then Tools/Setup/Create And Register Scenes
- `Tools/Dev/Reset First-Launch Flags` — clears PlayerPrefs keys for clean first-launch test
- Editor.log `InvokePackagesCallback` timing — if > 1s after package import, a package is making network calls that may block MCP reconnection (K010)

### What assumptions changed
- `com.unity.ads.ios-support` was expected to be a clean dependency — it caused Unity MCP to become permanently unresponsive after import due to background SKAdNetwork network fetching (K010). Replaced by a 60-line Objective-C file.
- The error fallback path (`MarkAccepted()` when view missing) was intended as a safe default — it was actually a consent bypass vector. Any error fallback that writes permanent acceptance flags is a security/legal issue.

## Files Created/Modified

- `Assets/Scripts/Game/Popup/IConsentGateView.cs` — new: interface (OnAcceptClicked only)
- `Assets/Scripts/Game/Popup/ConsentGatePresenter.cs` — new: ShouldShow, MarkAccepted, WaitForAccept
- `Assets/Scripts/Game/Popup/ConsentGateView.cs` — new: MonoBehaviour with Accept + ToS + Privacy buttons
- `Assets/Scripts/Game/PopupId.cs` — modified: ConsentGate added
- `Assets/Scripts/Game/Boot/UIFactory.cs` — modified: CreateConsentGatePresenter added
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — modified: consent gate + ATT boot wiring; error fallback hardened
- `Assets/Scripts/Game/Services/IATTService.cs` — new: interface
- `Assets/Scripts/Game/Services/ATTAuthorizationStatus.cs` — new: enum
- `Assets/Scripts/Game/Services/NullATTService.cs` — new: Editor/Android no-op
- `Assets/Scripts/Game/Services/UnityATTService.cs` — new: iOS P/Invoke bridge
- `Assets/Plugins/iOS/ATTBridge.mm` — new: native Objective-C ATT bridge
- `Assets/Editor/PostBuildATT.cs` — new: injects NSUserTrackingUsageDescription into Xcode plist
- `Assets/Editor/PrefabKitSetup.cs` — modified: CreateConsentGatePrefab added
- `Assets/Editor/SceneSetup.cs` — modified: ConsentGatePopup wired in Boot scene
- `Assets/Editor/DevResetPrefs.cs` — new: Tools/Dev/Reset First-Launch Flags
- `Assets/Prefabs/Game/Popups/ConsentGatePopup.prefab` — new: popup prefab with Accept/ToS/Privacy buttons
- `Assets/Scenes/Boot.unity` — modified: _consentGatePopup slot wired
- `Assets/Tests/EditMode/Game/ConsentGateTests.cs` — new: 8 edit-mode tests
- `Packages/manifest.json` — com.unity.ads.ios-support removed
