---
id: T02
parent: S01
milestone: M016
provides:
  - IPlayFabAuthService interface (LoginAsync, IsLoggedIn, PlayFabId)
  - PlayFabAuthService implementation (LoginWithCustomID, device ID, CreateAccount:true)
  - PlayFabLoginException typed exception for failure handling
  - GameBootstrapper.Start() awaits LoginAsync() before building services
  - Player ID persisted to PlayerPrefs under "PlayFab_PlayerId"
  - Offline-safe: login failure is non-fatal, game continues without cloud features
requires: []
affects: [T03, S02, S03, S04]
key_files:
  - Assets/Scripts/Game/Services/IPlayFabAuthService.cs
  - Assets/Scripts/Game/Services/PlayFabAuthService.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
key_decisions:
  - "Login failure is non-fatal — game continues offline with a warning log"
  - "Entity token is session-scoped; only PlayFabId stored in PlayerPrefs for diagnostics"
patterns_established:
  - "PlayFab callbacks bridged to UniTask via UniTaskCompletionSource — pattern used by S02/S03/S04"
  - "All PlayFab-dependent services check IPlayFabAuthService.IsLoggedIn before making calls"
drill_down_paths:
  - .gsd/milestones/M016/slices/S01/tasks/T01-PLAN.md
duration: 15min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# T02: IPlayFabAuthService + PlayFabAuthService + GameBootstrapper integration

**Anonymous PlayFab login wired into boot sequence via IPlayFabAuthService; UniTask adapter pattern established for all downstream PlayFab calls.**

## What Happened

Defined `IPlayFabAuthService` with `LoginAsync()`, `IsLoggedIn`, `PlayFabId`. Implemented `PlayFabAuthService` using `LoginWithCustomID` with `SystemInfo.deviceUniqueIdentifier` as the stable device ID. Bridged the PlayFab callback to UniTask via `UniTaskCompletionSource` — the pattern all S02/S03/S04 services will follow.

`GameBootstrapper.Start()` now awaits `_authService.LoginAsync()` before building any game services. Login failure is caught and logged as a warning; the game continues offline without cloud features (IsLoggedIn=false acts as the guard for all downstream cloud operations).

Runtime verification requires Unity Editor open with Title ID configured in `PlayFabSharedSettings`.

## Deviations

None from plan.

## Files Created/Modified
- `Assets/Scripts/Game/Services/IPlayFabAuthService.cs` — new interface
- `Assets/Scripts/Game/Services/PlayFabAuthService.cs` — new implementation
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — login step added before service construction
