---
estimated_steps: 6
estimated_files: 7
---

# T01: Create Core asmdef and restructure Core/Unity folders

**Slice:** S01 — Core Assembly Restructure + Generic Managers
**Milestone:** M002

## Description

Create the new `SimpleGame.Core.asmdef` assembly definition and move the three Unity-specific implementations (`UnitySceneLoader`, `UnityInputBlocker`, `UnityTransitionPlayer`) from `Assets/Scripts/Runtime/` into `Assets/Scripts/Core/Unity/` subfolders with updated namespaces.

## Steps

1. Create `Assets/Scripts/Core/SimpleGame.Core.asmdef` with the correct JSON content (name: `SimpleGame.Core`, references: `["UniTask", "UnityEngine.UI"]`, `autoReferenced: true`, `noEngineReferences: false`)
2. Create target directories: `Assets/Scripts/Core/Unity/ScreenManagement/`, `Assets/Scripts/Core/Unity/PopupManagement/`, `Assets/Scripts/Core/Unity/TransitionManagement/`
3. `git mv` `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` → `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs`
4. `git mv` `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` → `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs`
5. `git mv` `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` → `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs`
6. Update namespace in each moved file: `SimpleGame.Runtime.ScreenManagement` → `SimpleGame.Core.Unity.ScreenManagement`, `SimpleGame.Runtime.PopupManagement` → `SimpleGame.Core.Unity.PopupManagement`, `SimpleGame.Runtime.TransitionManagement` → `SimpleGame.Core.Unity.TransitionManagement`

## Must-Haves

- [ ] `Assets/Scripts/Core/SimpleGame.Core.asmdef` exists with name `SimpleGame.Core`
- [ ] `UnitySceneLoader.cs` is at `Assets/Scripts/Core/Unity/ScreenManagement/` with namespace `SimpleGame.Core.Unity.ScreenManagement`
- [ ] `UnityInputBlocker.cs` is at `Assets/Scripts/Core/Unity/PopupManagement/` with namespace `SimpleGame.Core.Unity.PopupManagement`
- [ ] `UnityTransitionPlayer.cs` is at `Assets/Scripts/Core/Unity/TransitionManagement/` with namespace `SimpleGame.Core.Unity.TransitionManagement`

## Verification

- `cat Assets/Scripts/Core/SimpleGame.Core.asmdef` shows `"name": "SimpleGame.Core"`
- `find Assets/Scripts/Core/Unity -name "*.cs" | sort` returns all three files
- `grep "namespace SimpleGame.Core.Unity" Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs`

## Inputs

- `Assets/Scripts/Runtime/ScreenManagement/UnitySceneLoader.cs` — source file to move
- `Assets/Scripts/Runtime/PopupManagement/UnityInputBlocker.cs` — source file to move
- `Assets/Scripts/Runtime/TransitionManagement/UnityTransitionPlayer.cs` — source file to move

## Expected Output

- `Assets/Scripts/Core/SimpleGame.Core.asmdef` — new assembly definition
- `Assets/Scripts/Core/Unity/ScreenManagement/UnitySceneLoader.cs` — moved, namespace updated
- `Assets/Scripts/Core/Unity/PopupManagement/UnityInputBlocker.cs` — moved, namespace updated
- `Assets/Scripts/Core/Unity/TransitionManagement/UnityTransitionPlayer.cs` — moved, namespace updated
