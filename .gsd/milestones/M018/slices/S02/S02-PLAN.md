# S02: ATT Integration

**Goal:** iOS App Tracking Transparency native dialog fires after consent accepted. LevelPlay ads init deferred until after ATT result. NSUserTrackingUsageDescription injected into Xcode plist via post-build script.

**Demo:** On iOS device — Accept consent → native "Allow Tracking?" dialog appears → game proceeds regardless of choice. Post-build: Xcode Info.plist contains NSUserTrackingUsageDescription.

## Must-Haves
- `IATTService` interface — `UniTask<ATTAuthorizationStatus> RequestAuthorizationAsync()`, `ATTAuthorizationStatus GetCurrentStatus()`
- `ATTAuthorizationStatus` enum — NotDetermined, Authorized, Denied, Restricted
- `UnityATTService` — iOS impl wrapping `ATTrackingStatusBinding` from `com.unity.ads.ios-support`, compiled with `#if UNITY_IOS`
- `NullATTService` — returns `NotDetermined`, no-op for Editor/Android
- `com.unity.ads.ios-support` added to `Packages/manifest.json`
- `GameBootstrapper` calls ATT after consent accepted, before LevelPlay init
- LevelPlay init (`unityAdService.Initialize(...)`) moved to after `await attService.RequestAuthorizationAsync()`
- `Assets/Editor/PostBuildATT.cs` — post-build script injecting `NSUserTrackingUsageDescription` into Xcode Info.plist
- All existing tests still pass

## Tasks

- [x] **T01: IATTService, implementations & package**
  `ATTAuthorizationStatus` enum, `IATTService` interface, `NullATTService`, `UnityATTService` (#if UNITY_IOS), `com.unity.ads.ios-support` in manifest.

- [x] **T02: Boot wiring & post-build script**
  `GameBootstrapper` ATT call after consent and before LevelPlay init. `PostBuildATT.cs` editor script injecting `NSUserTrackingUsageDescription`.

## Files Likely Touched
- `Assets/Scripts/Game/Services/IATTService.cs` (new)
- `Assets/Scripts/Game/Services/ATTAuthorizationStatus.cs` (new)
- `Assets/Scripts/Game/Services/NullATTService.cs` (new)
- `Assets/Scripts/Game/Services/UnityATTService.cs` (new)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs`
- `Assets/Editor/PostBuildATT.cs` (new)
- `Packages/manifest.json`
