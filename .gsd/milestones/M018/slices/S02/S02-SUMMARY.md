---
id: S02
parent: M018
milestone: M018
provides:
  - IATTService interface (RequestAuthorizationAsync, GetCurrentStatus)
  - ATTAuthorizationStatus enum (NotDetermined, Restricted, Denied, Authorized)
  - NullATTService (Editor/Android no-op)
  - UnityATTService (iOS P/Invoke bridge, #if UNITY_IOS guarded)
  - Assets/Plugins/iOS/ATTBridge.mm (native Objective-C ATT bridge)
  - GameBootstrapper ATT call after consent gate, before LevelPlay init
  - PostBuildATT.cs (injects NSUserTrackingUsageDescription into Xcode Info.plist)
requires:
  - slice: S01
    provides: ConsentGatePresenter.WaitForAccept() — ATT fires only after this resolves
affects: []
key_files:
  - Assets/Scripts/Game/Services/IATTService.cs
  - Assets/Scripts/Game/Services/ATTAuthorizationStatus.cs
  - Assets/Scripts/Game/Services/NullATTService.cs
  - Assets/Scripts/Game/Services/UnityATTService.cs
  - Assets/Plugins/iOS/ATTBridge.mm
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Editor/PostBuildATT.cs
key_decisions:
  - D095: Direct P/Invoke bridge (ATTBridge.mm) instead of com.unity.ads.ios-support package
  - D096: IATTService abstraction — NullATTService for Editor/Android, UnityATTService for iOS
  - D097: LevelPlay init deferred past ATT result so IDFA is available if authorized
patterns_established:
  - IService + NullService + UnityService triad — consistent with IAdService, IAnalyticsService
  - Native P/Invoke bridge via Assets/Plugins/iOS/*.mm — no package dependency required for simple ATT calls
  - PostBuildATT uses IPostprocessBuildWithReport (Unity's callbackOrder-based build post-process API)
observability_surfaces:
  - "[GameBootstrapper] ATT status: {status}" — logged after RequestAuthorizationAsync resolves
  - "[UnityATTService] ATT dialog shown — polling for user response." — confirms native call was made
  - "[UnityATTService] ATT result received: {status}" — confirms polling completed
  - "[UnityATTService] ATT poll timed out after 30s" — warns on timeout edge case
drill_down_paths:
  - .gsd/milestones/M018/slices/S02/tasks/T01-PLAN.md
  - .gsd/milestones/M018/slices/S02/tasks/T02-PLAN.md
duration: ~2 hours (including significant delay due to Unity package import issue)
verification_result: passed
completed_at: 2026-03-20
---

# S02: ATT Integration

**iOS ATT dialog fires after consent accepted via a direct P/Invoke native bridge — no external package dependency.**

## What Happened

T01 wrote the four-file ATT abstraction layer: `ATTAuthorizationStatus` enum, `IATTService` interface, `NullATTService` (Editor/Android no-op), and `UnityATTService`. The original plan called for `com.unity.ads.ios-support` to supply `ATTrackingStatusBinding`; however, that package caused Unity's MCP server to stop responding after import (its background SKAdNetwork network fetcher blocked Unity's editor reconnection). The package was replaced with a direct P/Invoke bridge: `Assets/Plugins/iOS/ATTBridge.mm` provides `_RequestATTAuthorization()` and `_GetATTAuthorizationStatus()` as extern C functions callable from C# via `[DllImport("__Internal")]`. The result is simpler — no package dependency, same ATT behaviour, cleaner build graph.

T02 wired ATT into `GameBootstrapper.Start()` between the consent gate and PlayFab login. `UnityATTService` is constructed with `#if UNITY_IOS / #else NullATTService` so both paths compile and run cleanly. `PostBuildATT.cs` uses `IPostprocessBuildWithReport` to inject `NSUserTrackingUsageDescription` into the Xcode Info.plist during iOS builds. Both wiring and post-build paths are guarded against non-iOS platforms.

Boot sequence order confirmed as: PopupManager → consent gate → ATT → PlayFab login → cloud save → services → LevelPlay init.

## Verification

- 336/336 edit-mode tests pass (job `69411bb2fe784558b530de4692702096`)
- No compile errors — only pre-existing warnings (CS0618 FindObjectOfType, CS0414 unused fields)
- `ATTBridge.mm` structure verified: `_RequestATTAuthorization` calls `requestTrackingAuthorizationWithCompletionHandler`, `_GetATTAuthorizationStatus` returns cast of `trackingAuthorizationStatus`; iOS < 14 fallback returns 3 (Authorized)
- `PostBuildATT.cs`: uses `PlistDocument` from `UnityEditor.iOS.Xcode` (built into Unity, no package) — injects `NSUserTrackingUsageDescription` with string "We use your device advertising ID to show you relevant ads and measure ad performance."

## Requirements Advanced

- R160 — ATT request fires after consent accepted, before any SDK IDFA access
- R161 — ATT result does not block game progression
- R162 — LevelPlay ads SDK initializes after ATT result is known
- R163 — NSUserTrackingUsageDescription injected into Xcode Info.plist via post-build script

## Requirements Validated

- R160 — `await _attService.RequestAuthorizationAsync()` is the sole ATT call; it resolves before `unityAdService.Initialize()`; verified by boot sequence ordering in GameBootstrapper
- R162 — `unityAdService.Initialize(...)` appears after the ATT await in GameBootstrapper.Start()

## New Requirements Surfaced

- none

## Requirements Invalidated or Re-scoped

- none

## Deviations

**`com.unity.ads.ios-support` package removed** — planned in D095 as the ATT source. Replaced with direct P/Invoke bridge (`ATTBridge.mm`) after the package's background SKAdNetwork network fetcher blocked Unity MCP reconnection after every domain reload triggered by package import. Decision updated in DECISIONS.md (D095 revised). The bridge provides identical ATT behaviour with no external dependency.

## Known Limitations

- ATT on iOS < 14: `ATTBridge.mm` returns hardcoded `3` (Authorized) — correct for pre-ATT iOS where tracking was unrestricted, but not verified on a real < 14 device (iOS 14 is effectively minimum supported now).
- `PostBuildATT.cs` callbackOrder is 0 — if other post-build scripts also modify Info.plist at callbackOrder 0, there may be ordering ambiguity. No other plist-modifying scripts currently exist.
- ATT dialog usage description is hardcoded in `PostBuildATT.cs` — not configurable without editing the script.
- Full E2E ATT flow (consent → native dialog) requires an iOS device build; not verified in this slice.

## Follow-ups

- On-device E2E test: consent popup → Accept → ATT native dialog appears → game proceeds (UAT item in M018 milestone definition of done)
- Confirm `NSUserTrackingUsageDescription` present in Xcode Info.plist after first iOS build
- Confirm LevelPlay log order: ATT resolves before LevelPlay `onInitSuccess` fires

## Files Created/Modified

- `Assets/Scripts/Game/Services/ATTAuthorizationStatus.cs` — new: enum (NotDetermined, Restricted, Denied, Authorized)
- `Assets/Scripts/Game/Services/IATTService.cs` — new: interface (RequestAuthorizationAsync, GetCurrentStatus)
- `Assets/Scripts/Game/Services/NullATTService.cs` — new: Editor/Android no-op
- `Assets/Scripts/Game/Services/UnityATTService.cs` — new: iOS P/Invoke bridge, polls every 100ms, 30s timeout
- `Assets/Plugins/iOS/ATTBridge.mm` — new: native Objective-C bridge calling ATTrackingManager
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — modified: _attService field; ATT construction + await between consent gate and PlayFab; LevelPlay init comment updated
- `Assets/Editor/PostBuildATT.cs` — new: IPostprocessBuildWithReport; injects NSUserTrackingUsageDescription
- `Packages/manifest.json` — modified: com.unity.ads.ios-support removed (was temporarily added at 1.0.0, removed in favour of direct bridge)
- `Packages/packages-lock.json` — modified: corresponding lock entry removed

## Forward Intelligence

### What the next slice should know
- M018 is now complete — no further slices planned
- The ATT bridge is compile-safe on all platforms: `#if UNITY_IOS` guards wrap every `[DllImport("__Internal")]` call in `UnityATTService`, and the `#else NullATTService` path is used at runtime on Editor/Android
- `PostBuildATT.cs` only runs on `BuildTarget.iOS` — the `#if UNITY_IOS` guard at build time is inside the post-process script itself

### What's fragile
- `ATTBridge.mm` — needs to be included in the Xcode build; Unity automatically includes `.mm` files from `Assets/Plugins/iOS/` but this has not been verified in an actual iOS build yet
- Consent gate → ATT ordering in `GameBootstrapper` — the `await _popupManager.ShowPopupAsync(PopupId.ConsentGate)` path assumes the `ConsentGatePopup` prefab slot is wired in the Boot scene; if missing, the fallback `MarkAccepted()` fires and ATT still runs

### Authoritative diagnostics
- Unity Editor.log at `C:\Users\Daniel\AppData\Local\Unity\Editor\Editor.log` — most reliable source of compile errors and domain reload timing
- `[GameBootstrapper] ATT status: {status}` in Unity console — confirms ATT resolved before PlayFab login log appears
- Test job via `mcporter call unityMCP.get_test_job --args '{"job_id":"..."}' ` — `status: succeeded`, `completed == total`, `failures_so_far: []`

### What assumptions changed
- `com.unity.ads.ios-support` was assumed to be a clean dependency — in practice its SKAdNetwork background fetcher caused Unity MCP to become unresponsive after every domain reload triggered by the package import. Removed in favour of a four-function P/Invoke bridge that is self-contained and has no background network activity.
