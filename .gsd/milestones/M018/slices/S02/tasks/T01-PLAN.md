# T01: IATTService, Implementations & Package

**Slice:** S02
**Milestone:** M018

## Goal
Define the ATT service abstraction and both implementations. Add `com.unity.ads.ios-support` package so `ATTrackingStatusBinding` is available for the iOS impl.

## Must-Haves

### Truths
- `NullATTService.RequestAuthorizationAsync()` returns `ATTAuthorizationStatus.NotDetermined` synchronously
- `NullATTService.GetCurrentStatus()` returns `ATTAuthorizationStatus.NotDetermined`
- `UnityATTService` code compiles cleanly — `#if UNITY_IOS` guards wrap all `ATTrackingStatusBinding` calls
- `com.unity.ads.ios-support` is listed in `Packages/manifest.json`
- Project compiles with no errors after package addition

### Artifacts
- `Assets/Scripts/Game/Services/ATTAuthorizationStatus.cs` — enum: NotDetermined, Authorized, Denied, Restricted
- `Assets/Scripts/Game/Services/IATTService.cs` — interface with `RequestAuthorizationAsync()` and `GetCurrentStatus()`
- `Assets/Scripts/Game/Services/NullATTService.cs` — no-op impl for Editor/Android
- `Assets/Scripts/Game/Services/UnityATTService.cs` — iOS impl with `#if UNITY_IOS` guards
- `Packages/manifest.json` — `com.unity.ads.ios-support` entry added

### Key Links
- `UnityATTService` → `ATTrackingStatusBinding` (from `com.unity.ads.ios-support`, `#if UNITY_IOS` only)
- `NullATTService` implements `IATTService` with no SDK dependency

## Steps
1. Write `ATTAuthorizationStatus` enum
2. Write `IATTService` interface
3. Write `NullATTService` — both methods return `NotDetermined`, `RequestAuthorizationAsync` returns immediately
4. Write `UnityATTService` — check `GetAuthorizationTrackingStatus()` first; if `NOT_DETERMINED`, call `RequestAuthorizationTracking()` and await via `UniTaskCompletionSource`; map status to `ATTAuthorizationStatus`; all wrapped in `#if UNITY_IOS` with Editor/Android fallback to `NullATTService`-equivalent behaviour
5. Add `com.unity.ads.ios-support` to `Packages/manifest.json` — use the package name `com.unity.advertisement.ios.support` (verify exact name)
6. Verify compile

## Context
- `com.unity.ads.ios-support` package ID: verify via Package Manager or Unity registry. The package exposes `Unity.Advertisement.IosSupport.ATTrackingStatusBinding` under `#if UNITY_IOS`.
- `ATTrackingStatusBinding.RequestAuthorizationTracking()` is fire-and-forget — need to poll or use a callback. The package doesn't have a direct async callback; use a coroutine or polling approach via `UniTask.Delay` to wait for status change from `NOT_DETERMINED`.
- Alternative: use a completion source with a timer/poll pattern — check status every 100ms until it changes from NOT_DETERMINED (max ~30s timeout).
- `SimpleGame.Game.asmdef` may need `Unity.Advertisement.IosSupport` added to references (under `#if UNITY_IOS` define constraint).
