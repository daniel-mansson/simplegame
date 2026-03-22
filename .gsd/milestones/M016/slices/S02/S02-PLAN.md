# S02: Cloud Save Sync

**Goal:** `MetaSaveData` is pushed to PlayFab User Data after sessions and pulled at boot with take-max-per-field merge.

**Demo:** Simulate reinstall (delete PlayerPrefs, fresh Play mode) — progress restored from PlayFab cloud.

## Must-Haves
- `MetaSaveData` gains a `savedAt` long timestamp field
- `ICloudSaveService` interface: `UniTask PushAsync(MetaSaveData)`, `UniTask<MetaSaveData> PullAsync()`
- `PlayFabCloudSaveService` uses `UpdateUserData` / `GetUserData`; guards on `IPlayFabAuthService.IsLoggedIn`
- Take-max merge: `coins = max(local, cloud)`, `goldenPieces = max(local, cloud)`, per-object `currentSteps = max(local, cloud)`
- `GameBootstrapper.Start()` calls `PullAsync()` after login, merges result, writes back via `IMetaSaveService`
- Push called from `GameBootstrapper.OnApplicationPause(true)` and after level complete/failed outcomes
- Edit-mode tests for merge logic (pure C#, no Unity or PlayFab deps)
- All existing edit-mode tests continue to pass

## Tasks

- [ ] **T01: MetaSaveData timestamp + ICloudSaveService + PlayFabCloudSaveService**
  Add `savedAt` to `MetaSaveData`. Define `ICloudSaveService`. Implement `PlayFabCloudSaveService` using UpdateUserData/GetUserData with UniTask bridge.

- [ ] **T02: Take-max merge logic + GameBootstrapper cloud sync wiring**
  Implement `MetaSaveMerge.TakeMax(local, cloud)`. Wire pull-at-boot and push-on-pause into `GameBootstrapper`. Wire push after level outcomes in `InGameSceneController`.

- [ ] **T03: Edit-mode tests for merge logic and cloud save service**
  Test `TakeMax` for all field combinations. Test `PlayFabCloudSaveService` mock path: push when logged in, skip when not, pull returns merged data.

## Files Likely Touched
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — add `savedAt` field
- `Assets/Scripts/Game/Services/ICloudSaveService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabCloudSaveService.cs` — new
- `Assets/Scripts/Game/Services/MetaSaveMerge.cs` — new (pure static merge logic)
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — pull at boot, push on pause
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — push after outcomes
- `Assets/Tests/EditMode/Game/CloudSaveServiceTests.cs` — new
