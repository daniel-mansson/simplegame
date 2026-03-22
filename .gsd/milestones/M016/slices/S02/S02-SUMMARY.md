---
id: S02
milestone: M016
provides:
  - MetaSaveData.savedAt timestamp field
  - ICloudSaveService interface (PushAsync, PullAsync)
  - PlayFabCloudSaveService — UpdateUserData/GetUserData with offline guard
  - MetaSaveMerge.TakeMax — pure static merge logic, no Unity/PlayFab deps
  - GameBootstrapper: cloud pull + take-max merge before navigation loop starts
  - GameBootstrapper: OnApplicationPause push to cloud
  - InGameSceneController: onSessionEnd push after level complete/quit
  - MockCloudSaveService — reusable test double for S03/S04
  - 13 edit-mode tests: TakeMax all field combinations, mock push/pull contract
requires:
  - slice: S01
    provides: IPlayFabAuthService, PlayFabAuthService, UniTask bridge pattern
affects: [S03, S04]
key_files:
  - Assets/Scripts/Game/Services/MetaSaveData.cs
  - Assets/Scripts/Game/Services/ICloudSaveService.cs
  - Assets/Scripts/Game/Services/MetaSaveMerge.cs
  - Assets/Scripts/Game/Services/PlayFabCloudSaveService.cs
  - Assets/Scripts/Game/Boot/GameBootstrapper.cs
  - Assets/Scripts/Game/InGame/InGameSceneController.cs
  - Assets/Tests/EditMode/Game/CloudSaveServiceTests.cs
key_decisions:
  - "Cloud pull happens before MetaProgressionService/CoinsService construction — merged data is the ground truth for the session"
  - "IMetaSaveService stays synchronous; cloud sync is explicit async at boot and session-end only"
  - "Push uses onSessionEnd callback injected into InGameSceneController — avoids coupling controller to cloud service directly"
  - "OnApplicationPause push is fire-and-forget — pause may be brief and we can't await in that context"
patterns_established:
  - "MockCloudSaveService public in test assembly — reusable by S03/S04 tests"
  - "TakeMax merge is pure static — testable without Unity or PlayFab"
drill_down_paths:
  - .gsd/milestones/M016/slices/S02/S02-PLAN.md
duration: 45min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# S02: Cloud Save Sync

**MetaSaveData synced to PlayFab cloud — pulled and merged at boot, pushed on session end and app pause.**

## What Was Built

Added `savedAt` timestamp to `MetaSaveData` (for audit — not used in conflict logic since take-max is field-level). `ICloudSaveService` defines `PushAsync`/`PullAsync`. `PlayFabCloudSaveService` serializes the full save blob as JSON under the key `"MetaSave"` in PlayFab User Data.

`MetaSaveMerge.TakeMax` is pure static logic: `coins = max(a,b)`, `goldenPieces = max(a,b)`, per-object `currentSteps = max(a,b)`. No Unity or PlayFab dependencies — fully testable in edit-mode.

Boot order in `GameBootstrapper`: (1) PlayFab login → (2) cloud pull → (3) TakeMax merge → (4) `IMetaSaveService.Save(merged)` → (5) construct `MetaProgressionService`/`CoinsService`/`GoldenPieceService` from merged local state. This ensures the merged data is the ground truth for the entire session.

Push happens in three places: `OnApplicationPause(true)` (fire-and-forget), `InGameSceneController` on level complete, and `InGameSceneController` on quit. The session-end push is delivered via an `onSessionEnd` callback to avoid coupling the controller to the cloud service.

13 edit-mode tests cover all TakeMax cases and the mock service contract.

## Deviations

None from plan.

## Files Created/Modified
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — added savedAt field
- `Assets/Scripts/Game/Services/ICloudSaveService.cs` — new
- `Assets/Scripts/Game/Services/MetaSaveMerge.cs` — new
- `Assets/Scripts/Game/Services/PlayFabCloudSaveService.cs` — new
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — pull at boot, push on pause, onSessionEnd wired
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — onSessionEnd callback added to Initialize
- `Assets/Tests/EditMode/Game/CloudSaveServiceTests.cs` — new (13 tests)
