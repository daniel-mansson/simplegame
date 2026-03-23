# M018: Consent & ATT

**Gathered:** 2026-03-20
**Status:** Ready for planning

## Project Description

Unity mobile jigsaw puzzle game. M017 shipped rewarded and interstitial ads via Unity LevelPlay. This milestone adds the mandatory first-launch consent flow required for App Store distribution.

## Why This Milestone

Apple requires ATT authorization before accessing IDFA (enforced since iOS 14.5). The App Store also requires explicit ToS acceptance before gameplay. Neither was in place after M017 — the ads SDK was initializing without ATT, meaning IDFA was never available even to willing users.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Launch the game and see a consent popup that cannot be dismissed — only accepted
- Tap the Terms of Service and Privacy Policy links and be taken to https://simplemagicstudios.com/play in the device browser
- Tap Accept and immediately see the native iOS "Allow Tracking?" system dialog
- Proceed to the main menu regardless of whether they allow or deny tracking
- On subsequent launches, go straight to the main menu (consent flag already set)

### Entry point / environment

- Entry point: Game launch (Boot scene, `GameBootstrapper.Start()`)
- Environment: iOS device (ATT dialog) or Unity Editor / Android (consent popup only, ATT compiled out)
- Live dependencies involved: PlayerPrefs (consent flag), iOS ATT framework, Unity Ads (LevelPlay)

## Completion Class

- Contract complete means: Consent popup prefab wired in Boot scene; PlayerPrefs flag written on accept; ATT call fires on iOS before LevelPlay init
- Integration complete means: Full boot sequence works on device — consent → ATT → ads init → main menu
- Operational complete means: n/a

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Fresh install on iOS device: consent popup blocks main menu, Accept fires ATT dialog, game proceeds after response
- Second launch on same device: consent popup does not appear, goes straight to main menu
- NSUserTrackingUsageDescription is present in the Xcode build Info.plist

## Risks and Unknowns

- `com.unity.ads.ios-support` package version compatibility with current Unity version — verify it installs cleanly
- Boot sequence reordering: LevelPlay init currently fires unconditionally in `GameBootstrapper.Start()` before this milestone's gate; must be deferred past ATT result
- ATT dialog can only be tested on a real iOS device — Editor simulation via the package's editor stub is the only in-Editor test path

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — boot sequence; LevelPlay init and first-launch PlatformLink popup live here; new consent gate inserts before PlayFab login
- `Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs` — established pattern for a first-launch-once popup with PlayerPrefs guard; consent popup follows the same shape
- `Assets/Scripts/Game/PopupId.cs` — add `ConsentGate` entry
- `Assets/Scripts/Game/Services/IAdService.cs` / `UnityAdService.cs` — ads init must move to after ATT result
- `Packages/manifest.json` — `com.unity.ads.ios-support` goes here
- `Assets/Editor/` — post-build script for NSUserTrackingUsageDescription injection

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R158 — First-launch consent gate (M018/S01)
- R159 — ToS and Privacy Policy links (M018/S01)
- R160 — iOS ATT native dialog (M018/S02)
- R161 — ATT precedes ad SDK initialization (M018/S02)
- R162 — NSUserTrackingUsageDescription in built plist (M018/S02)

## Scope

### In Scope

- Consent popup UI (no close button — Accept only)
- ToS + Privacy Policy links opening https://simplemagicstudios.com/play
- PlayerPrefs flag: show every launch until accepted
- iOS ATT native dialog via `com.unity.ads.ios-support` (`ATTrackingStatusBinding`)
- `IATTService` interface + `UnityATTService` (iOS) + `NullATTService` (Editor/Android)
- Deferred LevelPlay init — fires after ATT result, not at boot
- Post-build script injecting `NSUserTrackingUsageDescription` into Xcode Info.plist

### Out of Scope / Non-Goals

- GDPR/CCPA toggles
- Android tracking consent
- Any UI beyond the consent popup (no settings toggle, no re-prompt mechanism)

## Technical Constraints

- ATT code must be wrapped in `#if UNITY_IOS` guards — must compile cleanly on Android and in Editor
- `com.unity.ads.ios-support` package must be added via git URL or Package Manager; verify it resolves
- Post-build script must live in `Assets/Editor/` to be editor-only (no asmdef needed)
- Consent popup follows the existing MVP pattern: `IConsentGateView` interface + `ConsentGatePresenter` + view MonoBehaviour — consistent with `IPlatformLinkView` / `PlatformLinkPresenter`
- No close button on the popup — the view must not expose `OnCloseClicked` or any dismiss path

## Integration Points

- `GameBootstrapper.Start()` — consent gate inserts at the very top, before PlayFab login; LevelPlay init moves to after ATT result
- `PopupId` enum — add `ConsentGate`
- `UIFactory` — add `CreateConsentGatePresenter` factory method
- Boot scene — ConsentGate popup prefab must be pre-instantiated (same as PlatformLink pattern)

## Open Questions

- None — all decisions confirmed by user.
