# M018: Consent & ATT

**Vision:** First-launch ToS acceptance gate (no dismiss — Accept only) with iOS App Tracking Transparency native dialog firing immediately after acceptance, before any ad SDK initialization.

## Success Criteria

- Consent popup appears on every launch until the player taps Accept
- No close button, no dismiss path — Accept is the only exit
- Tapping ToS or Privacy Policy links opens https://simplemagicstudios.com/play in the device browser
- After Accept on iOS: native ATT system dialog fires
- ATT result (allow or deny) does not block game progression
- LevelPlay ads SDK initializes after ATT result is known
- `NSUserTrackingUsageDescription` present in the Xcode build Info.plist
- Second launch proceeds directly to main menu (flag set)

## Key Risks / Unknowns

- `com.unity.ads.ios-support` package install — version compatibility with current Unity not yet verified
- Boot sequence reordering — LevelPlay currently inits unconditionally; moving it past ATT result requires care not to break the existing ads flow

## Proof Strategy

- `com.unity.ads.ios-support` compatibility → retire in S02/T01 by successfully adding the package and compiling cleanly with `#if UNITY_IOS` guards
- Boot sequence reordering → retire in S02/T02 by verifying ads still load/show correctly after the reorder (existing NullAdService tests still pass; UnityAdService.Initialize call site moves)

## Verification Classes

- Contract verification: edit-mode tests for ConsentGatePresenter (ShouldShow flag, Accept sets flag, no dismiss path); ATT service interface verified with NullATTService
- Integration verification: full boot in Play mode with no compile errors; post-build script injects plist key (verified in Xcode build)
- Operational verification: none (no service lifecycle concerns)
- UAT / human verification: on-device test — ATT dialog fires after Accept on iOS 14.5+ device

## Milestone Definition of Done

This milestone is complete only when all are true:

- ConsentGate popup blocks boot until Accept on every fresh install simulation (clear PlayerPrefs)
- No dismiss path exists — view has no close/skip button or event
- ToS and Privacy Policy links open correct URL
- ATT dialog fires on iOS device immediately after Accept
- Ads SDK initializes after ATT result (verified by log order in device build)
- `NSUserTrackingUsageDescription` confirmed present in Xcode Info.plist after post-build script runs
- All edit-mode tests pass
- Final integrated acceptance: fresh install → consent popup → Accept → ATT dialog → main menu loads

## Requirement Coverage

- Covers: R158, R159, R160, R161, R162
- Partially covers: none
- Leaves for later: none
- Orphan risks: none

## Slices

- [x] **S01: Consent Gate Popup** `risk:medium` `depends:[]`
  > After this: launching the game shows a ToS/Privacy popup with no close button; Accept writes the PlayerPrefs flag and proceeds to main menu; popup never shows again on subsequent launches.

- [x] **S02: ATT Integration** `risk:medium` `depends:[S01]`
  > After this: on an iOS device, Accept triggers the native ATT system dialog; LevelPlay initializes after the ATT result; NSUserTrackingUsageDescription is in the Xcode build plist.

## Boundary Map

### S01 → S02

Produces:
- `IConsentGateView` interface — `event Action OnAcceptClicked`; `void SetAcceptInteractable(bool)` (no dismiss events)
- `ConsentGatePresenter` — `ShouldShow()` static method; `MarkAccepted()` static method; `WaitForAccept()` UniTask
- `PopupId.ConsentGate` enum value
- PlayerPrefs key `"ConsentGate_Accepted"` — written 1 on accept, read to gate future launches
- Boot wiring point in `GameBootstrapper` where consent is awaited before everything else

Consumes:
- nothing (first slice)

### S02 → (end)

Produces:
- `IATTService` interface — `UniTask<ATTAuthorizationStatus> RequestAuthorizationAsync()`; `ATTAuthorizationStatus GetCurrentStatus()`
- `UnityATTService` — iOS-only impl wrapping `ATTrackingStatusBinding` (compiled with `#if UNITY_IOS`)
- `NullATTService` — no-op returning `NotDetermined` (Editor / Android)
- `Assets/Editor/PostBuildATT.cs` — post-build script injecting `NSUserTrackingUsageDescription` into Xcode Info.plist
- Reordered boot sequence: consent gate → ATT request → LevelPlay init → PlayFab login → main menu

Consumes from S01:
- `ConsentGatePresenter.WaitForAccept()` — ATT fires only after this resolves
- Boot wiring point established by S01
