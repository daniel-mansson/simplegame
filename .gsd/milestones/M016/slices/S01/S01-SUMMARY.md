---
id: S01
milestone: M016
provides:
  - PlayFab SDK 2.230.260123 installed at Assets/PlayFabSDK/
  - IPlayFabAuthService interface and PlayFabAuthService implementation
  - Anonymous PlayFab login wired into GameBootstrapper boot sequence
  - UniTask/callback adapter pattern established for all downstream PlayFab calls
  - MockPlayFabAuthService reusable in S02/S03/S04 tests
  - PlayFabLoginException for typed failure handling
  - Offline-safe boot: login failure is non-fatal
requires: []
affects: [S02, S03, S04]
key_files:
  - Assets/PlayFabSDK/PlayFab.asmdef
  - Assets/PlayFabSDK/Client/PlayFabClientAPI.cs
  - Assets/PlayFabSDK/Shared/Public/Resources/PlayFabSharedSettings.asset
  - Assets/Scripts/Game/Services/IPlayFabAuthService.cs
  - Assets/Scripts/Game/Services/PlayFabAuthService.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Tests/EditMode/Game/PlayFabAuthServiceTests.cs
key_decisions:
  - "PlayFab SDK installed via git sparse-checkout from GitHub source (no unitypackage)"
  - "Login failure is non-fatal — game continues offline, IsLoggedIn=false guards cloud ops"
  - "UniTask bridge via UniTaskCompletionSource — established pattern for all PlayFab calls"
  - "Only PlayFabId stored in PlayerPrefs; entity token is session-scoped"
patterns_established:
  - "PlayFab callback → UniTask bridge: UniTaskCompletionSource, set result in success/error callbacks"
  - "Offline guard: check IPlayFabAuthService.IsLoggedIn before any cloud operation"
drill_down_paths:
  - .gsd/milestones/M016/slices/S01/tasks/T01-SUMMARY.md
  - .gsd/milestones/M016/slices/S01/tasks/T02-SUMMARY.md
  - .gsd/milestones/M016/slices/S01/tasks/T03-SUMMARY.md
duration: 35min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# S01: SDK + Anonymous Login

**PlayFab SDK installed and anonymous login wired into the boot sequence — Player ID logged on launch, same account recovered across sessions.**

## What Was Built

PlayFab Unity SDK 2.230.260123 fetched directly from GitHub via sparse-checkout into `Assets/PlayFabSDK/`. No manual `.unitypackage` import needed. Assembly references updated in both `SimpleGame.Game.asmdef` and `SimpleGame.Tests.Game.asmdef`.

`IPlayFabAuthService` defines the auth contract. `PlayFabAuthService` calls `LoginWithCustomID` with `SystemInfo.deviceUniqueIdentifier` and `CreateAccount: true` — first launch creates an account, subsequent launches recover it. The PlayFab Player ID is stored in PlayerPrefs under `PlayFab_PlayerId`.

`GameBootstrapper.Start()` now awaits `_authService.LoginAsync()` before constructing any game services or entering the navigation loop. Login failure is caught and logged as a warning; the game continues in offline mode with all cloud operations skipped via `IsLoggedIn` guard checks.

The UniTask bridge pattern (`UniTaskCompletionSource` wrapping PlayFab callbacks) is established here and will be used by S02 (cloud save), S03 (platform linking), and S04 (analytics).

`MockPlayFabAuthService` is a public, reusable test double with configurable success/failure, fake ID, and call counter. S02/S03/S04 tests reference it directly.

## Deviations

None from plan.

## Files Created/Modified
- `Assets/PlayFabSDK/` — SDK source (239 files, version 2.230.260123)
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — added PlayFab reference
- `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` — added PlayFab reference
- `Assets/Scripts/Game/Services/IPlayFabAuthService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabAuthService.cs` — new
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — login step added
- `Assets/Tests/EditMode/Game/PlayFabAuthServiceTests.cs` — new (5 tests)
