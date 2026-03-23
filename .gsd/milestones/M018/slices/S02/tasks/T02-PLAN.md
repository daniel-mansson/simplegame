# T02: Boot Wiring & Post-Build Script

**Slice:** S02
**Milestone:** M018

## Goal
Wire ATT into the boot sequence (after consent, before LevelPlay init) and add a post-build script that injects NSUserTrackingUsageDescription into the Xcode Info.plist.

## Must-Haves

### Truths
- `GameBootstrapper` instantiates `IATTService` (UnityATTService on iOS, NullATTService elsewhere)
- ATT `RequestAuthorizationAsync()` is called after consent gate resolves and before `unityAdService.Initialize(...)`
- LevelPlay init happens after ATT result — ads SDK receives IDFA if user authorized
- `PostBuildATT.cs` editor script runs as a `IPostprocessBuildWithReport` callback and injects `NSUserTrackingUsageDescription` into the Xcode Info.plist
- All existing 336 tests still pass

### Artifacts
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — `IATTService _attService` field; constructed early; called after consent, before LevelPlay init
- `Assets/Editor/PostBuildATT.cs` — implements `IPostprocessBuildWithReport`; reads Info.plist from built Xcode project; sets `NSUserTrackingUsageDescription` string

### Key Links
- `GameBootstrapper` → `IATTService.RequestAuthorizationAsync()` — between consent dismiss and `unityAdService.Initialize()`
- `PostBuildATT.cs` → Unity's `UnityEditor.iOS.Xcode.PlistDocument` API

## Steps
1. Add `IATTService _attService` field to `GameBootstrapper`
2. Construct `_attService` — `#if UNITY_IOS` → `new UnityATTService()`, else `new NullATTService()`
3. Move consent gate dismiss to before ATT call
4. After consent is accepted and before LevelPlay init: `await _attService.RequestAuthorizationAsync()`; log the result
5. Move `unityAdService.Initialize(...)` to after the ATT await
6. Write `Assets/Editor/PostBuildATT.cs` — `IPostprocessBuildWithReport.OnPostprocessBuild`; only runs on iOS target; reads `Info.plist`, injects `NSUserTrackingUsageDescription`
7. Verify compile, run tests

## Context
- `UnityEditor.iOS.Xcode` is in the `UnityEditor.iOS.Extensions` assembly — no additional package needed for the plist API; available in any project with iOS build support installed
- The plist injection must check if the key already exists to be idempotent
- NSUserTrackingUsageDescription text: "We use tracking to serve you personalized ads and measure app performance."
- The existing ads init block has a TODO about testMode — leave that comment in place, just move the Initialize() call below the ATT await
