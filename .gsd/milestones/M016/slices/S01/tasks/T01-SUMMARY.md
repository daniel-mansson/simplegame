---
id: T01
parent: S01
milestone: M016
provides:
  - PlayFab SDK source files in Assets/PlayFabSDK/ (239 files, version 2.230.260123)
  - PlayFab.asmdef present and auto-referenced
  - SimpleGame.Game.asmdef updated with "PlayFab" reference
  - SimpleGame.Tests.Game.asmdef updated with "PlayFab" reference
  - All downstream PlayFab API surface available: Client, Shared, Events, Authentication
requires: []
affects: [T02, T03, S02, S03, S04]
key_files:
  - Assets/PlayFabSDK/PlayFab.asmdef
  - Assets/PlayFabSDK/Client/PlayFabClientAPI.cs
  - Assets/PlayFabSDK/Shared/Public/PlayFabSettings.cs
  - Assets/PlayFabSDK/Shared/Public/Resources/PlayFabSharedSettings.asset
  - Assets/Scripts/Game/SimpleGame.Game.asmdef
  - Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef
key_decisions:
  - "SDK installed via git sparse-checkout from GitHub source — no unitypackage import needed"
  - "PlayFab.asmdef has autoReferenced:true — explicit Game.asmdef reference is belt-and-suspenders"
patterns_established:
  - "PlayFab SDK lives at Assets/PlayFabSDK/ — never modify SDK files directly"
drill_down_paths:
  - .gsd/milestones/M016/slices/S01/tasks/T01-PLAN.md
duration: 10min
verification_result: static-pass
completed_at: 2026-03-20T00:00:00Z
---

# T01: Install PlayFab SDK via git sparse checkout

**PlayFab Unity SDK 2.230.260123 fetched from GitHub and installed into Assets/PlayFabSDK/ via git sparse-checkout; assembly references updated.**

## What Happened

Used git sparse-checkout to fetch only `ExampleTestProject/Assets/PlayFabSDK/` from PlayFab/UnitySDK master. Copied into `Assets/PlayFabSDK/`. Added `"PlayFab"` to both `SimpleGame.Game.asmdef` and `SimpleGame.Tests.Game.asmdef` references.

Unity Editor must be opened to trigger recompilation. The `PlayFabSharedSettings.asset` in `Assets/PlayFabSDK/Shared/Public/Resources/` needs a Title ID configured in the Inspector before PlayFab API calls will succeed at runtime.

## Deviations

None from plan.

## Files Created/Modified
- `Assets/PlayFabSDK/` — full SDK source (239 files)
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — added PlayFab reference
- `Assets/Tests/EditMode/Game/SimpleGame.Tests.Game.asmdef` — added PlayFab reference
