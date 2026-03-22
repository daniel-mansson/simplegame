# S01: SDK + Anonymous Login

**Goal:** PlayFab SDK installed and compiling; anonymous login runs at boot; Player ID logged and persisted.

**Demo:** Unity Play mode boots, logs `[PlayFabAuth] Logged in. PlayFabId: XXXXXXXX`, same ID on second launch.

## Must-Haves
- `Assets/PlayFabSDK/` contains the full SDK source (Client, Shared, Events, Authentication folders minimum)
- `PlayFab.asmdef` present and referenced by `SimpleGame.Game.asmdef`
- `IPlayFabAuthService` interface exists with `LoginAsync()`, `PlayFabId`, `IsLoggedIn`
- `PlayFabAuthService` implementation: calls `LoginWithCustomID` with `SystemInfo.deviceUniqueIdentifier`, `CreateAccount: true`
- Player ID stored in `PlayerPrefs` under key `PlayFab_PlayerId`; same ID returned on second launch
- `GameBootstrapper.Start()` awaits `_authService.LoginAsync()` before navigation loop
- `[PlayFabAuth]` log line visible in Play mode console with a real PlayFab ID (requires configured Title ID)
- All existing edit-mode tests continue to pass

## Tasks

- [x] **T01: Install PlayFab SDK via git sparse checkout**
  Fetch `ExampleTestProject/Assets/PlayFabSDK/` from PlayFab/UnitySDK GitHub into `Assets/PlayFabSDK/`. Add `PlayFab` asmdef reference to `SimpleGame.Game.asmdef`. Verify project compiles.

- [x] **T02: IPlayFabAuthService + PlayFabAuthService + GameBootstrapper integration**
  Define `IPlayFabAuthService`. Implement `PlayFabAuthService` with UniTask adapter for `LoginWithCustomID`. Wire into `GameBootstrapper.Start()`. Verify Play mode logs a PlayFab ID.

- [x] **T03: Edit-mode tests for auth service**
  Mock `IPlayFabAuthService`. Test: login sets `IsLoggedIn`, `PlayFabId` persisted to PlayerPrefs, second login call returns same ID, graceful handling of login failure.

## Files Likely Touched
- `Assets/PlayFabSDK/` — new directory (SDK source)
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — add PlayFab asmdef reference
- `Assets/Scripts/Game/Services/IPlayFabAuthService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabAuthService.cs` — new
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — await login before loop
- `Assets/Scripts/Game/Boot/UIFactory.cs` — optionally receive auth service
- `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` — add PlayFab reference for mocks
