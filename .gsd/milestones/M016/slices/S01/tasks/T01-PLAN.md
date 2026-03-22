# T01: Install PlayFab SDK via git sparse checkout

**Slice:** S01
**Milestone:** M016

## Goal
Fetch the PlayFab Unity SDK source from GitHub into `Assets/PlayFabSDK/` so the project compiles with PlayFab types available, without requiring a manual Unity Editor `.unitypackage` import.

## Must-Haves

### Truths
- `Assets/PlayFabSDK/Client/PlayFabClientAPI.cs` exists
- `Assets/PlayFabSDK/Shared/Public/PlayFabSettings.cs` exists
- `Assets/PlayFabSDK/PlayFab.asmdef` exists
- `SimpleGame.Game.asmdef` references `PlayFab` assembly by name
- Project compiles (no CS errors in console after Unity opens)

### Artifacts
- `Assets/PlayFabSDK/` — full SDK source tree (Client, Shared, Events, Authentication, Entity subfolders minimum)
- `Assets/PlayFabSDK/PlayFab.asmdef` — SDK assembly definition
- `Assets/Scripts/Game/SimpleGame.Game.asmdef` — updated with `"PlayFab"` reference

### Key Links
- `SimpleGame.Game.asmdef` → `PlayFab.asmdef` via assembly name reference

## Steps
1. Use git sparse-checkout to clone only `ExampleTestProject/Assets/PlayFabSDK/` from PlayFab/UnitySDK into a temp directory
2. Copy the contents into `Assets/PlayFabSDK/`
3. Read `Assets/Scripts/Game/SimpleGame.Game.asmdef` and add `"PlayFab"` to the references array
4. Verify the SDK directory structure is correct (Client, Shared, Events folders present)
5. Read `Assets/PlayFabSDK/PlayFab.asmdef` to confirm it exists and has the right name
6. Commit the SDK files

## Context
- PlayFab SDK latest release: 2.230.260123 (Jan 2026)
- Source available at: https://github.com/PlayFab/UnitySDK/tree/master/ExampleTestProject/Assets/PlayFabSDK
- No UPM package exists — direct source copy is the correct approach
- The SDK includes a `PlayFab.asmdef` which Unity uses for compilation isolation
- We need Client, Shared, Events, Authentication at minimum; copying the whole SDK is fine and avoids missing transitive deps
- `PlayFabSharedSettings` ScriptableObject (in `Assets/PlayFabSDK/Shared/Public/Resources/`) needs Title ID configured — that is a separate manual step the user does in the Unity Editor. We document it but don't block on it for compilation.
